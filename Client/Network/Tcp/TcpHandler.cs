﻿using Client.Form;
using Client.Models;
using Client.Services;
using Shared;
using Shared.Models;
using Shared.Networking.Interfaces;
using Shared.Utils;

namespace Client.Network.Tcp;

public class TcpHandler : INetworkHandler
{
    public void OnDataReceived(string message, string sourceEndpoint) => HandleMessage(message);
    public void OnDataReceived(byte[] message, string sourceEndpoint) => HandleMessage(ByteUtils.GetStringFromBytes(message));
    public void OnClientDisconnect<T>(T? client) where T : class
    {
        //Todo Reconnect network
        throw new NotImplementedException();
    }

    private static Task HandleMessage(string message)
    {
        var msg = MessageNetwork<dynamic>.FromJson(message);
        //if (msg?.Code != StatusCode.Success) return Task.CompletedTask;
        switch (msg?.Type)
        {
            case CommandType.Login:
                if (msg.Code == StatusCode.Success && msg.TryParseData(out User? user) && user != null)
                {
                    var loginForm = FormController.GetForm<LoginForm>(FormType.Login);
                    if((bool)loginForm?.RememberMe) AuthService.SaveUserInfo(user);
                    if (user.Id != null)
                    {
                        SessionManager.SetUser(user);
                        //Show Home Form and Dialog Login Success
                        MessageBox.Show("Login Success.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        FormController.Show(FormType.Home);
                    }
                    else
                    {
                        AuthService.Logout();
                        FormController.Show(FormType.Login);
                    }
                }
                else
                {
                    AuthService.Logout();
                    FormController.Show(FormType.Login);
                    MessageBox.Show("Login Failed Try Again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                break;
            case CommandType.Registration:
                if (msg.Code == StatusCode.Success)
                {
                    MessageBox.Show("Register Success Login to Continue", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    FormController.Show(FormType.Login);
                }
                else
                {
                    MessageBox.Show("Register Failed Try Again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                break;
            case CommandType.GetAvailableClients:
                if (msg.TryParseData(out List<UserDto>? allUsers) && allUsers != null)
                {
                    FormController.GetForm<HomeForm>(FormType.Home)?.SetAllUsers(allUsers);
                }
                break;
            case CommandType.HandshakeRequest:
                if (msg.TryParseData(out HandshakeDto? dtoRequest) && dtoRequest != null)
                {
                    if (msg.Code == StatusCode.Success)
                    {
                        FormController.GetForm<HomeForm>(FormType.Home)?.ShowHandshakeConfirm(dtoRequest);
                    }
                    else
                    {
                        FormController.GetForm<HomeForm>(FormType.Home)?.ShowHandshakeError(dtoRequest.Description);
                    }
                }
                break;
            case CommandType.HandshakeResponse:
                if (msg.TryParseData(out HandshakeDto? dtoResponse) && dtoResponse != null)
                {
                    if (msg.Code == StatusCode.Success)
                    {
                        if (dtoResponse.Accepted)
                        {
                            FormController.GetForm<HomeForm>(FormType.Home)?.HandShakeSuccess(dtoResponse.Description);
                            FormController.Show(FormType.Chat);
                            FormController.GetForm<ChatForm>(FormType.Chat)?.SetUserTarget(dtoResponse.FromUser?.Id == SessionManager.GetUserId() ? dtoResponse.ToUser : dtoResponse.FromUser);
                        }
                        else
                        {
                            FormController.GetForm<HomeForm>(FormType.Home)?.ShowHandshakeError(
                                dtoResponse.FromUser?.Id == SessionManager.GetUserId()
                                    ? $"{dtoResponse.ToUser?.UserName} rejected handshake request."
                                    : $"You rejected handshake request.");
                        }
                        
                    } else
                    {
                        FormController.GetForm<HomeForm>(FormType.Home)?.ShowHandshakeError(dtoResponse.Description);
                    }
                }
                break;
            case CommandType.GetHandshakeUsers:
                if (msg.TryParseData(out ConversationRecord? c) && c != null)
                {
                    FormController.GetForm<HomeForm>(FormType.Home)?.LoadHandshake(c);
                }
                break;
            case CommandType.HandshakeCancel:
                if (msg.TryParseData(out HandshakeDto? cancel) && cancel != null)
                {
                    if (msg.Code == StatusCode.Success)
                    {
                        MessageBox.Show("Handshake Cancelled", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        FormController.Show(FormType.Home);
                    }
                    else
                    {
                        MessageBox.Show("Handshake Cancelled Try Again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                break;
            case CommandType.DispatchMessage:
                if (msg.TryParseData(out MessageDto? md) && md?.Data != null)
                {
                    var chatForm = FormController.GetForm<ChatForm>(FormType.Chat);
                    if (chatForm != null)
                    {
                        if (chatForm.InvokeRequired)
                        {
                            chatForm.Invoke(new Action(() => chatForm.AddMessage(md.Data, false)));
                        }
                        else
                        {
                            chatForm.AddMessage(md.Data, false);
                        }
                    }
                }else if (msg.TryParseData<FileChunkMessageDto>(out var fileChunkMsg) && fileChunkMsg != null)
                {
                    // Xử lý tin nhắn file bằng cách đưa các chunk vào FileChunkReceiver.
                    FileChunkService.Instance.ProcessChunk(fileChunkMsg, (fileId, fullData) =>
                    {
                        Logger.LogInfo($"Receive file success {fileId} with length {fullData.Length}");
                        var transferData = FileChunkService.ParseTransferData(fullData);
                        Logger.LogInfo($"Success parse {transferData}");
                        var chatForm = FormController.GetForm<ChatForm>(FormType.Chat);
                        if (chatForm == null) return;
                        if (chatForm.InvokeRequired)
                        {
                            chatForm.Invoke(() => chatForm.AddMessage(transferData, false));
                        }
                        else
                        {
                            _ = chatForm.AddMessage(transferData, false);
                        }
                    });
                }
                break;
            case CommandType.UpdateStatusUsers:
                if (msg.TryParseData(out UserDto? ud) && ud != null)
                {
                    FormController.GetForm<HomeForm>(FormType.Home)?.UpdateStatus(ud);
                }
                break;
            case CommandType.CancelDispatchMessage:
                if (msg.TryParseData(out FileChunkMessageDto? f) && f != null)
                {
                    FileChunkService.Instance.CancelProcessChunk(f);
                }
                break;
            default:
                Logger.LogWarning("Unknown Command");
                MessageBox.Show(@"Error... Please Try Again", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                break;
        }

        return Task.CompletedTask;
    }
    

    public void OnClientConnected<T>(T? client) where T : class
    {
    }

    public void BroadcastMessage(string message)
    {
        throw new NotImplementedException();
    }

    public void BroadcastMessageExcept<T>(T? excludedClient, string message) where T : class
    {
        throw new NotImplementedException();
    }

    public void BroadcastMessageExcept<T>(T[] excludedClient, string message) where T : class
    {
        throw new NotImplementedException();
    }
}