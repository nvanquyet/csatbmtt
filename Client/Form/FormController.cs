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
    public static void Show(FormType formType)
    {
        if (SynchronizationContext.Current != _uiSyncContext)
        {
            _uiSyncContext?.Send(_ => Show(formType), null);
            return;
        }

        // Nếu có form hiện tại, đóng và xóa nó trước khi mở form mới
        if (_currentForm != null)
        {
            _currentForm.Close();
            _currentForm.Dispose();
            _currentForm = null;
        }

        // Luôn tạo một form mới thay vì kiểm tra danh sách OpenForms
        var newForm = CreateForm(formType);
        _currentForm = newForm;

        OpenForms[formType] = newForm; // Lưu lại form mới

        newForm.Show();
        newForm.BringToFront();
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
