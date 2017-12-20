using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class LevelHandler : MonoBehaviour {

	public static string LEVEL_DATA_FILENAME;

	public static readonly int TYPE_MOVE_LIMIT = 0;
	public static readonly int TYPE_TIME_LIMIT = 1;

	private List<Level> levelList;
	public Level CurrentLevel { get; set; }

	public Level CopiedLevel { get; set; }
	
	void Awake () {
		DontDestroyOnLoad(gameObject);
#if UNITY_EDITOR
		LEVEL_DATA_FILENAME = Path.Combine(Application.streamingAssetsPath, "levels.dat");
		LoadLevelData();
#elif UNITY_ANDROID
		LEVEL_DATA_FILENAME = "jar:file://" + Application.dataPath + "!/assets/" + "levels.dat";
		LoadLevelDataAndroid();
#endif
		CurrentLevel = levelList[0];
		CopiedLevel = null;
	}


	public List<Level> GetLevelList() {
		return levelList;
	}


	public bool SetCurrentLevel(int levelNumber) {
		Debug.Log("levelList.Count " + levelList.Count);
		if (levelNumber < levelList.Count) {
			CurrentLevel = levelList[levelNumber];
			return true;
		} else return false;
	}

#if UNITY_EDITOR
	public void LoadLevelData() {
		if (File.Exists(LEVEL_DATA_FILENAME)) {
			try {
				BinaryFormatter bf = new BinaryFormatter();
				FileStream file = File.Open(LEVEL_DATA_FILENAME, FileMode.Open);
				levelList = bf.Deserialize(file) as List<Level>;
				file.Close();
				Debug.Log("LevelHandler.LoadLevelData success");
			} catch (Exception e) {
				levelList = new List<Level>();
				levelList.Add(new Level());
				Debug.Log("LevelHandler.LoadLevelData exception: " + e.Message);
			}
		} else {
			levelList = new List<Level>();
			levelList.Add(new Level());
			Debug.Log("LevelHandler.LoadLevelData file not found");
		}
	}
#elif UNITY_ANDROID
	private void LoadLevelDataAndroid() {
		WWW file = new WWW(LEVEL_DATA_FILENAME);
		while(!file.isDone);
		MemoryStream memStream = new MemoryStream(file.bytes);
		BinaryFormatter bf = new BinaryFormatter();
		levelList = bf.Deserialize(memStream) as List<Level>;
		memStream.Close();
	}
#endif


	public void SaveLevelData() {
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(LEVEL_DATA_FILENAME);
		bf.Serialize(file, levelList);
		file.Close();
	}
}
