using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LosePanel : MonoBehaviour {

	Animator animator;

	
	void Start () {
		animator = GetComponent<Animator>();
	}
	
	public void Lose() {
		animator.SetTrigger("Enter");
	}
}
