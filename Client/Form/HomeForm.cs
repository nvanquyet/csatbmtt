using Client.Models;
using Client.Network;
using Client.Services;
using MongoDB.Bson;
using Shared;
using Shared.Models;
using Shared.Security.Interface;
using Shared.Services;

namespace Client.Form
{
    public partial class HomeForm : Form
    {
        // Giả sử danh sách toàn bộ người dùng
        private List<UserDto> _allUsers = new();

        public void SetAllUsers(List<UserDto> users)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    _allUsers = users;
                }));
            }
            else
            {
                _allUsers = users;
            }

            //TestLoadHandshake();
        }

        private readonly Form _waitingForm;

        public HomeForm()
        {
            _waitingForm = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(250, 100),
                ControlBox = false,
                ShowInTaskbar = false
            };

            var lblWait = new Label
            {
                Text = @"Please wait, handshake in progress...",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            _waitingForm.Controls.Add(lblWait);

            //Send GetAvailable User
            var getAllAvailableUser = new MessageNetwork<UserDto>(type: CommandType.GetAvailableClients,
                code: StatusCode.Success,
                data: SessionManager.GetUserDto()).ToJson();
            NetworkManager.Instance.TcpService.Send(getAllAvailableUser);

            var getUserShake = new MessageNetwork<UserDto>(type: CommandType.GetHandshakeUsers, code: StatusCode.Success,
                data: SessionManager.GetUserDto()).ToJson();
            NetworkManager.Instance.TcpService.Send(getUserShake);

            InitializeComponent();
        }

        #region UserSuggestion
        private void lstUserSuggestions_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index < 0 || e.Index >= lstUserSuggestions.Items.Count)
                return;

            if (lstUserSuggestions.Items[e.Index] is UserDto user)
            {
                // Màu trạng thái
                Color statusColor = user.Status switch
                {
                    UserStatus.Available => Color.Green,
                    UserStatus.Busy => Color.Orange,
                    UserStatus.Inactive => Color.Gray,
                    _ => Color.Black
                };

                // Tính toán vị trí để căn giữa hình tròn với text
                int iconSize = 15;
                int centerY = e.Bounds.Top + (e.Bounds.Height - iconSize) / 2;

                // Vẽ hình tròn
                using (var brush = new SolidBrush(statusColor))
                {
                    e.Graphics.FillEllipse(brush, e.Bounds.Left + 5, centerY, iconSize, iconSize);
                }

                // Vẽ tên người dùng, căn chỉnh ngang cách hình tròn 10px
                var displayText = user.UserName;
                using (var textBrush = new SolidBrush(e.ForeColor))
                {
                    e.Graphics.DrawString(displayText, e.Font, textBrush, e.Bounds.Left + 25, e.Bounds.Top + (e.Bounds.Height - e.Font.Height) / 2);
                }
            }

            e.DrawFocusRectangle();
        }


        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            var query = txtSearch.Text.Trim();
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
                lstUserSuggestions.DisplayMember = "UserName"; // Dù sử dụng OwnerDraw, giá trị này vẫn cần
                lstUserSuggestions.Visible = true;
            }
        }


        // Hàm lọc danh sách người dùng dựa trên từ khóa tìm kiếm
        private List<UserDto> GetUserSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<UserDto>();

            return _allUsers
                .Where(u => u.UserName != null && u.UserName.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(u => GetStatusPriority(u.Status))
                .ToList();
        }

        // Sự kiện khi người dùng bấm vào một mục trong danh sách gợi ý
        private void LstUserSuggestions_Click(object sender, EventArgs e)
        {
            if (lstUserSuggestions.SelectedItem is not UserDto selectedItem)
                return;
            var selectedUser = _allUsers.FirstOrDefault(u => u == selectedItem);
            if (selectedUser == null)
                return;

            //Send request
            switch (selectedUser.Status)
            {
                case UserStatus.Inactive:
                    MessageBox.Show("User is offline. Please try again later.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case UserStatus.Busy:
                    MessageBox.Show("User is currently busy. Please try again later.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case UserStatus.Available:
                    SendHandshakeRequest(SessionManager.GetUserDto(), selectedUser);
                    break;
                default:
                    MessageBox.Show("User is currently busy. Please try again later.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }
        }

        #endregion

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            AuthService.Logout();
            FormController.Show(FormType.Login);
            NetworkManager.Instance.TcpService.Stop();
        }

        public void ShowHandshakeConfirm(HandshakeDto dtoRequest)
        {
            _waitingForm?.Close();
            // Xây dựng thông điệp hiển thị cho người dùng
            var message = $"Do you want to start a handshake with {dtoRequest.FromUser?.UserName}?";
            var caption = "Handshake Confirmation";

            // Hiển thị dialog xác nhận với nút Yes/No
            var result = MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            dtoRequest.Accepted = result.Equals(DialogResult.Yes);

            if (dtoRequest is { Accepted: true, ToUser: not null })
            {
                dtoRequest.ToUser.EncryptKey = EncryptionService.Instance.GetAlgorithm(EncryptionType.Rsa).EncryptKey;
            }

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
            lstChatHistory.Items.Clear();

            // Sắp xếp các interaction theo thời gian (ví dụ descending theo LastInteractionTime)
            // và sau đó theo thứ tự ưu tiên status (dựa trên _allUsers)
            var sortedInteractions = conversationRecord.Interactions
                .OrderByDescending(i => i.LastInteractionTime)
                .ThenBy(i =>
                {
                    var user = _allUsers.FirstOrDefault(u => u.Id == i.ParticipantId);
                    return user != null ? GetStatusPriority(user.Status) : int.MaxValue;
                });

            foreach (var interaction in sortedInteractions)
            {
                // Tìm kiếm user từ danh sách _allUsers dựa trên ParticipantId
                var user = _allUsers.FirstOrDefault(u => u.Id == interaction.ParticipantId);
                if (user == null)
                    continue;

                // Tạo ListViewItem với các cột: Username, Last Interaction, Status
                var listItem = new ListViewItem(user.UserName);
                listItem.SubItems.Add(interaction.LastInteractionTime.ToString("g"));
        
                // Hiển thị trạng thái dạng text, bạn có thể hiển thị "Online", "Busy", "Offline"
                var statusText = user.Status switch
                {
                    UserStatus.Available => "Online",
                    UserStatus.Busy => "Busy",
                    UserStatus.Inactive => "Offline",
                    _ => ""
                };
                listItem.SubItems.Add(statusText);

                // Thay đổi màu hiển thị dựa trên trạng thái
                listItem.ForeColor = user.Status switch
                {
                    UserStatus.Available => Color.Green,  // Ví dụ: Available hiển thị màu xanh
                    UserStatus.Busy => Color.Orange,        // Busy hiển thị màu cam
                    UserStatus.Inactive => Color.Gray,      // Inactive hiển thị màu xám nhạt
                    _ => Color.Black
                };

                // Lưu đối tượng user vào Tag để dễ truy xuất sau này
                listItem.Tag = user;
                lstChatHistory.Items.Add(listItem);
            }
        }

        // Sự kiện khi kích hoạt (double-click hoặc nhấn Enter) vào một item trong lstChatHistory
        private void OnHandshakeUserActivated(object sender, EventArgs e)
        {
            if (lstChatHistory.SelectedItems.Count == 0)
                return;

            var selectedItem = lstChatHistory.SelectedItems[0];
            if (selectedItem.Tag is UserDto selectedUser)
            {
                HandleChatHistoryItemClick(selectedUser);
            }
        }

        private void HandleChatHistoryItemClick(UserDto selectedUser)
        {
            switch (selectedUser.Status)
            {
                case UserStatus.Inactive:
                    MessageBox.Show("User is offline. Please try again later.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case UserStatus.Busy:
                    MessageBox.Show("User is currently busy. Please try again later.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case UserStatus.Available:
                    SendHandshakeRequest(SessionManager.GetUserDto(), selectedUser);
                    break;
                default:
                    MessageBox.Show("User is currently busy. Please try again later.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }
        }

        private void SendHandshakeRequest(UserDto fromUser, UserDto toUser)
        {
            fromUser.EncryptKey = EncryptionService.Instance.GetAlgorithm(EncryptionType.Rsa).EncryptKey;
            var request = new MessageNetwork<HandshakeDto>(type: CommandType.HandshakeRequest, StatusCode.Success,
                data: new HandshakeDto(fromUser, toUser));

            NetworkManager.Instance.TcpService.Send(request.ToJson());
            _waitingForm.ShowDialog();
        }

        public void HandShakeSuccess(string description)
        {
            _waitingForm.Close();
            MessageBox.Show(description, "Handshake Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        public void UpdateStatus(UserDto newData)
        {
            var user = _allUsers.FirstOrDefault(u => u.Id == newData.Id);
            if (user != null)
            {
                user.UserName = newData.UserName;
                user.Status = newData.Status;
            }

            UpdateSearchForm(newData);
            UpdateShakeForm(newData);
        }


        private void UpdateSearchForm(UserDto newData)
        {
            if (!lstUserSuggestions.Visible) return;
    
            var query = txtSearch.Text.Trim();
            var suggestions = GetUserSuggestions(query);

            lstUserSuggestions.Visible = suggestions.Count > 0;
            if (!lstUserSuggestions.Visible) return;
            lstUserSuggestions.DataSource = suggestions;
            lstUserSuggestions.DisplayMember = "UserName";
        }

        private void UpdateShakeForm(UserDto newData)
        {
            foreach (ListViewItem item in lstChatHistory.Items)
            {
                if (item.Tag is not UserDto user || user.Id != newData.Id) continue;
        
                item.Text = newData.UserName;
                item.SubItems[2].Text = newData.Status switch
                {
                    UserStatus.Available => "Online",
                    UserStatus.Busy => "Busy",
                    UserStatus.Inactive => "Offline",
                    _ => ""
                };

                item.ForeColor = newData.Status switch
                {
                    UserStatus.Available => Color.Green,
                    UserStatus.Busy => Color.Orange,
                    UserStatus.Inactive => Color.Gray,
                    _ => Color.Black
                };
            }
        }

        
        private int GetStatusPriority(UserStatus status)
        {
            // Lower value ưu tiên cao hơn.
            return status switch
            {
                UserStatus.Available => 1,
                UserStatus.Busy => 2,
                UserStatus.Inactive => 3,
                _ => 4,
            };
        }
        
        private void TestLoadHandshake()
        {
            // Tạo fake ConversationRecord cho owner "owner1"
            var conversationRecord = new ConversationRecord
            {
                Id = ObjectId.GenerateNewId().ToString(),
                OwnerId = SessionManager.GetUserDto().Id,
                Interactions =
                [
                    new InteractionDetail
                        { ParticipantId = _allUsers[1].Id, LastInteractionTime = DateTime.Now.AddMinutes(-5) },
                    new InteractionDetail
                        { ParticipantId = _allUsers[3].Id, LastInteractionTime = DateTime.Now.AddMinutes(-2) },
                    new InteractionDetail
                        { ParticipantId = _allUsers[5].Id, LastInteractionTime = DateTime.Now.AddMinutes(-10) },
                    new InteractionDetail
                        { ParticipantId = _allUsers[1].Id, LastInteractionTime = DateTime.Now.AddMinutes(-5) },
                    new InteractionDetail
                        { ParticipantId = _allUsers[3].Id, LastInteractionTime = DateTime.Now.AddMinutes(-2) },
                    new InteractionDetail
                        { ParticipantId = _allUsers[5].Id, LastInteractionTime = DateTime.Now.AddMinutes(-10) },
                    new InteractionDetail
                        { ParticipantId = _allUsers[1].Id, LastInteractionTime = DateTime.Now.AddMinutes(-5) },
                    new InteractionDetail
                        { ParticipantId = _allUsers[3].Id, LastInteractionTime = DateTime.Now.AddMinutes(-2) },
                    new InteractionDetail
                        { ParticipantId = _allUsers[5].Id, LastInteractionTime = DateTime.Now.AddMinutes(-10) },
                    new InteractionDetail
                        { ParticipantId = _allUsers[1].Id, LastInteractionTime = DateTime.Now.AddMinutes(-5) },
                    new InteractionDetail
                        { ParticipantId = _allUsers[3].Id, LastInteractionTime = DateTime.Now.AddMinutes(-2) },
                    new InteractionDetail
                        { ParticipantId = _allUsers[5].Id, LastInteractionTime = DateTime.Now.AddMinutes(-10) }
                ]
            };

            // Gọi hàm LoadHandshake với fake data
            LoadHandshake(conversationRecord);
        }
    }
}