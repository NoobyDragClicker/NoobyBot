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
        {0, 0, 0, 0, 0, 0, 0, 0, 69, 82, 69, 82, 70, 62, 38, 16, 60, 77, 105, 101, 110, 153, 143, 90, 43, 67, 70, 69, 95, 85, 89, 68, 28, 59, 56, 74, 74, 63, 70, 49, 28, 53, 52, 53, 69, 55, 87, 58, 28, 55, 47, 39, 58, 73, 95, 49, 0, 0, 0, 0, 0, 0, 0, 0},
        {143, 159, 208, 243, 249, 211, 172, 188, 225, 251, 267, 285, 267, 329, 240, 269, 231, 272, 302, 306, 340, 332, 304, 263, 233, 248, 275, 302, 285, 307, 259, 267, 216, 232, 253, 257, 267, 257, 255, 229, 196, 224, 240, 240, 253, 245, 247, 217, 180, 192, 211, 228, 230, 231, 212, 213, 137, 196, 172, 191, 199, 214, 199, 159},
        {238, 213, 205, 179, 196, 205, 200, 204, 236, 257, 246, 244, 269, 258, 260, 238, 239, 262, 268, 287, 271, 306, 278, 266, 230, 240, 266, 277, 276, 273, 241, 232, 214, 234, 240, 263, 260, 241, 239, 224, 227, 238, 237, 243, 243, 236, 240, 242, 230, 234, 246, 222, 231, 243, 251, 235, 200, 230, 213, 200, 205, 207, 225, 211},
        {347, 326, 339, 336, 365, 355, 332, 379, 341, 342, 357, 377, 366, 394, 373, 403, 320, 337, 345, 348, 374, 376, 400, 381, 299, 310, 315, 325, 335, 335, 335, 342, 275, 282, 287, 302, 307, 290, 313, 306, 270, 278, 288, 284, 292, 292, 329, 308, 266, 278, 294, 290, 296, 301, 321, 288, 290, 289, 302, 307, 313, 303, 316, 292},
        {485, 507, 542, 575, 572, 599, 586, 535, 528, 500, 518, 523, 536, 559, 532, 566, 525, 526, 528, 540, 544, 592, 584, 573, 506, 501, 518, 513, 521, 530, 519, 529, 491, 501, 493, 507, 508, 500, 516, 516, 494, 497, 493, 492, 497, 503, 518, 514, 487, 495, 505, 506, 504, 515, 523, 532, 475, 471, 480, 499, 487, 474, 506, 495},
        {-91, -110, -79, -129, -87, -16, 25, 66, -101, -46, -99, -36, -60, -37, 46, 29, -104, 2, -86, -101, -76, 6, 4, -30, -63, -77, -84, -160, -138, -95, -83, -102, -61, -66, -108, -144, -141, -105, -96, -111, -27, -19, -66, -81, -75, -78, -34, -46, 57, 14, -1, -34, -38, -19, 30, 40, 48, 76, 49, -53, 12, -34, 57, 53}
    };
    public static int[,] eg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 143, 119, 135, 114, 114, 111, 128, 142, 118, 117, 106, 106, 96, 92, 114, 115, 111, 98, 99, 87, 89, 89, 90, 92, 96, 92, 94, 94, 92, 91, 81, 79, 95, 88, 96, 101, 103, 98, 78, 79, 102, 94, 105, 101, 118, 102, 80, 84, 0, 0, 0, 0, 0, 0, 0, 0},
        {208, 257, 280, 262, 276, 250, 256, 184, 259, 267, 285, 285, 275, 260, 267, 239, 274, 284, 296, 299, 283, 285, 272, 258, 273, 297, 314, 314, 314, 308, 298, 271, 284, 297, 314, 314, 315, 307, 290, 268, 265, 284, 296, 310, 304, 289, 278, 265, 263, 277, 283, 282, 281, 279, 263, 266, 239, 234, 266, 267, 261, 251, 244, 256},
        {290, 302, 303, 312, 306, 296, 300, 283, 280, 296, 302, 303, 293, 297, 298, 279, 304, 302, 312, 301, 306, 308, 302, 298, 301, 320, 315, 327, 321, 315, 317, 301, 300, 318, 326, 324, 324, 320, 309, 291, 299, 306, 319, 320, 323, 317, 300, 292, 301, 294, 293, 304, 308, 294, 298, 274, 280, 294, 273, 292, 283, 292, 279, 255},
        {529, 538, 548, 547, 534, 529, 534, 517, 531, 541, 547, 539, 536, 521, 524, 509, 534, 535, 536, 531, 521, 513, 510, 504, 536, 534, 543, 537, 521, 517, 515, 509, 531, 534, 537, 532, 528, 523, 509, 508, 524, 522, 521, 527, 519, 510, 484, 492, 518, 521, 517, 520, 509, 504, 490, 501, 516, 520, 527, 527, 518, 514, 507, 506},
        {935, 934, 941, 930, 936, 896, 878, 915, 910, 947, 965, 973, 980, 954, 934, 920, 908, 922, 960, 969, 977, 959, 919, 924, 918, 950, 958, 983, 995, 983, 977, 949, 937, 942, 964, 981, 978, 974, 949, 939, 900, 939, 955, 958, 955, 952, 920, 901, 903, 910, 919, 919, 927, 889, 852, 821, 893, 906, 909, 919, 905, 886, 844, 846},
        {-70, -32, -20, 6, -8, -4, 1, -81, -20, 12, 24, 13, 31, 34, 27, 1, -6, 15, 37, 47, 51, 44, 39, 8, -15, 18, 39, 57, 58, 54, 39, 13, -24, 5, 34, 52, 53, 39, 24, 6, -31, -5, 13, 28, 28, 22, 4, -8, -38, -18, -8, 0, 7, 2, -14, -33, -68, -58, -40, -29, -46, -23, -44, -71}
    };
    public static int[,] passedPawnBonuses = {
        {0, 0, 0, 0, 0, 0, 0, 0, 69, 82, 69, 82, 70, 62, 38, 16, 32, 30, 19, 14, 13, -3, -35, -41, 16, 7, 17, 20, -1, 7, -29, -19, -1, -12, -23, -11, -22, -6, -22, -13, -7, -22, -27, -21, -25, -12, -25, 2, -14, -11, -17, -19, -10, -11, -1, -10, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 143, 119, 135, 114, 114, 111, 128, 142, 138, 137, 103, 75, 78, 91, 110, 128, 84, 80, 49, 42, 41, 49, 79, 81, 60, 53, 33, 22, 27, 33, 59, 59, 22, 32, 18, 11, 13, 12, 42, 22, 23, 24, 12, 4, -8, 6, 21, 20, 0, 0, 0, 0, 0, 0, 0, 0}
    };
    public static int[] isolatedPawnPenalty = {32, 11, -7, -24, -42, -68, -84, -77, 37};
    public static int doubledPawnPenalty = -29;
    public static int bishopPairBonusMG = 46;
    public static int bishopPairBonusEG = 54;

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


        ulong whitePawns = board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Pawn)];
        ulong blackPawns = board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Pawn)];

        while (whitePawns != 0)
        {
            int index = BitboardHelper.PopLSB(ref whitePawns);
            (int, int) pawnBonus = EvaluatePawnStrength(board, index, Board.WhiteIndex);
            mgScore += pawnBonus.Item1;
            egScore += pawnBonus.Item2;
        }

        while (blackPawns != 0)
        {
            int index = BitboardHelper.PopLSB(ref blackPawns);
            (int, int) pawnBonus = EvaluatePawnStrength(board, index, Board.BlackIndex);
            mgScore -= pawnBonus.Item1;
            egScore -= pawnBonus.Item2;
        }

        egScore += isolatedPawnPenalty[isolatedPawnCount[Board.WhiteIndex]];
        egScore -= isolatedPawnPenalty[isolatedPawnCount[Board.BlackIndex]];


        if(board.pieceCounts[Board.WhiteIndex, Piece.Bishop] >= 2)
        {
            mgScore += bishopPairBonusMG;
            egScore += bishopPairBonusMG;
        }
        if(board.pieceCounts[Board.BlackIndex, Piece.Bishop] >= 2)
        {
            mgScore -= bishopPairBonusMG;
            egScore -= bishopPairBonusMG;
        }

        int phase = (4 * (board.pieceCounts[Board.WhiteIndex, Piece.Queen] + board.pieceCounts[Board.BlackIndex, Piece.Queen])) + (2 * (board.pieceCounts[Board.WhiteIndex, Piece.Rook] + board.pieceCounts[Board.BlackIndex, Piece.Rook]));
        phase += board.pieceCounts[Board.WhiteIndex, Piece.Knight] + board.pieceCounts[Board.BlackIndex, Piece.Knight] + board.pieceCounts[Board.WhiteIndex, Piece.Bishop] + board.pieceCounts[Board.BlackIndex, Piece.Bishop];

        
        if (phase > 24) { phase = 24; }
        return (mgScore * phase + egScore * (totalPhase - phase)) / totalPhase * playerTurnMultiplier;
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

    (int, int) EvaluatePawnStrength(Board board, int pawnIndex, int currentColorIndex)
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
        if (board.PieceAt(pushSquare) == Piece.Pawn && board.ColorAt(pushSquare) == currentColor) { egBonus += doubledPawnPenalty; }
        if ((BitboardHelper.isolatedPawnMask[pawnIndex] & board.pieceBitboards[Board.PieceBitboardIndex(currentColorIndex, Piece.Pawn)]) == 0) { isolatedPawnCount[currentColorIndex]++; }

        return (mgBonus, egBonus);
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
