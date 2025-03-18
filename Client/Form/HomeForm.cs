namespace Client.Form
{
    public partial class HomeForm : System.Windows.Forms.Form
    {
        public HomeForm()
        {
            InitializeComponent();
            LoadChatHistory();
        }

        private void LoadChatHistory()
        {
            // Sample chat data
            string[,] chatData = {
                {"Alice", "Hey, how are you?"},
                {"Bob", "Let's meet up tomorrow."},
                {"Charlie", "Did you see the news?"},
                {"Diana", "I'll call you later."},
                {"Eve", "Great job on the project!"}
            };

            lstChatHistory.Items.Clear();
            for (int i = 0; i < chatData.GetLength(0); i++)
            {
                ListViewItem item = new ListViewItem(chatData[i, 0]);
                item.SubItems.Add(chatData[i, 1]);
                lstChatHistory.Items.Add(item);
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string searchQuery = txtSearch.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                MessageBox.Show("Please enter a username to search.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (ListViewItem item in lstChatHistory.Items)
            {
                if (item.Text.ToLower().Contains(searchQuery))
                {
                    item.Selected = true;
                    lstChatHistory.Select();
                    return;
                }
            }
            MessageBox.Show("User not found.", "Search", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}