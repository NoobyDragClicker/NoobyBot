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
       {0, 0, 0, 0, 0, 0, 0, 0, 62, 77, 67, 79, 66, 56, 22, -1, 67, 79, 103, 103, 111, 152, 132, 93, 52, 72, 70, 76, 96, 88, 85, 70, 40, 63, 62, 79, 80, 77, 69, 54, 41, 57, 57, 61, 75, 71, 87, 64, 41, 61, 47, 53, 59, 86, 88, 53, 0, 0, 0, 0, 0, 0, 0, 0},
        {144, 151, 194, 229, 244, 203, 167, 179, 222, 252, 269, 286, 271, 322, 245, 267, 232, 277, 308, 312, 341, 340, 305, 271, 239, 257, 282, 311, 292, 315, 270, 277, 224, 240, 265, 270, 281, 271, 265, 240, 206, 233, 253, 257, 273, 263, 258, 227, 191, 205, 222, 246, 244, 247, 226, 228, 145, 205, 188, 212, 216, 228, 213, 171},
        {198, 159, 150, 118, 148, 153, 151, 171, 189, 194, 180, 176, 201, 185, 194, 181, 192, 205, 199, 210, 195, 237, 214, 220, 188, 186, 193, 210, 204, 197, 188, 192, 182, 177, 176, 199, 195, 176, 179, 198, 191, 193, 189, 180, 186, 191, 196, 210, 200, 200, 198, 180, 187, 202, 222, 202, 188, 205, 190, 176, 185, 187, 199, 206},
        {307, 285, 284, 274, 296, 291, 295, 337, 302, 301, 312, 327, 315, 338, 332, 356, 286, 307, 304, 300, 320, 330, 358, 337, 285, 295, 292, 295, 304, 309, 308, 315, 276, 275, 275, 284, 294, 282, 297, 294, 277, 273, 280, 281, 295, 298, 322, 308, 278, 281, 290, 292, 298, 310, 317, 295, 303, 299, 302, 311, 318, 317, 316, 306},
        {474, 489, 518, 544, 547, 577, 580, 522, 509, 487, 503, 500, 515, 535, 523, 553, 509, 510, 516, 524, 530, 574, 573, 559, 493, 493, 507, 504, 510, 519, 517, 521, 485, 492, 488, 500, 504, 500, 509, 509, 484, 491, 492, 493, 498, 501, 513, 509, 480, 493, 501, 507, 501, 515, 521, 526, 469, 469, 480, 494, 487, 474, 500, 493},
        {-78, -86, -58, -107, -68, 6, 49, 113, -101, -28, -84, -8, -37, -20, 53, 39, -99, 19, -69, -79, -56, 27, 20, -18, -57, -64, -75, -148, -128, -90, -74, -103, -58, -57, -100, -135, -135, -97, -91, -114, -24, -13, -58, -77, -68, -75, -30, -48, 53, 20, -1, -31, -37, -15, 20, 30, 36, 69, 44, -39, 19, -30, 51, 46}
    };
    public static int[,] eg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 156, 131, 147, 123, 123, 125, 142, 155, 125, 126, 114, 116, 106, 103, 126, 121, 117, 107, 108, 95, 96, 96, 97, 96, 103, 100, 101, 103, 100, 96, 88, 84, 101, 95, 104, 107, 109, 104, 84, 83, 107, 101, 111, 116, 126, 108, 89, 87, 0, 0, 0, 0, 0, 0, 0, 0},
        {233, 278, 301, 284, 299, 273, 277, 211, 281, 292, 307, 308, 298, 286, 289, 261, 296, 306, 320, 320, 308, 308, 297, 281, 296, 321, 337, 336, 336, 332, 320, 293, 307, 319, 337, 336, 339, 328, 313, 289, 288, 306, 315, 333, 327, 307, 298, 289, 286, 299, 308, 305, 306, 300, 286, 286, 262, 278, 293, 296, 292, 280, 288, 277},
        {231, 240, 237, 247, 241, 229, 235, 225, 222, 213, 216, 215, 204, 210, 213, 224, 239, 217, 201, 190, 194, 196, 215, 233, 234, 220, 203, 194, 186, 201, 215, 232, 230, 222, 206, 193, 189, 200, 214, 221, 234, 219, 211, 209, 212, 208, 214, 225, 244, 216, 207, 217, 218, 208, 216, 221, 229, 237, 239, 230, 227, 243, 222, 204},
        {447, 452, 460, 455, 447, 453, 454, 439, 454, 461, 465, 453, 454, 445, 448, 435, 455, 452, 451, 446, 440, 435, 435, 434, 457, 448, 454, 446, 433, 436, 439, 440, 451, 447, 447, 440, 435, 436, 431, 436, 444, 438, 434, 437, 428, 420, 404, 415, 438, 435, 435, 435, 425, 417, 408, 422, 451, 437, 440, 433, 427, 439, 421, 438},
        {1035, 1042, 1057, 1050, 1051, 1007, 980, 1018, 1021, 1057, 1079, 1090, 1100, 1071, 1047, 1034, 1016, 1035, 1073, 1088, 1094, 1073, 1027, 1037, 1027, 1056, 1068, 1095, 1107, 1091, 1084, 1058, 1039, 1053, 1072, 1091, 1088, 1080, 1061, 1047, 1010, 1042, 1062, 1063, 1060, 1058, 1028, 1011, 1013, 1015, 1022, 1024, 1035, 997, 959, 930, 1004, 1018, 1026, 1050, 1023, 997, 961, 953},
        {-76, -35, -20, 5, -8, -5, -3, -94, -16, 13, 24, 12, 29, 35, 28, 3, -5, 17, 37, 48, 51, 45, 39, 6, -16, 18, 41, 59, 60, 55, 40, 14, -23, 6, 35, 54, 55, 40, 24, 8, -30, -5, 15, 31, 30, 23, 4, -7, -35, -17, -6, 1, 8, 2, -11, -31, -70, -57, -36, -25, -36, -19, -46, -78}
    };
    public static int[,] passedPawnBonuses = {
        {0, 0, 0, 0, 0, 0, 0, 0, 62, 77, 67, 79, 66, 56, 22, -1, 22, 30, 21, 12, 8, -7, -39, -61, 10, 5, 21, 17, -0, 5, -29, -25, -5, -14, -22, -10, -21, -14, -24, -17, -10, -21, -24, -18, -20, -18, -23, -1, -14, -13, -13, -15, -2, -10, 3, -8, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 156, 131, 147, 123, 123, 125, 142, 155, 148, 148, 115, 80, 83, 100, 118, 142, 90, 87, 53, 47, 45, 54, 84, 88, 62, 54, 35, 25, 28, 38, 62, 63, 21, 32, 18, 10, 10, 11, 42, 23, 21, 22, 13, -3, -11, 5, 19, 21, 0, 0, 0, 0, 0, 0, 0, 0}
    };
    public static int[] isolatedPawnPenalty = {31, 11, -7, -23, -42, -68, -85, -90, 49};
    public static EvalPair doubledPawnPenalty = new EvalPair(0, -34);
    public static EvalPair bishopPairBonus = new EvalPair(39, 52);
    public static EvalPair bishopMobility = new EvalPair(9, 14);
    public static EvalPair rookOpenFile = new EvalPair(40, -6);
    public static EvalPair rookSemiOpenFile = new EvalPair(11, 13);
    public static EvalPair rookMobility = new EvalPair(0, 12);
    public static EvalPair rookKingRingAttack = new EvalPair(12, -4);
    public static EvalPair kingOpenFile = new EvalPair(-41, 6);
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
        if((BitboardHelper.files[pieceIndex % 8] & board.GetPieces(colorIndex, Piece.Pawn)).Empty()){ 
            //None of our their pawns
            if((BitboardHelper.files[pieceIndex % 8] & board.GetPieces(1 - colorIndex, Piece.Pawn)).Empty())
            {
                score += rookOpenFile; 
            }
            else
            {
                score += rookSemiOpenFile; 
            }
        }

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