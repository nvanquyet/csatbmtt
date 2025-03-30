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

        private void InitializeComponent() {
            txtSearch = new TextBox();
            lstChatHistory = new ListView();
            btnLogout = new Button();
            lstUserSuggestions = new ListBox();
            SuspendLayout();
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(20, 13);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(250, 23);
            txtSearch.TabIndex = 1;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            // 
            // lstChatHistory
            // 
            lstChatHistory.FullRowSelect = true;
            lstChatHistory.Location = new Point(20, 42);
            lstChatHistory.Name = "lstChatHistory";
            lstChatHistory.Size = new Size(360, 238);
            lstChatHistory.TabIndex = 3;
            lstChatHistory.UseCompatibleStateImageBehavior = false;
            lstChatHistory.View = View.Details;
            this.lstChatHistory.Columns.Add("Username", 100);
            this.lstChatHistory.Columns.Add("Last Connected Time", 159);
            this.lstChatHistory.Columns.Add("Status", 80);
            lstChatHistory.ItemActivate += OnHandshakeUserActivated;
            // 
            // btnLogout
            // 
            btnLogout.Location = new Point(305, 12);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(75, 23);
            btnLogout.TabIndex = 0;
            btnLogout.Text = "Logout";
            btnLogout.Click += BtnLogout_Click;
            // 
            // lstUserSuggestions
            // 
            lstUserSuggestions.DrawMode = DrawMode.OwnerDrawFixed;
            lstUserSuggestions.Location = new Point(20, 42);
            lstUserSuggestions.Name = "lstUserSuggestions";
            lstUserSuggestions.Size = new Size(250, 52);
            lstUserSuggestions.TabIndex = 2;
            lstUserSuggestions.Visible = false;
            lstUserSuggestions.Click += LstUserSuggestions_Click;
            lstUserSuggestions.DrawItem += lstUserSuggestions_DrawItem;
            // 
            // HomeForm
            // 
            ClientSize = new Size(400, 300);
            Controls.Add(btnLogout);
            Controls.Add(txtSearch);
            Controls.Add(lstUserSuggestions);
            Controls.Add(lstChatHistory);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "HomeForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Home";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
