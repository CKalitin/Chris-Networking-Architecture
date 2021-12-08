using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientObject : MonoBehaviour {
    public int prefabIndex;
    [Space]
    public int objectId;

    Vector3 previousPos;
    Quaternion previousRot;
    Vector3 previousScale;

    private void Awake() {
        NetworkManager.ClientObjectNew(-1, this);
    }

    private void FixedUpdate() {
        if (transform.position != previousPos || transform.rotation != previousRot || transform.localScale != previousScale) {
            NetworkManager.ClientObjectUpdate(-1, objectId, transform.position, transform.rotation, transform.localScale);
        }

        previousPos = transform.position;
        previousRot = transform.rotation;
        previousScale = transform.localScale;
    }

    private void OnDestroy() {
        NetworkManager.ClientObjectDelete(-1, objectId);
    }
}
