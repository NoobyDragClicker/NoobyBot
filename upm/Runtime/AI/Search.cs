using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;


//todo tonight: use spans, add TT stats
public class Search
{
    Board board;
    AISettings aiSettings;
    Evaluation evaluation;
    MoveOrder moveOrder;
    public TranspositionTable tt;
    public static readonly Move nullMove = new Move(0, 0, false);


    Move bestMove = nullMove;
    Move bestMoveThisIteration;
    int bestEvalThisIteration;
    int bestEval;
    Move[,] killerMoves;
    int[,] history;

    bool abortSearch = false;
    const int positiveInfinity = 99999;
    const int negativeInfinity = -99999;
    const int checkmate = -99998;
    const int HISTORY_MAX = 32768;
    public event Action<Move> onSearchComplete;


    Stopwatch iterationTimer = new Stopwatch();
    Stopwatch evaluationTimer = new Stopwatch();
    Stopwatch moveGenTimer = new Stopwatch();
    Stopwatch quiescenceGenTimer = new Stopwatch();
    Stopwatch quiescenceTimer = new Stopwatch();
    Stopwatch moveOrderTimer = new Stopwatch();
    Stopwatch makeUnmakeTimer = new Stopwatch();
    Stopwatch searchTimer = new Stopwatch();
    Stopwatch reSearchTimer = new Stopwatch();
    SearchLogger logger;

    public Search(Board board, AISettings aiSettings, Move[,] killerMoves, int[,] history, SearchLogger logger)
    {
        this.logger = logger;
        this.board = board;
        this.aiSettings = aiSettings;
        this.killerMoves = killerMoves;
        this.history = history;
        evaluation = new Evaluation();
        tt = new TranspositionTable(board, 256);
        moveOrder = new MoveOrder();
    }

    public void StartSearch()
    {
        logger.AddToLog("Search started");
        bestMove = nullMove;
        bestMoveThisIteration = nullMove;
        abortSearch = false;
        try
        {
            bestEval = StartIterativeDeepening(aiSettings.maxDepth);
        }
        catch (Exception e)
        {
            logger.AddToLog("Iterative deepening error: " + e.Message);
        }

        if (bestMove.isNull())
        {
            Span<Move> legalMoves = new Move[256];
            int numMoves = MoveGenerator.GenerateLegalMoves(board, ref legalMoves, board.colorTurn);
            bestMove = legalMoves[0];
            logger.AddToLog($"Timed out, no move found. Num moves: {numMoves}. Generating random");
        }

        onSearchComplete?.Invoke(bestMove);

    }

    int StartIterativeDeepening(int maxDepth)
    {
        logger.currentDiagnostics.numBestMovesPerIndex = new int[256];
        logger.currentDiagnostics.msPerIteration = new int[maxDepth];

        searchTimer.Restart();
        reSearchTimer.Reset();
        moveGenTimer.Reset();
        moveOrderTimer.Reset();
        makeUnmakeTimer.Reset();
        quiescenceGenTimer.Reset();
        quiescenceTimer.Reset();
        evaluationTimer.Reset();
        for (int depth = 1; depth <= maxDepth; depth++)
        {
            DecayHistory();
            iterationTimer.Restart();
            SearchMoves(depth, 0, negativeInfinity, positiveInfinity, 0);

            if (!bestMoveThisIteration.isNull())
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
                infoLine = $"info depth {depth} score mate {(bestEval < 0 ? "-" : "")}{positiveInfinity - 1 - Math.Abs(bestEval)} currmove {Engine.convertMoveToUCI(bestMove)} nodes {logger.currentDiagnostics.nodesSearched} nps {logger.currentDiagnostics.nodesSearched / ((ulong)searchTimer.ElapsedMilliseconds + 1) * 1000}";
            }
            else
            {
                infoLine = $"info depth {depth} score cp {bestEval} currmove {Engine.convertMoveToUCI(bestMove)} nodes {logger.currentDiagnostics.nodesSearched} nps {logger.currentDiagnostics.nodesSearched / ((ulong)searchTimer.ElapsedMilliseconds + 1) * 1000}";
            }
            Console.WriteLine(infoLine);
            logger.AddToLog($"info depth {depth} score cp {bestEval} currmove {Engine.convertMoveToUCI(bestMove)} nodes {logger.currentDiagnostics.nodesSearched} nps {logger.currentDiagnostics.nodesSearched / ((ulong)searchTimer.ElapsedMilliseconds + 1) * 1000}");

            if (abortSearch)
            {
                iterationTimer.Stop();
                break;
            }
            if (IsMateScore(bestEvalThisIteration))
            {
                iterationTimer.Stop();
                break;
            }


            //Only save times for the fully searched depths
            iterationTimer.Stop();
            logger.currentDiagnostics.msPerIteration[depth - 1] = (int)iterationTimer.ElapsedMilliseconds;
        }


