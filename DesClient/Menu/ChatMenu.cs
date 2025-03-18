using System.Text;
using DesClient.Models;
using DesClient.Network;
using Shared.Models;
using Shared.Networking.Interfaces;
using Shared.Security.Interface;
using Shared.Services;

namespace DesClient.Menu;

public static class ChatMenu
{
    public static void ChatWith(List<UserDto> users, INetworkProtocol protocol)
    {
        Console.WriteLine("=== Chọn người để chat ===");

        // Lấy thông tin user hiện tại (giả sử có SessionManager)
        var currentUserId = SessionManager.GetUserId();

        // Lọc danh sách loại bỏ user có id trùng với currentUserId 
        var filteredUsers = users.Where(u => u.Id != currentUserId).ToList();

        for (int i = 0; i < filteredUsers.Count; i++)
            Console.WriteLine($"{i}: {filteredUsers[i].UserName}");

        Console.Write("Nhập số người muốn chat: ");
        if (!int.TryParse(Console.ReadLine(), out var index) || index < 0 || index >= filteredUsers.Count) return;

        //ShowChatMenu(filteredUsers[index], NetworkManager.Instance.TcpService);
        //Todo: Require connect to other client
        var selectedUser = filteredUsers[index];
        ShowChatMenu(selectedUser, protocol);
        // Gửi yêu cầu chat đến server
        // var chatRequest =
        //     new ChatRequestDto(fromUser: new UserDto(SessionManager.GetUserId(), SessionManager.GetUserName()),
        //         toUser: new UserDto(selectedUser.Id, selectedUser.UserName));
        //
        // NetworkManager.Instance.TcpService.Send(new MessageNetwork<ChatRequestDto>(
        //     CommandType.ChatRequest, StatusCode.Success, chatRequest).ToJson(), "");
        //
        // Console.WriteLine($"Wait {selectedUser.UserName}... response");
    }


    private static void ShowChatMenu(UserDto? targetUser, INetworkProtocol protocol, bool clearChat = true,
        bool loadHistory = true)
    {
        if (clearChat) Console.Clear();
        Console.WriteLine($"=== Chat với {targetUser?.UserName} ===");

        //Request load old message
        if (loadHistory)
            protocol.Send(new MessageNetwork<ChatHistoryRequest>(CommandType.LoadMessage,
                    StatusCode.Success,
                    new ChatHistoryRequest(senderId: SessionManager.GetUserId(), receiverId: targetUser?.Id))
                .ToJson(), "");

        Console.WriteLine("1. Gửi tin nhắn");
        Console.WriteLine("2. Load Old Messages");
        Console.WriteLine("3. Quay lại");
        Console.Write("Chọn: ");

        switch (Console.ReadLine())
        {
            case "1":
                Console.Write("Nhập tin nhắn: ");
                var message = Console.ReadLine() ?? "";
                var encryptedCode = new TransferData(TransferType.Text,
                        EncryptionService.Instance.EncryptData(EncryptionType.Des, Encoding.UTF8.GetBytes(message)));
                var chatConversation = new ChatConversation(senderId: SessionManager.GetUserId(),
                    receiverId: targetUser?.Id,
                    [
                        new ChatMessage(senderId: SessionManager.GetUserId(),
                            content: encryptedCode,
                            senderName: SessionManager.GetUserName(),
                            receiverName: targetUser?.UserName,
                            timestamp: DateTime.Now)
                    ]);

                var networkMessage = new MessageNetwork<ChatConversation>(
                    CommandType.SendMessage,
                    StatusCode.Success,
                    chatConversation
                );

                protocol.Send(networkMessage.ToJson());
                Console.WriteLine($"\nMe: {message} ... ({DateTime.UtcNow:HH:mm:ss})");
                ShowChatMenu(targetUser, protocol, false);
                break;
            case "2":
                protocol.Send(new MessageNetwork<ChatHistoryRequest>(CommandType.LoadMessage,
                        StatusCode.Success,
                        new ChatHistoryRequest(senderId: SessionManager.GetUserId(), receiverId: targetUser?.Id))
                    .ToJson());
                break;
            case "3":
                MainMenu.ShowMenu2(protocol);
                break;
            default:
                break;
        }
    }

    public static void LoadAllMessage(ChatMessage[]? allMessages)
    {
        if (allMessages == null) return;
        Console.Clear();
        foreach (var chatMessage in allMessages)
        {
            LoadMessage(chatMessage);
        }
    }

    public static void LoadMessage(ChatMessage? newMessage)
    {
        if (newMessage == null) return;
        var message = EncryptionService.Instance.DecryptData(EncryptionType.Des, newMessage.Content?.RawData!);
        Console.WriteLine(newMessage.SenderId == SessionManager.GetUserId()
            ? $"\nMe: {Encoding.UTF8.GetString(message)} ... ({DateTime.UtcNow:HH:mm:ss})"
            : $"\n{newMessage.SenderId}: {newMessage.Content} ... ({DateTime.UtcNow:HH:mm:ss})");
    }
    //
    // public static void ShowBoxConfirm(ChatRequestDto dto)
    // {
    //     Console.WriteLine($"Bạn nhận được yêu cầu chat từ {dto.FromUser?.UserName}. Chấp nhận? (Y/N)");
    //
    //     var response = Console.ReadLine();
    //     var isAccepted = response?.Trim().ToLower() == "y";
    //
    //     // Gửi phản hồi về server
    //     var chatResponse = new ChatResponseDto(fromUser: dto.ToUser, toUser: dto.FromUser, accepted: isAccepted);
    //
    //     NetworkManager.Instance.TcpService.Send(new MessageNetwork<ChatResponseDto>(
    //         CommandType.ChatResponse, StatusCode.Success, chatResponse).ToJson(), "");
    //
    //     if (isAccepted)
    //     {
    //         Console.WriteLine("Kết nối chat được thiết lập!");
    //         ShowChatMenu(dto.FromUser, NetworkManager.Instance.TcpService);
    //     }
    //     else
    //     {
    //         Console.WriteLine("Bạn đã từ chối yêu cầu chat.");
    //         MainMenu.ShowMenu2(NetworkManager.Instance.TcpService);
    //     }
    // }
}