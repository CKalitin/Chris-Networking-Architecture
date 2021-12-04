using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour {
    [HideInInspector] public static bool welcomeReceived;

    public static void Welcome(Packet _packet) {
        welcomeReceived = true;

        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from Server: {_msg}");
        Client.instance.myId = _myId;
        ClientSend.WelcomeReceived();

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    #region Client Object

    public static void ClientObjectUpdate(Packet _packet) {
        int _objectId = _packet.ReadInt();
        Vector2 _pos = _packet.ReadVector3();
        Quaternion _rot = _packet.ReadQuaternion();
        Vector3 _scale = _packet.ReadVector3();

        NetworkManager.instance.ClientObjectUpdate(_objectId, _pos, _rot, _scale);
    }

    public static void ClientObjectNew(Packet _packet) {
        int _prefabIndex = _packet.ReadInt();

        NetworkManager.instance.ClientObjectNew(_prefabIndex);
    }

    public static void ClientObjectDelete(Packet _packet) {
        int _objectId = _packet.ReadInt();
        NetworkManager.instance.ClientObjectDelete(_objectId);
    }

    public static void ClientDataPacket(Packet _packet) {
        int _clientDataPacketId = _packet.ReadInt(); // Read id of this clientDataPacket
        Client.clientDataPacketHandlers[_clientDataPacketId](_packet); // Send packet to handler of clientDataPacket of this id
    }

    #endregion

    #region Client Data Packets Handlers

    // Id = 1, Types = Int, 
    public static void ReadClientDataPacketTest(Packet _packet) {
        ClientDataPacket clientDataPacket = new ClientDataPacket(0); // Create new ClientDataPacket

        // Create list of variables (This is manual for every client data packet type)
        List<object> packetVars = new List<object>();
        packetVars.Add(1); // This adds the id of the clientDataPacket
        packetVars.Add(_packet.ReadInt());

        // Set ClientDataPacket vars to new list of vars
        clientDataPacket.Vars = packetVars;

        NetworkManager.instance.ClientDataPacket(clientDataPacket);
    }

    #endregion
}
