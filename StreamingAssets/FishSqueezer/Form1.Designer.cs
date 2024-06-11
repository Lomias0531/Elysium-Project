namespace FishSqueezer
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            TextEditer = new RichTextBox();
            SuspendLayout();
            // 
            // TextEditer
            // 
            TextEditer.AllowDrop = true;
            TextEditer.BackColor = Color.FromArgb(38, 38, 38);
            TextEditer.BorderStyle = BorderStyle.None;
            TextEditer.ForeColor = Color.FromArgb(138, 138, 138);
            TextEditer.Location = new Point(0, -1);
            TextEditer.Margin = new Padding(0);
            TextEditer.Name = "TextEditer";
            TextEditer.ScrollBars = RichTextBoxScrollBars.None;
            TextEditer.Size = new Size(286, 464);
            TextEditer.TabIndex = 0;
            TextEditer.Text = "";
            TextEditer.DragDrop += Form1_FileDrop;
            TextEditer.DragEnter += Form1_DropEnter;
            TextEditer.MouseDoubleClick += Form1_MouseDoubleClick;
            TextEditer.MouseDown += Form1_MouseDown;
            TextEditer.MouseMove += Form1_MouseMove;
            TextEditer.MouseUp += Form1_MouseUp;
            TextEditer.MouseWheel += Form1_MouseWheelMove;
            // 
            // Form1
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(284, 461);
            Controls.Add(TextEditer);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Form1";
            ShowIcon = false;
            ShowInTaskbar = false;
            Text = "Form1";
            TopMost = true;
            Load += Form1_Load;
            DragDrop += Form1_FileDrop;
            DragEnter += Form1_DropEnter;
            MouseDoubleClick += Form1_MouseDoubleClick;
            MouseDown += Form1_MouseDown;
            MouseMove += Form1_MouseMove;
            MouseUp += Form1_MouseUp;
            MouseWheel += Form1_MouseWheelMove;
            ResumeLayout(false);
        }

        #endregion

        private RichTextBox TextEditer;
    }
}
