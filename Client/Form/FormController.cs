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
    public static void ShowDialog(FormType formType, bool closeCurrent = true)
    {
        Console.WriteLine($"Showing form {formType}");
        if (!OpenForms.TryGetValue(formType, out var value))
        {
            value = CreatForm(formType);
            OpenForms.Add(formType, value);
        }
        _currentForm?.Close();
        _currentForm = value;
        value?.ShowDialog();
    }

    private static Form? CreatForm(FormType formType)
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
