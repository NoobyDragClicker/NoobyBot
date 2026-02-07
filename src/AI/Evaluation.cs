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
        {0, 0, 0, 0, 0, 0, 0, 0, 70, 83, 69, 82, 71, 63, 38, 15, 63, 77, 105, 103, 112, 153, 146, 93, 44, 69, 71, 70, 96, 86, 91, 69, 30, 60, 57, 75, 75, 64, 72, 51, 29, 54, 54, 54, 71, 57, 89, 59, 29, 56, 48, 40, 58, 74, 97, 51, 0, 0, 0, 0, 0, 0, 0, 0},
        {138, 158, 203, 241, 251, 211, 166, 184, 229, 253, 270, 289, 271, 333, 245, 273, 235, 277, 305, 311, 344, 337, 307, 269, 236, 252, 279, 306, 289, 311, 264, 273, 220, 236, 256, 260, 270, 261, 260, 234, 200, 228, 244, 244, 257, 249, 251, 220, 183, 196, 216, 232, 234, 234, 216, 218, 141, 199, 175, 197, 205, 217, 204, 163},
        {239, 214, 207, 174, 197, 205, 205, 203, 241, 264, 251, 247, 273, 264, 264, 245, 246, 271, 273, 294, 276, 310, 284, 273, 237, 246, 272, 283, 283, 279, 247, 239, 221, 240, 247, 268, 267, 248, 244, 231, 234, 244, 243, 249, 249, 242, 245, 249, 237, 240, 253, 229, 238, 250, 258, 240, 206, 236, 219, 207, 212, 214, 232, 219},
        {354, 334, 345, 342, 370, 362, 339, 387, 346, 346, 362, 381, 370, 399, 379, 409, 324, 343, 350, 353, 378, 380, 406, 387, 303, 315, 319, 330, 340, 340, 341, 348, 279, 286, 291, 306, 311, 295, 317, 311, 274, 281, 292, 289, 298, 297, 333, 313, 269, 282, 299, 295, 300, 305, 325, 292, 294, 294, 305, 311, 317, 307, 320, 296},
        {473, 493, 525, 553, 555, 583, 577, 522, 513, 487, 502, 502, 516, 542, 515, 551, 509, 510, 514, 526, 530, 577, 570, 560, 490, 488, 503, 500, 507, 515, 506, 516, 480, 488, 480, 493, 495, 487, 501, 502, 480, 485, 480, 479, 484, 490, 504, 502, 473, 482, 492, 493, 491, 501, 511, 519, 463, 457, 467, 487, 475, 460, 492, 483},
        {-82, -96, -67, -121, -73, -5, 39, 101, -106, -44, -99, -25, -49, -32, 46, 24, -108, -1, -86, -97, -72, 13, 8, -26, -67, -79, -89, -163, -141, -98, -87, -109, -63, -68, -114, -145, -144, -107, -100, -117, -29, -20, -67, -82, -77, -81, -35, -48, 57, 15, -2, -34, -39, -21, 30, 41, 47, 77, 49, -54, 12, -34, 58, 55}
    };
    public static int[,] eg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 148, 124, 141, 119, 118, 117, 132, 147, 121, 121, 110, 109, 98, 96, 117, 118, 113, 102, 103, 90, 92, 91, 92, 94, 99, 95, 98, 99, 95, 94, 83, 82, 97, 90, 100, 104, 106, 102, 80, 81, 104, 97, 107, 104, 121, 105, 82, 86, 0, 0, 0, 0, 0, 0, 0, 0},
        {222, 271, 293, 277, 291, 266, 271, 199, 274, 286, 301, 300, 290, 277, 283, 253, 289, 300, 313, 315, 300, 302, 288, 274, 292, 315, 330, 331, 331, 325, 316, 288, 301, 312, 331, 330, 333, 324, 308, 284, 281, 299, 313, 326, 322, 305, 293, 282, 277, 292, 298, 299, 298, 294, 281, 281, 254, 248, 282, 283, 278, 268, 261, 270},
        {305, 317, 317, 328, 323, 312, 315, 300, 294, 312, 318, 318, 308, 313, 312, 294, 320, 317, 329, 317, 321, 324, 318, 314, 318, 336, 332, 344, 336, 331, 333, 316, 315, 334, 342, 339, 339, 336, 326, 305, 316, 323, 334, 335, 339, 333, 317, 307, 316, 308, 308, 319, 324, 310, 314, 290, 294, 309, 287, 307, 300, 307, 296, 271},
        {561, 570, 580, 578, 564, 559, 565, 547, 562, 572, 577, 569, 566, 551, 553, 538, 564, 564, 566, 562, 551, 544, 538, 535, 566, 564, 573, 567, 551, 548, 544, 539, 562, 564, 567, 563, 557, 554, 539, 540, 554, 552, 551, 557, 550, 541, 516, 522, 547, 549, 548, 550, 539, 535, 522, 531, 545, 550, 557, 557, 549, 544, 536, 534},
        {1011, 1015, 1027, 1020, 1021, 979, 957, 993, 995, 1032, 1054, 1063, 1074, 1044, 1026, 1009, 993, 1009, 1048, 1058, 1066, 1047, 1005, 1012, 1005, 1036, 1045, 1071, 1083, 1069, 1065, 1038, 1020, 1030, 1051, 1069, 1065, 1062, 1038, 1026, 986, 1024, 1043, 1045, 1041, 1038, 1006, 990, 991, 996, 1002, 1006, 1012, 977, 940, 908, 980, 992, 995, 1004, 992, 975, 935, 935},
        {-73, -35, -22, 6, -10, -4, -2, -89, -16, 13, 25, 12, 29, 34, 28, 4, -5, 17, 37, 49, 51, 45, 40, 8, -15, 19, 41, 59, 60, 54, 41, 14, -23, 6, 36, 53, 55, 41, 25, 7, -31, -5, 15, 30, 30, 23, 5, -8, -37, -18, -8, 1, 7, 2, -14, -34, -70, -59, -40, -28, -47, -23, -46, -73}
    };
    public static int[,] passedPawnBonuses = {
        {0, 0, 0, 0, 0, 0, 0, 0, 70, 83, 69, 82, 71, 64, 38, 15, 30, 31, 20, 13, 11, -4, -39, -42, 16, 5, 17, 19, 0, 8, -30, -19, -2, -14, -23, -12, -22, -7, -24, -14, -7, -23, -27, -21, -26, -14, -25, 2, -14, -12, -18, -21, -11, -11, -1, -11, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 148, 124, 141, 119, 118, 117, 132, 147, 143, 142, 107, 78, 82, 95, 114, 134, 87, 83, 51, 44, 43, 50, 81, 83, 62, 54, 34, 23, 28, 35, 61, 60, 23, 33, 19, 10, 11, 12, 42, 23, 24, 25, 13, 6, -8, 7, 21, 22, 0, 0, 0, 0, 0, 0, 0, 0}
    };
    public static int[] isolatedPawnPenalty = {33, 12, -8, -25, -45, -71, -88, -87, 49};
    public static int doubledPawnPenalty = -29;
    public static int bishopPairBonusMG = 42;
    public static int bishopPairBonusEG = 55;

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
