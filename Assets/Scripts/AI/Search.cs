using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Search
{
    Board board;
    Move bestMove;
    Move bestMoveThisIteration;
    Evaluation evaluation;
    MoveOrder moveOrder;
    int bestEval;
    //Debug
    ulong bottomNodesSearched;
    ulong prunedTimes;

    bool useMoveSorting;
    const int positiveInfinity = 99999;
    const int negativeInfinity = -99999;
    const int checkmate = -99998;
    public event Action<Move> onSearchComplete;
    Stopwatch generatingStopwatch;

    public Search(Board board, bool useTestFeature, Stopwatch genStopwatch){
        this.board = board;
        evaluation = new Evaluation();
        useMoveSorting = useTestFeature;
        moveOrder = new MoveOrder();
        generatingStopwatch = genStopwatch;
    }

    public void StartSearch(){
        //Init a bunch of stuff, iterative deepening, etc
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        bestEval = SearchMoves(6, 0, negativeInfinity, positiveInfinity);
        UnityEngine.Debug.Log("Total time: " + stopwatch.Elapsed);
        stopwatch.Stop();

        bestMove = bestMoveThisIteration;
        onSearchComplete?.Invoke(bestMove);
    }
    
    int SearchMoves(int depth, int plyFromRoot, int alpha, int beta){
        if(board.IsRepetitionDraw()){return 0;}
        if(board.fiftyMoveCounter >= 100){return 0;}
        
        //Returns the actual eval of the position
        if(depth <= 0){
            bottomNodesSearched ++;
            return evaluation.EvaluatePosition(board);
        }

        List<Move> legalMoves = board.moveGenerator.GenerateLegalMoves(board, board.colorTurn);

        //Check for mate or stalemate
        if(legalMoves.Count == 0){
            if(board.isCurrentPlayerInCheck){
                return checkmate + plyFromRoot;
            } else {
                return 0;
            }
        }
        
        if(useMoveSorting){ legalMoves = moveOrder.OrderMoves(board, legalMoves);}
        for(int i = 0; i<legalMoves.Count; i++){
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
