using DesClient.Models;
using DesClient.Services;
using Shared.Models;

namespace DesClient.Menu;

public static class ChatMenu
{
    public static void ChatWith(List<User> users, TcpService tcpService)
    {
        Console.Clear();
        Console.WriteLine("=== Chọn người để chat ===");

        for (int i = 0; i < users.Count; i++)
            Console.WriteLine($"{i}: {users[i].UserName}");

        Console.Write("Nhập số người muốn chat: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index >= 0 && index < users.Count)
        {
            ShowChatMenu(users[index], tcpService);
        }
    }

    private static void ShowChatMenu(User targetUser, TcpService tcpService, bool clearChat = true, bool loadHistory = true)
    {
        if(clearChat) Console.Clear();
        Console.WriteLine($"=== Chat với {targetUser.UserName} ===");
        
        //Request load old message
        if(loadHistory) tcpService.SendTcpMessage(new MessageNetwork<ChatHistoryRequest>(CommandType.LoadMessage, StatusCode.Success, new ChatHistoryRequest(senderId: SessionManager.GetUserId(), receiverId: targetUser.Id))
            .ToJson());
        
        Console.WriteLine("1. Gửi tin nhắn");
        Console.WriteLine("2. Quay lại");
        Console.Write("Chọn: ");

        switch (Console.ReadLine())
        {
            case "1":
                Console.Write("Nhập tin nhắn: ");
                string message = Console.ReadLine() ?? "";

                var chatMessage = new ChatMessage
                (
                    senderId: SessionManager.GetUserId(),
                    receiverId: targetUser.Id, 
                    content: message,
                    timestamp: DateTime.UtcNow
                );

                var networkMessage = new MessageNetwork<ChatMessage>(
                    CommandType.SendMessage,
                    StatusCode.Success,
                    chatMessage
                );

                tcpService.SendTcpMessage(networkMessage.ToJson());
                Console.WriteLine($"\nMe: {message} ... ({DateTime.UtcNow:HH:mm:ss})");
                ShowChatMenu(targetUser, tcpService, false);
                break;
            case "2":
                MainMenu.ShowMenu2(tcpService);
                break;
            default:
                break;
        }
    }

    public static void LoadAllMessage(ChatMessage[]? allMessages)
    {
        if (allMessages == null) return;
        foreach (var chatMessage in allMessages)
        {
            LoadMessage(chatMessage);
        }
    }

    public static void LoadMessage(ChatMessage? newMessage)
    {
        if (newMessage == null) return;
        Console.WriteLine(newMessage.SenderId == SessionManager.GetUserId()
            ? $"\nMe: {newMessage.Content} ... ({DateTime.UtcNow:HH:mm:ss})"
            : $"\n{newMessage.ReceiverName}: {newMessage.Content} ... ({DateTime.UtcNow:HH:mm:ss})");
    }
}