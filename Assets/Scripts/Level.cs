using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Level {
	public enum GP_LUT {
		NULL,
		CUBE_TB,
		CUBE_EW,
		CUBE_NS,
		ROT_ANGLED_NE,
		ROT_ANGLED_ES,
		ROT_ANGLED_SW,
		ROT_ANGLED_WN,
		ROT_STRAIGHT_NS,
		ROT_STRAIGHT_EW,
		TOGGLE_ON,
		TOGGLE_OFF,
		START,
		END
	}
	
	public int levelNumber;
	public string levelName;

	public GP_LUT[,] pieceLU;

	public int levelType;
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
	private void InitVars() {
		pieceLU = new GP_LUT[GameBoard.BOARD_SIZE_X, GameBoard.BOARD_SIZE_Z];
		levelLimits = new int[4];
		levelRewards = new int[4];
		winMoveList = new List<int>();
		camZoomAdjustment = GameController.DEFAULT_ORTHO_SIZE;
	}
}
