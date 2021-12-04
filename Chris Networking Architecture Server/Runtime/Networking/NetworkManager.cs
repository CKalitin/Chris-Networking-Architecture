using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour {
    public static NetworkManager instance;

    public static List<ClientObject> clientObjects = new List<ClientObject>();

    public delegate void ServerDataCallback(ServerDataPacket _serverDataPacket); // Create a delegate for callback functions
    public static List<KeyValuePair<int, ServerDataCallback>> serverDataCallbacks = new List<KeyValuePair<int, ServerDataCallback>>(); // 2D List of ClientDataCallbacks, each ClientDataCallback is in one list inside another, so it has 2 indexes

    #region Core

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }

        if (Application.isEditor) {
            Application.runInBackground = true;
        }
    }

    private void Start() {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        Server.Start(50, 26950);
    }

    private void LateUpdate() {
        // Loop through all clients to get their InputManagers
        for (int i = 1; i <= Server.clients.Count; i++) {
            // Loop through all Input Managers keys to reset
            for (int x = 0; x < Server.clients[i].InputManager.LocalKeyCodesToReset.Count; x++) {
                InputManager inputManager = Server.clients[i].InputManager; // Get InputManager of client[i]

                bool pressed = inputManager.LocalKeyCodes[inputManager.LocalKeyCodesToReset[x]].pressed; // Get pressed variable of current LocalKeyCode in client
                inputManager.SetLocalKeyCode(inputManager.LocalKeyCodesToReset[x], false, false, pressed); // Reset Vars of current LocalKeyCode
            }
            Server.clients[i].InputManager.LocalKeyCodesToReset.Clear();
        }
    }

    private void OnApplicationQuit() {
        Server.Stop();
    }

    #endregion

    #region Sending Data

    public static void ClientObjectDestroy(GameObject _gameObject) {
        Destroy(_gameObject);
    }

    public static void ClientObjectNew(int _toClient, ClientObject clientObject) {
        clientObject.objectId = clientObjects.Count;
        clientObjects.Add(clientObject);

        ServerSend.ClientObjectNew(_toClient, clientObject.prefabIndex);
    }

    public static void ClientObjectDelete(int _toClient, int _objectId) {
        ServerSend.ClientObjectDelete(_toClient, _objectId);

        clientObjects.RemoveAt(_objectId);
        if (clientObjects.Count > 0 && _objectId <= clientObjects.Count) {
            for (int i = clientObjects.Count -1 ; i >= _objectId; i--) {
                clientObjects[i].objectId -= 1;
            }
        }
    }

    public static void ClientObjectUpdate(int _toClient, int _objectId, Vector3 _pos, Quaternion _rot, Vector3 _scale) {
        ServerSend.ClientObjectUpdate(_toClient, _objectId, _pos, _rot, _scale);
    }

    public static void ClientDataPacketSend(int _toClient, ClientDataPacket _clientDataPacket) {
        ServerSend.ClientDataPacket(_toClient, _clientDataPacket);
    }

    #endregion

    #region Handling Data

    public void ServerDataPacket(ServerDataPacket _serverDataPacket) {
        // Loop through dictionary of serverDataCallbacks
        foreach (var serverDataCallback in serverDataCallbacks) {
            if (serverDataCallback.Key == (int)_serverDataPacket.Vars[0]) { // If ServerDataPacket id of callback is equal to CDP of this scriptable object
                serverDataCallback.Value(_serverDataPacket); // Run callback and pass ServerDataPacket as a parameter
            }
        }
    }

    #endregion

    #region Other

    public void AddServerDataCallback(ServerDataCallback _callback, int _serverDataPacketId) {
        serverDataCallbacks.Add(new KeyValuePair<int, ServerDataCallback>(_serverDataPacketId, _callback)); // Add new serverDataCallback delegate to list of callbacks at proper position with id specified
    }

    #endregion
}
