using Client.Services;
using Shared;

namespace Client.Form
{
    public partial class RegisterForm : Form
    {
        public RegisterForm()
        {
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            var username = txtUsername.Text;
            var password = txtPassword.Text;
            var confirmPassword = txtConfirmPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show(@"Please fill in all fields.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show(@"Passwords do not match.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Log event (in a real application, you'd send data to a server or database)
            AuthService.Register(username, password);
        }

        private void BtnCancel_Click(object sender, EventArgs e) =>  this.Close();
    }
}