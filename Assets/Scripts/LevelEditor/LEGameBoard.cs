using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LEGameBoard : MonoBehaviour {

#if UNITY_EDITOR

	private LEPiece[,] lePieces;
	[SerializeField]
	LEPiece lePiecePrefab;
	[SerializeField]
	float adjacentSpacing = 1.25f;



	private void Start() {
		//lePieces = new LEPiece[GameBoard.BOARD_SIZE_X, GameBoard.BOARD_SIZE_Z];
		//Populate();
	}

	// Populate the GameBoard with GamePieces
	public void Populate() {
		lePieces = new LEPiece[GameBoard.BOARD_SIZE_X, GameBoard.BOARD_SIZE_Z];
		for (int x = 0; x < GameBoard.BOARD_SIZE_X; x++) {
			for (int z = 0; z < GameBoard.BOARD_SIZE_Z; z++) {
				GamePiece currentPiece = FindObjectOfType<GameBoard>().GetGamePiece(x, z);
				LEPiece newPiece = Instantiate(lePiecePrefab,
					currentPiece.transform.position,
					Quaternion.identity);
				newPiece.transform.parent = transform;
				lePieces[x, z] = newPiece;
				lePieces[x, z].XPos = x;
				lePieces[x, z].ZPos = z;
			}
		}
	}

#endif

}
