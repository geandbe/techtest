namespace Blurocket_TecTest
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnStart = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtPortNumber = new System.Windows.Forms.TextBox();
            this.btnStop = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtRunningTotal = new System.Windows.Forms.TextBox();
            this.tbThrottle = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.txtTopic = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtMessages = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.tbThrottle)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(31, 64);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(195, 23);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start Streaming";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnPause
            // 
            this.btnPause.Enabled = false;
            this.btnPause.Location = new System.Drawing.Point(31, 93);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(195, 23);
            this.btnPause.TabIndex = 1;
            this.btnPause.Text = "Pause Streaming";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Address";
            // 
            // txtPortNumber
            // 
            this.txtPortNumber.Location = new System.Drawing.Point(115, 13);
            this.txtPortNumber.Name = "txtPortNumber";
            this.txtPortNumber.Size = new System.Drawing.Size(112, 20);
            this.txtPortNumber.TabIndex = 3;
            this.txtPortNumber.Text = "tcp://127.0.0.1:4567";
            this.txtPortNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(31, 122);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(195, 23);
            this.btnStop.TabIndex = 4;
            this.btnStop.Text = "Stop Streaming";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(35, 241);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Running Total";
            // 
            // txtRunningTotal
            // 
            this.txtRunningTotal.Location = new System.Drawing.Point(32, 257);
            this.txtRunningTotal.Name = "txtRunningTotal";
            this.txtRunningTotal.ReadOnly = true;
            this.txtRunningTotal.Size = new System.Drawing.Size(194, 20);
            this.txtRunningTotal.TabIndex = 6;
            this.txtRunningTotal.Text = "0";
            this.txtRunningTotal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // tbThrottle
            // 
            this.tbThrottle.Location = new System.Drawing.Point(25, 173);
            this.tbThrottle.Maximum = 1000;
            this.tbThrottle.Name = "tbThrottle";
            this.tbThrottle.Size = new System.Drawing.Size(201, 45);
            this.tbThrottle.TabIndex = 7;
            this.tbThrottle.TickFrequency = 100;
            this.tbThrottle.Value = 1000;
            this.tbThrottle.Scroll += new System.EventHandler(this.tbThrottle_Scroll);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(29, 157);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(181, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Throttle Message Rate (Low -> High)";
            // 
            // txtTopic
            // 
            this.txtTopic.Location = new System.Drawing.Point(114, 38);
            this.txtTopic.Name = "txtTopic";
            this.txtTopic.Size = new System.Drawing.Size(112, 20);
            this.txtTopic.TabIndex = 10;
            this.txtTopic.Text = "ORDERS";
            this.txtTopic.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(28, 41);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(80, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Pub/Sub Topic";
            // 
            // txtMessages
            // 
            this.txtMessages.Location = new System.Drawing.Point(32, 220);
            this.txtMessages.Name = "txtMessages";
            this.txtMessages.ReadOnly = true;
            this.txtMessages.Size = new System.Drawing.Size(194, 20);
            this.txtMessages.TabIndex = 12;
            this.txtMessages.Text = "0";
            this.txtMessages.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(35, 204);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Messages Sent";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(238, 286);
            this.Controls.Add(this.txtMessages);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtTopic);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbThrottle);
            this.Controls.Add(this.txtRunningTotal);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.txtPortNumber);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnPause);
            this.Controls.Add(this.btnStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Blurocket - Tech Test";
            ((System.ComponentModel.ISupportInitialize)(this.tbThrottle)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPortNumber;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtRunningTotal;
        private System.Windows.Forms.TrackBar tbThrottle;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtTopic;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtMessages;
        private System.Windows.Forms.Label label5;
    }
}

