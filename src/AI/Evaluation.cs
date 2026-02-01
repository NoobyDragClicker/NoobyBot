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
        {90, 90, 90, 90, 90, 90, 90, 90, 125, 145, 120, 148, 142, 155, 124, 70, 42, 69, 113, 99, 104, 130, 99, 52, 60, 74, 87, 96, 99, 87, 87, 65, 42, 77, 78, 95, 96, 73, 79, 36, 58, 88, 75, 83, 87, 80, 97, 62, 61, 104, 88, 74, 77, 84, 107, 60, 90, 90, 90, 90, 90, 90, 90, 90},
        //Knight
        {150, 241, 269, 301, 359, 239, 301, 179, 286, 304, 425, 385, 396, 412, 345, 315, 333, 385, 397, 426, 433, 413, 391, 347, 360, 360, 367, 382, 385, 375, 360, 368, 329, 352, 369, 366, 364, 371, 342, 327, 329, 355, 366, 366, 370, 364, 352, 324, 339, 331, 343, 354, 353, 342, 330, 331, 253, 327, 326, 320, 320, 316, 325, 299},
        //Piece.Bishop
        {332, 349, 298, 315, 316, 340, 352, 319, 381, 399, 401, 381, 392, 379, 392, 377, 399, 404, 437, 426, 412, 427, 420, 408, 361, 388, 384, 413, 399, 402, 379, 377, 363, 394, 399, 419, 414, 392, 398, 365, 402, 395, 406, 402, 394, 401, 396, 397, 381, 419, 398, 387, 389, 396, 418, 395, 373, 377, 375, 368, 359, 377, 352, 383},
        //Rook
        {567, 584, 571, 586, 605, 580, 566, 586, 559, 562, 581, 590, 593, 606, 557, 568, 533, 557, 562, 588, 577, 551, 576, 564, 524, 532, 544, 539, 566, 554, 539, 540, 507, 514, 515, 534, 523, 514, 527, 514, 507, 524, 525, 531, 528, 516, 534, 512, 497, 516, 513, 515, 524, 521, 522, 480, 519, 521, 529, 542, 540, 530, 511, 517},
        //Piece.Queen
        {985, 1023, 1034, 1070, 1092, 1097, 1058, 1002, 1019, 1003, 1028, 1007, 994, 1042, 993, 1027, 1035, 1035, 1033, 1064, 1066, 1044, 1065, 1055, 1035, 1003, 1028, 1016, 1024, 1022, 1015, 1040, 1012, 1042, 1022, 1020, 1029, 1012, 1030, 1007, 1024, 1033, 1019, 1028, 1023, 1036, 1030, 999, 1016, 1043, 1037, 1025, 1023, 1038, 1039, 1013, 1040, 1012, 1005, 1031, 1032, 1016, 1007, 1016},
        //Piece.King
        {-32, 44, 29, 22, -36, -3, 20, 1, 23, 18, 18, -11, -7, 33, -13, -40, -15, 59, 32, -33, -21, -4, 36, -6, -32, -8, -17, -25, -20, -11, -20, -21, -73, 1, -32, -53, -55, -54, -34, -51, -8, 12, -45, -66, -73, -67, -17, 8, 14, 14, -34, -79, -77, -35, 19, 34, 10, 38, -23, 4, 6, -27, 41, 28}
    };

    public static int[,] eg_PSQT = {
        //Piece.None
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        //Piece.Pawn
        {90, 90, 90, 90, 90, 90, 90, 90, 242, 232, 227, 185, 190, 194, 252, 255, 213, 209, 185, 175, 176, 172, 212, 221, 137, 134, 115, 103, 103, 114, 132, 134, 114, 108, 97, 96, 94, 99, 113, 117, 107, 108, 102, 106, 107, 101, 103, 104, 110, 110, 119, 109, 117, 121, 111, 109, 90, 90, 90, 90, 90, 90, 90, 90},
        //Piece.Knight
        {278, 276, 293, 301, 274, 321, 263, 245, 295, 314, 288, 300, 299, 295, 293, 268, 300, 304, 333, 321, 318, 315, 296, 290, 303, 332, 349, 350, 352, 334, 334, 299, 313, 318, 342, 347, 357, 338, 330, 307, 293, 313, 320, 331, 333, 319, 323, 286, 282, 311, 314, 321, 314, 312, 293, 278, 299, 288, 301, 308, 297, 300, 282, 268},
        //Piece.Bishop
        {341, 324, 342, 328, 330, 330, 315, 340, 307, 340, 337, 321, 336, 333, 335, 311, 334, 340, 341, 336, 336, 348, 335, 340, 341, 345, 354, 360, 360, 336, 355, 337, 331, 331, 353, 339, 352, 358, 339, 321, 322, 330, 338, 355, 356, 345, 328, 319, 323, 328, 323, 347, 348, 331, 326, 315, 323, 313, 318, 336, 342, 319, 315, 314},
        //Piece.Rook
        {560, 549, 562, 558, 547, 560, 554, 556, 553, 566, 557, 548, 548, 549, 557, 550, 557, 548, 553, 538, 537, 549, 544, 547, 554, 546, 552, 555, 545, 549, 547, 549, 550, 545, 548, 553, 551, 550, 543, 543, 534, 537, 536, 544, 544, 547, 538, 537, 541, 544, 555, 554, 543, 546, 539, 548, 520, 549, 548, 549, 552, 547, 559, 530},
        //Piece.Queen
        {1049, 1062, 1059, 1076, 1039, 1043, 1017, 1055, 1039, 1052, 1082, 1067, 1113, 1071, 1058, 1019, 1053, 1052, 1082, 1087, 1086, 1057, 1041, 1010, 1039, 1085, 1076, 1109, 1088, 1087, 1078, 1035, 1019, 1053, 1060, 1092, 1078, 1070, 1060, 1059, 1020, 1034, 1066, 1048, 1053, 1047, 1023, 1035, 1019, 1013, 1014, 1037, 1061, 1015, 1013, 1016, 1006, 1010, 1007, 1006, 994, 1017, 1018, 1031},
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
                if (board.PieceAt(kingIndex - 8) != Piece.Pawn || board.ColorAt(kingIndex - 8) != Piece.White) { numPenalties += 1; }

                //White Pawn front left
                if (Coord.IndexToFile(kingIndex) != 1)
                {
                    if (board.PieceAt(kingIndex - 9) != Piece.Pawn || board.ColorAt(kingIndex - 9) != Piece.White) { numPenalties += 1; }
                }
                //White Pawn front right
                if (Coord.IndexToFile(kingIndex) != 8)
                {
                    if (board.PieceAt(kingIndex - 7) != Piece.Pawn || board.ColorAt(kingIndex - 7) != Piece.White) { numPenalties += 1; }
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
                if (board.PieceAt(kingIndex + 8) != Piece.Pawn || board.ColorAt(kingIndex + 8) != Piece.White) { numPenalties += 1; }

                //Black Pawn front left
                if (Coord.IndexToFile(kingIndex) != 1)
                {
                    if (board.PieceAt(kingIndex + 7) != Piece.Pawn || board.ColorAt(kingIndex + 7) != Piece.White) { numPenalties += 1; }
                }
                //Black Pawn front right
                if (Coord.IndexToFile(kingIndex) != 8)
                {
                    if (board.PieceAt(kingIndex + 9) != Piece.Pawn || board.ColorAt(kingIndex + 9) != Piece.White) { numPenalties += 1; }
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
            if (board.PieceAt(pawnIndex - 8) == Piece.Pawn && board.ColorAt(pawnIndex - 8) == Piece.White) { bonus -= doubledPawnPenalty; }
            if ((BitboardHelper.isolatedPawnMask[pawnIndex] & board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Pawn)]) == 0) { numWhiteIsolatedPawns++; }
        }
        else
        {
            int ppBonusIndex = 8 - Coord.IndexToRank(pawnIndex);
            //Passed pawn
            if ((board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Pawn)] & BitboardHelper.bPawnPassedMask[pawnIndex]) == 0) { bonus += passedPawnBonuses[ppBonusIndex]; }
            //Doubled pawn penalty
            if (board.PieceAt(pawnIndex + 8) == Piece.Pawn && board.ColorAt(pawnIndex + 8) == Piece.Black) { bonus -= doubledPawnPenalty; }
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
