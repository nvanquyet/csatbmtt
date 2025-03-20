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
    private static SynchronizationContext? _uiSyncContext = SynchronizationContext.Current;
    public static void InitializeUiContext()
    {
        _uiSyncContext = SynchronizationContext.Current;
        Console.WriteLine($"SynchronizationContext: {_uiSyncContext} from {_currentForm?.GetType().Name}");
    }
    /// <summary>
    /// Hiển thị form theo loại (FormType).
    /// </summary>
    public static void Show(FormType formType, bool closeCurrent = true)
    {
        // Đảm bảo thực thi trên UI thread
        if (SynchronizationContext.Current != _uiSyncContext)
        {
            _uiSyncContext?.Send(_ => Show(formType, closeCurrent), null);
            return;
        }

        // Lấy form từ danh sách đã mở hoặc tạo mới nếu chưa có
        if (!OpenForms.TryGetValue(formType, out var newForm))
        {
            newForm = CreateForm(formType);
            OpenForms.Add(formType, newForm);
        }

        var oldForm = _currentForm;
        _currentForm = newForm;

        if (newForm != null)
        {
            newForm.Show();
            newForm.BringToFront();
        }

        // Xử lý form cũ
        if (oldForm != null && oldForm != newForm)
        {
            if (closeCurrent)
            {
                oldForm.Close();
            }
            else
            {
                oldForm.Hide();
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