        logger.currentDiagnostics.totalSearchTime = searchTimer.Elapsed;
        logger.currentDiagnostics.reSearchTime = reSearchTimer.Elapsed;
        logger.currentDiagnostics.moveGenTime = moveGenTimer.Elapsed;
        logger.currentDiagnostics.moveOrderTime = moveOrderTimer.Elapsed;
        logger.currentDiagnostics.makeUnmakeTime = makeUnmakeTimer.Elapsed;
        logger.currentDiagnostics.quiescenceGenTime = quiescenceGenTimer.Elapsed;
        logger.currentDiagnostics.quiescenceTime = quiescenceTimer.Elapsed;
        logger.currentDiagnostics.evaluationTime = evaluationTimer.Elapsed;

        return bestEvalThisIteration;
    }

    int SearchMoves(int depth, int plyFromRoot, int alpha, int beta, int numCheckExtensions)
    {
        if (abortSearch) { return 0; }
        if (board.IsRepetitionDraw()) { return 0; }
        if (board.fiftyMoveCounter >= 100) { return 0; }

        //Check the TT for a valid entry
        int ttEval = tt.LookupEvaluation(depth, plyFromRoot, alpha, beta);
        if (ttEval != TranspositionTable.LookupFailed)
        {
            logger.currentDiagnostics.ttHits++;
            //Set the best move
            if (plyFromRoot == 0)
            {
                bestMoveThisIteration = tt.GetStoredMove();
                bestEvalThisIteration = ttEval;
            }
            return ttEval;
        }

        //Quiescence search
        if (depth <= 0)
        {
            logger.currentDiagnostics.nodesSearched++;
            quiescenceTimer.Start();
            int eval = QuiescenceSearch(alpha, beta, plyFromRoot + 1);
            quiescenceTimer.Stop();
            return eval;
        }


        moveGenTimer.Start();
        Span<Move> legalMoves = stackalloc Move[218];
        int numLegalMoves = MoveGenerator.GenerateLegalMoves(board, ref legalMoves, board.colorTurn);
        moveGenTimer.Stop();

        //Check for mate or stalemate
        if (numLegalMoves == 0)
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

        moveOrderTimer.Start();  //                          first search move                   
        int[] moveScores = moveOrder.ScoreMoves(board, legalMoves, (plyFromRoot == 0) ? bestMove : tt.GetStoredMove(), killerMoves, history, aiSettings);
        moveOrderTimer.Stop();

        int evaluationBound = TranspositionTable.UpperBound;
        Move bestMoveInThisPosition = nullMove;
        string bestMovesTracker = "pv progression: ";

        for (int i = 0; i < numLegalMoves; i++)
        {
            moveOrderTimer.Start(); 
            moveOrder.GetNextBestMove(moveScores, legalMoves, i);
            moveOrderTimer.Stop();

            makeUnmakeTimer.Start();
            board.Move(legalMoves[i], true);
            makeUnmakeTimer.Stop();

            board.GenerateMoveGenInfo();

            int eval;
            int reductions = 0;
            if (i >= 3 && depth > 3)
            {
                reductions++;
            }

            int extension = (numLegalMoves == 1) ? 1 : 0;
            if (board.isCurrentPlayerInCheck && numCheckExtensions < 15 && extension == 0)
            {
                extension = 1;
                numCheckExtensions++;
            }
            //First search including reductions
            eval = -SearchMoves(depth + extension - 1 - reductions, plyFromRoot + 1, -beta, -alpha, numCheckExtensions);

            //Reduced depth failed high; research
            if (eval > alpha && reductions > 0)
            {
                reductions = 0;
                logger.currentDiagnostics.timesReSearched_LMR++;
                reSearchTimer.Start();
                eval = -SearchMoves(depth + extension - 1, plyFromRoot + 1, -beta, -alpha, numCheckExtensions);
                reSearchTimer.Stop();
            }
            else { logger.currentDiagnostics.timesNotReSearched_LMR++; }

            makeUnmakeTimer.Start();
            board.UndoMove(legalMoves[i]);
            makeUnmakeTimer.Stop();

            if (abortSearch) { return 0; }

            //Move is too good, would be prevented by a previous move
            if (eval >= beta)
            {
                //Exiting search early, so it is a lower bound
                logger.currentDiagnostics.ttStores++;
                tt.StoreEvaluation(depth - reductions, plyFromRoot, beta, TranspositionTable.LowerBound, legalMoves[i]);
                if (!legalMoves[i].isCapture())
                {
                    for (int moveNum = 0; moveNum < 3; moveNum++)
                    {
                        if (!killerMoves[board.plyFromStart, moveNum].isNull())
                        {
                            killerMoves[board.plyFromStart, moveNum] = legalMoves[i];
                            break;
                        }
                    }

                    int historyVal = history[legalMoves[i].oldIndex, legalMoves[i].newIndex] + depth * depth;
                    history[legalMoves[i].oldIndex, legalMoves[i].newIndex] = (historyVal < HISTORY_MAX) ? historyVal : HISTORY_MAX;
                }
                return beta;
            }

            //This move is better than the current move
            if (eval > alpha)
            {
                evaluationBound = TranspositionTable.Exact;
                bestMoveInThisPosition = legalMoves[i];
                alpha = eval;
                logger.currentDiagnostics.numBestMovesPerIndex[i]++;

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

        tt.StoreEvaluation(depth + ((numLegalMoves == 1) ? 1 : 0), plyFromRoot, alpha, evaluationBound, bestMoveInThisPosition);

        return alpha;
    }

    int QuiescenceSearch(int alpha, int beta, int plyFromRoot)
    {
        if (board.IsCheckmate(board.colorTurn))
        {
            return checkmate + plyFromRoot;
        }

        evaluationTimer.Start();
        int eval = evaluation.EvaluatePosition(board, aiSettings);
        int standPat = eval;
        evaluationTimer.Stop();

        //Cutoffs
        if (eval >= beta)
        {
            return beta;
        }

        if (eval > alpha)
        {
            alpha = eval;
        }
        //If even after winning a queen it is still worse, don't bother searching
        if (eval + Evaluation.queenValue < alpha) { return alpha; }

        quiescenceGenTimer.Start();
        Span<Move> legalMoves = stackalloc Move[218];
        int numMoves = MoveGenerator.GenerateLegalMoves(board, ref legalMoves, board.colorTurn, true);
        quiescenceGenTimer.Stop();

        moveOrderTimer.Start();
        int[] moveScores = moveOrder.ScoreCaptures(board, legalMoves);
        moveOrderTimer.Stop();

        for (int i = 0; i < numMoves; i++)
        {
            moveOrder.GetNextBestMove(moveScores, legalMoves, i);

            //Delta pruning
            if ((standPat + getCapturedPieceVal(legalMoves[i]) + 200) < alpha){ continue; }

            makeUnmakeTimer.Start();
            board.Move(legalMoves[i], true);
            makeUnmakeTimer.Stop();

            eval = -QuiescenceSearch(-beta, -alpha, plyFromRoot + 1);

            makeUnmakeTimer.Start();
            board.UndoMove(legalMoves[i]);
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

    int getCapturedPieceVal(Move move) {
        int pieceVal = 0;
        if (move.flag != 7)
        {
            int pieceType = Piece.PieceType(board.board[move.newIndex]);

            switch (pieceType)
            {
                case Piece.Pawn: pieceVal = Evaluation.pawnValue; break;
                case Piece.Knight: pieceVal = Evaluation.knightValue; break;
                case Piece.Rook: pieceVal = Evaluation.rookValue; break;
                case Piece.Bishop: pieceVal = Evaluation.bishopValue; break;
                case Piece.Queen: pieceVal = Evaluation.queenValue; break;
            }
        }
        else { pieceVal = Evaluation.pawnValue; }
        return pieceVal;
    }

    public static bool IsMateScore(int score)
    {
        const int maxMatePly = 150;
        return Math.Abs(score) > (positiveInfinity - maxMatePly);
    }
    public void EndSearch() { abortSearch = true; }

    void DecayHistory()
    {
        for (int f = 0; f < 64; f++)
        {
            for (int t = 0; t < 64; t++)
            {
                history[f, t] -= history[f, t] >> 2; // ~75% retain; cheap & fast}
            }

        }
    }

}

