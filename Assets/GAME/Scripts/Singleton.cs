using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
    public static T _instance;

    public static T Instance => _instance;
    private void Awake() {
        if (_instance == null) {
            _instance = GetComponent<T>();
            return;
        }

        if (_instance.gameObject.GetInstanceID() != this.gameObject.GetInstanceID()) {
            Destroy(gameObject);
        }
    }
}