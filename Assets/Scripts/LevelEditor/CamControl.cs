using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamControl : MonoBehaviour {

	[SerializeField]
	float speed = 1f;

	void Update () {
		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");
		
		if (horizontal != 0 || vertical != 0) {
			transform.position += (transform.forward * vertical + transform.right * horizontal) * speed * Time.deltaTime;
		}

		if (Input.GetKey(KeyCode.E)) {
			transform.position += transform.up * speed * Time.deltaTime;
		} else if (Input.GetKey(KeyCode.Q)) {
			transform.position -= transform.up * speed * Time.deltaTime;
		} 
	}
}
