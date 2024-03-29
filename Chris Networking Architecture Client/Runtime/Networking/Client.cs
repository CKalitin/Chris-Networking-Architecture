﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour {
    #region Variables

    public static Client instance;
    public static int dataBufferSize = 4096;

    public string ip = "127.0.0.1";
    public int port = 26950;
    [HideInInspector] public int id = 0;
    [HideInInspector] public TCP tcp;
    [HideInInspector] public UDP udp;

    [HideInInspector] public bool isConnected = false;

    [Space]
    public int maxPlayers;

    private delegate void PacketHandler(Packet _packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    #endregion

    #region Core

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Start() {
        tcp = new TCP();
        udp = new UDP();
    }

    private void OnApplicationQuit() {
        Disconnect();
    }

    #endregion

    #region TCP & UDP

    public class TCP {
        public TcpClient socket;

        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public void Connect() {
            socket = new TcpClient {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult _result) {
            socket.EndConnect(_result);

            if (!socket.Connected) {
                return;
            }

            stream = socket.GetStream();

            receivedData = new Packet();

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        public void SendData(Packet _packet) {
            try {
                if (socket != null) {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            } catch (Exception _ex) {
                Debug.Log($"Error sending data to server via TCP: {_ex}");
                Disconnect();
            }
        }

        private void ReceiveCallback(IAsyncResult _result) {
            try {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0) {
                    instance.Disconnect();
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receivedData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            } catch {
                Disconnect();
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

            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength()){
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() => {
                    using (Packet _packet = new Packet(_packetBytes)) {
                        int _packetId = _packet.ReadInt();
                        packetHandlers[_packetId](_packet);
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

            if (_packetLength  <= 1) {
                return true;
            }

            return false;
        }

        private void Disconnect() {
            instance.Disconnect();

            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    public class UDP {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP() {
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        public void Connect(int _localPort) {
            socket = new UdpClient(_localPort);

            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            using (Packet _packet = new Packet()) {
                SendData(_packet);
            }
        }

        public void SendData(Packet _packet) {
            try {
                _packet.InsertInt(instance.id);
                if (socket != null) {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                }
            } catch (Exception _ex) {
                Debug.Log($"Error sending data to server via UDP: {_ex}");
                Disconnect();
            }
        }

        private void ReceiveCallback(IAsyncResult _result) {
            try {
                byte[] _data = socket.EndReceive(_result, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (_data.Length < 4) {
                    instance.Disconnect();
                    return;
                }

                HandleData(_data);
            } catch {
                Disconnect();
            }
        }

        private void HandleData(byte[] _data) {
            using (Packet _packet = new Packet(_data)) {
                int _packetLength = _packet.ReadInt();
                _data = _packet.ReadBytes(_packetLength);
            };

            ThreadManager.ExecuteOnMainThread(() => {
                using (Packet _packet = new Packet(_data)) {
                    int _packetId = _packet.ReadInt();
                    packetHandlers[_packetId](_packet);
                }
            });
        }

        private void Disconnect() {
            instance.Disconnect();

            endPoint = null;
            socket = null;
        }
    }

    #endregion

    #region Functions

    public void SetIP(string _ip) {
        if (_ip != null) {
            instance.ip = _ip;
        }
        // Hey future 2022 Aug 2nd Chris here taking notes for SNL package rewrite of this horrible code :)
        // You didn't fuckin Close the UDP socket bro
        // Ur past Chris I'm allowed to make fun of you
        // Btw hey there future 2022+ Chris, I know I'm a giant fucking idiot but i'm trying! "Do or do not" lol
        udp = null;
        udp = new UDP();
    }

    public void ConnectToServer() {
        if (!isConnected) {
            InitializeClientData();

            tcp.Connect();
            if (udp == null) {
                udp = new UDP();
            }

            isConnected = true;
            Debug.Log("Connected to Server of IP: " + ip);
        }
    }

    private void InitializeClientData() {
        // Initialize Packets
        packetHandlers = new Dictionary<int, PacketHandler>() {
            { (int)ServerPackets.welcome, ClientHandle.Welcome },
            { (int)ServerPackets.clientObjectUpdate, ClientHandle.ClientObjectUpdate },
            { (int)ServerPackets.clientObjectNew, ClientHandle.ClientObjectNew },
            { (int)ServerPackets.clientObjectDelete, ClientHandle.ClientObjectDelete },
            { (int)ServerPackets.clientData, ClientHandle.ClientDataObject },
        };
        Debug.Log("Initializing Packets...");
    }

    public void Disconnect() {
        if (isConnected) {
            tcp.socket.Close();

            try {
                udp.socket.Close();
            } catch {

            }
            udp = null;

            NetworkManager.instance.ResetClientObjects();
            NetworkManager.instance.ResetCallbacks();

            isConnected = false;

            NetworkManager.instance.CallDisconnectedFromServerCallbacks();
    
            Debug.Log("Disconnected from server.");
        }
    }

    #endregion
}
