namespace Client.Form
{
    partial class RegisterForm
    {
        private System.ComponentModel.IContainer components = null;
        private Panel panelContainer;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblPassword;
        private TextBox txtPassword;
        private Label lblConfirmPassword;
        private TextBox txtConfirmPassword;
        private Button btnRegister;
        private Button btnCancel;

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
            this.lblConfirmPassword = new Label();
            this.txtConfirmPassword = new TextBox();
            this.btnRegister = new Button();
            this.btnCancel = new Button();

            // Form Settings
            this.Text = "Register";
            this.ClientSize = new System.Drawing.Size(400, 280);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Panel Container
            panelContainer.Size = new System.Drawing.Size(350, 230);
            panelContainer.Location = new System.Drawing.Point(25, 25);
            this.Controls.Add(panelContainer);

            // Username Label
            lblUsername.Text = "Username:";
            lblUsername.Location = new System.Drawing.Point(20, 30);
            panelContainer.Controls.Add(lblUsername);

            // Username TextBox
            txtUsername.Location = new System.Drawing.Point(150, 25);
            txtUsername.Width = 180;
            panelContainer.Controls.Add(txtUsername);

            // Password Label
            lblPassword.Text = "Password:";
            lblPassword.Location = new System.Drawing.Point(20, 70);
            panelContainer.Controls.Add(lblPassword);

            // Password TextBox
            txtPassword.Location = new System.Drawing.Point(150, 65);
            txtPassword.Width = 180;
            txtPassword.PasswordChar = '*';
            panelContainer.Controls.Add(txtPassword);

            // Confirm Password Label
            lblConfirmPassword.Text = "Confirm Password:";
            lblConfirmPassword.Location = new System.Drawing.Point(20, 110);
            panelContainer.Controls.Add(lblConfirmPassword);

            // Confirm Password TextBox
            txtConfirmPassword.Location = new System.Drawing.Point(150, 105);
            txtConfirmPassword.Width = 180;
            txtConfirmPassword.PasswordChar = '*';
            panelContainer.Controls.Add(txtConfirmPassword);

            // Register Button
            btnRegister.Text = "Register";
            btnRegister.Location = new System.Drawing.Point(60, 160);
            btnRegister.Width = 100;
            btnRegister.Click += new EventHandler(this.BtnRegister_Click);
            panelContainer.Controls.Add(btnRegister);

            // Cancel Button
            btnCancel.Text = "Cancel";
            btnCancel.Location = new System.Drawing.Point(180, 160);
            btnCancel.Width = 100;
            btnCancel.Click += new EventHandler(this.BtnCancel_Click);
            panelContainer.Controls.Add(btnCancel);
            
            this.ResumeLayout(false);
            this.PerformLayout();
            FormController.InitializeUiContext();
        }
    }
}