﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinPanel : MonoBehaviour {
	
	Animator animator;
	[SerializeField]
	Text pointsText;

	int stars;
	int points;

	[SerializeField]
	AudioClip starClip;

	
	void Start () {
		animator = GetComponent<Animator>();
	}


	public void Win(int stars, int points) {
		animator.SetTrigger("Enter");
		this.stars = stars;
		this.points = points;
	}
	public void EnterFinished() {
		StartCoroutine(ShowPoints());
		if (stars > 0) {
			animator.SetTrigger("Star1");
			SoundManager.instance.PlaySingle(starClip, false);
		}
	}
	public void Star1Finished() {
		if (stars > 1) {
			animator.SetTrigger("Star2");
			SoundManager.instance.PlaySingle(starClip, false, 1.03f);
		}
	}
	public void Star2Finished() {
		if (stars > 2) {
			animator.SetTrigger("Star3");
			SoundManager.instance.PlaySingle(starClip, false, 1.06f);
		}
	}
	private IEnumerator ShowPoints() {
		for (float t = 0; t < 1f; t += Time.deltaTime) {
			pointsText.text = "Points: +" + (int)Mathf.Lerp(0, points, t);
			yield return null;
		}
		pointsText.text = "Points: +" + points;
	}
}
