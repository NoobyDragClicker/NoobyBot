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
    Move bestMove = null;
    Move bestMoveThisIteration;
    int bestEvalThisIteration;
    Evaluation evaluation;
    MoveOrder moveOrder;
    public TranspositionTable tt;

    AISettings aiSettings;
    int bestEval;

    bool abortSearch = false;
    const int positiveInfinity = 99999;
    const int negativeInfinity = -99999;
    const int checkmate = -99998;
    public event Action<Move> onSearchComplete;

    public Search(Board board, Stopwatch genStopwatch, Stopwatch makeMoveStopwatch, Stopwatch unmakeMoveStopwatch, AISettings aiSettings){
        this.board = board;
        this.aiSettings = aiSettings;
        evaluation = new Evaluation();
        tt = new TranspositionTable(board, 512);
        moveOrder = new MoveOrder();
    }

    public void StartSearch(){
        bestMove = null;
        abortSearch = false;
        bestEval = StartIterativeDeepening(aiSettings.maxDepth);
        if(bestMove == null){
            bestMove = board.moveGenerator.GenerateLegalMoves(board, board.colorTurn)[0];
        }
        onSearchComplete?.Invoke(bestMove);
    }
    int StartIterativeDeepening(int maxDepth){
        ulong startKey = board.zobristKey;
        for(int depth = 1; depth <= maxDepth; depth++){
            SearchMoves(depth, 0, negativeInfinity, positiveInfinity, 0);
            if(bestMoveThisIteration != null){
                bestMove = bestMoveThisIteration;
                bestEval = bestEvalThisIteration;
            }
            
            if(board.zobristKey !=startKey && depth == 1){
                GameLogger.LogGame(board, 0);
                UnityEngine.Debug.Log("Asynced at depth: " + depth);
            }

            if(abortSearch){
                if(aiSettings.sayMaxDepth){UnityEngine.Debug.Log("Max depth of: " + depth);}
                break;
            }
            if(IsMateScore(bestEvalThisIteration)){
                break;
            }
        }
        return bestEvalThisIteration;
    }
    int SearchMoves(int depth, int plyFromRoot, int alpha, int beta, int numExtensions){
        if(abortSearch){return 0;}
        if(board.IsRepetitionDraw()){return 0;}
        if(board.fiftyMoveCounter >= 100){return 0;}

        if(aiSettings.useTT){
            int ttEval = tt.LookupEvaluation(depth, plyFromRoot, alpha, beta);
            //TT score found
            if(ttEval != TranspositionTable.LookupFailed){
                //Set the best move
                if(plyFromRoot == 0){
                    bestMoveThisIteration = tt.GetStoredMove();
                    bestEvalThisIteration = ttEval;
                }
                return ttEval;
            }
        }
        
        //Returns the actual eval of the position
        if(depth <= 0){
            return evaluation.EvaluatePosition(board, aiSettings);
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
        int searchExtension = 0;
        if(aiSettings.useSearchExtensions && legalMoves.Count == 1 && numExtensions < 16){searchExtension++;}
        
        Move firstSearchMove = null;
        if(aiSettings.useTT){
            firstSearchMove = (plyFromRoot == 0) ? bestMove : tt.GetStoredMove();
        } else{
            firstSearchMove = (plyFromRoot == 0) ? bestMove : null;
        }
        
        legalMoves = moveOrder.OrderMoves(board, legalMoves, firstSearchMove);
        int evaluationBound = TranspositionTable.UpperBound;
        Move bestMoveInThisPosition = null;
        for(int i = 0; i<legalMoves.Count; i++){
            int localExtension = searchExtension;
            
            //Make move -> search move -> unmake move
            board.Move(legalMoves[i], true);
            if((legalMoves[i].isPromotion() || board.isCurrentPlayerInCheck) && aiSettings.useSearchExtensions && numExtensions < 16){localExtension++;}
            int eval = -SearchMoves(depth + localExtension - 1 , plyFromRoot + 1, -beta, -alpha, numExtensions + localExtension);
            board.UndoMove(legalMoves[i]);

            if(abortSearch){return 0;}

            //Move is too good, would be prevented by a previous move
            if(eval >= beta){
                //Exiting search early, so it is a lower bound
                if(aiSettings.useTT){tt.StoreEvaluation(depth, plyFromRoot, beta, TranspositionTable.LowerBound, legalMoves[i]);}
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
                    bestEvalThisIteration = eval;
                }
            }
        }
        if(aiSettings.useTT){tt.StoreEvaluation(depth + searchExtension, plyFromRoot, alpha, evaluationBound, bestMoveInThisPosition);}
        return alpha;
    }
    public static bool IsMateScore(int score){
        const int maxMatePly = 150;
        return Math.Abs(score) > positiveInfinity - maxMatePly;
    }
    public void EndSearch(){
        abortSearch = true;
    }

}
