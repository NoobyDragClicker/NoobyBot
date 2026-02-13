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
         {0, 0, 0, 0, 0, 0, 0, 0, 56, 74, 63, 73, 62, 52, 21, -5, 59, 78, 100, 100, 105, 145, 131, 86, 46, 70, 68, 73, 93, 83, 84, 65, 35, 62, 61, 78, 78, 73, 67, 51, 37, 56, 57, 61, 75, 68, 86, 61, 39, 61, 48, 53, 59, 84, 86, 52, 0, 0, 0, 0, 0, 0, 0, 0},
        {144, 151, 192, 227, 241, 201, 166, 181, 223, 252, 269, 287, 271, 323, 244, 267, 233, 278, 308, 311, 341, 339, 305, 269, 238, 256, 282, 311, 291, 314, 269, 276, 224, 240, 264, 270, 280, 270, 264, 239, 206, 232, 252, 257, 273, 262, 257, 226, 192, 204, 222, 245, 244, 248, 224, 226, 142, 209, 191, 216, 219, 227, 215, 169},
        {198, 157, 147, 115, 146, 151, 149, 171, 188, 193, 180, 176, 200, 185, 194, 180, 192, 205, 198, 211, 194, 237, 214, 220, 188, 185, 194, 209, 203, 196, 187, 191, 182, 177, 175, 198, 194, 176, 178, 197, 192, 192, 188, 180, 185, 191, 195, 209, 201, 199, 197, 179, 187, 202, 221, 202, 186, 206, 193, 179, 188, 189, 201, 203},
        {318, 291, 289, 278, 299, 290, 294, 344, 313, 309, 322, 337, 325, 345, 337, 365, 294, 312, 310, 310, 328, 329, 357, 339, 287, 296, 297, 301, 309, 306, 303, 312, 273, 274, 277, 288, 296, 278, 288, 285, 272, 271, 281, 282, 294, 291, 311, 298, 272, 278, 294, 294, 299, 305, 313, 289, 295, 295, 306, 315, 319, 311, 312, 297},
        {475, 489, 517, 543, 545, 576, 578, 522, 510, 486, 504, 501, 516, 537, 523, 555, 509, 510, 515, 525, 530, 573, 572, 557, 492, 492, 506, 503, 510, 518, 515, 519, 485, 491, 487, 499, 503, 498, 506, 508, 484, 490, 491, 492, 497, 500, 511, 507, 481, 491, 500, 506, 500, 515, 520, 529, 468, 472, 485, 498, 492, 475, 503, 493},
        {-79, -86, -57, -108, -69, 5, 47, 110, -101, -27, -84, -8, -37, -19, 53, 38, -99, 20, -69, -77, -55, 29, 20, -18, -58, -64, -74, -147, -127, -89, -74, -103, -60, -58, -101, -134, -134, -97, -90, -114, -26, -16, -60, -77, -69, -75, -31, -49, 49, 17, -4, -32, -39, -15, 19, 29, 31, 67, 45, -35, 24, -27, 52, 44}
    };
    public static int[,] eg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 155, 130, 146, 122, 122, 125, 141, 155, 124, 125, 113, 114, 105, 104, 125, 121, 116, 106, 107, 93, 96, 96, 96, 96, 102, 99, 100, 101, 99, 96, 88, 83, 100, 94, 103, 105, 109, 103, 83, 83, 106, 99, 109, 116, 126, 107, 88, 86, 0, 0, 0, 0, 0, 0, 0, 0},
        {233, 278, 301, 284, 299, 274, 277, 211, 282, 293, 308, 308, 298, 287, 289, 261, 297, 307, 320, 320, 308, 308, 297, 282, 297, 321, 337, 336, 337, 333, 320, 294, 306, 319, 337, 336, 339, 328, 314, 289, 287, 305, 315, 333, 327, 307, 299, 289, 285, 299, 307, 305, 306, 299, 286, 286, 262, 279, 292, 295, 291, 279, 289, 276},
        {231, 240, 237, 247, 241, 229, 235, 225, 223, 213, 216, 215, 204, 210, 213, 224, 240, 218, 202, 190, 195, 197, 216, 234, 235, 221, 203, 194, 187, 201, 215, 233, 230, 222, 207, 193, 190, 201, 214, 222, 233, 219, 211, 209, 212, 208, 214, 225, 243, 216, 208, 218, 218, 208, 216, 220, 229, 237, 240, 230, 226, 243, 221, 205},
        {438, 445, 454, 450, 441, 447, 447, 430, 445, 454, 459, 447, 448, 436, 439, 426, 449, 446, 446, 440, 433, 429, 428, 428, 452, 444, 449, 441, 428, 431, 435, 436, 448, 444, 444, 436, 431, 433, 428, 434, 442, 435, 430, 434, 425, 417, 402, 414, 437, 433, 430, 431, 422, 415, 406, 421, 445, 434, 434, 429, 424, 434, 418, 433},
        {1033, 1042, 1057, 1051, 1052, 1008, 981, 1017, 1021, 1058, 1078, 1090, 1100, 1070, 1048, 1034, 1018, 1035, 1074, 1087, 1095, 1075, 1028, 1039, 1029, 1058, 1069, 1096, 1107, 1093, 1086, 1059, 1040, 1055, 1073, 1093, 1089, 1082, 1063, 1047, 1010, 1043, 1063, 1064, 1062, 1059, 1029, 1011, 1012, 1016, 1024, 1025, 1035, 997, 959, 926, 1003, 1016, 1022, 1049, 1018, 996, 959, 952},
        {-75, -34, -19, 6, -8, -4, -3, -94, -15, 13, 24, 12, 29, 35, 28, 4, -5, 17, 38, 48, 51, 45, 39, 7, -15, 18, 41, 59, 60, 55, 40, 14, -22, 6, 36, 54, 55, 40, 24, 8, -30, -5, 16, 31, 30, 23, 4, -7, -34, -17, -6, 1, 8, 2, -11, -31, -69, -57, -36, -26, -36, -19, -47, -78}
    };
    public static int[,] passedPawnBonuses = {
        {0, 0, 0, 0, 0, 0, 0, 0, 56, 74, 63, 73, 62, 52, 21, -5, 22, 27, 19, 8, 8, -7, -38, -61, 10, 5, 19, 16, -0, 5, -28, -25, -6, -15, -23, -10, -20, -13, -24, -18, -10, -21, -25, -20, -21, -17, -23, -1, -14, -13, -12, -15, -5, -10, 3, -9, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 155, 130, 146, 122, 122, 125, 141, 155, 147, 148, 115, 80, 83, 100, 117, 142, 89, 86, 53, 48, 44, 53, 84, 87, 62, 54, 35, 25, 28, 38, 62, 63, 21, 31, 18, 11, 10, 11, 42, 23, 20, 22, 12, -3, -11, 4, 19, 20, 0, 0, 0, 0, 0, 0, 0, 0}
    };
    public static int[] isolatedPawnPenalty = {31, 11, -7, -23, -42, -67, -84, -88, 49};
    public static EvalPair doubledPawnPenalty = new EvalPair(0, -32);
    public static EvalPair bishopPairBonus = new EvalPair(39, 52);
    public static EvalPair bishopMobility = new EvalPair(9, 14);
    public static EvalPair rookOpenFile = new EvalPair(7, -12);
    public static EvalPair rookMobility = new EvalPair(2, 13);
    public static EvalPair rookKingRingAttack = new EvalPair(14, -5);
    public static EvalPair kingOpenFile = new EvalPair(-42, 5);
    public static EvalPair kingPawnShield = new EvalPair(17, -9);
    


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
        if((BitboardHelper.files[pieceIndex % 8] & (board.allPiecesBitboard ^ 1ul << pieceIndex)) == 0){ score += rookOpenFile; }

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