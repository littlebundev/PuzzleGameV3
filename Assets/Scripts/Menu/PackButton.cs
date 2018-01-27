﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PackButton : MonoBehaviour {

	[SerializeField]
	Text packText;

	[SerializeField]
	Image centerIconImage;
	[SerializeField]
	Image outerIconImage;
	[SerializeField]
	GameObject iconObject;
	[SerializeField]
	Image lockImage;
	[SerializeField]
	GameObject highlight;
	[SerializeField]
	Color color;
	[SerializeField]
	float dimmedAlpha;

	[SerializeField]
	float animationTime;

	private bool inputAllowed;
	private Pack.PackID packId;
	private bool unlocked;


	private void Start() {
		inputAllowed = true;
	}


	public void Click() {
		if (inputAllowed && unlocked) {
			StartCoroutine(ActivateAnimation());
			FindObjectOfType<MenuController>().PackButtonClick(packId);
		}
	}

	private IEnumerator ActivateAnimation() {
		inputAllowed = false;
		ResetHighlights();
		SetHighlight(true);
		StartCoroutine(Util.RotateObjectAround(iconObject, Vector3.back, 90f, 2 * animationTime));
		yield return Util.ScaleObject(centerIconImage.gameObject, new Vector3(2, 2, 1), animationTime);
		yield return Util.ScaleObject(centerIconImage.gameObject, new Vector3(1, 1, 1), animationTime);
		inputAllowed = true;
	}

	
	public void Init(Pack pack, bool unlocked) {
		packId = pack.packId;
		packText.text = pack.title;
		if (!unlocked) {
			Color dimmedColor = color;
			dimmedColor.a = dimmedAlpha;
			packText.color = dimmedColor;
			outerIconImage.color = dimmedColor;
			centerIconImage.color = dimmedColor;
		} else {
			lockImage.gameObject.SetActive(false);
		}
		this.unlocked = unlocked;
	}
	public void SetHighlight(bool isHighlighted) {
		highlight.SetActive(isHighlighted);
	}
	public void ResetHighlights() {
		PackButton[] packButtons = FindObjectsOfType<PackButton>();
		foreach(PackButton packButton in packButtons) {
			packButton.SetHighlight(false);
		}
	}
}