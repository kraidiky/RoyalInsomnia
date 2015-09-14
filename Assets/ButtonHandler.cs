using System;
using UnityEngine;
using System.Collections;

public class ButtonHandler : MonoBehaviour {
    public event Action Click;
	// Use this for initialization
	void OnMouseUpAsButton() {
        if (Click != null)
            Click();
	}
	
}
