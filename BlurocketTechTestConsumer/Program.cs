using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blurocket.TechTestShared;
using ZMQ;

namespace BlurocketTechTestConsumer
{
    class Program
    {
        internal static Lazy<Context> Context = new Lazy<Context>(() => { return new Context(); });
        internal static long _total = 0;
        internal static long _receivedCount = 0;
        static void Main(string[] args)
        {
            Socket skt = Context.Value.Socket(SocketType.SUB);
            skt.Connect("tcp://127.0.0.1:4567");
            skt.Subscribe("ORDERS", Encoding.Unicode);
            while (true)
            {
                var topicName = skt.Recv(Encoding.Unicode);
                var objectData = skt.Recv();

                if (objectData != null)
                {
                    DataItem messageData = SerializationHelper.DeserializeFromBinary<DataItem>(objectData);
                    Interlocked.Add(ref _total, messageData.Amount);
                    Interlocked.Add(ref _receivedCount, 1);
                    if (_receivedCount % 100 == 0)
                    {
                        System.Console.WriteLine("Current Total: " + _total.ToString("c"));
                    }
                }
            }
        }
    }
}
