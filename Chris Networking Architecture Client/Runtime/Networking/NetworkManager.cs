using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour {
    #region Variables

    public static NetworkManager instance;

    [Header("Game Management")]
    public static List<GameObject> clientObjects = new List<GameObject>();
    [SerializeField] List<GameObject> clientObjectPrefabs = new List<GameObject>();

    public delegate void ClientDataCallback(ClientDataObject _clientDataObject); // Create a delegate for callback functions
    public static List<KeyValuePair<int, ClientDataCallback>> clientDataCallbacks = new List<KeyValuePair<int, ClientDataCallback>>(); // 2D List of ClientDataCallbacks, each ClientDataCallback is in one list inside another, so it has 2 indexes

    [Header("Other")]
    [SerializeField] private Camera cam;
    public Camera Cam { get { return cam; } set { cam = value; } }

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
        SendTCPInput();
        SendUDPInput();
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

    // Receive Client Object vars and apply them
    public void ClientObjectUpdate(int _objectId, Vector3 _pos, Quaternion _rot, Vector3 _scale) {
        if (_objectId < clientObjects.Count) {
            clientObjects[_objectId].transform.position = _pos;
            clientObjects[_objectId].transform.rotation = _rot;
            clientObjects[_objectId].transform.localScale = _scale;
        }
    }

    // Add ClientObject no Color
    public void ClientObjectNew(int _prefabIndex) {
        clientObjects.Add(Instantiate(clientObjectPrefabs[_prefabIndex], new Vector3(0, -10000, 0), Quaternion.identity));
    }
    
    // Remove Client Object
    public void ClientObjectDelete(int _index) {
        if (clientObjects[_index]) {
            Destroy(clientObjects[_index]);
            clientObjects.RemoveAt(_index);
        } else {
            Debug.LogWarning("No Client Object to delete at index: " + _index);
        }
    }

    public void ClientDataObject(ClientDataObject _clientDataObject) {
        // Loop through dictionary of clientDataCallbacks
        foreach (var clientDataCallback in clientDataCallbacks) {
            if (clientDataCallback.Key ==_clientDataObject.Id) { // If ClientDataObject id of callback is equal to CDP of this scriptable object
                clientDataCallback.Value(_clientDataObject); // Run callback and pass ClientDataObject as a parameter
            }
        }
    }

    #endregion

    #region Other

    public void AddClientDataCallback(ClientDataCallback _callback, int _clientDataObjectId) {
        clientDataCallbacks.Add(new KeyValuePair<int, ClientDataCallback>(_clientDataObjectId, _callback)); // Add new clientDataCallback delegate to list of callbacks at proper position with id specified
    }

    #endregion
}
