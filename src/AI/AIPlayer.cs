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

    public TimeSpan hardCap;
    public TimeSpan softCap;
    private CancellationTokenSource hardCapToken;
    private CancellationTokenSource softCapToken;


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
        if (clockType != ClockType.Infinite)
        {
            int millisecondsForHardCap = 100;
            int millisecondsForSoftCap = 100;
            if (clockType == ClockType.Regular)
            {
                millisecondsForHardCap = (int)(timeRemaining.TotalMilliseconds / 2);
                millisecondsForSoftCap = (int)((timeRemaining.TotalMilliseconds / 20) + (increment.TotalSeconds * 500));
            }
            else if (clockType == ClockType.PerMove)
            {
                millisecondsForHardCap = (int)(timeRemaining.TotalMilliseconds * 0.75f);
                millisecondsForSoftCap = millisecondsForHardCap;
            }
            hardCap = TimeSpan.FromMilliseconds(millisecondsForHardCap);
            softCap = TimeSpan.FromMilliseconds(millisecondsForSoftCap);

            hardCapToken = new CancellationTokenSource();
            softCapToken = new CancellationTokenSource();
            // Start monitoring in background
            Task.Run(() => MonitorSoftCap(softCapToken.Token));
            Task.Run(() => MonitorHardCap(hardCapToken.Token));
        }
        logger.startNewSearch();
        Task.Run(() => search.StartSearch(true));
    }

    private async Task MonitorHardCap(CancellationToken token)
    {
        try
        {
            await Task.Delay(hardCap, token);
            search.EndSearch();
        }
        //Expected when search finishes earlier
        catch (TaskCanceledException){}
    }

    private async Task MonitorSoftCap(CancellationToken token)
    {
        try
        {
            await Task.Delay(softCap, token);
            search.TriggerSoftCap();
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
        hardCapToken.Cancel();
        softCapToken.Cancel();
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
