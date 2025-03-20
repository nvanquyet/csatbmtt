using Client.Models;
using Client.Network;
using Client.Services;
using MongoDB.Bson;
using Shared.Models;

namespace Client.Form
{
    public partial class HomeForm : Form
    {
        // Giả sử danh sách toàn bộ người dùng
        private List<UserDto> _allUsers = [];

        public void SetAllUsers(List<UserDto> users)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => { _allUsers = users; }));
            }
            else
            {
                _allUsers = users;
            }
        }

        private readonly Form _waitingForm;

        public HomeForm()
        {
            using (_waitingForm = new Form())
            {
                _waitingForm.FormBorderStyle = FormBorderStyle.None;
                _waitingForm.StartPosition = FormStartPosition.CenterParent;
                _waitingForm.Size = new Size(250, 100);
                _waitingForm.ControlBox = false;
                _waitingForm.ShowInTaskbar = false;

                var lblWait = new Label()
                {
                    Text = "Please wait, handshake in progress...",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 10, FontStyle.Bold)
                };
                _waitingForm.Controls.Add(lblWait);
            }

            //Send GetAvailable User
            var getAllAvailableUser = new MessageNetwork<UserDto>(type: CommandType.GetAvailableClients,
                code: StatusCode.Success,
                data: SessionManager.GetUserDto()).ToJson();
            NetworkManager.Instance.TcpService.Send(getAllAvailableUser);
            var getUserShake = new MessageNetwork<UserDto>(type: CommandType.GetUserShake, code: StatusCode.Success,
                data: SessionManager.GetUserDto()).ToJson();
            NetworkManager.Instance.TcpService.Send(getUserShake);
            
            
            InitializeComponent();
        }

        #region UserSuggestion

        // Sự kiện TextChanged của ô tìm kiếm
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            var query = txtSearch.Text.Trim();
            Console.WriteLine($"Query {query}");
            if (string.IsNullOrEmpty(query))
            {
                lstUserSuggestions.Visible = false;
                return;
            }

            var suggestions = GetUserSuggestions(query);
            if (suggestions.Count == 0)
            {
                lstUserSuggestions.Visible = false;
            }
            else
            {
                lstUserSuggestions.DataSource = suggestions;
                lstUserSuggestions.Visible = true;
            }
        }

        // Hàm lọc danh sách người dùng dựa trên từ khóa tìm kiếm
        private List<UserDto> GetUserSuggestions(string query)
        {
            return string.IsNullOrWhiteSpace(query)
                ? []
                : _allUsers.Where(u =>
                    u.UserName != null && u.UserName.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Sự kiện khi người dùng bấm vào một mục trong danh sách gợi ý
        private async void LstUserSuggestions_Click(object sender, EventArgs e)
        {
            if (lstUserSuggestions.SelectedItem is not UserDto selectedUser)
                return;
            //Send request
            SendHandshakeRequest(SessionManager.GetUserDto(), selectedUser);
        }

        #endregion

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            AuthService.Logout();
            FormController.ShowDialog(FormType.Login);
        }

        public void ShowHandshakeConfirm(HandshakeDto dtoRequest)
        {
            _waitingForm?.Close();
            // Xây dựng thông điệp hiển thị cho người dùng
            string message = $"Do you want to start a handshake with {dtoRequest.FromUser}?";
            string caption = "Handshake Confirmation";

            // Hiển thị dialog xác nhận với nút Yes/No
            var result = MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            dtoRequest.Accepted = result.Equals(DialogResult.Yes);

            var response =
                new MessageNetwork<HandshakeDto>(type: CommandType.HandshakeResponse, code: StatusCode.Success,
                    dtoRequest).ToJson();

            NetworkManager.Instance.TcpService.Send(response);
        }


        #region HandShake

        /// <summary>
        /// Hiển thị thông báo lỗi khi handshake không thành công.
        /// </summary>
        public void ShowHandshakeError(string errorMessage)
        {
            _waitingForm.Close();
            MessageBox.Show(errorMessage, "Handshake Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        public void LoadHandshake(ConversationRecord conversationRecord)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    lstChatHistory.Items.Clear();

                    var sortedInteractions = conversationRecord.Interactions
                        .OrderByDescending(i => i.LastInteractionTime);

                    foreach (var interaction in sortedInteractions)
                    {
                        // Tìm kiếm user từ danh sách _allUsers dựa trên ParticipantId
                        var user = _allUsers.FirstOrDefault(u => u.Id == interaction.ParticipantId);
                        if (user == null) continue;

                        // Sử dụng tên người dùng (UserName) làm hiển thị chính
                        var listItem = new ListViewItem(user.UserName);
                        listItem.SubItems.Add(interaction.LastInteractionTime.ToString("g"));

                        // Lưu đối tượng user vào Tag để dễ truy xuất sau này
                        listItem.Tag = user;
                        lstChatHistory.Items.Add(listItem);
                    }
                }));
            }
            else
            {
                lstChatHistory.Items.Clear();

                var sortedInteractions = conversationRecord.Interactions
                    .OrderByDescending(i => i.LastInteractionTime);

                foreach (var interaction in sortedInteractions)
                {
                    // Tìm kiếm user từ danh sách _allUsers dựa trên ParticipantId
                    var user = _allUsers.FirstOrDefault(u => u.Id == interaction.ParticipantId);
                    if (user == null) continue;

                    // Sử dụng tên người dùng (UserName) làm hiển thị chính
                    var listItem = new ListViewItem(user.UserName);
                    listItem.SubItems.Add(interaction.LastInteractionTime.ToString("g"));

                    // Lưu đối tượng user vào Tag để dễ truy xuất sau này
                    listItem.Tag = user;
                    lstChatHistory.Items.Add(listItem);
                }
            }
        }

        // Sự kiện khi kích hoạt (double-click hoặc nhấn Enter) vào một item trong lstChatHistory
        private void OnHandshakedUserActivated(object sender, EventArgs e)
        {
            if (lstChatHistory.SelectedItems.Count == 0)
                return;

            var selectedItem = lstChatHistory.SelectedItems[0];
            if (selectedItem.Tag is UserDto selectedUser)
            {
                HandleChatHistoryItemClick(selectedUser);
            }
        }

        // Hàm xử lý riêng khi người dùng click vào item trong danh sách chat history
        private void HandleChatHistoryItemClick(UserDto selectedUser) =>
            SendHandshakeRequest(SessionManager.GetUserDto(), selectedUser);

        private void SendHandshakeRequest(UserDto fromUser, UserDto toUser)
        {
            var request = new MessageNetwork<HandshakeDto>(type: CommandType.HandshakeRequest, StatusCode.Success,
                data: new HandshakeDto(fromUser, toUser));

            //Generate Key


            NetworkManager.Instance.TcpService.Send(request.ToJson());
            _waitingForm.Show();
        }

        public void HandShakeSuccess(string description)
        {
            _waitingForm.Close();
            MessageBox.Show(description, "Handshake Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion
    }
}