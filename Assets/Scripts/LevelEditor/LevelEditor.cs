using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelEditor : MonoBehaviour {

#if UNITY_EDITOR
	//private static LevelEditor instance;

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
	private Level editingLevel;
	private Camera cam;


	private void Start() {
		levelHandler = FindObjectOfType<LevelHandler>();
		cam = FindObjectOfType<Camera>();
		editingLevel = levelHandler.CurrentLevel;
		editingMode.text = mode.ToString();
		UpdateUIFromLevelData();
	}


	private void UpdateUIFromLevelData() {
		levelNumber.text = "Lvl: " + editingLevel.levelNumber + " Save";
		titleField.text = editingLevel.levelName;

		if (editingLevel.levelType == 0) timeLimitToggle.isOn = false;
		else timeLimitToggle.isOn = true;

		limit3Field.text = editingLevel.levelLimits[0].ToString();
		limit2Field.text = editingLevel.levelLimits[1].ToString();
		limit1Field.text = editingLevel.levelLimits[2].ToString();
		reward3Field.text = editingLevel.levelLimits[0].ToString();
		reward2Field.text = editingLevel.levelLimits[1].ToString();
		reward1Field.text = editingLevel.levelLimits[2].ToString();

		camZoomSlider.value = editingLevel.camZoomAdjustment - GameController.DEFAULT_ORTHO_SIZE;
	}


	public void LEPieceClick(int x, int z) {
		if (mode == Mode.TOGGLE_TYPE) {
			gameBoard.LEChangePieceType(x, z);
		} else if (mode == Mode.TOGGLE_ROTATION) {
			gameBoard.LERotatePiece(x, z);
		}
	}
	public void SetGpLU(int x, int z, Level.GP_LUT gpLU) {
		editingLevel.pieceLU[x, z] = gpLU;
		string printLUT = "";
		for (int i = 0; i < GameBoard.BOARD_SIZE_X; i++) {
			for (int j = 0; j < GameBoard.BOARD_SIZE_Z; j++) {
				printLUT += "(" + i + "," + j + ")" + editingLevel.pieceLU[i,j] + ", ";
			}
		}
		Debug.Log(printLUT);
	}

	// Canvas button clicks and input fields-------------------------------------
	public void UISave() {
		editingLevel.levelName = titleField.text;

		if (timeLimitToggle.isOn) editingLevel.levelType = 1;
		else editingLevel.levelType = 0;

		editingLevel.levelLimits[0] = int.Parse(limit3Field.text);
		editingLevel.levelLimits[1] = int.Parse(limit2Field.text);
		editingLevel.levelLimits[2] = int.Parse(limit1Field.text);
		editingLevel.levelRewards[0] = int.Parse(limit3Field.text);
		editingLevel.levelRewards[1] = int.Parse(limit2Field.text);
		editingLevel.levelRewards[2] = int.Parse(limit1Field.text);

		editingLevel.camZoomAdjustment = GameController.DEFAULT_ORTHO_SIZE - camZoomSlider.value;

		levelHandler.GetLevelList()[editingLevel.levelNumber] = editingLevel;
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
		levelHandler.GetLevelList().RemoveAt(editingLevel.levelNumber);
		for (int i = editingLevel.levelNumber; i < levelHandler.GetLevelList().Count; i++) {
			levelHandler.GetLevelList()[i].levelNumber--;
		}
		if (levelHandler.GetLevelList().Count == 0) levelHandler.GetLevelList().Add(new Level());
		levelHandler.SaveLevelData();
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
	public void UIReset() {
		SceneManager.LoadScene("LevelEditor");
	}
	public void UICopy() {
		levelHandler.CopiedLevel = editingLevel;
	}
	public void UIPaste() {
		editingLevel = levelHandler.CopiedLevel;
		gameBoard.InitGamePieces(editingLevel);
		UpdateUIFromLevelData();
	}


	private void ChangeLevel(int index) {
		levelHandler.CurrentLevel = levelHandler.GetLevelList()[index];
		SceneManager.LoadScene("LevelEditor");
	}
	private void CreateNewLevel(int index) {
		levelHandler.GetLevelList().Insert(index, new Level(index));
		for (int i = index + 1; i < levelHandler.GetLevelList().Count; i++) {
			levelHandler.GetLevelList()[i].levelNumber++;
		}
		ChangeLevel(index);
	}


	public void CamZoomSliderValueChanged() {
		editingLevel.camZoomAdjustment = GameController.DEFAULT_ORTHO_SIZE - camZoomSlider.value;
		cam.orthographicSize = editingLevel.camZoomAdjustment;
	}

#endif
}
