namespace Client.Form;

public partial class LoginForm : System.Windows.Forms.Form
{
    public LoginForm()
    {
        InitializeComponent();
    }

    private void BtnLogin_Click(object sender, EventArgs e)
    {
        string username = txtUsername.Text;
        string password = txtPassword.Text;
        Console.WriteLine($"[Login Attempt] Username: {username}, Password Length: {password.Length}");

        MessageBox.Show("Login successful! Check console for log.");
    }

    private void BtnRegister_Click(object sender, EventArgs e)
    {
        RegisterForm registerForm = new RegisterForm();
        registerForm.ShowDialog();
    }
}