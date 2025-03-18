namespace Client.Form
{
   public partial class ChatForm : System.Windows.Forms.Form
    {
        private FlowLayoutPanel messageContainer;
        private TextBox txtMessage;
        private Button btnSend;
        private Button btnSendFile;
        private Label lblSelectedFile;
        private Button btnRemoveFile;
        private OpenFileDialog openFileDialog;
        
        private void InitializeComponent()
        {
            this.messageContainer = new();
            this.txtMessage = new();
            this.btnSend = new();
            this.btnSendFile = new();
            this.lblSelectedFile = new();
            this.btnRemoveFile = new();
            this.openFileDialog = new ();
            
            this.SuspendLayout();
            
            // Message Container
            this.messageContainer = new FlowLayoutPanel();
            this.messageContainer.AutoScroll = true;
            this.messageContainer.FlowDirection = FlowDirection.TopDown;
            this.messageContainer.WrapContents = false;
            this.messageContainer.Location = new Point(10, 10);
            this.messageContainer.Size = new Size(380, 300);
            this.messageContainer.BorderStyle = BorderStyle.FixedSingle;

            // Xóa hoặc giảm margin/padding để bớt khoảng trống
            this.messageContainer.Margin = new Padding(0);
            this.messageContainer.Padding = new Padding(0);
            
            //File Dialog 
            this.openFileDialog.Multiselect = true;
            this.openFileDialog.Filter = "All Files|*.*"; // Có thể chỉnh sửa định dạng file
            
            // Selected File Label
            this.lblSelectedFile.AutoSize = true;
            this.lblSelectedFile.Location = new Point(10, 320);
            this.lblSelectedFile.Size = new Size(250, 20);
            this.lblSelectedFile.Visible = false;

            // Remove File Button
            this.btnRemoveFile.Text = "❌";
            this.btnRemoveFile.Size = new Size(30, 25);
            this.btnRemoveFile.Location = new Point(this.lblSelectedFile.Right + 10, 315); // Dịch ra xa hơn
            this.btnRemoveFile.Visible = false;
            this.btnRemoveFile.Click += new EventHandler(this.BtnRemoveFile_Click);
            
            // Textbox Message
            this.txtMessage.Location = new Point(10, 350);
            this.txtMessage.Size = new Size(260, 25);
            
            // Send Button
            this.btnSend.Text = "Send";
            this.btnSend.Size = new Size(60, 25);
            this.btnSend.Location = new Point(280, 350);
            this.btnSend.Click += new EventHandler(this.BtnSend_Click);
            
            // Send File Button
            this.btnSendFile.Text = "📎";
            this.btnSendFile.Size = new Size(40, 25);
            this.btnSendFile.Location = new Point(340, 350);
            this.btnSendFile.Click += new EventHandler(this.BtnSendFile_Click);
            
            // ChatForm Settings
            this.ClientSize = new Size(400, 400);
            this.Controls.Add(this.messageContainer);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.btnSendFile);
            this.Controls.Add(this.lblSelectedFile);
            this.Controls.Add(this.btnRemoveFile);
            this.Text = "Chat";
            this.StartPosition = FormStartPosition.CenterScreen;
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
