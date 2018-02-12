using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour {

	public void ShowAd() {
		if (Advertisement.IsReady()) {
			Advertisement.Show("", new ShowOptions() { resultCallback = HandleAdResult });
		}
	}
	private void HandleAdResult(ShowResult result) {
		switch(result) {
			case ShowResult.Finished:
				Debug.Log("Ad finished");
				break;
			case ShowResult.Skipped:
				Debug.Log("Ad skipped");
				break;
			case ShowResult.Failed:
				Debug.Log("Ad failed");
				break;
		}
		GetComponent<MainController>().AdFinished();
	}


	public void ShowRewardedAd() {
		if (Advertisement.IsReady()) {
			Advertisement.Show("rewardedVideo", new ShowOptions() { resultCallback = HandleRewardedAdResult });
		}
	}
	private void HandleRewardedAdResult(ShowResult result) {
		switch (result) {
			case ShowResult.Finished:
				Debug.Log("Rewarded Ad finished");
				// Give the reward
				GetComponent<MainController>().SkipLevel();
				break;
			case ShowResult.Skipped:
				Debug.Log("Rewarded Ad skipped");
				break;
			case ShowResult.Failed:
				Debug.Log("Rewarded Ad failed");
				break;
		}
	}
}
