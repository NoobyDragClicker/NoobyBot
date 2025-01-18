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
    TranspositionTable tt;
    int bestEval;
    //Debug
    ulong bottomNodesSearched;
    ulong prunedTimes;
    ulong ttUsed;

    bool useMoveSorting;
    const int positiveInfinity = 99999;
    const int negativeInfinity = -99999;
    const int checkmate = -99998;
    public event Action<Move> onSearchComplete;
    Stopwatch generatingStopwatch;
    Stopwatch makeMoveWatch;
    Stopwatch unmakeMoveWatch;

    public Search(Board board, bool useTestFeature, Stopwatch genStopwatch, Stopwatch makeMoveWatch, Stopwatch unmakeMoveStopwatch){
        this.board = board;
        evaluation = new Evaluation();
        tt = new TranspositionTable(board, 512);
        useMoveSorting = useTestFeature;
        moveOrder = new MoveOrder();
        generatingStopwatch = genStopwatch;
        this.makeMoveWatch = makeMoveWatch;
        this.unmakeMoveWatch = unmakeMoveStopwatch;
    }

    public void StartSearch(){
        //Init a bunch of stuff, iterative deepening, etc
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        bestEval = SearchMoves(6, 0, negativeInfinity, positiveInfinity);
        stopwatch.Stop();
        /*UnityEngine.Debug.Log("Total time: " + stopwatch.Elapsed);
        UnityEngine.Debug.Log("Transposition used: " + ttUsed);
        UnityEngine.Debug.Log("Transpositions stored: " + tt.numStored);
        UnityEngine.Debug.Log("Percent used: " + ((float) tt.numStored/tt.count * 100));*/
        UnityEngine.Debug.Log("Best eval: " + bestEval);

        bestMove = bestMoveThisIteration;
        onSearchComplete?.Invoke(bestMove);
    }
    
    int SearchMoves(int depth, int plyFromRoot, int alpha, int beta){
        if(board.IsRepetitionDraw()){return 0;}
        if(board.fiftyMoveCounter >= 100){return 0;}

        int ttEval = tt.LookupEvaluation(depth, plyFromRoot, alpha, beta);
        //TT score found
        if(ttEval != TranspositionTable.LookupFailed){
            ttUsed++;
            //Set the best move
            if(plyFromRoot == 0){
                bestMoveThisIteration = tt.GetStoredMove();
            }
            return ttEval;
        }
        
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

        int evaluationBound = TranspositionTable.UpperBound;
        Move bestMoveInThisPosition = null;

        for(int i = 0; i<legalMoves.Count; i++){
            makeMoveWatch.Start();
            board.Move(legalMoves[i], true);
            makeMoveWatch.Stop();


            generatingStopwatch.Start();
            int eval = -SearchMoves(depth - 1, plyFromRoot + 1, -beta, -alpha);
            generatingStopwatch.Stop();

            unmakeMoveWatch.Start();
            board.UndoMove(legalMoves[i]);
            unmakeMoveWatch.Stop();

            //Move is too good, would be prevented by a previous move
            if(eval >= beta){
                //Exiting search early, so it is a lower bound
                tt.StoreEvaluation(depth, plyFromRoot, beta, TranspositionTable.LowerBound, legalMoves[i]);
                prunedTimes ++;
                return beta;
            }
            //This move is better than the current move
            if(eval > alpha){
                evaluationBound = TranspositionTable.Exact;
                bestMoveInThisPosition = legalMoves[i];
                alpha = eval;
                //If this is a root move, set it to the best move
                if(plyFromRoot == 0){
                    bestMoveThisIteration = legalMoves[i];
                    UnityEngine.Debug.Log(Coord.GetMoveNotation(bestMoveThisIteration.oldIndex, bestMoveThisIteration.newIndex));
                }
            }
        }

        tt.StoreEvaluation(depth, plyFromRoot, alpha, evaluationBound, bestMoveInThisPosition);
        return alpha;
    }

    public static bool IsMateScore(int score){
        const int maxMatePly = 150;
        return Math.Abs(score) > positiveInfinity - maxMatePly;
    }
}
