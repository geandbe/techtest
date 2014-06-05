// Self-hosted Dashboard View Hub
using System;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;
using System.Reactive.Linq;

namespace ViewHubHost
{
    class Program
    {
        static void Main(string[] args)
        {
            int ticks = 0;
            // This will *ONLY* bind to localhost, if you want to bind to all addresses
            // use http://*:8080 to bind to all addresses. 
            // See http://msdn.microsoft.com/en-us/library/system.net.httplistener.aspx 
            // for more information.
            string url = "http://localhost:8080";

            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
            {
                e.Cancel = true;
                Console.WriteLine("Ticks passed: {0}", ticks);
            };


            using (WebApp.Start(url))
            {
                Console.WriteLine("SignalR ViewHub is running on {0}; press any key to continue...", url);
                Console.ReadKey();

                var mark = Observable.Interval(TimeSpan.FromMilliseconds(100)).TimeInterval();

                //using (mark.Subscribe(x => SendUpdate(x.Value.ToString())))
                using (mark.Subscribe(x => { ticks++; SendUpdate(x.Value.ToString()); }))
                {
                    Console.WriteLine("Press any key to unsubscribe");
                    Console.ReadKey();
                }

                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }
        
        /// <summary>
        /// Method to be called on clients
        /// </summary>
        /// <param name="msg">Value to be sent</param>
        public static void SendUpdate(string msg)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ViewHub>();
            context.Clients.All.AddMessage(msg);
        }

    }



 }
