using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class LevelHandler : MonoBehaviour {

	public static string LEVEL_DATA_FILENAME;

	public static readonly float DEFAULT_CAM_POS_X = -2.5f;
	public static readonly float DEFAULT_CAM_POS_Y = 5f;
	public static readonly float DEFAULT_CAM_POS_Z = -5.75f;

	//public static readonly int TYPE_MOVE_LIMIT = 0;
	//public static readonly int TYPE_TIME_LIMIT = 1;

	private PackList packList;
	public Pack CurrentPack { get; set; }
	public Pack.Level CurrentLevel { get; set; }
	
	public Pack.Level CopiedLevel { get; set; }
	

	void Awake () {
		DontDestroyOnLoad(gameObject);
#if UNITY_EDITOR
		LEVEL_DATA_FILENAME = Path.Combine(Application.streamingAssetsPath, "levels.dat");
		LoadLevelData();
#elif UNITY_ANDROID
		LEVEL_DATA_FILENAME = "jar:file://" + Application.dataPath + "!/assets/" + "levels.dat";
		LoadLevelDataAndroid();
#endif
		SetCurrentPack(Pack.PackID.INTRO);
		SetCurrentLevel(CurrentPack.levelList[0].levelName);
		CopiedLevel = new Pack.Level(CurrentLevel);
	}


	public List<Pack.Level> GetLevelList() {
		return CurrentPack.levelList;
	}
	public PackList GetPackList() {
		return packList;
	}


	public bool SetCurrentLevel(int levelNumber) {
		if (levelNumber < CurrentPack.levelList.Count) {
			CurrentLevel = CurrentPack.levelList[levelNumber];
			return true;
		} else return false;
	}
	public bool SetCurrentLevel(string levelName) {
		foreach (Pack.Level level in CurrentPack.levelList) {
			if (level.levelName == levelName) {
				CurrentLevel = level;
				return true;
			}
		}
		return false;
	}
	public void SetCurrentPack(Pack.PackID packId) {
		CurrentPack = packList.GetPack(packId);
	}


	public bool GoToNextPack() {
		Pack nextPack = packList.NextPack(CurrentPack);
		if (nextPack != null && nextPack.packId != Pack.PackID.INTRO) {
			CurrentPack = nextPack;
			return true;
		} else return false;
	}
	public bool GoToPrevPack() {
		Pack prevPack = packList.PrevPack(CurrentPack);
		if (prevPack != null) {
			CurrentPack = prevPack;
			return true;
		} else return false;
	}
	public Pack GetNextPack() {
		return packList.NextPack(CurrentPack);
	}


	public int GetPackIndex(Pack.PackID packId) {
		for (int i = 0; i < packList.Count(); i++) {
			if (packList.GetList()[i].packId == packId) {
				return i;
			}
		}
		return -1;
	}
	public int GetLevelIndex(Pack.PackID packId, string levelName) {
		for (int i = 0; i < packList.GetPack(packId).levelList.Count; i++) {
			if (packList.GetPack(packId).levelList[i].levelName == levelName) {
				return i;
			}
		}
		return -1;
	}


	//public int GetCurrentLevelNumber() {
	//	int totalLevelCount = 0;
	//	foreach (Pack pack in packList.GetList()) {
	//		bool levelFound = false;
	//		foreach(Pack.Level level in pack.levelList) {
	//			totalLevelCount++;
	//			if (level.levelName == CurrentLevel.levelName) {
	//				levelFound = true;
	//				break;
	//			}
	//		}
	//		if (levelFound) break;
	//	}
	//	return totalLevelCount;
	//}


#if UNITY_EDITOR
	public void LoadLevelData() {
		if (File.Exists(LEVEL_DATA_FILENAME)) {
			try {
				BinaryFormatter bf = new BinaryFormatter();
				FileStream file = File.Open(LEVEL_DATA_FILENAME, FileMode.Open);
				SetupNewPackList();
				ComparePackListFile(bf.Deserialize(file) as PackList);
				file.Close();
				Debug.Log("LevelHandler.LoadLevelData success");
			} catch (Exception e) {
				SetupNewPackList();
				Debug.Log("LevelHandler.LoadLevelData exception: " + e.Message);
			}
		} else {
			SetupNewPackList();
			Debug.Log("LevelHandler.LoadLevelData file not found");
		}
	}
	private void SetupNewPackList() {
		packList = new PackList();
		foreach (Pack.PackID packId in Enum.GetValues(typeof(Pack.PackID))) {
			packList.Add(new Pack(packId));
		}
	}
	private void ComparePackListFile(PackList packListFromFile) {
		for (int i = 0; i < packList.GetList().Count; i++) {
			foreach (Pack packFromFile in packListFromFile.GetList()) {
				if (packList.GetList()[i].packId == packFromFile.packId) {
					packList.GetList()[i] = packFromFile;
				}
			}
		}
	}
#elif UNITY_ANDROID
	private void LoadLevelDataAndroid() {
		WWW file = new WWW(LEVEL_DATA_FILENAME);
		while(!file.isDone);
		MemoryStream memStream = new MemoryStream(file.bytes);
		BinaryFormatter bf = new BinaryFormatter();
		packList = bf.Deserialize(memStream) as PackList;
		memStream.Close();
	}
#endif


	public void SaveLevelData() {
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(LEVEL_DATA_FILENAME);
		bf.Serialize(file, packList);
		file.Close();
	}



	[System.Serializable]
	public class PackList {
		List<Pack> packList = new List<Pack>();
		int version = 1;

		//public PackList() {
		//	Debug.Log("Packlist constructor");
		//	foreach (Pack.PackID packId in Enum.GetValues(typeof(Pack.PackID))) {
		//		//Debug.Log(packId);
		//		packList.Add(new Pack(packId));
		//	}
		//}

		public List<Pack> GetList() {
			return packList;
		}

		public Pack GetPack(Pack.PackID packId) {
			foreach (Pack pack in packList) {
				if (packId == pack.packId) {
					return pack;
				}
			}
			return null;
		}
		public Pack NextPack(Pack pack) {
			for (int i = 0; i < packList.Count - 1; i++) {
				if (packList[i].packId == pack.packId && packList[i+1].packId != Pack.PackID.INTRO) {
					return packList[i + 1];
				}
			}
			return null;
		}
		public Pack PrevPack(Pack pack) {
			if (packList[0].packId == pack.packId) return null;
			for (int i = 1; i < packList.Count; i++) {
				if (packList[i].packId == pack.packId) {
					return packList[i - 1];
				}
			}
			return null;
		}

		public void Add(Pack pack) {
			packList.Add(pack);
		}
		public int Count() {
			return packList.Count;
		}
	}
}
