using System;
using System.Drawing;
using System.Linq.Expressions;
using System.Net.Security;

public class Evaluation
{

    int colorTurn;

    int[] isolatedPawnCount;
    SearchLogger logger;

    //Unused in actual eval
    public static int pawnValue = 90;
    public static int knightValue = 336;
    public static int bishopValue = 366;
    public static int rookValue = 538;
    public static int queenValue = 1024;
    
    public static int[,] mg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 68, 79, 69, 82, 70, 60, 28, 4, 60, 79, 102, 102, 109, 150, 135, 90, 44, 69, 68, 74, 93, 82, 84, 64, 30, 60, 60, 77, 77, 68, 65, 46, 30, 54, 56, 60, 74, 61, 83, 55, 29, 58, 47, 53, 56, 74, 82, 45, 0, 0, 0, 0, 0, 0, 0, 0},
        {139, 159, 207, 244, 254, 212, 174, 181, 230, 255, 274, 292, 276, 335, 247, 276, 236, 280, 309, 314, 345, 343, 309, 271, 239, 256, 281, 311, 290, 313, 268, 276, 222, 238, 262, 267, 278, 269, 262, 238, 203, 230, 250, 254, 269, 259, 255, 223, 186, 201, 219, 242, 240, 241, 222, 223, 142, 200, 179, 202, 209, 220, 207, 168},
        {194, 166, 160, 130, 159, 160, 159, 166, 195, 195, 183, 180, 207, 195, 198, 184, 194, 206, 200, 214, 195, 241, 214, 223, 190, 184, 194, 209, 202, 196, 186, 191, 181, 176, 173, 196, 191, 173, 177, 195, 190, 190, 185, 177, 182, 186, 193, 207, 197, 197, 195, 176, 183, 195, 219, 200, 185, 199, 184, 164, 176, 181, 193, 201},
        {340, 325, 336, 336, 360, 350, 337, 375, 334, 336, 357, 376, 363, 394, 374, 399, 316, 341, 347, 352, 373, 380, 406, 382, 302, 316, 320, 331, 339, 342, 342, 346, 282, 291, 297, 311, 316, 304, 320, 314, 278, 285, 298, 299, 308, 308, 338, 318, 275, 288, 305, 305, 308, 314, 328, 298, 299, 299, 309, 315, 322, 312, 322, 304},
        {477, 495, 527, 558, 559, 587, 582, 526, 518, 490, 508, 505, 520, 547, 526, 560, 512, 514, 518, 528, 531, 578, 574, 561, 494, 492, 507, 503, 511, 518, 516, 520, 483, 490, 486, 497, 501, 497, 505, 507, 482, 488, 488, 489, 494, 496, 510, 506, 476, 488, 497, 502, 496, 507, 515, 522, 465, 460, 470, 487, 478, 464, 491, 488},
        {-73, -86, -59, -110, -68, 1, 45, 111, -98, -34, -90, -15, -41, -26, 52, 35, -103, 8, -78, -88, -63, 20, 15, -20, -60, -70, -81, -155, -134, -95, -81, -101, -59, -62, -104, -136, -136, -98, -94, -113, -26, -16, -59, -76, -69, -76, -32, -46, 52, 17, -2, -28, -35, -17, 21, 34, 41, 67, 43, -48, 18, -37, 54, 53}
    };
    public static int[,] eg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 149, 126, 142, 120, 119, 119, 136, 151, 124, 123, 113, 114, 103, 98, 120, 120, 115, 104, 105, 91, 94, 94, 94, 96, 101, 97, 98, 100, 97, 94, 86, 83, 99, 92, 100, 102, 106, 101, 81, 83, 106, 98, 108, 107, 123, 106, 86, 87, 0, 0, 0, 0, 0, 0, 0, 0},
        {224, 273, 295, 278, 293, 268, 272, 202, 275, 288, 302, 302, 292, 279, 284, 255, 291, 302, 315, 317, 303, 302, 291, 276, 293, 317, 332, 333, 334, 328, 318, 289, 304, 314, 332, 331, 334, 323, 309, 286, 284, 301, 313, 326, 321, 304, 294, 284, 280, 294, 301, 298, 300, 295, 281, 281, 257, 251, 283, 285, 281, 271, 264, 271},
        {230, 237, 233, 243, 237, 225, 231, 223, 218, 210, 213, 212, 200, 206, 209, 221, 236, 214, 199, 189, 192, 193, 212, 231, 232, 219, 201, 193, 185, 199, 215, 229, 227, 220, 207, 192, 189, 202, 212, 220, 233, 218, 209, 207, 211, 207, 212, 224, 241, 214, 205, 214, 215, 206, 216, 218, 226, 231, 214, 223, 220, 230, 218, 203},
        {557, 563, 572, 568, 558, 559, 562, 546, 558, 566, 568, 557, 558, 548, 551, 537, 558, 557, 557, 550, 543, 540, 536, 532, 560, 556, 564, 556, 543, 544, 541, 535, 555, 556, 557, 551, 547, 548, 536, 535, 547, 545, 542, 546, 539, 534, 513, 517, 541, 543, 540, 540, 531, 530, 517, 524, 545, 547, 551, 548, 541, 543, 534, 532},
        {1018, 1024, 1036, 1028, 1029, 988, 964, 1001, 1002, 1041, 1060, 1072, 1082, 1051, 1029, 1015, 1001, 1017, 1056, 1068, 1076, 1055, 1012, 1022, 1013, 1043, 1052, 1080, 1089, 1076, 1069, 1045, 1027, 1039, 1058, 1077, 1074, 1067, 1048, 1034, 995, 1030, 1047, 1050, 1045, 1046, 1014, 998, 999, 1001, 1009, 1010, 1021, 984, 947, 916, 990, 1002, 1006, 1017, 1002, 983, 947, 941},
        {-74, -36, -23, 5, -11, -6, -4, -92, -17, 12, 23, 11, 28, 34, 27, 3, -5, 16, 37, 48, 50, 44, 39, 7, -16, 18, 41, 59, 60, 54, 40, 13, -23, 6, 35, 53, 54, 40, 24, 7, -31, -5, 15, 30, 30, 24, 5, -8, -34, -16, -6, 2, 8, 3, -10, -31, -69, -57, -39, -28, -48, -21, -47, -74}
    };
    public static int[,] passedPawnBonuses = {
        {0, 0, 0, 0, 0, 0, 0, 0, 68, 79, 69, 82, 70, 60, 28, 4, 30, 29, 21, 12, 9, -4, -37, -55, 15, 6, 20, 17, -0, 9, -27, -21, -3, -14, -24, -10, -21, -10, -23, -14, -8, -22, -27, -23, -26, -17, -25, -1, -14, -14, -14, -19, -10, -10, 1, -10, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 149, 126, 142, 120, 119, 119, 136, 151, 142, 143, 110, 77, 82, 97, 115, 138, 87, 84, 51, 46, 44, 52, 81, 85, 61, 54, 35, 24, 28, 37, 61, 61, 23, 33, 20, 12, 12, 13, 42, 23, 24, 24, 12, 1, -9, 7, 20, 22, 0, 0, 0, 0, 0, 0, 0, 0}
    };
    public static int[] isolatedPawnPenalty = {33, 12, -8, -24, -43, -69, -85, -83, 49};
    public static EvalPair doubledPawnPenalty = new EvalPair(0, -28);
    public static EvalPair bishopPairBonus = new EvalPair(39, 53);
    public static EvalPair bishopMobility = new EvalPair(10, 14);
    public static EvalPair rookOpenFile = new EvalPair(23, 21);
    public static EvalPair kingOpenFile = new EvalPair(-43, 5);
    public static EvalPair kingPawnShield = new EvalPair(19, -8);
    


    int playerTurnMultiplier;
    public Evaluation(SearchLogger logger)
    {
        this.logger = logger;
        isolatedPawnCount = new int[2];
    }
    public int EvaluatePosition(Board board)
    {
        colorTurn = board.colorTurn;
        playerTurnMultiplier = (colorTurn == Piece.White) ? 1 : -1;

        isolatedPawnCount[0] = 0;
        isolatedPawnCount[1] = 0;
        int boardVal = IncrementalCount(board);
        return boardVal;
    }

    int IncrementalCount(Board board)
    {
        const int totalPhase = 24;
        int mgScore = board.gameStateHistory[board.fullMoveClock].mgPSQTVal;
        int egScore = board.gameStateHistory[board.fullMoveClock].egPSQTVal;
        EvalPair score = new EvalPair(mgScore, egScore);


        ulong whitePawns = board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Pawn)];
        ulong blackPawns = board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Pawn)];

        ulong whiteBishops = board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Bishop)];
        ulong blackBishops = board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Bishop)];

        ulong whiteRooks = board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Rook)];
        ulong blackRooks = board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Rook)];

        ulong whiteKing = board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.King)];
        ulong blackKing = board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.King)];


        while (whitePawns != 0)
        {
            int index = BitboardHelper.PopLSB(ref whitePawns);
            score += EvaluatePawnStrength(board, index, Board.WhiteIndex);
        }

        while (blackPawns != 0)
        {
            int index = BitboardHelper.PopLSB(ref blackPawns);
            score -= EvaluatePawnStrength(board, index, Board.BlackIndex);

        }

        while (whiteRooks != 0)
        {
            int index = BitboardHelper.PopLSB(ref whiteRooks);
            if((BitboardHelper.files[index % 8] & (board.allPiecesBitboard ^ 1ul << index)) == 0){ score += rookOpenFile; }
        }

        while (blackRooks != 0)
        {
            int index = BitboardHelper.PopLSB(ref blackRooks);
            if((BitboardHelper.files[index % 8] & (board.allPiecesBitboard ^ 1ul << index)) == 0){ score -= rookOpenFile; }
        }

        while (whiteBishops != 0)
        {
            int index = BitboardHelper.PopLSB(ref whiteBishops);
            score += EvaluateBishopMobility(board, index, Board.WhiteIndex);
        }
        while (blackBishops != 0)
        {
            int index = BitboardHelper.PopLSB(ref blackBishops);
            score -= EvaluateBishopMobility(board, index, Board.BlackIndex);
        }


        int whiteKingIndex = BitboardHelper.PopLSB(ref whiteKing);
        int blackKingIndex = BitboardHelper.PopLSB(ref blackKing);
        score += EvaluateKingSafety(board, whiteKingIndex, Piece.White);
        score -= EvaluateKingSafety(board, blackKingIndex, Piece.Black);


        score.eg += isolatedPawnPenalty[isolatedPawnCount[Board.WhiteIndex]];
        score.eg -= isolatedPawnPenalty[isolatedPawnCount[Board.BlackIndex]];


        if(board.pieceCounts[Board.WhiteIndex, Piece.Bishop] >= 2){ score += bishopPairBonus; }
        if(board.pieceCounts[Board.BlackIndex, Piece.Bishop] >= 2){ score -= bishopPairBonus; }

        int phase = (4 * (board.pieceCounts[Board.WhiteIndex, Piece.Queen] + board.pieceCounts[Board.BlackIndex, Piece.Queen])) + (2 * (board.pieceCounts[Board.WhiteIndex, Piece.Rook] + board.pieceCounts[Board.BlackIndex, Piece.Rook]));
        phase += board.pieceCounts[Board.WhiteIndex, Piece.Knight] + board.pieceCounts[Board.BlackIndex, Piece.Knight] + board.pieceCounts[Board.WhiteIndex, Piece.Bishop] + board.pieceCounts[Board.BlackIndex, Piece.Bishop];

        
        if (phase > 24) { phase = 24; }
        return (score.mg * phase + score.eg * (totalPhase - phase)) / totalPhase * playerTurnMultiplier;
    }


    EvalPair EvaluateKingSafety(Board board, int kingIndex, int kingColor)
    {
        EvalPair score = new EvalPair();

        int currentColorIndex = kingColor == Piece.White ? Board.WhiteIndex : Board.BlackIndex;
        if(((board.sideBitboard[currentColorIndex] ^ 1ul << kingIndex) & BitboardHelper.files[kingIndex % 8]) == 0)
        {
            score += kingOpenFile;
        }
        
        int direction = kingColor == Piece.White ? -1 : 1;
        int frontSquare = kingIndex + (direction * 8);

        if(frontSquare >= 0 && frontSquare <= 63)
        {
            if(BitboardHelper.ContainsSquare(board.pieceBitboards[Board.PieceBitboardIndex(currentColorIndex, Piece.Pawn)], frontSquare))
            {
                score += kingPawnShield;
            }
        }

        return score;
    }

    EvalPair EvaluatePawnStrength(Board board, int pawnIndex, int currentColorIndex)
    {
        int mgBonus = 0;
        int egBonus = 0;

        int oppositeColorIndex = 1 - currentColorIndex;
        int currentColor = currentColorIndex == Board.WhiteIndex ? Piece.White : Piece.Black;

        bool passer = (board.pieceBitboards[Board.PieceBitboardIndex(oppositeColorIndex, Piece.Pawn)] & BitboardHelper.pawnPassedMask[currentColorIndex, pawnIndex]) == 0;
        int pushSquare = pawnIndex + (currentColorIndex == Board.WhiteIndex ? -8 : 8);
        //Passed pawn
        if (passer) { 
            int psqtIndex = currentColorIndex == Board.WhiteIndex ? pawnIndex : pawnIndex ^ 56;
            mgBonus += passedPawnBonuses[0, psqtIndex]; 
            egBonus += passedPawnBonuses[1, psqtIndex]; 
        }

        //Doubled pawn penalty
        if (board.PieceAt(pushSquare) == Piece.Pawn && board.ColorAt(pushSquare) == currentColor) { egBonus += doubledPawnPenalty.eg; }
        if ((BitboardHelper.isolatedPawnMask[pawnIndex] & board.pieceBitboards[Board.PieceBitboardIndex(currentColorIndex, Piece.Pawn)]) == 0) { isolatedPawnCount[currentColorIndex]++; }

        return new EvalPair(mgBonus, egBonus);
    }

    EvalPair EvaluateBishopMobility(Board board, int pieceIndex, int colorIndex)
    {
        int numMoves = 0;
        ulong simpleBishopMoves = BitboardHelper.GetBishopAttacks(pieceIndex, board.allPiecesBitboard);
        while (simpleBishopMoves != 0) { numMoves++; BitboardHelper.PopLSB(ref simpleBishopMoves); }
        return new EvalPair(numMoves * bishopMobility.mg, numMoves * bishopMobility.eg);
    }

    int EvaluateRookMobility(Board board, int pieceIndex, int pieceColor)
    {
        int numMoves = 0;
        ulong simpleRookMoves = BitboardHelper.GetRookAttacks(pieceIndex, board.allPiecesBitboard) & board.sideBitboard[pieceColor == Piece.White ? Board.WhiteIndex : Board.BlackIndex];
        while (simpleRookMoves != 0) { numMoves++; BitboardHelper.PopLSB(ref simpleRookMoves); }
        return (numMoves * 2) - 10;
    }

}

public struct EvalPair
{
    public int mg;
    public int eg;
    public EvalPair(int mg, int eg)
    {
        this.mg = mg;
        this.eg = eg;
    }

    public static EvalPair operator +(EvalPair a, EvalPair b)
    {
        return new EvalPair
        {
            mg = a.mg + b.mg,
            eg = a.eg + b.eg
        };
    }
    public static EvalPair operator -(EvalPair a, EvalPair b)
    {
        return new EvalPair
        {
            mg = a.mg - b.mg,
            eg = a.eg - b.eg
        };
    }
}