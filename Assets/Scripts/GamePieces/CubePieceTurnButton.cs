using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubePieceTurnButton : MonoBehaviour {

	[SerializeField]
	CubePiece parentPiece;
	[SerializeField]
	CubePiece.Direction direction;

	public void Clicked() {
		parentPiece.Turn(direction, true);
		//StartCoroutine(parentPiece.Rotate(direction, true));
	}
}
