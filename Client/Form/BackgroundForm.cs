﻿using Client.Models;
using Client.Network;
using Shared;
using Shared.Models;
using Timer = System.Windows.Forms.Timer;

namespace Client.Form;

public partial class BackgroundForm : Form
{
    public BackgroundForm()
    {
        InitializeComponent();
        this.Load += BackgroundForm_Load;
        FormController.Show(FormType.Login);
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
        timer.Tick += async (sender, e) =>
        {
            if (Application.OpenForms.Count > 1) return;
            timer.Stop();
            var response = new MessageNetwork<UserDto>(type: CommandType.ClientDisconnect, code: StatusCode.Success,
                data: SessionManager.GetUserDto()).ToJson();
            _ = Task.Run(() =>
            {
                NetworkManager.Instance.TcpService.Send(response);
                NetworkManager.Instance.TcpService.Stop();
                NetworkManager.Instance.UdpService.Stop();
            });
            Logger.LogWarning($"Exit");
            //Delay more 2s to exit
            await Task.Delay(100); 
            Application.Exit();
            Environment.Exit(0);
        };
        timer.Start();
    }
}