using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


public class PlayerData {
	public static readonly string DATA_FILENAME = "/player.dat";
	private static readonly int LEVELS_BETWEEN_ADS = 4;

	public enum Purchase {
		DISABLE_ADS,

	}
	public enum LevelProgress {
		NONE, STAR1, STAR2, STAR3
	}

	Data data;


	public void Save() {
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
				Debug.Log("PlayerData: " + data.ToString());
				file.Close();
				if (IsUpdateNeeded(packList)) {
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

		if (data.packTrackerList.Count != packList.GetList().Count) {
			Debug.Log("PlayerData update needed: pack count difference");
			return true;
		}

		for (int i = 0; i < packList.GetList().Count; i++) {
			if (packList.GetList()[i].packId != data.packTrackerList[i].packId
					|| packList.GetList()[i].levelList.Count != data.packTrackerList[i].levelTrackerList.Count) {
				Debug.Log("PlayerData update needed: pack order or level count difference");
				return true;
			}
			for (int j = 0; j < packList.GetList()[i].levelList.Count; j ++) {
				if (packList.GetList()[i].levelList[j].levelName != data.packTrackerList[i].levelTrackerList[j].levelName) {
					Debug.Log("PlayerData update needed: level order difference " + i + " " + j + " ll:" + packList.GetList()[i].levelList[j].levelName + " pd:" + data.packTrackerList[i].levelTrackerList[j].levelName);
					return true;
				}
			}
		}
		return false;
	}
	private void ReconstructPlayerData(LevelHandler.PackList packList) {
		// reconstruct playerdata file since it is inconsistent with level data file
		// foreach pack: find matching packid in packtrackerlist, else make new packtracker
		// foreach level in pack.levellist: find matching levelName in leveltracker from matching packtracker, else make new leveltracker
		Debug.Log("Reconstructing PlayerData");
		Data newData = new Data(packList);
		newData.points = data.points;
		newData.soundOn = data.soundOn;
		newData.musicOn = data.musicOn;
		newData.adDue = data.adDue;
		newData.levelsPlayed = data.levelsPlayed;
		newData.lastPlayedLevel = data.lastPlayedLevel;
		newData.lastPlayedPackID = data.lastPlayedPackID;
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
		data = newData;
		Save();
		Debug.Log(data.ToString());
	}


	public void SetPoints(int points) {
		data.points = points;
		Save();
	}
	public int GetPoints() {
		return data.points;
	}


	// For tracking which packs have been unlocked
	public bool IsPackUnlocked(Pack.PackID packId) {
		//return packIndex < data.levelPackUnlockedList.Count && data.levelPackUnlockedList[packIndex];
		foreach (PackTracker packTracker in data.packTrackerList) {
			if (packId == packTracker.packId) {
				return packTracker.isUnlocked;
			}
		}
		return false;
	}
	public void SetPackUnlocked(Pack pack, bool unlocked) {
		foreach (PackTracker packTracker in data.packTrackerList) {
			if (pack.packId == packTracker.packId) {
				packTracker.isUnlocked = unlocked;
				Save();
			}
		}
	}


	// For tracking stars gained for each level
	public LevelProgress GetLevelProgress(Pack.PackID packId, int levelNumber) {
		foreach (PackTracker packTracker in data.packTrackerList) {
			if (packId == packTracker.packId) {
				if (packTracker.levelTrackerList.Count > levelNumber)
					return packTracker.levelTrackerList[levelNumber].levelProgress;
				else
					break;
			}
		}
		return LevelProgress.NONE;
	}
	public void SetLevelProgress(Pack.PackID packId, string levelName, int stars) {
		foreach (PackTracker packTracker in data.packTrackerList) {
			if (packId == packTracker.packId) {
				foreach(LevelTracker levelTracker in packTracker.levelTrackerList) {
					//Debug.Log(levelTracker.ToString() + " " + levelName);
					if (levelName == levelTracker.levelName) {
						levelTracker.levelProgress = (LevelProgress)stars;
						Save();
						break;
					}
				}
				break;
			}
		}
	}
	public bool GetSkipped(Pack pack, int levelNumber) {
		foreach (PackTracker packTracker in data.packTrackerList) {
			if (pack.packId == packTracker.packId) {
				if (packTracker.levelTrackerList.Count > levelNumber)
					return packTracker.levelTrackerList[levelNumber].skipped;
				else
					break;
			}
		}
		return false;
	}
	public void SetSkipped(Pack pack, Pack.Level level) {
		foreach (PackTracker packTracker in data.packTrackerList) {
			if (pack.packId == packTracker.packId) {
				foreach (LevelTracker levelTracker in packTracker.levelTrackerList) {
					if (level.levelName == levelTracker.levelName) {
						levelTracker.skipped = true;
						break;
					}
				}
				break;
			}
		}
	}


	// For tracking how far player has made it though levels
	public Pack.PackID GetLastPlayedPack() {
		return data.lastPlayedPackID;
	}
	public string GetLastPlayedLevel() {
		return data.lastPlayedLevel;
	}
	public void SaveLastPlayedLevel(Pack pack, Pack.Level level) {
		data.lastPlayedPackID = pack.packId;
		data.lastPlayedLevel = level.levelName;
		Save();
	}


	public bool IncrementLevelsPlayed() {
		if (!data.adDue && data.levelsPlayed < LEVELS_BETWEEN_ADS) {
			data.levelsPlayed++;
		} else {
			data.levelsPlayed = 0;
			data.adDue = true;
		}
		Save();
		return data.adDue;
	}
	public void AdPlayed() {
		data.adDue = false;
		Save();
	}


	public bool GetSoundOn() {
		return data.soundOn;
	}
	public void SetSoundOn(bool state) {
		data.soundOn = state;
		Save();
	}
	public bool GetMusicOn() {
		return data.musicOn;
	}
	public void SetMusicOn(bool state) {
		data.musicOn = state;
		Save();
	}
	public int GetMusicIndex() {
		int index = data.lastSongIndex;
		if (data.lastSongIndex >= 3) {
			data.lastSongIndex = 0;
		} else {
			data.lastSongIndex++;
		}
		return index;
	}


	public void Reset(LevelHandler.PackList packList) {
		int levelsPlayed = data.levelsPlayed;
		bool adDue = data.adDue;
		bool soundOn = data.soundOn;
		bool musicOn = data.musicOn;
		int lastSongIndex = data.lastSongIndex;
		data = new Data(packList);
		data.levelsPlayed = levelsPlayed;
		data.adDue = adDue;
		data.soundOn = soundOn;
		data.musicOn = musicOn;
		data.lastSongIndex = lastSongIndex;
		Save();
	}

	


	[Serializable]
	class Data {
		public List<PackTracker> packTrackerList;

		// Player's accumulated points
		public int points;
		public Pack.PackID lastPlayedPackID;
		public string lastPlayedLevel = "";
		public int version;
		public int levelsPlayed;
		public bool adDue;

		// Options
		public bool soundOn;
		public bool musicOn;
		public int lastSongIndex;


		public Data(LevelHandler.PackList packList) {
			// Initialize PackTracker list to contain all packs
			packTrackerList = new List<PackTracker>();
			foreach (Pack pack in packList.GetList()) {
				packTrackerList.Add(new PackTracker(pack));
			}
			lastPlayedPackID = Pack.PackID.PACK_1;
			soundOn = true;
			musicOn = true;
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
			if (packId == Pack.PackID.PACK_1)
				isUnlocked = true;
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
		public bool skipped;

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
