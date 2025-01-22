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
    bool isTurnToMove;
    Search search;
    System.Diagnostics.Stopwatch generatingStopwatch = new System.Diagnostics.Stopwatch();
    System.Diagnostics.Stopwatch makeMoveWatch = new System.Diagnostics.Stopwatch();
    System.Diagnostics.Stopwatch unmakeMoveWatch = new System.Diagnostics.Stopwatch();
    float timeHardCap;



    public AIPlayer(Board board, AISettings aiSettings, float startTime, bool useClock){
        this.board = board;
        this.useClock = useClock;
        if(useClock){timeRemaining = startTime;}

        search = new Search(this.board, generatingStopwatch, makeMoveWatch, unmakeMoveWatch, aiSettings);
        search.onSearchComplete += OnSearchComplete;
        moveFound = false;
    }

    //Allows us to remain synchronous with Unity, and still interact with the board
    public override void Update(){
        if(useClock){
            timeRemaining -= Time.deltaTime;
        }
        if(isTurnToMove && timeRemaining <= timeHardCap){
            search.EndSearch();
        }
        if(moveFound){
            moveFound = false;
            ChoseMove(move);
        }
    }

    //Called when it is our turn to move
    public override void NotifyToMove(){
        timeHardCap = timeRemaining - (timeRemaining / 20);
        moveFound = false;
        isTurnToMove = true;
        Task.Factory.StartNew (() => search.StartSearch(), TaskCreationOptions.LongRunning);
        //search.StartSearch();
    }

    //Called when it is our turn to move
    public override void NotifyGameOver(){
        Debug.Log("Total time generating moves: " + generatingStopwatch.Elapsed);
        Debug.Log("Total time making moves: " + makeMoveWatch.Elapsed);
        Debug.Log("Total time unmaking moves: " + unmakeMoveWatch.Elapsed);
    }

    //Triggered by the onSearchComplete event
    void OnSearchComplete(Move move){
        isTurnToMove = false;
        moveFound = true;
        this.move = move;
    }

}

public struct AISettings{
    public bool useTT;
    public bool useIterativeDeepening;
    public int maxDepth;

    public AISettings(bool useTT, bool useIterativeDeepening, int maxDepth){
        this.useTT = useTT;
        this.useIterativeDeepening  = useIterativeDeepening;
        this.maxDepth = maxDepth;
    }

}
