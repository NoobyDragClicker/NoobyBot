using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : Player
{
    public override void Update()
    {
        return;
    }
    public override void NotifyToMove()
    {
        return;
    }

    public void ChooseSelectedMove(Move move){
        ChoseMove(move);
    }
}
