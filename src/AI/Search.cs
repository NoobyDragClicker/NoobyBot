using System.Diagnostics;
using System.Numerics;



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
    int[] staticEvals = new int[MAX_GAME_PLY];
    int selDepth;

    bool abortSearch = false;
    bool softCapHit = true;
    const int POSITIVE_INFINITY = 99999;
    const int NEGATIVE_INFINITY = -99999;
    const int CHECKMATE = -99998;
    public const int MAX_GAME_PLY = 1024;
    public event Action<Move> onSearchComplete;

    Stopwatch searchTimer = new Stopwatch();
    SearchLogger logger;

    //PARAMS:

    const int ASP_WINDOW = 100;
    const int RFP_MARGIN = 150;
    const int RFP_IMPROVING_MARGIN = 100;
    
    //SEE
    const int SEE_QUIET_MARGIN = -100;
    const int SEE_NOISY_MARGIN = -50;
    int[] SEEPieceVals = [0, 100, 300, 300, 500, 900, 0];


    

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
        softCapHit = false;
        bestEval = StartIterativeDeepening(aiSettings.maxDepth, writeInfoLine);
        if (bestMove.isNull())
        {
            Span<Move> legalMoves = new Move[256];
            int numMoves = MoveGenerator.GenerateLegalMoves(board, ref legalMoves, board.colorTurn);
            bestMove = legalMoves[0];
            Console.WriteLine($"Timed out, no move found. Num moves: {numMoves}. Generating random");
        }

        onSearchComplete?.Invoke(bestMove);
    }

    int StartIterativeDeepening(int maxDepth, bool writeInfoLine)
    {
        selDepth = 0;

        searchTimer.Restart();

        bestEval = SearchMoves(1, 0, NEGATIVE_INFINITY, POSITIVE_INFINITY, 0);
        
        for (int depth = 2; depth <= maxDepth; depth++)
        {

            bestMoveThisIteration = nullMove;

            //Aspiration windows
            int alpha = bestEval - ASP_WINDOW;
            int beta = bestEval + ASP_WINDOW;
            bestEval = SearchMoves(depth, 0, alpha, beta, 0);
            if(bestEval <= alpha || bestEval >= beta)
            {
                bestEval = SearchMoves(depth, 0, NEGATIVE_INFINITY, POSITIVE_INFINITY, 0);
            }
            

            if (bestMoveThisIteration.isNull())
            {
                if (!abortSearch)
                {
                    logger.AddToLog($"No move found at depth {depth}", SearchLogger.LoggingLevel.Warning);
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

            string scoreString = IsMateScore(bestEval) ? $"mate {(bestEval < 0 ? "-" : "")}{POSITIVE_INFINITY - 1 - Math.Abs(bestEval)}" : $"cp {bestEval}";

            infoLine = $"info depth {depth} seldepth {selDepth} score {scoreString} nodes {logger.currentDiagnostics.nodesSearched} nps {logger.currentDiagnostics.nodesSearched / ((ulong)searchTimer.ElapsedMilliseconds + 1) * 1000f} pv {pv}";

            if (writeInfoLine)
            {
                Console.WriteLine(infoLine);
            }
            logger.AddToLog(infoLine, SearchLogger.LoggingLevel.Info);

            if (abortSearch) { break; }
            if(softCapHit){ break; }
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
        (Move, int) ttInfo = tt.LookupEvaluation(depth, plyFromRoot, alpha, beta);
        if (ttInfo.Item2 != TranspositionTable.LookupFailed)
        {
            //Set the best move
            if (plyFromRoot == 0)
            {
                bestMoveThisIteration = ttInfo.Item1;
            }
            return ttInfo.Item2;
        }
        Move ttMove = ttInfo.Item1;

        //Quiescence search
        if (depth <= 0)
        {
            int eval = QuiescenceSearch(alpha, beta, plyFromRoot + 1);
            return eval;
        }

        //IIR
        if(depth >= 5 && ttMove.isNull())
        {
            depth--;
        }

        int staticEval = evaluation.EvaluatePosition(board);
        staticEvals[board.fullMoveClock] = board.gameStateHistory[board.fullMoveClock].isInCheck ? NEGATIVE_INFINITY : staticEval;
        bool isImproving = isPositionImproving(board.fullMoveClock, board.gameStateHistory[board.fullMoveClock].isInCheck);

        if (plyFromRoot > 0 )
        {
            //NMP
            if (depth > 2)
            {
                int currentColorIndex = (board.colorTurn == Piece.White) ? Board.WhiteIndex : Board.BlackIndex;
                int nonPawnCount = board.pieceCounts[currentColorIndex, Piece.Knight] + board.pieceCounts[currentColorIndex, Piece.Bishop] + board.pieceCounts[currentColorIndex, Piece.Rook] + board.pieceCounts[currentColorIndex, Piece.Queen];
                if (!board.gameStateHistory[board.fullMoveClock].isInCheck && nonPawnCount > 0 && staticEval > beta)
                {
                    int r = 2;

                    board.MakeNullMove();
                    moveOrder.movesAndPieceTypes[board.fullMoveClock] = (nullMove, 0);
                    int eval = -SearchMoves(depth - r - 1, plyFromRoot + 1, -beta, -(beta - 1), numCheckExtensions);
                    board.UnmakeNullMove();

                    if (abortSearch) { return 0; }
                    if (eval >= beta) {return eval; }
                }
            }
            //RFP
            if (depth < 4 && !board.gameStateHistory[board.fullMoveClock].isInCheck && staticEval >= beta + (isImproving ? RFP_IMPROVING_MARGIN : RFP_MARGIN) * depth )
            {
                return staticEval;
            }
        }


        MoveOrder.Stage stage = MoveOrder.Stage.Other;
        Span<Move> legalMoves = stackalloc Move[218];
        int numLegalMoves = MoveGenerator.GenerateLegalMoves(board, ref legalMoves, board.colorTurn);
        

        //Check for mate or stalemate
        if (numLegalMoves == 0)
        {
            if (board.gameStateHistory[board.fullMoveClock].isInCheck){ return CHECKMATE + plyFromRoot;}
            else { return 0; }
        }

        //Move ordering
        int[] moveScores = moveOrder.ScoreMoves(board, legalMoves, (plyFromRoot == 0) ? bestMove : ttMove);

        int evaluationBound = TranspositionTable.UpperBound;
        Move bestMoveInThisPosition = nullMove;


        int bestScore = NEGATIVE_INFINITY;
        int moveNum = -1;
        while(stage != MoveOrder.Stage.Finished)
        {
            moveNum++;
            Move currentMove = moveOrder.GetNextBestMove(moveScores, legalMoves, moveNum);
            if(moveNum == numLegalMoves - 1){ stage++; }

            //Store the move and piece type
            moveOrder.movesAndPieceTypes[board.fullMoveClock] = (currentMove, Piece.PieceType(board.board[currentMove.oldIndex]));


            bool isTactical = currentMove.isCapture() || currentMove.isPromotion();

            if(!board.gameStateHistory[board.fullMoveClock].isInCheck && !isTactical)
            {
                //Futility pruning
                if (depth < 4 && (staticEval + (150 * depth)) < alpha) { continue; }
                //Late Move pruning
                if(moveNum > 10 + depth * depth ){ continue; }
            }

            //SEE pruning
            int seeMargin = isTactical ? SEE_NOISY_MARGIN * depth : SEE_QUIET_MARGIN * depth;
            if(depth < 5 && !SEE(currentMove, seeMargin)){ continue; }


            board.Move(currentMove, true);
            logger.currentDiagnostics.nodesSearched++;

            //Check extension
            int extension = (numLegalMoves == 1) ? 1 : 0;
            if (board.gameStateHistory[board.fullMoveClock].isInCheck && numCheckExtensions < 15 && extension == 0)
            {
                extension = 1;
                numCheckExtensions++;
            }


            int reductions = 0;
            //LMR
            if (moveNum > 0 && depth > 3)
            {
                reductions = 1 + (int)(Math.Log(moveNum) * Math.Log(depth) / 3);
            }

            int eval = -SearchMoves(depth + extension - 1 - reductions, plyFromRoot + 1, -beta, -alpha, numCheckExtensions);

            if (eval > alpha && reductions > 0)
            {
                eval = -SearchMoves(depth + extension - 1, plyFromRoot + 1, -beta, -alpha, numCheckExtensions);
            }

            board.UndoMove(currentMove);

            if (abortSearch) { return 0; }

            if (eval > bestScore)
            {
                bestScore = eval;
                bestMoveInThisPosition = currentMove;
                //If this is a root move, set it to the best move
                if (plyFromRoot == 0)
                {
                    bestMoveThisIteration = currentMove;
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
                tt.StoreEvaluation(depth - reductions, plyFromRoot, bestScore, TranspositionTable.LowerBound, currentMove);
                //Saving quiet move to killers
                if (!currentMove.isCapture())
                {
                    moveOrder.UpdateMoveOrderTables(depth, board.fullMoveClock, board.colorTurn);
                    if (moveNum > 0)
                    {
                        moveOrder.ApplyHistoryPenalties(ref legalMoves, moveNum, depth, board);
                    }
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
            return CHECKMATE + plyFromRoot;
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

        MoveOrder.Stage stage = MoveOrder.Stage.Other;
        if(numMoves == 0){ stage++; }

        int moveNum = -1;
        while(stage !=  MoveOrder.Stage.Finished)
        {
            moveNum++;
            Move currentMove = moveOrder.GetNextBestMove(moveScores, legalMoves, moveNum);
            if(moveNum == numMoves - 1){ stage++; }

            //Skip a move with a bad SEE
            if(!SEE(currentMove, 0)){ continue; }

            //Delta pruning
            if ((standPat + getCapturedPieceVal(currentMove) + 150) < alpha){ continue; }

            board.Move(currentMove, true);
            logger.currentDiagnostics.nodesSearched++;
            int eval = -QuiescenceSearch(-beta, -alpha, plyFromRoot + 1);
            board.UndoMove(currentMove);
            
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
            pieceVal = GetPieceValue(pieceType);
        }
        else { pieceVal = Evaluation.pawnValue; }
        return pieceVal;
    }

    int GetPieceValue(int pieceType)
    {
        switch (pieceType)
        {
            case Piece.Pawn: return Evaluation.pawnValue;
            case Piece.Knight: return Evaluation.knightValue; 
            case Piece.Rook: return Evaluation.rookValue; 
            case Piece.Bishop: return Evaluation.bishopValue; 
            case Piece.Queen: return Evaluation.queenValue;
            default: return 0;
        }
    }

    bool isPositionImproving(int fullMoveClock, bool isInCheck)
    {
        if (isInCheck) { return false; }
        if (fullMoveClock > 1 && staticEvals[fullMoveClock - 2] != NEGATIVE_INFINITY) { return staticEvals[fullMoveClock] > staticEvals[fullMoveClock - 2]; }
        return true;
    }

    bool SEE(Move move, int threshold)
    {
        //Implementation from ethereal
        int nextVictim = move.isPromotion() ? move.PromotedPieceType() : board.MovedPieceType(move);
        int balance = EstimatedCaptureValue(move) - threshold;

        //Capture is not worth the threshold
        if(balance < 0){ return false; }
        
        //If the moved piece is captured and we are still better, it is a good capture
        balance -= SEEPieceVals[nextVictim];
        if(balance >= 0){ return true; }

        ulong bishops = board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Bishop)] | board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Bishop)] | board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Queen)] | board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Queen)];
        ulong rooks = board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Rook)] | board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Rook)] | board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Queen)] | board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Queen)];

        //Update occupancy
        ulong allPieces = board.allPiecesBitboard;
        allPieces = (allPieces ^ (1ul<<move.oldIndex)) | (1ul<<move.newIndex);
        if(move.flag == 7){ allPieces ^= 1ul<<board.enPassantIndex; }

        ulong attackers = board.GetAttackersToSquare(move.newIndex, allPieces, rooks, bishops) & allPieces;

        int currentColorIndex = board.colorTurn == Piece.White ? Board.BlackIndex : Board.WhiteIndex;

        ulong myAttackers;
        while (true)
        {
            myAttackers = attackers & board.sideBitboard[currentColorIndex];
            if(myAttackers == 0){ break; }
            for(nextVictim = Piece.Pawn; nextVictim <= Piece.Queen; nextVictim++)
            {
                if((myAttackers & board.pieceBitboards[Board.PieceBitboardIndex(currentColorIndex, nextVictim)]) != 0){ break; }
            }


            //Update occupancy
            allPieces ^= (1ul << BitOperations.TrailingZeroCount(myAttackers & board.pieceBitboards[Board.PieceBitboardIndex(currentColorIndex, nextVictim)]));

            //A diagonal move can reveal a bishop or a queen attacker
            if(nextVictim == Piece.Pawn || nextVictim == Piece.Bishop || nextVictim == Piece.Queen)
            {
                attackers |= BitboardHelper.GetBishopAttacks(move.newIndex, allPieces) & bishops;
            }
            //Rook or queen move can reveal a rook or queen attacker
            if(nextVictim == Piece.Rook || nextVictim == Piece.Queen)
            {
                attackers |= BitboardHelper.GetRookAttacks(move.newIndex, allPieces) & rooks;
            }

            //Remove any already used attacks
            attackers &= allPieces;

            currentColorIndex = 1 - currentColorIndex;

            balance = -balance - 1 - SEEPieceVals[nextVictim];

            if(balance >= 0)
            {
                //If our last attacking piece is a king, and the opponent has attackers, we have lost as the move we followed would be illegal
                if(nextVictim == Piece.King && (attackers & board.sideBitboard[currentColorIndex]) != 0)
                {
                    currentColorIndex = 1 - currentColorIndex;
                }
                break;
            }
        }
        return ((board.colorTurn == Piece.White) ? Board.WhiteIndex : Board.BlackIndex) != currentColorIndex;
    }

    int EstimatedCaptureValue(Move move)
    {
        if(move.flag == 7){ return SEEPieceVals[Piece.Pawn]; }
        else if (move.isPromotion()){
            return SEEPieceVals[Piece.PieceType(board.board[move.newIndex])] + SEEPieceVals[move.PromotedPieceType()] - SEEPieceVals[Piece.Pawn];
        }
        else
        {
            return SEEPieceVals[Piece.PieceType(board.board[move.newIndex])];
        }
    }

    public static bool IsMateScore(int score)
    {
        const int maxMatePly = 150;
        return Math.Abs(score) > (POSITIVE_INFINITY - maxMatePly);
    }
    public void TriggerSoftCap(){ softCapHit = true; }
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

