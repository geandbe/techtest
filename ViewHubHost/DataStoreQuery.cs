using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NServiceKit.Redis;
using NServiceKit.Redis.Generic;
using Blurocket.TechTestShared;
using BlurocketTest;

namespace ViewHubHost
{
    public class DataStoreQuery
    {
        private DataStoreQuery() { }

        private static RedisClient client = new RedisClient();
                
        public static RedisClient Client { 
            get { return client; }
        }

        public static RedisAggregator.Analytics GetCurrentAnalytics()
        {
            return  DataStoreQuery.Client.Get<RedisAggregator.Analytics>("urn:blurocket:analytics");
        }

        public static int GetCurrentMax()
        {
            return DataStoreQuery.Client.Get<int>("urn:blurocket:max");
        }
    }
}
