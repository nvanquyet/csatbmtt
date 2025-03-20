namespace Client.Form;

public enum FormType
{
    Login,
    Register,
    Home,
    Chat
}

public static class FormController
{
    private static readonly Dictionary<FormType, Form?> OpenForms = new();
    private static Form? _currentForm;

    /// <summary>
    /// Hiển thị form theo loại (FormType).
    /// </summary>
    public static void Show(FormType formType)
    {
        _ = Task.Run(() => SwitchToFormAsync(formType));
    }
    private static async Task SwitchToFormAsync(FormType formType, bool closeCurrent = true)
    {
        Console.WriteLine($"Switching to form {formType}");
        if (!OpenForms.TryGetValue(formType, out Form? newForm))
        {
            newForm = CreateForm(formType);
            OpenForms.Add(formType, newForm);
        }

        // Sử dụng Task.Yield để cho phép UI thread xử lý các message pending trước khi chuyển đổi
        await Task.Yield();

        if (_currentForm != null)
        {
            if (closeCurrent)
            {
                if (_currentForm.InvokeRequired)
                {
                    await _currentForm.InvokeAsync(new Action(() => _currentForm.Close()));
                }
                else
                {
                    _currentForm.Close();
                }
            }
            else
            {
                if (_currentForm.InvokeRequired)
                {
                    await _currentForm.InvokeAsync(new Action(() => _currentForm.Hide()));
                }
                else
                {
                    _currentForm.Hide();
                }
            }
        }

        _currentForm = newForm;

        if (newForm is { InvokeRequired: true })
        {
            await newForm.InvokeAsync(new Action(() =>
            {
                newForm.Show();
                newForm.BringToFront();
            }));
        }
        else
        {
            newForm?.Show();
            newForm.BringToFront();
        }
    }


    private static Form? CreateForm(FormType formType)
    {
        return formType switch
        {
            FormType.Login => new LoginForm(),
            FormType.Register => new RegisterForm(),
            FormType.Home => new HomeForm(),
            FormType.Chat => new ChatForm(),
            _ => throw new ArgumentOutOfRangeException(nameof(formType), formType, null)
        };
    }

    /// <summary>
    /// Lấy form theo FormType.
    /// </summary>
    public static System.Windows.Forms.Form? GetForm(FormType formType) => OpenForms.GetValueOrDefault(formType);

    /// <summary>
    /// Lấy form theo FormType.
    /// </summary>
    public static T? GetForm<T>(FormType formType) where T : Form => OpenForms.GetValueOrDefault(formType) as T;

    /// <summary>
    /// Đóng một form theo FormType.
    /// </summary>
    public static void CloseForm(FormType formType)
    {
        if (!OpenForms.TryGetValue(formType, out var value)) return;
        value?.Close();
        OpenForms.Remove(formType);
    }
}

public class Form : System.Windows.Forms.Form
{
}