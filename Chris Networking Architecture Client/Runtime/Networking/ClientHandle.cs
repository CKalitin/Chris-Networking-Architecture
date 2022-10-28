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
        Client.instance.maxPlayers = _packet.ReadInt();

        Debug.Log($"Message from Server: {_msg}");
        Client.instance.id = _myId;
        ClientSend.WelcomeReceived();

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);

        NetworkManager.instance.Welcome();
    }

    public static void ClientObjectUpdate(Packet _packet) {
        int _objectId = _packet.ReadInt();
        Vector3 _pos = _packet.ReadVector3();
        Quaternion _rot = _packet.ReadQuaternion();
        Vector3 _scale = _packet.ReadVector3();

        NetworkManager.instance.ClientObjectUpdate(_objectId, _pos, _rot, _scale);
    }

    public static void ClientObjectNew(Packet _packet) {
        int _prefabIndex = _packet.ReadInt();
        int _index = _packet.ReadInt();

        NetworkManager.instance.ClientObjectNew(_prefabIndex, _index);
    }

    public static void ClientObjectDelete(Packet _packet) {
        int _objectId = _packet.ReadInt();

        NetworkManager.instance.ClientObjectDelete(_objectId);
    }

    public static void ClientDataObject(Packet _packet) {
        int clientDataObjectId = _packet.ReadInt(); // Read id of this clientDataObject

        ClientDataObject clientDataObject = new ClientDataObject(clientDataObjectId); // Create new ClientDataObject

        // Loop through variables in serverDataStruct at index of _serverDataObjectIndex
        for (int i = 0; i < NetworkManager.instance.clientDataStructs[clientDataObjectId].variables.Count; i++) {
            // Get current data type in loop
            NetworkManager.DataType dataType = NetworkManager.instance.clientDataStructs[clientDataObjectId].variables[i];

            // Loop through dataTypes list in the serverDataStructs that's set by the user in Network Manager
            // If you find the type that the variable is, read it and add it to the list of variables
            if (dataType.isByte) {
                clientDataObject.Write(_packet.ReadByte());
            } else if (dataType.isByteArray) {
                clientDataObject.Write(_packet.ReadBytes(_packet.ReadInt()));
            } else if (dataType.isShort) {
                clientDataObject.Write(_packet.ReadInt());
            } else if (dataType.isInt) {
                clientDataObject.Write(_packet.ReadInt());
            } else if (dataType.isLong) {
                clientDataObject.Write(_packet.ReadLong());
            } else if (dataType.isFloat) {
                clientDataObject.Write(_packet.ReadFloat());
            } else if (dataType.isBool) {
                clientDataObject.Write(_packet.ReadBool());
            } else if (dataType.isString) {
                clientDataObject.Write(_packet.ReadString());
            } else if (dataType.isVector2) {
                clientDataObject.Write(_packet.ReadVector2());
            } else if (dataType.isVector3) {
                clientDataObject.Write(_packet.ReadVector3());
            } else if (dataType.isQuaternion) {
                clientDataObject.Write(_packet.ReadQuaternion());
            }
        }

        NetworkManager.instance.ClientDataObject(clientDataObject);
    }
}
