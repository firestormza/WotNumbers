﻿namespace WinApp.Gadget
{
	partial class ucGaugeWN9
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnToday = new BadButton();
            this.btnWeek = new BadButton();
            this.btnMonth = new BadButton();
            this.btnMonth3 = new BadButton();
            this.btnTotal = new BadButton();
            this.aGauge1 = new AGaugeApp.AGauge();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // btnToday
            // 
            this.btnToday.BlackButton = true;
            this.btnToday.Checked = false;
            this.btnToday.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnToday.Image = null;
            this.btnToday.Location = new System.Drawing.Point(159, 148);
            this.btnToday.Margin = new System.Windows.Forms.Padding(0);
            this.btnToday.Name = "btnToday";
            this.btnToday.Size = new System.Drawing.Size(34, 16);
            this.btnToday.TabIndex = 15;
            this.btnToday.Text = "Today";
            this.btnToday.ToolTipText = "";
            this.btnToday.Click += new System.EventHandler(this.btnTime_Click);
            // 
            // btnWeek
            // 
            this.btnWeek.BlackButton = true;
            this.btnWeek.Checked = false;
            this.btnWeek.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnWeek.Image = null;
            this.btnWeek.Location = new System.Drawing.Point(121, 148);
            this.btnWeek.Margin = new System.Windows.Forms.Padding(0);
            this.btnWeek.Name = "btnWeek";
            this.btnWeek.Size = new System.Drawing.Size(34, 16);
            this.btnWeek.TabIndex = 14;
            this.btnWeek.Text = "Week";
            this.btnWeek.ToolTipText = "";
            this.btnWeek.Click += new System.EventHandler(this.btnTime_Click);
            // 
            // btnMonth
            // 
            this.btnMonth.BlackButton = true;
            this.btnMonth.Checked = false;
            this.btnMonth.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMonth.Image = null;
            this.btnMonth.Location = new System.Drawing.Point(83, 148);
            this.btnMonth.Margin = new System.Windows.Forms.Padding(0);
            this.btnMonth.Name = "btnMonth";
            this.btnMonth.Size = new System.Drawing.Size(34, 16);
            this.btnMonth.TabIndex = 13;
            this.btnMonth.Text = "Month";
            this.btnMonth.ToolTipText = "";
            this.btnMonth.Click += new System.EventHandler(this.btnTime_Click);
            // 
            // btnMonth3
            // 
            this.btnMonth3.BlackButton = true;
            this.btnMonth3.Checked = false;
            this.btnMonth3.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMonth3.Image = null;
            this.btnMonth3.Location = new System.Drawing.Point(45, 148);
            this.btnMonth3.Margin = new System.Windows.Forms.Padding(0);
            this.btnMonth3.Name = "btnMonth3";
            this.btnMonth3.Size = new System.Drawing.Size(34, 16);
            this.btnMonth3.TabIndex = 12;
            this.btnMonth3.Text = "3 Mth";
            this.btnMonth3.ToolTipText = "";
            this.btnMonth3.Click += new System.EventHandler(this.btnTime_Click);
            // 
            // btnTotal
            // 
            this.btnTotal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.btnTotal.BlackButton = true;
            this.btnTotal.Checked = false;
            this.btnTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTotal.Image = null;
            this.btnTotal.Location = new System.Drawing.Point(7, 148);
            this.btnTotal.Margin = new System.Windows.Forms.Padding(0);
            this.btnTotal.Name = "btnTotal";
            this.btnTotal.Size = new System.Drawing.Size(34, 16);
            this.btnTotal.TabIndex = 11;
            this.btnTotal.Text = "Total";
            this.btnTotal.ToolTipText = "";
            this.btnTotal.Click += new System.EventHandler(this.btnTime_Click);
            // 
            // aGauge1
            // 
            this.aGauge1.BaseArcColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(206)))));
            this.aGauge1.BaseArcRadius = 70;
            this.aGauge1.BaseArcStart = 150;
            this.aGauge1.BaseArcSweep = 240;
            this.aGauge1.BaseArcWidth = 1;
            this.aGauge1.Cap_Idx = ((byte)(1));
            this.aGauge1.CapColors = new System.Drawing.Color[] {
        System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(206))))),
        System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(206))))),
        System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(206))))),
        System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(206))))),
        System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(206)))))};
            this.aGauge1.CapPosition = new System.Drawing.Point(10, 10);
            this.aGauge1.CapsPosition = new System.Drawing.Point[] {
        new System.Drawing.Point(128, 90),
        new System.Drawing.Point(10, 10),
        new System.Drawing.Point(10, 10),
        new System.Drawing.Point(10, 10),
        new System.Drawing.Point(10, 10)};
            this.aGauge1.CapsText = new string[] {
        "",
        "",
        "",
        "",
        ""};
            this.aGauge1.CapText = "";
            this.aGauge1.Center = new System.Drawing.Point(100, 90);
            this.aGauge1.CenterSubText = "";
            this.aGauge1.CenterText = "";
            this.aGauge1.CenterTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(206)))));
            this.aGauge1.CenterTextFont = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.aGauge1.Dock = System.Windows.Forms.DockStyle.Top;
            this.aGauge1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            this.aGauge1.Location = new System.Drawing.Point(0, 0);
            this.aGauge1.Name = "aGauge1";
            this.aGauge1.NeedleColor1 = AGaugeApp.AGauge.NeedleColorEnum.WotNumbers;
            this.aGauge1.NeedleColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(206)))));
            this.aGauge1.NeedleRadius = 58;
            this.aGauge1.NeedleType = 0;
            this.aGauge1.NeedleWidth = 2;
            this.aGauge1.Range_Idx = ((byte)(0));
            this.aGauge1.RangeEnabled = false;
            this.aGauge1.RangeEndValue = 0F;
            this.aGauge1.RangeInnerRadius = 70;
            this.aGauge1.RangeOuterRadius = 72;
            this.aGauge1.RangesEnabled = new bool[] {
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false};
            this.aGauge1.RangesEndValue = new float[] {
        0F,
        0F,
        0F,
        0F,
        0F,
        0F,
        0F,
        0F,
        0F,
        0F};
            this.aGauge1.RangesInnerRadius = new int[] {
        70,
        70,
        70,
        70,
        70,
        70,
        70,
        70,
        70,
        70};
            this.aGauge1.RangesOuterRadius = new int[] {
        72,
        72,
        72,
        72,
        72,
        72,
        72,
        72,
        72,
        72};
            this.aGauge1.RangesStartValue = new float[] {
        0F,
        0F,
        0F,
        0F,
        0F,
        0F,
        0F,
        0F,
        0F,
        0F};
            this.aGauge1.RangeStartValue = 0F;
            this.aGauge1.ScaleLinesInterColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(206)))));
            this.aGauge1.ScaleLinesInterInnerRadius = 63;
            this.aGauge1.ScaleLinesInterOuterRadius = 70;
            this.aGauge1.ScaleLinesInterWidth = 1;
            this.aGauge1.ScaleLinesMajorColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(206)))));
            this.aGauge1.ScaleLinesMajorInnerRadius = 60;
            this.aGauge1.ScaleLinesMajorOuterRadius = 70;
            this.aGauge1.ScaleLinesMajorWidth = 2;
            this.aGauge1.ScaleLinesMinorColor = System.Drawing.Color.FromArgb(((int)(((byte)(85)))), ((int)(((byte)(85)))), ((int)(((byte)(89)))));
            this.aGauge1.ScaleLinesMinorInnerRadius = 65;
            this.aGauge1.ScaleLinesMinorOuterRadius = 70;
            this.aGauge1.ScaleLinesMinorWidth = 1;
            this.aGauge1.ScaleNumbersColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(206)))));
            this.aGauge1.ScaleNumbersFormat = null;
            this.aGauge1.ScaleNumbersRadius = 88;
            this.aGauge1.ScaleNumbersRotation = 0;
            this.aGauge1.ScaleNumbersStartScaleLine = 0;
            this.aGauge1.ScaleNumbersStepScaleLines = 1;
            this.aGauge1.Size = new System.Drawing.Size(200, 140);
            this.aGauge1.TabIndex = 0;
            this.aGauge1.Text = "aGauge1";
            this.aGauge1.Value = 0F;
            this.aGauge1.ValueMax = 20F;
            this.aGauge1.ValueMin = -20F;
            this.aGauge1.ValueScaleLinesMajorStepValue = 5F;
            this.aGauge1.ValueScaleLinesMinorNumOf = 4;
            // 
            // ucGaugeWN8
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.Controls.Add(this.btnToday);
            this.Controls.Add(this.btnWeek);
            this.Controls.Add(this.btnMonth);
            this.Controls.Add(this.btnMonth3);
            this.Controls.Add(this.btnTotal);
            this.Controls.Add(this.aGauge1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ucGaugeWN8";
            this.Size = new System.Drawing.Size(200, 170);
            this.Load += new System.EventHandler(this.ucGauge_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ucGaugeWN9_Paint);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Timer timer1;
		private AGaugeApp.AGauge aGauge1;
		private BadButton btnToday;
		private BadButton btnWeek;
		private BadButton btnMonth;
		private BadButton btnMonth3;
		private BadButton btnTotal;

















	}
}
