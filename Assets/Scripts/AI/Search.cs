using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


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
    ulong nodesSearched;
    Stopwatch iterationTimer = new Stopwatch();
    Stopwatch evaluationTimer = new Stopwatch();
    Stopwatch moveGenTimer = new Stopwatch();
    Stopwatch moveOrderTimer = new Stopwatch();
    Stopwatch makeUnmakeTimer = new Stopwatch();
    Stopwatch ttLookupTimer = new Stopwatch();
    Stopwatch ttStoreTimer = new Stopwatch();
    SearchLogger logger;

    public Search(Board board, AISettings aiSettings, Move[,] killerMoves, SearchLogger logger)
    {
        this.logger = logger;
        this.board = board;
        this.aiSettings = aiSettings;
        this.killerMoves = killerMoves;
        evaluation = new Evaluation();
        tt = new TranspositionTable(board, 256);
        moveOrder = new MoveOrder();
    }

    public void StartSearch()
    {
        logger.AddToLog("Search started");
        nodesSearched = 0;
        bestMove = null;
        bestMoveThisIteration = null;
        abortSearch = false;
        try
        {
            bestEval = StartIterativeDeepening(aiSettings.maxDepth);
        }
        catch (Exception e)
        {
            logger.AddToLog("Iterative deepening error: " + e.Message);
        }

        if (bestMove == null)
        {
            bestMove = board.moveGenerator.GenerateLegalMoves(board, board.colorTurn)[0];
            logger.AddToLog($"Timed out, no move found. Num moves: {board.moveGenerator.GenerateLegalMoves(board, board.colorTurn).Count}. Generating random");
        }

        onSearchComplete?.Invoke(bestMove);
        
    }

    int StartIterativeDeepening(int maxDepth){
        for (int depth = 1; depth <= maxDepth; depth++)
        {
            moveOrderTimer.Reset();
            moveGenTimer.Reset();
            evaluationTimer.Reset();
            makeUnmakeTimer.Reset();
            ttLookupTimer.Reset();
            ttStoreTimer.Reset();

            iterationTimer.Restart();
            SearchMoves(depth, 0, negativeInfinity, positiveInfinity, 0);
            iterationTimer.Stop();

            if (bestMoveThisIteration != null)
            {
                bestMove = bestMoveThisIteration;
                bestEval = bestEvalThisIteration;
            }
            else
            {
                logger.AddToLog("best move was null");
            }


            string infoLine;
            if (IsMateScore(bestEval))
            {
                infoLine = $"info depth {depth} score mate {(bestEval < 0 ? "-" : "")}{positiveInfinity - 1 - Math.Abs(bestEval)} currmove {Engine.convertMoveToUCI(bestMove)} nodes {nodesSearched} nps {nodesSearched / ((ulong)iterationTimer.ElapsedMilliseconds + 1) * 1000 }";
            }
            else
            {
                infoLine = $"info depth {depth} score cp {bestEval} currmove {Engine.convertMoveToUCI(bestMove)} nodes {nodesSearched} nps {nodesSearched / ((ulong)iterationTimer.ElapsedMilliseconds + 1) * 1000 }";
            }
            Console.WriteLine(infoLine);
            logger.AddToLog($"info depth {depth} score cp {bestEval} currmove {Engine.convertMoveToUCI(bestMove)} nodes {nodesSearched} nps {nodesSearched / ((ulong)iterationTimer.ElapsedMilliseconds + 1) * 1000 } total {iterationTimer.Elapsed} gen {moveGenTimer.Elapsed} order {moveOrderTimer.Elapsed} eval {evaluationTimer.Elapsed} makeUnmake {makeUnmakeTimer.Elapsed} lookup {ttLookupTimer.Elapsed} store {ttStoreTimer.Elapsed}");

            if (abortSearch)
            {
                break;
            }
            if (IsMateScore(bestEvalThisIteration))
            {
                break;
            }

        }
        return bestEvalThisIteration;
    }

    int SearchMoves(int depth, int plyFromRoot, int alpha, int beta, int numExtensions){  
        if (abortSearch) {return 0;}
        nodesSearched++;
        if (board.IsRepetitionDraw()) { return 0; }
        if(board.fiftyMoveCounter >= 100){return 0;}

        ttLookupTimer.Start();
        int ttEval = tt.LookupEvaluation(depth, plyFromRoot, alpha, beta);
        ttLookupTimer.Stop();

        //TT score found
        if (ttEval != TranspositionTable.LookupFailed)
        {
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

        moveGenTimer.Start();
        List<Move> legalMoves = board.moveGenerator.GenerateLegalMoves(board, board.colorTurn);
        moveGenTimer.Stop();

        //Check for mate or stalemate
        if (legalMoves.Count == 0)
        {
            if (board.isCurrentPlayerInCheck)
            {
                return checkmate + plyFromRoot;
            }
            else
            {
                return 0;
            }
        }

        //Search extension for single legal moves
        int searchExtension = 0;
        if(legalMoves.Count == 1 && numExtensions < aiSettings.maxSearchExtensionDepth){searchExtension++;}
        
        Move firstSearchMove = (plyFromRoot == 0) ? bestMove : tt.GetStoredMove();

        moveOrderTimer.Start();
        legalMoves = moveOrder.OrderMoves(board, legalMoves, firstSearchMove, killerMoves, aiSettings);
        moveOrderTimer.Stop();

        int evaluationBound = TranspositionTable.UpperBound;
        Move bestMoveInThisPosition = null;
        string bestMovesTracker = "pv progression: ";

        for (int i = 0; i < legalMoves.Count; i++)
        {
            int localExtension = searchExtension;
            makeUnmakeTimer.Start();
            board.Move(legalMoves[i], true);
            makeUnmakeTimer.Stop();

            board.UpdateCheckingInfo();
            //Search extensions for promotion and checks
            if ((legalMoves[i].isPromotion() || board.isCurrentPlayerInCheck) && numExtensions < aiSettings.maxSearchExtensionDepth) { localExtension++; }

            int eval;
            int reductions = 0;
            //More aggressive LMR (depth > 3), sorted best to worst
            //Test 1: reductions = Math.Min(depth / 2, (i + 1) * depth / 24); depth 12 score cp -31 currmove e7e5 nodes 3257758
            //Test 2: reductions = Math.Min(depth / 2, (int)(Math.Sqrt(i - 2) * depth / 6)); depth 12 score cp -31 currmove e7e5 nodes 3423364
            //Test 3: reductions = Math.Min(depth / 2, (int)(Math.Log(i + 1) * depth / 8)); depth 12 score cp -31 currmove e7e5 nodes 3423364
            //Test 4: reductions = Math.Min(depth / 2, ((i - 2) * depth) / 16); depth 12 score cp -31 currmove e7e5 nodes 5051817
            //Test 6: reductions = Math.Min(depth / 2, (i + 1) * depth / 20); depth 12 score cp -31 currmove e7e5 nodes 5942180
            //Test 5: reductions = Math.Min(depth / 2, (i + 1) * depth / 30); depth 12 score cp 39 currmove g1f3 nodes 6516523
            //depth > 2: 
            //Test 7: reductions = Math.Min(depth / 2, (i + 1) * depth / 24); depth 12 score cp 47 currmove d2d4 nodes 1221668
            if (i >= 3 && depth > 3)
            {
                reductions++;
            }

            eval = -SearchMoves(depth + localExtension - 1 - reductions, plyFromRoot + 1, -beta, -alpha, numExtensions + localExtension);

            //Reduced depth failed high; research
            if (eval > alpha && reductions > 0)
            {
                reductions = 0;
                eval = -SearchMoves(depth + localExtension - 1, plyFromRoot + 1, -beta, -alpha, numExtensions + localExtension);
            }

            makeUnmakeTimer.Start();
            board.UndoMove(legalMoves[i]);
            makeUnmakeTimer.Stop();

            if (abortSearch) { return 0; }

            //Move is too good, would be prevented by a previous move
            if (eval >= beta)
            {
                //Exiting search early, so it is a lower bound
                ttStoreTimer.Start();
                tt.StoreEvaluation(depth - reductions, plyFromRoot, beta, TranspositionTable.LowerBound, legalMoves[i]);
                ttStoreTimer.Stop();
                if (!legalMoves[i].isCapture())
                {
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
                    bestMovesTracker += Coord.GetMoveNotation(legalMoves[i].oldIndex, legalMoves[i].newIndex) + $" ({i}), ";
                    bestMoveThisIteration = legalMoves[i];
                    bestEvalThisIteration = eval;
                }
            }
        }
        if (plyFromRoot == 0)
        {
            logger.AddToLog(bestMovesTracker);
        }

        ttStoreTimer.Start();
        try
        {
            tt.StoreEvaluation(depth + searchExtension, plyFromRoot, alpha, evaluationBound, bestMoveInThisPosition);
        }
        catch (Exception e)
        {
            logger.AddToLog("Error storing eval:" + e.Message);
        }
        ttStoreTimer.Stop();
        return alpha;
    }

    int QuiescenceSearch(int alpha, int beta, int plyFromRoot){
        if(board.IsCheckmate(board.colorTurn)){
            return checkmate + plyFromRoot;
        } else if(board.IsDraw()){
            return 0;
        }

        evaluationTimer.Start();
        int eval = 0;
        try
        {
            eval = evaluation.EvaluatePosition(board, aiSettings);
        }
        catch (Exception e)
        {
            logger.AddToLog("Evaluation error: " + e.Message);
        }
        
        evaluationTimer.Stop();

        //Cutoffs
        if (eval >= beta)
        {
            return beta;
        }

		if (eval > alpha) {
			alpha = eval;
		}

        moveGenTimer.Start();
        List<Move> captures = board.moveGenerator.GenerateLegalMoves(board, board.colorTurn, true);
        moveGenTimer.Stop();

        moveOrderTimer.Start();
        captures = moveOrder.OrderMoves(board, captures, null, killerMoves, aiSettings);
        moveOrderTimer.Stop();

        for (int i = 0; i < captures.Count; i++)
        {
            makeUnmakeTimer.Start();
            board.Move(captures[i], true);
            makeUnmakeTimer.Stop();

            eval = -QuiescenceSearch(-beta, -alpha, plyFromRoot + 1);

            makeUnmakeTimer.Start();
            board.UndoMove(captures[i]);
            makeUnmakeTimer.Stop();
            if (eval >= beta)
            {
                return beta;
            }
            if (eval > alpha)
            {
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
    public void EndSearch(){abortSearch = true; logger.AddToLog("Abort search set to true"); }
}
