using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util : MonoBehaviour {

	public static IEnumerator RotateObject(GameObject obj, Vector3 rotationAmount, Vector3 rotationDirection, float rotationTime) {
		Vector3 beginningRotation = obj.transform.eulerAngles;
		float endTime = Time.time + rotationTime;
		while (Time.time <= endTime) {
			Vector3 d = rotationAmount * (Time.deltaTime / rotationTime);
			obj.transform.Rotate(d, Space.World);
			yield return null;
		}
	}

	public static IEnumerator RotateObjectAround(GameObject obj, Vector3 axis, float angle, float rotationTime, Space relativeTo = Space.World) {
		Quaternion startRotation = obj.transform.rotation;
		float endTime = Time.time + rotationTime;
		while(Time.time < endTime) {
			float rotAmt = angle * (Time.deltaTime / rotationTime);
			obj.transform.Rotate(axis, rotAmt, relativeTo);
			yield return null;
		}
		obj.transform.rotation = startRotation;
		obj.transform.Rotate(axis, angle, relativeTo);
	}


	public static IEnumerator MoveObject(GameObject obj, Vector3 moveVector, float moveTime) {
		Vector3 startPosition = obj.transform.position;
		Vector3 endPosition = startPosition + moveVector;
		float t = 0f;
		while (t < 1) {
			t += Time.deltaTime / moveTime;
			obj.transform.position = Vector3.Slerp(startPosition, endPosition, t);
			yield return null;
		}
	}

	public static IEnumerator TranslateObject(GameObject obj, Vector3 axis, float amount, float moveTime, Space relativeTo = Space.Self) {
		Vector3 startPosition = obj.transform.position;
		float endTime = Time.time + moveTime;
		while (Time.time < endTime) {
			float moveIncrement = amount * (Time.deltaTime / moveTime);
			obj.transform.Translate(axis * moveIncrement, relativeTo);
			yield return null;
		}
		obj.transform.position = startPosition;
		obj.transform.Translate(axis * amount, relativeTo);
	}

	public static IEnumerator ScaleObject(GameObject obj, Vector3 newScaleVector, float scaleTime) {
		Vector3 startScale = obj.transform.localScale;
		float t = 0f;
		while (t < 1) {
			t += Time.deltaTime / scaleTime;
			obj.transform.localScale = Vector3.Slerp(startScale, newScaleVector, t);
			yield return null;
		}
	}
}
