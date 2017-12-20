using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainController : MonoBehaviour {

	[SerializeField]
	string gameScene;

	void Start () {
		DontDestroyOnLoad(gameObject);
		SceneManager.LoadScene(gameScene);
	}
	
	
}
