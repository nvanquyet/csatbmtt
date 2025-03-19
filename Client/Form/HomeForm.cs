namespace Client.Form
{
    public partial class HomeForm : Form
    {
        // Giả sử danh sách toàn bộ người dùng
        private List<string> _allUsers = new List<string>(){ "Alice", "Bob", "Charlie", "Diana", "Eve" };
        
        public HomeForm()
        {
            InitializeComponent();
        }

        // Sự kiện TextChanged của ô tìm kiếm
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            // var query = txtSearch.Text.Trim();
            // if (string.IsNullOrEmpty(query))
            // {
            //     lstUserSuggestions.Visible = false;
            //     return;
            // }
            //
            // var suggestions = GetUserSuggestions(query);
            // if (suggestions.Count == 0)
            // {
            //     lstUserSuggestions.Visible = false;
            // }
            // else
            // {
            //     lstUserSuggestions.DataSource = suggestions;
            //     lstUserSuggestions.Visible = true;
            // }
        }

        // Hàm lọc danh sách người dùng dựa trên từ khóa tìm kiếm
        private List<string> GetUserSuggestions(string query)
        {
            return string.IsNullOrWhiteSpace(query) ? [] : _allUsers.Where(u => u.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Sự kiện khi người dùng bấm vào một mục trong danh sách gợi ý
        private void LstUserSuggestions_Click(object sender, EventArgs e)
        {
            // if (lstUserSuggestions.SelectedItem == null) return;
            // var selectedUser = lstUserSuggestions.SelectedItem.ToString();
            // // Ví dụ: chuyển sang ChatForm, có thể truyền selectedUser nếu cần
            // FormController.ShowForm(FormType.Chat);
            // // Ẩn danh sách gợi ý sau khi chọn
            // lstUserSuggestions.Visible = false;
        }

        // Sự kiện khi bấm vào danh sách chat history (có thể chuyển sang form chat tương ứng)
        private void LstChatHistory_Click(object sender, EventArgs e)
        {
            // // Xử lý chuyển đổi form chat nếu cần
            // FormController.ShowForm(FormType.Chat);
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            // AuthService.Logout();  
            // FormController.ShowForm(FormType.Login);
        }
    }
}
