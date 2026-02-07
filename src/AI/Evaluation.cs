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
        {0, 0, 0, 0, 0, 0, 0, 0, 60, 73, 62, 73, 63, 54, 34, 18, 52, 70, 100, 92, 97, 144, 130, 81, 43, 63, 69, 69, 91, 82, 83, 66, 29, 58, 52, 69, 68, 62, 63, 45, 26, 51, 52, 46, 69, 52, 79, 56, 25, 54, 47, 38, 59, 68, 94, 45, 0, 0, 0, 0, 0, 0, 0, 0},
        {132, 86, 145, 155, 153, 142, 77, 151, 187, 221, 239, 238, 230, 282, 189, 210, 183, 244, 278, 282, 305, 290, 271, 223, 208, 235, 255, 282, 269, 284, 243, 247, 201, 208, 238, 240, 251, 244, 233, 210, 179, 211, 226, 220, 235, 234, 230, 200, 162, 174, 197, 212, 217, 211, 194, 199, 109, 185, 155, 173, 185, 196, 187, 129},
        {203, 148, 141, 141, 140, 158, 116, 164, 208, 228, 214, 195, 231, 226, 221, 210, 220, 236, 243, 260, 237, 278, 246, 244, 203, 222, 241, 251, 250, 243, 224, 212, 194, 211, 225, 243, 240, 223, 221, 206, 212, 219, 220, 222, 224, 217, 219, 223, 207, 214, 232, 203, 211, 222, 234, 216, 178, 207, 198, 180, 181, 191, 193, 186},
        {277, 240, 256, 254, 273, 227, 206, 271, 296, 300, 314, 320, 319, 331, 308, 325, 280, 292, 303, 306, 326, 327, 318, 315, 262, 272, 278, 285, 300, 296, 286, 297, 244, 243, 249, 266, 269, 262, 275, 269, 241, 248, 256, 251, 262, 265, 302, 279, 240, 246, 268, 260, 263, 275, 287, 258, 262, 262, 277, 276, 288, 276, 289, 265},
        {441, 418, 441, 458, 459, 443, 450, 469, 501, 480, 492, 481, 499, 524, 507, 537, 501, 493, 509, 516, 517, 552, 555, 553, 486, 485, 496, 499, 503, 510, 500, 519, 484, 487, 480, 495, 492, 491, 504, 502, 488, 486, 477, 480, 481, 487, 505, 502, 475, 487, 491, 496, 490, 499, 510, 515, 466, 461, 469, 486, 478, 462, 484, 461},
        {-18, -19, -14, -19, -13, -0, 9, 2, -17, -2, -19, -4, -6, -0, 19, 15, -22, 15, -17, -26, -16, 14, 15, 1, -11, -16, -24, -64, -57, -34, -26, -31, -12, -17, -50, -75, -74, -47, -45, -47, 5, 13, -20, -38, -26, -34, 10, -1, 83, 47, 33, 8, 3, 21, 70, 80, 76, 109, 88, -8, 49, 6, 92, 89}
    };
    public static int[,] eg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 124, 103, 116, 96, 98, 93, 105, 119, 104, 106, 96, 88, 80, 84, 107, 105, 100, 89, 87, 78, 73, 75, 81, 81, 86, 81, 80, 82, 82, 80, 70, 66, 85, 74, 84, 85, 88, 86, 66, 69, 94, 88, 90, 89, 102, 90, 76, 76, 0, 0, 0, 0, 0, 0, 0, 0},
        {150, 186, 219, 208, 218, 184, 177, 130, 195, 206, 221, 226, 216, 202, 203, 178, 214, 222, 233, 234, 219, 227, 209, 197, 203, 227, 246, 247, 244, 239, 228, 203, 217, 233, 247, 245, 248, 239, 219, 197, 201, 212, 231, 240, 238, 222, 210, 197, 192, 205, 213, 217, 216, 210, 191, 193, 154, 173, 201, 201, 193, 185, 182, 163},
        {223, 238, 240, 246, 240, 230, 232, 214, 215, 231, 240, 240, 235, 230, 233, 210, 239, 237, 249, 240, 241, 243, 237, 227, 238, 253, 252, 264, 253, 246, 250, 230, 230, 253, 260, 259, 256, 252, 242, 222, 233, 240, 253, 255, 255, 249, 235, 224, 232, 226, 224, 240, 239, 223, 232, 202, 212, 223, 207, 222, 215, 228, 201, 180},
        {433, 441, 450, 451, 438, 442, 438, 423, 429, 436, 444, 440, 435, 422, 427, 412, 433, 436, 433, 429, 416, 412, 415, 403, 431, 425, 437, 430, 414, 410, 410, 403, 420, 422, 432, 426, 416, 414, 398, 397, 412, 410, 414, 418, 409, 397, 369, 378, 407, 411, 409, 410, 401, 393, 377, 386, 414, 415, 422, 421, 413, 409, 400, 403},
        {684, 692, 702, 694, 703, 668, 630, 652, 631, 662, 675, 684, 683, 659, 613, 610, 616, 633, 664, 673, 675, 665, 614, 615, 615, 644, 669, 689, 696, 682, 670, 646, 637, 635, 670, 682, 685, 670, 654, 629, 576, 650, 661, 675, 660, 663, 618, 572, 575, 606, 638, 638, 643, 593, 538, 469, 570, 591, 608, 646, 605, 576, 480, 455},
        {-55, -33, -21, -11, -17, -2, 14, -33, -26, 7, 8, 8, 19, 26, 31, 9, -17, 14, 24, 32, 39, 39, 38, 6, -22, 10, 29, 44, 46, 42, 33, 5, -29, -0, 25, 40, 40, 31, 18, 2, -29, -7, 9, 21, 25, 17, 4, -6, -35, -18, -9, -2, 4, 3, -16, -37, -56, -55, -33, -27, -43, -21, -37, -64}
    };
    public static int[,] passedPawnBonuses = {
        {0, 0, 0, 0, 0, 0, 0, 0, 60, 73, 62, 73, 63, 54, 34, 18, 29, 31, 19, 13, 18, -3, -18, -30, 18, 8, 19, 19, -2, 3, -18, -16, -4, -9, -21, -13, -22, -9, -20, -14, -8, -21, -29, -20, -25, -13, -24, -2, -16, -13, -18, -15, -9, -14, -4, -12, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 124, 103, 116, 96, 98, 93, 105, 119, 120, 115, 82, 64, 68, 77, 88, 104, 70, 70, 41, 37, 35, 41, 70, 65, 50, 42, 27, 18, 23, 29, 50, 54, 15, 25, 17, 10, 11, 13, 37, 18, 16, 24, 7, 1, -13, 4, 20, 13, 0, 0, 0, 0, 0, 0, 0, 0}
    };
    public static int[] isolatedPawnPenalty = {48, 30, 12, 3, -13, -35, -38, -7, 0};
    public static int doubledPawnPenalty = -26;
    public static int bishopPairBonusMG = 48;
    public static int bishopPairBonusEG = 53;

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
