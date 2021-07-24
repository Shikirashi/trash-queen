using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitButton : MonoBehaviour{
	public GameObject pauseButton;
	public GameObject exitBtn;
	bool isPaused = false;

    void Update(){
        
    }

	public void PauseGame() {
		if (!isPaused) {
			isPaused = !isPaused;
			Time.timeScale = 0f;
			Debug.Log("game paused");
		}
		else if (isPaused) {
			isPaused = !isPaused;
			Time.timeScale = 1f;
			Debug.Log("game unpaused");
		}
	}

	public void ExitGame() {
		Debug.Log("exiting game");
		Application.Quit();
	}
}
