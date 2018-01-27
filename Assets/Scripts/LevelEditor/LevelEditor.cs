using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelEditor : MonoBehaviour {

#if UNITY_EDITOR
	// Level Editor not needed in android build-------------------------------------------------------

	const string glyphs = "abcdefghijklmnopqrstuvwxyz0123456789";

	public enum Mode {
		TOGGLE_TYPE,
		TOGGLE_ROTATION,
		MIX_FOR_GAME
	}
	public Mode mode;

	[Header("Boards")]
	[SerializeField]
	LEGameBoard leGameBoard;
	[SerializeField]
	GameBoard gameBoard;

	[Header("Info Fields/Buttons")]
	[SerializeField]
	Text levelNumber;
	[SerializeField]
	InputField titleField;
	[SerializeField]
	Text editingMode;
	[SerializeField]
	Text packText;
	[SerializeField]
	InputField packTitleField;

	[Header("Limits/Rewards Fields")]
	[SerializeField]
	Toggle timeLimitToggle;
	[SerializeField]
	InputField limit3Field;
	[SerializeField]
	InputField limit2Field;
	[SerializeField]
	InputField limit1Field;
	[SerializeField]
	InputField reward3Field;
	[SerializeField]
	InputField reward2Field;
	[SerializeField]
	InputField reward1Field;

	[Header("Cam Fields")]
	[SerializeField]
	Slider camZoomSlider;

	[Header("Other Fields")]
	[SerializeField]
	Text winMovesText;
	

	private LevelHandler levelHandler;
	//private Level editingLevel;
	private Pack.Level editingLevel;
	private Camera cam;


	private void Start() {
		levelHandler = FindObjectOfType<LevelHandler>();
		cam = FindObjectOfType<Camera>();
		editingLevel = new Pack.Level(levelHandler.CurrentLevel);
		editingMode.text = mode.ToString();
		UpdateUIFromLevelData();
	}


	private void UpdateUIFromLevelData() {
		levelNumber.text = "Lvl: " + (editingLevel.levelNumber + 1);
		titleField.text = editingLevel.levelName;
		packText.text = levelHandler.CurrentPack.packId.ToString();
		packTitleField.text = levelHandler.CurrentPack.title;

		if (editingLevel.levelType == 0) timeLimitToggle.isOn = false;
		else timeLimitToggle.isOn = true;

		limit3Field.text = editingLevel.levelLimits[0].ToString();
		limit2Field.text = editingLevel.levelLimits[1].ToString();
		limit1Field.text = editingLevel.levelLimits[2].ToString();
		reward3Field.text = editingLevel.levelRewards[0].ToString();
		reward2Field.text = editingLevel.levelRewards[1].ToString();
		reward1Field.text = editingLevel.levelRewards[2].ToString();

		camZoomSlider.value = GameController.DEFAULT_ORTHO_SIZE - editingLevel.camZoomAdjustment;
		Debug.Log(camZoomSlider.value);
		cam.orthographicSize = editingLevel.camZoomAdjustment;
	}


	public void LEPieceClick(int x, int z) {
		if (mode == Mode.TOGGLE_TYPE) {
			gameBoard.LEChangePieceType(x, z);
		} else if (mode == Mode.TOGGLE_ROTATION) {
			gameBoard.LERotatePiece(x, z);
		}
	}
	public void SetGpLU(int x, int z, Pack.Level.GP_LUT gpLU) {
		editingLevel.pieceLU[x, z] = gpLU;
		PrintGpLU(editingLevel.pieceLU);
	}
	public void PrintGpLU(Pack.Level.GP_LUT[,] arr) {
		string printLUT = "";
		for (int i = 0; i < GameBoard.BOARD_SIZE_X; i++) {
			for (int j = 0; j < GameBoard.BOARD_SIZE_Z; j++) {
				if (arr[i,j] != Pack.Level.GP_LUT.NULL)
					printLUT += "(" + i + "," + j + ")" + arr[i, j] + ", ";
			}
		}
		Debug.Log(printLUT);
	}

	// Canvas button clicks and input fields-------------------------------------
	public void UISave() {
		//editingLevel.levelName = titleField.text;
		if (string.IsNullOrEmpty(editingLevel.levelName)) editingLevel.levelName = GenerateLevelName();
		levelHandler.CurrentPack.title = packTitleField.text;

		if (timeLimitToggle.isOn) editingLevel.levelType = Pack.Level.LevelType.TIMED;
		else editingLevel.levelType = Pack.Level.LevelType.MOVES;

		editingLevel.levelLimits[0] = int.Parse(limit3Field.text);
		editingLevel.levelLimits[1] = int.Parse(limit2Field.text);
		editingLevel.levelLimits[2] = int.Parse(limit1Field.text);
		editingLevel.levelRewards[0] = int.Parse(reward3Field.text);
		editingLevel.levelRewards[1] = int.Parse(reward2Field.text);
		editingLevel.levelRewards[2] = int.Parse(reward1Field.text);

		editingLevel.camZoomAdjustment = GameController.DEFAULT_ORTHO_SIZE - camZoomSlider.value;

		levelHandler.CurrentLevel.Copy(editingLevel);
		levelHandler.SaveLevelData();
	}
	public void UISaveWinState() {
		if (editingLevel.winStatePieceLU == null)
			editingLevel.winStatePieceLU = new Pack.Level.GP_LUT[GameBoard.BOARD_SIZE_X, GameBoard.BOARD_SIZE_Z];
		System.Array.Copy(editingLevel.pieceLU,
				editingLevel.winStatePieceLU,
				editingLevel.pieceLU.GetLength(0) * editingLevel.pieceLU.GetLength(1));
		if (levelHandler.CurrentLevel.winStatePieceLU == null)
			levelHandler.CurrentLevel.winStatePieceLU = new Pack.Level.GP_LUT[GameBoard.BOARD_SIZE_X, GameBoard.BOARD_SIZE_Z];
		System.Array.Copy(editingLevel.pieceLU,
				levelHandler.CurrentLevel.winStatePieceLU,
				editingLevel.pieceLU.GetLength(0) * editingLevel.pieceLU.GetLength(1));
		levelHandler.SaveLevelData();
	}
	public void UIChangeMode() {
		if (mode == Mode.MIX_FOR_GAME) mode = Mode.TOGGLE_TYPE;
		else mode++;
		if (mode == Mode.MIX_FOR_GAME) {
			leGameBoard.gameObject.SetActive(false);
		} else {
			leGameBoard.gameObject.SetActive(true);
		}
		editingMode.text = mode.ToString();
	}
	public void UIRecMoves() {

	}
	public void UINextPack() {
		if (levelHandler.NextPack()) {
			ChangeLevel(0);
			SceneManager.LoadScene("LevelEditor");
		} else Debug.Log("Next pack does not exist.");
	}
	public void UIPrevPack() {
		if (levelHandler.PrevPack()) {
			ChangeLevel(0);
			SceneManager.LoadScene("LevelEditor");
		} else Debug.Log("Prev pack does not exist.");
	}
	public void UINextLevel() {
		if (levelHandler.GetLevelList().Count > editingLevel.levelNumber + 1) {
			ChangeLevel(editingLevel.levelNumber + 1);
		} else Debug.Log("Next level does not exist.");
	}
	public void UIPrevLevel() {
		if (editingLevel.levelNumber > 0 && levelHandler.GetLevelList().Count > editingLevel.levelNumber - 1) {
			ChangeLevel(editingLevel.levelNumber - 1);
		} else Debug.Log("Previous level does not exist.");
	}
	public void UIDeleteLevel() {
		int tempLevelNumber = editingLevel.levelNumber;
		if (tempLevelNumber >= levelHandler.GetLevelList().Count - 1)
			tempLevelNumber--;
		levelHandler.GetLevelList().RemoveAt(editingLevel.levelNumber);
		for (int i = editingLevel.levelNumber; i < levelHandler.GetLevelList().Count; i++) {
			levelHandler.GetLevelList()[i].levelNumber--;
		}
		if (levelHandler.GetLevelList().Count == 0) levelHandler.GetLevelList().Add(new Pack.Level());
		levelHandler.SaveLevelData();
		ChangeLevel(tempLevelNumber);
	}
	public void UIInsertBefore() {
		CreateNewLevel(editingLevel.levelNumber);
		levelHandler.SaveLevelData();
	}
	public void UIInsertAfter() {
		CreateNewLevel(editingLevel.levelNumber + 1);
		levelHandler.SaveLevelData();
		ChangeLevel(editingLevel.levelNumber + 1);
	}
	public void UIRestoreWinState() {
		if (levelHandler.CurrentLevel.winStatePieceLU == null)
			editingLevel.winStatePieceLU = new Pack.Level.GP_LUT[GameBoard.BOARD_SIZE_X, GameBoard.BOARD_SIZE_Z];
		else {
			System.Array.Copy(levelHandler.CurrentLevel.winStatePieceLU,
					editingLevel.pieceLU,
					levelHandler.CurrentLevel.winStatePieceLU.GetLength(0) *
					levelHandler.CurrentLevel.winStatePieceLU.GetLength(1));
			gameBoard.Clear();
			gameBoard.InitGamePieces(editingLevel);
		}
	}
	public void UIReset() {
		System.Array.Copy(levelHandler.CurrentLevel.pieceLU,
				editingLevel.pieceLU,
				levelHandler.CurrentLevel.pieceLU.GetLength(0) *
				levelHandler.CurrentLevel.pieceLU.GetLength(1));
		gameBoard.Clear();
		gameBoard.InitGamePieces(editingLevel);
		UpdateUIFromLevelData();
	}
	public void UICopy() {
		levelHandler.CopiedLevel.Copy(editingLevel);
	}
	public void UIPaste() {
		//Need to update level number etc
		int tempLevelNumber = editingLevel.levelNumber;
		editingLevel.Copy(levelHandler.CopiedLevel);
		editingLevel.levelNumber = tempLevelNumber;
		gameBoard.InitGamePieces(editingLevel);
		UpdateUIFromLevelData();
	}


	private void ChangeLevel(int index) {
		levelHandler.CurrentLevel = levelHandler.GetLevelList()[index];
		SceneManager.LoadScene("LevelEditor");
	}
	private void CreateNewLevel(int index) {
		levelHandler.GetLevelList().Insert(index, new Pack.Level(index));
		for (int i = index + 1; i < levelHandler.GetLevelList().Count; i++) {
			levelHandler.GetLevelList()[i].levelNumber++;
		}
		ChangeLevel(index);
	}


	public void CamZoomSliderValueChanged() {
		editingLevel.camZoomAdjustment = GameController.DEFAULT_ORTHO_SIZE - camZoomSlider.value;
		cam.orthographicSize = editingLevel.camZoomAdjustment;
	}


	private string GenerateLevelName() {
		int length = Random.Range(8, 12);
		string generatedLevelName = "";
		do {
			for (int i = 0; i < length; i++) {
				generatedLevelName += glyphs[Random.Range(0, glyphs.Length)];
			}
		} while (!IsUniqueLevelName(generatedLevelName));
		titleField.text = generatedLevelName;
		return generatedLevelName;
	}
	private bool IsUniqueLevelName(string name) {
		foreach (Pack.Level level in levelHandler.CurrentPack.levelList) {
			if (level.levelName == name) return false;
		}
		return true;
	}

#endif
}
