using UnityEngine;
using System.Collections;

public class SelectDemoType : MonoBehaviour {

	void OnGUI () {
        if (GUI.Button(new Rect(5, 5, 250, 60), "15 Gameplay Demo"))
            Application.LoadLevel("Gameplay15Test");
        if (GUI.Button(new Rect(5, 70, 250, 60), "Tetris Gameplay Demo"))
            Application.LoadLevel("GamePlayTest");
	}
}
