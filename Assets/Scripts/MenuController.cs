using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {

	private LevelHandler levelHandler;


	private void Start() {
		levelHandler = FindObjectOfType<LevelHandler>();
	}

	public void SelectLevel(string levelNumber) {
		Debug.Log("SelectLevel " + levelNumber);
		if (levelHandler.SetCurrentLevel(int.Parse(levelNumber) - 1)) {
			Debug.Log("true");
			SceneManager.LoadScene("Game");
		}
	}
}
