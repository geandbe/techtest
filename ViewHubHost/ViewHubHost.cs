// Self-hosted Dashboard View Hub
using System;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;
using System.Reactive.Linq;
using BlurocketTest;

namespace ViewHubHost
{
    class Program
    {
        static void Main(string[] args)
        {
            // This will *ONLY* bind to localhost, if you want to bind to all addresses
            // use http://*:8080 to bind to all addresses. 
            // See http://msdn.microsoft.com/en-us/library/system.net.httplistener.aspx 
            // for more information.
            string url = "http://localhost:8080";

            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
            {
                e.Cancel = true;
                Console.WriteLine("Last DataItem Id Persisted: {0}", DataStoreQuery.GetCurrentAnalytics().LastId);
            };


            using (WebApp.Start(url))
            {
                Console.WriteLine("SignalR ViewHub is running on {0}; press any key to continue...", url);
                Console.ReadKey();

                //TODO: Instead of Rx time interval-driven polling switch to Redis PubSub event-driven 
                var mark = Observable.Interval(TimeSpan.FromMilliseconds(100)).TimeInterval();

                using (DataStoreQuery.Client)
                {
                    using (mark.Subscribe(x => UpdateView()))
                    {
                        Console.WriteLine("Press any key to unsubscribe");
                        Console.ReadKey();
                    }
                }

                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Performed on each refresh view tick
        /// </summary>
        public static void UpdateView()
        {
            SendUpdate(DataStoreQuery.GetCurrentAnalytics());
            SendUpdate(DataStoreQuery.GetCurrentMax());
        }

        public static void SendUpdate(RedisAggregator.Analytics newAnalytics)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ViewHub>();
            context.Clients.All.UpdateAnalytics(newAnalytics);
        }

        public static void SendUpdate(int newMax)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ViewHub>();
            context.Clients.All.UpdateMax(newMax);
        }

    }



 }
