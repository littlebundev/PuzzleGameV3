using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchManager : MonoBehaviour {

	[SerializeField]
	bool editingLevels;
	

	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			//Prevent gamepiece click through ui click
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
			if (EventSystem.current.IsPointerOverGameObject(0)) {
				return;
			}
#else
			if (EventSystem.current.IsPointerOverGameObject(-1)) {
				return;
			}
#endif

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
			Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
#else
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
#endif
			RaycastHit hit;

#if UNITY_EDITOR
			// Level Editor not needed in android build-------------------------------------------------------
			//if (editingLevels) {
			//	if (Physics.Raycast(ray, out hit)) {
			//		if (hit.collider != null) {
			//			GameObject obj = hit.collider.gameObject;
			//			if (obj.GetComponent<GamePiece>() != null) {
			//				obj.GetComponent<GamePiece>().ClickedEditing();
			//			}
			//		}
			//	}
			//} else {
#endif
				switch (GameController.GetInstance().CurrentState) {

					case GameController.State.READY:
						if (Physics.Raycast(ray, out hit)) {
							if (hit.collider != null) {
								GameObject obj = hit.collider.gameObject;
								if (obj.GetComponent<GamePiece>() != null) {
									obj.GetComponent<GamePiece>().Clicked();
								}
							}
						}
						break;

					case GameController.State.PIECE_ACTIVATED:
						if (Physics.Raycast(ray, out hit)) {
							if (hit.collider != null) {
								GameObject obj = hit.collider.gameObject;
								// Check for activated CubePiece click
								if (obj.GetComponent<CubePieceTurnButton>() != null) {
									obj.GetComponent<CubePieceTurnButton>().Clicked();
								} else if (GameController.GetInstance().ActivePiece != null && GameController.GetInstance().ActivePiece.GetComponent<CubePiece>() != null) {
									GameController.GetInstance().ActivePiece.GetComponent<CubePiece>().Clicked();
								}
							}
						} else if (GameController.GetInstance().ActivePiece != null && GameController.GetInstance().ActivePiece.GetComponent<CubePiece>() != null) {
							GameController.GetInstance().ActivePiece.GetComponent<CubePiece>().Clicked();
						}
						break;
				}
#if UNITY_EDITOR
			//}
#endif
		}
	}
}
