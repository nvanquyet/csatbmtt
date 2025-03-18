using System;
using System.Drawing;
using System.Windows.Forms;

namespace Client.Form
{
    public partial class ChatForm : System.Windows.Forms.Form
    {
        private string _selectedFilePath = "";

        public ChatForm()
        {
            InitializeComponent();
            messageContainer.AutoScroll = true;
            LoadSampleMessages();
        }

        private void LoadSampleMessages()
        {
            AddMessage("Hello! How are you?", false);
            AddMessage("I'm good, thanks! What about you?", true);
            AddMessage("I'm doing well too!", false);
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMessage.Text)) return;
            AddMessage(txtMessage.Text, true);
            txtMessage.Clear();
        }

        private void BtnSendFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                _selectedFilePath = fileDialog.SafeFileName;
                ShowSelectedFile();
            }
        }

        private void ShowSelectedFile()
        {
            lblSelectedFile.Text = "Selected File: " + _selectedFilePath;
            lblSelectedFile.Visible = true;
            btnRemoveFile.Visible = true;
        }

        private void BtnRemoveFile_Click(object sender, EventArgs e)
        {
            _selectedFilePath = "";
            lblSelectedFile.Visible = false;
            btnRemoveFile.Visible = false;
        }

        private void AddMessage(string text, bool isMe)
        {
            // rowPanel: 1 hàng (row) cho mỗi tin nhắn
            FlowLayoutPanel rowPanel = new FlowLayoutPanel();
            rowPanel.FlowDirection = FlowDirection.LeftToRight;
            rowPanel.WrapContents = false;
            rowPanel.AutoSize = true;
            rowPanel.Margin = new Padding(7, 5, 0, 5); // Cách dưới 5px giữa các tin
            rowPanel.Padding = new Padding(0);
            rowPanel.Width = messageContainer.ClientSize.Width; // Chiều rộng bằng container

            // Bong bóng tin nhắn
            Panel messagePanel = new Panel();
            messagePanel.AutoSize = true;
            messagePanel.Padding = new Padding(0);
            messagePanel.Margin = new Padding(0);
            messagePanel.BorderStyle = BorderStyle.FixedSingle;
            messagePanel.BackColor = isMe ? Color.LightBlue : Color.LightGray;

            // Nội dung tin nhắn
            Label messageLabel = new Label();
            messageLabel.Text = text;
            messageLabel.AutoSize = true;
            messageLabel.MaximumSize = new Size(250, 0);
            messageLabel.Margin = new Padding(0);
            messageLabel.Padding = new Padding(8);
            messageLabel.Font = new Font("Arial", 10);

            messagePanel.Controls.Add(messageLabel);

            // Tính kích thước thực tế
            messagePanel.PerformLayout();
            int bubbleWidth = messagePanel.PreferredSize.Width;

            // filler: panel đệm, đẩy tin nhắn sang phải nếu là mình
            Panel filler = new Panel();
            filler.AutoSize = false;
            filler.Margin = new Padding(0);
            filler.Padding = new Padding(0);
            filler.Width = 0;
            filler.Height = 1;

            // Nếu là tin nhắn của mình => căn phải
            if (isMe)
            {
                // Tính khoảng trống còn lại (bạn có thể tăng/giảm con số 15 để chỉnh khoảng cách)
                filler.Width = rowPanel.Width - bubbleWidth - 15;
                // Thêm filler trước => đẩy bong bóng sang phải
                rowPanel.Controls.Add(filler);
                rowPanel.Controls.Add(messagePanel);
            }
            else
            {
                // Người khác => căn trái
                rowPanel.Controls.Add(messagePanel);
                filler.Width = rowPanel.Width - bubbleWidth - 15;
                rowPanel.Controls.Add(filler);
            }

            // Thêm rowPanel vào container
            messageContainer.Controls.Add(rowPanel);
            messageContainer.ScrollControlIntoView(rowPanel);
        }
    }
}