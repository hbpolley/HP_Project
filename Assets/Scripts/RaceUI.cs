using UnityEngine;
using TMPro;

public class RaceUI : MonoBehaviour
{
    //Reference to checkpoint manager
    public TrackCheckpoints trackCheckpoints;

    //Player car we're tracking
    public Transform playerCar;

    //UI text objects
    public TMP_Text lapText;
    public TMP_Text timerText;

    //Panel displayed when the race ends
    public GameObject finishPanel;

    //Total race time in seconds
    private float raceTimer;
    //Text shown on the finish screen
    public TMP_Text finalTimeText;

    private void Start()
    {
        //Hide finish screen when race begins
        finishPanel.SetActive(false);

        //Listen for race completion
        trackCheckpoints.OnRaceFinished += TrackCheckpoints_OnRaceFinished;
    }

    private void Update()
    {
        //Increase timer every frame
        raceTimer += Time.deltaTime;

        //Get current lap from checkpoint system
        int lap = trackCheckpoints.GetLapCount(playerCar);

        //Update lap display
        lapText.text = "Lap: " + lap;

        //Convert timer into minutes, seconds and milliseconds
        int minutes = Mathf.FloorToInt(raceTimer / 60f);
        int seconds = Mathf.FloorToInt(raceTimer % 60f);
        int milliseconds = Mathf.FloorToInt((raceTimer * 1000f) % 1000f);

        //Format:
        //01:23.456
        timerText.text = $"{minutes:00}:{seconds:00}.{milliseconds:000}";
    }

    //Called automatically when TrackCheckpoints
    //raises the OnRaceFinished event
    private void TrackCheckpoints_OnRaceFinished(object sender, System.EventArgs e)
    {
        //Show finish screen
        finishPanel.SetActive(true);

        //Convert final race time into minutes/seconds/milliseconds
        int minutes = Mathf.FloorToInt(raceTimer / 60f);
        int seconds = Mathf.FloorToInt(raceTimer % 60f);
        int milliseconds = Mathf.FloorToInt((raceTimer * 1000f) % 1000f);

        //Display final time
        finalTimeText.text = $"Time: {minutes:00}:{seconds:00}.{milliseconds:000}";

        //Pause the game
        Time.timeScale = 0f;
    }
}
