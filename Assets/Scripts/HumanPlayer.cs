using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

public class HumanPlayer : Player
{
    public HumanPlayer(int startTime, int incrementMS, bool useClock){
        //Keeps track of the total time remaining
        TotalTimeRemaining = TimeSpan.FromSeconds(startTime);
        increment = TimeSpan.FromMilliseconds(incrementMS);
        //Keeps track of the current move time
        moveStopwatch = new Stopwatch();
    }

    public override void NotifyToMove()
    {
        moveStopwatch.Restart();
    }

    public override void NotifyGameOver()
    {
        return;
    }

    public void ChooseSelectedMove(Move move, string name){
        moveStopwatch.Stop();
        TotalTimeRemaining -= moveStopwatch.Elapsed;
        TotalTimeRemaining += increment;
        ChoseMove(move, name);
    }
}
