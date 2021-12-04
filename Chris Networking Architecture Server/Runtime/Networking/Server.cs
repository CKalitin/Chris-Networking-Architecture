using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using UnityEngine;

public class Server {
    public static int MaxClients { get; private set; }

    public static int Port { get; private set; }

    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

    public delegate void PacketHandler(int _fromClient, Packet _packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    // Dictionary of delegates (reference to function) to clientDataPacketHandlers
    public delegate void ServerDataPacketHandler(int _fromClient, Packet _packet);
    public static Dictionary<int, ServerDataPacketHandler> serverDataPacketHandlers;

    private static TcpListener tcpListener;
    private static UdpClient udpListener;

    public static void Start(int _maxClients, int _port) {
        MaxClients = _maxClients;
        Port = _port;

        Debug.Log("Starting server...");
        InitializeServerData();

        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

        udpListener = new UdpClient(Port);
        udpListener.BeginReceive(UDPReceiveCallback, null);

        Debug.Log($"Server started on {Port}.");
    }

    private static void TCPConnectCallback(IAsyncResult _result) {
        TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
        Debug.Log($"Incoming connection from {_client.Client.RemoteEndPoint}...");

        for (int i = 1; i <= MaxClients; i++) {
            if (clients[i].tcp.socket == null) {
                clients[i].tcp.Connect(_client);
                return;
            }
        }

        Debug.Log($"{_client.Client.RemoteEndPoint} failed to connect: Server full.");
    }

    private static void UDPReceiveCallback(IAsyncResult _result) {
        try {
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            if (_data.Length < 4) {
                return;
            }

            using (Packet _packet = new Packet(_data)) {
                int _clientId = _packet.ReadInt();

                if (_clientId == 0) {
                    return;
                }

                if (clients[_clientId].udp.endPoint == null) {
                    clients[_clientId].udp.Connect(_clientEndPoint);
                    return;
                }

                if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString()) {
                    clients[_clientId].udp.handleData(_packet);
                }
            }
        } catch (Exception _ex) {
            Debug.Log($"Error receoving UDP data: {_ex}");
        }
    }

    public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet) {
        try {
            if (_clientEndPoint != null) {
                udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
            }
        } catch (Exception _ex) {
            Debug.Log($"Error sending data tp {_clientEndPoint} via UDP {_ex}");
        }
    }

    private static void InitializeServerData() {
        for (int i = 1; i <= MaxClients; i++) {
            clients.Add(i, new Client(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>() {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.tcpInput, ServerHandle.TCPInput },
                { (int)ClientPackets.udpInput, ServerHandle.UDPInput },
                { (int)ClientPackets.serverData, ServerHandle.ServerDataPacket },
            };
        Debug.Log("Initialized packets.");

        // Initialize ClientDataPacketHandlers with delegates (search it up, they're cool)
        serverDataPacketHandlers = new Dictionary<int, ServerDataPacketHandler>() {
                { (int)ServerDataPacket.ServerDataPacketTypes.test, ServerHandle.ReadServerDataPacketTest },
            };
        Debug.Log("Initialized Server Data Packet Handlers.");
    }

    public static void Stop() {
        tcpListener.Stop();
        udpListener.Close();
    }
}
