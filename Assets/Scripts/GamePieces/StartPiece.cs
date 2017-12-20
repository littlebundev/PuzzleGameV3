using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPiece : GamePiece {

	public override void Clicked() {

	}

	public override bool[] CheckConnections() {
		return new bool[] { true, true, true, true };
	}

	//public override void InWinPath(bool inWinPath) {
	//}
}
