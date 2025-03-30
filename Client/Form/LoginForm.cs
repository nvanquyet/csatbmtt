using System.Net.Sockets;
using Client.Network;
using Client.Network.Tcp;
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

    private void BtnLogin_Click(object? sender, EventArgs? e)
    {
        //Check to reconnect
        if (!NetworkManager.Instance.TcpService.IsRunning)
        {
            _ = NetworkManager.Instance.StartTcp(() => BtnLogin_Click(null, null));
        }
        var username = txtUsername.Text;
        var password = txtPassword.Text;
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Username or password are required");
            return;
        }
        AuthService.Login(username, password);
    }

    private void BtnRegister_Click(object sender, EventArgs e)
    {
        FormController.Show(FormType.Register);
        Logger.LogInfo($"Tcp Connected ? {(NetworkManager.Instance.TcpService as TcpProtocol)?.IsConnected}");
    }
}