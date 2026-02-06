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
        {0, 0, 0, 0, 0, 0, 0, 0, 60, 72, 61, 73, 62, 54, 34, 17, 52, 69, 100, 91, 96, 143, 129, 80, 43, 61, 69, 67, 91, 81, 81, 64, 28, 57, 51, 69, 68, 60, 62, 44, 27, 51, 50, 44, 65, 51, 79, 55, 26, 54, 46, 38, 57, 70, 92, 46, 0, 0, 0, 0, 0, 0, 0, 0},
        {133, 86, 144, 155, 152, 142, 76, 152, 184, 219, 236, 236, 227, 279, 187, 207, 181, 241, 275, 278, 301, 287, 267, 220, 204, 231, 251, 278, 264, 280, 239, 243, 197, 204, 234, 236, 248, 240, 230, 206, 176, 207, 222, 216, 231, 229, 226, 196, 158, 170, 194, 210, 212, 207, 190, 195, 106, 181, 152, 169, 182, 192, 183, 126},
        {202, 147, 140, 141, 139, 157, 115, 163, 203, 224, 211, 193, 228, 222, 218, 206, 215, 232, 239, 255, 233, 275, 241, 238, 199, 217, 236, 246, 245, 239, 218, 207, 188, 206, 219, 238, 235, 217, 217, 201, 206, 213, 215, 217, 217, 211, 214, 218, 201, 208, 226, 197, 207, 217, 228, 211, 173, 202, 192, 175, 176, 185, 188, 181},
        {272, 236, 253, 250, 269, 224, 204, 267, 290, 294, 308, 314, 313, 326, 303, 319, 274, 286, 297, 300, 320, 322, 312, 309, 256, 266, 271, 279, 294, 290, 279, 290, 237, 237, 243, 259, 262, 255, 269, 262, 235, 241, 249, 244, 255, 258, 294, 272, 233, 239, 260, 253, 256, 268, 280, 251, 254, 254, 269, 270, 280, 269, 281, 258},
        {433, 412, 435, 452, 453, 439, 444, 462, 489, 468, 481, 471, 489, 512, 496, 526, 488, 482, 497, 504, 505, 541, 542, 539, 474, 472, 483, 486, 490, 497, 486, 504, 469, 474, 466, 481, 478, 477, 490, 488, 474, 471, 463, 465, 467, 472, 491, 488, 462, 473, 476, 480, 473, 485, 496, 503, 452, 447, 454, 471, 463, 448, 473, 451},
        {-18, -19, -15, -19, -14, -1, 8, 2, -18, -3, -20, -5, -7, -1, 19, 14, -23, 13, -18, -28, -18, 13, 14, -0, -12, -18, -27, -66, -59, -36, -29, -33, -14, -20, -53, -78, -78, -50, -48, -50, 2, 9, -24, -42, -30, -39, 6, -6, 78, 42, 28, 3, -2, 16, 64, 74, 70, 104, 83, -13, 44, 1, 88, 83}
    };
    public static int[,] eg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 125, 104, 116, 97, 98, 93, 105, 119, 104, 107, 96, 88, 81, 84, 107, 105, 100, 89, 87, 78, 74, 75, 82, 82, 86, 81, 82, 82, 82, 80, 70, 66, 85, 75, 84, 85, 89, 87, 66, 69, 95, 87, 91, 89, 103, 90, 75, 77, 0, 0, 0, 0, 0, 0, 0, 0},
        {149, 186, 219, 208, 218, 184, 177, 130, 196, 206, 222, 226, 216, 202, 203, 178, 214, 223, 234, 234, 220, 228, 209, 198, 204, 228, 247, 247, 245, 240, 229, 204, 218, 234, 248, 246, 248, 240, 220, 198, 202, 213, 232, 241, 239, 223, 211, 198, 193, 206, 214, 217, 218, 211, 192, 194, 154, 175, 202, 202, 194, 186, 183, 164},
        {224, 238, 240, 246, 240, 230, 232, 214, 216, 232, 241, 240, 236, 231, 233, 211, 240, 238, 250, 241, 242, 243, 238, 229, 239, 255, 254, 265, 254, 247, 251, 232, 231, 254, 261, 261, 257, 254, 243, 223, 234, 242, 254, 256, 257, 250, 236, 226, 233, 227, 226, 242, 241, 224, 235, 203, 213, 225, 209, 223, 216, 230, 202, 181},
        {434, 442, 451, 451, 438, 442, 438, 424, 430, 437, 446, 441, 436, 423, 428, 413, 434, 437, 434, 430, 418, 413, 416, 404, 432, 426, 438, 431, 416, 411, 411, 404, 421, 423, 433, 428, 417, 416, 399, 398, 413, 412, 416, 420, 410, 399, 371, 380, 409, 413, 411, 412, 403, 395, 378, 388, 417, 418, 423, 423, 415, 410, 402, 406},
        {685, 692, 702, 694, 703, 667, 630, 653, 634, 666, 677, 685, 684, 661, 615, 613, 620, 636, 667, 675, 678, 667, 617, 619, 618, 648, 672, 692, 699, 686, 673, 651, 643, 639, 675, 687, 690, 675, 659, 633, 580, 657, 667, 682, 666, 671, 623, 576, 579, 611, 647, 648, 653, 598, 542, 471, 574, 596, 614, 656, 611, 580, 481, 455},
        {-55, -33, -21, -12, -17, -2, 14, -33, -26, 7, 7, 8, 19, 25, 31, 9, -17, 13, 24, 31, 39, 39, 38, 6, -23, 10, 29, 44, 46, 42, 33, 4, -29, -0, 25, 40, 40, 31, 18, 2, -30, -6, 10, 21, 26, 18, 4, -6, -35, -17, -9, -2, 5, 3, -15, -36, -55, -54, -33, -27, -43, -20, -36, -63}
    };
    public static int[,] passedPawnBonuses = {
        {0, 0, 0, 0, 0, 0, 0, 0, 60, 72, 61, 73, 62, 54, 34, 17, 29, 30, 18, 13, 18, -3, -18, -30, 18, 8, 19, 19, -1, 3, -18, -16, -4, -9, -21, -12, -23, -8, -19, -13, -9, -21, -28, -20, -24, -13, -23, -2, -17, -13, -18, -14, -9, -13, -3, -12, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 125, 104, 116, 97, 98, 93, 105, 119, 120, 115, 83, 64, 68, 77, 88, 105, 71, 70, 41, 37, 34, 41, 70, 65, 51, 42, 27, 18, 23, 29, 50, 54, 15, 25, 16, 9, 9, 13, 37, 18, 17, 24, 7, 1, -13, 4, 20, 13, 0, 0, 0, 0, 0, 0, 0, 0}
    };
    public static int[] isolatedPawnPenalty = {49, 31, 14, 4, -11, -34, -39, -7, 0};
    public static int doubledPawnPenalty = -26;
    public static int bishopPairBonusMG = 50;
    public static int bishopPairBonusEG = 52;

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
