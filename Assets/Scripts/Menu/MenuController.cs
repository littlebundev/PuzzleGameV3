using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour {

	enum MenuState {
		Main,
		Right,
		Popup
	}
	
	private MainController mainController;
	private ColorManager colorManager;
	private MenuState menuState;
	private bool inputAllowed;

	[SerializeField]
	PlayerData playerData;
	[SerializeField]
	GameBoard gameBoard;
	[SerializeField]
	Animator menuAnimator;
	[SerializeField]
	Fader fader;
	[SerializeField]
	Text startText;
	[SerializeField]
	Text pointsText;

	[Header("Select Screen")]
	[SerializeField]
	GameObject selectMenu;
	[SerializeField]
	GameObject packGroup;
	[SerializeField]
	GameObject packButtonPrefab;
	[SerializeField]
	GameObject levelGroup;
	[SerializeField]
	GameObject levelButtonPrefab;

	[Header("Store Screen")]
	[SerializeField]
	GameObject storeMenu;

	[Header("Options Screen")]
	[SerializeField]
	GameObject optionsMenu;
	[SerializeField]
	Text soundText;
	[SerializeField]
	Text musicText;
	[SerializeField]
	GameObject resetPopup;

	[Header("Audio")]
	[SerializeField]
	AudioClip menuClickClip;
	[SerializeField]
	AudioClip music;



	private void Awake() {
		
	}

	private void Start() {
		//Screen.SetResolution(1280, 720, false);
		//FindObjectOfType<Camera>().aspect = 16f / 9f;

		fader.gameObject.SetActive(true);
		mainController = FindObjectOfType<MainController>();
		colorManager = FindObjectOfType<ColorManager>();
		// Init menu gameboard
		mainController.SetCurrentPack(Pack.PackID.INTRO);
		mainController.SetCurrentLevel(0);
		gameBoard.InitGamePieces(mainController.GetCurrentPack(), mainController.GetCurrentLevel());

		pointsText.text = "Points: " + mainController.GetPoints();
		mainController.SetToLastPlayedLevel();
		InitMenus();
		StartCoroutine(Fade(true));
		menuState = MenuState.Main;
		StartCoroutine(GameBoardAnimation());
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.Escape) && inputAllowed) {
			if (menuState == MenuState.Main) {
				MenuQuit();
			} else if (menuState == MenuState.Right) {
				MenuMoveStart(true);
			} else if (menuState == MenuState.Popup) {
				OptionsResetDismiss();
			}
		}
	}


	public void MenuStart() {
		SoundManager.instance.PlaySingle(menuClickClip, true);
		StartCoroutine(Fade(false));
	}
	public void MenuSelect() {
		selectMenu.SetActive(true);
		MenuMoveStart(false);
	}
	public void MenuStore() {
		storeMenu.SetActive(true);
		MenuMoveStart(false);
	}
	public void MenuQuit() {
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_ANDROID
				AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
				activity.Call<bool>("moveTaskToBack" , true);
#endif
	}
	public void MenuOptions() {
		optionsMenu.SetActive(true);
		MenuMoveStart(false);
	}
	public void OptionsSound() {
		if (mainController.ToggleSound()) {
			soundText.text = "Sound FX: On";
		} else {
			soundText.text = "Sound FX: Off";
		}
		SoundManager.instance.PlaySingle(menuClickClip, true);
	}
	public void OptionsMusic() {
		if (mainController.ToggleMusic()) {
			musicText.text = "Music: On";
			SoundManager.instance.MusicOn();
		} else {
			musicText.text = "Music: Off";
			SoundManager.instance.MusicOff();
		}
		SoundManager.instance.PlaySingle(menuClickClip, true);
	}
	public void OptionsReset() {
		menuState = MenuState.Popup;
		SoundManager.instance.PlaySingle(menuClickClip, true);
		resetPopup.SetActive(true);
	}
	public void OptionsResetConfirm() {
		SoundManager.instance.PlaySingle(menuClickClip, true);
		mainController.ResetPlayerData();
		StartCoroutine(Fade(false, "Menu"));
	}
	public void OptionsResetDismiss() {
		resetPopup.SetActive(false);
		menuState = MenuState.Right;
		SoundManager.instance.PlaySingle(menuClickClip, true);
	}

	private void InitMenus() {
		if (!mainController.CheckIfFirstLevel())
			startText.text = "Continue";

		int packCount = 0;
		foreach (Pack pack in mainController.GetPackList().GetList()) {
			if (pack.packId != Pack.PackID.INTRO) {
				colorManager.SetPackIndex(pack.packId);
				GameObject packButton = Instantiate(packButtonPrefab);
				packButton.transform.SetParent(packGroup.transform, false);
				packButton.GetComponent<PackButton>().Init(pack, mainController.IsPackUnlocked(pack), colorManager.GetPrimaryColor());
				if (mainController.GetCurrentPack().packId == pack.packId)
					packButton.GetComponent<PackButton>().SetHighlight(true);
				packCount++;
			}
		}
		Util.SetRectTransform(packGroup, 400, 125 * packCount + 120);

		colorManager.SetPackIndex(mainController.GetCurrentPack().packId);
		InitLevelButtons();

		if (!mainController.IsSoundOn())
			soundText.text = "Sound FX: Off";

		if (!mainController.IsMusicOn())
			musicText.text = "Music: Off";
	}
	private void InitLevelButtons() {
		int levelCount = mainController.GetInitialLevelCount();
		bool unlocked = true;
		int i = 0;
		for (i = 0; i < mainController.GetLevelList().Count; i++) {
			GameObject levelButton = Instantiate(levelButtonPrefab);
			levelButton.transform.SetParent(levelGroup.transform, false);
			int levelProgress = mainController.GetLevelProgress(i);
			if (i == 0 && !mainController.IsPackUnlocked(mainController.GetCurrentPack()))
				unlocked = false;
			levelButton.GetComponent<LevelButton>().Init(i, levelCount, levelProgress, unlocked, colorManager.GetPrimaryColor());
			if (levelProgress == 0 && !mainController.GetLevelSkipped(i)) unlocked = false;
			levelCount++;
		}
		Util.SetRectTransform(levelGroup, 880, 125 * (i / 2 + i % 2) + 120);
	}


	private IEnumerator Fade(bool fadingIn, string sceneToLoad = "Game") {
		if (fadingIn) {
			yield return StartCoroutine(fader.FadeIn());
			inputAllowed = true;
		} else {
			inputAllowed = false;
			yield return StartCoroutine(fader.FadeOut());
			SceneManager.LoadScene(sceneToLoad);
		}
	}

	public void PackButtonClick(Pack.PackID packId) {
		SoundManager.instance.PlaySingle(menuClickClip, true);
		foreach (Transform child in levelGroup.transform) {
			Destroy(child.gameObject);
		}
		mainController.SetCurrentPack(packId);
		colorManager.SetPackIndex(packId);
		InitLevelButtons();
	}
	public void LevelButtonClick(int levelNumber) {
		if (inputAllowed && mainController.IsPackUnlocked(mainController.GetCurrentPack())) {
			SoundManager.instance.PlaySingle(menuClickClip, true);
			mainController.SetCurrentLevel(levelNumber);
			StartCoroutine(Fade(false));
		}
	}
	public void LevelSelectBackClick() {
		MenuMoveStart(true);
	}


	private void MenuMoveStart(bool toMain) {
		inputAllowed = false;
		if (toMain) {
			menuState = MenuState.Main;
			menuAnimator.SetTrigger("MoveMain");
		} else {
			menuState = MenuState.Right;
			menuAnimator.SetTrigger("MoveLeft");
		}
		SoundManager.instance.PlaySingle(menuClickClip, true);
	}
	public void MenuMoveFinished(bool toMain) {
		inputAllowed = true;
		if (toMain) {
			selectMenu.SetActive(false);
			storeMenu.SetActive(false);
			optionsMenu.SetActive(false);
		}
	}


	private IEnumerator GameBoardAnimation() {
		int[,] moves = { { 5, 3 }, { 4, 1 }, { 4, 4 }, { 4, 0 }, { 7, 3 }, { 2, 2 }, { 8, 1 }, { 2, 0 }, { 1, 1 }, { 5, 3 }, { 1, 1 } };
		int i = 0;
		while (true) {
			yield return new WaitForSeconds(1.5f);
			gameBoard.gamePieces[moves[i,0], moves[i,1]].Clicked();
			i++;
			if (i >= 10)
				i = 0;
		}
	}
}
