using UnityEngine;
using System.Collections;

public class UserSettings : MonoBehaviour {
		public string DatasetPath;

    void Awake() {
        DontDestroyOnLoad(transform.gameObject);
    }
}
