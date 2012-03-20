using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace MyServer.Saga
{
    using System;
    using NServiceBus.Saga;

    public class Counter
    {
        private int _count;

        public int Next()
        {
            return Interlocked.Increment(ref _count);
        }

        public static implicit operator int(Counter c)
        {
            return c._count;
        }

        public override string ToString()
        {
            return _count.ToString(CultureInfo.InvariantCulture);
        }
    }

    public class SimpleSaga:Saga<SimpleSagaData>,
        IAmStartedByMessages<StartSagaMessage>,
        IHandleTimeouts<MyTimeOutState>
    {
        public static Counter FiredTimeoutsCount = new Counter();
        public static Counter ReceivedTimeoutsCount = new Counter();

        public static ConcurrentBag<int> FiredTimeoutNumbers = new ConcurrentBag<int>();
        public static ConcurrentBag<int> ReceivedTimeoutNumbers = new ConcurrentBag<int>();

        public void Handle(StartSagaMessage message)
        {
            Data.OrderId = message.OrderId;
            int runningNumber = FiredTimeoutsCount.Next();

            RequestUtcTimeout(TimeSpan.FromSeconds(10), new MyTimeOutState { SomeValue = runningNumber });

            FiredTimeoutNumbers.Add(runningNumber);
            LogMessage("Fired  #" + runningNumber);
        }

        public override void ConfigureHowToFindSaga()
        {
            ConfigureMapping<StartSagaMessage>(s => s.OrderId, m => m.OrderId);
        }

        void LogMessage(string message)
        {
            Console.WriteLine(string.Format("{0} - {1}", DateTime.Now.ToLongTimeString(),message));
        }

        public void Timeout(MyTimeOutState state)
        {
            LogMessage("Received  #" + ReceivedTimeoutsCount.Next() + " of " + FiredTimeoutsCount);
            ReceivedTimeoutNumbers.Add(state.SomeValue);
            MarkAsComplete();
        }
    }
}