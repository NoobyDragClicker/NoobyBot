#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;


public class BoardManager
{
    //Main board for the game
    public Board board = new Board();
    public Board searchBoard = new Board();

    Player whitePlayer;
    Player blackPlayer;
    public Player playerToMove;

    ResultStatus result;
    public GameStatus gameStatus;

    public enum ResultStatus { Draw, White_Won, Black_Won }
    public enum GameStatus { PreGame, Playing, Finished }

    public Action<ResultStatus, int> gameFinished;
    public Action<int> moveMade;

    public int boardNumber;
    BookLoader bookLoader;


    Player.ClockType clockType;
    TimeSpan increment;
    Stopwatch makeMoveWatch = new Stopwatch();
    public TimeSpan whiteTimeRemaining;
    public TimeSpan blackTimeRemaining;

    public BoardManager(int boardNumber, BookLoader bookLoader)
    {
        this.boardNumber = boardNumber;
        this.bookLoader = bookLoader;
    }

    public void StartGame(Player.ClockType clockType, int startTime, int incrementMS, bool useCustomPos, string customStr, AISettings whiteSettings, AISettings blackSettings, int gameNumber, bool whiteHuman = false, bool blackHuman = false)
    {
        Engine engine = new Engine();
        engine.ReceiveCommand("ucinewgame");
        try
        {
            engine.ReceiveCommand("position startpos");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e.Message);
        }
        

        gameStatus = GameStatus.PreGame;
        this.clockType = clockType;

        if (!useCustomPos)
        {
            board.setPosition(Board.startPos, new MoveGenerator());
            searchBoard.setPosition(Board.startPos, new MoveGenerator());
        }
        else
        {
            board.setPosition(customStr, new MoveGenerator());
            searchBoard.setPosition(customStr, new MoveGenerator());
        }

        whiteTimeRemaining = TimeSpan.FromSeconds(startTime);
        blackTimeRemaining = TimeSpan.FromSeconds(startTime);
        TimeSpan currentPlayerTime = TimeSpan.FromSeconds(startTime);

        increment = TimeSpan.FromMilliseconds(incrementMS);


        //Syncing OnMoveChosen
        if (whitePlayer == null) { whitePlayer = whiteHuman ? new HumanPlayer("White Player") : new AIPlayer("White Bot: " + gameNumber.ToString()); }

        whitePlayer.NewGame(searchBoard, whiteSettings, bookLoader);
        whitePlayer.onMoveChosen += OnMoveChosen;

        if (blackPlayer == null) { blackPlayer = blackHuman ? new HumanPlayer("Black Player") : new AIPlayer("Black Bot: " + gameNumber.ToString()); }

        blackPlayer.NewGame(searchBoard, blackSettings, bookLoader);
        blackPlayer.onMoveChosen += OnMoveChosen;


        if (board.colorTurn == Piece.Black) { playerToMove = blackPlayer; }
        if (board.colorTurn == Piece.White) { playerToMove = whitePlayer; }

        gameStatus = GameStatus.Playing;
        makeMoveWatch.Restart();
        playerToMove.NotifyToMove(currentPlayerTime, increment, clockType);
        moveMade.Invoke(boardNumber);
    }

    void OnMoveChosen(Move move, string name)
    {
        MoveGenerator moveGenerator = new MoveGenerator();
        List<Move> moves = moveGenerator.GenerateLegalMoves(board, board.colorTurn);
        int value = move.GetIntValue();
        bool isLegal = false;
        foreach (Move legalMove in moves)
        {
            if (legalMove.GetIntValue() == value)
            {
                isLegal = true;
                break;
            }
        }

        board.Move(move, false);
        searchBoard.Move(move, true);

        if (isLegal == false)
        {
            //Debug.Log("Illegal move attempted, board " + boardNumber);
            GameLogger.LogGame(board, boardNumber);
        }

        if (searchBoard.zobristKey != board.zobristKey)
        {
            //Debug.Log("mismatch, board " + boardNumber);
        }
        //Checking if there is a draw or mate
        if (board.IsCheckmate(board.colorTurn))
        {
            if (board.colorTurn == Piece.White) { EndGame(ResultStatus.Black_Won); }
            if (board.colorTurn == Piece.Black) { EndGame(ResultStatus.White_Won); }
        }

        if (board.IsDraw()) { EndGame(ResultStatus.Draw); }

        if (gameStatus == GameStatus.Playing)
        {
            if (board.colorTurn == Piece.Black)
            {
                playerToMove = blackPlayer;

                whiteTimeRemaining -= makeMoveWatch.Elapsed;
                whiteTimeRemaining += increment;
            }
            if (board.colorTurn == Piece.White)
            {
                playerToMove = whitePlayer;
                blackTimeRemaining -= makeMoveWatch.Elapsed;
                blackTimeRemaining += increment;
            }

            if (whiteTimeRemaining <= TimeSpan.Zero)
            {
                EndGame(ResultStatus.Black_Won);
                UnityEngine.Debug.Log("Timed out");
            }
            else if (blackTimeRemaining <= TimeSpan.Zero)
            {
                EndGame(ResultStatus.White_Won);
                UnityEngine.Debug.Log("Timed out");
            }
            else
            {
                makeMoveWatch.Restart();
                TimeSpan currentPlayerTimeRemaining = (playerToMove == whitePlayer) ? whiteTimeRemaining : blackTimeRemaining;
                playerToMove.NotifyToMove(currentPlayerTimeRemaining, increment, clockType);
            }
        }

        //Updates the main board display
        moveMade.Invoke(boardNumber);
    }
    public void EndGame(ResultStatus resultStatus)
    {
        gameStatus = GameStatus.Finished;
        result = resultStatus;
        blackPlayer.NotifyGameOver();
        whitePlayer.NotifyGameOver();
        blackPlayer.onMoveChosen -= OnMoveChosen;
        whitePlayer.onMoveChosen -= OnMoveChosen;
        gameFinished.Invoke(result, boardNumber);
    }

    public int getWhiteMSRemaining()
    {
        if (playerToMove == whitePlayer) {return (int) (whiteTimeRemaining - makeMoveWatch.Elapsed).TotalMilliseconds;}
        else{ return (int)whiteTimeRemaining.TotalMilliseconds; }
    }
    public int getBlackMSRemaining()
    {
        if (playerToMove == blackPlayer) {return (int) (blackTimeRemaining - makeMoveWatch.Elapsed).TotalMilliseconds;}
        else{ return (int)blackTimeRemaining.TotalMilliseconds; }
    }
}
#endif