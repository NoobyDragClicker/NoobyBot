using System.Threading.Tasks;
using System.Threading;
using System;
#if UNITY_EDITOR
using UnityEngine;
#endif


public class AIPlayer : Player
{
    Board board;
    OpeningBook openingBook;
    AISettings aiSettings;
    
    public Search search;
    public SearchLogger logger;

    public TimeSpan MoveTimeLimit;
    private CancellationTokenSource moveTimeoutTokenSource;
    public bool isInBook;
    Move[,] killers;
    const string logPath = "C:/Users/Spencer/Desktop/Chess/Logs/";


    public AIPlayer(string name)
    {
        this.name = name;
        logger = new SearchLogger(name, logPath);
    }

    public override void NewGame(Board board, AISettings aiSettings, BookLoader bookLoader)
    {
        this.board = board;
        this.aiSettings = aiSettings;
        killers = new Move[1024, 3];
        search = new Search(this.board, aiSettings, killers, logger);
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
        bool needsSearch = !isInBook;

        if ((board.gameMoveHistory.Count >= aiSettings.openingBookDepth) && isInBook)
        {
            isInBook = false;
            logger.AddToLog("Out of book, depth limit reached: " + board.gameMoveHistory.Count);
            needsSearch = true;
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
                logger.AddToLog($"Out of book, no line found, time remaining: {timeRemaining}");
                needsSearch = true;
            }
        }

        if (needsSearch)
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

                logger.AddToLog($"time for move: {millisecondsForMove}");
                MoveTimeLimit = TimeSpan.FromMilliseconds(millisecondsForMove);

                moveTimeoutTokenSource = new CancellationTokenSource();
                // Start monitoring in background
                Task.Run(() => MonitorMoveTime(moveTimeoutTokenSource.Token));
            }
            Task.Run(() => search.StartSearch());
        }
        
    }

    private async Task MonitorMoveTime(CancellationToken token)
    {
        try
        {
            await Task.Delay(MoveTimeLimit, token);
            logger.AddToLog("Time ran out");
            search.EndSearch();
        }
        //Expected when search finishes earlier
        catch (TaskCanceledException){}
    }

    public override void NotifyGameOver()
    {
        search.EndSearch();
        search.tt.DeleteEntries();
    }

    //Triggered by the onSearchComplete event, makes move
    void OnSearchComplete(Move move)
    {
        if (!isInBook)
        {
            moveTimeoutTokenSource.Cancel();
        }
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
