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
        {0, 0, 0, 0, 0, 0, 0, 0, 60, 69, 61, 71, 63, 52, 35, 16, 52, 72, 97, 96, 101, 142, 128, 80, 37, 65, 60, 64, 87, 83, 78, 61, 25, 53, 55, 71, 67, 57, 63, 42, 24, 49, 49, 47, 66, 49, 79, 50, 23, 51, 43, 32, 53, 67, 85, 41, 0, 0, 0, 0, 0, 0, 0, 0},
        {133, 83, 133, 154, 150, 137, 76, 148, 181, 212, 232, 237, 224, 279, 188, 206, 187, 242, 271, 275, 300, 286, 261, 220, 205, 221, 249, 272, 260, 278, 239, 241, 194, 207, 227, 232, 244, 232, 229, 207, 172, 202, 217, 212, 229, 218, 222, 193, 155, 165, 187, 204, 208, 206, 183, 189, 106, 176, 143, 164, 176, 190, 182, 127},
        {225, 170, 157, 161, 154, 178, 130, 186, 233, 258, 244, 221, 253, 253, 249, 232, 239, 261, 272, 290, 263, 302, 269, 275, 235, 248, 270, 281, 278, 272, 247, 237, 222, 241, 250, 270, 267, 254, 247, 234, 236, 249, 242, 257, 256, 244, 246, 246, 239, 244, 255, 231, 242, 249, 261, 243, 205, 235, 224, 204, 214, 220, 218, 217},
        {270, 240, 248, 249, 268, 219, 202, 265, 293, 290, 303, 323, 310, 327, 297, 318, 267, 284, 292, 299, 319, 318, 313, 310, 256, 262, 267, 278, 293, 284, 278, 289, 234, 237, 242, 259, 263, 253, 267, 261, 235, 237, 249, 247, 251, 257, 291, 269, 230, 239, 256, 252, 255, 262, 281, 248, 255, 256, 266, 272, 274, 271, 273, 258},
        {432, 412, 432, 448, 451, 432, 446, 457, 488, 468, 482, 471, 485, 511, 493, 524, 490, 479, 495, 501, 504, 543, 543, 533, 475, 472, 478, 484, 491, 498, 487, 497, 467, 470, 460, 476, 478, 474, 488, 487, 467, 466, 463, 460, 465, 474, 488, 487, 464, 468, 476, 474, 473, 482, 497, 502, 456, 446, 451, 470, 461, 446, 471, 451},
        {-16, -20, -15, -18, -12, 1, 9, 2, -19, -2, -20, -4, -8, -2, 21, 15, -21, 15, -20, -27, -17, 11, 13, -1, -12, -19, -26, -69, -61, -34, -27, -32, -17, -17, -53, -76, -81, -53, -50, -53, 4, 9, -28, -38, -30, -39, 5, -9, 76, 45, 26, 1, -1, 21, 66, 73, 72, 108, 81, -18, 45, 3, 90, 86}
    };

    public static int[,] eg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 125, 105, 116, 96, 99, 94, 104, 120, 107, 110, 97, 93, 81, 78, 108, 102, 97, 87, 88, 76, 76, 78, 78, 83, 83, 79, 80, 81, 82, 79, 70, 66, 85, 76, 83, 88, 90, 86, 71, 70, 95, 85, 93, 89, 105, 90, 75, 78, 0, 0, 0, 0, 0, 0, 0, 0},
        {146, 186, 215, 204, 215, 186, 175, 127, 198, 205, 221, 226, 215, 200, 200, 179, 211, 223, 234, 238, 220, 229, 208, 195, 201, 233, 249, 250, 249, 241, 234, 204, 217, 232, 249, 249, 247, 241, 222, 204, 201, 218, 232, 245, 241, 225, 215, 196, 191, 208, 216, 219, 217, 210, 193, 199, 154, 176, 197, 201, 195, 182, 182, 162},
        {224, 243, 245, 250, 246, 234, 236, 218, 218, 230, 242, 241, 235, 237, 232, 211, 244, 239, 249, 240, 241, 247, 240, 231, 240, 252, 253, 261, 256, 249, 250, 237, 231, 257, 262, 256, 257, 255, 244, 221, 233, 239, 254, 254, 253, 249, 234, 225, 231, 233, 225, 239, 247, 225, 235, 203, 216, 226, 209, 229, 217, 229, 204, 184},
        {431, 442, 452, 449, 437, 438, 437, 422, 431, 434, 446, 439, 434, 424, 425, 414, 428, 434, 430, 433, 419, 411, 417, 402, 433, 430, 435, 429, 419, 416, 415, 407, 424, 427, 432, 426, 420, 415, 399, 396, 417, 412, 418, 421, 411, 402, 374, 379, 414, 415, 412, 415, 400, 397, 382, 385, 409, 416, 419, 421, 415, 410, 405, 408},
        {685, 697, 704, 695, 696, 667, 633, 656, 637, 665, 676, 682, 685, 662, 617, 615, 619, 635, 666, 676, 681, 668, 620, 621, 622, 652, 666, 691, 702, 685, 673, 650, 642, 640, 677, 685, 689, 674, 660, 629, 578, 655, 669, 682, 671, 668, 625, 576, 582, 615, 644, 645, 652, 598, 539, 468, 575, 599, 613, 657, 614, 578, 480, 456},
        {-56, -30, -22, -11, -16, -3, 13, -32, -26, 8, 10, 7, 20, 22, 28, 13, -13, 12, 24, 30, 39, 39, 39, 4, -21, 10, 31, 43, 47, 43, 31, 2, -26, 2, 23, 42, 42, 31, 19, -2, -28, -8, 11, 24, 21, 17, 0, -8, -34, -15, -9, -1, 5, 4, -13, -34, -57, -52, -34, -26, -43, -20, -38, -61}
    };
    public static int[,] passedPawnBonuses = {
        {0, 0, 0, 0, 0, 0, 0, 0, 60, 69, 61, 71, 63, 52, 35, 16, 28, 28, 19, 21, 17, 0, -18, -34, 14, 8, 16, 21, -2, 9, -20, -15, -3, -10, -21, -15, -21, -8, -18, -17, -8, -24, -28, -20, -25, -13, -24, -3, -17, -13, -21, -17, -10, -14, -5, -4, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 125, 105, 116, 96, 99, 94, 104, 120, 120, 117, 85, 65, 66, 72, 88, 105, 68, 70, 41, 32, 37, 38, 69, 71, 55, 45, 29, 20, 21, 30, 49, 55, 18, 31, 17, 7, 10, 9, 37, 16, 18, 22, 10, 2, -13, 1, 25, 19, 0, 0, 0, 0, 0, 0, 0, 0}
    };
    public static int[] isolatedPawnPenalty = {39, 18, 6, -9, -22, -41, -41, -6, 0};
    public static int doubledPawnPenalty = -29;

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

        int pawnMGEval = 0;
        int pawnEGEval = 0;

        ulong whitePawns = board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Pawn)];
        ulong blackPawns = board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Pawn)];

        while (whitePawns != 0)
        {
            int index = BitboardHelper.PopLSB(ref whitePawns);
            (int, int) pawnBonus = EvaluatePawnStrength(board, index, Piece.White);
            pawnMGEval += pawnBonus.Item1;
            pawnEGEval += pawnBonus.Item2;
        }

        while (blackPawns != 0)
        {
            int index = BitboardHelper.PopLSB(ref blackPawns);
            (int, int) pawnBonus = EvaluatePawnStrength(board, index, Piece.Black);
            pawnMGEval -= pawnBonus.Item1;
            pawnEGEval -= pawnBonus.Item2;
        }

        pawnEGEval += isolatedPawnPenalty[numWhiteIsolatedPawns];
        pawnEGEval -= isolatedPawnPenalty[numBlackIsolatedPawns];

        int phase = (4 * (board.pieceCounts[Board.WhiteIndex, Piece.Queen] + board.pieceCounts[Board.BlackIndex, Piece.Queen])) + (2 * (board.pieceCounts[Board.WhiteIndex, Piece.Rook] + board.pieceCounts[Board.BlackIndex, Piece.Rook]));
        phase += board.pieceCounts[Board.WhiteIndex, Piece.Knight] + board.pieceCounts[Board.BlackIndex, Piece.Knight] + board.pieceCounts[Board.WhiteIndex, Piece.Bishop] + board.pieceCounts[Board.BlackIndex, Piece.Bishop];

        int egScore = egMaterialCount + pawnEGEval;
        int mgScore = mgMaterialCount + pawnMGEval;
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

    (int, int) EvaluatePawnStrength(Board board, int pawnIndex, int pawnColor)
    {
        int mgBonus = 0;
        int egBonus = 0;
        if (pawnColor == Piece.White)
        {
            //Passed pawn
            if ((board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Pawn)] & BitboardHelper.wPawnPassedMask[pawnIndex]) == 0) { 
                mgBonus += passedPawnBonuses[0, pawnIndex]; 
                egBonus += passedPawnBonuses[1, pawnIndex]; 
            }
            //Doubled pawn penalty
            if (board.PieceAt(pawnIndex - 8) == Piece.Pawn && board.ColorAt(pawnIndex - 8) == Piece.White) { egBonus += doubledPawnPenalty; }
            if ((BitboardHelper.isolatedPawnMask[pawnIndex] & board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Pawn)]) == 0) { numWhiteIsolatedPawns++; }
        }
        else
        {
            //Passed pawn
            if ((board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Pawn)] & BitboardHelper.bPawnPassedMask[pawnIndex]) == 0) { 
                mgBonus += passedPawnBonuses[0, pawnIndex ^ 56]; 
                egBonus += passedPawnBonuses[1, pawnIndex ^ 56]; 
            }
            //Doubled pawn penalty
            if (board.PieceAt(pawnIndex + 8) == Piece.Pawn && board.ColorAt(pawnIndex + 8) == Piece.Black) { egBonus += doubledPawnPenalty; }
            if((BitboardHelper.isolatedPawnMask[pawnIndex] & board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Pawn)]) == 0){ numBlackIsolatedPawns++; }
        }

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
