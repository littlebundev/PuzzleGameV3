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
	bool inSkipMenu;
	bool inRestartMenu;

	[SerializeField]
	GameBoard gameBoard;

	[SerializeField]
	WinPanel winPanel;
	[SerializeField]
	LosePanel losePanel;
	[SerializeField]
	GameObject skipPanel;
	[SerializeField]
	GameObject restartPanel;
	[SerializeField]
	Text limitText;
	[SerializeField]
	Text levelText;
	[SerializeField]
	Fader fader;
	[SerializeField]
	Camera cam;
	[SerializeField]
	Animator camAnimator;

	[Header("Audio")]
	[SerializeField]
	AudioClip clickClip;
	[SerializeField]
	AudioClip glowClip;
	[SerializeField]
	AudioClip winClip;

	MainController mainController;

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
		//levelHandler = FindObjectOfType<LevelHandler>();
	}
	private void Start() {
		if (!inMenu) {
			CurrentState = State.ANIMATING;
			mainController = FindObjectOfType<MainController>();
			//Camera cam = Camera.main;
			//cam.aspect = 16f / 9f;
			cam.transform.LookAt(Vector3.zero);
			if (mainController.GetCurrentLevel().camZoomAdjustment > 0)
				cam.orthographicSize = mainController.GetCurrentLevel().camZoomAdjustment;
			else
				cam.orthographicSize = DEFAULT_ORTHO_SIZE;
			gameBoard.InitGamePieces(mainController.GetCurrentPack(), mainController.GetCurrentLevel());
			if (!editingLevel) {
				limitText.text = "Moves: " + mainController.GetCurrentLevel().levelLimits[2].ToString();
				levelText.text = "Level " + mainController.GetCurrentLevelTotalNumber();
				cam.transform.position = mainController.GetCameraPositionVector();
				Debug.Log("cam.transform.position: " + Camera.main.transform.position.ToString());
			}
			//Debug.Log("CurrentPack: " + levelHandler.CurrentPack.packId + " CurrentLevel: " + GetCurrentLevel().levelName);
			fader.gameObject.SetActive(true);
			StartCoroutine(Fade(true));
		}
	}
	private void Update() {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			if (inSkipMenu) {
				SkipLevelEscape();
			} else if (inRestartMenu) {
				RestartDialogEscape();
			} else {
				BackToMenu();
			}
		}
	}


	public IEnumerator Fade(bool fadingIn) {
		if (fadingIn) {
			yield return StartCoroutine(fader.FadeIn());
			CurrentState = State.READY;
			cam.transform.position = mainController.GetCameraPositionVector();
		} else {
			CurrentState = State.ANIMATING;
			yield return StartCoroutine(fader.FadeOut());
			SceneManager.LoadScene("Menu");
		}
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
				StartCoroutine(gameBoard.UpdatePathsAndCheckForWin());
				UpdateNeeded = false;
			}
			if (NextState == State.PIECE_ACTIVATED)
				NextState = State.READY;
		}
	}


	public void PieceAction(int x, int z, bool wasCubePiece = false, CubePiece.Direction direction = CubePiece.Direction.NORTH) {
		if (!editingLevel && ! inMenu) {
			movesMade++;
			limitText.text = "Moves: " + (mainController.GetCurrentLevel().levelLimits[2] - movesMade).ToString();
		}
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
			if (!mainController.IsAdDue(true)) {
				CompleteWin();
			}
		}
	}
	public void CompleteWin() {
		//SoundManager.instance.PlaySingle(winClip, true);
		//cam.GetComponent<Animator>().SetTrigger("Win");
		camAnimator.SetTrigger("Win");

		if (movesMade <= mainController.GetCurrentLevel().levelLimits[0]) {
			Debug.Log("Win " + 3);
			winPanel.Win(3, mainController.GetCurrentLevel().levelRewards[0]);
			mainController.SetLevelProgress(3, mainController.GetCurrentLevel().levelRewards[0]);
		} else if (movesMade <= mainController.GetCurrentLevel().levelLimits[1]) {
			Debug.Log("Win " + 2);
			winPanel.Win(2, mainController.GetCurrentLevel().levelRewards[1]);
			mainController.SetLevelProgress(2, mainController.GetCurrentLevel().levelRewards[1]);
		} else if (movesMade <= mainController.GetCurrentLevel().levelLimits[2]) {
			Debug.Log("Win " + 1);
			winPanel.Win(1, mainController.GetCurrentLevel().levelRewards[2]);
			mainController.SetLevelProgress(1, mainController.GetCurrentLevel().levelRewards[2]);
		}
		// Save last played pack/level
		mainController.SaveLastPlayedLevel();
	}
	public void CheckForLoss() {
		if ( !editingLevel && movesMade >= mainController.GetCurrentLevel().levelLimits[2] && CurrentState != State.END) {
			CurrentState = State.END;
			if (!mainController.IsAdDue(false))
				CompleteLoss();
		}
	}
	public void CompleteLoss() {
		//cam.GetComponent<Animator>().SetTrigger("Lose");
		camAnimator.SetTrigger("Lose");
		losePanel.Lose();
	}


	public void BackToMenu() {
		if (CurrentState == State.READY || CurrentState == State.PIECE_ACTIVATED || CurrentState == State.END) {
			SoundManager.instance.PlaySingle(clickClip);
			StartCoroutine(Fade(false));
		}
	}
	public void RestartLevel() {
		SoundManager.instance.PlaySingle(clickClip);
		SceneManager.LoadScene("Game");
	}
	public void NextLevel() {
		if (mainController.AdvanceLevel()) {
			SoundManager.instance.PlaySingle(clickClip);
			// Load next level
			SceneManager.LoadScene("Game");
		} else {
			BackToMenu();
		}
	}
	public void SkipLevel() {
		if (CurrentState == State.READY || CurrentState == State.PIECE_ACTIVATED) {
			SoundManager.instance.PlaySingle(clickClip);
			inSkipMenu = true;
			skipPanel.SetActive(true);
			Debug.Log("skipLevel clicked");
		}
	}
	public void SkipLevelConfirm() {
		CurrentState = State.END;
		SkipLevelEscape();
		mainController.InitiateSkipLevel();
	}
	public void SkipLevelEscape() {
		SoundManager.instance.PlaySingle(clickClip);
		inSkipMenu = false;
		skipPanel.SetActive(false);
	}
	public void ShowRestartDialog() {
		if (CurrentState == State.READY || CurrentState == State.PIECE_ACTIVATED) {
			SoundManager.instance.PlaySingle(clickClip);
			inRestartMenu = true;
			restartPanel.SetActive(true);
		}
	}
	public void RestartDialogEscape() {
		SoundManager.instance.PlaySingle(clickClip);
		inRestartMenu = false;
		restartPanel.SetActive(false);
	}


#if UNITY_EDITOR
	// Level Editor methods, not needed in android build-------------------------------------------------------

	public void LEPieceClicked(int x, int z) {
		if (FindObjectOfType<LevelEditor>() != null)
			FindObjectOfType<LevelEditor>().LEPieceClick(x, z);
	}
#endif
}
