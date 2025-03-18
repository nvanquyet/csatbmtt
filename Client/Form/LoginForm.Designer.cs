namespace Client.Form
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;
        private Panel panelContainer;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblPassword;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnRegister;

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
            this.panelContainer = new Panel();
            this.lblUsername = new Label();
            this.txtUsername = new TextBox();
            this.lblPassword = new Label();
            this.txtPassword = new TextBox();
            this.btnLogin = new Button();
            this.btnRegister = new Button();

            // Form Settings
            this.Text = "Login";
            this.ClientSize = new System.Drawing.Size(400, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Panel Container
            panelContainer.Size = new System.Drawing.Size(350, 200);
            panelContainer.Location = new System.Drawing.Point(25, 25);
            
            this.Controls.Add(panelContainer);

            // Username Label
            lblUsername.Text = "Username:";
            lblUsername.Location = new System.Drawing.Point(20, 30);
            panelContainer.Controls.Add(lblUsername);

            // Username TextBox
            txtUsername.Location = new System.Drawing.Point(120, 25);
            txtUsername.Width = 200;
            panelContainer.Controls.Add(txtUsername);

            // Password Label
            lblPassword.Text = "Password:";
            lblPassword.Location = new System.Drawing.Point(20, 70);
            panelContainer.Controls.Add(lblPassword);

            // Password TextBox
            txtPassword.Location = new System.Drawing.Point(120, 65);
            txtPassword.Width = 200;
            txtPassword.PasswordChar = '*';
            panelContainer.Controls.Add(txtPassword);

            // Login Button
            btnLogin.Text = "Login";
            btnLogin.Location = new System.Drawing.Point(60, 120);
            btnLogin.Width = 100;
            btnLogin.Click += new EventHandler(this.BtnLogin_Click);
            panelContainer.Controls.Add(btnLogin);

            // Register Button
            btnRegister.Text = "Register";
            btnRegister.Location = new System.Drawing.Point(180, 120);
            btnRegister.Width = 100;
            btnRegister.Click += new EventHandler(this.BtnRegister_Click);
            panelContainer.Controls.Add(btnRegister);
        }
    }
}
