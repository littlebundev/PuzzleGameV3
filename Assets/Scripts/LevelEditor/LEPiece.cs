using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LEPiece : MonoBehaviour {

#if UNITY_EDITOR

	public int XPos { get; set; }
	public int ZPos { get; set; }

	public void Clicked() {
		GameController.GetInstance().LEPieceClicked(XPos, ZPos);
	}

#endif
}
