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
        {0, 0, 0, 0, 0, 0, 0, 0, 56, 71, 60, 74, 61, 49, 16, -8, 66, 76, 103, 104, 114, 152, 131, 95, 49, 65, 64, 71, 91, 83, 80, 69, 35, 55, 55, 71, 73, 71, 62, 49, 32, 41, 46, 49, 61, 60, 72, 53, 39, 54, 45, 50, 55, 84, 83, 53, 0, 0, 0, 0, 0, 0, 0, 0},
        {144, 151, 194, 230, 243, 203, 166, 179, 221, 252, 269, 288, 271, 321, 245, 266, 231, 278, 309, 312, 342, 340, 306, 272, 238, 258, 283, 314, 294, 316, 271, 277, 224, 240, 267, 271, 282, 272, 266, 241, 206, 233, 253, 257, 273, 263, 259, 227, 191, 205, 223, 246, 245, 247, 226, 227, 145, 205, 189, 213, 218, 229, 213, 172},
        {201, 161, 151, 119, 149, 154, 154, 171, 191, 199, 182, 180, 204, 189, 198, 182, 194, 208, 204, 214, 199, 242, 216, 224, 190, 189, 196, 214, 208, 200, 192, 194, 186, 180, 180, 203, 199, 181, 183, 200, 192, 197, 192, 184, 190, 195, 200, 211, 200, 203, 202, 183, 191, 204, 226, 203, 192, 208, 193, 179, 188, 190, 201, 209},
        {307, 284, 283, 274, 296, 289, 291, 334, 302, 301, 312, 327, 315, 337, 331, 354, 286, 308, 304, 301, 321, 330, 359, 337, 286, 297, 294, 297, 305, 310, 310, 316, 277, 275, 276, 285, 295, 283, 299, 295, 278, 274, 280, 281, 296, 299, 323, 310, 279, 282, 291, 292, 299, 311, 319, 296, 304, 300, 303, 312, 319, 318, 317, 307},
        {475, 490, 519, 546, 549, 578, 581, 521, 509, 489, 503, 501, 515, 536, 525, 555, 509, 510, 517, 525, 532, 577, 574, 561, 492, 494, 508, 504, 512, 520, 519, 522, 486, 493, 489, 501, 504, 502, 510, 510, 485, 492, 493, 495, 499, 502, 514, 510, 481, 495, 503, 507, 502, 515, 522, 526, 470, 471, 481, 494, 488, 475, 500, 493},
        {-71, -77, -48, -99, -61, 5, 52, 116, -86, -17, -72, 5, -26, -15, 55, 40, -85, 30, -58, -69, -45, 33, 25, -13, -42, -53, -69, -141, -123, -85, -67, -95, -53, -54, -97, -135, -133, -92, -87, -112, -25, -14, -58, -76, -67, -73, -30, -49, 54, 19, -2, -31, -38, -14, 19, 31, 32, 67, 44, -40, 18, -32, 49, 43}
    };
    public static int[,] eg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 140, 114, 130, 106, 105, 108, 125, 140, 121, 124, 109, 106, 98, 100, 123, 118, 113, 105, 105, 91, 94, 94, 95, 95, 100, 99, 97, 98, 96, 92, 87, 81, 98, 92, 100, 104, 107, 100, 82, 81, 105, 101, 108, 112, 124, 105, 88, 85, 0, 0, 0, 0, 0, 0, 0, 0},
        {232, 278, 301, 283, 298, 274, 278, 212, 282, 293, 308, 308, 298, 287, 289, 263, 297, 307, 320, 320, 308, 309, 297, 281, 297, 321, 337, 336, 336, 332, 320, 294, 307, 319, 336, 336, 339, 329, 313, 289, 288, 305, 317, 333, 327, 308, 299, 289, 286, 299, 308, 305, 306, 300, 287, 286, 263, 278, 293, 296, 292, 280, 288, 276},
        {232, 242, 238, 248, 243, 230, 236, 227, 224, 214, 218, 216, 205, 211, 214, 227, 241, 219, 203, 192, 196, 198, 218, 235, 236, 222, 205, 196, 188, 203, 216, 235, 231, 223, 208, 195, 192, 202, 216, 223, 235, 220, 212, 211, 215, 209, 215, 226, 245, 217, 209, 219, 219, 210, 217, 223, 229, 238, 240, 232, 228, 244, 224, 205},
        {449, 454, 462, 456, 449, 456, 457, 442, 455, 463, 466, 454, 455, 446, 449, 437, 457, 454, 453, 448, 441, 437, 436, 437, 458, 450, 455, 447, 434, 437, 441, 442, 452, 449, 448, 442, 437, 438, 432, 437, 445, 440, 435, 438, 429, 421, 406, 417, 439, 437, 435, 436, 426, 418, 409, 423, 452, 438, 441, 434, 428, 440, 422, 438},
        {1036, 1044, 1058, 1051, 1052, 1010, 983, 1021, 1023, 1058, 1081, 1091, 1101, 1072, 1048, 1034, 1020, 1038, 1076, 1090, 1096, 1074, 1028, 1037, 1031, 1059, 1071, 1099, 1109, 1094, 1085, 1059, 1041, 1056, 1075, 1094, 1090, 1081, 1063, 1047, 1011, 1044, 1064, 1064, 1062, 1060, 1030, 1012, 1014, 1015, 1023, 1027, 1036, 999, 962, 933, 1003, 1017, 1028, 1052, 1024, 998, 963, 954},
        {-75, -35, -20, 7, -6, -1, -2, -93, -19, 9, 21, 9, 28, 35, 29, 6, -11, 9, 31, 43, 47, 42, 36, 5, -25, 8, 33, 52, 54, 50, 34, 10, -30, -1, 29, 49, 51, 35, 19, 7, -32, -8, 13, 29, 28, 22, 3, -5, -33, -16, -4, 3, 10, 5, -7, -27, -63, -53, -31, -20, -31, -12, -40, -71}
    };
    public static int[,] passedPawnBonuses = {
        {0, 0, 0, 0, 0, 0, 0, 0, 56, 71, 60, 74, 61, 49, 16, -8, 12, 30, 20, 11, 8, -6, -40, -66, 4, 11, 31, 24, 9, 18, -17, -21, -11, -9, -10, 4, -4, 0, -13, -9, -15, -14, -12, -1, -5, -6, -15, 6, -22, -9, -4, -6, 8, -7, 6, -7, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 140, 114, 130, 106, 105, 108, 125, 140, 117, 112, 81, 50, 49, 61, 80, 108, 57, 48, 11, 6, -0, 7, 38, 47, 28, 13, -9, -22, -22, -14, 11, 18, -12, -9, -26, -39, -45, -42, -11, -25, -12, -23, -37, -49, -63, -51, -38, -29, 0, 0, 0, 0, 0, 0, 0, 0}
    };
    public static int[] isolatedPawnPenalty = {26, 9, -6, -19, -34, -56, -71, -74, 49};
    public static EvalPair doubledPawnPenalty = new EvalPair(0, -30);
    public static EvalPair protectedPawn = new EvalPair(14, 5);
    public static int[,] friendlyKingDistPasser = {
        {0, -4, -11, -9, -3, 4, 21, 6},
        {0, 66, 55, 38, 30, 27, 24, 24}
    };
    public static EvalPair bishopPairBonus = new EvalPair(40, 52);
    public static EvalPair bishopMobility = new EvalPair(9, 14);
    public static EvalPair rookOpenFile = new EvalPair(40, -7);
    public static EvalPair rookSemiOpenFile = new EvalPair(11, 11);
    public static EvalPair rookMobility = new EvalPair(0, 12);
    public static EvalPair rookKingRingAttack = new EvalPair(12, -4);
    public static EvalPair kingOpenFile = new EvalPair(-40, 8);
    public static EvalPair kingPawnShield = new EvalPair(19, -10);
    


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

        int ourKing = board.GetPieces(currentColorIndex, Piece.King).GetLSB();

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
                int dist = Coord.ChebyshevDist(ourKing, index);
                score.mg += friendlyKingDistPasser[0, dist]; 
                score.eg += friendlyKingDistPasser[1, dist]; 
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