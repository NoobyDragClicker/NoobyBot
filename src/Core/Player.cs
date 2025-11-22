using System;

public abstract class Player
{
  public string name = "";
  public event Action<Move, string> onMoveChosen;
  public abstract void NotifyToMove(TimeSpan timeRemaining, TimeSpan increment, ClockType clockType);
  public abstract void NotifyGameOver();
  public abstract void NewGame(Board board, AISettings aiSettings);

  public enum ClockType{PerMove, Regular, Infinite}
  public virtual void ChoseMove(Move move, string name)
  {
    onMoveChosen?.Invoke(move, name);
  }

}
