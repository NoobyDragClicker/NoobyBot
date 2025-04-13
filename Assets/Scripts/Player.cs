using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Player
{
    public string name = "";
    public float timeRemaining;
    public int increment = 0;
    public bool useClock;
    public event System.Action<Move, string> onMoveChosen;
    public abstract void Update();
    public abstract void NotifyToMove();
    public abstract void NotifyGameOver();
    public virtual void ChoseMove (Move move, string name) {
		onMoveChosen?.Invoke(move, name);
	}

}
