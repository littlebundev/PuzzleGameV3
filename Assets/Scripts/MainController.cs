using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainController : MonoBehaviour {

	private static MainController instance;
	private LevelHandler levelHandler;

	[SerializeField]
	PlayerData playerData = new PlayerData();

	[SerializeField]
	string firstScene;

	private bool gameWon;


	private void Awake() {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy(gameObject);
		}
		levelHandler = FindObjectOfType<LevelHandler>();
	}


	void Start () {
		DontDestroyOnLoad(gameObject);
		//Screen.SetResolution(1280, 720, false);
		//FindObjectOfType<Camera>().aspect = 16f / 9f;
		ConfigureResolution();
		playerData.Load(levelHandler.GetPackList());
		SceneManager.LoadScene(firstScene);
	}
	public void ConfigureResolution() {
		float deviceAspect = (float)Screen.width / (float)Screen.height;
		float targetAspect = 16f / 9f;
		
		if (deviceAspect > targetAspect) {
			// Device wider than target, expand width
			int newWidth = (int)(1280 * (deviceAspect / targetAspect));
			Debug.Log("New Width " + newWidth);
			Screen.SetResolution(newWidth, 720, true);
		} else {
			int newHeight = (int)(720 / (deviceAspect / targetAspect));
			Debug.Log("New Height " + newHeight);
			Screen.SetResolution(1280, newHeight, false);
		}
	}


	public LevelHandler.PackList GetPackList() {
		return levelHandler.GetPackList();
	}
	public Pack GetCurrentPack() {
		return levelHandler.CurrentPack;
	}
	public void SetCurrentPack(Pack.PackID packId) {
		levelHandler.SetCurrentPack(packId);
	}
	public List<Pack.Level> GetLevelList() {
		return levelHandler.GetLevelList();
	}
	public Pack.Level GetCurrentLevel() {
		return levelHandler.CurrentLevel;
	}
	public bool SetCurrentLevel(Pack.Level level) {
		return levelHandler.SetCurrentLevel(level.levelName);
	}
	public bool SetCurrentLevel(int levelNumber) {
		return levelHandler.SetCurrentLevel(levelNumber);
	}
	public Vector3 GetCameraPositionVector() {
		Vector3 posVector = new Vector3(GetCurrentLevel().camX, GetCurrentLevel().camY, GetCurrentLevel().camZ);
		Debug.Log("posVector: " + posVector.ToString());
		if (!posVector.Equals(Vector3.zero)) {
			Debug.Log("posVector not zero");
			return posVector;
		} else {
			return new Vector3(LevelHandler.DEFAULT_CAM_POS_X, LevelHandler.DEFAULT_CAM_POS_Y, LevelHandler.DEFAULT_CAM_POS_Z);
		}
	}


	public void SetToLastPlayedLevel() {
		levelHandler.SetCurrentPack(playerData.GetLastPlayedPack());
		if (!levelHandler.SetCurrentLevel(playerData.GetLastPlayedLevel())) {
			levelHandler.SetCurrentLevel(0);
			Debug.Log("Setting to first level");
			playerData.SaveLastPlayedLevel(GetCurrentPack(), GetCurrentLevel());
		}
	}
	public void SaveLastPlayedLevel() {
		// Check if current level is farthest level played before saving
		if (levelHandler.GetPackIndex(levelHandler.CurrentPack.packId) >= levelHandler.GetPackIndex(playerData.GetLastPlayedPack()) &&
				levelHandler.GetLevelIndex(levelHandler.CurrentPack.packId, levelHandler.CurrentLevel.levelName) >=
				levelHandler.GetLevelIndex(playerData.GetLastPlayedPack(), playerData.GetLastPlayedLevel())) {
			Debug.Log("Saving LastPlayedLevel");
			//
			if (GetCurrentPack().levelList.Count > GetCurrentLevel().levelNumber + 1) {
				playerData.SaveLastPlayedLevel(GetCurrentPack(), GetCurrentPack().levelList[GetCurrentLevel().levelNumber + 1]);
			} else if (levelHandler.GetPackList().NextPack(GetCurrentPack()) != null) {
				playerData.SaveLastPlayedLevel(levelHandler.GetPackList().NextPack(GetCurrentPack()), levelHandler.GetPackList().NextPack(GetCurrentPack()).levelList[0]);
			}
			//playerData.SaveLastPlayedLevel(levelHandler.CurrentPack., levelHandler.CurrentLevel);
		}
	}


	public bool IsPackUnlocked(Pack pack) {
		return playerData.IsPackUnlocked(pack.packId);
	}
	public int GetInitialLevelCount() {
		int levelCount = 0;
		foreach (Pack pack in levelHandler.GetPackList().GetList()) {
			if (GetCurrentPack().packId == pack.packId) {
				break; 
			} else if (pack.packId != Pack.PackID.INTRO) {
				foreach (Pack.Level level in pack.levelList) {
					levelCount++;
				}
			}
		}
		return levelCount;
	}
	public int GetCurrentLevelTotalNumber() {
		int totalLevelNumber = GetInitialLevelCount();
		foreach (Pack.Level level in GetCurrentPack().levelList) {
			totalLevelNumber++;
			if (level.levelName == GetCurrentLevel().levelName)
				break;
		}
		return totalLevelNumber;
	}


	public int GetLevelProgress(int levelNumber) {
		//Debug.Log("packId: " + levelHandler.CurrentPack.packId + " levelNumber: " + levelNumber);
		return (int)playerData.GetLevelProgress(levelHandler.CurrentPack.packId, levelNumber);
	}
	public void SetLevelProgress(int stars, int points) {
		if (stars > GetLevelProgress(levelHandler.CurrentLevel.levelNumber)) {
			playerData.SetLevelProgress(GetCurrentPack().packId, GetCurrentLevel().levelName, stars);
			playerData.SetPoints(playerData.GetPoints() + points);
			// Check if next pack needs to be unlocked
			if (GetCurrentPack().levelList[GetCurrentPack().levelList.Count - 1].levelName == GetCurrentLevel().levelName) {
				if (levelHandler.GetNextPack() != null) {
					playerData.SetPackUnlocked(levelHandler.GetNextPack(), true);
				}
			}
		}
	}


	public int GetPoints() {
		return playerData.GetPoints();
	}


	public bool AdvanceLevel() {
		if (GetCurrentPack().levelList.Count > GetCurrentLevel().levelNumber + 1) {
			SetCurrentLevel(GetCurrentPack().levelList[GetCurrentLevel().levelNumber + 1]);
		} else if (levelHandler.GoToNextPack()) {
			SetCurrentLevel(GetCurrentPack().levelList[0]);
		} else return false;
		return true;
	}


	public void InitiateSkipLevel() {
		GetComponent<AdManager>().ShowRewardedAd();
	}
	public void SkipLevel() {
		// Mark level as skipped
		playerData.SetSkipped(GetCurrentPack(), GetCurrentLevel());
		// Move to next level
		SaveLastPlayedLevel();
		// Check if next pack needs to be unlocked
		if (GetCurrentPack().levelList[GetCurrentPack().levelList.Count - 1].levelName == GetCurrentLevel().levelName) {
			if (levelHandler.GetNextPack() != null) {
				playerData.SetPackUnlocked(levelHandler.GetNextPack(), true);
			}
		}
		FindObjectOfType<GameController>().NextLevel();
	}
	public bool GetLevelSkipped(int levelNumber) {
		return playerData.GetSkipped(GetCurrentPack(), levelNumber);
	}


	public bool CheckIfFirstLevel() {
		return (GetCurrentPack().packId == Pack.PackID.PACK_1 && !GetLevelSkipped(0) && GetLevelProgress(0) == 0);
	}


	public bool IsAdDue(bool gameWon) {
		this.gameWon = gameWon;
		bool playingAd = playerData.IncrementLevelsPlayed();
		if (playingAd) {
			GetComponent<AdManager>().ShowAd();
			playerData.AdPlayed();
		}
		return playingAd;
	}
	public void AdFinished() {
		GameController gameController = FindObjectOfType<GameController>();
		if (gameController != null) {
			if (gameWon)
				gameController.CompleteWin();
			else
				gameController.CompleteLoss();
		}
	}


	public bool ToggleSound() {
		if (playerData.GetSoundOn()) {
			playerData.SetSoundOn(false);
			return false;
		} else {
			playerData.SetSoundOn(true);
			return true;
		}
	}
	public bool IsSoundOn() {
		return playerData.GetSoundOn();
	}
	public bool ToggleMusic() {
		if (playerData.GetMusicOn()) {
			playerData.SetMusicOn(false);
			return false;
		} else {
			playerData.SetMusicOn(true);
			return true;
		}
	}
	public bool IsMusicOn() {
		return playerData.GetMusicOn();
	}
	public int GetMusicIndex() {
		return playerData.GetMusicIndex();
	}



	public void ResetPlayerData() {
		playerData.Reset(levelHandler.GetPackList());
	}
}
