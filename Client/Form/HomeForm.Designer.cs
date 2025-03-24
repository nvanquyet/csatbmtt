using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;

namespace Client.Form
{
    partial class HomeForm
    {
        private IContainer components = null;
        private TextBox txtSearch;
        private ListView lstChatHistory;
        private Button btnLogout;           // Nút Đăng xuất
        private ListBox lstUserSuggestions; // Danh sách gợi ý người dùng

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtSearch = new TextBox();
            this.lstChatHistory = new ListView();
            this.btnLogout = new Button();
            this.lstUserSuggestions = new ListBox();
            this.SuspendLayout();
            // 
            // Form Settings
            // 
            this.Text = "Home";
            this.ClientSize = new System.Drawing.Size(400, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            // 
            // btnLogout
            // 
            this.btnLogout.Text = "Logout";
            this.btnLogout.Size = new Size(75, 23);
            this.btnLogout.Location = new Point(310, 10); // Góc trên bên phải
            this.btnLogout.Click += new System.EventHandler(this.BtnLogout_Click);
            this.Controls.Add(this.btnLogout);
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new Point(20, 40);
            this.txtSearch.Width = 250;
            this.txtSearch.TextChanged += new System.EventHandler(this.TxtSearch_TextChanged);
            this.Controls.Add(this.txtSearch);
            // 
            // lstUserSuggestions
            // 
            this.lstUserSuggestions.Location = new Point(20, 65);
            this.lstUserSuggestions.Size = new Size(340, 60);
            this.lstUserSuggestions.Visible = false; // Ẩn ban đầu, sẽ hiển thị khi có dữ liệu gợi ý
            this.lstUserSuggestions.Click += new System.EventHandler(this.LstUserSuggestions_Click);
            this.Controls.Add(this.lstUserSuggestions);
            // 
            // lstChatHistory
            // 
            this.lstChatHistory.Location = new Point(20, 130);
            this.lstChatHistory.Size = new Size(360, 150);
            this.lstChatHistory.View = View.Details;
            this.lstChatHistory.FullRowSelect = true;
            this.lstChatHistory.Columns.Add("Username", 120);
            this.lstChatHistory.Columns.Add("Last Message", 220);
            this.lstChatHistory.Columns.Add("Status", 80); // Thêm cột Status
            this.lstChatHistory.ItemActivate += new System.EventHandler(this.OnHandshakeUserActivated);
            this.Controls.Add(this.lstChatHistory);
            // 
            // HomeForm
            // 
            this.ResumeLayout(false);
            this.PerformLayout();
            FormController.InitializeUiContext();
        }
    }
}
