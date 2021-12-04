using System.Collections.Generic;
using UnityEngine;

public class ServerHandle {
    #region Packets

    public static void WelcomeReceived(int _fromClient, Packet _packet) {
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();

        Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now Player {_fromClient}.");
        if (_fromClient != _clientIdCheck) {
            Debug.Log($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
        }
    }

    public static void TCPInput(int _fromClient, Packet _packet) {
        List<int> keysDown = new List<int>();
        List<int> keysUp = new List<int>();

        int keysDownCount = _packet.ReadInt();
        int keysUpCount = _packet.ReadInt();

        if (keysDownCount > 0) {
            for (int i = 0; i < keysDownCount; i++) {
                keysDown.Add(_packet.ReadInt());
            }
        }

        if (keysUpCount > 0) {
            for (int i = 0; i < keysUpCount; i++) {
                keysUp.Add(_packet.ReadInt());
            }
        }

        // If client that sent the data is still connected
        if (Server.clients[_fromClient].connected) {
            Server.clients[_fromClient].InputManager.SetTCPInput(keysDown, keysUp);
        }
    }

    public static void UDPInput(int _fromClient, Packet _packet) {
        Vector2 _mousePos = _packet.ReadVector2(); // Read mouse pos (dependent on client monitor)

        // If client that sent the data is still connected
        if (Server.clients[_fromClient].connected) {
            Server.clients[_fromClient].InputManager.SetUDPInput(_mousePos);
        }
    }

    public static void ServerDataPacket(int _fromClient, Packet _packet) {
        int _serverDataPacketId = _packet.ReadInt(); // Read id of this clientDataPacket
        Server.serverDataPacketHandlers[_serverDataPacketId](_fromClient, _packet); // Send packet to handler of clientDataPacket of this id
    }

    #endregion

    #region Server Data Packets Handlers

    // Id = 1, Types = Int, 
    public static void ReadServerDataPacketTest(int _fromClient, Packet _packet) {
        ServerDataPacket serverDataPacket = new ServerDataPacket(0); // Create new ServerDataPacket

        // Create list of variables (This is manual for every server data packet type)
        List<object> packetVars = new List<object>();
        packetVars.Add(1); // This adds the id of the serverDataPacket
        packetVars.Add(_fromClient); // This adds the client that send this packet to the serverDataPacket
        packetVars.Add(_packet.ReadInt());

        // Set ServerDataPacket vars to new list of vars
        serverDataPacket.Vars = packetVars;

        NetworkManager.instance.ServerDataPacket(serverDataPacket);
    }

    #endregion
}
