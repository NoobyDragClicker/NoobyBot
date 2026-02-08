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
        {0, 0, 0, 0, 0, 0, 0, 0, 55, 74, 62, 72, 61, 52, 22, -3, 59, 78, 99, 100, 105, 145, 132, 87, 46, 70, 68, 73, 93, 81, 83, 63, 35, 62, 61, 78, 78, 71, 66, 49, 38, 56, 57, 61, 76, 67, 85, 60, 41, 61, 48, 54, 60, 82, 86, 51, 0, 0, 0, 0, 0, 0, 0, 0},
        {144, 149, 190, 225, 239, 199, 164, 180, 221, 251, 268, 285, 269, 324, 243, 268, 232, 277, 306, 310, 340, 339, 306, 269, 237, 256, 281, 310, 290, 313, 268, 275, 224, 239, 263, 269, 279, 270, 263, 239, 205, 231, 252, 256, 273, 262, 256, 226, 192, 204, 221, 245, 244, 247, 223, 226, 141, 211, 192, 218, 221, 228, 217, 168},
        {198, 156, 145, 113, 145, 150, 148, 171, 187, 192, 179, 175, 199, 185, 194, 180, 191, 205, 198, 210, 193, 238, 213, 220, 188, 185, 193, 208, 203, 196, 187, 190, 182, 177, 175, 198, 194, 175, 178, 196, 192, 191, 188, 180, 185, 190, 195, 209, 202, 200, 197, 179, 187, 202, 221, 202, 186, 207, 195, 181, 190, 190, 201, 203},
        {320, 293, 293, 285, 309, 301, 303, 351, 312, 309, 325, 342, 332, 360, 350, 377, 293, 313, 314, 317, 342, 348, 379, 361, 282, 291, 292, 297, 306, 315, 316, 323, 267, 268, 271, 281, 290, 283, 297, 291, 266, 264, 274, 275, 288, 291, 318, 301, 266, 271, 287, 288, 293, 303, 314, 288, 289, 290, 301, 310, 314, 307, 313, 295},
        {475, 486, 514, 539, 541, 573, 576, 522, 509, 485, 502, 499, 513, 536, 523, 554, 508, 509, 514, 523, 528, 572, 571, 557, 491, 491, 505, 502, 509, 516, 514, 519, 484, 490, 486, 498, 502, 497, 505, 507, 483, 489, 490, 491, 496, 499, 510, 507, 481, 491, 500, 506, 500, 514, 519, 528, 468, 473, 487, 500, 494, 476, 505, 494},
        {-79, -86, -58, -108, -69, 4, 46, 109, -103, -31, -87, -10, -39, -22, 50, 36, -102, 14, -73, -82, -59, 24, 16, -21, -60, -68, -78, -150, -131, -93, -79, -104, -62, -63, -106, -140, -140, -102, -95, -113, -27, -20, -65, -83, -75, -81, -35, -48, 50, 13, -7, -35, -42, -18, 19, 31, 32, 67, 48, -34, 27, -26, 55, 48}
    };
    public static int[,] eg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 155, 130, 146, 122, 122, 125, 141, 154, 124, 125, 113, 114, 105, 104, 124, 121, 116, 106, 107, 93, 96, 97, 96, 97, 102, 99, 100, 101, 99, 96, 88, 84, 100, 94, 102, 105, 108, 104, 83, 83, 105, 99, 109, 116, 126, 108, 88, 87, 0, 0, 0, 0, 0, 0, 0, 0},
        {234, 279, 302, 284, 300, 274, 277, 211, 282, 293, 308, 308, 298, 286, 289, 261, 297, 307, 321, 321, 308, 309, 297, 282, 297, 321, 337, 336, 337, 333, 321, 294, 307, 319, 337, 336, 340, 329, 314, 289, 287, 306, 315, 333, 327, 307, 299, 289, 285, 299, 308, 305, 306, 300, 286, 286, 263, 278, 292, 295, 291, 279, 288, 276},
        {232, 241, 238, 248, 242, 229, 235, 225, 224, 214, 217, 216, 204, 210, 213, 225, 240, 218, 202, 190, 195, 197, 216, 234, 235, 221, 203, 194, 187, 202, 216, 233, 230, 222, 207, 194, 190, 201, 214, 222, 233, 219, 211, 209, 212, 208, 214, 225, 243, 215, 208, 218, 218, 208, 216, 220, 229, 236, 240, 229, 226, 243, 221, 205},
        {437, 444, 451, 446, 436, 442, 443, 427, 444, 453, 456, 444, 444, 430, 434, 421, 447, 444, 443, 436, 427, 422, 420, 421, 452, 444, 448, 441, 427, 427, 429, 431, 448, 444, 444, 436, 431, 430, 424, 431, 443, 436, 431, 434, 426, 416, 399, 412, 437, 433, 431, 432, 422, 414, 404, 420, 445, 434, 434, 429, 424, 435, 417, 431},
        {1031, 1042, 1057, 1051, 1052, 1008, 980, 1015, 1021, 1057, 1077, 1089, 1099, 1069, 1046, 1033, 1017, 1035, 1073, 1086, 1094, 1074, 1028, 1038, 1028, 1057, 1069, 1095, 1107, 1093, 1085, 1058, 1039, 1054, 1073, 1092, 1088, 1081, 1062, 1046, 1009, 1042, 1062, 1063, 1061, 1058, 1028, 1010, 1011, 1015, 1023, 1024, 1034, 996, 958, 925, 1003, 1014, 1020, 1048, 1016, 994, 957, 951},
        {-76, -34, -20, 5, -9, -5, -4, -95, -15, 13, 25, 12, 29, 35, 28, 3, -5, 18, 38, 49, 52, 45, 40, 6, -15, 19, 42, 59, 60, 55, 41, 13, -23, 7, 36, 54, 56, 41, 24, 7, -30, -4, 16, 32, 31, 24, 4, -8, -35, -16, -5, 2, 8, 2, -11, -32, -70, -57, -37, -27, -38, -20, -48, -79}
    };
    public static int[,] passedPawnBonuses = {
        {0, 0, 0, 0, 0, 0, 0, 0, 55, 74, 62, 72, 61, 52, 22, -3, 21, 26, 19, 8, 7, -5, -38, -59, 10, 5, 19, 16, -1, 8, -27, -23, -6, -15, -23, -10, -20, -11, -23, -16, -10, -22, -25, -20, -23, -16, -24, -1, -15, -13, -13, -16, -8, -9, 2, -9, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 155, 130, 146, 122, 122, 125, 141, 154, 147, 148, 115, 80, 84, 100, 118, 142, 90, 86, 53, 48, 45, 53, 84, 87, 62, 54, 35, 25, 28, 37, 62, 62, 21, 31, 18, 11, 10, 10, 42, 23, 20, 22, 12, -3, -10, 4, 19, 20, 0, 0, 0, 0, 0, 0, 0, 0}
    };
    public static int[] isolatedPawnPenalty = {31, 11, -7, -23, -42, -67, -83, -88, 49};
    public static EvalPair doubledPawnPenalty = new EvalPair(0, -32);
    public static EvalPair bishopPairBonus = new EvalPair(39, 52);
    public static EvalPair bishopMobility = new EvalPair(9, 14);
    public static EvalPair rookOpenFile = new EvalPair(8, -13);
    public static EvalPair rookMobility = new EvalPair(3, 13);
    public static EvalPair kingOpenFile = new EvalPair(-42, 5);
    public static EvalPair kingPawnShield = new EvalPair(18, -9);
    


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
            score += EvaluateRookMobility(board, index);
        }

        while (blackRooks != 0)
        {
            int index = BitboardHelper.PopLSB(ref blackRooks);
            if((BitboardHelper.files[index % 8] & (board.allPiecesBitboard ^ 1ul << index)) == 0){ score -= rookOpenFile; }
            score -= EvaluateRookMobility(board, index);
        }

        while (whiteBishops != 0)
        {
            int index = BitboardHelper.PopLSB(ref whiteBishops);
            score += EvaluateBishopMobility(board, index);
        }
        while (blackBishops != 0)
        {
            int index = BitboardHelper.PopLSB(ref blackBishops);
            score -= EvaluateBishopMobility(board, index);
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

    EvalPair EvaluateBishopMobility(Board board, int pieceIndex)
    {
        int numMoves = 0;
        ulong simpleBishopMoves = BitboardHelper.GetBishopAttacks(pieceIndex, board.allPiecesBitboard);
        while (simpleBishopMoves != 0) { numMoves++; BitboardHelper.PopLSB(ref simpleBishopMoves); }
        return new EvalPair(numMoves * bishopMobility.mg, numMoves * bishopMobility.eg);
    }

    EvalPair EvaluateRookMobility(Board board, int pieceIndex)
    {
        int numMoves = 0;
        ulong simpleRookMoves = BitboardHelper.GetRookAttacks(pieceIndex, board.allPiecesBitboard);
        while (simpleRookMoves != 0) { numMoves++; BitboardHelper.PopLSB(ref simpleRookMoves); }
        return new EvalPair(numMoves * rookMobility.mg, numMoves * rookMobility.eg);
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