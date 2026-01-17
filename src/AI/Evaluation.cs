using System;
using System.Linq.Expressions;
using System.Net.Security;

public class Evaluation
{

    int colorTurn;

    int numWhiteIsolatedPawns;
    int numBlackIsolatedPawns;
    SearchLogger logger;

    public static int pawnValue = 90;
    public static int knightValue = 336;
    public static int bishopValue = 366;
    public static int rookValue = 538;
    public static int queenValue = 1024;
    public static int protectedPawnBonus = 5;
    public static int doubledPawnPenalty = 20;

    public static int[,] mg_PSQT = {
        //Piece.None
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        //Piece.Pawn
        {0, 0, 0, 0, 0, 0, 0, 0, 35, 55, 30, 58, 52, 65, 34, -20, -48, -21, 23, 9, 14, 40, 9, -38, -30, -16, -3, 6, 9, -3, -3, -25, -48, -13, -12, 5, 6, -17, -11, -54, -32, -2, -15, -7, -3, -10, 7, -28, -29, 14, -2, -16, -13, -6, 17, -30, 0, 0, 0, 0, 0, 0, 0, 0},
        //Piece.Knight
        {-186, -95, -67, -35, 23, -97, -35, -157, -50, -32, 89, 49, 60, 76, 9, -21, -3, 49, 61, 90, 97, 77, 55, 11, 24, 24, 31, 46, 49, 39, 24, 32, -7, 16, 33, 30, 28, 35, 6, -9, -7, 19, 30, 30, 34, 28, 16, -12, 3, -5, 7, 18, 17, 6, -6, -5, -83, -9, -10, -16, -16, -20, -11, -37},
        //Piece.Bishop
        {-34, -17, -68, -51, -50, -26, -14, -47, 15, 33, 35, 15, 26, 13, 26, 11, 33, 38, 71, 60, 46, 61, 54, 42, -5, 22, 18, 47, 33, 36, 13, 11, -3, 28, 33, 53, 48, 26, 32, -1, 36, 29, 40, 36, 28, 35, 30, 31, 15, 53, 32, 21, 23, 30, 52, 29, 7, 11, 9, 2, -7, 11, -14, 17},
        //Piece.Rook
        {29, 46, 33, 48, 67, 42, 28, 48, 21, 24, 43, 52, 55, 68, 19, 30, -5, 19, 24, 50, 39, 13, 38, 26, -14, -6, 6, 1, 28, 16, 1, 2, -31, -24, -23, -4, -15, -24, -11, -24, -31, -14, -13, -7, -10, -22, -4, -26, -41, -22, -25, -23, -14, -17, -16, -58, -19, -17, -9, 4, 2, -8, -27, -21},
        //Piece.Queen
        {-39, -1, 10, 46, 68, 73, 34, -22, -5, -21, 4, -17, -30, 18, -31, 3, 11, 11, 9, 40, 42, 20, 41, 31, 11, -21, 4, -8, 0, -2, -9, 16, -12, 18, -2, -4, 5, -12, 6, -17, 0, 9, -5, 4, -1, 12, 6, -25, -8, 19, 13, 1, -1, 14, 15, -11, 16, -12, -19, 7, 8, -8, -17, -8},
        //Piece.King
        {-32, 44, 29, 22, -36, -3, 20, 1, 23, 18, 18, -11, -7, 33, -13, -40, -15, 59, 32, -33, -21, -4, 36, -6, -32, -8, -17, -25, -20, -11, -20, -21, -73, 1, -32, -53, -55, -54, -34, -51, -8, 12, -45, -66, -73, -67, -17, 8, 14, 14, -34, -79, -77, -35, 19, 34, 10, 38, -23, 4, 6, -27, 41, 28}
    };

