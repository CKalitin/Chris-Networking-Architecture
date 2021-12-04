using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientDataPacket {
    // List of ids for client data packets (this is for receiving them the same way regular packets work, see Packet.cs)
    public enum ClientDataPacketTypes {
        test = 1, // Int
    }

    //Constructor to set id of ClientDataPacket
    public ClientDataPacket(int _id) {
        vars.Add(_id); // Add _id object to list of vars
    }

    // This is a list of objects so any element can be any data type
    List<object> vars = new List<object>();
    public List<object> Vars { get { return vars; } set { vars = value; } }

    #region Write

    // Write functions, these create new PacketDataTypes and put them in the list of vars to send

    public void Write(byte _byte) {
        vars.Add(_byte); // Add paramater to list of object vars
    }

    public void Write(byte[] _bytes) {
        vars.Add(_bytes); // Add paramater to list of object vars
    }

    public void Write(short _short) {
        vars.Add(_short); // Add paramater to list of object vars
    }

    public void Write(int _int) {
        vars.Add(_int); // Add paramater to list of object vars
    }

    public void Write(long _long) {
        vars.Add(_long); // Add paramater to list of object vars
    }

    public void Write(float _float) {
        vars.Add(_float); // Add paramater to list of object vars
    }

    public void Write(bool _bool) {
        vars.Add(_bool); // Add paramater to list of object vars
    }

    public void Write(string _string) {
        vars.Add(_string); // Add paramater to list of object vars
    }

    public void Write(Vector2 _vector2) {
        vars.Add(_vector2); // Add paramater to list of object vars
    }

    public void Write(Vector3 _vector3) {
        vars.Add(_vector3); // Add paramater to list of object vars
    }

    public void Write(Quaternion _quaternion) {
        vars.Add(_quaternion); // Add paramater to list of object vars
    }

    #endregion
}
