using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LevelButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler {

	[SerializeField]
	Image iconImage;
	[SerializeField]
	Text levelText;
	[SerializeField]
	GameObject[] starFills;
	[SerializeField]
	Image[] starBorders;

	[SerializeField]
	Color color;
	[SerializeField]
	float dimmedAlpha;

	int levelNumber;


	//public void Click() {
	//	FindObjectOfType<MenuController>().LevelButtonClick(levelNumber);
	//}

	public void Init(int levelNumber, int levelNumberText, int stars, bool unlocked, Color color) {
		this.levelNumber = levelNumber;
		levelText.text = (levelNumberText + 1).ToString();
		iconImage.color = color;
		foreach (Image image in starBorders) {
			image.color = color;
		}
		if (unlocked) {
			for (int i = 1; i <= stars; i++) {
				starFills[i - 1].SetActive(true);
				starFills[i - 1].GetComponent<Image>().color = color;
			}
		} else {
			Color dimmedColor = color;
			dimmedColor.a = dimmedAlpha;
			iconImage.color = dimmedColor;
			foreach (GameObject star in starFills) {
				star.GetComponent<Image>().color = dimmedColor;
			}
			for (int i = 0; i < 3; i++) {
				starBorders[i].color = dimmedColor;
			}
		}
	}


	public void OnPointerDown(PointerEventData eventData) { }
	public void OnPointerUp(PointerEventData eventData) { }
	public void OnPointerClick(PointerEventData eventData) {
		FindObjectOfType<MenuController>().LevelButtonClick(levelNumber);
	}
}