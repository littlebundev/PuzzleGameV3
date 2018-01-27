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


	private void Awake() {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy(gameObject);
		}
		//DontDestroyOnLoad(gameObject);
		levelHandler = FindObjectOfType<LevelHandler>();
	}


	void Start () {
		DontDestroyOnLoad(gameObject);
		playerData.Load(levelHandler.GetPackList());
		SceneManager.LoadScene(firstScene);
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
	public Pack.Level GetCurrentLevel() {
		return levelHandler.CopiedLevel;
	}
	public bool SetCurrentLevel(int levelNumber) {
		return levelHandler.SetCurrentLevel(levelNumber);
	}
	public void SetLastPlayedLevel() {
		levelHandler.SetCurrentPack(playerData.GetLastPlayedPack());
		levelHandler.SetCurrentLevel(playerData.GetLastPlayedLevel());
	}
	public List<Pack.Level> GetLevelList() {
		return levelHandler.GetLevelList();
	}
	public bool IsPackUnlocked(Pack pack) {
		return playerData.IsPackUnlocked(pack.packId);
	}
	public int GetLevelProgress(int levelNumber) {
		return (int)playerData.GetLevelProgress(levelHandler.CurrentPack.packId, levelNumber);
	}
	public void SetLevelProgress(int stars) {
		playerData.SetLevelProgress(GetCurrentPack().packId, GetCurrentLevel().levelName, stars);
	}

	
	public void AdvanceLevel() {

	}
}
