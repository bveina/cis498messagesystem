namespace DrawingBox
{
    partial class DrawingBox3
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DrawingBox3));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.cmdDraw = new System.Windows.Forms.ToolStripButton();
            this.cmdErase = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.cmdSizeBig = new System.Windows.Forms.ToolStripButton();
            this.cmdSizeMid = new System.Windows.Forms.ToolStripButton();
            this.cmdSizeTiny = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.cmdUndo = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmdDraw,
            this.cmdErase,
            this.toolStripSeparator1,
            this.cmdSizeBig,
            this.cmdSizeMid,
            this.cmdSizeTiny,
            this.toolStripSeparator2,
            this.cmdUndo,
            this.toolStripSeparator3,
            this.toolStripButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 212);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(384, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // cmdDraw
            // 
            this.cmdDraw.CheckOnClick = true;
            this.cmdDraw.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.cmdDraw.Image = global::DrawingBox.Properties.Resources.Pen_32x32;
            this.cmdDraw.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdDraw.Name = "cmdDraw";
            this.cmdDraw.Size = new System.Drawing.Size(23, 22);
            this.cmdDraw.Text = "Draw";
            this.cmdDraw.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // cmdErase
            // 
            this.cmdErase.CheckOnClick = true;
            this.cmdErase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.cmdErase.Image = global::DrawingBox.Properties.Resources.Eraser_2_32x32;
            this.cmdErase.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdErase.Name = "cmdErase";
            this.cmdErase.Size = new System.Drawing.Size(23, 22);
            this.cmdErase.Text = "Erase Line";
            this.cmdErase.Click += new System.EventHandler(this.toolStripButton2_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // cmdSizeBig
            // 
            this.cmdSizeBig.CheckOnClick = true;
            this.cmdSizeBig.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.cmdSizeBig.Image = global::DrawingBox.Properties.Resources.Large_Brush;
            this.cmdSizeBig.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdSizeBig.Name = "cmdSizeBig";
            this.cmdSizeBig.Size = new System.Drawing.Size(23, 22);
            this.cmdSizeBig.Text = "Large Brush";
            this.cmdSizeBig.Click += new System.EventHandler(this.toolStripButton3_Click);
            // 
            // cmdSizeMid
            // 
            this.cmdSizeMid.CheckOnClick = true;
            this.cmdSizeMid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.cmdSizeMid.Image = global::DrawingBox.Properties.Resources.Medium_Brush;
            this.cmdSizeMid.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdSizeMid.Name = "cmdSizeMid";
            this.cmdSizeMid.Size = new System.Drawing.Size(23, 22);
            this.cmdSizeMid.Text = "Medium Brush";
            this.cmdSizeMid.Click += new System.EventHandler(this.toolStripButton4_Click);
            // 
            // cmdSizeTiny
            // 
            this.cmdSizeTiny.CheckOnClick = true;
            this.cmdSizeTiny.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.cmdSizeTiny.Image = global::DrawingBox.Properties.Resources.Small_Brush;
            this.cmdSizeTiny.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdSizeTiny.Name = "cmdSizeTiny";
            this.cmdSizeTiny.Size = new System.Drawing.Size(23, 22);
            this.cmdSizeTiny.Text = "Small Brush";
            this.cmdSizeTiny.Click += new System.EventHandler(this.toolStripButton5_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // cmdUndo
            // 
            this.cmdUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.cmdUndo.Image = global::DrawingBox.Properties.Resources.undo_32x32;
            this.cmdUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdUndo.Name = "cmdUndo";
            this.cmdUndo.Size = new System.Drawing.Size(23, 22);
            this.cmdUndo.Text = "Undo";
            this.cmdUndo.Click += new System.EventHandler(this.toolStripButton6_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "Color Box";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click_1);
            // 
            // DrawingBox3
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Controls.Add(this.toolStrip1);
            this.Cursor = System.Windows.Forms.Cursors.Cross;
            this.DoubleBuffered = true;
            this.Name = "DrawingBox3";
            this.Size = new System.Drawing.Size(384, 237);
            this.Load += new System.EventHandler(this.DrawingBox3_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.DrawingBox3_Paint);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DrawingBox3_MouseMove);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DrawingBox3_MouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.DrawingBox3_MouseUp);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton cmdDraw;
        private System.Windows.Forms.ToolStripButton cmdErase;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton cmdSizeBig;
        private System.Windows.Forms.ToolStripButton cmdSizeMid;
        private System.Windows.Forms.ToolStripButton cmdSizeTiny;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton cmdUndo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButton1;

    }
}
