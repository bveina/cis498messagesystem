namespace ComplexChatRoo
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.tabPages = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.cmdSend = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lstMessages = new System.Windows.Forms.ListBox();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.drawingBox31 = new DrawingBox.DrawingBox3();
            this.tabPages.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // tabPages
            // 
            this.tabPages.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tabPages.Controls.Add(this.tabPage1);
            this.tabPages.Controls.Add(this.tabPage2);
            this.tabPages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPages.HotTrack = true;
            this.tabPages.ItemSize = new System.Drawing.Size(200, 50);
            this.tabPages.Location = new System.Drawing.Point(0, 0);
            this.tabPages.Name = "tabPages";
            this.tabPages.SelectedIndex = 0;
            this.tabPages.Size = new System.Drawing.Size(567, 483);
            this.tabPages.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabPages.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tableLayoutPanel2);
            this.tabPage1.Location = new System.Drawing.Point(4, 4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(559, 425);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Compose";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Controls.Add(this.cmdSend, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.drawingBox31, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 87.11217F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.88783F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(553, 419);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // cmdSend
            // 
            this.cmdSend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdSend.Location = new System.Drawing.Point(3, 367);
            this.cmdSend.Name = "cmdSend";
            this.cmdSend.Size = new System.Drawing.Size(547, 49);
            this.cmdSend.TabIndex = 0;
            this.cmdSend.Text = "Send";
            this.cmdSend.UseVisualStyleBackColor = true;
            this.cmdSend.Click += new System.EventHandler(this.cmdSend_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.splitContainer1);
            this.tabPage2.Location = new System.Drawing.Point(4, 4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(559, 425);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Acknowladge";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pictureBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lstMessages);
            this.splitContainer1.Size = new System.Drawing.Size(553, 419);
            this.splitContainer1.SplitterDistance = 431;
            this.splitContainer1.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(431, 419);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // lstMessages
            // 
            this.lstMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstMessages.FormattingEnabled = true;
            this.lstMessages.Location = new System.Drawing.Point(0, 0);
            this.lstMessages.Name = "lstMessages";
            this.lstMessages.Size = new System.Drawing.Size(118, 407);
            this.lstMessages.TabIndex = 0;
            this.lstMessages.SelectedIndexChanged += new System.EventHandler(this.lstMessages_SelectedIndexChanged);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = " ";
            this.notifyIcon1.Visible = true;
            // 
            // drawingBox31
            // 
            this.drawingBox31.BackColor = System.Drawing.SystemColors.Control;
            this.drawingBox31.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("drawingBox31.BackgroundImage")));
            this.drawingBox31.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.drawingBox31.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.drawingBox31.Cursor = System.Windows.Forms.Cursors.Cross;
            this.drawingBox31.Dock = System.Windows.Forms.DockStyle.Fill;
            this.drawingBox31.lineColor = System.Drawing.Color.Black;
            this.drawingBox31.lineWidth = 1;
            this.drawingBox31.Location = new System.Drawing.Point(3, 3);
            this.drawingBox31.Name = "drawingBox31";
            this.drawingBox31.Size = new System.Drawing.Size(547, 358);
            this.drawingBox31.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(567, 483);
            this.Controls.Add(this.tabPages);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.tabPages.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabPages;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button cmdSend;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox lstMessages;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private DrawingBox.DrawingBox3 drawingBox31;
    }
}

