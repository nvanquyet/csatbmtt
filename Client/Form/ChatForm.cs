using Client.Models;
using Client.Network;
using Client.Services;
using Shared;
using Shared.Models;
using Shared.Security.Interface;
using Shared.Services;
using Shared.Utils;

namespace Client.Form
{
    public partial class ChatForm : Form
    {
        private string _selectedFilePath = "";
        private UserDto? _targetDto;

        public ChatForm()
        {
            InitializeComponent();
            messageContainer.AutoScroll = true;
            //LoadSampleMessages();
        }

        public void SetUserTarget(UserDto? userDto)
        {
            _targetDto = userDto;
        }

        #region Add Message

        public void AddMessage(TransferData? data, bool isMe)
        {
            if (data == null) return;

            var rowPanel = CreateRowPanel(isMe);

            if (!isMe) data = DecryptTransferData(data);

            if (data.TransferType == TransferType.Text)
            {
                if (data.RawData != null) AddTextMessage(rowPanel, ByteUtils.GetStringFromBytes(data.RawData), isMe);
            }
            else
            {
                AddFileMessage(rowPanel, data, isMe);
            }

            messageContainer.Controls.Add(rowPanel);
            messageContainer.ScrollControlIntoView(rowPanel);
            messageContainer.VerticalScroll.Value = messageContainer.VerticalScroll.Maximum;
            messageContainer.PerformLayout();
        }

        private void AddTextMessage(FlowLayoutPanel rowPanel, string text, bool isMe)
        {
            int maxBubbleWidth = messageContainer.ClientSize.Width - 40;

            // Panel chứa tin nhắn
            var messagePanel = new Panel
            {
                AutoSize = true,
                MaximumSize = new Size(maxBubbleWidth, 0),
                Padding = new Padding(10),
                Margin = new Padding(5),
                BackColor = isMe ? Color.LightBlue : Color.LightGray,
                BorderStyle = BorderStyle.FixedSingle
            };

            var messageLabel = new Label
            {
                Text = text,
                AutoSize = true,
                MaximumSize = new Size(maxBubbleWidth - 20, 0),
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 10)
            };
            messagePanel.Controls.Add(messageLabel);

            // Filler panel giúp đẩy messagePanel sang phải (nếu isMe = true)
            var filler = new Panel
            {
                Height = 1,
                Margin = Padding.Empty,
                Width = Math.Max(rowPanel.Width - messagePanel.PreferredSize.Width - 35, 10)
            };

            rowPanel.Controls.Clear();
            if (isMe)
            {
                // Tin nhắn của "mình" nằm bên phải
                rowPanel.Controls.Add(filler);
                rowPanel.Controls.Add(messagePanel);
            }
            else
            {
                // Tin nhắn của "người khác" nằm bên trái
                rowPanel.Controls.Add(messagePanel);
                rowPanel.Controls.Add(filler);
            }
        }

