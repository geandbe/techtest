// Self-hosted Dashboard View Hub
using System;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;
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
                Console.WriteLine("Last DataItem Id Persisted: {0}", EventStoreQuery.GetCurrentAnalytics().LastId);
            };


            using (WebApp.Start(url))
            {
                Console.WriteLine("SignalR ViewHub is running on {0}", url);

                using (EventStoreQuery.Client)
                {
                    var typedClient = EventStoreQuery.Client.As<string>();
                    var eventQueue = typedClient.Lists["urn:blurocket:events"];
                    bool run = true;
                    while (run)
                    {
                        switch (typedClient.BlockingDequeueItemFromList(eventQueue, null))
                        {
                            case "analytics":
                                UpdateView(EventStoreQuery.GetCurrentAnalytics());
                                break;
                            case "max":
                                UpdateMax(EventStoreQuery.GetCurrentMax());
                                break;
                            case "stop":
                                run = false;
                                break;
                            default:
                                throw new InvalidOperationException(String.Format("Unknown EventStore descriptor"));
                        }
                    }
                }

                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }


        private static void UpdateView(RedisAggregator.Analytics newAnalytics)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ViewHub>();
            context.Clients.All.UpdateAnalytics(newAnalytics);
        }

        private static void UpdateMax(int newMax)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ViewHub>();
            context.Clients.All.UpdateMax(newMax);
        }

    }



 }
