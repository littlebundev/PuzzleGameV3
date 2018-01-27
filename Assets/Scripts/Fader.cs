using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Fader : MonoBehaviour {

	Image fadeImage;

	[SerializeField]
	float fadeTime;

	// Use this for initialization
	void Awake () {
		fadeImage = GetComponent<Image>();
	}
	
	public IEnumerator FadeIn() {
		yield return StartCoroutine(Util.FadeImageAlpha(fadeImage, 0f, fadeTime));
	}

	public IEnumerator FadeOut() {
		yield return StartCoroutine(Util.FadeImageAlpha(fadeImage, 1f, fadeTime));
	}

	
}
