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
		PIECE_ACTIVATED,
		END
	}

	private static GameController instance = null;

	public State CurrentState { set; get; }
	public State NextState { set; get; }
	private int animationCount = 0;
	public GamePiece ActivePiece { set; get; }
	public bool UpdateNeeded { set; get; }

	[SerializeField]
	public bool editingLevel;
	[SerializeField]
	public bool inMenu;

	[SerializeField]
	GameBoard gameBoard;

	[SerializeField]
	WinPanel winPanel;
	[SerializeField]
	LosePanel losePanel;
	[SerializeField]
	Text limitText;
	[SerializeField]
	Fader fader;

	LevelHandler levelHandler;

	int movesMade;


	public static GameController GetInstance() {
		return instance;
	}


	private void Awake() {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy(gameObject);
		}
		levelHandler = FindObjectOfType<LevelHandler>();
	}
	private void Start() {
		if (!inMenu) {
			CurrentState = State.ANIMATING;
			Camera cam = FindObjectOfType<Camera>();
			cam.transform.LookAt(Vector3.zero);
			if (levelHandler.CurrentLevel.camZoomAdjustment > 0)
				cam.orthographicSize = levelHandler.CurrentLevel.camZoomAdjustment;
			else
				cam.orthographicSize = DEFAULT_ORTHO_SIZE;
			gameBoard.InitGamePieces(GetCurrentLevel());
			fader.gameObject.SetActive(true);
			StartCoroutine(Fade(true));
		}
	}
	private void Update() {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			BackToMenu();
		}
	}


	private IEnumerator Fade(bool fadingIn) {
		if (fadingIn) {
			yield return StartCoroutine(fader.FadeIn());
			CurrentState = State.READY;
		} else {
			CurrentState = State.ANIMATING;
			yield return StartCoroutine(fader.FadeOut());
			SceneManager.LoadScene("Menu");
		}
	}


	public Pack.Level GetCurrentLevel() {
		return levelHandler.CurrentLevel;
	}


	// Keep track of how many animations are going and manage state
	public void AnimateStart(int num = 1) {
		if (animationCount == 0) {
			CurrentState = State.ANIMATING;
		}
		animationCount += num;
		//Debug.Log("animationCount after ++: " + animationCount);
	}
	// Notify that an animation has ended, check if any are still going
	public void AnimateEnd() {
		animationCount--;
		//Debug.Log("animationCount after --: " + animationCount);
		if (animationCount <= 0) {
			animationCount = 0;
			CurrentState = NextState;
			if (UpdateNeeded) {
				//Debug.Log("Animation done, checking paths");
				StartCoroutine(gameBoard.UpdatePathsAndCheckForWin());
				UpdateNeeded = false;
			}
			if (NextState == State.PIECE_ACTIVATED)
				NextState = State.READY;
		}
		//if (CurrentState == State.READY) StartCoroutine(gameBoard.UpdatePathsAndCheckForWin());
	}


	public void PieceAction(int x, int z, bool wasCubePiece = false, CubePiece.Direction direction = CubePiece.Direction.NORTH) {
		movesMade++;
		limitText.text = "Moves: " + (GetCurrentLevel().levelLimits[2] - movesMade).ToString();
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


	public void Win() {
		if (!editingLevel) {
			CurrentState = State.END;
			FindObjectOfType<Camera>().GetComponent<Animator>().SetTrigger("Win");
			if (movesMade <= GetCurrentLevel().levelLimits[0]) {
				winPanel.Win(3, GetCurrentLevel().levelRewards[0]);
				FindObjectOfType<MainController>().SetLevelProgress(3);
			} else if (movesMade <= GetCurrentLevel().levelLimits[1]) {
				winPanel.Win(2, GetCurrentLevel().levelRewards[1]);
				FindObjectOfType<MainController>().SetLevelProgress(2);
			} else if (movesMade <= GetCurrentLevel().levelLimits[2]) {
				winPanel.Win(1, GetCurrentLevel().levelRewards[2]);
				FindObjectOfType<MainController>().SetLevelProgress(1);
			}
		}
	}
	public void CheckForLoss() {
		if (movesMade >= GetCurrentLevel().levelLimits[2]) {
			CurrentState = State.END;
			losePanel.Lose();
		}
	}
	public void BackToMenu() {
		if (CurrentState == State.READY || CurrentState == State.PIECE_ACTIVATED || CurrentState == State.END)
			StartCoroutine(Fade(false));
	}
	public void RestartLevel() {

	}
	public void NextLevel() {

	}


#if UNITY_EDITOR
	// Level Editor methods, not needed in android build-------------------------------------------------------

	public void LEPieceClicked(int x, int z) {
		if (FindObjectOfType<LevelEditor>() != null)
			FindObjectOfType<LevelEditor>().LEPieceClick(x, z);
	}
#endif
}
