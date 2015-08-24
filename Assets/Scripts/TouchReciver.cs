using UnityEngine;
using System.Collections;

public class TouchReciver : MonoBehaviour {

    private bool _mouseDowned;
    private Vector3 _mousePosition;
    private Vector3 _mouseStartPosition;
    void OnMouseUp() {
        _mouseDowned = false;
        if ((_mouseStartPosition - Input.mousePosition).magnitude < 10) {
            GameObject.FindObjectOfType<MapControler>().ReciveMouseClick();
        }
	}
	
	void OnMouseDown() {
        _mouseDowned = true;
        _mouseStartPosition = _mousePosition = Input.mousePosition;
	}
    void Update() {
        if (_mouseDowned) {
            GameObject.FindObjectOfType<MapControler>().ReciveMouseMove(Input.mousePosition - _mousePosition);
            _mousePosition = Input.mousePosition;
        }
    }
}
