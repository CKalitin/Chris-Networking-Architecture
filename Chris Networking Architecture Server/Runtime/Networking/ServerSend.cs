using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend {
    private static void SendTCPData(int _toClient, Packet _packet) {
        _packet.WriteLength();
        Server.clients[_toClient].tcp.SendData(_packet);
    }
    private static void SendTCPDataToAll(Packet _packet) {
        _packet.WriteLength();
        for (int i = 1; i < Server.MaxClients; i++) {
            Server.clients[i].tcp.SendData(_packet);
        }
    }

    private static void SendTCPDataToAll(int _excpetClient, Packet _packet) {
        _packet.WriteLength();
        for (int i = 1; i < Server.MaxClients; i++) {
            if (i != _excpetClient) {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
    }

    private static void SendUDPData(int _toClient, Packet _packet) {
        _packet.WriteLength();
        Server.clients[_toClient].udp.SendData(_packet);
    }

    private static void SendUDPDataToAll(Packet _packet) {
        _packet.WriteLength();
        for (int i = 1; i < Server.MaxClients; i++) {
            Server.clients[i].udp.SendData(_packet);
        }
    }

    private static void SendUDPDataToAll(int _excpetClient, Packet _packet) {
        _packet.WriteLength();
        for (int i = 1; i < Server.MaxClients; i++) {
            if (i != _excpetClient) {
                Server.clients[i].udp.SendData(_packet);
            }
        }
    }

    #region Packets

    public static void Welcome(int _toClient, string _msg) {
        using (Packet _packet = new Packet((int)ServerPackets.welcome)) { // Using creates a new objects then destroys it
            _packet.Write(_msg);
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }
    }

    public static void ClientObjectUpdate(int _toClient, int _objectId, Vector3 _pos, Quaternion _rot, Vector3 _scale) {
        using (Packet _packet = new Packet((int)ServerPackets.clientObjectUpdate)) { // Using creates a new objects then destroys it
            _packet.Write(_objectId);
            _packet.Write(_pos);
            _packet.Write(_rot);
            _packet.Write(_scale);

            // if id of client to send to is lower than 1, send to all clients
            if (_toClient < 0) {
                SendUDPDataToAll(_packet);
            } else {
                SendUDPData(_toClient, _packet);
            }
        }
    }

    // Send New Client Object
    public static void ClientObjectNew(int _toClient, int _prefabIndex) {
        using (Packet _packet = new Packet((int)ServerPackets.clientObjectNew)) { // Using creates a new objects then destroys it
            // Prefab index in Client
            _packet.Write(_prefabIndex);

            // if id of client to send to is lower than 1, send to all clients
            if (_toClient < 0) {
                SendTCPDataToAll(_packet);
            } else {
                SendTCPData(_toClient, _packet);
            }
        }
    }

    public static void ClientObjectDelete(int _toClient, int _objectId) {
        using (Packet _packet = new Packet((int)ServerPackets.clientObjectDelete)) { // Using creates a new objects then destroys it
            _packet.Write(_objectId);

            // if id of client to send to is lower than 1, send to all clients
            if (_toClient < 0) {
                SendTCPDataToAll(_packet);
            } else {
                SendTCPData(_toClient, _packet);
            }
        }
    }

    public static void ClientDataObject(int _toClient, ClientDataObject _clientDataObject) {
        // Use a try catch statement to throw an error if code is not run successfully
        try {
            using (Packet _packet = new Packet((int)ServerPackets.clientData)) { // Using creates a new object then destroys it
                List<object> variables = _clientDataObject.Vars; // Get list of object variables in the Server Data Packet

                _packet.Write(_clientDataObject.Id); // First value in _clientData is type / index of the ClientDataObject, write it to packet

                //Iterate through vars in _clientData
                for (int i = 0; i < variables.Count; i++) {
                    // Check type of Vars[i], if it is the type in the if statement, write data to packet of that type
                    // This is a horrible way to write data, but the solution i came up with, pls fix this future chris
                    if (variables[i] is byte) {
                        _packet.Write((Byte)variables[i]);
                    } else if (variables[i] is byte[]) {
                        _packet.Write((Byte[])variables[i]);
                    } else if (variables[i] is short) {
                        _packet.Write((short)variables[i]);
                    } else if (variables[i] is int) {
                        _packet.Write((int)variables[i]);
                    } else if (variables[i] is long) {
                        _packet.Write((long)variables[i]);
                    } else if (variables[i] is float) {
                        _packet.Write((float)variables[i]);
                    } else if (variables[i] is bool) {
                        _packet.Write((bool)variables[i]);
                    } else if (variables[i] is string) {
                        _packet.Write((string)variables[i]);
                    } else if (variables[i] is Vector2) {
                        _packet.Write((Vector2)variables[i]);
                    } else if (variables[i] is Vector3) {
                        _packet.Write((Vector3)variables[i]);
                    } else if (variables[i] is Quaternion) {
                        _packet.Write((Quaternion)variables[i]);
                    }
                }

                // if id of client to send to is lower than 1, send to all clients
                if (_toClient < 0) {
                    SendTCPDataToAll(_packet);
                } else {
                    SendTCPData(_toClient, _packet);
                }
            }
        } catch {
            Debug.LogError($"Could not send Client Data Packet, Assumed Id is: {_clientDataObject.Id}");
        }
    }

    #endregion
}
