﻿namespace OneBank.Common
{
    using System.Collections.Concurrent;
    using System.Threading;

    public class RequestCallContext
    {
        public static AsyncLocal<string> AuthToken { get; set; } = new AsyncLocal<string>();
    }
}