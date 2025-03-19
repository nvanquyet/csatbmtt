using Timer = System.Windows.Forms.Timer;

namespace Client.Form;

public partial class BackgroundForm : Form
{
    public BackgroundForm()
    {
        InitializeComponent();
        this.Load += BackgroundForm_Load;
        FormController.ShowDialog(FormType.Login);
        MonitorDialogsAndExit();
    }


    private void BackgroundForm_Load(object? sender, EventArgs e)
    {
        this.Hide();
        this.Opacity = 0;
        this.WindowState = FormWindowState.Minimized;
    }
    
    /// <summary>
    /// Hàm này sẽ kiểm tra liên tục (mỗi 500ms) xem có form nào mở không.
    /// Nếu không có form nào mở, nó sẽ gọi Application.Exit() để thoát ứng dụng.
    /// </summary>
    private void MonitorDialogsAndExit()
    {
        var timer = new Timer();
        timer.Interval = 500; // kiểm tra mỗi 500ms
        timer.Tick += (sender, e) =>
        {
            if (Application.OpenForms.Count > 1) return;
            timer.Stop();
            Console.WriteLine($"Exit");
            Application.Exit();
        };
        timer.Start();
    }
}