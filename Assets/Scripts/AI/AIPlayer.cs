using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using System.Threading.Tasks;

public class AIPlayer : Player
{
    Board board;
    Move move;
    bool moveFound;
    Search search;
    System.Diagnostics.Stopwatch generatingStopwatch = new System.Diagnostics.Stopwatch();


    public AIPlayer(Board board, bool useTestFeature){
        this.board = board;
        search = new Search(this.board, useTestFeature, generatingStopwatch);
        search.onSearchComplete += OnSearchComplete;
        moveFound = false;
    }

    //Allows us to remain synchronous with Unity, and still interact with the board
    public override void Update(){
        if(moveFound){
            moveFound = false;
            ChoseMove(move);
        }
    }

    //Called when it is our turn to move
    public override void NotifyToMove(){
        moveFound = false;
        Task.Factory.StartNew (() => search.StartSearch (), TaskCreationOptions.LongRunning);
        //search.StartSearch();
    }
    
    //Called when it is our turn to move
    public override void NotifyGameOver(){
        Debug.Log("Total time generating moves: " + generatingStopwatch.Elapsed);
    }

    //Triggered by the onSearchComplete event
    void OnSearchComplete(Move move){
        moveFound = true;
        this.move = move;
    }

}
