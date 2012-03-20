using System.Linq;
using System.Threading.Tasks;

namespace MyServer
{
    using System;
    using DeferedProcessing;
    using NServiceBus;
    using Saga;

    class Starter:IWantToRunAtStartup
    {
        public IBus Bus { get; set; }

        public void Run()
        {
            Console.WriteLine("Type 's' followed by a number + <Enter> to start n sagas");
            Console.WriteLine("Type '?' + <Enter> for some statistics");
            Console.WriteLine("To exit, press Ctrl + C");

            string cmd;

            while ((cmd = Console.ReadLine()).ToLower() != "q")
            {
                if (cmd.StartsWith("s")){
                   Parallel.For(0, int.Parse(cmd.Substring(1)), i => StartSaga());
                }
                else if (cmd == "?")
                {
                    PrintMissedNumbers();
                }
                else{
                    Console.WriteLine();
                }
            }
        }

        private void PrintMissedNumbers()
        {
            var missed = SimpleSaga.FiredTimeoutNumbers.Except(SimpleSaga.ReceivedTimeoutNumbers).Select(i =>i.ToString()).ToArray();
            Console.WriteLine(string.Format("{0} - {1}", DateTime.Now.ToLongTimeString(), "Missed " + missed.Length + " timeouts: " + String.Join(", ", missed)));
        }

        void DeferMessage()
        {
            Bus.Defer(DateTime.UtcNow.AddSeconds(-10), new DeferredMessage());
            Console.WriteLine(string.Format("{0} - {1}", DateTime.Now.ToLongTimeString(), "Sent a message that is deferred for 10 seconds")); 
        }

        void StartSaga(string tennant = "")
        {
            var message = new StartSagaMessage
                              {
                                  OrderId = Guid.NewGuid()
                              };
            if (!string.IsNullOrEmpty(tennant))            
                message.SetHeader("tennant", tennant);
            
                
            Bus.SendLocal(message);
            Console.WriteLine(string.Format("{0} - {1}", DateTime.Now.ToLongTimeString(), "Saga start message sent")); 
        }

       
        public void Stop()
        {
        }
    }
}