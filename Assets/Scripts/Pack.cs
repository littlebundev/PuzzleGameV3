using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pack {

	public enum PackID {
		PACK_1, PACK_2,
		PAID_PACK_1 = 1000,
		INTRO = 2000
	}

	public PackID packId;
	public List<Level> levelList;
	public string title;


	public Pack(PackID ID) {
		packId = ID;
		levelList = new List<Level>();
		levelList.Add(new Level());
	}

	public void Add(Level level) {
		levelList.Add(level);
	}



	[System.Serializable]
	public class Level {

		public enum GP_LUT {
		NULL,
		CUBE_TB, CUBE_EW, CUBE_NS,
		ROT_ANGLED_NE, ROT_ANGLED_ES, ROT_ANGLED_SW, ROT_ANGLED_WN,
		ROT_STRAIGHT_NS, ROT_STRAIGHT_EW,
		TOGGLE_ON, TOGGLE_OFF,
		START, END
		}
		public enum LevelType {
			MOVES,
			TIMED,
			BLACKOUT
		}

		public int levelNumber;
		public string levelName;

		public GP_LUT[,] pieceLU;
		public GP_LUT[,] winStatePieceLU;


		public LevelType levelType;

		public int[] levelLimits;
		public int[] levelRewards;

		public List<int> winMoveList;

		public float camZoomAdjustment;
	

		public Level() {
			InitVars();
		}
		public Level(int number) {
			levelNumber = number;
			InitVars();
		}
		public Level(Level levelToCopy) {
			InitVars();
			Copy(levelToCopy);
		}
		private void InitVars() {
			pieceLU = new GP_LUT[GameBoard.BOARD_SIZE_X, GameBoard.BOARD_SIZE_Z];
			winStatePieceLU = new GP_LUT[GameBoard.BOARD_SIZE_X, GameBoard.BOARD_SIZE_Z];
			levelLimits = new int[4] { 10, 8, 5, 0 };
			levelRewards = new int[4] { 150, 125, 100, 0 };
			winMoveList = new List<int>();
			camZoomAdjustment = GameController.DEFAULT_ORTHO_SIZE;
		}

		public void Copy(Level levelToCopy) {
			levelNumber = levelToCopy.levelNumber;
			levelName = levelToCopy.levelName;
			System.Array.Copy(levelToCopy.pieceLU, pieceLU, levelToCopy.pieceLU.GetLength(0) * levelToCopy.pieceLU.GetLength(1));
			if (levelToCopy.winStatePieceLU != null && winStatePieceLU != null)
				System.Array.Copy(levelToCopy.winStatePieceLU, winStatePieceLU, levelToCopy.winStatePieceLU.GetLength(0) * levelToCopy.winStatePieceLU.GetLength(1));
			else
				winStatePieceLU = new GP_LUT[GameBoard.BOARD_SIZE_X, GameBoard.BOARD_SIZE_Z];
			levelType = levelToCopy.levelType;
			System.Array.Copy(levelToCopy.levelLimits, levelLimits, levelToCopy.levelLimits.Length);
			System.Array.Copy(levelToCopy.levelRewards, levelRewards, levelToCopy.levelRewards.Length);
			winMoveList = new List<int>(levelToCopy.winMoveList);
			camZoomAdjustment = levelToCopy.camZoomAdjustment;
		}
	}
}
