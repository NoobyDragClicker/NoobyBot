using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Search
{
    Board board;
    Move bestMove;
    int bestEval;
    const int positiveInfinity = 99999;
    const int negativeInfinity = -99999;
    public event Action<Move> onSearchComplete;
    System.Random rnd;

    public Search(Board board){
        this.board = board;
        rnd = new System.Random();
    }

    public void StartSearch(){
        //Init a bunch of stuff, iterative deepening, etc
        bestEval = SearchMoves(3, 0, negativeInfinity, positiveInfinity);
    }
    
    int SearchMoves(int depth, int plyFromRoot, int alpha, int beta){
        List<Move> legalMoves = board.moveGenerator.GenerateLegalMoves(board, board.colorTurn);

        
        //Random
        bestMove = legalMoves[rnd.Next(0, legalMoves.Count - 1)];
        onSearchComplete?.Invoke (bestMove);
        return 0;
    }
}
