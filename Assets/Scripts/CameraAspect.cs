using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAspect : MonoBehaviour {

	void Start() {
		Screen.SetResolution(1280, 720, true);

		// set the desired aspect ratio (the values in this example are
		// hard-coded for 16:9, but you could make them into public
		// variables instead so you can set them at design time)
		float targetAspect = 16.0f / 9.0f;

		// determine the game window's current aspect ratio
		float windowAspect = (float)Screen.width / (float)Screen.height;

		// current viewport height should be scaled by this amount
		float scaledHeight = windowAspect / targetAspect;

		// obtain camera component so we can modify its viewport
		Camera camera = GetComponent<Camera>();

		// if scaled height is less than current height, add letterbox
		if (scaledHeight < 1.0f) {
			Rect rect = camera.rect;

			rect.width = 1.0f;
			rect.height = scaledHeight;
			rect.x = 0;
			rect.y = (1.0f - scaledHeight) / 2.0f;

			camera.rect = rect;
		} else // add pillarbox
		  {
			float scalewidth = 1.0f / scaledHeight;

			Rect rect = camera.rect;

			rect.width = scalewidth;
			rect.height = 1.0f;
			rect.x = (1.0f - scalewidth) / 2.0f;
			rect.y = 0;

			camera.rect = rect;
		}

		//Screen.SetResolution(1280, 720, true);
	}
}
