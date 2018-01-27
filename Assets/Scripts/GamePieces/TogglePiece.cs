using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TogglePiece : GamePiece {

	public bool toggled;

	[SerializeField]
	float flipTime;


	private void Start() {
	}


	public override void Clicked() {
		StartCoroutine(Flip(true));
	}

	public IEnumerator Flip(bool mainAction) {
		toggled = !toggled;
		//GameController.GetInstance().AnimateStart();
		if (mainAction) {
			GameController.GetInstance().AnimateStart(5);
			GameController.GetInstance().PieceAction(XPos, ZPos);
		}
		if (!GameController.GetInstance().editingLevel) {
			if (!toggled) {
				yield return Util.RotateObjectAround(model.gameObject, gameObject.transform.right, 90, 2f * flipTime / 3f);
				yield return Util.RotateObjectAround(model.gameObject, gameObject.transform.up, -45, flipTime / 3f);
			} else {
				yield return Util.RotateObjectAround(model.gameObject, gameObject.transform.up, 45, flipTime / 3f);
				yield return Util.RotateObjectAround(model.gameObject, gameObject.transform.right, -90, 2f * flipTime / 3f);
			}
		} else {
#if UNITY_EDITOR
			Debug.Log("Test mainAction toggle");

#endif
		}
		pieceAnimating = false;
		GameController.GetInstance().AnimateEnd();
	}


	public override bool[] CheckConnections() {
		if (pieceAnimating) return new bool[4];
		else if (toggled) return new bool[4] { true, true, true, true };
		else return new bool[4];
	}


	//public override void InWinPath(bool inWinPath) {
	//	winPathObj.SetActive(inWinPath);
	//}
}
