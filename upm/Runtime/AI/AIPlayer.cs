using System.Threading.Tasks;
using System.Threading;
using System;


public class AIPlayer : Player
{
    Board board;
    OpeningBook openingBook;
    AISettings aiSettings;

    public Search search;
    public SearchLogger logger;
    public SearchDiagnostics currentDiagnostics;

    public TimeSpan MoveTimeLimit;
    private CancellationTokenSource moveTimeoutTokenSource;
    public bool isInBook;
    Move[,] killers;
    int[,] history;


    public AIPlayer(string name, SearchLogger logger)
    {
        this.name = name;
        this.logger = logger;
    }

    public override void NewGame(Board board, AISettings aiSettings, BookLoader bookLoader)
    {
        this.board = board;
        this.aiSettings = aiSettings;
        killers = new Move[1024, 3];
        history = new int[64, 64];
        search = new Search(board, aiSettings, killers, history, logger);
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
            needsSearch = true;
        }

        if (aiSettings.openingBookDepth > 0 && isInBook)
        {
            Move openingBookMove = openingBook.getBookMove(board);
            if (!openingBookMove.isNull())
            {
                OnSearchComplete(openingBookMove);
            }
            else
            {
                isInBook = false;
                logger.AddToLog($"Out of book, no line found, time remaining: {timeRemaining}", SearchLogger.LoggingLevel.Info);
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
                MoveTimeLimit = TimeSpan.FromMilliseconds(millisecondsForMove);

                moveTimeoutTokenSource = new CancellationTokenSource();
                // Start monitoring in background
                Task.Run(() => MonitorMoveTime(moveTimeoutTokenSource.Token));
            }
            logger.startNewSearch();
            Task.Run(() => search.StartSearch(true));
        }

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
        if (!isInBook)
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
