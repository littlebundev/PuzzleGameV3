using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

	public static SoundManager instance = null;
	[SerializeField]
	MainController mainController;

	[SerializeField]
	AudioSource sfxSource;
	[SerializeField]
	AudioSource musicSource;
	[SerializeField]
	List<AudioClip> musicClips;

	[SerializeField]
	float lowPitchRange = .95f;
	[SerializeField]
	public float highPitchRange = 1.05f;

	bool paused;


	private void Awake() {
		if (instance == null)
			instance = this;
		else if (instance != null)
			Destroy(gameObject);

		DontDestroyOnLoad(gameObject);
	}


	private void Update() {
		if (!musicSource.isPlaying && !paused) {
			PlayMusic(musicClips[mainController.GetMusicIndex()]);
		}
	}


	public void PlaySingle(AudioClip clip, bool randomizePitch = false, float pitchMult = 1.0f) {
		if (mainController.IsSoundOn()) {
			sfxSource.clip = clip;
			if (randomizePitch)
				sfxSource.pitch = Random.Range(lowPitchRange, highPitchRange);
			sfxSource.pitch = sfxSource.pitch * pitchMult;
			sfxSource.Play();
		}
	}

	public void PlayMusic(AudioClip clip) {
		if (mainController.IsMusicOn()) {
			musicSource.clip = clip;
			musicSource.Play();
		}
	}

	public void MusicOn() {
			musicSource.UnPause();
			paused = false;
	}

	public void MusicOff() {
			musicSource.Pause();
			paused = true;
	}
}
