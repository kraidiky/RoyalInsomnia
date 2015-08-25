using UnityEngine;

public class RotateButton : MonoBehaviour {
	// Use this for initialization
	void OnMouseUpAsButton() {
        GameObject.FindObjectOfType<MapControler>().CurrentTetrisBlock.RotateElements();
	}
}
