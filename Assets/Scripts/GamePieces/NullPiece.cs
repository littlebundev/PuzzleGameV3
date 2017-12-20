using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NullPiece : GamePiece {

	public override void Clicked() {

	}

	public override bool[] CheckConnections() {
		return new bool[4];
	}


	//public override void InWinPath(bool inWinPath) {
	//}
}
