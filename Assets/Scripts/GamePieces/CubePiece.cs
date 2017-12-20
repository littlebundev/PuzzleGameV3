using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubePiece : GamePiece {

	public enum Direction { NORTH, EAST, SOUTH, WEST }

	// Orientation of correct sides
	public enum Orientation {
		TOP_BOTTOM, NORTH_SOUTH, EAST_WEST
	}
	public Orientation currentOrientation;

	[SerializeField]
	float activateMvtAmt;
	[SerializeField]
	float activationTime;
	[SerializeField]
	float rotationTime;
	[SerializeField]
	GameObject directionButtonGroup;


	private void Start() {
		directionButtonGroup.SetActive(false);
	}


	// Piece clicked, start activation and prepare states
	public override void Clicked() {
		if (GameController.GetInstance().CurrentState == GameController.State.READY) {
			GameController.GetInstance().ActivePiece = this;
			GameController.GetInstance().NextState = GameController.State.PIECE_ACTIVATED;
			StartCoroutine(Activate(true));
		} else if (GameController.GetInstance().CurrentState == GameController.State.PIECE_ACTIVATED) {
			GameController.GetInstance().ActivePiece = null;
			StartCoroutine(Activate(false));
		}
	}


	public IEnumerator Activate(bool activated) {
		pieceAnimating = true;
		GameController.GetInstance().AnimateStart();
		if (activated) {
			yield return StartCoroutine(Util.TranslateObject(gameObject, Vector3.up, activateMvtAmt, activationTime));
			directionButtonGroup.SetActive(true);
		} else {
			directionButtonGroup.SetActive(false);
			yield return StartCoroutine(Util.TranslateObject(gameObject, Vector3.up, -activateMvtAmt, activationTime));
		}
		pieceAnimating = false;
		GameController.GetInstance().AnimateEnd();
	}


	public void Turn(Direction direction, bool mainAction) {
		StartCoroutine(Rotate(direction, mainAction));

	}
	public IEnumerator Rotate(Direction direction, bool mainAction) {
		
		//if (!mainAction)
		//	pieceAnimating = true;
		//GameController.GetInstance().AnimateStart();
		//GameController.GetInstance().PieceActionStart();
		if (mainAction) {
			GameController.GetInstance().AnimateStart(5);
			GameController.GetInstance().PieceAction(XPos, ZPos, true, direction);
		}
		directionButtonGroup.SetActive(false);
		switch (direction) {
			case Direction.NORTH:
				yield return StartCoroutine(Util.RotateObjectAround(model.gameObject, gameObject.transform.right, 90, rotationTime));
				ChangeOrientation(Orientation.TOP_BOTTOM, Orientation.NORTH_SOUTH);
				break;
			case Direction.SOUTH:
				yield return StartCoroutine(Util.RotateObjectAround(model.gameObject, gameObject.transform.right, -90, rotationTime));
				ChangeOrientation(Orientation.TOP_BOTTOM, Orientation.NORTH_SOUTH);
				break;
			case Direction.EAST:
				yield return StartCoroutine(Util.RotateObjectAround(model.gameObject, gameObject.transform.forward, -90, rotationTime));
				ChangeOrientation(Orientation.TOP_BOTTOM, Orientation.EAST_WEST);
				break;
			case Direction.WEST:
				yield return StartCoroutine(Util.RotateObjectAround(model.gameObject, gameObject.transform.forward, 90, rotationTime));
				ChangeOrientation(Orientation.TOP_BOTTOM, Orientation.EAST_WEST);
				break;
		}
		
		if (mainAction) {
			yield return StartCoroutine(Activate(false));
		} else
			pieceAnimating = false;
		GameController.GetInstance().PieceActionFinished();
		GameController.GetInstance().AnimateEnd();
	}
	private void ChangeOrientation(Orientation orientation1, Orientation orientation2) {
		if (currentOrientation == orientation1) currentOrientation = orientation2;
		else if (currentOrientation == orientation2) currentOrientation = orientation1;
	}


	public override bool[] CheckConnections() {
		if (pieceAnimating) return new bool[4];
		else if (currentOrientation == Orientation.TOP_BOTTOM) return new bool[4] { true, true, true, true };
		//else if (currentOrientation == Orientation.EAST_WEST) return new bool[4] { true, false, true, false };
		//else return new bool[4] { false, true, false, true };
		else return new bool[4];
	}


	//public override void InWinPath(bool inWinPath) {
	//}
}
