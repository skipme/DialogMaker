namespace DialogMaker
{
    partial class Dialogs
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
            // Dialogs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "Dialogs";
            this.Size = new System.Drawing.Size(522, 325);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Dialogs_Paint);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Dialogs_MouseMove);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Dialogs_MouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Dialogs_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
