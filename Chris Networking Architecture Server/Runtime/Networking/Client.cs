using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client {
    public static int dataBufferSize = 4096;
    public TCP tcp;
    public UDP udp;

    public int id;

    public bool connected = false;

    private InputManager inputManager;
    public InputManager Input { get { return inputManager; } }

    public Client(int _clientID) {
        id = _clientID;
        tcp = new TCP(id, this);
        udp = new UDP(id);
        inputManager = new InputManager();
    }

    public class TCP {
        public TcpClient socket;

        private readonly int id;
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        Client client;

        public TCP(int _id, Client _client) {
            id = _id;
            client = _client;
        }

        public void Connect(TcpClient _socket) {
            socket = _socket;
            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;

            stream = socket.GetStream();

            receivedData = new Packet();
            receiveBuffer = new byte[dataBufferSize];

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

            ServerSend.Welcome(id, "Welcome to the server!");

            client.connected = true;
        }

        public void SendData(Packet _packet) {
            try {
                if (socket != null) {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            } catch (Exception _ex) {
                Debug.Log($"Error sending data to client {id} via TCP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result) {
            try {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0) {
                    Server.clients[id].Disconnect();
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receivedData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            } catch (Exception _ex) {
                Debug.Log($"Error recieving TCP data: {_ex}");
                Server.clients[id].Disconnect();
            }
        }


        private bool HandleData(byte[] _data) {
            int _packetLength = 0;

            receivedData.SetBytes(_data);

            if (receivedData.UnreadLength() >= 4) {
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0) {
                    return true;
                }
            }

            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength()) {
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() => {
                    using (Packet _packet = new Packet(_packetBytes)) {
                        int _packetId = _packet.ReadInt();
                        Server.packetHandlers[_packetId](id, _packet);
                    }
                });

                _packetLength = 0;
                if (receivedData.UnreadLength() >= 4) {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0) {
                        return true;
                    }
                }
            }

            if (_packetLength <= 1) {
                return true;
            }

            return false;
        }

        public void Disconnect() {
            socket.Close();
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    public class UDP {
        public IPEndPoint endPoint;

        private int id;

        public UDP(int _id) {
            id = _id;
        }

        public void Connect(IPEndPoint _endPoint) {
            endPoint = _endPoint;
        }

        public void SendData(Packet _packet) {
            Server.SendUDPData(endPoint, _packet);
        }

        public void handleData(Packet _packetData) {
            int _packetLength = _packetData.ReadInt();
            byte[] _packetBytes = _packetData.ReadBytes(_packetLength);


            ThreadManager.ExecuteOnMainThread(() => {
                using (Packet _packet = new Packet(_packetBytes)) {
                    int _packetId = _packet.ReadInt();
                    Server.packetHandlers[_packetId](id, _packet);
                }
            });
        }

        public void Disconnect() {
            endPoint = null;
        }
    }

    public void Disconnect() {
        Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");

        connected = false;

        ThreadManager.ExecuteOnMainThread(() => {
            /*if (player != null) {
                UnityEngine.Object.Destroy(player.transform.parent.gameObject);
                player = null;
            }*/
        });

        tcp.Disconnect();
        udp.Disconnect();
    }
}
