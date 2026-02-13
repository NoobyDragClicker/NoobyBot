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
        {0, 0, 0, 0, 0, 0, 0, 0, 63, 78, 68, 80, 67, 56, 22, -1, 69, 79, 103, 104, 111, 154, 133, 94, 56, 72, 70, 77, 97, 91, 86, 72, 43, 64, 62, 80, 81, 80, 70, 57, 45, 58, 57, 62, 77, 74, 88, 66, 46, 62, 48, 55, 61, 90, 89, 56, 0, 0, 0, 0, 0, 0, 0, 0},
        {144, 150, 194, 229, 243, 202, 167, 179, 221, 252, 269, 287, 271, 322, 244, 267, 231, 277, 308, 312, 341, 339, 304, 269, 238, 257, 282, 311, 291, 314, 269, 276, 224, 240, 264, 270, 280, 271, 264, 240, 206, 232, 252, 257, 273, 262, 258, 227, 191, 204, 222, 245, 244, 247, 225, 227, 145, 206, 189, 213, 218, 228, 214, 170},
        {198, 158, 150, 117, 148, 153, 150, 171, 186, 194, 180, 177, 201, 185, 194, 181, 191, 206, 199, 211, 195, 237, 214, 219, 188, 186, 194, 210, 204, 197, 188, 191, 182, 178, 175, 200, 195, 176, 179, 197, 191, 192, 189, 180, 186, 191, 196, 209, 200, 200, 198, 180, 187, 202, 222, 202, 188, 205, 192, 177, 186, 188, 200, 205},
        {314, 290, 290, 281, 301, 292, 297, 342, 309, 308, 320, 335, 323, 344, 337, 361, 289, 309, 309, 308, 328, 327, 357, 337, 284, 292, 293, 299, 305, 303, 302, 312, 271, 271, 274, 285, 293, 275, 290, 288, 274, 269, 278, 280, 293, 292, 315, 304, 275, 276, 289, 291, 296, 305, 311, 291, 300, 295, 301, 310, 316, 313, 310, 302},
        {473, 488, 518, 544, 547, 576, 579, 521, 505, 486, 504, 501, 515, 535, 522, 553, 507, 509, 516, 525, 531, 573, 572, 557, 492, 492, 507, 503, 510, 518, 515, 520, 484, 492, 487, 499, 504, 499, 508, 508, 483, 490, 491, 492, 498, 500, 512, 508, 479, 492, 501, 506, 501, 515, 521, 527, 468, 470, 482, 496, 489, 475, 502, 493},
        {-78, -86, -57, -108, -69, 5, 49, 113, -101, -28, -84, -8, -37, -20, 53, 39, -99, 18, -69, -79, -56, 26, 19, -18, -57, -65, -75, -148, -128, -90, -75, -103, -58, -58, -101, -135, -135, -96, -91, -114, -25, -13, -59, -77, -68, -75, -30, -48, 53, 19, -2, -32, -38, -14, 20, 29, 35, 68, 44, -38, 21, -29, 51, 45}
    };
    public static int[,] eg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 155, 130, 146, 122, 122, 125, 141, 155, 124, 126, 114, 115, 106, 102, 126, 121, 115, 107, 108, 94, 96, 95, 96, 96, 101, 100, 101, 103, 99, 94, 88, 83, 100, 94, 104, 107, 109, 103, 83, 82, 106, 100, 110, 116, 126, 106, 88, 86, 0, 0, 0, 0, 0, 0, 0, 0},
        {233, 278, 301, 284, 299, 274, 277, 211, 281, 292, 307, 308, 298, 286, 289, 261, 296, 307, 320, 320, 308, 308, 297, 281, 296, 321, 337, 336, 337, 332, 320, 293, 307, 319, 337, 336, 339, 328, 314, 290, 288, 306, 316, 333, 327, 307, 299, 289, 286, 299, 308, 305, 306, 300, 287, 287, 262, 277, 292, 295, 292, 280, 287, 277},
        {232, 240, 236, 246, 241, 229, 235, 225, 223, 213, 216, 215, 203, 210, 212, 224, 239, 217, 201, 190, 194, 196, 215, 234, 235, 220, 202, 194, 186, 201, 215, 232, 230, 222, 207, 193, 189, 201, 213, 222, 234, 219, 210, 209, 212, 208, 214, 225, 244, 215, 207, 217, 217, 208, 216, 220, 229, 237, 238, 230, 226, 242, 222, 204},
        {447, 452, 459, 453, 447, 455, 456, 440, 453, 460, 463, 450, 452, 445, 448, 436, 456, 452, 451, 444, 438, 438, 437, 436, 459, 450, 454, 445, 434, 440, 443, 443, 454, 450, 449, 441, 437, 441, 435, 440, 447, 441, 436, 438, 430, 424, 409, 419, 441, 439, 436, 436, 427, 422, 412, 425, 454, 441, 442, 435, 430, 442, 426, 441},
        {1035, 1043, 1057, 1050, 1051, 1008, 981, 1018, 1022, 1057, 1078, 1089, 1099, 1070, 1047, 1034, 1016, 1035, 1073, 1086, 1093, 1074, 1027, 1038, 1027, 1056, 1068, 1095, 1106, 1091, 1085, 1058, 1039, 1053, 1072, 1092, 1087, 1081, 1061, 1047, 1010, 1042, 1062, 1063, 1060, 1059, 1029, 1011, 1013, 1015, 1022, 1024, 1035, 997, 959, 929, 1005, 1017, 1023, 1048, 1020, 996, 960, 953},
        {-76, -34, -20, 5, -8, -4, -3, -94, -16, 12, 24, 12, 29, 35, 28, 4, -5, 17, 37, 48, 51, 45, 39, 7, -16, 18, 41, 59, 60, 55, 40, 14, -23, 6, 35, 54, 55, 40, 24, 8, -30, -5, 16, 31, 30, 23, 4, -7, -35, -17, -6, 1, 8, 2, -11, -30, -70, -57, -36, -26, -37, -19, -47, -77}
    };
    public static int[,] passedPawnBonuses = {
        {0, 0, 0, 0, 0, 0, 0, 0, 63, 78, 68, 80, 67, 56, 22, -1, 20, 31, 22, 12, 9, -9, -38, -62, 8, 6, 21, 16, -0, 4, -29, -25, -6, -14, -21, -9, -20, -16, -25, -17, -11, -21, -24, -18, -20, -19, -24, -1, -15, -12, -10, -15, -4, -11, 2, -8, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 155, 130, 146, 122, 122, 125, 141, 155, 148, 148, 114, 80, 82, 100, 117, 143, 90, 86, 52, 47, 45, 54, 84, 88, 63, 54, 35, 24, 28, 39, 62, 63, 22, 32, 18, 10, 10, 12, 42, 24, 21, 22, 12, -2, -10, 5, 19, 21, 0, 0, 0, 0, 0, 0, 0, 0}
    };
    public static int[] isolatedPawnPenalty = {31, 11, -7, -23, -42, -67, -85, -89, 49};
    public static EvalPair doubledPawnPenalty = new EvalPair(0, -35);
    public static EvalPair bishopPairBonus = new EvalPair(39, 53);
    public static EvalPair bishopMobility = new EvalPair(9, 14);
    public static EvalPair rookOpenFile = new EvalPair(20, 5);
    public static EvalPair rookMobility = new EvalPair(1, 12);
    public static EvalPair rookKingRingAttack = new EvalPair(12, -4);
    public static EvalPair kingOpenFile = new EvalPair(-40, 5);
    public static EvalPair kingPawnShield = new EvalPair(17, -8);
    


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


        Bitboard whitePawns = board.GetPieces(Board.WhiteIndex, Piece.Pawn);
        Bitboard blackPawns = board.GetPieces(Board.BlackIndex, Piece.Pawn);

        Bitboard whiteBishops = board.GetPieces(Board.WhiteIndex, Piece.Bishop);
        Bitboard blackBishops = board.GetPieces(Board.BlackIndex, Piece.Bishop);

        Bitboard whiteRooks = board.GetPieces(Board.WhiteIndex, Piece.Rook);
        Bitboard blackRooks = board.GetPieces(Board.BlackIndex, Piece.Rook);

        while (!whitePawns.Empty())
        {
            int index = whitePawns.PopLSB();
            score += EvaluatePawnStrength(board, index, Board.WhiteIndex);
        }

        while (!blackPawns.Empty())
        {
            int index = blackPawns.PopLSB();
            score -= EvaluatePawnStrength(board, index, Board.BlackIndex);

        }

        while (!whiteRooks.Empty())
        {
            int index = whiteRooks.PopLSB();
            score += EvaluateRookMobility(board, index, Board.WhiteIndex);
        }

        while (!blackRooks.Empty())
        {
            int index = blackRooks.PopLSB();
            score -= EvaluateRookMobility(board, index, Board.BlackIndex);
        }

        while (!whiteBishops.Empty())
        {
            int index = whiteBishops.PopLSB();
            score += EvaluateBishopMobility(board, index);
        }
        while (!blackBishops.Empty())
        {
            int index = blackBishops.PopLSB();
            score -= EvaluateBishopMobility(board, index);
        }


        int whiteKingIndex = board.GetPieces(Board.WhiteIndex, Piece.King).GetLSB();
        int blackKingIndex = board.GetPieces(Board.BlackIndex, Piece.King).GetLSB();
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
        if(((board.sideBitboard[currentColorIndex] ^ 1ul << kingIndex) & BitboardHelper.files[kingIndex % 8]).Empty())
        {
            score += kingOpenFile;
        }
        
        int direction = kingColor == Piece.White ? -1 : 1;
        int frontSquare = kingIndex + (direction * 8);

        if(frontSquare >= 0 && frontSquare <= 63)
        {
            if(board.GetPieces(currentColorIndex, Piece.Pawn).ContainsSquare(frontSquare))
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

        bool passer = (board.GetPieces(oppositeColorIndex, Piece.Pawn) & BitboardHelper.pawnPassedMask[currentColorIndex, pawnIndex]).Empty();
        int pushSquare = pawnIndex + (currentColorIndex == Board.WhiteIndex ? -8 : 8);
        //Passed pawn
        if (passer) { 
            int psqtIndex = currentColorIndex == Board.WhiteIndex ? pawnIndex : pawnIndex ^ 56;
            mgBonus += passedPawnBonuses[0, psqtIndex]; 
            egBonus += passedPawnBonuses[1, psqtIndex]; 
        }

        //Doubled pawn penalty
        if (board.PieceAt(pushSquare) == Piece.Pawn && board.ColorAt(pushSquare) == currentColor) { egBonus += doubledPawnPenalty.eg; }
        if ((BitboardHelper.isolatedPawnMask[pawnIndex] & board.GetPieces(currentColorIndex, Piece.Pawn)).Empty()) { isolatedPawnCount[currentColorIndex]++; }

        return new EvalPair(mgBonus, egBonus);
    }

    EvalPair EvaluateBishopMobility(Board board, int pieceIndex)
    {
        
        Bitboard simpleBishopMoves = BitboardHelper.GetBishopAttacks(pieceIndex, board.allPiecesBitboard);
        int numMoves = simpleBishopMoves.PopCount();
        return new EvalPair(numMoves * bishopMobility.mg, numMoves * bishopMobility.eg);
    }

    EvalPair EvaluateRookMobility(Board board, int pieceIndex, int colorIndex)
    {
        EvalPair score = new EvalPair();
        if((BitboardHelper.files[pieceIndex % 8] & board.GetPieces(colorIndex, Piece.Pawn)).Empty()){ score += rookOpenFile; }

        Bitboard simpleRookMoves = BitboardHelper.GetRookAttacks(pieceIndex, board.allPiecesBitboard);
        Bitboard rookAttacks = simpleRookMoves & BitboardHelper.kingRing[1 - colorIndex, board.GetPieces(1 - colorIndex, Piece.King).GetLSB()];
        int numMoves = simpleRookMoves.PopCount();
        int numAttacks = rookAttacks.PopCount();

        score.mg += numMoves * rookMobility.mg + numAttacks * rookKingRingAttack.mg;
        score.eg += numMoves * rookMobility.eg + numAttacks * rookKingRingAttack.eg;
        return score;
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