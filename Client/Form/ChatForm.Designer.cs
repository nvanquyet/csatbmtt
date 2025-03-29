using System;
using System.Drawing;
using System.Windows.Forms;

namespace Client.Form
{
    partial class ChatForm
    {
        private FlowLayoutPanel messageContainer;
        private TextBox txtMessage;
        private Button btnSend;
        private Button btnSendFile;
        private Label lblSelectedFile;
        private Button btnRemoveFile;
        private Button btnBack;
        private OpenFileDialog openFileDialog;
        private TextBox txtDesKey;           // Nhập DES key
        private Label lblEncryptionStatus;   // Hiển thị thông báo mã hóa thành công và thời gian mã hóa
        private ProgressBar progressFileSending; // Hiển thị tiến trình gửi file
        private Button btnCancelSendFile;    // Nút hủy tiến trình gửi file

        private void InitializeComponent() {
            messageContainer = new FlowLayoutPanel();
            txtMessage = new TextBox();
            btnSend = new Button();
            btnSendFile = new Button();
            lblSelectedFile = new Label();
            btnRemoveFile = new Button();
            btnBack = new Button();
            openFileDialog = new OpenFileDialog();
            txtDesKey = new TextBox();
            lblEncryptionStatus = new Label();
            progressFileSending = new ProgressBar();
            btnCancelSendFile = new Button();
            btnRandomDesKey = new Button();
            SuspendLayout();
            // 
            // messageContainer
            // 
            messageContainer.AutoScroll = true;
            messageContainer.BorderStyle = BorderStyle.FixedSingle;
            messageContainer.FlowDirection = FlowDirection.TopDown;
            messageContainer.Location = new Point(10, 50);
            messageContainer.Margin = new Padding(0);
            messageContainer.Name = "messageContainer";
            messageContainer.Size = new Size(800, 400);
            messageContainer.TabIndex = 1;
            messageContainer.WrapContents = false;
            // 
            // txtMessage
            // 
            txtMessage.Location = new Point(12, 557);
            txtMessage.Name = "txtMessage";
            txtMessage.Size = new Size(661, 23);
            txtMessage.TabIndex = 4;
            txtMessage.KeyDown += TxtMessage_KeyDown;
            // 
            // btnSend
            // 
            btnSend.Location = new Point(735, 557);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(80, 30);
            btnSend.TabIndex = 6;
            btnSend.Text = "Send";
            btnSend.Click += BtnSend_Click;
            // 
            // btnSendFile
            // 
            btnSendFile.Location = new Point(679, 557);
            btnSendFile.Name = "btnSendFile";
            btnSendFile.Size = new Size(50, 30);
            btnSendFile.TabIndex = 5;
            btnSendFile.Text = "📎";
            btnSendFile.Click += BtnSendFile_Click;
            // 
            // lblSelectedFile
            // 
            lblSelectedFile.AutoSize = true;
            lblSelectedFile.Location = new Point(10, 450);
            lblSelectedFile.Name = "lblSelectedFile";
            lblSelectedFile.Size = new Size(0, 15);
            lblSelectedFile.TabIndex = 2;
            // 
            // btnRemoveFile
            // 
            btnRemoveFile.Location = new Point(300, 450);
            btnRemoveFile.Name = "btnRemoveFile";
            btnRemoveFile.Size = new Size(30, 25);
            btnRemoveFile.TabIndex = 3;
            btnRemoveFile.Text = "❌";
            btnRemoveFile.Click += BtnRemoveFile_Click;
            // 
            // btnBack
            // 
            btnBack.Location = new Point(10, 10);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(80, 30);
            btnBack.TabIndex = 0;
            btnBack.Text = "Back";
            btnBack.Click += BtnBack_Click;
            // 
            // openFileDialog
            // 
            openFileDialog.Filter = "All Files|*.*";
            openFileDialog.Multiselect = true;
            // 
            // txtDesKey
            // 
            txtDesKey.Location = new Point(10, 528);
            txtDesKey.Name = "txtDesKey";
            txtDesKey.PlaceholderText = "Enter DES Key";
            txtDesKey.Size = new Size(150, 23);
            txtDesKey.TabIndex = 8;
            // 
            // lblEncryptionStatus
            // 
            lblEncryptionStatus.AutoSize = true;
            lblEncryptionStatus.Font = new Font("Arial", 9F, FontStyle.Bold);
            lblEncryptionStatus.ForeColor = Color.Green;
            lblEncryptionStatus.Location = new Point(780, 50);
            lblEncryptionStatus.Name = "lblEncryptionStatus";
            lblEncryptionStatus.Size = new Size(0, 15);
            lblEncryptionStatus.TabIndex = 9;
            // 
            // progressFileSending
            // 
            progressFileSending.Location = new Point(10, 490);
            progressFileSending.Name = "progressFileSending";
            progressFileSending.Size = new Size(764, 25);
            progressFileSending.TabIndex = 10;
            // 
            // btnCancelSendFile
            // 
            btnCancelSendFile.Location = new Point(780, 490);
            btnCancelSendFile.Name = "btnCancelSendFile";
            btnCancelSendFile.Size = new Size(30, 25);
            btnCancelSendFile.TabIndex = 11;
            btnCancelSendFile.Text = "❌";
            btnCancelSendFile.Click += BtnCancelSendFile_Click;
            // 
            // btnRandomDesKey
            // 
            btnRandomDesKey.Location = new Point(166, 523);
            btnRandomDesKey.Name = "btnRandomDesKey";
            btnRandomDesKey.Size = new Size(100, 30);
            btnRandomDesKey.TabIndex = 12;
            btnRandomDesKey.Text = "RandomKey";
            btnRandomDesKey.Click += OnClickRandomKey;
            // 
            // ChatForm
            // 
            ClientSize = new Size(819, 593);
            Controls.Add(btnRandomDesKey);
            Controls.Add(btnBack);
            Controls.Add(messageContainer);
            Controls.Add(lblSelectedFile);
            Controls.Add(btnRemoveFile);
            Controls.Add(txtMessage);
            Controls.Add(btnSendFile);
            Controls.Add(btnSend);
            Controls.Add(txtDesKey);
            Controls.Add(lblEncryptionStatus);
            Controls.Add(progressFileSending);
            Controls.Add(btnCancelSendFile);
            Name = "ChatForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Chat";
            ResumeLayout(false);
            PerformLayout();
        }

        private void TxtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                BtnSend_Click(sender, e);
                e.SuppressKeyPress = true;
            }
        }
        private Button btnRandomDesKey;
    }
}
