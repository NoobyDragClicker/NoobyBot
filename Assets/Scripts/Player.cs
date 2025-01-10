using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Player
{
    public event System.Action<Move> onMoveChosen;
    public abstract void Update();
    public abstract void NotifyToMove();
    public virtual void ChoseMove (Move move) {
		onMoveChosen?.Invoke (move);
	}

}
