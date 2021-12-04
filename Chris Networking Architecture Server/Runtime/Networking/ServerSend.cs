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

    public static void ClientDataPacket(int _toClient, ClientDataPacket _clientData) {
        using (Packet _packet = new Packet((int)ServerPackets.clientData)) { // Using creates a new objects then destroys it
            // First value in _clientData is type of the ClientDataPacket, write it to packet
            _packet.Write((int)_clientData.Vars[0]);

            //Iterate through vars in _clientData
            for (int i = 1; i < _clientData.Vars.Count; i++) {
                // Check type of Vars[i], if it is the type in the if statement, write data to packet of that type
                // This is a horrible way to write data, but the solution i came up with, pls fix this future chris
                if (_clientData.Vars[i] is byte) {
                    _packet.Write((Byte)_clientData.Vars[i]);
                } else if (_clientData.Vars[i] is byte[]) {
                    _packet.Write((Byte[])_clientData.Vars[i]);
                } else if (_clientData.Vars[i] is short) {
                    _packet.Write((short)_clientData.Vars[i]);
                } else if (_clientData.Vars[i] is int) {
                    _packet.Write((int)_clientData.Vars[i]);
                } else if (_clientData.Vars[i] is long) {
                    _packet.Write((long)_clientData.Vars[i]);
                } else if (_clientData.Vars[i] is float) {
                    _packet.Write((float)_clientData.Vars[i]);
                } else if (_clientData.Vars[i] is bool) {
                    _packet.Write((bool)_clientData.Vars[i]);
                } else if (_clientData.Vars[i] is string) {
                    _packet.Write((string)_clientData.Vars[i]);
                } else if (_clientData.Vars[i] is Vector2) {
                    _packet.Write((Vector2)_clientData.Vars[i]);
                } else if (_clientData.Vars[i] is Vector3) {
                    _packet.Write((Vector3)_clientData.Vars[i]);
                } else if (_clientData.Vars[i] is Quaternion) {
                    _packet.Write((Quaternion)_clientData.Vars[i]);
                }
            }

            // if id of client to send to is lower than 1, send to all clients
            if (_toClient < 0) {
                SendTCPDataToAll(_packet);
            } else {
                SendTCPData(_toClient, _packet);
            }
        }
    }

    #endregion
}