    public static int[,] eg_PSQT = {
        //Piece.None
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        //Piece.Pawn
        {0, 0, 0, 0, 0, 0, 0, 0, 152, 142, 137, 95, 100, 104, 162, 165, 123, 119, 95, 85, 86, 82, 122, 131, 47, 44, 25, 13, 13, 24, 42, 44, 24, 18, 7, 6, 4, 9, 23, 27, 17, 18, 12, 16, 17, 11, 13, 14, 20, 20, 29, 19, 27, 31, 21, 19, 0, 0, 0, 0, 0, 0, 0, 0},
        //Piece.Knight
        {-58, -60, -43, -35, -62, -15, -73, -91, -41, -22, -48, -36, -37, -41, -43, -68, -36, -32, -3, -15, -18, -21, -40, -46, -33, -4, 13, 14, 16, -2, -2, -37, -23, -18, 6, 11, 21, 2, -6, -29, -43, -23, -16, -5, -3, -17, -13, -50, -54, -25, -22, -15, -22, -24, -43, -58, -37, -48, -35, -28, -39, -36, -54, -68},
        //Piece.Bishop
        {-25, -42, -24, -38, -36, -36, -51, -26, -59, -26, -29, -45, -30, -33, -31, -55, -32, -26, -25, -30, -30, -18, -31, -26, -25, -21, -12, -6, -6, -30, -11, -29, -35, -35, -13, -27, -14, -8, -27, -45, -44, -36, -28, -11, -10, -21, -38, -47, -43, -38, -43, -19, -18, -35, -40, -51, -43, -53, -48, -30, -24, -47, -51, -52},
        //Piece.Rook
        {22, 11, 24, 20, 9, 22, 16, 18, 15, 28, 19, 10, 10, 11, 19, 12, 19, 10, 15, 0, -1, 11, 6, 9, 16, 8, 14, 17, 7, 11, 9, 11, 12, 7, 10, 15, 13, 12, 5, 5, -4, -1, -2, 6, 6, 9, 0, -1, 3, 6, 17, 16, 5, 8, 1, 10, -18, 11, 10, 11, 14, 9, 21, -8},
        //Piece.Queen
        {25, 38, 35, 52, 15, 19, -7, 31, 15, 28, 58, 43, 89, 47, 34, -5, 29, 28, 58, 63, 62, 33, 17, -14, 15, 61, 52, 85, 64, 63, 54, 11, -5, 29, 36, 68, 54, 46, 36, 35, -4, 10, 42, 24, 29, 23, -1, 11, -5, -11, -10, 13, 37, -9, -11, -8, -18, -14, -17, -18, -30, -7, -6, 7},
        //Piece.King
        {-47, 0, -14, -15, -2, -1, -17, -40, -9, 18, 21, 9, 3, 6, 18, 10, 4, 21, 21, 18, 19, 25, 23, 8, 1, 15, 28, 21, 18, 19, 17, -12, -1, 3, 21, 31, 31, 31, 13, -11, -14, 4, 22, 31, 33, 25, 6, -20, -21, -5, 19, 30, 27, 19, -4, -31, -53, -34, -13, -35, -36, -16, -34, -65}
    };
    public static int[] passedPawnBonuses = {0, 15, 23, 39, 63, 98, 53, 0};
    public static int[] isolatedPawnPenalty = {5, -19, -27, -52, -75, -75, -75, -75, -75};

    int playerTurnMultiplier;
    public Evaluation(SearchLogger logger)
    {
        this.logger = logger;
    }
    public int EvaluatePosition(Board board)
    {
        colorTurn = board.colorTurn;
        playerTurnMultiplier = (colorTurn == Piece.White) ? 1 : -1;

        numWhiteIsolatedPawns = 0;
        numBlackIsolatedPawns = 0;
        int boardVal = IncrementalCount(board);
        return boardVal;
    }

