using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class pauseMenu : MonoBehaviour {
	public Transform canvas;
	public Transform Player;
	public LevelLoader loader;


	// Update is called once per frame
	void Update () {
		if (!loader.loading)
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (canvas.gameObject.activeInHierarchy == false) {
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
					canvas.gameObject.SetActive(true);
					Player.GetComponent<FirstPersonController>().enabled = false;

				}
				else {
					Cursor.lockState = CursorLockMode.Confined;
					Cursor.visible = false;
					canvas.gameObject.SetActive(false);
					Player.GetComponent<FirstPersonController>().enabled = true;

				}
			}
		}
	}
}
