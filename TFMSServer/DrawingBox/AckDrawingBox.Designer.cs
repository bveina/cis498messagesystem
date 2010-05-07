namespace DrawingBox
{
    partial class AckDrawingBox
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
            this.SuspendLayout();
            // 
            // DrawingBox2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Name = "DrawingBox2";
            this.Size = new System.Drawing.Size(146, 146);
            this.Load += new System.EventHandler(this.DrawingBox_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.DrawingBox_Paint);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DrawingBox_MouseMove);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DrawingBox_MouseDown);
            this.Resize += new System.EventHandler(this.DrawingBox_Resize);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.DrawingBox_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
