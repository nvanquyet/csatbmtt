using System.ComponentModel;

namespace Client.Form;

partial class HomeForm
{
        private System.ComponentModel.IContainer components = null;
        private TextBox txtSearch;
        private Button btnSearch;
        private ListView lstChatHistory;

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
            this.btnSearch = new Button();
            this.lstChatHistory = new ListView();

            // Form Settings
            this.Text = "Home";
            this.ClientSize = new System.Drawing.Size(400, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Search TextBox
            txtSearch.Location = new System.Drawing.Point(20, 20);
            txtSearch.Width = 250;
            this.Controls.Add(txtSearch);

            // Search Button
            btnSearch.Text = "Search";
            btnSearch.Location = new System.Drawing.Point(280, 18);
            btnSearch.Click += new EventHandler(this.BtnSearch_Click);
            this.Controls.Add(btnSearch);

            // Chat History ListView
            lstChatHistory.Location = new System.Drawing.Point(20, 60);
            lstChatHistory.Size = new System.Drawing.Size(360, 200);
            lstChatHistory.View = View.Details;
            lstChatHistory.FullRowSelect = true;
            lstChatHistory.Columns.Add("Username", 120);
            lstChatHistory.Columns.Add("Last Message", 220);
            this.Controls.Add(lstChatHistory);
        }
    
}