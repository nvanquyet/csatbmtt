using System.ComponentModel;

namespace Client.Form;

partial class EncryptionForm
{
        private System.ComponentModel.IContainer components = null;
        private TextBox inputTextBox;
        private ComboBox algorithmComboBox;
        private Button sendButton;
        private ListBox chatBox;

        private void InitializeComponent()
        {
            this.chatBox = new System.Windows.Forms.ListBox();
            this.inputTextBox = new System.Windows.Forms.TextBox();
            this.algorithmComboBox = new System.Windows.Forms.ComboBox();
            this.sendButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            
            // chatBox
            this.chatBox.FormattingEnabled = true;
            this.chatBox.Location = new System.Drawing.Point(10, 10);
            this.chatBox.Size = new System.Drawing.Size(360, 250);
            
            // inputTextBox
            this.inputTextBox.Location = new System.Drawing.Point(10, 270);
            this.inputTextBox.Size = new System.Drawing.Size(260, 20);
            
            // algorithmComboBox
            this.algorithmComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.algorithmComboBox.Items.AddRange(new object[] { "DES", "RSA" });
            this.algorithmComboBox.Location = new System.Drawing.Point(280, 270);
            this.algorithmComboBox.Size = new System.Drawing.Size(80, 21);
            this.algorithmComboBox.SelectedIndex = 0;
            
            // sendButton
            this.sendButton.Location = new System.Drawing.Point(10, 310);
            this.sendButton.Size = new System.Drawing.Size(75, 23);
            this.sendButton.Text = "Send";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.SendButton_Click);
            
            // ChatEncryptionForm
            this.ClientSize = new System.Drawing.Size(400, 350);
            this.Controls.Add(this.chatBox);
            this.Controls.Add(this.inputTextBox);
            this.Controls.Add(this.algorithmComboBox);
            this.Controls.Add(this.sendButton);
            this.Name = "ChatEncryptionForm";
            this.Text = "Chat Encryption";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
}
