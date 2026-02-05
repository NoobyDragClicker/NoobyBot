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
                {0, 0, 0, 0, 0, 0, 0, 0, 56, 74, 63, 74, 63, 54, 39, 15, 55, 69, 99, 91, 98, 141, 123, 78, 39, 60, 66, 64, 91, 76, 82, 64, 26, 57, 54, 69, 69, 58, 63, 45, 25, 49, 50, 50, 65, 47, 81, 53, 28, 50, 46, 37, 55, 69, 84, 44, 0, 0, 0, 0, 0, 0, 0, 0},
        {136, 85, 138, 161, 150, 144, 77, 152, 186, 219, 237, 237, 224, 279, 188, 210, 187, 240, 269, 276, 303, 286, 268, 220, 205, 229, 251, 278, 266, 280, 240, 239, 194, 204, 229, 236, 251, 235, 231, 211, 177, 204, 220, 215, 228, 230, 231, 198, 155, 169, 190, 208, 212, 207, 190, 197, 105, 180, 149, 173, 179, 191, 185, 127},
        {204, 152, 138, 137, 139, 153, 113, 162, 200, 227, 216, 196, 226, 221, 220, 202, 209, 233, 238, 255, 234, 271, 243, 238, 200, 216, 239, 248, 250, 245, 215, 204, 191, 212, 215, 236, 232, 215, 214, 201, 207, 214, 213, 220, 222, 209, 213, 219, 207, 208, 222, 201, 209, 219, 225, 210, 172, 203, 189, 168, 179, 183, 190, 185},
        {272, 237, 249, 249, 270, 220, 204, 268, 291, 299, 305, 319, 309, 331, 300, 323, 271, 282, 297, 298, 321, 320, 312, 311, 253, 264, 273, 278, 292, 291, 278, 291, 237, 241, 245, 262, 264, 256, 269, 267, 235, 240, 249, 246, 253, 257, 296, 272, 230, 242, 259, 250, 257, 267, 278, 252, 257, 256, 268, 274, 278, 269, 283, 259},
        {432, 412, 432, 451, 451, 431, 444, 461, 484, 474, 482, 473, 488, 514, 494, 521, 492, 479, 494, 502, 504, 542, 540, 543, 480, 473, 489, 484, 489, 497, 493, 501, 463, 474, 470, 479, 477, 471, 483, 492, 471, 468, 466, 459, 468, 473, 492, 491, 458, 468, 477, 477, 472, 488, 502, 503, 448, 443, 457, 467, 459, 447, 467, 455},
        {-16, -19, -16, -18, -13, -0, 9, 2, -17, -3, -18, -4, -8, -1, 19, 14, -20, 16, -20, -29, -20, 12, 13, -1, -13, -20, -27, -68, -60, -35, -29, -33, -16, -22, -53, -76, -80, -52, -47, -51, 4, 8, -29, -42, -35, -38, 0, -8, 72, 39, 30, 2, -4, 15, 62, 69, 66, 107, 79, -24, 43, -1, 89, 83}
    };
    public static int[,] eg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 122, 102, 114, 99, 96, 94, 105, 117, 112, 114, 97, 96, 84, 87, 116, 110, 106, 105, 91, 80, 81, 85, 93, 92, 96, 102, 89, 85, 87, 85, 90, 76, 94, 99, 91, 94, 94, 92, 87, 77, 100, 107, 100, 93, 109, 98, 91, 86, 0, 0, 0, 0, 0, 0, 0, 0},
        {149, 186, 214, 206, 221, 182, 171, 130, 197, 204, 222, 226, 211, 202, 203, 177, 210, 221, 229, 237, 221, 226, 210, 194, 205, 231, 251, 249, 247, 239, 229, 206, 216, 232, 251, 244, 252, 243, 221, 200, 202, 214, 232, 242, 239, 224, 211, 195, 191, 203, 215, 216, 218, 211, 189, 199, 154, 174, 200, 199, 193, 184, 181, 166},
        {222, 239, 241, 242, 243, 228, 234, 211, 214, 233, 239, 238, 229, 235, 234, 208, 239, 239, 244, 239, 242, 246, 236, 231, 236, 255, 252, 261, 252, 248, 249, 232, 234, 254, 260, 258, 259, 252, 244, 223, 232, 240, 252, 257, 258, 251, 231, 227, 230, 228, 224, 239, 244, 228, 236, 203, 211, 225, 209, 221, 213, 229, 201, 182},
        {432, 439, 446, 451, 438, 440, 441, 423, 430, 438, 443, 441, 436, 420, 424, 411, 433, 434, 435, 430, 420, 413, 417, 405, 429, 427, 437, 433, 418, 416, 410, 406, 423, 428, 430, 427, 420, 414, 399, 393, 414, 414, 416, 424, 411, 397, 371, 378, 412, 413, 411, 414, 402, 394, 377, 388, 411, 416, 420, 425, 411, 410, 404, 404},
        {681, 692, 702, 697, 698, 665, 628, 651, 634, 661, 675, 678, 680, 660, 615, 612, 617, 631, 667, 674, 677, 667, 615, 619, 615, 647, 669, 689, 699, 688, 670, 648, 644, 638, 673, 686, 686, 675, 656, 627, 580, 656, 666, 676, 666, 669, 625, 575, 579, 609, 644, 643, 650, 598, 544, 472, 574, 598, 612, 659, 611, 584, 480, 457},
        {-55, -34, -27, -13, -18, 0, 13, -33, -26, 6, 10, 8, 13, 25, 27, 11, -15, 11, 28, 38, 38, 43, 34, 7, -17, 11, 32, 41, 47, 41, 31, 5, -26, -0, 28, 41, 43, 32, 20, -1, -30, -5, 9, 23, 24, 16, 2, -6, -32, -11, -9, -1, 5, -0, -10, -33, -56, -51, -38, -24, -41, -18, -35, -60}
    };
    public static int[,] passedPawnBonuses = {
        {0, 0, 0, 0, 0, 0, 0, 0, 56, 74, 63, 74, 63, 54, 39, 15, 25, 30, 23, 17, 16, 3, -21, -26, 15, 7, 18, 17, 0, 3, -22, -17, -4, -12, -21, -11, -21, -4, -16, -15, -9, -20, -19, -18, -22, -16, -23, -2, -16, -11, -19, -15, -9, -14, -1, -11, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 122, 102, 114, 99, 96, 94, 105, 117, 108, 106, 81, 63, 66, 78, 79, 98, 62, 57, 38, 36, 34, 37, 54, 59, 43, 35, 22, 17, 22, 25, 37, 41, 3, 15, 16, 2, 7, 5, 30, 11, 9, 6, 6, -1, -14, 1, 7, 4, 0, 0, 0, 0, 0, 0, 0, 0}
    };
    public static int[] isolatedPawnPenalty = {31, 27, 16, 4, -16, -40, -46, -5, -0};
    public static int doubledPawnPenalty = -39;
    public static int bishopPairBonusMG = 49;
    public static int bishopPairBonusEG = 49;

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
