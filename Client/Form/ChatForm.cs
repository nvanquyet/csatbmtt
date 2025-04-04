﻿using System.Collections.Concurrent;
using Client.Models;
using Client.Network;
using Client.Network.Tcp;
using Client.Services;
using Newtonsoft.Json;
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
        private readonly Dictionary<string, TransferData?> _encryptedFile = new();
        private CancellationTokenSource? _sendCts;

        public ChatForm()
        {
            InitializeComponent();
            messageContainer.AutoScroll = true;
            OnClickRandomKey(null, null);
            //LoadSampleMessages();
        }

        public void SetUserTarget(UserDto? userDto)
        {
            _targetDto = userDto;
        }

        #region Add Message

        public async Task AddMessage(TransferData? data, bool isMe)
        {
            if (data == null) return;

            var rowPanel = CreateRowPanel(isMe);

            if (!isMe) data = await DecryptTransferDataAsync(data);

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
                        $@"{FileHelper.GetFileTypeText(data.TransferType)}
{(data.RawData != null ? ByteUtils.GetFileSize(data.RawData.Length) : 10)}",
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

                _ = AddMessage(transferData, true);
                txtMessage.Clear();
                SendToServer(transferData: transferData);
            }

            if (_selectedFilePath.Length > 0)
            {
                SendToServer(transferData: _encryptedFile[_selectedFilePath]!);
                lblSelectedFile.Visible = false;
                btnRemoveFile.Visible = false;
            }
        }


        private void BtnSendFile_Click(object sender, EventArgs e)
        {
            // Lấy DES key từ txtDesKey
            string desKey = txtDesKey.Text.Trim();
            if (string.IsNullOrEmpty(desKey))
            {
                MessageBox.Show(@"Please enter DES key.");
                BtnRemoveFile_Click(null, null);
                return;
            }

            if (desKey.Length != 8)
            {
                MessageBox.Show(@"Des key must be 8 characters long.");
                BtnRemoveFile_Click(null, null);
                return;
            }

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

            lblSelectedFile.Text = $@"Selected File: {fileName}";
            lblSelectedFile.Visible = true;
            btnRemoveFile.Visible = true;
            _ = BtnEncryptFile_Click();
        }

        private void BtnRemoveFile_Click(object? sender, EventArgs? e)
        {
            _encryptedFile[_selectedFilePath] = null;
            _selectedFilePath = "";
            lblSelectedFile.Visible = false;
            btnRemoveFile.Visible = false;

            lblEncryptionStatus.Text = "";
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
            saveDialog.Title = @"Lưu file";
            saveDialog.FileName = FileHelper.GenerateFileName(data.TransferType);
            saveDialog.Filter = FileHelper.GetFileFilter(data.TransferType);

            if (saveDialog.ShowDialog() != DialogResult.OK) return;
            try
            {
                if (data.RawData != null) File.WriteAllBytes(saveDialog.FileName, data.RawData);
                MessageBox.Show(@"Lưu file thành công!", @"Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Lỗi khi lưu file: {ex.Message}", @"Lỗi",
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
            _sendCts?.Cancel();
            var response = new MessageNetwork<HandshakeDto>(type: CommandType.HandshakeCancel, StatusCode.Success,
                new HandshakeDto(SessionManager.GetUserDto(), _targetDto)).ToJson();
            NetworkManager.Instance.TcpService.Send(response);
            base.Dispose(disposing);
        }

        #region Encryption

        private ChunkDto? _currentChunk;

        private async Task BtnEncryptFile_Click()
        {
            if (string.IsNullOrEmpty(_selectedFilePath) || !File.Exists(_selectedFilePath))
            {
                MessageBox.Show(@"Please select a valid file first.");
                BtnRemoveFile_Click(null, null);
                return;
            }

            string desKey = txtDesKey.Text.Trim();
            if (string.IsNullOrEmpty(desKey))
            {
                MessageBox.Show(@"DES key cannot be empty.");
                return;
            }

            Encrypting(true);
            var sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                lblEncryptionStatus.Text = @"Encrypting...";

                byte[]? encryptedData = await Task.Run(() =>
                {
                    try
                    {
                        byte[] fileData = File.ReadAllBytes(_selectedFilePath);
                        var desCrypt = EncryptionService.Instance.GetAlgorithm(EncryptionType.Des);
                        return desCrypt.Encrypt(fileData, ByteUtils.GetBytesFromString(desKey));
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"File encryption error: {ex.Message}");
                        return null;
                    }
                });

                if (encryptedData == null)
                {
                    MessageBox.Show(@"Encryption failed.");
                    return;
                }

                if (_targetDto is { EncryptKey.Length: > 0 })
                {
                    var tsData = new TransferData(FileHelper.GetTransferType(_selectedFilePath), encryptedData)
                    {
                        KeyDecrypt = EncryptionService.Instance
                            .GetAlgorithm(EncryptionType.Rsa)
                            .Encrypt(ByteUtils.GetBytesFromString(desKey), _targetDto.EncryptKey)
                    };

                    _encryptedFile[_selectedFilePath] = tsData;
                }

                lblEncryptionStatus.Text = $@"Encryption Successful – Time: {sw.ElapsedMilliseconds} ms";
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Encryption failed: {ex.Message}");
                Logger.LogError($"Encryption failed: {ex}");
            }
            finally
            {
                sw.Stop();
                Encrypting(false);
            }
        }


        private void Encrypting(bool isEncrypt)
        {
            btnSend.Enabled = !isEncrypt;
            btnSendFile.Enabled = !isEncrypt;
            btnRemoveFile.Visible = !isEncrypt;
            btnRandomDesKey.Enabled = !isEncrypt;
            txtDesKey.ReadOnly = isEncrypt;
        }

        private void BtnCancelSendFile_Click(object sender, EventArgs e)
        {
            // Gọi phương thức hủy gửi file
            // Ví dụ: NetworkManager.Instance.TcpService.CancelSend();
            // Sau đó ẩn progress bar và nút cancel
            progressFileSending.Visible = false;
            btnCancelSendFile.Visible = false;
            var message = new MessageNetwork<FileChunkMessageDto>(CommandType.CancelDispatchMessage, StatusCode.Success,
                new FileChunkMessageDto()
                {
                    ReceiverId = _targetDto?.Id,
                    Chunk = _currentChunk
                }).ToJson();
            NetworkManager.Instance.TcpService.Send(message);
            MessageBox.Show(@"File sending cancelled.");
            _sendCts?.Cancel();
        }

        private void SendToServer(TransferData transferData)
        {
            if (transferData.TransferType != TransferType.Text)
            {
                // Serialize toàn bộ đối tượng TransferData
                byte[] serializedData = FileChunkService.SerializeTransferData(transferData);
                int chunkSize = 16384;
                int totalChunks = (int)Math.Ceiling((double)serializedData.Length / chunkSize);
                Guid messageId = Guid.NewGuid(); // ID duy nhất cho TransferData

                progressFileSending.Visible = true;
                btnCancelSendFile.Visible = true;
                _sendCts = new CancellationTokenSource();

                // Gửi các chunk trong Task riêng để không block luồng chính
                Task.Run(async () =>
                {
                    for (int i = 0; i < totalChunks; i++)
                    {
                        _sendCts?.Token.ThrowIfCancellationRequested();

                        int offset = i * chunkSize;
                        int currentChunkSize = Math.Min(chunkSize, serializedData.Length - offset);
                        byte[] chunkPayload = new byte[currentChunkSize];
                        Buffer.BlockCopy(serializedData, offset, chunkPayload, 0, currentChunkSize);

                        // Tạo ChunkDto cho chunk hiện tại
                        _currentChunk = new ChunkDto
                        {
                            MessageId = messageId,
                            ChunkIndex = i,
                            TotalChunks = totalChunks,
                            Payload = chunkPayload
                        };

                        // Đóng gói ReceiverId và ChunkDto vào FileChunkMessageDto
                        var fileChunkMessage = new FileChunkMessageDto
                        {
                            ReceiverId = _targetDto?.Id,
                            Chunk = _currentChunk
                        };

                        // Đóng gói vào MessageNetwork với CommandType.DispatchMessage
                        var message = new MessageNetwork<FileChunkMessageDto>(
                            type: CommandType.DispatchMessage,
                            code: StatusCode.Success,
                            data: fileChunkMessage
                        );

                        string messageJson = message.ToJson();

                        if (NetworkManager.Instance.TcpService is TcpProtocol tcp)
                        {
                            tcp.Send(messageJson, (data) =>
                            {
                                // Convert data to FileChunkMessageDto
                                var d = MessageNetwork<FileChunkMessageDto>.FromJson(
                                    ByteUtils.GetStringFromBytes(data));
                                if (d?.Data?.Chunk == null || totalChunks == 0) return;

                                var value = (d.Data.Chunk.ChunkIndex * 100) / totalChunks;

                                progressFileSending.BeginInvoke((MethodInvoker)(async () =>
                                {
                                    Logger.LogInfo($"Index {_currentChunk.ChunkIndex} {_currentChunk.TotalChunks - 1}");
                                    if (_currentChunk.ChunkIndex >= _currentChunk.TotalChunks - 1)
                                    {
                                        await AddMessage(new TransferData(
                                            FileHelper.GetTransferType(_selectedFilePath),
                                            await File.ReadAllBytesAsync(_selectedFilePath)), true);
                                        _selectedFilePath = "";
                                        progressFileSending.Visible = false;
                                        btnCancelSendFile.Visible = false;
                                    }
                                    else if (progressFileSending.Value != value) // Chỉ update nếu giá trị thay đổi
                                    {
                                        progressFileSending.Value = value;
                                    }
                                }));
                            });
                        }

                        if (_sendCts != null) await Task.Delay(10, _sendCts.Token);
                    }
                }, _sendCts.Token);
            }
            else
            {
                var response = new MessageNetwork<MessageDto>(
                    type: CommandType.DispatchMessage,
                    code: StatusCode.Success,
                    data: new MessageDto(receiverId: _targetDto?.Id, transferData)
                ).ToJson();
                NetworkManager.Instance.TcpService.Send(response);
            }
        }


        private async Task<TransferData> DecryptTransferDataAsync(TransferData transferData)
        {
            if (transferData.TransferType == TransferType.Text)
                return transferData;

            var desEncrypt = EncryptionService.Instance.GetAlgorithm(EncryptionType.Des);
            var rsaEncrypt = EncryptionService.Instance.GetAlgorithm(EncryptionType.Rsa);

            if (transferData.KeyDecrypt == null || transferData.RawData == null)
                return transferData;

            var sw = System.Diagnostics.Stopwatch.StartNew(); // Bắt đầu đếm thời gian

            try
            {
                // Cập nhật UI trên luồng chính
                await lblEncryptionStatus.InvokeAsync((Action)(() =>
                {
                    lblEncryptionStatus.Visible = true;
                    lblEncryptionStatus.Text = @"Receiving data. Decrypting...";
                }));

                // Giải mã khóa DES bằng RSA trên luồng nền
                transferData.KeyDecrypt = await Task.Run(() =>
                    rsaEncrypt.Decrypt(transferData.KeyDecrypt, rsaEncrypt.DecryptKey));

                // Giải mã dữ liệu bằng DES nếu giải mã khóa thành công
                if (transferData.KeyDecrypt != null)
                {
                    transferData.RawData = await Task.Run(() =>
                        desEncrypt.Decrypt(transferData.RawData, transferData.KeyDecrypt));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Decryption error: {ex.Message}");
            }
            finally
            {
                sw.Stop(); // Dừng đếm thời gian

                // Cập nhật UI trên luồng chính sau khi hoàn thành
                await lblEncryptionStatus.InvokeAsync((Action)(() =>
                {
                    lblEncryptionStatus.Text = $@"Decryption completed in {sw.ElapsedMilliseconds} ms";
                }));
            }

            return transferData;
        }

        #endregion

        private void OnClickRandomKey(object? sender, EventArgs? e)
        {
            var des = EncryptionService.Instance.GetAlgorithm(EncryptionType.Des);
            des.GenerateKey();
            txtDesKey.Text = ByteUtils.GetStringFromBytes(des.EncryptKey);
        }
    }
}