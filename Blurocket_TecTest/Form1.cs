using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Blurocket_TecTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private ServerState _state = ServerState.Stopped;
        private Streamer _streamer;
        private System.Windows.Forms.Timer _timer = null;
        private int _messagePauseMilliseconds = 500;

        private void btnStart_Click(object sender, EventArgs e)
        {
            this.FormClosing += Form1_FormClosing;
            _state = ServerState.Started;
            setFormBasedOnState();
            _streamer = new Streamer(this.txtPortNumber.Text, this.txtTopic.Text);
            setThrottle();
            _streamer.Start();

            _timer = new System.Windows.Forms.Timer();
            _timer.Tick += _timer_Tick;
            _timer.Interval = 100;
            _timer.Start();
        }

        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_state != ServerState.Stopped)
            {
                btnStop_Click(this, EventArgs.Empty);
            }
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            updateTotal();
        }

        private void updateTotal()
        {
            if (_streamer != null)
            {
                this.txtRunningTotal.Text = _streamer.Total.ToString("c");
                this.txtMessages.Text = _streamer.MessagesSent.ToString() + " (appox " + _streamer.MessagesPerSecond + " msgs per second)";
            }
            else
            {
                this.txtRunningTotal.Text = "$0";
                this.txtMessages.Text = "0";
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (_state == ServerState.Started)
            {
                _state = ServerState.Paused;
                _streamer.Pause();
            }
            else
            {
                _state = ServerState.Started;
                setThrottle();
                _streamer.Resume();
            }
            setFormBasedOnState();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _state = ServerState.Stopped;
            setFormBasedOnState();
            _streamer.Stop();
            _streamer.Dispose();
            _timer.Stop();
            _timer.Dispose();
        }


        private void setFormBasedOnState()
        {
            switch (_state)
            {
                case ServerState.Stopped:
                    btnStart.Text = "Start";
                    btnPause.Text = "Pause";
                    btnStart.Enabled = true;
                    btnPause.Enabled = false;
                    btnStop.Enabled = false;
                    break;
                case ServerState.Started:
                    btnStart.Text = "Started..";
                    btnPause.Text = "Pause";
                    btnStart.Enabled = false;
                    btnPause.Enabled = true;
                    btnStop.Enabled = true;
                    break;
                case ServerState.Paused:
                    btnPause.Text = "Resume";
                    btnStart.Text = "Paused..";
                    btnStart.Enabled = false;
                    btnPause.Enabled = true;
                    btnStop.Enabled = true;
                    break;
                default:
                    throw new ApplicationException("Invalid State");
            }
            updateTotal();
        }

        private void setThrottle()
        {
            if (_streamer != null)
            {
                _messagePauseMilliseconds = Convert.ToInt32((double)this.tbThrottle.Value / (1000.0 / (double)this.tbThrottle.Value));
                _streamer.SetThrottleLevel(_messagePauseMilliseconds);
            }
        }

        private void tbThrottle_Scroll(object sender, EventArgs e)
        {
            setThrottle();
        }

        private enum ServerState
        {
            Stopped,
            Started,
            Paused
        }

    }
}
