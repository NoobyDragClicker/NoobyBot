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

    public bool hasGameStarted = false;
    public bool hasGameEnded = false;

    const string startingFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    [SerializeField]
    AISettings testSettings = new AISettings(true, true, 9);
    
    [SerializeField]
    AISettings regularSettings = new AISettings(false, false, 9); 

    UIManager uiManager;
    public BoardManager(UIManager uiManager){
        this.uiManager = uiManager;
    }
    
    public void StartGame(bool useClock, int startTime, bool useCustomPos, string customStr, int whitePlayerType, int blackPlayerType){
        this.useClock = useClock;
        if(!useCustomPos){
            //Creates new board
            board = new Board(startingFEN, new MoveGenerator());
            searchBoard = new Board(startingFEN, new MoveGenerator());
        } else{
            board = new Board(customStr, new MoveGenerator());
            searchBoard = new Board(customStr, new MoveGenerator());
        }
        uiManager.UpdateBoard(board);

        AISettings whiteSettings = (whitePlayerType == 2)? testSettings : regularSettings;
        AISettings blackSettings = (blackPlayerType == 2)? testSettings : regularSettings;
        
        //Syncing OnMoveChosen
        whitePlayer = (whitePlayerType == 0) ? new HumanPlayer(startTime, useClock) : new AIPlayer(searchBoard, whiteSettings, startTime, useClock);
        whitePlayer.onMoveChosen += OnMoveChosen;
        blackPlayer = (blackPlayerType == 0) ? new HumanPlayer(startTime, useClock) : new AIPlayer(searchBoard, blackSettings, startTime, useClock);
        blackPlayer.onMoveChosen += OnMoveChosen;
        
        if(board.colorTurn == Piece.Black){playerToMove = blackPlayer;}
        if(board.colorTurn == Piece.White){playerToMove = whitePlayer;}

        hasGameStarted = true;
        playerToMove.NotifyToMove();
    }

    void OnMoveChosen(Move move){
        board.Move(move, false);
        searchBoard.Move(move, true);
        if(board.colorTurn == Piece.Black){playerToMove = blackPlayer;}
        if(board.colorTurn == Piece.White){playerToMove = whitePlayer;}
        //Updates the main board display
        uiManager.UpdateBoard(board);

        //Checking if there is a draw or mate
        if(board.IsCheckmate(board.colorTurn)){
            if(board.colorTurn == Piece.White){EndGame(Piece.Black, false);}
            if(board.colorTurn == Piece.Black){EndGame(Piece.White, false);}
        }
        if(board.IsDraw()){EndGame(0, true);}

        if(!hasGameEnded){
            playerToMove.NotifyToMove();
        }
    }

    public void Update(){
        if(hasGameStarted){
            if(useClock && playerToMove.timeRemaining <= 0f && !hasGameEnded){
                Debug.Log("timed out");
                int winner = (board.colorTurn == Piece.White) ? Piece.Black: Piece.White;
                EndGame(winner, false);
            }
            if(hasGameStarted && !hasGameEnded){playerToMove.Update();}
        }
        
    }
    public void EndGame(int winningColor, bool isDraw){
        Debug.Log("game over");
        hasGameEnded = true;
        if(!isDraw){uiManager.DisplayResult(winningColor);}
        else{uiManager.DisplayResult(0);}

        Debug.Log("Black: ");
        blackPlayer.NotifyGameOver();
        Debug.Log("White: ");
        whitePlayer.NotifyGameOver();
    }

}
