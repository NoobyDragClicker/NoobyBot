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
        {0, 0, 0, 0, 0, 0, 0, 0, 57, 70, 64, 75, 68, 56, 38, 11, 43, 65, 97, 81, 92, 135, 123, 67, 31, 53, 57, 56, 78, 70, 68, 49, 16, 45, 42, 59, 59, 45, 51, 31, 16, 39, 39, 40, 54, 38, 65, 41, 16, 42, 33, 29, 44, 55, 72, 32, 0, 0, 0, 0, 0, 0, 0, 0},
        {151, 143, 191, 237, 215, 198, 148, 188, 175, 207, 221, 248, 219, 270, 193, 209, 175, 218, 252, 253, 281, 272, 246, 204, 182, 194, 221, 247, 233, 251, 203, 210, 163, 178, 199, 205, 215, 202, 201, 178, 145, 172, 189, 186, 198, 195, 194, 165, 126, 138, 158, 177, 179, 177, 158, 163, 84, 147, 119, 137, 150, 161, 152, 98},
        {210, 175, 171, 194, 174, 182, 153, 180, 173, 197, 190, 205, 220, 196, 203, 175, 174, 200, 208, 219, 203, 246, 207, 196, 166, 170, 199, 206, 207, 206, 170, 168, 142, 166, 171, 192, 191, 172, 174, 153, 158, 167, 166, 174, 174, 165, 171, 172, 159, 164, 175, 153, 163, 173, 181, 163, 126, 158, 145, 130, 135, 141, 150, 134},
        {270, 253, 268, 265, 282, 276, 256, 292, 269, 271, 285, 303, 297, 315, 290, 308, 250, 264, 278, 276, 300, 306, 307, 295, 234, 238, 244, 253, 264, 264, 256, 267, 206, 215, 214, 229, 235, 220, 241, 237, 204, 205, 215, 210, 219, 220, 253, 236, 199, 209, 222, 217, 223, 231, 247, 216, 223, 220, 232, 236, 242, 234, 241, 223},
        {404, 427, 452, 467, 459, 457, 442, 428, 401, 388, 416, 433, 445, 440, 413, 432, 394, 403, 418, 431, 431, 470, 441, 431, 382, 378, 403, 404, 418, 417, 401, 401, 361, 380, 371, 389, 392, 380, 392, 386, 360, 367, 367, 366, 370, 375, 384, 377, 351, 360, 372, 373, 372, 377, 376, 368, 334, 336, 346, 367, 354, 336, 347, 325},
        {-85, -99, -74, -99, -84, -42, 5, 5, -71, -28, -80, -43, -59, -46, 30, 29, -70, 11, -68, -94, -80, -24, -11, -38, -49, -57, -70, -131, -120, -84, -71, -77, -48, -52, -95, -120, -122, -90, -76, -86, -18, -20, -56, -72, -62, -67, -26, -35, 50, 13, -2, -31, -35, -16, 28, 36, 36, 67, 45, -50, 11, -34, 51, 44}
    };
    public static int[,] eg_PSQT = {
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 117, 97, 106, 89, 88, 84, 98, 112, 97, 97, 85, 82, 77, 73, 97, 98, 92, 82, 81, 71, 73, 73, 76, 82, 80, 77, 77, 78, 76, 75, 69, 63, 81, 73, 79, 83, 85, 80, 66, 66, 89, 83, 88, 83, 100, 86, 71, 75, 0, 0, 0, 0, 0, 0, 0, 0},
        {136, 180, 198, 173, 194, 168, 176, 120, 180, 181, 201, 196, 191, 178, 183, 159, 195, 202, 209, 212, 194, 202, 189, 178, 179, 212, 228, 225, 224, 218, 209, 187, 202, 214, 225, 224, 224, 218, 201, 182, 187, 195, 210, 221, 214, 199, 190, 182, 188, 199, 196, 194, 196, 192, 178, 183, 158, 159, 184, 184, 172, 164, 169, 183},
        {206, 219, 221, 218, 220, 207, 215, 201, 200, 216, 222, 220, 211, 217, 214, 197, 223, 223, 230, 222, 224, 225, 221, 217, 222, 240, 236, 245, 239, 231, 238, 222, 222, 238, 245, 242, 242, 239, 228, 214, 222, 226, 237, 240, 242, 235, 221, 216, 225, 218, 213, 223, 229, 211, 222, 201, 206, 214, 198, 210, 200, 214, 200, 183},
        {370, 374, 381, 380, 371, 364, 367, 355, 372, 380, 386, 379, 375, 364, 364, 355, 374, 377, 376, 373, 363, 356, 356, 350, 375, 373, 383, 376, 362, 360, 357, 353, 370, 371, 377, 372, 365, 361, 344, 342, 365, 362, 361, 367, 356, 346, 320, 332, 359, 360, 357, 358, 348, 340, 327, 341, 364, 361, 366, 367, 358, 356, 350, 356},
        {545, 539, 539, 535, 537, 527, 522, 539, 514, 531, 535, 535, 538, 532, 514, 513, 508, 515, 536, 542, 541, 542, 521, 518, 506, 529, 535, 547, 549, 543, 538, 529, 531, 524, 539, 546, 543, 538, 528, 523, 499, 535, 536, 543, 535, 538, 516, 502, 502, 512, 532, 526, 532, 499, 481, 468, 494, 501, 506, 546, 503, 482, 461, 463},
        {-62, -32, -24, -6, -15, -3, 10, -53, -29, 6, 12, 7, 20, 25, 22, 2, -15, 8, 26, 39, 43, 41, 35, 4, -20, 12, 32, 48, 50, 46, 33, 7, -28, -0, 27, 43, 44, 32, 18, 4, -34, -8, 8, 22, 22, 16, 1, -8, -33, -17, -10, -1, 5, 3, -9, -29, -55, -50, -32, -26, -36, -18, -32, -56}
    };
    public static int[,] passedPawnBonuses = {
        {0, 0, 0, 0, 0, 0, 0, 0, 57, 70, 64, 75, 68, 56, 38, 11, 32, 30, 16, 24, 18, -3, -24, -30, 17, 8, 20, 22, 2, 6, -20, -14, 3, -8, -18, -8, -18, -4, -11, -10, -3, -19, -20, -15, -21, -12, -20, 3, -8, -7, -12, -12, -4, -11, -2, -7, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 117, 97, 106, 89, 88, 84, 98, 112, 113, 110, 78, 61, 61, 74, 83, 99, 70, 68, 37, 34, 34, 40, 63, 62, 52, 47, 26, 20, 22, 27, 46, 52, 17, 30, 18, 8, 13, 11, 39, 17, 20, 24, 11, 0, -13, 5, 25, 16, 0, 0, 0, 0, 0, 0, 0, 0}
    };
    public static int[] isolatedPawnPenalty = {41, 22, 8, -6, -19, -43, -42, -7, 0};
    public static int doubledPawnPenalty = -26;
    public static int bishopPairBonusMG = 68;
    public static int bishopPairBonusEG = 34;

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
