namespace ServerList
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.RichTextBox rtbContent;
        private System.Windows.Forms.Button btnSave;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            txtFilePath = new TextBox();
            btnBrowse = new Button();
            rtbContent = new RichTextBox();
            btnSave = new Button();
            SuspendLayout();
            // 
            // txtFilePath
            // 
            txtFilePath.Location = new Point(10, 10);
            txtFilePath.Name = "txtFilePath";
            txtFilePath.ReadOnly = true;
            txtFilePath.Size = new Size(586, 23);
            txtFilePath.TabIndex = 0;
            txtFilePath.Click += BtnBrowse_Click;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(602, 8);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(61, 25);
            btnBrowse.TabIndex = 1;
            btnBrowse.Text = "...";
            // 
            // rtbContent
            // 
            rtbContent.Font = new Font("Consolas", 10F);
            rtbContent.Location = new Point(10, 45);
            rtbContent.Name = "rtbContent";
            rtbContent.Size = new Size(778, 537);
            rtbContent.TabIndex = 3;
            rtbContent.Text = "";
            rtbContent.WordWrap = false;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(669, 8);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(119, 25);
            btnSave.TabIndex = 2;
            btnSave.Text = "Save";
            // 
            // Form1
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 594);
            Controls.Add(txtFilePath);
            Controls.Add(btnBrowse);
            Controls.Add(btnSave);
            Controls.Add(rtbContent);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            Text = "Server List Editor";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
