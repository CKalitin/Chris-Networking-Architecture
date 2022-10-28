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
            _packet.Write(Client.instance.id);

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

    public static void ServerDataObject(ServerDataObject _serverDataObject) {
        // Use a try catch statement to throw an error if code is not run successfully
        try {
            using (Packet _packet = new Packet((int)ClientPackets.serverData)) { // Using creates a new object then destroys it
                List<object> variables = _serverDataObject.Vars; // Get list of objects in the Server Data Packet

                _packet.Write(_serverDataObject.Id); // Write index / id of _serverDataObject to packet

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

                // Send to server
                SendTCPData(_packet);
            }
        } catch {
            Debug.LogError($"Could not send Server Data Packet, Assumed Id is: {_serverDataObject.Id}");
        }
    }

    #endregion
}
