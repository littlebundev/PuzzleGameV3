using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasScaler : MonoBehaviour {

	private void Start() {
		float deviceAspect = (float)Screen.width / (float)Screen.height;
		float targetAspect = 16f / 9f;
		if (deviceAspect > targetAspect) {
			//GetComponent<CanvasScaler>().m
		}
	}
}
