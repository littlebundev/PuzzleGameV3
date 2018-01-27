using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


public class PlayerData {
	public static readonly string DATA_FILENAME = "/player.dat";

	public enum Purchase {
		DISABLE_ADS,

	}
	public enum LevelProgress {
		NONE, STAR1, STAR2, STAR3
	}

	Data data;


	public void Save() {
		Debug.Log("Saving PlayerData");
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(Application.persistentDataPath + DATA_FILENAME);
		bf.Serialize(file, data);
		file.Close();
	}

	public void Load(LevelHandler.PackList packList) {
		if (File.Exists(Application.persistentDataPath + DATA_FILENAME)) {
			Debug.Log("PersistentDataPath: " + Application.persistentDataPath);
			try {
				BinaryFormatter bf = new BinaryFormatter();
				FileStream file = File.Open(Application.persistentDataPath + DATA_FILENAME, FileMode.Open);
				data = bf.Deserialize(file) as Data;
				//Debug.Log("PlayerData: " + data.ToString());
				file.Close();
				if (IsUpdateNeeded(packList)) {
					Debug.Log("PlayerData update needed...");
					ReconstructPlayerData(packList);
				}
			} catch (Exception e) {
				Debug.Log("PlayerProgress.Load exception: " + e.ToString());
				data = new Data(packList);
				Save();
			}
		} else {
			Debug.Log("PlayerProgress.Load file not found");
			data = new Data(packList);
			Save();
		}
	}

	private bool IsUpdateNeeded(LevelHandler.PackList packList) {
		// find out if playerdata file matches level data file
		// compare packlist length
		// foreach pack: compare packids, levellist length
		// foreach level in levellist: compare levelname

		if (data.packTrackerList.Count != packList.GetList().Count) return true;

		for (int i = 0; i < packList.GetList().Count; i++) {
			if (packList.GetList()[i].packId != data.packTrackerList[i].packId
					|| packList.GetList()[i].levelList.Count != data.packTrackerList[i].levelTrackerList.Count)
				return true;
			for (int j = 0; j < packList.GetList()[i].levelList.Count; j ++) {
				if (packList.GetList()[i].levelList[j].levelName != data.packTrackerList[i].levelTrackerList[j].levelName)
					return true;
			}
		}
		return false;
	}
	private void ReconstructPlayerData(LevelHandler.PackList packList) {
		// reconstruct playerdata file since it is inconsistent with level data file
		// foreach pack: find matching packid in packtrackerlist, else make new packtracker
		// foreach level in pack.levellist: find matching levelName in leveltracker from matching packtracker, else make new leveltracker
		Data newData = new Data(packList);
		newData.points = data.points;
		newData.soundOn = data.soundOn;
		foreach (PackTracker newPackTracker in newData.packTrackerList) {
			foreach (PackTracker oldPackTracker in data.packTrackerList) {
				if (newPackTracker.packId == oldPackTracker.packId) {
					newPackTracker.isUnlocked = oldPackTracker.isUnlocked;
					bool wasLastPlayedPack = data.lastPlayedPackID == oldPackTracker.packId;
					if (wasLastPlayedPack)
						newData.lastPlayedPackID = oldPackTracker.packId;
					foreach (LevelTracker newLevelTracker in newPackTracker.levelTrackerList) {
						foreach (LevelTracker oldLevelTracker in oldPackTracker.levelTrackerList) {
							if (newLevelTracker.levelName == oldLevelTracker.levelName) {
								newLevelTracker.levelProgress = oldLevelTracker.levelProgress;
								if (wasLastPlayedPack && newLevelTracker.levelName == data.lastPlayedLevel)
									newData.lastPlayedLevel = data.lastPlayedLevel;
								break;
							}
						}
					}
					break;
				}
			}
		}
		Save();
	}


	public int GetPoints() {
		return data.points;
	}
	public bool IsPackUnlocked(Pack.PackID packId) {
		//return packIndex < data.levelPackUnlockedList.Count && data.levelPackUnlockedList[packIndex];
		foreach (PackTracker packTracker in data.packTrackerList) {
			if (packId == packTracker.packId) {
				return packTracker.isUnlocked;
			}
		}
		return false;
	}
	public LevelProgress GetLevelProgress(Pack.PackID packId, int levelNumber) {
		foreach (PackTracker packTracker in data.packTrackerList) {
			if (packId == packTracker.packId) {
				return packTracker.levelTrackerList[levelNumber].levelProgress;
			}
		}
		return LevelProgress.NONE;
	}
	public void SetLevelProgress(Pack.PackID packId, string levelName, int stars) {
		foreach (PackTracker packTracker in data.packTrackerList) {
			if (packId == packTracker.packId) {
				foreach(LevelTracker levelTracker in packTracker.levelTrackerList) {
					if (levelName == levelTracker.levelName) {
						levelTracker.levelProgress = (LevelProgress)stars;
						Save();
					}
					break;
				}
			}
			break;
		}
	}
	public Pack.PackID GetLastPlayedPack() {
		return data.lastPlayedPackID;
	}
	public string GetLastPlayedLevel() {
		return data.lastPlayedLevel;
	}


	[Serializable]
	class Data {
		public List<PackTracker> packTrackerList;

		// Player's accumulated points
		public int points;
		public Pack.PackID lastPlayedPackID;
		public string lastPlayedLevel = "";
		public int version;

		// Options
		public bool soundOn;


		public Data(LevelHandler.PackList packList) {
			// Initialize PackTracker list to contain all packs
			packTrackerList = new List<PackTracker>();
			foreach (Pack pack in packList.GetList()) {
				packTrackerList.Add(new PackTracker(pack));
			}

			soundOn = true;
		}

		public override string ToString() {
			string str = "";
			str += "points: " + points + "\n";
			str += "lastPlayedPackId: " + lastPlayedPackID.ToString() + "\n";
			str += "lastPlayedLevel: " + lastPlayedLevel + "\n";
			str += "soundOn: " + soundOn + "\n";
			str += "=====PackTrackers=====\n";
			foreach (PackTracker packTracker in packTrackerList)
				str += packTracker.ToString() + "\n";
			return str;
		}
	}

	[Serializable]
	class PackTracker {
		public bool isUnlocked;
		public Pack.PackID packId;
		public List<LevelTracker> levelTrackerList;

		public PackTracker(Pack pack) {
			// Initialize tracker for all levels in Pack
			packId = pack.packId;
			// Check if first pack, unlocked by default
			if (packId == Pack.PackID.PACK_1) isUnlocked = true;
			levelTrackerList = new List<LevelTracker>();
			foreach (Pack.Level level in pack.levelList) {
				levelTrackerList.Add(new LevelTracker(level.levelName));
			}
		}

		public override string ToString() {
			string str = "";
			str += "PackId: " + packId.ToString() + "\n";
			str += "isUnlocked: " + isUnlocked + "\n";
			str += "-----LevelTrackers-----\n";
			foreach (LevelTracker levelTracker in levelTrackerList)
				str += levelTracker.ToString() + "\n";
			return str;
		}
	}
	[Serializable]
	class LevelTracker {
		public LevelProgress levelProgress;
		public string levelName;

		public LevelTracker(string name) {
			levelName = name;
		}

		public override string ToString() {
			string str = "";
			str += levelName + ": " + levelProgress.ToString();
			return str;
		}
	}
}
