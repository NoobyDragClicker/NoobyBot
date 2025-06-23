
using System;
using System.Diagnostics;

public abstract class Player
{
  public string name = "";
  public event Action<Move, string> onMoveChosen;
  public abstract void NotifyToMove(TimeSpan timeRemaining, TimeSpan increment, ClockType clockType);
  public abstract void NotifyGameOver();
  public abstract void NewGame(Board board, AISettings aiSettings, BookLoader bookLoader);

  public enum ClockType{None, PerMove, Regular}
  public virtual void ChoseMove(Move move, string name)
  {
    onMoveChosen?.Invoke(move, name);
  }

}
