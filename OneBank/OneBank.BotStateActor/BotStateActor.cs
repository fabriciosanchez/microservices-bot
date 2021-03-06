﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Data;
using OneBank.BotStateActor.Interfaces;

namespace BotStateActor
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class BotStateActor : Actor, IBotStateActor
    {
        public BotStateActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }
        public async Task<BotStateContext> GetBotStateAsync(string key, CancellationToken cancellationToken)
        {
            ActorEventSource.Current.ActorMessage(this, $"Getting bot state from actor key - {key}");
            ConditionalValue<BotStateContext> result = await this.StateManager.TryGetStateAsync<BotStateContext>(key, cancellationToken);

            if (result.HasValue)
            {
                return result.Value;
            }
            else
            {
                return null;
            }
        }

        public async Task<BotStateContext> SaveBotStateAsync(string key, BotStateContext dialogState, CancellationToken cancellationToken)
        {
            ActorEventSource.Current.ActorMessage(this, $"Adding bot state for actor key - {key}");
            return await this.StateManager.AddOrUpdateStateAsync(
                key,
                dialogState,
                (k, v) =>
            {
                return (dialogState.UserData.ETag != "*" && dialogState.UserData.ETag != v.UserData.ETag) ||
                      (dialogState.ConversationData.ETag != "*" && dialogState.ConversationData.ETag != v.UserData.ETag) ||
                        (dialogState.PrivateConversationData.ETag != "*" && dialogState.PrivateConversationData.ETag != v.UserData.ETag)
                        ? throw new Exception() : v = dialogState;
            },
            cancellationToken);
        }
    }
}