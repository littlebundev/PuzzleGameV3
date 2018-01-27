using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour {

	[SerializeField]
	Image iconImage;
	[SerializeField]
	Text levelText;
	[SerializeField]
	GameObject[] starFills;

	[SerializeField]
	Color color;
	[SerializeField]
	float dimmedAlpha;


	public void Click() {
		FindObjectOfType<MenuController>().LevelButtonClick(int.Parse(levelText.text) - 1);
	}

	public void Init(int levelNumber, int stars, bool unlocked) {
		levelText.text = (levelNumber + 1).ToString();
		if (unlocked) {
			for (int i = 1; i <= stars; i++) {
				starFills[i - 1].SetActive(true);
			}
		} else {
			Color dimmedColor = color;
			dimmedColor.a = dimmedAlpha;
			//levelText.color = dimmedColor;
			iconImage.color = dimmedColor;
			foreach (GameObject star in starFills) {
				star.GetComponent<Image>().color = dimmedColor;
			}
		}
	}
}