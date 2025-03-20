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
    public static void Show(FormType formType, bool closeCurrent = true)
    {
        // Lấy form từ danh sách đã mở hoặc tạo mới nếu chưa có
        if (!OpenForms.TryGetValue(formType, out var newForm))
        {
            newForm = CreateForm(formType);
            OpenForms.Add(formType, newForm);
        }

        // Nếu có form hiện tại, ẩn hoặc đóng tùy thuộc vào tham số closeCurrent
        if (_currentForm != null)
        {
            if (closeCurrent)
            {
                if (_currentForm.InvokeRequired)
                    _currentForm.Invoke(new Action(() => _currentForm.Hide()));
                else
                    _currentForm.Hide();
            }
            else
            {
                if (_currentForm.InvokeRequired)
                    _currentForm.Invoke(new Action(() => _currentForm.Hide()));
                else
                    _currentForm.Hide();
            }
        }

        _currentForm = newForm;

        if (newForm != null)
        {
            // Nếu form mới được gọi từ luồng không phải UI, chuyển qua UI thread
            if (newForm.InvokeRequired)
            {
                newForm.Invoke(new Action(() =>
                {
                    newForm.Show();
                    newForm.BringToFront();
                }));
            }
            else
            {
                newForm.Show();
                newForm.BringToFront();
            }
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