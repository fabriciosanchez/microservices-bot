﻿using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Newtonsoft.Json;
using OneBank.BotStateActor.Interfaces;

namespace OneBank.Common
{
    public class ServiceFabricBotDataStore : IBotDataStore<BotData>
    {
        private static readonly JsonSerializerSettings SerializationSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore
        };
        private readonly string botName;

        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        private IBotStateActor botStateActor;

        private StoreCacheEntry storeCache;

        public ServiceFabricBotDataStore(string botName)
        {
            this.botName = botName;
        }

        public async Task<bool> FlushAsync(IAddress key, CancellationToken cancellationToken)
        {
            var botStateActor = await this.GetActorInstance(key.UserId, key.ChannelId);

            if (this.storeCache != null)
            {
                BotStateContext botStateContext = new BotStateContext()
                {
                    BotId = key.BotId,
                    ChannelId = key.ChannelId,
                    ConversationId = key.ConversationId,
                    UserId = key.UserId,
                    ConversationData = new StateData() { ETag = this.storeCache.ConversationData.ETag, Data = Serialize(this.storeCache.ConversationData.Data) },
                    PrivateConversationData = new StateData() { ETag = this.storeCache.PrivateConversationData.ETag, Data = Serialize(this.storeCache.PrivateConversationData.Data) },
                    UserData = new StateData() { ETag = this.storeCache.UserData.ETag, Data = Serialize(this.storeCache.UserData.Data) },
                    TimeStamp = DateTime.UtcNow
                };

                this.storeCache = null;
                await botStateActor.SaveBotStateAsync(this.GetStateKey(key), botStateContext, cancellationToken);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<BotData> LoadAsync(IAddress key, BotStoreType botStoreType, CancellationToken cancellationToken)
        {
            await this.semaphoreSlim.WaitAsync();

            try
            {
                if (this.storeCache != null)
                {
                    return this.GetFromStoreCache(botStoreType);
                }
                else
                {
                    var botStateActor = await this.GetActorInstance(key.UserId, key.ChannelId);
                    var botStateContext = await botStateActor.GetBotStateAsync(this.GetStateKey(key), cancellationToken);

                    this.storeCache = new StoreCacheEntry();

                    if (botStateContext != null)
                    {
                        this.storeCache.ConversationData = new BotData(botStateContext.ConversationData.ETag, Deserialize(botStateContext.ConversationData.Data));
                        this.storeCache.PrivateConversationData = new BotData(botStateContext.PrivateConversationData.ETag, Deserialize(botStateContext.PrivateConversationData.Data));
                        this.storeCache.UserData = new BotData(botStateContext.UserData.ETag, Deserialize(botStateContext.UserData.Data));
                    }
                    else
                    {
                        this.storeCache.ConversationData = new BotData("*", null);
                        this.storeCache.PrivateConversationData = new BotData("*", null);
                        this.storeCache.UserData = new BotData("*", null);
                    }

                    return this.GetFromStoreCache(botStoreType);
                }
            }
            finally
            {
                this.semaphoreSlim.Release();
            }
        }

        public async Task SaveAsync(IAddress key, BotStoreType botStoreType, BotData data, CancellationToken cancellationToken)
        {
            if (this.storeCache == null)
            {
                this.storeCache = new StoreCacheEntry();
            }

            switch (botStoreType)
            {
                case BotStoreType.BotConversationData:
                    this.storeCache.ConversationData = data;
                    break;
                case BotStoreType.BotPrivateConversationData:
                    this.storeCache.PrivateConversationData = data;
                    break;
                case BotStoreType.BotUserData:
                    this.storeCache.UserData = data;
                    break;
                default:
                    throw new ArgumentException("Unsupported bot store type!");
            }

            await Task.CompletedTask;
        }

        private static byte[] Serialize(object data)
        {
            using (var cmpStream = new MemoryStream())
            using (var stream = new GZipStream(cmpStream, CompressionMode.Compress))
            using (var streamWriter = new StreamWriter(stream))
            {
                var serializedJSon = JsonConvert.SerializeObject(data, SerializationSettings);
                streamWriter.Write(serializedJSon);
                streamWriter.Close();
                stream.Close();
                return cmpStream.ToArray();
            }
        }

        private static object Deserialize(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            using (var gz = new GZipStream(stream, CompressionMode.Decompress))
            using (var streamReader = new StreamReader(gz))
            {
                return JsonConvert.DeserializeObject(streamReader.ReadToEnd());
            }
        }

        private async Task<IBotStateActor> GetActorInstance(string userId, string channelId)
        {
            if (this.botStateActor == null)
            {
                this.botStateActor = ActorProxy.Create<IBotStateActor>(new ActorId($"{userId}-{channelId}"), new Uri("fabric:/OneBank.FabricApp/BotStateActorService"));
            }

            return this.botStateActor;
        }

        private string GetStateKey(IAddress key)
        {
            return $"{this.botName}{key.ConversationId}";
        }

        private BotData GetFromStoreCache(BotStoreType botStoreType)
        {
            switch (botStoreType)
            {
                case BotStoreType.BotConversationData:
                    return this.storeCache.ConversationData;

                case BotStoreType.BotUserData:
                    return this.storeCache.UserData;

                case BotStoreType.BotPrivateConversationData:
                    return this.storeCache.PrivateConversationData;

                default:
                    throw new ArgumentException("Unsupported bot store type!");
            }
        }
    }

    public class StoreCacheEntry
    {
        public BotData ConversationData { get; set; }

        public BotData PrivateConversationData { get; set; }

        public BotData UserData { get; set; }
    }
}