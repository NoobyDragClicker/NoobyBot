using System;
using System.Drawing;
using System.Linq.Expressions;
using System.Net.Security;

public class Evaluation
{

    int colorTurn;

    SearchLogger logger;

    //Unused in actual eval
    public static int pawnValue = 90;
    public static int knightValue = 336;
    public static int bishopValue = 366;
    public static int rookValue = 538;
    public static int queenValue = 1024;
    
    public static int[,] mg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 62, 78, 67, 80, 66, 56, 23, -1, 65, 75, 102, 101, 111, 151, 130, 94, 48, 65, 63, 70, 91, 83, 79, 69, 35, 54, 55, 70, 72, 71, 62, 49, 31, 41, 45, 48, 61, 59, 72, 53, 39, 54, 44, 49, 54, 83, 82, 52, 0, 0, 0, 0, 0, 0, 0, 0},
        {144, 151, 194, 230, 243, 203, 168, 179, 222, 252, 269, 288, 272, 322, 246, 266, 232, 279, 309, 313, 342, 341, 306, 272, 238, 258, 283, 314, 295, 316, 271, 277, 224, 240, 267, 271, 282, 272, 266, 241, 206, 233, 254, 258, 273, 263, 259, 227, 191, 205, 223, 246, 245, 247, 226, 227, 145, 205, 189, 213, 218, 229, 213, 172},
        {201, 161, 152, 120, 150, 154, 154, 172, 192, 199, 183, 180, 204, 189, 199, 183, 195, 208, 204, 215, 200, 243, 216, 224, 190, 190, 197, 215, 208, 200, 193, 195, 186, 180, 180, 204, 199, 181, 183, 201, 192, 197, 192, 184, 190, 195, 200, 211, 200, 203, 203, 183, 191, 204, 226, 203, 192, 208, 193, 180, 189, 190, 201, 209},
        {308, 285, 284, 275, 297, 291, 295, 336, 302, 301, 312, 327, 316, 337, 333, 355, 287, 308, 304, 301, 321, 330, 359, 338, 286, 297, 294, 297, 305, 310, 310, 317, 277, 276, 276, 285, 295, 283, 299, 295, 278, 274, 281, 282, 296, 299, 323, 310, 280, 282, 291, 292, 299, 311, 320, 296, 304, 300, 303, 312, 319, 318, 318, 307},
        {475, 490, 519, 546, 549, 579, 581, 522, 509, 489, 503, 501, 515, 536, 525, 554, 510, 511, 518, 525, 532, 577, 573, 561, 493, 495, 508, 505, 512, 519, 519, 522, 486, 493, 490, 501, 504, 502, 509, 510, 485, 492, 493, 495, 499, 502, 514, 510, 481, 494, 503, 507, 502, 515, 522, 527, 470, 471, 481, 495, 488, 475, 501, 493},
        {-77, -84, -57, -107, -68, 6, 49, 113, -101, -27, -84, -7, -36, -20, 54, 40, -99, 19, -68, -78, -54, 28, 20, -17, -58, -63, -74, -147, -127, -89, -73, -102, -59, -58, -100, -136, -135, -96, -91, -114, -25, -14, -59, -77, -69, -75, -31, -49, 54, 20, -2, -31, -38, -14, 20, 32, 34, 69, 45, -39, 19, -31, 51, 45}
    };
    public static int[,] eg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 155, 129, 146, 122, 122, 124, 141, 154, 123, 127, 112, 114, 104, 102, 126, 120, 114, 106, 105, 92, 94, 94, 96, 94, 101, 100, 98, 99, 97, 92, 88, 81, 99, 93, 101, 105, 107, 101, 82, 81, 106, 101, 109, 114, 125, 105, 88, 86, 0, 0, 0, 0, 0, 0, 0, 0},
        {233, 278, 301, 283, 299, 273, 277, 211, 281, 292, 307, 308, 297, 286, 288, 261, 296, 306, 319, 320, 307, 308, 296, 281, 296, 320, 337, 336, 336, 332, 320, 293, 306, 319, 336, 336, 339, 328, 313, 289, 288, 305, 316, 332, 327, 307, 298, 289, 285, 299, 307, 305, 305, 300, 286, 286, 262, 278, 292, 295, 292, 279, 287, 276},
        {232, 240, 237, 247, 242, 229, 235, 226, 223, 213, 217, 216, 205, 210, 213, 225, 239, 218, 202, 190, 195, 197, 216, 234, 235, 221, 204, 195, 187, 202, 216, 233, 230, 223, 207, 194, 191, 201, 214, 222, 234, 219, 212, 210, 214, 208, 214, 225, 245, 216, 207, 218, 218, 209, 216, 222, 228, 238, 239, 230, 227, 243, 223, 205},
        {447, 452, 460, 455, 447, 453, 454, 439, 454, 461, 465, 453, 454, 445, 447, 435, 456, 452, 452, 446, 440, 435, 435, 435, 457, 448, 454, 446, 433, 436, 439, 440, 451, 448, 447, 441, 436, 437, 431, 436, 445, 438, 434, 437, 428, 420, 405, 415, 438, 435, 434, 435, 425, 417, 407, 422, 451, 437, 440, 434, 427, 440, 421, 437},
        {1035, 1043, 1057, 1051, 1052, 1008, 981, 1019, 1021, 1057, 1079, 1091, 1101, 1072, 1046, 1035, 1017, 1036, 1074, 1089, 1095, 1074, 1028, 1038, 1029, 1057, 1069, 1097, 1108, 1093, 1085, 1059, 1041, 1055, 1073, 1093, 1088, 1080, 1063, 1047, 1011, 1043, 1063, 1063, 1061, 1059, 1029, 1011, 1014, 1015, 1022, 1026, 1036, 998, 960, 931, 1003, 1017, 1027, 1051, 1023, 997, 962, 953},
        {-77, -35, -21, 5, -9, -5, -4, -95, -17, 12, 24, 12, 29, 35, 28, 3, -6, 17, 37, 48, 51, 45, 39, 6, -16, 18, 41, 59, 60, 55, 40, 14, -23, 6, 35, 54, 55, 40, 24, 8, -30, -5, 16, 31, 30, 23, 4, -7, -36, -17, -6, 1, 8, 2, -11, -32, -70, -58, -36, -25, -36, -18, -46, -78}
    };
    public static int[,] passedPawnBonuses = {
        {0, 0, 0, 0, 0, 0, 0, 0, 62, 78, 67, 80, 66, 56, 23, -1, 23, 31, 20, 12, 8, -9, -41, -63, 14, 11, 24, 17, 0, 8, -25, -24, -1, -10, -19, -8, -18, -12, -21, -14, -5, -14, -20, -12, -15, -12, -18, 3, -11, -9, -9, -12, 2, -8, 5, -6, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 155, 129, 146, 122, 122, 124, 141, 154, 146, 146, 114, 79, 82, 99, 116, 141, 89, 85, 53, 47, 44, 53, 83, 86, 60, 52, 35, 26, 29, 39, 61, 62, 21, 31, 19, 10, 10, 11, 42, 23, 19, 20, 12, -3, -11, 5, 18, 19, 0, 0, 0, 0, 0, 0, 0, 0}
    };
    public static int[] isolatedPawnPenalty = {25, 9, -5, -18, -33, -56, -71, -75, 49};
    public static EvalPair doubledPawnPenalty = new EvalPair(0, -30);
    public static EvalPair protectedPawn = new EvalPair(14, 5);
    public static EvalPair bishopPairBonus = new EvalPair(40, 52);
    public static EvalPair bishopMobility = new EvalPair(9, 14);
    public static EvalPair rookOpenFile = new EvalPair(39, -6);
    public static EvalPair rookSemiOpenFile = new EvalPair(11, 12);
    public static EvalPair rookMobility = new EvalPair(0, 12);
    public static EvalPair rookKingRingAttack = new EvalPair(12, -4);
    public static EvalPair kingOpenFile = new EvalPair(-39, 5);
    public static EvalPair kingPawnShield = new EvalPair(19, -9);
    


    int playerTurnMultiplier;
    public Evaluation(SearchLogger logger)
    {
        this.logger = logger;
    }
    public int EvaluatePosition(Board board)
    {
        colorTurn = board.colorTurn;
        playerTurnMultiplier = (colorTurn == Piece.White) ? 1 : -1;
        int boardVal = IncrementalCount(board);
        return boardVal;
    }

    int IncrementalCount(Board board)
    {
        const int totalPhase = 24;
        int mgScore = board.gameStateHistory[board.fullMoveClock].mgPSQTVal;
        int egScore = board.gameStateHistory[board.fullMoveClock].egPSQTVal;
        EvalPair score = new EvalPair(mgScore, egScore);

        Bitboard whiteBishops = board.GetPieces(Board.WhiteIndex, Piece.Bishop);
        Bitboard blackBishops = board.GetPieces(Board.BlackIndex, Piece.Bishop);

        Bitboard whiteRooks = board.GetPieces(Board.WhiteIndex, Piece.Rook);
        Bitboard blackRooks = board.GetPieces(Board.BlackIndex, Piece.Rook);

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

        score += EvaluatePawnStructure(board, Board.WhiteIndex);
        score -= EvaluatePawnStructure(board, Board.BlackIndex);


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

    EvalPair EvaluatePawnStructure(Board board, int currentColorIndex)
    {
        EvalPair score = new EvalPair();
        int oppositeColorIndex = 1 - currentColorIndex;
        int currentColor = currentColorIndex == Board.WhiteIndex ? Piece.White : Piece.Black;
        Bitboard ourPawns = board.GetPieces(currentColorIndex, Piece.Pawn);
        Bitboard ourPawnsTemp = ourPawns;
        Bitboard theirPawns = board.GetPieces(oppositeColorIndex, Piece.Pawn);
        int isolatedPawnCount = 0;

        while (!ourPawnsTemp.Empty())
        {
            int index = ourPawnsTemp.PopLSB();
            bool passer = (theirPawns & BitboardHelper.pawnPassedMask[currentColorIndex, index]).Empty();
            int pushSquare = index + (currentColorIndex == Board.WhiteIndex ? -8 : 8);

            //Passed pawn
            if (passer) { 
                int psqtIndex = currentColorIndex == Board.WhiteIndex ? index : index ^ 56;
                score.mg += passedPawnBonuses[0, psqtIndex]; 
                score.eg += passedPawnBonuses[1, psqtIndex]; 
            }

            if (board.PieceAt(pushSquare) == Piece.Pawn && board.ColorAt(pushSquare) == currentColor) { score.eg += doubledPawnPenalty.eg; }
            if ((BitboardHelper.isolatedPawnMask[index] & ourPawns).Empty()) { isolatedPawnCount++; }
        }
        score.eg += isolatedPawnPenalty[isolatedPawnCount];
        
        int defended = (BitboardHelper.GetAllPawnAttacks(ourPawns, currentColor) & ourPawns).PopCount();
        score.mg += defended * protectedPawn.mg;
        score.eg += defended * protectedPawn.eg;
        return score;
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