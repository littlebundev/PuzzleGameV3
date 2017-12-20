using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GamePiece : MonoBehaviour {

	public enum PieceType {
		NULL,
		TOGGLE,
		ROTATION,
		CUBE,
		NUMBER,
		START,
		END
	}

	public PieceType pieceType;
	[SerializeField]
	Level.GP_LUT gpLU;

	public int XPos { get; set; }
	public int ZPos { get; set; }

	public bool pieceAnimating;
	private bool isGlowing;

	[Header("GamePiece Options")]
	[SerializeField]
	protected GameObject model;
	[SerializeField]
	GameObject glowObject;
	[SerializeField]
	Material[] materials;
	[SerializeField]
	int prefabIndex;



	public abstract void Clicked();

	public abstract bool[] CheckConnections();

	public int GetPrefabIndex() {
		return prefabIndex;
	}

	public Level.GP_LUT GetGPLU() {
		return gpLU;
	}

	public void InWinPath(bool inWinPath) {
		if (glowObject != null) {
			if (inWinPath) {
				StartCoroutine(Util.ScaleObject(model.gameObject, new Vector3(1, 1, 1), .15f));
				glowObject.GetComponent<Renderer>().sharedMaterial = materials[0];
				isGlowing = true;
			} else {
				StartCoroutine(Util.ScaleObject(model.gameObject, new Vector3(.85f, .85f, .85f), .15f));
				glowObject.GetComponent<Renderer>().sharedMaterial = materials[1];
				isGlowing = false;
			}
		}
	}
	public bool IsGlowing() {
		return isGlowing;
	}
}
