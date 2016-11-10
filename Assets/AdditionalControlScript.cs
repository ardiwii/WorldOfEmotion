using UnityEngine;
using System.Collections;

public class AdditionalControlScript : MonoBehaviour {

	Light flashlight;
	public GameObject MainCanvas;
	public GameObject inGameCanvas;
	public GameObject mainCamera;

	void Start(){
		flashlight = GetComponent<Light> ();
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			if (flashlight.enabled) {
				flashlight.enabled = false;
			} else {
				flashlight.enabled = true;
			}
		}
		if (Input.GetKeyDown (KeyCode.P)) {
			transform.parent.parent.gameObject.SetActive(false);
			MainCanvas.SetActive(true);
			inGameCanvas.SetActive (false);
			mainCamera.SetActive (true);
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
			Cursor.lockState = CursorLockMode.Confined;
		}
	}
}
