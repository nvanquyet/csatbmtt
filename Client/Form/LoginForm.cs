using Client.Network;
using Client.Services;
using Shared;

namespace Client.Form;

public partial class LoginForm : Form
{
    public bool RememberMe => chkRememberMe.Checked;
    public LoginForm()
    {
        InitializeComponent();
    }

    public static void TryLogin() => AuthService.TryAutoLogin(NetworkManager.Instance.TcpService);

    private void BtnLogin_Click(object sender, EventArgs e)
    {
        var username = txtUsername.Text;
        var password = txtPassword.Text;
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Username or password are required");
            return;
        }
        Logger.LogInfo($"[Login Attempt] Username: {username}, Password Length: {password.Length}");
        AuthService.Login(username, password);
    }

    private void BtnRegister_Click(object sender, EventArgs e)
    {
        FormController.Show(FormType.Register);
    }
}