    int IncrementalCount(Board board)
    {
        const int totalPhase = 24;
        int mgMaterialCount = board.gameStateHistory[board.fullMoveClock].mgPSQTVal;
        int egMaterialCount = board.gameStateHistory[board.fullMoveClock].egPSQTVal;

        int pawnEval = 0;

        ulong whitePawns = board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Pawn)];
        ulong blackPawns = board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Pawn)];

        while (whitePawns != 0)
        {
            int index = BitboardHelper.PopLSB(ref whitePawns);
            pawnEval += EvaluatePawnStrength(board, index, Piece.White);
        }

        while (blackPawns != 0)
        {
            int index = BitboardHelper.PopLSB(ref blackPawns);
            pawnEval -= EvaluatePawnStrength(board, index, Piece.Black);
        }


        pawnEval += isolatedPawnPenalty[numWhiteIsolatedPawns];
        pawnEval -= isolatedPawnPenalty[numBlackIsolatedPawns];

        mgMaterialCount += pawnValue * (board.pieceCounts[Board.WhiteIndex, Piece.Pawn] - board.pieceCounts[Board.BlackIndex, Piece.Pawn]);
        egMaterialCount += pawnValue * (board.pieceCounts[Board.WhiteIndex, Piece.Pawn] - board.pieceCounts[Board.BlackIndex, Piece.Pawn]);

        mgMaterialCount += knightValue * (board.pieceCounts[Board.WhiteIndex, Piece.Knight] - board.pieceCounts[Board.BlackIndex, Piece.Knight]);
        egMaterialCount += knightValue * (board.pieceCounts[Board.WhiteIndex, Piece.Knight] - board.pieceCounts[Board.BlackIndex, Piece.Knight]);

        mgMaterialCount += bishopValue * (board.pieceCounts[Board.WhiteIndex, Piece.Bishop] - board.pieceCounts[Board.BlackIndex, Piece.Bishop]);
        egMaterialCount += bishopValue * (board.pieceCounts[Board.WhiteIndex, Piece.Bishop] - board.pieceCounts[Board.BlackIndex, Piece.Bishop]);

        mgMaterialCount += rookValue * (board.pieceCounts[Board.WhiteIndex, Piece.Rook] - board.pieceCounts[Board.BlackIndex, Piece.Rook]);
        egMaterialCount += rookValue * (board.pieceCounts[Board.WhiteIndex, Piece.Rook] - board.pieceCounts[Board.BlackIndex, Piece.Rook]);

        mgMaterialCount += queenValue * (board.pieceCounts[Board.WhiteIndex, Piece.Queen] - board.pieceCounts[Board.BlackIndex, Piece.Queen]);
        egMaterialCount += queenValue * (board.pieceCounts[Board.WhiteIndex, Piece.Queen] - board.pieceCounts[Board.BlackIndex, Piece.Queen]);

        int phase = (4 * (board.pieceCounts[Board.WhiteIndex, Piece.Queen] + board.pieceCounts[Board.BlackIndex, Piece.Queen])) + (2 * (board.pieceCounts[Board.WhiteIndex, Piece.Rook] + board.pieceCounts[Board.BlackIndex, Piece.Rook]));
        phase += board.pieceCounts[Board.WhiteIndex, Piece.Knight] + board.pieceCounts[Board.BlackIndex, Piece.Knight] + board.pieceCounts[Board.WhiteIndex, Piece.Bishop] + board.pieceCounts[Board.BlackIndex, Piece.Bishop];

        int egScore = egMaterialCount + pawnEval;
        if (phase > 24) { phase = 24; }
        return (mgMaterialCount * phase + egScore * (totalPhase - phase)) / totalPhase * playerTurnMultiplier;
    }


    int CountMaterial(Board board)
    {
        const int totalPhase = 24;
        int phase = 0;

        int mgMaterialCount = 0;
        int egMaterialCount = 0;

        int pawnEval = 0;

        ulong whitePawns = board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Pawn)];
        ulong blackPawns = board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Pawn)];

        ulong whiteKnights = board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Knight)];
        ulong blackKnights = board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Knight)];

        ulong whiteBishops = board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Bishop)];
        ulong blackBishops = board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Bishop)];

        ulong whiteRooks = board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Rook)];
        ulong blackRooks = board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Rook)];

        ulong whiteQueens = board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Queen)];
        ulong blackQueens = board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Queen)];

        ulong whiteKing = board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.King)];
        ulong blackKing = board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.King)];

        while (whitePawns != 0)
        {
            int index = BitboardHelper.PopLSB(ref whitePawns);
            mgMaterialCount += pawnValue + mg_PSQT[Piece.Pawn, index];
            egMaterialCount += pawnValue + eg_PSQT[Piece.Pawn, index];
            pawnEval += EvaluatePawnStrength(board, index, Piece.White);
        }
        

        while (blackPawns != 0)
        {
            int index = BitboardHelper.PopLSB(ref blackPawns);
            mgMaterialCount -= pawnValue + mg_PSQT[Piece.Pawn, 63 - index];
            egMaterialCount -= pawnValue + eg_PSQT[Piece.Pawn, 63 - index];
            pawnEval -= EvaluatePawnStrength(board, index, Piece.Black);
        }


        while (whiteKnights != 0)
        {
            int index = BitboardHelper.PopLSB(ref whiteKnights);
            mgMaterialCount += knightValue + mg_PSQT[Piece.Knight, index];
            egMaterialCount += knightValue + eg_PSQT[Piece.Knight, index];
            phase += 1;
        }
        while (blackKnights != 0)
        {
            int index = BitboardHelper.PopLSB(ref blackKnights);
            mgMaterialCount -= knightValue + mg_PSQT[Piece.Knight, 63 - index];
            egMaterialCount -= knightValue + eg_PSQT[Piece.Knight, 63 - index];
            phase += 1;
        }

        while (whiteBishops != 0)
        {
            int index = BitboardHelper.PopLSB(ref whiteBishops);
            mgMaterialCount += bishopValue + mg_PSQT[Piece.Bishop, index];
            egMaterialCount += bishopValue + eg_PSQT[Piece.Bishop, index];
            phase += 1;
        }
        while (blackBishops != 0)
        {
            int index = BitboardHelper.PopLSB(ref blackBishops);
            mgMaterialCount -= bishopValue + mg_PSQT[Piece.Bishop, 63 - index];
            egMaterialCount -= bishopValue + eg_PSQT[Piece.Bishop, 63 - index];
            phase += 1;
        }

        while (whiteRooks != 0)
        {
            int index = BitboardHelper.PopLSB(ref whiteRooks);
            mgMaterialCount += rookValue + mg_PSQT[Piece.Rook, index];
            egMaterialCount += rookValue + eg_PSQT[Piece.Rook, index];
            phase += 2;
        }

        while (blackRooks != 0)
        {
            int index = BitboardHelper.PopLSB(ref blackRooks);
            mgMaterialCount -= rookValue + mg_PSQT[Piece.Rook, 63 - index];
            egMaterialCount -= rookValue + eg_PSQT[Piece.Rook, 63 - index];
            phase += 2;
        }

        while (whiteQueens != 0)
        {
            int index = BitboardHelper.PopLSB(ref whiteQueens);
            mgMaterialCount += queenValue + mg_PSQT[Piece.Queen, index];
            egMaterialCount += queenValue + eg_PSQT[Piece.Queen, index];
            phase += 4;
        }
        
        while (blackQueens != 0)
        {
            int index = BitboardHelper.PopLSB(ref blackQueens);
            mgMaterialCount -= queenValue + mg_PSQT[Piece.Queen, 63 - index];
            egMaterialCount -= queenValue + eg_PSQT[Piece.Queen, 63 - index];
            phase += 4;
        }


        while (whiteKing != 0)
        {
            int index = BitboardHelper.PopLSB(ref whiteKing);
            mgMaterialCount += mg_PSQT[Piece.King, index];
            egMaterialCount += eg_PSQT[Piece.King, index];
        }
        while (blackKing != 0)
        {
            int index = BitboardHelper.PopLSB(ref blackKing);
            mgMaterialCount -= mg_PSQT[Piece.King, 63 - index];
            egMaterialCount -= eg_PSQT[Piece.King, 63 - index];
        }

        pawnEval += isolatedPawnPenalty[numWhiteIsolatedPawns];
        pawnEval -= isolatedPawnPenalty[numBlackIsolatedPawns];

        int egScore = egMaterialCount + pawnEval;

        if (phase > 24) { phase = 24; }
        return (mgMaterialCount * phase + egScore * (totalPhase - phase)) / totalPhase * playerTurnMultiplier;
    }

    int EvaluateKingSafety(Board board, int kingIndex, int kingColor)
    {
        int penaltyMultiplier;
        int numPenalties = 0;
        if (kingColor == Piece.White)
        {
            //Pawn shield
            //Not back rank
            if (Coord.IndexToRank(kingIndex) != 8)
            {
                //White Pawn in front
                if (Piece.PieceType(board.board[kingIndex - 8]) != Piece.Pawn || Piece.Color(board.board[kingIndex - 8]) != Piece.White) { numPenalties += 1; }

                //White Pawn front left
                if (Coord.IndexToFile(kingIndex) != 1)
                {
                    if (Piece.PieceType(board.board[kingIndex - 9]) != Piece.Pawn || Piece.Color(board.board[kingIndex - 9]) != Piece.White) { numPenalties += 1; }
                }
                //White Pawn front right
                if (Coord.IndexToFile(kingIndex) != 8)
                {
                    if (Piece.PieceType(board.board[kingIndex - 7]) != Piece.Pawn || Piece.Color(board.board[kingIndex - 7]) != Piece.White) { numPenalties += 1; }
                }
            }
            penaltyMultiplier = (!board.HasKingsideRight(Piece.White) && !board.HasQueensideRight(Piece.White)) ? 6 : 1;
        }
        else
        {
            //Pawn shield
            //Not back rank
            if (Coord.IndexToRank(kingIndex) != 1)
            {
                //Black Pawn in front
                if (Piece.PieceType(board.board[kingIndex + 8]) != Piece.Pawn || Piece.Color(board.board[kingIndex + 8]) != Piece.White) { numPenalties += 1; }

                //Black Pawn front left
                if (Coord.IndexToFile(kingIndex) != 1)
                {
                    if (Piece.PieceType(board.board[kingIndex + 7]) != Piece.Pawn || Piece.Color(board.board[kingIndex + 7]) != Piece.White) { numPenalties += 1; }
                }
                //Black Pawn front right
                if (Coord.IndexToFile(kingIndex) != 8)
                {
                    if (Piece.PieceType(board.board[kingIndex + 9]) != Piece.Pawn || Piece.Color(board.board[kingIndex + 9]) != Piece.White) { numPenalties += 1; }
                }
            }
            penaltyMultiplier = (!board.HasKingsideRight(Piece.Black) && !board.HasQueensideRight(Piece.Black)) ? 6 : 1;
        }
        return numPenalties * -5 * penaltyMultiplier;
    }

    int EvaluatePawnStrength(Board board, int pawnIndex, int pawnColor)
    {
        int bonus = 0;
        if (pawnColor == Piece.White)
        {
            int ppBonusIndex = Coord.IndexToRank(pawnIndex) - 1;
            //Passed pawn
            if ((board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Pawn)] & BitboardHelper.wPawnPassedMask[pawnIndex]) == 0) { bonus += passedPawnBonuses[ppBonusIndex]; }
            //Doubled pawn penalty
            if (Piece.PieceType(board.board[pawnIndex - 8]) == Piece.Pawn && Piece.Color(board.board[pawnIndex - 8]) == Piece.White) { bonus -= doubledPawnPenalty; }
            //Defended from left
            if (Coord.IndexToFile(pawnIndex) != 1 && Piece.PieceType(board.board[pawnIndex + 7]) == Piece.Pawn && Piece.Color(board.board[pawnIndex + 7]) == Piece.White) { bonus += protectedPawnBonus; }
            //Defended from right
            if (Coord.IndexToFile(pawnIndex) != 8 && Piece.PieceType(board.board[pawnIndex + 9]) == Piece.Pawn && Piece.Color(board.board[pawnIndex + 9]) == Piece.White) { bonus += protectedPawnBonus; }
            if ((BitboardHelper.isolatedPawnMask[pawnIndex] & board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Pawn)]) == 0) { numWhiteIsolatedPawns++; }
        }
        else
        {
            int ppBonusIndex = 8 - Coord.IndexToRank(pawnIndex);
            //Passed pawn
            if ((board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Pawn)] & BitboardHelper.bPawnPassedMask[pawnIndex]) == 0) { bonus += passedPawnBonuses[ppBonusIndex]; }
            //Doubled pawn penalty
            if (Piece.PieceType(board.board[pawnIndex + 8]) == Piece.Pawn && Piece.Color(board.board[pawnIndex + 8]) == Piece.Black) { bonus -= doubledPawnPenalty; }
            //Defended from left
            if (Coord.IndexToFile(pawnIndex) != 1 && Piece.PieceType(board.board[pawnIndex - 9]) == Piece.Pawn && Piece.Color(board.board[pawnIndex - 9]) == Piece.Black) { bonus += protectedPawnBonus; }
            //Defended from right
            if (Coord.IndexToFile(pawnIndex) != 8 && Piece.PieceType(board.board[pawnIndex - 7]) == Piece.Pawn && Piece.Color(board.board[pawnIndex - 7]) == Piece.Black) { bonus += protectedPawnBonus; }
            if((BitboardHelper.isolatedPawnMask[pawnIndex] & board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Pawn)]) == 0){ numBlackIsolatedPawns++; }
        }

        return bonus;
    }

    int EvaluateBishopMobility(Board board, int pieceIndex, int pieceColor)
    {
        int numMoves = 0;
        ulong simpleBishopMoves = BitboardHelper.GetBishopAttacks(pieceIndex, board.allPiecesBitboard) & board.sideBitboard[pieceColor == Piece.White ? Board.WhiteIndex : Board.BlackIndex];
        while (simpleBishopMoves != 0) { numMoves++; BitboardHelper.PopLSB(ref simpleBishopMoves); }
        return (numMoves * 2) - 10;
    }
    int EvaluateRookMobility(Board board, int pieceIndex, int pieceColor)
    {
        int numMoves = 0;
        ulong simpleRookMoves = BitboardHelper.GetRookAttacks(pieceIndex, board.allPiecesBitboard) & board.sideBitboard[pieceColor == Piece.White ? Board.WhiteIndex : Board.BlackIndex];
        while (simpleRookMoves != 0) { numMoves++; BitboardHelper.PopLSB(ref simpleRookMoves); }
        return (numMoves * 2) - 10;
    }

}
