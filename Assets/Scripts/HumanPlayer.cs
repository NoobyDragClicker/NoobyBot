using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : Player
{
    public HumanPlayer(float startTime, bool useClock){
        this.useClock = useClock;
        if(useClock){timeRemaining = startTime;}
    }
    public override void Update()
    {
        if(useClock){
            timeRemaining -= Time.deltaTime;
        }
        return;
    }
    public override void NotifyToMove()
    {
        return;
    }
    public override void NotifyGameOver()
    {
        return;
    }


    public void ChooseSelectedMove(Move move){
        ChoseMove(move);
    }
}
