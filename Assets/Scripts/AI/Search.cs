using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Search
{
    Board board;
    Move bestMove;
    Move bestMoveThisIteration;
    Evaluation evaluation;
    int bestEval;
    //Debug
    ulong bottomNodesSearched;
    ulong prunedTimes;


    const int positiveInfinity = 99999;
    const int negativeInfinity = -99999;
    const int checkmate = -99999;
    public event Action<Move> onSearchComplete;
    System.Diagnostics.Stopwatch generatingStopwatch = new System.Diagnostics.Stopwatch();

    public Search(Board board){
        this.board = board;
        evaluation = new Evaluation();
    }

    public void StartSearch(){
        //Init a bunch of stuff, iterative deepening, etc
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        bestEval = SearchMoves(5, 0, negativeInfinity, positiveInfinity);
        Debug.Log(stopwatch.Elapsed);
        stopwatch.Stop();  
        Debug.Log("Bottom nodes searched: " + bottomNodesSearched); 
        Debug.Log("Best eval: " + bestEval);
        Debug.Log("Time spent evaluating: " + evaluation.stopwatch.Elapsed);
        Debug.Log("Time spent generating moves: " + generatingStopwatch.Elapsed);
        bestMove = bestMoveThisIteration;

        onSearchComplete?.Invoke(bestMove);
    }
    
    int SearchMoves(int depth, int plyFromRoot, int alpha, int beta){
        //Returns the actual eval of the position
        if(depth <= 0){
            bottomNodesSearched ++;
            return evaluation.EvaluatePosition(board);
        }

        List<Move> legalMoves = board.moveGenerator.GenerateLegalMoves(board, board.colorTurn);
        
        //Check for mate or stalemate
        if(legalMoves.Count == 0){
            if(board.isCurrentPlayerInCheck){
                return checkmate;
            } else {
                return 0;
            }
        }

        for(int i = 0; i<legalMoves.Count; i++){
            //Debug.Log(depth);
            board.Move(legalMoves[i], true);
            generatingStopwatch.Start();
            int eval = -SearchMoves(depth - 1, plyFromRoot + 1, -beta, -alpha);
            generatingStopwatch.Stop();
            board.UndoMove(legalMoves[i]);

            //Move is too good, would be prevented by a previous move
            if(eval >= beta){
                prunedTimes ++;
                return beta;
            }
            //This move is better than the current move
            if(eval > alpha){
                alpha = eval;
                //If this is a root move, set it to the best move
                if(plyFromRoot == 0){
                    bestMoveThisIteration = legalMoves[i];
                }
            }
        }
        return alpha;
    }
}
