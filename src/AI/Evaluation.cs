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
        {0, 0, 0, 0, 0, 0, 0, 0, 60, 75, 55, 67, 56, 54, 41, 17, 66, 75, 103, 103, 112, 151, 131, 94, 49, 65, 64, 70, 91, 83, 80, 69, 35, 55, 55, 71, 72, 71, 62, 49, 32, 41, 45, 49, 61, 60, 72, 53, 39, 54, 45, 50, 54, 84, 83, 53, 0, 0, 0, 0, 0, 0, 0, 0},
        {143, 151, 193, 230, 243, 202, 165, 179, 221, 252, 269, 288, 271, 321, 246, 266, 231, 278, 309, 312, 342, 341, 306, 272, 238, 258, 283, 314, 294, 316, 271, 277, 224, 240, 266, 271, 281, 272, 266, 241, 206, 233, 253, 257, 273, 263, 258, 226, 190, 204, 222, 245, 244, 247, 226, 227, 145, 205, 189, 213, 217, 229, 213, 172},
        {200, 161, 150, 120, 149, 154, 153, 171, 191, 199, 182, 180, 204, 189, 199, 181, 194, 207, 203, 214, 199, 242, 216, 223, 190, 189, 196, 214, 208, 200, 192, 194, 186, 180, 179, 203, 198, 180, 182, 200, 192, 196, 192, 184, 189, 194, 199, 210, 199, 202, 202, 182, 191, 203, 226, 203, 192, 208, 192, 179, 188, 189, 200, 209},
        {308, 285, 283, 275, 296, 289, 290, 333, 302, 302, 312, 327, 316, 337, 331, 353, 287, 308, 304, 301, 322, 330, 358, 336, 287, 298, 294, 297, 305, 310, 309, 316, 277, 276, 277, 285, 295, 283, 299, 295, 278, 274, 281, 282, 296, 299, 323, 309, 280, 282, 291, 292, 299, 311, 319, 296, 304, 300, 303, 312, 319, 318, 317, 306},
        {474, 489, 519, 547, 548, 578, 579, 521, 510, 489, 504, 501, 516, 537, 525, 556, 509, 511, 518, 525, 533, 578, 575, 562, 493, 495, 509, 505, 512, 520, 519, 523, 486, 493, 490, 501, 504, 502, 510, 511, 485, 492, 493, 495, 499, 502, 514, 510, 481, 495, 503, 507, 502, 515, 522, 526, 470, 471, 481, 495, 488, 475, 500, 493},
        {-82, -86, -55, -103, -69, -10, 42, 104, -95, -28, -79, -1, -35, -34, 47, 28, -93, 22, -67, -79, -57, 11, 8, -27, -44, -55, -74, -151, -135, -94, -76, -104, -50, -50, -99, -140, -137, -91, -82, -112, -24, -7, -51, -73, -67, -67, -27, -48, 51, 22, 3, -27, -34, -11, 21, 30, 29, 67, 44, -40, 19, -31, 49, 42}
    };
    public static int[,] eg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 134, 111, 133, 113, 115, 116, 125, 136, 121, 125, 109, 109, 102, 102, 123, 119, 113, 105, 105, 92, 95, 95, 96, 95, 100, 99, 97, 98, 97, 92, 87, 82, 98, 93, 100, 104, 106, 100, 81, 81, 105, 101, 108, 112, 124, 105, 88, 85, 0, 0, 0, 0, 0, 0, 0, 0},
        {234, 280, 303, 285, 300, 275, 279, 211, 282, 293, 309, 309, 299, 288, 289, 262, 298, 308, 321, 322, 309, 309, 297, 281, 298, 322, 338, 337, 338, 333, 321, 294, 309, 320, 338, 338, 341, 330, 315, 290, 289, 307, 318, 335, 329, 309, 300, 291, 287, 301, 309, 307, 307, 302, 289, 289, 263, 278, 294, 297, 294, 281, 289, 278},
        {236, 244, 240, 251, 246, 231, 238, 229, 226, 217, 220, 220, 207, 214, 216, 228, 242, 221, 205, 194, 199, 200, 220, 237, 238, 224, 207, 199, 191, 205, 219, 236, 233, 225, 211, 198, 195, 205, 218, 226, 237, 223, 215, 214, 218, 212, 218, 229, 249, 219, 211, 222, 222, 213, 219, 225, 231, 239, 243, 233, 230, 247, 227, 208},
        {449, 454, 463, 457, 449, 456, 459, 444, 456, 463, 467, 455, 456, 447, 449, 438, 458, 454, 454, 449, 441, 437, 436, 437, 459, 450, 456, 448, 435, 437, 442, 442, 452, 449, 449, 443, 438, 438, 433, 438, 446, 441, 437, 440, 431, 422, 408, 418, 441, 438, 437, 437, 428, 420, 411, 425, 453, 439, 443, 436, 429, 442, 424, 440},
        {1041, 1047, 1062, 1054, 1056, 1012, 987, 1024, 1025, 1061, 1083, 1094, 1104, 1074, 1050, 1035, 1022, 1040, 1077, 1092, 1098, 1076, 1028, 1039, 1033, 1061, 1073, 1101, 1112, 1096, 1089, 1061, 1045, 1058, 1077, 1097, 1093, 1084, 1066, 1051, 1014, 1047, 1067, 1067, 1065, 1063, 1033, 1015, 1017, 1019, 1026, 1030, 1040, 1002, 965, 938, 1006, 1021, 1031, 1055, 1027, 1002, 966, 960},
        {-63, -24, -13, 10, -3, 5, 2, -82, -9, 15, 24, 10, 25, 35, 29, 12, -4, 12, 30, 40, 43, 38, 34, 6, -22, 7, 28, 46, 47, 43, 30, 10, -29, -5, 22, 41, 43, 28, 14, 3, -30, -11, 7, 22, 23, 17, 2, -5, -29, -15, -5, 2, 9, 6, -5, -23, -56, -48, -26, -13, -25, -7, -35, -65}
    };
    public static int[,] passedPawnBonuses = {
        {0, 0, 0, 0, 0, 0, 0, 0, 60, 75, 55, 67, 56, 54, 41, 17, 13, 25, 9, -3, -8, -15, -41, -57, 5, 5, 21, 12, -4, 5, -27, -26, -7, -12, -18, -3, -13, -11, -23, -12, -8, -14, -12, 0, -4, -10, -16, 9, -14, -3, 2, 3, 17, 1, 14, 2, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 134, 111, 133, 113, 115, 116, 125, 136, 107, 109, 87, 60, 64, 75, 86, 107, 45, 42, 11, 10, 7, 14, 42, 44, 12, 2, -15, -28, -27, -15, 8, 8, -32, -26, -42, -57, -60, -55, -25, -43, -34, -44, -60, -75, -88, -75, -62, -54, 0, 0, 0, 0, 0, 0, 0, 0}
    };
    public static int[] isolatedPawnPenalty = {25, 9, -6, -18, -33, -55, -70, -75, 49};
    public static EvalPair doubledPawnPenalty = new EvalPair(0, -31);
    public static EvalPair protectedPawn = new EvalPair(14, 6);
    public static int[,] friendlyKingDistPasser = {
        {0, -12, -16, -12, -4, 3, 22, 10},
        {0, 55, 42, 23, 12, 8, 4, 5}
    };
    public static int[,] enemyKingDistPasser = {
        {0, -60, 22, 8, 5, -3, -7, -32},
        {0, -29, 2, 26, 36, 42, 45, 49}
    };
    public static EvalPair bishopPairBonus = new EvalPair(40, 51);
    public static EvalPair bishopMobility = new EvalPair(9, 14);
    public static EvalPair rookOpenFile = new EvalPair(40, -7);
    public static EvalPair rookSemiOpenFile = new EvalPair(11, 13);
    public static EvalPair rookMobility = new EvalPair(0, 12);
    public static EvalPair rookKingRingAttack = new EvalPair(13, -4);
    public static EvalPair kingOpenFile = new EvalPair(-40, 4);
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
        int theirKing = board.GetPieces(oppositeColorIndex, Piece.King).GetLSB();

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
                int ourDist = Coord.ChebyshevDist(ourKing, index);
                score.mg += friendlyKingDistPasser[0, ourDist]; 
                score.eg += friendlyKingDistPasser[1, ourDist]; 
                int theirDist = Coord.ChebyshevDist(theirKing, index);
                score.mg += enemyKingDistPasser[0, theirDist]; 
                score.eg += enemyKingDistPasser[1, theirDist]; 
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