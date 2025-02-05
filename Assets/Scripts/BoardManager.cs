using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;


public class BoardManager
{
    //Main board for the game
    public Board board;
    public Board searchBoard;
    bool useClock;

    Player whitePlayer;
    Player blackPlayer;
    public Player playerToMove;

    ResultStatus result;
    public GameStatus gameStatus;

    const string startingFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public enum ResultStatus {Draw, White_Won, Black_Won}
    public enum GameStatus{PreGame, Playing, Finished}

    public Action<ResultStatus, int> gameFinished;
    public Action<int> moveMade;

    public int boardNumber;
    public int numMismatchesFound = 0;

    public BoardManager(int boardNumber){this.boardNumber = boardNumber;}
    
    public void StartGame(bool useClock, int startTime, int increment, bool useCustomPos, string customStr, AISettings whiteSettings, AISettings blackSettings, bool whiteHuman = false, bool blackHuman = false){
        gameStatus = GameStatus.PreGame;
        this.useClock = useClock;
        if(!useCustomPos){
            //Creates new board
            board = new Board(startingFEN, new MoveGenerator());
            searchBoard = new Board(startingFEN, new MoveGenerator());
        } else{
            board = new Board(customStr, new MoveGenerator());
            searchBoard = new Board(customStr, new MoveGenerator());
        }
        
        //Syncing OnMoveChosen
        whitePlayer = whiteHuman ? new HumanPlayer(startTime, useClock) : new AIPlayer(searchBoard, whiteSettings, startTime, increment, useClock);
        whitePlayer.onMoveChosen += OnMoveChosen;
        blackPlayer = blackHuman ? new HumanPlayer(startTime, useClock) : new AIPlayer(searchBoard, blackSettings, startTime, increment, useClock);
        blackPlayer.onMoveChosen += OnMoveChosen;
        
        if(board.colorTurn == Piece.Black){playerToMove = blackPlayer;}
        if(board.colorTurn == Piece.White){playerToMove = whitePlayer;}
        
        gameStatus = GameStatus.Playing;
        playerToMove.NotifyToMove();
        moveMade.Invoke(boardNumber);
    }

    void OnMoveChosen(Move move){
        MoveGenerator moveGenerator = new MoveGenerator();
        List<Move> moves = moveGenerator.GenerateLegalMoves(board, board.colorTurn);
        int value = move.GetIntValue();
        bool isLegal = false;
        foreach(Move legalMove in moves){
            if(legalMove.GetIntValue() == value){
                isLegal = true;
            }
        }
        board.Move(move, false);
        searchBoard.Move(move, true);
        
        if(isLegal == false){
            Debug.Log("Illegal move attempted, board " + boardNumber);
        }

        if(searchBoard.zobristKey != board.zobristKey){
            Debug.Log("mismatch, board " + boardNumber);
        }
        //Checking if there is a draw or mate
        if(board.IsCheckmate(board.colorTurn)){
            if(board.colorTurn == Piece.White){EndGame(ResultStatus.Black_Won);}
            if(board.colorTurn == Piece.Black){EndGame(ResultStatus.White_Won);}
        }
        if(board.IsDraw()){EndGame(ResultStatus.Draw);}

        if(gameStatus == GameStatus.Playing){
            if(board.colorTurn == Piece.Black){playerToMove = blackPlayer;}
            if(board.colorTurn == Piece.White){playerToMove = whitePlayer;}
            playerToMove.NotifyToMove();
        }
        
        //Updates the main board display
        moveMade.Invoke(boardNumber);
        return;
    }
    public void Update(){
        if(gameStatus == GameStatus.Playing){
            if(useClock && playerToMove.timeRemaining <= 0f){
                ResultStatus resultStatus = (board.colorTurn == Piece.White) ? ResultStatus.Black_Won: ResultStatus.White_Won;
                EndGame(resultStatus);
            }
            if(gameStatus == GameStatus.Playing){playerToMove.Update();}
        }
    }
    public void EndGame(ResultStatus resultStatus){
        gameStatus = GameStatus.Finished;
        result = resultStatus;
        blackPlayer.NotifyGameOver();
        whitePlayer.NotifyGameOver();
        gameFinished.Invoke(result, boardNumber);
        return;
    }


}
