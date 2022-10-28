using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkManager : MonoBehaviour {
    #region Variables

    // Everything here except Data Packets is hidden in Inspector because of the static keyword
    public static NetworkManager instance;

    public static Dictionary<int, ClientObject> clientObjects = new Dictionary<int, ClientObject>();

    public delegate void ServerDataCallback(ServerDataObject _serverDataObject); // Create a delegate for callback functions
    // List of serverDataCallbacks with the id of the ServerDataPacket
    public static List<KeyValuePair<int, ServerDataCallback>> serverDataCallbacks = new List<KeyValuePair<int, ServerDataCallback>>();

    // List of Client Connected Callbacks
    public delegate void ClientConnectedCallback(int _clientId); // Create a delegate for callback functions
    public static List<ClientConnectedCallback> clientConnectedCallbacks = new List<ClientConnectedCallback>();

    // List of Client Disconnected Callbacks
    public delegate void ClientDisconnectedCallback(int _clientId); // Create a delegate for callback functions
    public static List<ClientDisconnectedCallback> clientDisconnectedCallbacks = new List<ClientDisconnectedCallback>();

    #region Data Packets

    [Header("Networking")]
    public int port;
    public int maxPlayers;

    [Header("Data Packets")]
    public List<ServerDataStruct> serverDataStructs;
    public List<ClientDataStruct> clientDataStructs;

    [System.Serializable]
    public struct ServerDataStruct {
        public string name;
        public string description;
        public List<DataType> variables;
    };

    [System.Serializable]
    public struct ClientDataStruct {
        public string name;
        public string description;
        public List<DataType> variables;
    };

    [System.Serializable]
    public struct DataType {
        public string name;
        [Space]
        public bool isByte;
        public bool isByteArray;
        public bool isShort;
        public bool isInt;
        public bool isLong;
        public bool isFloat;
        public bool isBool;
        public bool isString;
        public bool isVector2;
        public bool isVector3;
        public bool isQuaternion;
    }

    #endregion

    #endregion

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

        Server.Start(maxPlayers, port);
    }

    private void LateUpdate() {
        // Loop through all clients to get their InputManagers
        for (int i = 1; i <= Server.clients.Count; i++) {
            // Loop through all Input Managers keys to reset
            for (int x = 0; x < Server.clients[i].Input.LocalKeyCodesToReset.Count; x++) {
                InputManager inputManager = Server.clients[i].Input; // Get InputManager of client[i]

                bool pressed = inputManager.LocalKeyCodes[inputManager.LocalKeyCodesToReset[x]].pressed; // Get pressed variable of current LocalKeyCode in client
                inputManager.SetLocalKeyCode(inputManager.LocalKeyCodesToReset[x], false, false, pressed); // Reset Vars of current LocalKeyCode
            }
            Server.clients[i].Input.LocalKeyCodesToReset.Clear();
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
        // If there is more than 1 client object, find lowest key
        if (clientObjects.Count > 0) {
            // Iterate through clientObjects and find lowest unassigned key
            for (int i = 0; i < clientObjects.Keys.Max() + 2; i++) {
                if (!clientObjects.ContainsKey(i)) {
                    clientObject.objectId = i;
                    break;
                }
            }
        } else {
            clientObject.objectId = 0;
        }

        // Add clientOjbect to list of clientObjects with new objectId
        clientObjects.Add(clientObject.objectId, clientObject);

        ServerSend.ClientObjectNew(_toClient, clientObject.prefabIndex, clientObject.objectId);

        ServerSend.ClientObjectUpdate(_toClient, clientObject.objectId, clientObject.transform.position, clientObject.transform.rotation, clientObject.transform.localScale);
    }

    public static void ClientObjectDelete(int _toClient, int _objectId) {
        //FindObjectOfType<NetworkManager>().StartCoroutine(RemoveClientObjectDelay(_objectId));
        clientObjects.Remove(_objectId);
        ServerSend.ClientObjectDelete(_toClient, _objectId);
    }

    private static IEnumerator RemoveClientObjectDelay(int _objectId) {
        yield return new WaitForSeconds(1);
        clientObjects.Remove(_objectId);
        //print("Removed objectId: " + _objectId);
    }

    public static void ClientObjectUpdate(int _toClient, int _objectId, Vector3 _pos, Quaternion _rot, Vector3 _scale) {
        ServerSend.ClientObjectUpdate(_toClient, _objectId, _pos, _rot, _scale);
    }

    public static void SendClientDataObject(int _toClient, ClientDataObject _clientDataObject) {
        ServerSend.ClientDataObject(_toClient, _clientDataObject);
    }

    #endregion

    #region Handling Data

    public void ServerDataObject(ServerDataObject _serverDataObject) {
        // Loop through dictionary of serverDataCallbacks
        if (serverDataCallbacks.Count > 0) {
            // Use .ToList() to copy the callbacks into another list so you don't get 'Collection was modified' error
            foreach (var serverDataCallback in serverDataCallbacks.ToList()) {
                if (serverDataCallback.Key == _serverDataObject.Id) { // If ServerDataObject id of callback is equal to CDP of this scriptable object
                    serverDataCallback.Value(_serverDataObject); // Run callback and pass ServerDataObject as a parameter
                }
            }
        }
    }

    #endregion

    #region Other

    public void AddServerDataCallback(ServerDataCallback _callback, int _serverDataObjectId) {
        // Add new serverDataCallback delegate to list of callbacks at proper position with id specified
        serverDataCallbacks.Add(new KeyValuePair<int, ServerDataCallback>(_serverDataObjectId, _callback));
    }

    public void RemoveServerDataCallback(ServerDataCallback _callback, int _serverDataObjectId) {
        // Remove callback in list of callbacks by value
        serverDataCallbacks.Remove(new KeyValuePair<int, ServerDataCallback>(_serverDataObjectId, _callback));
    }

    public void AddClientConnectedCallback(ClientConnectedCallback _callback) {
        clientConnectedCallbacks.Add(_callback);
    }

    public void RemoveClientConnectedCallback(ClientConnectedCallback _callback) {
        clientConnectedCallbacks.Remove(_callback);
    }

    public void AddClientDisconnectedCallback(ClientDisconnectedCallback _callback) {
        clientDisconnectedCallbacks.Add(_callback);
    }

    public void RemoveClientDisconnectedCallback(ClientDisconnectedCallback _callback) {
        clientDisconnectedCallbacks.Remove(_callback);
    }

    public void CallClientConnectedCallbacks(int _clientId) {
        for (int i = 0; i < clientConnectedCallbacks.Count; i++) {
            clientConnectedCallbacks[i](_clientId);
        }
    }

    public void CallClientDisconnectedCallbacks(int _clientId) {
        for (int i = 0; i < clientDisconnectedCallbacks.Count; i++) {
            clientDisconnectedCallbacks[i](_clientId);
        }
    }

    // This is called when a client sends the welcome received packet
    public void SendClientObjectsOnClient(int _clientId) {
        foreach (ClientObject clientObject in clientObjects.Values) {
            ServerSend.ClientObjectNew(_clientId, clientObject.prefabIndex, clientObject.objectId); // Create new client object on client
            ServerSend.ClientObjectUpdate(_clientId, clientObject.objectId, clientObject.transform.position, clientObject.transform.rotation, clientObject.transform.localScale); // Set position, etc. for new client object on client
        }

        // Send clients connected
        for (int i = 1; i < Server.clients.Count; i++) {
            if (Server.clients[i].isConnected && i != _clientId) {
                NetworkManager.SendClientDataObject(_clientId, new ClientDataObject(6, new List<object>() { i, true })); // Send client connect data packet
            }
        }
    }

    public void ResetClientObjects() {
        // Loop through all clientObjects and delete them, must have .ToList() to prevent error 'IncalidOperationExpception'
        foreach (ClientObject clientObject in clientObjects.Values.ToList()) {
            Destroy(clientObject.gameObject);
            NetworkManager.ClientObjectDelete(-1, clientObject.objectId);
        }
        clientObjects.Clear();
    }

    #endregion
}
