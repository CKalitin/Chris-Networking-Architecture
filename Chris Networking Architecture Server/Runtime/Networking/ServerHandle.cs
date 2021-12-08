using System.Collections.Generic;
using UnityEngine;

public class ServerHandle {
    #region Packets

    public static void WelcomeReceived(int _fromClient, Packet _packet) {
        int _clientIdCheck = _packet.ReadInt();

        Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now Player {_fromClient}.");
        if (_fromClient != _clientIdCheck) {
            Debug.Log($"ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck}.");
        }

        NetworkManager.instance.SendClientObjectsOnClient(_fromClient);
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
        if (Server.clients[_fromClient].isConnected) {
            Server.clients[_fromClient].Input.SetTCPInput(keysDown, keysUp);
        }
    }

    public static void UDPInput(int _fromClient, Packet _packet) {
        Vector2 _mousePos = _packet.ReadVector2(); // Read mouse pos (dependent on client monitor)

        // If client that sent the data is still connected
        if (Server.clients[_fromClient].isConnected) {
            Server.clients[_fromClient].Input.SetUDPInput(_mousePos);
        }
    }

    public static void ServerDataObject(int _fromClient, Packet _packet) {
        int serverDataObjectId = _packet.ReadInt(); // Read id of this clientDataObject
        // Use a try catch statement to throw an error if code is not run successfully
        try {
            ServerDataObject serverDataObject = new ServerDataObject(serverDataObjectId); // Create new ServerDataObject
            serverDataObject.FromClient = _fromClient; // Set fromClient in ServerDataObject to client that sent this packet
            // Loop through variables in serverDataStruct at index of _serverDataObjectIndex
            for (int i = 0; i < NetworkManager.instance.serverDataStructs[serverDataObjectId].variables.Count; i++) {
                // Get current data type in loop
                NetworkManager.DataType dataType = NetworkManager.instance.serverDataStructs[serverDataObjectId].variables[i];

                // Loop through dataTypes list in the serverDataStructs that's set by the user in Network Manager
                // If you find the type that the variable is, read it and add it to the list of variables
                if (dataType.isByte) {
                    serverDataObject.Write(_packet.ReadByte());
                } else if (dataType.isByteArray) {
                    serverDataObject.Write(_packet.ReadBytes(_packet.ReadInt()));
                } else if (dataType.isShort) {
                    serverDataObject.Write(_packet.ReadInt());
                } else if (dataType.isInt) {
                    serverDataObject.Write(_packet.ReadInt());
                } else if (dataType.isLong) {
                    serverDataObject.Write(_packet.ReadLong());
                } else if (dataType.isFloat) {
                    serverDataObject.Write(_packet.ReadFloat());
                } else if (dataType.isBool) {
                    serverDataObject.Write(_packet.ReadBool());
                } else if (dataType.isString) {
                    serverDataObject.Write(_packet.ReadString());
                } else if (dataType.isVector2) {
                    serverDataObject.Write(_packet.ReadVector2());
                } else if (dataType.isVector3) {
                    serverDataObject.Write(_packet.ReadVector3());
                } else if (dataType.isQuaternion) {
                    serverDataObject.Write(_packet.ReadQuaternion());
                }
            }
            NetworkManager.instance.ServerDataObject(serverDataObject);
        } catch {
            Debug.LogError($"Could not read Server Data Packet of id: {serverDataObjectId}");
        }
    }

    #endregion
}
