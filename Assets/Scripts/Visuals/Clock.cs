using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour
{

    public TMPro.TMP_Text timerDisplay;
    public int startSeconds;
    public bool isTurnToMove;
    public bool hasLost;
    float secondsRemaining;
    // Start is called before the first frame update
    public void StartGame()
    {
        secondsRemaining = startSeconds;
    }

    // Update is called once per frame
    void Update()
    {
        if(isTurnToMove){
            secondsRemaining -= Time.deltaTime;
            //Sets it to zero if it is lower than 0
            secondsRemaining = Mathf.Max(0, secondsRemaining);
        }
        hasLost = (secondsRemaining == 0)? true : false;
        int numMinutes = (int) (secondsRemaining/60);
        int numSeconds = (int) (secondsRemaining - numMinutes * 60);

        //Forces to display with 2 digits each
        timerDisplay.text = $"{numMinutes:00}:{numSeconds:00}";
    }
}
