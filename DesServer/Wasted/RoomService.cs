// using System.Net.Sockets;
// using DesServer.Models;
// using Shared.Services;
//
// namespace DesServer.Services;
//
// public class RoomService
// {
//     private readonly Dictionary<string, Room> _rooms = new();
//
//     public Room? GetRoom(string roomId)
//     {
//         if(!_rooms.TryGetValue(roomId, out var room)) return null;  
//         return room;
//     }
//     
//     public void CreateRoom(string roomId, TcpClient? hostClient, string password = "") 
//     {
//         if (!_rooms.ContainsKey(roomId))
//         {
//             _rooms[roomId] = new Room(id: roomId, password: password);
//             _rooms[roomId].AddClient(hostClient);
//             MsgService.SendTcpMessage(hostClient, "Room created successfully!");
//         }
//         else
//         {
//             MsgService.SendTcpMessage(hostClient, "Room ID already exists.");
//         }
//     }
//
//     public void JoinRoom(string roomId, TcpClient? client)
//     {
//         if (_rooms.TryGetValue(roomId, out var room))
//         {
//             room.AddClient(client);
//             MsgService.SendTcpMessage(client, "Joined room successfully!");
//         }
//         else
//         {
//             MsgService.SendTcpMessage(client, "Room does not exist.");
//         }
//     }
//
//     public void LeaveRoom(string roomId, TcpClient? client)
//     {
//         if (_rooms.TryGetValue(roomId, out var room))
//         {
//             room.RemoveClient(client, () => RemoveRoom(roomId));
//             MsgService.SendTcpMessage(client, "Left room successfully!");
//         }
//         else
//         {
//             MsgService.SendTcpMessage(client, "Room does not exist.");
//         }
//     }
//
//     private void RemoveRoom(string roomId)
//     {
//         if(_rooms.ContainsKey(roomId)) _rooms.Remove(roomId); 
//     }
// }