using System;
using System.Linq.Expressions;
using System.Net.Security;

public class Evaluation
{

    int colorTurn;

    int numWhiteIsolatedPawns;
    int numBlackIsolatedPawns;
    SearchLogger logger;

    //Unused in actual eval
    public static int pawnValue = 90;
    public static int knightValue = 336;
    public static int bishopValue = 366;
    public static int rookValue = 538;
    public static int queenValue = 1024;
    
    public static int[,] mg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 103, 124, 103, 121, 99, 86, 60, 21, 60, 76, 111, 107, 115, 140, 123, 68, 33, 56, 62, 71, 88, 76, 79, 53, 20, 45, 48, 67, 61, 56, 64, 35, 17, 44, 44, 45, 61, 48, 79, 44, 20, 44, 41, 31, 53, 60, 81, 36, 0, 0, 0, 0, 0, 0, 0, 0},
        {134, 82, 138, 154, 150, 140, 78, 148, 183, 217, 233, 234, 225, 275, 189, 207, 184, 237, 268, 280, 304, 286, 262, 214, 202, 225, 248, 273, 262, 279, 238, 237, 190, 202, 222, 230, 240, 227, 225, 203, 175, 202, 214, 216, 226, 219, 221, 191, 157, 168, 188, 202, 207, 200, 183, 190, 103, 176, 142, 166, 175, 182, 177, 123},
        {231, 166, 155, 159, 156, 180, 131, 185, 231, 257, 242, 223, 253, 248, 251, 234, 241, 260, 268, 288, 269, 298, 266, 272, 230, 249, 267, 280, 281, 267, 250, 235, 224, 239, 247, 266, 263, 253, 242, 234, 235, 248, 244, 250, 249, 244, 250, 249, 241, 243, 252, 233, 243, 249, 259, 244, 202, 240, 224, 204, 209, 219, 218, 216},
        {269, 237, 253, 254, 269, 222, 201, 268, 289, 291, 301, 318, 308, 329, 295, 319, 270, 284, 299, 302, 324, 319, 310, 310, 253, 258, 271, 280, 293, 286, 272, 285, 238, 239, 243, 256, 268, 249, 264, 260, 230, 241, 245, 249, 257, 253, 286, 273, 230, 238, 254, 253, 258, 263, 280, 252, 258, 257, 263, 272, 276, 265, 276, 259},
        {428, 410, 432, 449, 448, 433, 445, 454, 484, 470, 480, 473, 485, 514, 494, 522, 489, 481, 495, 503, 504, 542, 542, 535, 473, 470, 483, 485, 489, 498, 488, 501, 464, 468, 466, 478, 474, 471, 488, 486, 472, 465, 460, 462, 461, 474, 482, 490, 461, 467, 475, 470, 472, 481, 497, 497, 449, 443, 448, 471, 462, 448, 468, 451},
        {-17, -21, -15, -19, -12, 0, 8, 1, -19, -2, -20, -5, -7, -1, 20, 12, -21, 14, -19, -27, -17, 10, 12, -1, -14, -17, -25, -68, -62, -35, -28, -33, -14, -20, -51, -76, -82, -52, -52, -56, 6, 11, -27, -40, -35, -37, 6, -9, 76, 49, 27, -0, -7, 16, 66, 69, 70, 109, 85, -17, 48, 0, 89, 83}
    };

    public static int[,] eg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 59, 17, 43, 13, 9, -2, 24, 46, 123, 119, 82, 55, 52, 61, 99, 106, 115, 97, 85, 67, 68, 74, 89, 91, 98, 93, 80, 75, 75, 78, 80, 78, 94, 84, 84, 80, 83, 81, 76, 77, 106, 92, 88, 81, 96, 88, 81, 84, 0, 0, 0, 0, 0, 0, 0, 0},
        {146, 187, 219, 210, 216, 184, 176, 128, 196, 207, 220, 225, 214, 198, 199, 176, 214, 221, 234, 235, 220, 226, 207, 197, 205, 230, 250, 250, 249, 242, 229, 210, 219, 230, 249, 246, 251, 242, 224, 202, 200, 218, 237, 245, 241, 225, 213, 194, 190, 205, 215, 216, 219, 209, 192, 196, 151, 176, 198, 201, 192, 184, 184, 163},
        {227, 239, 246, 251, 248, 232, 240, 217, 215, 238, 241, 247, 235, 233, 235, 211, 240, 242, 248, 239, 243, 245, 239, 230, 240, 257, 258, 261, 258, 249, 253, 238, 234, 257, 262, 261, 257, 255, 247, 220, 236, 243, 253, 256, 261, 251, 235, 227, 234, 233, 230, 238, 248, 227, 232, 202, 215, 226, 210, 232, 219, 228, 206, 185},
        {430, 443, 454, 451, 439, 442, 441, 424, 428, 434, 443, 440, 436, 421, 425, 413, 429, 436, 432, 432, 421, 413, 416, 410, 432, 429, 437, 432, 419, 415, 414, 411, 423, 428, 432, 427, 419, 414, 401, 399, 417, 416, 416, 420, 411, 402, 376, 380, 410, 415, 410, 412, 402, 397, 380, 392, 416, 416, 422, 419, 417, 412, 406, 406},
        {684, 690, 706, 698, 697, 668, 637, 658, 635, 663, 679, 682, 685, 662, 615, 618, 619, 636, 668, 677, 678, 672, 619, 623, 623, 649, 666, 688, 706, 684, 675, 648, 642, 640, 679, 687, 690, 678, 656, 632, 579, 652, 666, 680, 671, 668, 629, 577, 581, 612, 648, 648, 654, 597, 542, 471, 574, 597, 612, 656, 617, 581, 483, 459},
        {-55, -37, -25, -12, -17, -1, 10, -35, -30, 6, 9, 6, 17, 22, 28, 8, -15, 15, 22, 32, 37, 38, 35, 7, -17, 12, 30, 37, 40, 41, 33, 8, -27, -1, 23, 43, 42, 32, 19, 2, -30, -5, 10, 22, 23, 15, 3, -8, -29, -16, -9, -3, 4, -1, -13, -30, -58, -50, -33, -25, -43, -18, -38, -63}
    };
    public static int[] passedPawnBonuses = {0, 7, 16, 31, 54, 102, 198, 0};
    public static int[] isolatedPawnPenalty = {35, 20, 6, -11, -21, -39, -45, -7, 0};
    public static int doubledPawnPenalty = -27;

    int playerTurnMultiplier;
    public Evaluation(SearchLogger logger)
    {
        this.logger = logger;
    }
    public int EvaluatePosition(Board board)
    {
        colorTurn = board.colorTurn;
        playerTurnMultiplier = (colorTurn == Piece.White) ? 1 : -1;

        numWhiteIsolatedPawns = 0;
        numBlackIsolatedPawns = 0;
        int boardVal = IncrementalCount(board);
        return boardVal;
    }

    int IncrementalCount(Board board)
    {
        const int totalPhase = 24;
        int mgMaterialCount = board.gameStateHistory[board.fullMoveClock].mgPSQTVal;
        int egMaterialCount = board.gameStateHistory[board.fullMoveClock].egPSQTVal;

        int pawnEval = 0;

        ulong whitePawns = board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Pawn)];
        ulong blackPawns = board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Pawn)];

        while (whitePawns != 0)
        {
            int index = BitboardHelper.PopLSB(ref whitePawns);
            pawnEval += EvaluatePawnStrength(board, index, Piece.White);
        }

        while (blackPawns != 0)
        {
            int index = BitboardHelper.PopLSB(ref blackPawns);
            pawnEval -= EvaluatePawnStrength(board, index, Piece.Black);
        }

        pawnEval += isolatedPawnPenalty[numWhiteIsolatedPawns];
        pawnEval -= isolatedPawnPenalty[numBlackIsolatedPawns];

        int phase = (4 * (board.pieceCounts[Board.WhiteIndex, Piece.Queen] + board.pieceCounts[Board.BlackIndex, Piece.Queen])) + (2 * (board.pieceCounts[Board.WhiteIndex, Piece.Rook] + board.pieceCounts[Board.BlackIndex, Piece.Rook]));
        phase += board.pieceCounts[Board.WhiteIndex, Piece.Knight] + board.pieceCounts[Board.BlackIndex, Piece.Knight] + board.pieceCounts[Board.WhiteIndex, Piece.Bishop] + board.pieceCounts[Board.BlackIndex, Piece.Bishop];

        int egScore = egMaterialCount + pawnEval;
        if (phase > 24) { phase = 24; }
        return (mgMaterialCount * phase + egScore * (totalPhase - phase)) / totalPhase * playerTurnMultiplier;
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

    int EvaluatePawnStrength(Board board, int pawnIndex, int pawnColor)
    {
        int bonus = 0;
        if (pawnColor == Piece.White)
        {
            int ppBonusIndex = Coord.IndexToRank(pawnIndex) - 1;
            //Passed pawn
            if ((board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Pawn)] & BitboardHelper.wPawnPassedMask[pawnIndex]) == 0) { bonus += passedPawnBonuses[ppBonusIndex]; }
            //Doubled pawn penalty
            if (board.PieceAt(pawnIndex - 8) == Piece.Pawn && board.ColorAt(pawnIndex - 8) == Piece.White) { bonus += doubledPawnPenalty; }
            if ((BitboardHelper.isolatedPawnMask[pawnIndex] & board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Pawn)]) == 0) { numWhiteIsolatedPawns++; }
        }
        else
        {
            int ppBonusIndex = 8 - Coord.IndexToRank(pawnIndex);
            //Passed pawn
            if ((board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Pawn)] & BitboardHelper.bPawnPassedMask[pawnIndex]) == 0) { bonus += passedPawnBonuses[ppBonusIndex]; }
            //Doubled pawn penalty
            if (board.PieceAt(pawnIndex + 8) == Piece.Pawn && board.ColorAt(pawnIndex + 8) == Piece.Black) { bonus += doubledPawnPenalty; }
            if((BitboardHelper.isolatedPawnMask[pawnIndex] & board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Pawn)]) == 0){ numBlackIsolatedPawns++; }
        }

        return bonus;
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
