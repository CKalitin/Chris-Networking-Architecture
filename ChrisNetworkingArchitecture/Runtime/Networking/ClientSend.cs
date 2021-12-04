using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour {
    public static bool singlePlayer = false;

    private static void SendTCPData(Packet _packet) {
        _packet.WriteLength();
        if (singlePlayer) {

        } else {
            if (Client.instance.isConnected && ClientHandle.welcomeReceived) {
                Client.instance.tcp.SendData(_packet);
            }
        }
    }

    private static void SendUDPData(Packet _packet) {
        _packet.WriteLength();
        if (singlePlayer) {

        } else {
            if (Client.instance.isConnected && ClientHandle.welcomeReceived) {
                Client.instance.udp.SendData(_packet);
            }
        }
    }

    #region Packets

    public static void WelcomeReceived() {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived)) { // Using creates a new objects then destroys it
            _packet.Write(Client.instance.myId);
            _packet.Write(NetworkManager.instance.Username);

            SendTCPData(_packet);
        }
    }

    public static void TCPInput(List<int> _keysDown, List<int> _keysUp) {
        using (Packet _packet = new Packet((int)ClientPackets.tcpInput)) { // Using creates a new objects then destroys it
            _packet.Write(_keysDown.Count); // Write length of _keysDown so Server knows how many times to iterate the for loop 
            _packet.Write(_keysUp.Count); // Write length of _keysUp so Server knows how many times to iterate the for loop

            // Write the ids of the keys down
            if (_keysDown.Count > 0) {
                for (int i = 0; i < _keysDown.Count; i++) {
                    _packet.Write(_keysDown[i]);
                }
            }

            // Write the ids of the keys up
            if (_keysUp.Count > 0) {
                for (int i = 0; i < _keysUp.Count; i++) {
                    _packet.Write(_keysUp[i]);
                }
            }

            SendTCPData(_packet); // Send Packet
        }
    }

    public static void UDPInput(Vector2 _mousePos) {
        using (Packet _packet = new Packet((int)ClientPackets.udpInput)) { // Using creates a new objects then destroys it
            _packet.Write(_mousePos);

            SendUDPData(_packet);
        }
    }

    public static void ServerDataPacket(ServerDataPacket _serverDataPacket) {
        using (Packet _packet = new Packet((int)ClientPackets.serverData)) { // Using creates a new objects then destroys it
            // First value in _clientData is type of the ClientDataPacket, write it to packet
            _packet.Write((int)_serverDataPacket.Vars[0]);

            //Iterate through vars in _clientData
            for (int i = 1; i < _serverDataPacket.Vars.Count; i++) {
                // Check type of Vars[i], if it is the type in the if statement, write data to packet of that type
                // This is a horrible way to write data, but the solution i came up with, pls fix this future chris
                if (_serverDataPacket.Vars[i] is byte) {
                    _packet.Write((Byte)_serverDataPacket.Vars[i]);
                } else if (_serverDataPacket.Vars[i] is byte[]) {
                    _packet.Write((Byte[])_serverDataPacket.Vars[i]);
                } else if (_serverDataPacket.Vars[i] is short) {
                    _packet.Write((short)_serverDataPacket.Vars[i]);
                } else if (_serverDataPacket.Vars[i] is int) {
                    _packet.Write((int)_serverDataPacket.Vars[i]);
                } else if (_serverDataPacket.Vars[i] is long) {
                    _packet.Write((long)_serverDataPacket.Vars[i]);
                } else if (_serverDataPacket.Vars[i] is float) {
                    _packet.Write((float)_serverDataPacket.Vars[i]);
                } else if (_serverDataPacket.Vars[i] is bool) {
                    _packet.Write((bool)_serverDataPacket.Vars[i]);
                } else if (_serverDataPacket.Vars[i] is string) {
                    _packet.Write((string)_serverDataPacket.Vars[i]);
                } else if (_serverDataPacket.Vars[i] is Vector2) {
                    _packet.Write((Vector2)_serverDataPacket.Vars[i]);
                } else if (_serverDataPacket.Vars[i] is Vector3) {
                    _packet.Write((Vector3)_serverDataPacket.Vars[i]);
                } else if (_serverDataPacket.Vars[i] is Quaternion) {
                    _packet.Write((Quaternion)_serverDataPacket.Vars[i]);
                }
            }

            // Send to srever
            SendTCPData(_packet);
        }
    }


    #endregion
}
