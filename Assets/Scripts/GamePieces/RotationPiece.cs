using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationPiece : GamePiece {

	public enum RotationPieceType {
		STRAIGHT,
		ANGLED
	}
	public RotationPieceType rotationPieceType;

	public enum Orientation {
		NORTH,
		EAST,
		SOUTH,
		WEST
	}
	[Header("RotationPiece Options")]
	public Orientation currentOrientation;

	bool[] connections;

	[SerializeField]
	float rotationTime;


	public override void Clicked() {
		StartCoroutine(Rotate(true));
	}

	public IEnumerator Rotate(bool mainAction) {
		//pieceAnimating = true;
		//InWinPath(false);
		//GameController.GetInstance().AnimateStart();
		//GameController.GetInstance().PieceActionStart();
		if (mainAction) {
			GameController.GetInstance().AnimateStart(5);
			GameController.GetInstance().PieceAction(XPos, ZPos);
		}
		if (currentOrientation < Orientation.WEST)
			currentOrientation++;
		else
			currentOrientation = Orientation.NORTH;
		yield return Util.RotateObjectAround(model.gameObject, gameObject.transform.up, 90, rotationTime);
		pieceAnimating = false;
		//GameController.GetInstance().PieceActionFinished();
		Debug.Log("finished rotate");
		GameController.GetInstance().AnimateEnd();
	}


	public override bool[] CheckConnections() {
		if (pieceAnimating) return new bool[4];
		else if (rotationPieceType == RotationPieceType.STRAIGHT) {
			if (currentOrientation == Orientation.NORTH || currentOrientation == Orientation.SOUTH)
				return new bool[4] { true, false, true, false };
			else return new bool[4] { false, true, false, true };
		} else {
			switch (currentOrientation) {
				case Orientation.NORTH:
					return new bool[4] { true, true, false, false };
				case Orientation.EAST:
					return new bool[4] { false, true, true, false };
				case Orientation.SOUTH:
					return new bool[4] { false, false, true, true };
				case Orientation.WEST:
					return new bool[4] { true, false, false, true };
				default:
					return new bool[4];
			}
		}
	}


	//public override void InWinPath(bool inWinPath) {
	//	if (inWinPath) {
	//		glowObject.GetComponent<Renderer>().material = glowObject.GetComponent<Renderer>().sharedMaterials[1];
	//	} else {
	//		glowObject.GetComponent<Renderer>().material = glowObject.GetComponent<Renderer>().sharedMaterials[0];
	//	}
	//}
}
