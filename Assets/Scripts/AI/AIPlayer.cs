using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using System.Threading.Tasks;
using System.Data;
using System;
using System.Runtime.InteropServices;

public class AIPlayer : Player
{
    Board board;
    Move move;
    OpeningBook openingBook;
    BookLoader bookLoader;
    AISettings aiSettings;
    bool moveFound;
    bool isTurnToMove = false;
    bool isInBook = true;
    Search search;
    System.Diagnostics.Stopwatch generatingStopwatch = new System.Diagnostics.Stopwatch();
    System.Diagnostics.Stopwatch makeMoveWatch = new System.Diagnostics.Stopwatch();
    System.Diagnostics.Stopwatch unmakeMoveWatch = new System.Diagnostics.Stopwatch();
    float timeHardCap;



    public AIPlayer(Board board, AISettings aiSettings, BookLoader bookLoader, float startTime, int increment, bool useClock, string name){
        this.name = name;
        this.board = board;
        this.bookLoader = bookLoader;
        this.useClock = useClock;
        this.increment = increment;
        this.aiSettings = aiSettings;
        if(useClock){timeRemaining = startTime;}

        search = new Search(this.board, generatingStopwatch, makeMoveWatch, unmakeMoveWatch, aiSettings);
        search.onSearchComplete += OnSearchComplete;
        if(aiSettings.openingBookDepth > 0){
            openingBook = new OpeningBook(bookLoader);
        }
        moveFound = false;
    }

    //Allows us to remain synchronous with Unity, and still interact with the board
    public override void Update(){
        if(moveFound){
            moveFound = false;
            isTurnToMove = false;
            ChoseMove(move, name);
            timeRemaining += increment;
        }
        if(useClock){
            timeRemaining -= Time.deltaTime;
        }
        if(isTurnToMove && timeRemaining <= timeHardCap){
            search.EndSearch();
        }
    }

    //Called when it is our turn to move
    public override void NotifyToMove(){
        timeHardCap = timeRemaining - ((timeRemaining / 20) + (increment/2));
        moveFound = false;
        isTurnToMove = true;
        if(board.gameMoveHistory.Count >= aiSettings.openingBookDepth && isInBook){
            isInBook = false;
            if (aiSettings.sayMaxDepth){
                Debug.Log("Out of book");
            }
        }
        if(aiSettings.openingBookDepth > 0 && isInBook) {
            Move openingBookMove = openingBook.getBookMove(board);
            if(openingBookMove != null){
                OnSearchComplete(openingBookMove);
            } else{
                isInBook = false;
                if (aiSettings.sayMaxDepth){
                    Debug.Log("Out of book");
                }
                Task.Factory.StartNew (() => search.StartSearch(), TaskCreationOptions.LongRunning);
            }

        } else{
            Task.Factory.StartNew (() => search.StartSearch(), TaskCreationOptions.LongRunning);
        } 
    }

    //Called when it is our turn to move
    public override void NotifyGameOver(){
        //Debug.Log("Total time generating moves: " + generatingStopwatch.Elapsed);
        /*Debug.Log("Total time making moves: " + makeMoveWatch.Elapsed);
        Debug.Log("Total time unmaking moves: " + unmakeMoveWatch.Elapsed);*/
        search.EndSearch();
        isTurnToMove = false;
        search.onSearchComplete -= OnSearchComplete;
        search.tt.DeleteEntries();
        search.tt = null;
        search = null;
        GC.Collect();
    }

    //Triggered by the onSearchComplete event
    void OnSearchComplete(Move move){
        isTurnToMove = false;
        this.move = move;
        moveFound = true;
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
