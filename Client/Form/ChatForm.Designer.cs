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
        
        private void InitializeComponent()
        {
            // Kích thước form: 800x600 (tỷ lệ 4:3)
            int formWidth = 800;
            int formHeight = 600;

            this.SuspendLayout();
            
            // 1) Tạo các control
            this.messageContainer = new FlowLayoutPanel();
            this.txtMessage = new TextBox();
            this.btnSend = new Button();
            this.btnSendFile = new Button();
            this.lblSelectedFile = new Label();
            this.btnRemoveFile = new Button();
            this.btnBack = new Button();
            this.openFileDialog = new OpenFileDialog();

            // 2) Thuộc tính Form
            this.ClientSize = new Size(formWidth, formHeight);
            this.Text = "Chat";
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // 3) Nút Back
            //    Đặt ở góc trên bên trái, kích thước vừa phải
            this.btnBack.Text = "Back";
            this.btnBack.Size = new Size(80, 30);
            this.btnBack.Location = new Point(10, 10);
            this.btnBack.Click += new EventHandler(this.BtnBack_Click);
            this.Controls.Add(this.btnBack);
            
            // 4) messageContainer
            //    Đặt bên dưới nút Back, chiếm phần lớn diện tích chính giữa để hiển thị tin nhắn
             messageContainer.AutoScroll = true;
            this.messageContainer.FlowDirection = FlowDirection.TopDown;
            this.messageContainer.WrapContents = false;
            messageContainer.HorizontalScroll.Enabled = false;
            messageContainer.HorizontalScroll.Visible = false;
            // Vị trí x = 10, y = 50 (dưới nút Back)
            // Kích thước: rộng 780, cao 400
            this.messageContainer.Location = new Point(10, 50);
            this.messageContainer.Size = new Size(formWidth - 20, 400);
            this.messageContainer.BorderStyle = BorderStyle.FixedSingle;
            this.messageContainer.Margin = new Padding(0);
            this.messageContainer.Padding = new Padding(0);
            
            this.Controls.Add(this.messageContainer);

            // 5) openFileDialog
            this.openFileDialog.Multiselect = true;
            this.openFileDialog.Filter = "All Files|*.*";
            
            // 6) lblSelectedFile
            //    Dùng để hiển thị tên file đã chọn, đặt bên dưới messageContainer
            this.lblSelectedFile.AutoSize = true;
            this.lblSelectedFile.Location = new Point(10, this.messageContainer.Bottom + 10);
            this.lblSelectedFile.Size = new Size(300, 20);
            this.lblSelectedFile.Visible = false;
            this.Controls.Add(this.lblSelectedFile);
            
            // 7) btnRemoveFile
            //    Nút xóa file đã chọn, nằm kế bên lblSelectedFile
            this.btnRemoveFile.Text = "❌";
            this.btnRemoveFile.Size = new Size(30, 25);
            this.btnRemoveFile.Location = new Point(300 + 100, this.messageContainer.Bottom + 5);
            this.btnRemoveFile.Visible = false;
            this.btnRemoveFile.Click += new EventHandler(this.BtnRemoveFile_Click);
            this.Controls.Add(this.btnRemoveFile);
            
            // 8) txtMessage
            //    Ô nhập tin nhắn, đặt gần đáy form, phía trái
            this.txtMessage.Location = new Point(10, formHeight - 60);
            this.txtMessage.Size = new Size(625, 30);
            this.txtMessage.KeyDown += new KeyEventHandler(this.TxtMessage_KeyDown);
            this.Controls.Add(this.txtMessage);
            // 10) btnSendFile
            //     Nút gửi file (hoặc chọn file), đặt kế bên nút Send
            this.btnSendFile.Text = "📎";
            this.btnSendFile.Size = new Size(50, 30);
            this.btnSendFile.Location = new Point(this.txtMessage.Right + 10, formHeight - 60);
            this.btnSendFile.Click += new EventHandler(this.BtnSendFile_Click);
            this.Controls.Add(this.btnSendFile);
            
            // 9) btnSend
            //    Nút gửi tin nhắn, đặt kế bên txtMessage
            this.btnSend.Text = "Send";
            this.btnSend.Size = new Size(80, 30);
            this.btnSend.Location = new Point(formWidth - 10 - 80, formHeight - 60);
            this.btnSend.Click += new EventHandler(this.BtnSend_Click);
            this.Controls.Add(this.btnSend);
            
           
            
            // Hoàn thiện
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void TxtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                BtnSend_Click(sender, e);
                e.SuppressKeyPress = true;
            }
        }
    }
}
