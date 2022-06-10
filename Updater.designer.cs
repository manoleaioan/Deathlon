namespace Deathlon
{
    partial class Updater
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Updater));
            this.bar = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.bar)).BeginInit();
            this.SuspendLayout();
            // 
            // bar
            // 
            this.bar.Location = new System.Drawing.Point(33, 198);
            this.bar.Margin = new System.Windows.Forms.Padding(0);
            this.bar.Name = "bar";
            this.bar.Size = new System.Drawing.Size(38, 1);
            this.bar.TabIndex = 0;
            this.bar.TabStop = false;
            // 
            // Updater
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::Deathlon.Properties.Resources.v1;
            this.ClientSize = new System.Drawing.Size(420, 241);
            this.Controls.Add(this.bar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Updater";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Updater";
            this.Load += new System.EventHandler(this.Updater_Load);
            ((System.ComponentModel.ISupportInitialize)(this.bar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox bar;
    }
}