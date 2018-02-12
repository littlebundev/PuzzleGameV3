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
	Pack.Level.GP_LUT gpLU;

	public int XPos { get; set; }
	public int ZPos { get; set; }

	public bool pieceAnimating;
	protected bool isGlowing;

	[Header("GamePiece Options")]
	[SerializeField]
	protected GameObject model;
	[SerializeField]
	GameObject glowObject;
	[SerializeField]
	GameObject nonGlowObject;
	[SerializeField]
	Material[] materials;
	[SerializeField]
	int prefabIndex;



	public abstract void Clicked();

	public abstract bool[] CheckConnections();

	public int GetPrefabIndex() {
		return prefabIndex;
	}

	public Pack.Level.GP_LUT GetGPLU() {
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


	public virtual void InitMaterials(Material[] newMaterials) {
		if (pieceType == PieceType.TOGGLE || pieceType == PieceType.ROTATION || pieceType == PieceType.CUBE || pieceType == PieceType.END) {
			materials[0] = newMaterials[1];
			materials[1] = newMaterials[2];

			if (isGlowing) {
				glowObject.GetComponent<Renderer>().sharedMaterial = materials[0];
			} else {
				glowObject.GetComponent<Renderer>().sharedMaterial = materials[1];
			}

			if (pieceType != PieceType.END)
				nonGlowObject.GetComponent<Renderer>().sharedMaterial = newMaterials[0];
		}
	}

#if UNITY_EDITOR
	// Level Editor methods, not needed in android build-------------------------------------------------------

	//public void ClickedEditing() {
	//	GameController.GetInstance().ClickedEditing(XPos, ZPos);
	//}

#endif
}
