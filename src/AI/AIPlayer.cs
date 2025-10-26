using System.Threading.Tasks;
using System.Threading;
using System;


public class AIPlayer : Player
{
    Board board;
    AISettings aiSettings;

    public Search search;
    public SearchLogger logger;
    public SearchDiagnostics currentDiagnostics;

    public TimeSpan MoveTimeLimit;
    private CancellationTokenSource moveTimeoutTokenSource;


    public AIPlayer(string name, SearchLogger logger)
    {
        this.name = name;
        this.logger = logger;
    }

    public override void NewGame(Board board, AISettings aiSettings)
    {
        this.board = board;
        this.aiSettings = aiSettings;
        search = new Search(board, aiSettings, logger);
        search.onSearchComplete += OnSearchComplete;
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
        logger.startNewSearch();
        Task.Run(() => search.StartSearch(true));
    }

    private async Task MonitorMoveTime(CancellationToken token)
    {
        try
        {
            await Task.Delay(MoveTimeLimit, token);
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
        moveTimeoutTokenSource.Cancel();
        try
        {
            logger.logSingleSearch();
        }
        catch (Exception e)
        {
            logger.AddToLog(e.Message, SearchLogger.LoggingLevel.Warning);
        }
        
        ChoseMove(move, name);
    }

}

public struct AISettings{
    public int maxDepth;
    public int ttSize;
    public int openingBookDepth;

    public AISettings(int maxDepth, int openingBookDepth, int ttSize)
    {
        this.openingBookDepth = openingBookDepth;
        this.maxDepth = maxDepth;
        this.ttSize = ttSize;
    }

}
