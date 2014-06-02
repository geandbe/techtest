using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Blurocket.TechTestShared
{
    [DataContract]
    public class DataItem
    {
        public DataItem()
        {
        }

        public DataItem(DateTime created, int amount, long messageId)
        {
            Created = created;
            Amount = amount;
            MessageId = messageId;
        }

        [DataMember(Order = 1)]
        public DateTime Created { get; private set; }

        [DataMember(Order = 2)]
        public int Amount { get; private set; }

        [DataMember(Order = 3)]
        public long MessageId { get; private set; }

        #region Static
        private static Random _rand = new Random(DateTime.UtcNow.Millisecond);
        private static long _messageId = 0;
        public static DataItem CreateRandom()
        {
            Interlocked.Add(ref _messageId, 1);
            return new DataItem(DateTime.UtcNow, _rand.Next(1000), _messageId);
        }
        #endregion
    }
}
