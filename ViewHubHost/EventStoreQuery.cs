using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServiceStack.Redis;
using ServiceStack.Redis.Generic;
using Blurocket.TechTestShared;
using BlurocketTest;

namespace ViewHubHost
{
    public class EventStoreQuery
    {
        private EventStoreQuery() { }

        private static RedisClient client = new RedisClient();

        public static RedisClient Client = client; 

        public static RedisAggregator.Analytics GetCurrentAnalytics()
        {
            return  EventStoreQuery.Client.Get<RedisAggregator.Analytics>("urn:blurocket:analytics");
        }

        public static int GetCurrentMax()
        {
            return EventStoreQuery.Client.Get<int>("urn:blurocket:max");
        }
    }
}
