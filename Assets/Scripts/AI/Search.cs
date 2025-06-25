using System;
using System.Collections.Generic;
using System.Diagnostics;

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
    Move[,] killerMoves;

    public Search(Board board, AISettings aiSettings, Move[,] killerMoves)
    {
        this.board = board;
        this.aiSettings = aiSettings;
        this.killerMoves = killerMoves;
        evaluation = new Evaluation();
        tt = new TranspositionTable(board, 256);
        moveOrder = new MoveOrder();
    }

    public void StartSearch(){
        bestMove = null;
        bestMoveThisIteration = null;
        abortSearch = false;
        bestEval = StartIterativeDeepening(aiSettings.maxDepth);
        if (bestMove == null)
        {
            bestMove = board.moveGenerator.GenerateLegalMoves(board, board.colorTurn)[0];
            Engine.LogToFile($"Timed out, no move found. Num moves: {board.moveGenerator.GenerateLegalMoves(board, board.colorTurn).Count}. Generating random");
        }

        onSearchComplete?.Invoke(bestMove);
    }

    int StartIterativeDeepening(int maxDepth){

        ulong startKey = board.zobristKey;
        for(int depth = 1; depth <= maxDepth; depth++){
            SearchMoves(depth, 0, negativeInfinity, positiveInfinity, 0);

            if (bestMoveThisIteration != null)
            {
                bestMove = bestMoveThisIteration;
                bestEval = bestEvalThisIteration;
            }


            string infoLine = "";
            if (IsMateScore(bestEval))
            {
                infoLine = $"info depth {depth} score mate {(bestEval < 0 ? "-" : "")}{positiveInfinity - 1 - Math.Abs(bestEval)} currmove {Engine.convertMoveToUCI(bestMove)}";
            }
            else
            {
                infoLine = $"info depth {depth} score cp {bestEval} currmove {Engine.convertMoveToUCI(bestMove)}";
            }
            Console.WriteLine(infoLine);
            Engine.LogToFile(infoLine);
            


            if (abortSearch)
            {
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

        
        int ttEval = tt.LookupEvaluation(depth, plyFromRoot, alpha, beta);
        //TT score found
        if(ttEval != TranspositionTable.LookupFailed){
            //Set the best move
            if (plyFromRoot == 0)
            {
                bestMoveThisIteration = tt.GetStoredMove();
                bestEvalThisIteration = ttEval;
            }
            return ttEval;
        }

        //Returns the actual eval of the position
        if (depth <= 0)
        {
            int eval = QuiescenceSearch(alpha, beta, plyFromRoot + 1);
            return eval;
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

        //Search extension for single legal moves
        int searchExtension = 0;
        if(legalMoves.Count == 1 && numExtensions < aiSettings.maxSearchExtensionDepth){searchExtension++;}
        
        Move firstSearchMove = (plyFromRoot == 0) ? bestMove : tt.GetStoredMove();
        
        legalMoves = moveOrder.OrderMoves(board, legalMoves, firstSearchMove, killerMoves, aiSettings);

        int evaluationBound = TranspositionTable.UpperBound;
        Move bestMoveInThisPosition = null;
        for (int i = 0; i < legalMoves.Count; i++)
        {
            int localExtension = searchExtension;
            board.Move(legalMoves[i], true);

            //Search extensions for promotion and checks
            if ((legalMoves[i].isPromotion() || board.isCurrentPlayerInCheck) && numExtensions < aiSettings.maxSearchExtensionDepth) { localExtension++; }

            int eval = -SearchMoves(depth + localExtension - 1, plyFromRoot + 1, -beta, -alpha, numExtensions + localExtension);

            board.UndoMove(legalMoves[i]);

            if (abortSearch) { return 0; }

            //Move is too good, would be prevented by a previous move
            if (eval >= beta)
            {
                //Exiting search early, so it is a lower bound
                tt.StoreEvaluation(depth, plyFromRoot, beta, TranspositionTable.LowerBound, legalMoves[i]);
                if (!legalMoves[i].isCapture()) {
                    for (int moveNum = 0; moveNum < 3; moveNum++)
                    {
                        if (killerMoves[board.plyFromStart, moveNum] == null)
                        {
                            killerMoves[board.plyFromStart, moveNum] = legalMoves[i];
                            break;
                        }
                    }
                }
                
                return beta;
            }

            //This move is better than the current move
            if (eval > alpha)
            {
                evaluationBound = TranspositionTable.Exact;
                bestMoveInThisPosition = legalMoves[i];
                alpha = eval;

                //If this is a root move, set it to the best move
                if (plyFromRoot == 0)
                {
                    bestMoveThisIteration = legalMoves[i];
                    bestEvalThisIteration = eval;
                }
            }
        }
        tt.StoreEvaluation(depth + searchExtension, plyFromRoot, alpha, evaluationBound, bestMoveInThisPosition);
        return alpha;
    }

    int QuiescenceSearch(int alpha, int beta, int plyFromRoot){
        if(board.IsCheckmate(board.colorTurn)){
            return checkmate + plyFromRoot;
        } else if(board.IsDraw()){
            return 0;
        }
        
        int eval = evaluation.EvaluatePosition(board, aiSettings);
        //Cutoffs
        if (eval >= beta) {
			return beta;
		}

		if (eval > alpha) {
			alpha = eval;
		}

        List<Move> captures = board.moveGenerator.GenerateLegalMoves(board, board.colorTurn, true);
        captures = moveOrder.OrderMoves(board, captures, null, killerMoves, aiSettings);

        for (int i = 0; i < captures.Count; i++) {
			board.Move (captures[i], true);
			eval = -QuiescenceSearch (-beta, -alpha, plyFromRoot + 1);
			board.UndoMove(captures[i]);
            if (eval >= beta) {
				return beta;
			}
			if (eval > alpha) {
				alpha = eval;
			}
        }
        return alpha;
    }
    
    public static bool IsMateScore(int score)
    {
        const int maxMatePly = 150;
        return Math.Abs(score) >  positiveInfinity - maxMatePly;
    }
    public void EndSearch(){abortSearch = true;}
}
