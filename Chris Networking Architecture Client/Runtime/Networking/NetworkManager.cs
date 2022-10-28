using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkManager : MonoBehaviour {
    #region Variables

    public static NetworkManager instance;

    [Header("Game Management")]
    public static Dictionary<int, GameObject> clientObjects = new Dictionary<int, GameObject>();
    [SerializeField] List<GameObject> clientObjectPrefabs = new List<GameObject>();

    public delegate void ClientDataCallback(ClientDataObject _clientDataObject); // Create a delegate for callback functions
    public static List<KeyValuePair<int, ClientDataCallback>> clientDataCallbacks = new List<KeyValuePair<int, ClientDataCallback>>(); // List of ClientDataCallbacks, each ClientDataCallback is in one list inside another, so it has 2 indexes

    public delegate void ConnectedToServerCallback(); // Create a delegate for callback functions
    public static List<ConnectedToServerCallback> connectedToServerCallbacks = new List<ConnectedToServerCallback>(); // List of callbacks

    public delegate void DisconnectedToServerCallback(); // Create a delegate for callback functions
    public static List<DisconnectedToServerCallback> disconnectedToServerCallbacks = new List<DisconnectedToServerCallback>(); // List of callbacks

    [Header("Other")]
    [SerializeField] private Camera cam;
    public Camera Cam { get { return cam; } set { cam = value; } }

    private bool sendInput = true;
    public bool SendInput { get { return sendInput; } set { sendInput = value; } }

    #region Data Packets

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
        // Singleton loop
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Update() {
        if (sendInput) {
            SendTCPInput();
            SendUDPInput();
        }
    }

    #endregion

    #region Sending Data

    // Send TCP Input
    private void SendTCPInput() {
        // Initialize lists of keys down and up
        List<int> keysDown = new List<int>();
        List<int> keysUp = new List<int>();

        // Loop through every single key (This is terrible)
        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode))) {
            if (Input.GetKeyDown(key)) {
                keysDown.Add((int)key);
            }
            if (Input.GetKeyUp(key)) {
                keysUp.Add((int)key);
            }
        }

        if (keysDown.Count > 0 || keysUp.Count > 0) {
            ClientSend.TCPInput(keysDown, keysUp);
        }
    }

    // Send UDP Input
    private void SendUDPInput() {
        Vector2 _mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        ClientSend.UDPInput(_mousePos);
    }

    public static void SendServerDataObject(ServerDataObject _serverDataObject) {
        ClientSend.ServerDataObject(_serverDataObject);
    }

    #endregion

    #region Handing Data

    // Server sends this packet when player connects
    public void Welcome() {
        // Loop through connected to server callbacks
        for (int i = 0; i < connectedToServerCallbacks.Count; i++) {
            connectedToServerCallbacks[i](); // Call function
        }
    }

    // Receive Client Object vars and apply them
    public void ClientObjectUpdate(int _objectId, Vector3 _pos, Quaternion _rot, Vector3 _scale) {
        if (clientObjects.ContainsKey(_objectId)) {
            //clientObjects[_objectId].transform.position = _pos;
            clientObjects[_objectId].transform.rotation = _rot;
            clientObjects[_objectId].transform.localScale = _scale;
            clientObjects[_objectId].GetComponent<ClientObject>().SetTargetPos(_pos);
        }
    }

    // Add ClientObject no Color
    public void ClientObjectNew(int _prefabIndex, int _index) {
        // If client object at _index already exists, delete and remove it
        if (clientObjects.ContainsKey(_index)) {
            Destroy(clientObjects[_index]);
            clientObjects.Remove(_index);
        }
        clientObjects.Add(_index, Instantiate(clientObjectPrefabs[_prefabIndex], new Vector3(0, -10000, 0), Quaternion.identity));
    }
    
    // Remove Client Object
    public void ClientObjectDelete(int _index) {
        if (clientObjects.ContainsKey(_index)) {
            Destroy(clientObjects[_index]);
            clientObjects.Remove(_index);
        }
    }

    public void ClientDataObject(ClientDataObject _clientDataObject) {
        // Loop through dictionary of clientDataCallbacks
        if (clientDataCallbacks.Count > 0) {
            // Use .ToList() to copy the callbacks into another list so you don't get 'Collection was modified' error
            foreach (var clientDataCallback in clientDataCallbacks.ToList()) {
                if (clientDataCallback.Key == _clientDataObject.Id) { // If ClientDataObject id of callback is equal to CDP of this scriptable object
                    clientDataCallback.Value(_clientDataObject); // Run callback and pass ClientDataObject as a parameter
                }
            }
        }
    }

    #endregion

    #region Other

    public void AddClientDataCallback(ClientDataCallback _callback, int _clientDataObjectId) {
        clientDataCallbacks.Add(new KeyValuePair<int, ClientDataCallback>(_clientDataObjectId, _callback)); // Add new clientDataCallback delegate to list of callbacks at proper position with id specified
    }

    public void RemoveClientDataCallback(ClientDataCallback _callback, int _clientDataObjectId) {
        clientDataCallbacks.Remove(new KeyValuePair<int, ClientDataCallback>(_clientDataObjectId, _callback)); // Remove key value pair of values
    }

    public void AddConnectedToServerCallback(ConnectedToServerCallback _callback) {
        connectedToServerCallbacks.Add(_callback);
    }

    public void RemoveConnectedToServerCallback(ConnectedToServerCallback _callback) {
        connectedToServerCallbacks.Remove(_callback);
    }

    public void AddDisconnectedToServerCallback(DisconnectedToServerCallback _callback) {
        disconnectedToServerCallbacks.Add(_callback);
    }

    public void RemoveDisconnectedToServerCallback(DisconnectedToServerCallback _callback) {
        disconnectedToServerCallbacks.Remove(_callback);
    }

    public void ResetClientObjects() {
        foreach (GameObject clientObject in clientObjects.Values) {
            Destroy(clientObject);
        }
        clientObjects.Clear();
    }

    public void ResetCallbacks() {
        clientDataCallbacks.Clear();
        connectedToServerCallbacks.Clear();
        disconnectedToServerCallbacks.Clear();
    }

    public void CallDisconnectedFromServerCallbacks() {
        // Loop through disconnected to server callbacks
        for (int i = 0; i < disconnectedToServerCallbacks.Count; i++) {
            disconnectedToServerCallbacks[i](); // Call function
        }
    }

    #endregion
}
