using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;


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
    int[] staticEvals = new int[maxGamePly];
    int selDepth;

    bool abortSearch = false;
    const int positiveInfinity = 99999;
    const int negativeInfinity = -99999;
    const int checkmate = -99998;
    const int window = 100;
    const int RFPMargin = 150;
    const int RFPImprovingMargin = 100;
    public const int maxGamePly = 1024;
    public event Action<Move> onSearchComplete;


    Stopwatch searchTimer = new Stopwatch();
    SearchLogger logger;

    public Search(Board board, AISettings aiSettings, SearchLogger logger)
    {
        this.logger = logger;
        this.board = board;
        this.aiSettings = aiSettings;
        evaluation = new Evaluation(logger);
        tt = new TranspositionTable(board, aiSettings.ttSize);
        moveOrder = new MoveOrder();
    }

    public void StartSearch(bool writeInfoLine)
    {
        bestMove = nullMove;
        abortSearch = false;
        try
        {
            bestEval = StartIterativeDeepening(aiSettings.maxDepth, writeInfoLine);
        }
        catch (Exception e)
        {
            logger.AddToLog("Iterative deepening error: " + e.Message, SearchLogger.LoggingLevel.Deadly);
        }

        if (bestMove.isNull())
        {
            Span<Move> legalMoves = new Move[256];
            int numMoves = MoveGenerator.GenerateLegalMoves(board, ref legalMoves, board.colorTurn);
            bestMove = legalMoves[0];
            logger.AddToLog($"Timed out, no move found. Num moves: {numMoves}. Generating random", SearchLogger.LoggingLevel.Deadly);
        }

        onSearchComplete?.Invoke(bestMove);
    }

    int StartIterativeDeepening(int maxDepth, bool writeInfoLine)
    {
        selDepth = 0;

        searchTimer.Restart();

        bestEval = SearchMoves(1, 0, negativeInfinity, positiveInfinity, 0);
        
        for (int depth = 2; depth <= maxDepth; depth++)
        {

            bestMoveThisIteration = nullMove;
            moveOrder.DecayHistory();

            //Aspiration windows
            int alpha = bestEval - window;
            int beta = bestEval + window;
            try
            {
                bestEval = SearchMoves(depth, 0, alpha, beta, 0);
                if(bestEval <= alpha || bestEval >= beta)
                {
                    bestEval = SearchMoves(depth, 0, negativeInfinity, positiveInfinity, 0);
                }
            }
            catch (Exception e)
            {
                logger.AddToLog("SearchMoves error: " + e.Message, SearchLogger.LoggingLevel.Deadly);
            }

            if (bestMoveThisIteration.isNull())
            {
                if (!abortSearch)
                {
                    logger.AddToLog($"Draw detected: {board.ConvertToFEN()}", SearchLogger.LoggingLevel.Warning);
                    logger.AddToLog($"Start pos: {board.startFen}", SearchLogger.LoggingLevel.Warning);
                    string message = "";
                    Move[] moves = new Move[board.gameMoveHistory.Count];
                    board.gameMoveHistory.CopyTo(moves, 0);
                    for (int x = 0; x < moves.Length; x++)
                    {
                        message += Coord.GetUCIMoveNotation(moves[x]);
                    }
                    logger.AddToLog("Moves: " + message, SearchLogger.LoggingLevel.Warning);
                }
            }
            else
            {
                bestMove = bestMoveThisIteration;
            }


            string infoLine;
            string pv = "";
            try
            {
                pv = ExtractPV();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            string scoreString = IsMateScore(bestEval) ? $"mate {(bestEval < 0 ? "-" : "")}{positiveInfinity - 1 - Math.Abs(bestEval)}" : $"cp {bestEval}";

            infoLine = $"info depth {depth} seldepth {selDepth} score {scoreString} currmove {Engine.convertMoveToUCI(bestMove)} nodes {logger.currentDiagnostics.nodesSearched} nps {logger.currentDiagnostics.nodesSearched / ((ulong)searchTimer.ElapsedMilliseconds + 1) * 1000f} pv {pv}";

            if (writeInfoLine)
            {
                Console.WriteLine(infoLine);
            }
            logger.AddToLog(infoLine, SearchLogger.LoggingLevel.Info);

            if (abortSearch){ break; }
            if (IsMateScore(bestEvalThisIteration)){ break; }

        }
        logger.currentDiagnostics.totalSearchTime = searchTimer.Elapsed;

        return bestEvalThisIteration;
    }

    int SearchMoves(int depth, int plyFromRoot, int alpha, int beta, int numCheckExtensions)
    {
        if (abortSearch) { return 0; }
        if (plyFromRoot > 0 && board.IsSearchDraw()) { return 0; }

        //Check the TT for a valid entry
        int ttEval = tt.LookupEvaluation(depth, plyFromRoot, alpha, beta);
        if (ttEval != TranspositionTable.LookupFailed)
        {
            //Set the best move
            if (plyFromRoot == 0)
            {
                bestMoveThisIteration = tt.GetStoredMove();
            }
            return ttEval;
        }

        //Quiescence search
        if (depth <= 0)
        {
            int eval = QuiescenceSearch(alpha, beta, plyFromRoot + 1);
            return eval;
        }

        int staticEval = evaluation.EvaluatePosition(board);
        staticEvals[board.fullMoveClock] = (board.currentGameState.isInCheck) ? negativeInfinity : staticEval;
        bool isImproving = isPositionImproving(board.fullMoveClock, board.currentGameState.isInCheck);

        if (plyFromRoot > 0 )
        {
            //NMP
            if (depth > 2)
            {
                int currentColorIndex = (board.colorTurn == Piece.White) ? Board.WhiteIndex : Board.BlackIndex;
                int nonPawnCount = board.pieceCounts[currentColorIndex, Piece.Knight] + board.pieceCounts[currentColorIndex, Piece.Bishop] + board.pieceCounts[currentColorIndex, Piece.Rook] + board.pieceCounts[currentColorIndex, Piece.Queen];
                if (!board.currentGameState.isInCheck && nonPawnCount > 0 && staticEval > beta)
                {
                    int r = 2;

                    board.MakeNullMove();
                    int eval = -SearchMoves(depth - r - 1, plyFromRoot + 1, -beta, -(beta - 1), numCheckExtensions);
                    board.UnmakeNullMove();

                    if (abortSearch) { return 0; }
                    if (eval >= beta) {return eval; }
                }
            }
            //RFP
            if (depth < 4 && !board.currentGameState.isInCheck && staticEval >= beta + (isImproving ? RFPImprovingMargin : RFPMargin) * depth )
            {
                return staticEval;
            }
        }

        Span<Move> legalMoves = stackalloc Move[218];
        int numLegalMoves = MoveGenerator.GenerateLegalMoves(board, ref legalMoves, board.colorTurn);

        //Check for mate or stalemate
        if (numLegalMoves == 0)
        {
            if (board.currentGameState.isInCheck){ return checkmate + plyFromRoot;}
            else { return 0; }
        }

        //Move ordering
        int[] moveScores = moveOrder.ScoreMoves(board, legalMoves, (plyFromRoot == 0) ? bestMove : tt.GetStoredMove());

        int evaluationBound = TranspositionTable.UpperBound;
        Move bestMoveInThisPosition = nullMove;

        int bestScore = negativeInfinity;

        for (int i = 0; i < numLegalMoves; i++)
        {
            moveOrder.GetNextBestMove(moveScores, legalMoves, i);

            if(!board.currentGameState.isInCheck && depth < 4 && !legalMoves[i].isCapture() && !legalMoves[i].isPromotion())
            {
                if((staticEval + (150 * depth)) < alpha ){ continue; }
            }

            board.Move(legalMoves[i], true);
            logger.currentDiagnostics.nodesSearched++;

            //Check extension
            int extension = (numLegalMoves == 1) ? 1 : 0;
            if (board.currentGameState.isInCheck && numCheckExtensions < 15 && extension == 0)
            {
                extension = 1;
                numCheckExtensions++;
            }


            int reductions = 0;
            //LMR
            if (i > 0 && depth > 3)
            {
                reductions = 1 + (int)(Math.Log(i) * Math.Log(depth) / 3);
            }

            int eval = -SearchMoves(depth + extension - 1 - reductions, plyFromRoot + 1, -beta, -alpha, numCheckExtensions);

            if (eval > alpha && reductions > 0)
            {
                eval = -SearchMoves(depth + extension - 1, plyFromRoot + 1, -beta, -alpha, numCheckExtensions);
            }

            board.UndoMove(legalMoves[i]);

            if (abortSearch) { return 0; }

            if (eval > bestScore)
            {
                bestScore = eval;
                bestMoveInThisPosition = legalMoves[i];
                //If this is a root move, set it to the best move
                if (plyFromRoot == 0)
                {
                    bestMoveThisIteration = legalMoves[i];
                }

                //This move is better than the current move
                if (eval > alpha)
                {
                    evaluationBound = TranspositionTable.Exact;
                    alpha = eval;
                }
            }

            //Move is too good, would be prevented by a previous move
            if (eval >= beta)
            {
                //Exiting search early, so it is a lower bound
                tt.StoreEvaluation(depth - reductions, plyFromRoot, bestScore, TranspositionTable.LowerBound, legalMoves[i]);
                //Saving quiet move to killers
                if (!legalMoves[i].isCapture())
                {
                    moveOrder.UpdateMoveOrderTables(legalMoves[i], depth, board.fullMoveClock);
                }
                return bestScore;
            }
        }
        
        tt.StoreEvaluation(depth + ((numLegalMoves == 1) ? 1 : 0), plyFromRoot, bestScore, evaluationBound, bestMoveInThisPosition);
        return bestScore;
    }

    public int QuiescenceSearch(int alpha, int beta, int plyFromRoot)
    {
        if(plyFromRoot > selDepth) { selDepth = plyFromRoot; }
        if (board.IsCheckmate(board.colorTurn))
        {
            return checkmate + plyFromRoot;
        }

        int bestEval = 0;
        bestEval = evaluation.EvaluatePosition(board);
        
        int standPat = bestEval;

        //Cutoffs
        if (bestEval >= beta)
        {
            return bestEval;
        }

        if (bestEval > alpha)
        {
            alpha = bestEval;
        }
        //If even after winning a queen it is still worse, don't bother searching
        //if (bestEval + Evaluation.queenValue < alpha) { return alpha; }

        Span<Move> legalMoves = stackalloc Move[218];
        int numMoves = MoveGenerator.GenerateLegalMoves(board, ref legalMoves, board.colorTurn, true);
        int[] moveScores = moveOrder.ScoreCaptures(board, legalMoves);

        for (int i = 0; i < numMoves; i++)
        {
            moveOrder.GetNextBestMove(moveScores, legalMoves, i);

            //Delta pruning
            if ((standPat + getCapturedPieceVal(legalMoves[i]) + 200) < alpha){ continue; }

            board.Move(legalMoves[i], true);
            logger.currentDiagnostics.nodesSearched++;
            int eval = -QuiescenceSearch(-beta, -alpha, plyFromRoot + 1);
            board.UndoMove(legalMoves[i]);
            
            if (eval > bestEval)
            {
                bestEval = eval;
                if (eval > alpha)
                {
                    alpha = eval;
                }
            }
            if (eval >= beta)
            {
                return bestEval;
            }
            
        }
        return bestEval;
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

    bool isPositionImproving(int fullMoveClock, bool isInCheck)
    {
        if (isInCheck) { return false; }
        if (fullMoveClock > 1 && staticEvals[fullMoveClock - 2] != negativeInfinity) { return staticEvals[fullMoveClock] > staticEvals[fullMoveClock - 2]; }
        return true;
    }


    public static bool IsMateScore(int score)
    {
        const int maxMatePly = 150;
        return Math.Abs(score) > (positiveInfinity - maxMatePly);
    }
    public void EndSearch() { abortSearch = true; }
    string ExtractPV()
    {
        Stack<Move> moveList = new Stack<Move>();
        string pv = "";
        bool breakInPv = false;
        int counter = 0;
        while (!breakInPv)
        {
            
            counter++;
            TranspositionTable.Entry entry = tt.GetEntryForPos();
            if (entry.key == board.zobristKey && entry.nodeType == TranspositionTable.Exact)
            {
                if (!entry.move.isNull())
                {
                    board.Move(entry.move, true);
                    moveList.Push(entry.move);
                    if (!board.IsSearchDraw())
                    {
                        pv += Coord.GetUCIMoveNotation(entry.move) + " ";
                    } else{ breakInPv = true; }
                }
                else { breakInPv = true; }

            }
            else { breakInPv = true; }
        }
        while (moveList.Count > 0)
        {
            board.UndoMove(moveList.Pop());
        }
        return pv;
    }

}

