using System.Runtime.CompilerServices;
public class Evaluation
{

    int colorTurn;

    //Unused in actual eval
    public static int pawnValue = 90;
    public static int knightValue = 336;
    public static int bishopValue = 366;
    public static int rookValue = 538;
    public static int queenValue = 1024;
    
    public static EvalPair[,] PSQT = {
    {
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)
    },
    {
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0),
    (66, 139), (78, 116), (58, 138), (69, 119), (58, 121), (52, 123), (42, 131), (21, 141),
    (72, 127), (75, 132), (110, 113), (108, 111), (119, 105), (158, 104), (135, 129), (104, 122),
    (55, 119), (66, 114), (69, 108), (76, 94), (96, 97), (89, 98), (81, 103), (76, 100),
    (41, 107), (53, 109), (59, 101), (76, 100), (76, 100), (76, 95), (62, 96), (56, 87),
    (37, 104), (42, 101), (50, 103), (54, 106), (66, 108), (66, 102), (74, 89), (60, 86),
    (41, 112), (51, 110), (47, 111), (52, 114), (57, 127), (87, 108), (81, 97), (56, 91),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)
    },
    {
    (153, 241), (162, 286), (210, 308), (246, 289), (256, 303), (208, 280), (174, 281), (189, 216),
    (233, 291), (267, 301), (284, 316), (302, 315), (278, 305), (333, 296), (251, 298), (276, 268),
    (242, 306), (288, 316), (321, 329), (323, 331), (356, 316), (347, 318), (316, 304), (277, 288),
    (247, 306), (266, 331), (291, 348), (320, 347), (299, 348), (323, 341), (278, 329), (287, 301),
    (231, 316), (248, 329), (273, 348), (276, 349), (288, 351), (278, 338), (273, 323), (247, 297),
    (211, 296), (240, 314), (257, 326), (264, 344), (280, 337), (267, 316), (265, 307), (232, 298),
    (197, 295), (211, 309), (229, 318), (250, 315), (249, 315), (253, 311), (233, 296), (233, 295),
    (153, 273), (208, 286), (198, 303), (220, 308), (225, 302), (235, 289), (216, 296), (182, 288)
    },
    {
    (211, 246), (176, 252), (165, 249), (139, 258), (161, 253), (163, 240), (164, 246), (183, 237),
    (205, 235), (217, 226), (196, 230), (192, 229), (221, 216), (200, 224), (212, 225), (187, 239),
    (204, 251), (216, 231), (215, 215), (226, 202), (208, 207), (252, 209), (223, 230), (231, 244),
    (199, 246), (196, 234), (207, 217), (224, 209), (219, 200), (209, 214), (198, 227), (205, 243),
    (193, 242), (189, 234), (185, 220), (212, 209), (208, 205), (185, 213), (191, 226), (207, 233),
    (198, 245), (203, 232), (198, 225), (188, 225), (194, 227), (201, 221), (206, 226), (218, 235),
    (205, 257), (207, 226), (208, 220), (187, 231), (196, 230), (209, 222), (230, 226), (210, 232),
    (199, 241), (214, 252), (196, 252), (188, 243), (196, 239), (194, 256), (206, 237), (219, 216)
    },
    {
    (309, 446), (287, 451), (287, 459), (275, 454), (295, 446), (288, 452), (289, 455), (334, 439),
    (316, 443), (317, 447), (324, 450), (339, 437), (329, 438), (345, 435), (342, 439), (362, 431),
    (302, 445), (322, 438), (315, 437), (310, 430), (330, 426), (341, 422), (366, 426), (346, 428),
    (301, 448), (310, 437), (305, 443), (307, 436), (315, 422), (320, 425), (318, 431), (325, 435),
    (286, 449), (287, 444), (285, 444), (293, 439), (304, 433), (290, 432), (306, 427), (302, 435),
    (286, 446), (281, 439), (287, 436), (288, 440), (303, 430), (303, 420), (328, 404), (316, 417),
    (287, 442), (289, 438), (298, 439), (299, 438), (306, 428), (316, 421), (323, 413), (302, 427),
    (306, 454), (306, 440), (307, 444), (316, 436), (324, 430), (320, 443), (321, 424), (307, 440)
    },
    {
    (494, 1045), (510, 1050), (539, 1064), (566, 1055), (565, 1059), (593, 1020), (594, 994), (539, 1030),
    (522, 1042), (507, 1074), (521, 1093), (520, 1102), (534, 1112), (551, 1088), (543, 1063), (567, 1050),
    (518, 1041), (524, 1056), (532, 1091), (541, 1103), (541, 1111), (590, 1090), (585, 1044), (570, 1053),
    (503, 1050), (505, 1077), (520, 1088), (518, 1113), (524, 1124), (530, 1108), (527, 1102), (530, 1075),
    (492, 1059), (504, 1071), (498, 1091), (509, 1112), (515, 1105), (510, 1098), (517, 1078), (517, 1063),
    (494, 1025), (498, 1059), (500, 1081), (500, 1081), (506, 1079), (506, 1079), (520, 1046), (517, 1028),
    (488, 1027), (501, 1030), (508, 1038), (512, 1044), (506, 1053), (521, 1017), (528, 979), (533, 948),
    (475, 1019), (480, 1031), (488, 1043), (497, 1069), (495, 1040), (481, 1014), (509, 977), (498, 973)
    },
    {
    (-75, -60), (-72, -22), (-45, -12), (-95, 11), (-68, -1), (-10, 7), (38, 3), (103, -81),
    (-88, -7), (-25, 19), (-75, 27), (5, 11), (-32, 27), (-28, 37), (42, 33), (16, 15),
    (-89, -2), (24, 14), (-60, 31), (-71, 40), (-51, 45), (18, 40), (5, 38), (-28, 8),
    (-42, -20), (-50, 8), (-70, 28), (-145, 46), (-128, 48), (-90, 44), (-72, 31), (-105, 12),
    (-47, -28), (-45, -4), (-94, 22), (-132, 40), (-131, 42), (-87, 28), (-80, 15), (-112, 4),
    (-23, -28), (-4, -10), (-48, 6), (-69, 21), (-64, 21), (-64, 16), (-25, 1), (-47, -6),
    (50, -28), (21, -14), (3, -5), (-28, 2), (-34, 9), (-11, 4), (20, -6), (29, -25),
    (31, -56), (66, -47), (40, -26), (-39, -13), (15, -25), (-29, -8), (47, -37), (42, -68)
    }
    };
    public static EvalPair[] passedPawnBonuses = {(0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (66, 139), (78, 116), (58, 138), (69, 119), (58, 121), (52, 123), (42, 131), (21, 141), (18, 104), (28, 107), (8, 88), (-3, 63), (-12, 67), (-20, 79), (-43, 86), (-54, 106), (10, 41), (10, 38), (21, 13), (12, 12), (-5, 10), (2, 16), (-25, 39), (-24, 42), (-5, 7), (-8, -4), (-18, -15), (-2, -28), (-14, -26), (-12, -14), (-21, 4), (-11, 5), (-5, -37), (-13, -32), (-13, -42), (0, -57), (-5, -59), (-12, -53), (-17, -29), (10, -46), (-11, -39), (-2, -50), (1, -60), (4, -75), (16, -87), (-1, -74), (13, -67), (4, -57), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)};
    public static EvalPair[] isolatedPawnPenalty = {(7, 11), (0, 5), (-3, -3), (-12, -6), (-15, -15), (-7, -36), (-18, -44), (34, -69), (49, 49)};
    public static EvalPair doubledPawnPenalty = (0, -32);
    public static EvalPair protectedPawn = (11, 8);
    public static EvalPair isolatedExposed = (-11, -4);
    public static EvalPair[] friendlyKingDistPasser = {(0, 0), (-11, 54), (-16, 40), (-10, 22), (-2, 11), (6, 7), (25, 3), (11, 4)};
    public static EvalPair[] enemyKingDistPasser = {(0, 0), (-57, -35), (25, 1), (10, 25), (6, 35), (-2, 42), (-6, 45), (-29, 47)};
    public static EvalPair bishopPairBonus = (41, 51);
    public static EvalPair bishopMobility = (9, 14);
    public static EvalPair rookOpenFile = (36, -9);
    public static EvalPair rookSemiOpenFile = (19, -6);
    public static EvalPair rookMobility = (0, 13);
    public static EvalPair rookKingRingAttack = (13, -6);
    public static EvalPair[] rookThreats = {(0, 0), (-14, 28), (3, 28), (11, 27), (0, 0), (85, -27), (0, 0)};
    public static EvalPair kingOpenFile = (-38, 3);
    public static EvalPair kingPawnShield = (19, -11);
    public static EvalPair tempo = (22, 18);
    


    int playerTurnMultiplier;
    public int EvaluatePosition(Board board)
    {
        colorTurn = board.colorTurn;
        playerTurnMultiplier = (colorTurn == Piece.White) ? 1 : -1;
        int boardVal = IncrementalCount(board);
        return boardVal;
    }

    int IncrementalCount(Board board)
    {
        if (IsTrivialDraw(board)){ return 0; }
        const int totalPhase = 24;
        EvalPair eval = board.gameStateHistory[board.fullMoveClock].PSQTVal;

        Bitboard whiteBishops = board.GetPieces(Board.WhiteIndex, Piece.Bishop);
        Bitboard blackBishops = board.GetPieces(Board.BlackIndex, Piece.Bishop);

        Bitboard whiteRooks = board.GetPieces(Board.WhiteIndex, Piece.Rook);
        Bitboard blackRooks = board.GetPieces(Board.BlackIndex, Piece.Rook);

        while (!whiteRooks.Empty())
        {
            int index = whiteRooks.PopLSB();
            eval += EvaluateRookMobility(board, index, Board.WhiteIndex);
        }

        while (!blackRooks.Empty())
        {
            int index = blackRooks.PopLSB();
            eval -= EvaluateRookMobility(board, index, Board.BlackIndex);
        }

        while (!whiteBishops.Empty())
        {
            int index = whiteBishops.PopLSB();
            eval += EvaluateBishopMobility(board, index);
        }
        while (!blackBishops.Empty())
        {
            int index = blackBishops.PopLSB();
            eval -= EvaluateBishopMobility(board, index);
        }

        int whiteKingIndex = board.GetPieces(Board.WhiteIndex, Piece.King).GetLSB();
        int blackKingIndex = board.GetPieces(Board.BlackIndex, Piece.King).GetLSB();
        eval += EvaluateKingSafety(board, whiteKingIndex, Piece.White);
        eval -= EvaluateKingSafety(board, blackKingIndex, Piece.Black);

        eval += EvaluatePawnStructure(board, Board.WhiteIndex);
        eval -= EvaluatePawnStructure(board, Board.BlackIndex);


        if(board.pieceCounts[Board.WhiteIndex, Piece.Bishop] >= 2){ eval += bishopPairBonus; }
        if(board.pieceCounts[Board.BlackIndex, Piece.Bishop] >= 2){ eval -= bishopPairBonus; }

        int phase = (4 * (board.pieceCounts[Board.WhiteIndex, Piece.Queen] + board.pieceCounts[Board.BlackIndex, Piece.Queen])) + (2 * (board.pieceCounts[Board.WhiteIndex, Piece.Rook] + board.pieceCounts[Board.BlackIndex, Piece.Rook]));
        phase += board.pieceCounts[Board.WhiteIndex, Piece.Knight] + board.pieceCounts[Board.BlackIndex, Piece.Knight] + board.pieceCounts[Board.WhiteIndex, Piece.Bishop] + board.pieceCounts[Board.BlackIndex, Piece.Bishop];

        if (board.colorTurn == Piece.White)
        {
            eval += tempo;
        }
        else
        {
            eval -= tempo;
        }
        if (phase > 24) { phase = 24; }
        return (eval.mg * phase + eval.eg * (totalPhase - phase)) / totalPhase * playerTurnMultiplier;
    }

    bool IsTrivialDraw(Board board)
    {
        if (board.pieceCounts[Board.WhiteIndex, Piece.Pawn] + board.pieceCounts[Board.BlackIndex, Piece.Pawn] != 0) { return false; }
        if (board.pieceCounts[Board.WhiteIndex, Piece.Rook] + board.pieceCounts[Board.BlackIndex, Piece.Rook] + board.pieceCounts[Board.WhiteIndex, Piece.Queen] + board.pieceCounts[Board.BlackIndex, Piece.Queen] != 0) { return false; }

        int wBishops = board.pieceCounts[Board.WhiteIndex, Piece.Bishop];
        int wKnights = board.pieceCounts[Board.WhiteIndex, Piece.Knight];
        int wTotal = wBishops + wKnights;
        int bBishops = board.pieceCounts[Board.BlackIndex, Piece.Bishop];
        int bKnights = board.pieceCounts[Board.BlackIndex, Piece.Knight];
        int bTotal = bBishops + bKnights;

        //KB vs KB, KN vs KN, KB vs KN
        if(wTotal <= 1 && bTotal <= 1){ return true; }
        //KNN vs K
        else if ((bTotal == 0 && wKnights == 2 && wTotal == 2) || (wTotal == 0 && bKnights == 2 && bTotal == 2)) { return true; }
        return false;
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
            Bitboard stoppers = theirPawns & BitboardHelper.pawnPassedMask[currentColorIndex, index];
            bool passer = stoppers.Empty();
            int pushSquare = index + (currentColorIndex == Board.WhiteIndex ? -8 : 8);

            //Passed pawn
            if (passer) { 
                int psqtIndex = currentColorIndex == Board.WhiteIndex ? index : index ^ 56;
                score += passedPawnBonuses[psqtIndex]; 
                score += friendlyKingDistPasser[Coord.ChebyshevDist(ourKing, index)]; 
                score += enemyKingDistPasser[Coord.ChebyshevDist(theirKing, index)]; 
            }

            if (board.PieceAt(pushSquare) == Piece.Pawn && board.ColorAt(pushSquare) == currentColor) { score.eg += doubledPawnPenalty.eg; }
            if ((BitboardHelper.isolatedPawnMask[index] & ourPawns).Empty()) { 
                isolatedPawnCount++; 
                if ((stoppers & BitboardHelper.files[index%8]).Empty())
                {
                    score += isolatedExposed;
                }
            }
        }
        score += isolatedPawnPenalty[isolatedPawnCount];
        
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
        Bitboard threats = simpleRookMoves & board.sideBitboard[1 - colorIndex];
        Bitboard rookAttacks = simpleRookMoves & BitboardHelper.kingRing[1 - colorIndex, board.GetPieces(1 - colorIndex, Piece.King).GetLSB()];
        int numMoves = simpleRookMoves.PopCount();
        int numAttacks = rookAttacks.PopCount();

        score.mg += numMoves * rookMobility.mg + numAttacks * rookKingRingAttack.mg;
        score.eg += numMoves * rookMobility.eg + numAttacks * rookKingRingAttack.eg;

        while (!threats.Empty())
        {
            int index = threats.PopLSB();
            int pieceType = board.PieceAt(index);
            score += rookThreats[pieceType];
        }
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
    public static implicit operator EvalPair((int mg, int eg) value)=> new EvalPair(value.mg, value.eg);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EvalPair operator +(EvalPair a, EvalPair b)
    {
        return new EvalPair
        {
            mg = a.mg + b.mg,
            eg = a.eg + b.eg
        };
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EvalPair operator -(EvalPair a, EvalPair b)
    {
        return new EvalPair
        {
            mg = a.mg - b.mg,
            eg = a.eg - b.eg
        };
    }
}