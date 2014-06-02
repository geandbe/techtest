using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blurocket.TechTestShared;
using ZMQ;

namespace Blurocket_TecTest
{
    /// <summary>
    /// Streamer sends out a stream of random DataItem objects
    /// </summary>
    internal class Streamer : IDisposable
    {
        internal Streamer(string publishPort, string topicName)
        {
            _topicName = topicName;
            _address = publishPort;
            _timer = new Timer(new TimerCallback(o =>
                {
                    _messagesPerSecond = _messageCount - _lastMessageCount;
                    _lastMessageCount = _messageCount;
                }), null,1000,1000);
        }

        private Lazy<Context> Context = new Lazy<Context>(() => { return new Context(); });

        private Socket _socket = null;
        private string _topicName = String.Empty;
        private string _address = String.Empty;
        private bool _stopped = false;
        private bool _paused = false;
        private int _throttleMilliseconds = 0;
        private long _total = 0;
        private long _messageCount = 0;
        private long _lastMessageCount = 0;
        private long _messagesPerSecond = 0;
        private Timer _timer;

        internal void Start()
        {
            var t = new Thread(() =>
                {
                    _socket = Context.Value.Socket(SocketType.PUB);
                    _socket.SetSockOpt(SocketOpt.LINGER, 0); //Stops context from waiting at shutdown
                    _socket.Bind(_address);
                    while (!_stopped)
                    {
                        if (_throttleMilliseconds > 0)
                        {
                            Thread.Sleep(_throttleMilliseconds);
                        }
                        while (_paused)
                        {
                            Thread.Sleep(50);
                        }
                        if (_stopped) { break; }
                        var data = DataItem.CreateRandom();
                        _socket.SendMore(_topicName, Encoding.Unicode);
                        Interlocked.Add(ref _total, (long)data.Amount);
                        Interlocked.Add(ref _messageCount, 1);
                        _socket.Send(SerializationHelper.SerializeToBinary<DataItem>(data));
                    }
                });
            t.Start();
        }

        internal long Total
        {
            get
            {
                return _total;
            }
        }

        internal long MessagesSent
        {
            get
            {
                return _messageCount;
            }
        }

        internal long MessagesPerSecond
        {
            get
            {
                return _messagesPerSecond;
            }
        }

        internal void Pause()
        {
            _paused = true;
        }

        internal void Resume()
        {
            _paused = false;
        }

        internal void Stop()
        {
            _paused = false;
            _stopped = true;
        }

        internal void SetThrottleLevel(int throttleMilliseconds)
        {
            _throttleMilliseconds = throttleMilliseconds;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(Boolean freeManagedObjectsAlso)
        {
            if (freeManagedObjectsAlso)
            {
                if (_socket != null)
                {
                    try
                    {
                        _timer.Dispose();
                        _socket.Dispose();
                    }
                    catch (System.Exception ex)
                    {
                        //Do nothing for now
                    }
                }
            }
        }

        ~Streamer()
        {
            Dispose(false);
        }

        #endregion
    }
}
