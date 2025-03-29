using System.Security.Cryptography;
using System.Text;
using Shared;
using Shared.Security.Interface;
using Shared.Services;
using Shared.Utils;

namespace Client.Form;

public partial class EncryptionForm : Form
{
    public EncryptionForm()
    {
        InitializeComponent();
    }
    private void SendButton_Click(object sender, EventArgs e)
    {
        string input = inputTextBox.Text;
        var algorithm = algorithmComboBox.SelectedItem?.ToString();
        if (algorithm == "RSA")
        {
            input = ByteUtils.GetStringFromBytes(EncryptionService.Instance.GetAlgorithm(EncryptionType.Des).EncryptKey);
            Logger.LogInfo($"Input length {input.Length}");
        }
        // Kiểm tra dữ liệu đầu vào
        if (string.IsNullOrEmpty(algorithm) )
        {
            MessageBox.Show("Vui lòng chọn thuật toán!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
    
        if (string.IsNullOrEmpty(input))
        {
            MessageBox.Show("Vui lòng nhập văn bản!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
    
        try
        {
            // Chuyển text sang byte[] (UTF8)
            byte[]? inputBytes = Encoding.UTF8.GetBytes(input);

            // Mã hóa
            byte[]? encryptedBytes = EncryptText(inputBytes, algorithm);
            string encryptedText = Convert.ToBase64String(encryptedBytes);

            // Giải mã
            byte[]? decryptedBytes = DecryptText(encryptedBytes, algorithm);
            string decryptedText = Encoding.UTF8.GetString(decryptedBytes);

            // Hiển thị kết quả
            chatBox.Items.Add($"[Bạn]: {input}");
            chatBox.Items.Add($"[Mã hóa]: {encryptedText}");
            chatBox.Items.Add($"[Giải mã]: {decryptedText}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi mã hóa: {ex.Message}\nChi tiết: {ex.StackTrace}", 
                "Lỗi nghiêm trọng", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Error);
        }
    
        chatBox.Items.Add(new string('-', 50));
        inputTextBox.Clear();
    }
    private byte[]? EncryptText(byte[]? input, string algorithm)
    {
        if (algorithm == "DES")
            return EncryptDes(input);
        else if (algorithm == "RSA")
            return EncryptRsa(input);
        return [];
    }
    
    private byte[]? DecryptText(byte[]? input, string algorithm)
    {
        if (algorithm == "DES")
            return DecryptDes(input);
        else if (algorithm == "RSA")
            return DecryptRsa(input);
        return [];
    }

    private byte[]? EncryptDes(byte[]? input)
    {
        try
        {
            var des = EncryptionService.Instance.GetAlgorithm(EncryptionType.Des);
            return des.Encrypt(input, des.EncryptKey);
            return [];
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Encryption error: {ex.Message}");
            return [];
        }
    }

    private byte[]? DecryptDes(byte[]? input)
    {
        try
        {
            var des = EncryptionService.Instance.GetAlgorithm(EncryptionType.Des);
            return des.Decrypt(input, des.DecryptKey);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Decryption error: {ex.Message}");
            return [];
        }
    }

    private byte[]? EncryptRsa(byte[]? input)
    {
        var rsa = EncryptionService.Instance.GetAlgorithm(EncryptionType.Rsa);
        return rsa.Encrypt(input, rsa.EncryptKey);
    }
    

    private byte[]? DecryptRsa(byte[]? input)
    {
        var rsa = EncryptionService.Instance.GetAlgorithm(EncryptionType.Rsa);
        return rsa.Decrypt(input, rsa.DecryptKey);
    }

}