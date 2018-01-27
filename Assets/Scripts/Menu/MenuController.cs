using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {

	enum MenuState {
		Main,
		Left
	}
	
	private MainController mainController;
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



	private void Awake() {
		
	}

	private void Start() {
		fader.gameObject.SetActive(true);
		mainController = FindObjectOfType<MainController>();
		gameBoard.InitGamePieces(mainController.GetCurrentLevel());
		mainController.SetLastPlayedLevel();
		InitMenus();
		StartCoroutine(Fade(true));
		menuState = MenuState.Main;
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.Escape) && inputAllowed) {
			if (menuState == MenuState.Main) {
				// Quit the game
#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_ANDROID
				AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
				activity.Call<bool>("moveTaskToBack" , true);
#endif
			} else if (menuState == MenuState.Left) {
				MenuMoveStart(true);
				
			}
		}
	}


	public void MenuStart() {
		// Load level from PlayerData
		Debug.Log("MenuStart");
		mainController.SetLastPlayedLevel();
		StartCoroutine(Fade(false));
	}
	public void MenuSelect() {
		selectMenu.SetActive(true);
		MenuMoveStart(false);
	}
	public void MenuStore() {
		selectMenu.SetActive(true);
		MenuMoveStart(false);
	}
	public void MenuOptions() {
		selectMenu.SetActive(true);
		MenuMoveStart(false);
	}


	private void InitMenus() {
		int packCount = 0;
		foreach (Pack pack in mainController.GetPackList().GetList()) {
			if (pack.packId != Pack.PackID.INTRO) {
				GameObject packButton = Instantiate(packButtonPrefab);
				packButton.transform.SetParent(packGroup.transform, false);
				packButton.GetComponent<PackButton>().Init(pack, mainController.IsPackUnlocked(pack));
				if (packCount == 0) packButton.GetComponent<PackButton>().SetHighlight(true);
				packCount++;
			}
		}
		Util.SetRectTransform(packGroup, 400, 125 * packCount + 60);

		InitLevelButtons();
	}
	private void InitLevelButtons() {
		int levelCount = 0;
		bool unlocked = true;
		for (int i = 0; i < mainController.GetLevelList().Count; i++) {
			GameObject levelButton = Instantiate(levelButtonPrefab);
			levelButton.transform.SetParent(levelGroup.transform, false);
			int levelProgress = mainController.GetLevelProgress(i);
			levelButton.GetComponent<LevelButton>().Init(i, levelProgress, unlocked);
			if (levelProgress == 0) unlocked = false;
			levelCount++;
		}
		Util.SetRectTransform(levelGroup, 880, 125 * (levelCount / 2) + 60);
	}


	private IEnumerator Fade(bool fadingIn) {
		if (fadingIn) {
			yield return StartCoroutine(fader.FadeIn());
			inputAllowed = true;
		} else {
			inputAllowed = false;
			yield return StartCoroutine(fader.FadeOut());
			SceneManager.LoadScene("Game");
		}
	}

	public void PackButtonClick(Pack.PackID packId) {
		Debug.Log(packId);
		foreach (Transform child in levelGroup.transform) {
			Destroy(child.gameObject);
		}
		mainController.SetCurrentPack(packId);
		InitLevelButtons();
	}
	public void LevelButtonClick(int levelNumber) {
		if (inputAllowed) {
			Debug.Log("LevelButtonClicked");
			mainController.SetCurrentLevel(levelNumber);
			StartCoroutine(Fade(false));
		}
	}


	private void MenuMoveStart(bool toMain) {
		inputAllowed = false;
		if (toMain) {
			menuState = MenuState.Main;
			menuAnimator.SetTrigger("MoveMain");
		} else {
			menuState = MenuState.Left;
			menuAnimator.SetTrigger("MoveLeft");
		}
	}
	public void MenuMoveFinished(bool toMain) {
		inputAllowed = true;
		if (toMain) {
			selectMenu.SetActive(false);
			storeMenu.SetActive(false);
			optionsMenu.SetActive(false);
		}
	}
}
