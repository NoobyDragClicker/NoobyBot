using System;

public class HumanPlayer : Player
{
    public HumanPlayer(string name)
    {
        this.name = name;
    }

    public override void NewGame(Board board, AISettings aiSettings){}

    public override void NotifyToMove(TimeSpan timeRemaining, TimeSpan increment, ClockType clockType){}

    public override void NotifyGameOver(){}

    public void ChooseSelectedMove(Move move, string name){
        ChoseMove(move, name);
    }
}
