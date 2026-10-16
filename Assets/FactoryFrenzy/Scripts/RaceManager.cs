using System.Collections;
using UnityEngine;
using TMPro;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance { get; private set; }

    public enum State { Countdown, Racing, Finished }
    public State CurrentState { get; private set; }

    [Header("Scene refs")]
    public Transform startSpawn;       // SpawnPoint on the Start platform
    public PlayerController player;    // the root Player

    [Header("UI")]
    public TMP_Text countdownText;

    [Header("Settings")]
    public int countdownSeconds = 3;

    float raceStartTime;
    public float ElapsedTime => Time.time - raceStartTime;

    void Awake() { Instance = this; }

    void Start()
    {
        if (player == null || startSpawn == null || countdownText == null)
        {
            Debug.LogError($"RaceManager on '{name}' has an empty reference", this);
            return;
        }

        // line the player up on the start line; it's also the first checkpoint
        var respawn = player.GetComponent<PlayerRespawn>();
        respawn.SetCheckpoint(startSpawn.position, startSpawn.rotation);
        respawn.Respawn();

        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        CurrentState = State.Countdown;
        player.enabled = false;                 // no movement during the countdown

        for (int i = countdownSeconds; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        countdownText.text = "GO!";
        player.enabled = true;                  // the race starts
        CurrentState = State.Racing;
        raceStartTime = Time.time;

        yield return new WaitForSeconds(1f);
        countdownText.text = "";
    }
    public void OnPlayerFinished()
    {
        if (CurrentState != State.Racing) return;   // ignore touches before GO
        CurrentState = State.Finished;

        player.enabled = false;
        float time = ElapsedTime;
        Debug.Log("Finished in " + time + " s");

        if (ResultsScreen.Instance != null)
            ResultsScreen.Instance.Show(time);
    }
}