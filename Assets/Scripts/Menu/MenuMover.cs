using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuMover : MonoBehaviour {

	MenuController menuController;

	private void Start() {
		menuController = FindObjectOfType<MenuController>();
	}

	public void MoveToMainFinished() {
		menuController.MenuMoveFinished(true);
	}
	public void MoveLeftFinished() {
		menuController.MenuMoveFinished(false);
	}
}
