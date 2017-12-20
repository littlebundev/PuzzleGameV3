using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

	public static readonly float DEFAULT_ORTHO_SIZE = 3.25f;

	public enum State {
		READY,
		ANIMATING,
		PIECE_ACTIVATED
	}

	private static GameController instance = null;

	public State CurrentState { set; get; }
	public State NextState { set; get; }
	private int animationCount = 0;
	public GamePiece ActivePiece { set; get; }
	public bool UpdateNeeded { set; get; }

	[SerializeField]
	GameBoard gameBoard;

	[SerializeField]
	GameObject winPanel;

	LevelHandler levelHandler;


	private void Awake() {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy(gameObject);
		}
		//DontDestroyOnLoad(gameObject);
		levelHandler = FindObjectOfType<LevelHandler>();
	}
	private void Start() {
		if (levelHandler.CurrentLevel.camZoomAdjustment > 0)
			FindObjectOfType<Camera>().orthographicSize = levelHandler.CurrentLevel.camZoomAdjustment;
		else
			FindObjectOfType<Camera>().orthographicSize = DEFAULT_ORTHO_SIZE;
	}
	public static GameController GetInstance() {
		return instance;
	}


	public Level GetCurrentLevel() {
		return levelHandler.CurrentLevel;
	}


	// Keep track of how many animations are going and manage state
	public void AnimateStart(int num = 1) {
		if (animationCount == 0) {
			CurrentState = State.ANIMATING;
		}
		animationCount += num;
		Debug.Log("animationCount after ++: " + animationCount);
	}
	// Notify that an animation has ended, check if any are still going
	public void AnimateEnd() {
		animationCount--;
		Debug.Log("animationCount after --: " + animationCount);
		if (animationCount <= 0) {
			animationCount = 0;
			CurrentState = NextState;
			if (UpdateNeeded) {
				Debug.Log("Animation done, checking paths");
				StartCoroutine(gameBoard.UpdatePathsAndCheckForWin());
				UpdateNeeded = false;
			}
			if (NextState == State.PIECE_ACTIVATED)
				NextState = State.READY;
		}
		//if (CurrentState == State.READY) StartCoroutine(gameBoard.UpdatePathsAndCheckForWin());
	}


	public void PieceAction(int x, int z, bool wasCubePiece = false, CubePiece.Direction direction = CubePiece.Direction.NORTH) {
		int tempX = x;
		int tempZ = z + 1;
		for (int i = 0; i < 4; i++) {
			if (gameBoard.DoesPieceExist(tempX, tempZ))
				gameBoard.GetGamePiece(tempX, tempZ).pieceAnimating = true;
			if (i == 0) tempX += 1;
			else tempX -= 1;
			if (i == 2) tempZ += 1;
			else tempZ -= 1;
		}
		UpdateNeeded = true;
		StartCoroutine(gameBoard.UpdatePathsAndCheckForWin(false));
		StartCoroutine(gameBoard.PieceAction(x, z, wasCubePiece, direction));
	}


	public void PieceActionFinished() {
		//StartCoroutine(gameBoard.UpdatePathsAndCheckForWin());
	}


	public void PieceActionStart() {
		//StartCoroutine(gameBoard.UpdatePathsAndCheckForWin(false));
	}


	public void Win() {
		winPanel.SetActive(true);
	}
	public void BackToMenu() {
		if (CurrentState == State.READY)
			SceneManager.LoadScene("Menu");
	}


#if UNITY_EDITOR

	public void LEPieceClicked(int x, int z) {
		if (FindObjectOfType<LevelEditor>() != null)
			FindObjectOfType<LevelEditor>().LEPieceClick(x, z);
	}

#endif
}
