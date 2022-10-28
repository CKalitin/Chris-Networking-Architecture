using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientObject : MonoBehaviour {
    [SerializeField] bool active = true;

    private float lerpTime = 0.016666f;

    private Vector3 targetPos;
    private float currentLerp;

    void Update() {
        currentLerp += Time.deltaTime / lerpTime; // Add to total lerp with deltaTime / total time to get to target pos
        transform.position = Vector3.Lerp(transform.position, targetPos, currentLerp); // Set pos by lerping
    }

    public void SetTargetPos(Vector3 _targetPos) {
        if (!active) {
            transform.position = _targetPos;
        }
        targetPos = _targetPos; // Set target position
        currentLerp = 0; // Set currentLerp amount to 0
    }
}
