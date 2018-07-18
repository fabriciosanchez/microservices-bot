﻿using System;

namespace OneBank.BotStateActor.Interfaces
{
    [Serializable]
    public class BotStateContext
    {
        public string BotId { get; set; }
        public string UserId { get; set; }

        public string ChannelId { get; set; }

        public string ConversationId { get; set; }

        public DateTime TimeStamp { get; set; }

        public StateData UserData { get; set; } = new StateData();

        public StateData ConversationData { get; set; } = new StateData();

        public StateData PrivateConversationData { get; set; } = new StateData();
    }

    public class StateData
    {
        public byte[] Data { get; set; }

        public string ETag { get; set; }
    }
}