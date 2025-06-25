using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System;
using System.Collections.Generic;


public class AIPlayer : Player
{
    Board board;
    OpeningBook openingBook;
    AISettings aiSettings;
    
    Search search;

    public TimeSpan MoveTimeLimit;
    private CancellationTokenSource moveTimeoutTokenSource;
    public bool isInBook;
    Move[,] killers;


    public AIPlayer(string name)
    {
        this.name = name;
    }

    public override void NewGame(Board board, AISettings aiSettings, BookLoader bookLoader)
    {
        this.board = board;
        this.aiSettings = aiSettings;
        killers = new Move[1024, 3];
        search = new Search(this.board, aiSettings, killers);
        search.onSearchComplete += OnSearchComplete;

        if (aiSettings.openingBookDepth > 0)
        {
            ResetOpeningBook(bookLoader);
        }
    }

    public void ResetOpeningBook(BookLoader bookLoader)
    {
        openingBook = new OpeningBook(bookLoader);
        isInBook = true;
    }
    //Called when it is our turn to move
    public override void NotifyToMove(TimeSpan timeRemaining, TimeSpan increment, ClockType clockType)
    {
        if (clockType != ClockType.None)
        {
            int millisecondsForMove = 100;
            if (clockType == ClockType.Regular)
            {
                millisecondsForMove = (int)((timeRemaining.TotalMilliseconds / 20) + (increment.TotalSeconds * 500));
            }
            else if (clockType == ClockType.PerMove)
            {
                millisecondsForMove = (int)(timeRemaining.TotalMilliseconds * 0.75f);
            }

            MoveTimeLimit = TimeSpan.FromMilliseconds(millisecondsForMove);
            moveTimeoutTokenSource = new CancellationTokenSource();

            // Start monitoring in background
            Task.Run(() => MonitorMoveTime(moveTimeoutTokenSource.Token));
        }

        if ((board.gameMoveHistory.Count >= aiSettings.openingBookDepth) && isInBook)
        {
            isInBook = false;
            Engine.LogToFile("Out of book, depth limit reached: " + board.gameMoveHistory.Count);
        }

        if (aiSettings.openingBookDepth > 0 && isInBook)
        {
            Move openingBookMove = openingBook.getBookMove(board);
            if (openingBookMove != null)
            {
                OnSearchComplete(openingBookMove);
            }
            else
            {
                isInBook = false;
                Engine.LogToFile("Out of book, no line found");
                Task.Factory.StartNew(() => search.StartSearch(), TaskCreationOptions.LongRunning);
            }
        }
        else
        {
            Task.Factory.StartNew(() => search.StartSearch(), TaskCreationOptions.LongRunning);
        }
    }

    private async Task MonitorMoveTime(CancellationToken token)
    {
        try
        {
            await Task.Delay(MoveTimeLimit, token);
            if (!token.IsCancellationRequested)
            {
                search.EndSearch();
            }
        }
        catch (TaskCanceledException) { }
    }

    public override void NotifyGameOver()
    {
        search.EndSearch();
        search.onSearchComplete -= OnSearchComplete;
        search.tt.DeleteEntries();
    }

    //Triggered by the onSearchComplete event, makes move
    void OnSearchComplete(Move move)
    {
        moveTimeoutTokenSource.Cancel();
        ChoseMove(move, name);
    }

}

public struct AISettings{
    public int maxDepth;
    public int maxSearchExtensionDepth;
    public int openingBookDepth;
    public bool sayMaxDepth;

    public AISettings(int maxDepth, int maxSearchExtensionDepth, int openingBookDepth, bool sayMaxDepth){
        this.openingBookDepth = openingBookDepth;
        this.maxDepth = maxDepth;
        this.maxSearchExtensionDepth = maxSearchExtensionDepth;
        this.sayMaxDepth = sayMaxDepth;
    }

}
