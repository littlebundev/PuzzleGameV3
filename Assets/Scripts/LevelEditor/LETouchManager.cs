using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LETouchManager : MonoBehaviour {

	
	
	void Update () {
		if (Input.GetMouseButtonDown(0)) {

			if (EventSystem.current.IsPointerOverGameObject(-1)) {
				return;
			}

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
			Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
#else
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
#endif
			RaycastHit hit;

#if UNITY_EDITOR

			if (Physics.Raycast(ray, out hit)) {
				if (hit.collider != null) {
					GameObject obj = hit.collider.gameObject;
					if (obj.GetComponent<LEPiece>() != null) {
						obj.GetComponent<LEPiece>().Clicked();
					}
				}
			}

#endif

		} 
	}
}
