using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class AIPlayer : Player
{
    Board board;
    Move move;
    bool moveFound;
    Search search;


    public AIPlayer(Board board){
        this.board = board;
        search = new Search(this.board);
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
        search.StartSearch();
    }

    //Triggered by the onSearchComplete event
    void OnSearchComplete(Move move){
        moveFound = true;
        this.move = move;
    }

}
