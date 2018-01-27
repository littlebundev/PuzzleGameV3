using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour {

	public static readonly int BOARD_SIZE_X = 9;
	public static readonly int BOARD_SIZE_Z = 5;
	// XPos, ZPos, corresponding connection index
	public static readonly int[,] CON_MATRIX = { { 0, 1, 2 }, { 1, 0, 3 }, { 0, -1, 0 }, { -1, 0, 1 } };

	private GamePiece[,] gamePieces;
	private StartPiece startPiece;
	private List<EndPiece> endPieceList = new List<EndPiece>();
	private int endCount = 0;

	[Header("Prefabs")]
	[SerializeField]
	GamePiece nullPrefab;
	[SerializeField]
	CubePiece[] cubePrefabs;
	[SerializeField]
	RotationPiece[] rotationAngledPrefabs;
	[SerializeField]
	RotationPiece[] rotationStraightPrefabs;
	[SerializeField]
	TogglePiece[] togglePrefabs;
	[SerializeField]
	StartPiece startPiecePrefab;
	[SerializeField]
	EndPiece endPiecePrefab;

	[Header("Placement")]
	[SerializeField]
	float adjacentSpacing = 1.25f;
	[SerializeField]
	float xStartPos = -6f;
	[SerializeField]
	float zStartPos = -4f;

	[Header("Animation")]
	[SerializeField]
	float adjacentAnimWait;
	[SerializeField]
	float connectedAnimWait;
	bool[,] prevConnections = new bool[BOARD_SIZE_X, BOARD_SIZE_Z];



	private void Start() {
		//FindObjectOfType<Camera>().transform.LookAt(Vector3.zero);
		//SetStartPositions();
		//InitGamePieces(GameController.GetInstance().GetCurrentLevel());
		//StartCoroutine(UpdatePathsAndCheckForWin());
#if UNITY_EDITOR
		LEGameBoard leGameBoard = FindObjectOfType<LEGameBoard>();
		if (leGameBoard != null) leGameBoard.Populate();
#endif
	}
	private void SetStartPositions() {
		float xAdjustment = BOARD_SIZE_X;
		if (xAdjustment % 2 != 0) xAdjustment -= 1;
		float zAdjustment = BOARD_SIZE_Z;
		if (zAdjustment % 2 != 0) zAdjustment -= 1;
		xStartPos = -(xAdjustment * adjacentSpacing / 2f);
		zStartPos = -(zAdjustment * adjacentSpacing / 2f);
	}


	// Populate board from level data file
	public void InitGamePieces(Pack.Level level) {
		SetStartPositions();
		gamePieces = new GamePiece[BOARD_SIZE_X, BOARD_SIZE_Z];
		Pack.Level.GP_LUT[,] pieceLU;
		pieceLU = level.pieceLU;
		for (int x = 0; x < BOARD_SIZE_X; x++) {
			for (int z = 0; z < BOARD_SIZE_Z; z++) {
				gamePieces[x, z] = Instantiate(GamePieceFromLevelLUT(pieceLU[x, z]),
						new Vector3(xStartPos + adjacentSpacing * (float)x, 0, zStartPos + adjacentSpacing * (float)z),
						Quaternion.identity);
				if (gamePieces[x, z].pieceType == GamePiece.PieceType.START) startPiece = gamePieces[x, z] as StartPiece;
				if (gamePieces[x, z].pieceType == GamePiece.PieceType.END) endPieceList.Add(gamePieces[x, z] as EndPiece);
				gamePieces[x, z].XPos = x;
				gamePieces[x, z].ZPos = z;
			}
		}
		StartCoroutine(UpdatePathsAndCheckForWin());
	}
#if UNITY_EDITOR
	public void Clear() {
		for (int x = 0; x < BOARD_SIZE_X; x++) {
			for (int z = 0; z < BOARD_SIZE_Z; z++) {
				DestroyImmediate(gamePieces[x, z].gameObject);
			}
		}
		startPiece = null;
		endPieceList.Clear();
	}
#endif


	// Piece action was performed by player, cascade to adjacent pieces if needed
	public IEnumerator PieceAction(int x, int z, bool wasCubePiece, CubePiece.Direction direction) {
		// Perform action on adjacent GamePieces, N->E->S->W
		//Debug.Log("PieceAction " + x + "," + z);

#if UNITY_EDITOR
		if (GameController.GetInstance().editingLevel) {
			LERotatePiece(x, z, true, direction);
			if (wasCubePiece) {
				GameController.GetInstance().ActivePiece = gamePieces[x, z];
				StartCoroutine((gamePieces[x, z] as CubePiece).Activate(false));
			}
		}
#endif

		z = z + 1;
		for (int i = 0; i < 4; i++) {
			yield return new WaitForSeconds(adjacentAnimWait);
			// If piece exists, perform action, else decrease animation counter
			if (DoesPieceExist(x, z)) {
#if UNITY_EDITOR
				if (GameController.GetInstance().editingLevel)
					if (wasCubePiece) {
						LERotatePiece(x, z, true, direction);
					} else {
						switch (i) {
							case 0:
								LERotatePiece(x, z, true, CubePiece.Direction.NORTH);
								break;
							case 1:
								LERotatePiece(x, z, true, CubePiece.Direction.EAST);
								break;
							case 2:
								LERotatePiece(x, z, true, CubePiece.Direction.SOUTH);
								break;
							case 3:
								LERotatePiece(x, z, true, CubePiece.Direction.WEST);
								break;
						}
					}
#endif
				DetermineAction(x, z, direction);
			} else
				GameController.GetInstance().AnimateEnd();
			if (!wasCubePiece) direction++;

			if (i == 0) x += 1;
			else x -= 1;
			if (i == 2) z += 1;
			else z -= 1;
		}
	}
	// Check if x,z is on GameBoard and GamePiece exists at x,z
	public bool DoesPieceExist(int x, int z) {
		return x >= 0 && x < BOARD_SIZE_X &&
			   z >= 0 && z < BOARD_SIZE_Z &&
			   gamePieces[x, z] != null &&
			   gamePieces[x, z].pieceType != GamePiece.PieceType.NULL;
	}
	// Determine what type of GamePiece needs action and perform the action
	public void DetermineAction(int x, int z, CubePiece.Direction direction) {
		if (gamePieces[x, z].pieceType == GamePiece.PieceType.CUBE) {
			//if (!GameController.GetInstance().editingLevel) {
				StartCoroutine(((CubePiece)gamePieces[x, z]).Rotate(direction, false));
			//} else {
			//	Debug.Log("test");
			//	GameController.GetInstance().AnimateEnd();
			//}
		} else if (gamePieces[x, z].pieceType == GamePiece.PieceType.ROTATION) {
			if (!GameController.GetInstance().editingLevel) {
				StartCoroutine(((RotationPiece)gamePieces[x, z]).Rotate(false));
			} else {
				Debug.Log("test");
				GameController.GetInstance().AnimateEnd();
			}
		} else if (gamePieces[x, z].pieceType == GamePiece.PieceType.TOGGLE) {
			if (!GameController.GetInstance().editingLevel) {
				StartCoroutine(((TogglePiece)gamePieces[x, z]).Flip(false));
			} else {
				Debug.Log("test");
				GameController.GetInstance().AnimateEnd();
			}
		} else
			GameController.GetInstance().AnimateEnd();
	}


	public IEnumerator UpdatePathsAndCheckForWin(bool delayBtwPieces = true) {
		if (startPiece != null) {
			//GameController.GetInstance().AnimateStart();
			bool[,] checkedPieces = new bool[BOARD_SIZE_X, BOARD_SIZE_Z];
			//Debug.Log("StartPiece at " + startPiece.XPos + "," + startPiece.ZPos);
			yield return UpdatePathsRecursively(startPiece, checkedPieces, delayBtwPieces);
			for (int x = 0; x < BOARD_SIZE_X; x++) {
				for (int z = 0; z < BOARD_SIZE_Z; z++) {
					if (!checkedPieces[x, z])
						gamePieces[x, z].InWinPath(false);
				}
			}

			//GameController.GetInstance().AnimateEnd();
			if (!GameController.GetInstance().inMenu) {
				if (endCount == endPieceList.Count && GameController.GetInstance().CurrentState == GameController.State.READY) {
					GameController.GetInstance().Win();
				} else
					GameController.GetInstance().CheckForLoss();
			}
			endCount = 0;
		}
	}
	private IEnumerator UpdatePathsRecursively(GamePiece currentPiece, bool[,] checkedPieces, bool delayBtwPieces = true) {
		checkedPieces[currentPiece.XPos, currentPiece.ZPos] = true;
		bool alreadyGlowing = true;
		if (!currentPiece.IsGlowing()) {
			currentPiece.InWinPath(true);
			alreadyGlowing = false;
		}
		// End piece found
		if (currentPiece.pieceType == GamePiece.PieceType.END) {
			endCount++;
		}
		// Loop through sides of current piece
		for (int i = 0; i < 4; i++) {
			// Check if current side of current piece is open
			if (currentPiece.CheckConnections()[i]) {
				// Check if connection piece exists, hasn't yet been checked, and is open on corresponding side
				if (DoesPieceExist(currentPiece.XPos + CON_MATRIX[i, 0], currentPiece.ZPos + CON_MATRIX[i, 1])
					&& !checkedPieces[currentPiece.XPos + CON_MATRIX[i, 0], currentPiece.ZPos + CON_MATRIX[i, 1]]
					&& gamePieces[currentPiece.XPos + CON_MATRIX[i, 0], currentPiece.ZPos + CON_MATRIX[i, 1]].CheckConnections()[CON_MATRIX[i, 2]]) {
					// Continue to next piece
					//Debug.Log("Continuing to piece (" + (currentPiece.XPos + CON_MATRIX[i, 0]) + "," + (currentPiece.ZPos + CON_MATRIX[i, 1]) + ")");
					if (delayBtwPieces && !alreadyGlowing)
						yield return new WaitForSeconds(connectedAnimWait);
					yield return UpdatePathsRecursively(gamePieces[currentPiece.XPos + CON_MATRIX[i, 0], currentPiece.ZPos + CON_MATRIX[i, 1]], checkedPieces, delayBtwPieces);
				}
			}
		}
	}


	private GamePiece GamePieceFromLevelLUT(Pack.Level.GP_LUT gpLU) {
		if (gpLU == Pack.Level.GP_LUT.CUBE_TB) return cubePrefabs[0];
		else if (gpLU == Pack.Level.GP_LUT.CUBE_EW) return cubePrefabs[1];
		else if (gpLU == Pack.Level.GP_LUT.CUBE_NS) return cubePrefabs[2];
		else if (gpLU == Pack.Level.GP_LUT.ROT_ANGLED_NE) return rotationAngledPrefabs[0];
		else if (gpLU == Pack.Level.GP_LUT.ROT_ANGLED_ES) return rotationAngledPrefabs[1];
		else if (gpLU == Pack.Level.GP_LUT.ROT_ANGLED_SW) return rotationAngledPrefabs[2];
		else if (gpLU == Pack.Level.GP_LUT.ROT_ANGLED_WN) return rotationAngledPrefabs[3];
		else if (gpLU == Pack.Level.GP_LUT.ROT_STRAIGHT_NS) return rotationStraightPrefabs[0];
		else if (gpLU == Pack.Level.GP_LUT.ROT_STRAIGHT_EW) return rotationStraightPrefabs[1];
		else if (gpLU == Pack.Level.GP_LUT.TOGGLE_OFF) return togglePrefabs[0];
		else if (gpLU == Pack.Level.GP_LUT.TOGGLE_ON) return togglePrefabs[1];
		else if (gpLU == Pack.Level.GP_LUT.START) {
			return startPiecePrefab;
		} else if (gpLU == Pack.Level.GP_LUT.END) {
			return endPiecePrefab;
		} else return nullPrefab;
	}


	public GamePiece GetGamePiece(int x, int z) {
		return gamePieces[x, z];
	}


#if UNITY_EDITOR

	// Level Editor methods, not needed in android build-------------------------------------------------------

	public void LEChangePieceType(int x, int z) {
		switch (gamePieces[x, z].pieceType) {
			case GamePiece.PieceType.NULL:
				ReplaceGamePiece(x, z, togglePrefabs[0]);
				break;
			case GamePiece.PieceType.TOGGLE:
				ReplaceGamePiece(x, z, rotationAngledPrefabs[0]);
				break;
			case GamePiece.PieceType.ROTATION:
				if ((gamePieces[x, z] as RotationPiece).rotationPieceType == RotationPiece.RotationPieceType.ANGLED)
					ReplaceGamePiece(x, z, rotationStraightPrefabs[0]);
				else
					ReplaceGamePiece(x, z, cubePrefabs[0]);
				break;
			case GamePiece.PieceType.CUBE:
				ReplaceGamePiece(x, z, startPiecePrefab);
				break;
			case GamePiece.PieceType.START:
				ReplaceGamePiece(x, z, endPiecePrefab);
				break;
			case GamePiece.PieceType.END:
				ReplaceGamePiece(x, z, nullPrefab);
				break;
		}
		StartCoroutine(UpdatePathsAndCheckForWin());
	}
	public void LERotatePiece(int x, int z, bool mixing = false, CubePiece.Direction direction = CubePiece.Direction.NORTH) {
		int prefabIndex;
		switch (gamePieces[x, z].pieceType) {
			case GamePiece.PieceType.TOGGLE:
				if (gamePieces[x, z].GetPrefabIndex() == togglePrefabs.Length - 1) prefabIndex = 0;
				else prefabIndex = gamePieces[x, z].GetPrefabIndex() + 1;
				ReplaceGamePiece(x, z, togglePrefabs[prefabIndex]);
				break;
			case GamePiece.PieceType.ROTATION:
				if ((gamePieces[x, z] as RotationPiece).rotationPieceType == RotationPiece.RotationPieceType.ANGLED) {
					if (gamePieces[x, z].GetPrefabIndex() == rotationAngledPrefabs.Length - 1) prefabIndex = 0;
					else prefabIndex = gamePieces[x, z].GetPrefabIndex() + 1;
					ReplaceGamePiece(x, z, rotationAngledPrefabs[prefabIndex]);
				} else {
					if (gamePieces[x, z].GetPrefabIndex() == rotationStraightPrefabs.Length - 1) prefabIndex = 0;
					else prefabIndex = gamePieces[x, z].GetPrefabIndex() + 1;
					ReplaceGamePiece(x, z, rotationStraightPrefabs[prefabIndex]);
				}
				break;
			case GamePiece.PieceType.CUBE:
				if (!mixing) {
					if (gamePieces[x, z].GetPrefabIndex() == cubePrefabs.Length - 1) prefabIndex = 0;
					else prefabIndex = gamePieces[x, z].GetPrefabIndex() + 1;
					ReplaceGamePiece(x, z, cubePrefabs[prefabIndex]);
				} else {
					switch (gamePieces[x, z].GetPrefabIndex()) {
						case 0:
							switch (direction) {
								case CubePiece.Direction.NORTH:
								case CubePiece.Direction.SOUTH:
									ReplaceGamePiece(x, z, cubePrefabs[2]);
									break;
								case CubePiece.Direction.EAST:
								case CubePiece.Direction.WEST:
									ReplaceGamePiece(x, z, cubePrefabs[1]);
									break;
							}
							break;
						case 1:
							switch (direction) {
								case CubePiece.Direction.EAST:
								case CubePiece.Direction.WEST:
									ReplaceGamePiece(x, z, cubePrefabs[0]);
									break;
							}
							break;
						case 2:
							switch (direction) {
								case CubePiece.Direction.NORTH:
								case CubePiece.Direction.SOUTH:
									ReplaceGamePiece(x, z, cubePrefabs[0]);
									break;
							}
							break;
					}
				}
				break;
		}
		StartCoroutine(UpdatePathsAndCheckForWin());
	}
	private void ReplaceGamePiece(int x, int z, GamePiece newPiecePrefab) {
		GamePiece newPiece = Instantiate(newPiecePrefab, gamePieces[x, z].transform.position, Quaternion.identity);
		newPiece.XPos = gamePieces[x, z].XPos;
		newPiece.ZPos = gamePieces[x, z].ZPos;
		FindObjectOfType<LevelEditor>().SetGpLU(x, z, newPiece.GetGPLU());
		Destroy(gamePieces[x, z].gameObject);
		gamePieces[x, z] = newPiece;
	}

	
#endif
}

