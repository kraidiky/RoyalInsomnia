using UnityEngine;
using System.Collections;

public class RestartButton : MonoBehaviour {

	// Use this for initialization
	void OnMouseUp() {
        Debug.Log("Application.LoadLevel(0)");
        Application.LoadLevel(0);
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