        private void AddFileMessage(FlowLayoutPanel rowPanel, TransferData data, bool isMe)
        {
            int maxBubbleWidth = messageContainer.ClientSize.Width - 40;

            // Tạo TableLayoutPanel với 1 hàng, 2 cột
            var tablePanel = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10),
                Margin = new Padding(5),
                //BackColor = isMe ? Color.LightBlue : Color.LightGray,
                //BorderStyle = BorderStyle.FixedSingle,
                MaximumSize = new Size(maxBubbleWidth, 0)
            };

            // Đặt cột: cột 0 chiếm khoảng 70% và cột 1 chiếm 30%
            var isImage = data is { TransferType: TransferType.Image, RawData: not null };
            tablePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, isImage ? 10F : 30F));
            tablePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, isImage ? 90F : 70F));
            tablePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Tạo control hiển thị nội dung file (nếu ảnh thì PictureBox, nếu khác thì Label)
            Control contentControl;
            if (isImage && data.RawData != null)
            {
                var pictureBox = new PictureBox
                {
                    Image = Image.FromStream(new MemoryStream(data.RawData)),
                    SizeMode = PictureBoxSizeMode.AutoSize,
                    Dock = DockStyle.Fill,
                };
                contentControl = pictureBox;
            }
            else
            {
                var lblFileInfo = new Label
                {
                    Text =
                        $"{FileHelper.GetFileTypeText(data.TransferType)}\n{(data.RawData != null ? ByteUtils.GetFileSize(data.RawData.Length) : 10)}",
                    AutoSize = true,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Arial", 10)
                };
                contentControl = lblFileInfo;
            }

            // Thêm control nội dung vào cột 0, hàng 0
            tablePanel.Controls.Add(contentControl, 1, 0);

            // Tạo nút Save
            var btnSave = new Button
            {
                Text = "💾 Save",
                Tag = data,
                AutoSize = true,
                Cursor = Cursors.Hand,
                Margin = new Padding(0)
            };
            btnSave.Click += BtnSaveFile_Click;

            // Để nút Save nằm bên dưới, chúng ta tạo 1 Panel chứa btnSave và dock nó xuống dưới
            var btnPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoSize = true
            };
            btnSave.Dock = DockStyle.Bottom;
            btnPanel.Controls.Add(btnSave);

            // Thêm panel chứa nút Save vào cột 1, hàng 0
            tablePanel.Controls.Add(btnPanel, 0, 0);

            // Tạo filler panel để căn chỉnh bong bóng theo lề (nếu cần)
            var filler = new Panel
            {
                Height = 1,
                Margin = Padding.Empty,
                Width = Math.Max(rowPanel.Width - tablePanel.PreferredSize.Width - 35, 10)
            };

            rowPanel.Controls.Clear();
            if (isMe)
            {
                // Nếu là tin nhắn của "mình", đẩy sang bên phải
                rowPanel.Controls.Add(filler);
                rowPanel.Controls.Add(tablePanel);
            }
            else
            {
                // Nếu là tin nhắn của "người khác", đặt bên trái
                rowPanel.Controls.Add(tablePanel);
                rowPanel.Controls.Add(filler);
            }
        }

        #endregion

        private void BtnSend_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                var transferData = new TransferData(TransferType.Text, ByteUtils.GetBytesFromString(txtMessage.Text));

                AddMessage(transferData, true);
                txtMessage.Clear();
                SendToServer(transferData: transferData);
            }

            if (_selectedFilePath.Length > 0)
            {
                var transferData = new TransferData(FileHelper.GetTransferType(_selectedFilePath),
                    File.ReadAllBytes(_selectedFilePath));
                AddMessage(transferData, true);
                _selectedFilePath = "";
                lblSelectedFile.Visible = false;
                btnRemoveFile.Visible = false;
                SendToServer(transferData: transferData);
            }
        }


        private void BtnSendFile_Click(object sender, EventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() != DialogResult.OK) return;
            _selectedFilePath = fileDialog.FileName;
            ShowSelectedFile();
        }

        private void ShowSelectedFile()
        {
            //Cut with / to get final path
            var index = _selectedFilePath.LastIndexOf('\\');
            var fileName = (index >= 0)
                ? _selectedFilePath.Substring(index + 1)
                : _selectedFilePath;

            lblSelectedFile.Text = "Selected File: " + fileName;
            lblSelectedFile.Visible = true;
            btnRemoveFile.Visible = true;
        }

        private void BtnRemoveFile_Click(object sender, EventArgs e)
        {
            _selectedFilePath = "";
            lblSelectedFile.Visible = false;
            btnRemoveFile.Visible = false;
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            //Show confirm 
            var message = $"Do you want to cancel a handshake with {_targetDto?.UserName}?";
            var caption = "Cancel Handshake Confirmation";

            // Hiển thị dialog xác nhận với nút Yes/No
            var result = MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (!result.Equals(DialogResult.Yes)) return;
            var response = new MessageNetwork<HandshakeDto>(type: CommandType.HandshakeCancel, StatusCode.Success,
                new HandshakeDto(SessionManager.GetUserDto(), _targetDto)).ToJson();
            NetworkManager.Instance.TcpService.Send(response);
        }

        private void BtnSaveFile_Click(object? sender, EventArgs e)
        {
            // Kiểm tra dữ liệu hợp lệ
            if (sender is not Button { Tag: TransferData data })
                return;

            // Tạo dialog lưu file
            using var saveDialog = new SaveFileDialog();
            saveDialog.Title = "Lưu file";
            saveDialog.FileName = FileHelper.GenerateFileName(data.TransferType);
            saveDialog.Filter = FileHelper.GetFileFilter(data.TransferType);

            if (saveDialog.ShowDialog() != DialogResult.OK) return;
            try
            {
                if (data.RawData != null) File.WriteAllBytes(saveDialog.FileName, data.RawData);
                MessageBox.Show("Lưu file thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu file: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private FlowLayoutPanel CreateRowPanel(bool isMe)
        {
            FlowLayoutPanel rowPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Margin = new Padding(5, 5, 0, 5),
                Width = messageContainer.ClientSize.Width,
                Padding = new Padding(0)
            };

            if (!isMe) return rowPanel;
            var filler = new Panel
            {
                AutoSize = false,
                Margin = new Padding(0),
                Height = 1,
                Width = rowPanel.Width - 15
            };
            rowPanel.Controls.Add(filler);
            return rowPanel;
        }

        protected override void Dispose(bool disposing)
        {
            var response = new MessageNetwork<HandshakeDto>(type: CommandType.HandshakeCancel, StatusCode.Success,
                new HandshakeDto(SessionManager.GetUserDto(), _targetDto)).ToJson();
            NetworkManager.Instance.TcpService.Send(response);
            base.Dispose(disposing);
        }

        #region Encryption

        private void SendToServer(TransferData transferData)
        {
            var desEncrypt = EncryptionService.Instance.GetAlgorithm(EncryptionType.Des);
            var rsaEncrypt = EncryptionService.Instance.GetAlgorithm(EncryptionType.Rsa);
            if (_targetDto == null || transferData.RawData == null) return;

            // Encrypt raw data using DES
            transferData.RawData = desEncrypt.Encrypt(transferData.RawData, desEncrypt.EncryptKey);

            // Encrypt the DES key (to be used for decryption) with target's RSA key
            Logger.LogInfo($"Length {desEncrypt.DecryptKey.Length}, {_targetDto.EncryptKey.Length}");
            transferData.KeyDecrypt = rsaEncrypt.Encrypt(desEncrypt.DecryptKey, _targetDto.EncryptKey);
            // transferData.KeyDecrypt = desEncrypt.DecryptKey;

            // Giả sử nếu TransferData là kiểu file hoặc dữ liệu lớn, ta sẽ chunk toàn bộ TransferData
            if (transferData.TransferType != TransferType.Text)
            {
                // Serialize toàn bộ đối tượng TransferData
                byte[] serializedData = FileChunkService.SerializeTransferData(transferData);

                int chunkSize = 8192; // 8KB mỗi chunk
                int totalChunks = (int)Math.Ceiling((double)serializedData.Length / chunkSize);
                Guid messageId = Guid.NewGuid(); // ID duy nhất cho TransferData

                // Gửi các chunk trong Task riêng để không block luồng chính
                Task.Run(async () =>
                {
                    for (int i = 0; i < totalChunks; i++)
                    {
                        int offset = i * chunkSize;
                        int currentChunkSize = Math.Min(chunkSize, serializedData.Length - offset);
                        byte[] chunkPayload = new byte[currentChunkSize];
                        Buffer.BlockCopy(serializedData, offset, chunkPayload, 0, currentChunkSize);

                        // Tạo ChunkDto chứa thông tin của chunk
                        var chunkDto = new ChunkDto
                        {
                            MessageId = messageId,
                            ChunkIndex = i,
                            TotalChunks = totalChunks,
                            Payload = chunkPayload
                        };

                        // Đóng gói thông tin ReceiverId và chunk vào FileChunkMessageDto
                        var fileChunkMessage = new FileChunkMessageDto
                        {
                            ReceiverId = _targetDto?.Id,
                            Chunk = chunkDto
                        };

                        // Đóng gói vào MessageNetwork với CommandType.DispatchMessage
                        var message = new MessageNetwork<FileChunkMessageDto>(
                            type: CommandType.DispatchMessage,
                            code: StatusCode.Success,
                            data: fileChunkMessage
                        );

                        // Serialize thành JSON (có newline để phân cách tin nhắn)
                        string messageJson = message.ToJson();

                        // Gửi chunk đến Server
                        NetworkManager.Instance.TcpService.Send(messageJson);

                        // Optional: Delay ngắn giữa các chunk để tránh quá tải
                        await Task.Delay(10);
                    }
                });
            }
            else
            {
                // Nếu là tin nhắn text, gửi trực tiếp như cũ
                var response = new MessageNetwork<MessageDto>(
                    type: CommandType.DispatchMessage,
                    code: StatusCode.Success,
                    data: new MessageDto(receiverId: _targetDto?.Id, transferData)
                ).ToJson();
                NetworkManager.Instance.TcpService.Send(response);
            }
        }


        private TransferData DecryptTransferData(TransferData transferData)
        {
            var desEncrypt = EncryptionService.Instance.GetAlgorithm(EncryptionType.Des);
            var rsaEncrypt = EncryptionService.Instance.GetAlgorithm(EncryptionType.Rsa);

            if (transferData.KeyDecrypt == null || transferData.RawData == null) return transferData;
            // Decrypt the encrypted DES key using RSA private key
            transferData.KeyDecrypt =
                rsaEncrypt.Decrypt(transferData.KeyDecrypt, rsaEncrypt.DecryptKey);
            // Decrypt the raw data using the decrypted DES key
            transferData.RawData = desEncrypt.Decrypt(transferData.RawData, transferData.KeyDecrypt);
            return transferData;
        }

        #endregion
    }
}