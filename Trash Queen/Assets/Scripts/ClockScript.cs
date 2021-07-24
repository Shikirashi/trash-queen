using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ClockScript : MonoBehaviour{
	private static float timer;
	private static float smolTimer;
	public static bool clockStarted = false;
	public static bool generationTimerStarted = false;
	public int generationCount;
	public int minuteLimit;
	public string timerFormatted;
	public string smolTimerFormatted;
	public TextMeshProUGUI clockText;
	public TextMeshProUGUI smolClockText;
	public TextMeshProUGUI generationCountText;
	public GenerationManager generator;
	public TargetInstantiate targetor;
	GameObject[] truckAmount;
	void Start(){
		clockStarted = true;
		generationTimerStarted = true;
		generationCount = 1;
		generationCountText.text = "Generasi " + generationCount;
	}

    void FixedUpdate(){
		truckAmount = null;
		truckAmount = GameObject.FindGameObjectsWithTag("Truck");
		if (clockStarted) {
			timer += Time.deltaTime;
			System.TimeSpan t = System.TimeSpan.FromSeconds(timer);
			timerFormatted = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms", t.Hours, t.Minutes, t.Seconds, t.Milliseconds);
			clockText.text = timerFormatted;
		}
		if (generationTimerStarted && clockStarted) {
			smolTimer += Time.deltaTime;
			System.TimeSpan st = System.TimeSpan.FromSeconds(smolTimer);
			smolTimerFormatted = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms", st.Hours, st.Minutes, st.Seconds, st.Milliseconds);
			smolClockText.text = smolTimerFormatted;
			if (st.Minutes >= minuteLimit) {
				generationTimerStarted = false;
				targetor.RefreshTargets();
				generator.CalculateFitness();
				generationCount++;
			}
			generationCountText.text = "Generasi " + generationCount + ": " + truckAmount.Length + " truk tersisa";
		}
		if (!generationTimerStarted) {
			smolTimer = 0f;
			generationTimerStarted = true;
		}
		if(generationCount == 100) {
			clockStarted = false;
			generationTimerStarted = false;
			generator.GenerateResult();
		}
    }
}
