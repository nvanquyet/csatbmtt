﻿using Shared.Networking;
using Shared.Networking.Interfaces;

namespace DesClient.Network.Udp;

public class UdpProtocol(INetworkHandler dataHandler) : ANetworkProtocol(dataHandler)
{
    public override void Start(int port)
    {
        throw new NotImplementedException();
    }

    public override void Send(byte[] data, string endpoint)
    {
        throw new NotImplementedException();
    }
}