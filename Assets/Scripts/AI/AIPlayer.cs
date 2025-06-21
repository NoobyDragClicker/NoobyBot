using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System;


public class AIPlayer : Player
{
    Board board;
    OpeningBook openingBook;
    AISettings aiSettings;
    bool isInBook = true;
    Search search;
    Stopwatch generatingStopwatch = new Stopwatch();
    Stopwatch makeMoveWatch = new Stopwatch();
    Stopwatch unmakeMoveWatch = new Stopwatch();

    
    public TimeSpan MoveTimeLimit { get; private set; }
    private CancellationTokenSource moveTimeoutTokenSource;


    public AIPlayer(Board board, AISettings aiSettings, BookLoader bookLoader, int startTime, int incrementMS, bool useClock, string name)
    {
        //Keeps track of the total time remaining
        TotalTimeRemaining = TimeSpan.FromSeconds(startTime);
        //Keeps track of the current move time
        moveStopwatch = new Stopwatch();

        this.name = name;
        this.board = board;
        this.useClock = useClock;
        increment = TimeSpan.FromMilliseconds(incrementMS);
        this.aiSettings = aiSettings;

        search = new Search(this.board, generatingStopwatch, makeMoveWatch, unmakeMoveWatch, aiSettings);
        search.onSearchComplete += OnSearchComplete;

        if (aiSettings.openingBookDepth > 0)
        {
            openingBook = new OpeningBook(bookLoader);
        }
    }

    //Called when it is our turn to move
    public override void NotifyToMove()
    {
        int millisecondsForMove = (int)((TotalTimeRemaining.TotalMilliseconds / 20) + (increment.TotalSeconds * 500));
        MoveTimeLimit = TimeSpan.FromMilliseconds(millisecondsForMove);

        moveStopwatch.Restart();

        moveTimeoutTokenSource = new CancellationTokenSource();

        // Start monitoring in background
        Task.Run(() => MonitorMoveTime(moveTimeoutTokenSource.Token));

        if ((board.gameMoveHistory.Count >= aiSettings.openingBookDepth) && isInBook)
        {
            isInBook = false;
            if (aiSettings.sayMaxDepth)
            {
                UnityEngine.Debug.Log("Out of book");
            }
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
                if (aiSettings.sayMaxDepth)
                {
                    UnityEngine.Debug.Log("Out of book");
                }
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
        moveStopwatch.Stop();
        moveTimeoutTokenSource.Cancel();
        TotalTimeRemaining -= moveStopwatch.Elapsed;
        TotalTimeRemaining += increment;
        ChoseMove(move, name);
    }

}

public struct AISettings{
    public int maxDepth;
    public int openingBookDepth;
    public bool useTT;
    public bool useQuiescence;
    public bool useSearchExtensions;
    public bool useRandomTest;
    public bool sayMaxDepth;


    public AISettings(bool useTT, int maxDepth, int openingBookDepth, bool useQuiescence, bool useSearchExtensions, bool useRandomTest, bool sayMaxDepth){
        this.useTT = useTT;
        this.openingBookDepth = openingBookDepth;
        this.maxDepth = maxDepth;
        this.useQuiescence = useQuiescence;
        this.useSearchExtensions = useSearchExtensions;
        this.useRandomTest = useRandomTest;
        this.sayMaxDepth = sayMaxDepth;
    }

}
