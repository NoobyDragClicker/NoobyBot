
using System;
using System.Diagnostics;
using UnityEngine;

public abstract class Player
{
  public string name = "";

  public TimeSpan increment;
  public Stopwatch moveStopwatch;
  public TimeSpan TotalTimeRemaining;


  public bool useClock;
  public event Action<Move, string> onMoveChosen;
  public abstract void NotifyToMove();
  public abstract void NotifyGameOver();
  public virtual void ChoseMove(Move move, string name)
  {
    onMoveChosen?.Invoke(move, name);
  }
  public int getSecondsRemaining()
    {
        return (int)(TotalTimeRemaining - moveStopwatch.Elapsed).TotalSeconds;
    }

}
