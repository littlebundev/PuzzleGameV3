using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManager : MonoBehaviour {

	[SerializeField]
	Material[] pieceMaterials;
	[SerializeField]
	Material[] glowOnMaterials;
	[SerializeField]
	Material[] glowOffMaterials;

	private int packIndex = 0;

	private void Awake() {
		//DontDestroyOnLoad(gameObject);
	}

	public void SetPackIndex(Pack.PackID packId) {
		packIndex = FindObjectOfType<LevelHandler>().GetPackIndex(packId);
	}

	public Material[] GetMaterials() {
		return new Material[] { pieceMaterials[packIndex], glowOnMaterials[packIndex], glowOffMaterials[packIndex] };
	}
	public Color GetPrimaryColor() {
		return pieceMaterials[packIndex].color;
	}
}
