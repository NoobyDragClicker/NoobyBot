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
    (65, 139), (70, 118), (54, 140), (65, 121), (50, 124), (51, 124), (33, 134), (20, 142),
    (73, 127), (74, 133), (102, 115), (103, 112), (112, 108), (151, 106), (132, 131), (103, 123),
    (54, 120), (63, 115), (65, 110), (67, 99), (86, 102), (85, 100), (76, 105), (74, 101),
    (42, 107), (49, 111), (57, 102), (72, 103), (70, 104), (75, 96), (57, 98), (56, 88),
    (36, 106), (41, 102), (49, 104), (52, 107), (65, 109), (63, 104), (73, 90), (58, 87),
    (41, 113), (51, 110), (46, 111), (51, 114), (56, 127), (87, 108), (80, 97), (56, 91),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)
    },
    {
    (156, 241), (163, 287), (208, 308), (248, 288), (255, 303), (213, 278), (174, 281), (192, 215),
    (222, 294), (253, 306), (255, 321), (275, 320), (275, 304), (304, 301), (249, 299), (261, 272),
    (240, 307), (272, 320), (312, 331), (321, 329), (333, 321), (353, 314), (301, 309), (289, 285),
    (251, 305), (279, 327), (302, 343), (327, 345), (319, 339), (337, 335), (299, 321), (295, 298),
    (234, 316), (259, 327), (285, 343), (290, 344), (297, 348), (290, 333), (285, 320), (251, 297),
    (216, 297), (246, 313), (265, 326), (272, 343), (286, 337), (276, 316), (270, 307), (237, 298),
    (200, 295), (213, 311), (232, 318), (253, 316), (252, 317), (255, 313), (236, 298), (233, 297),
    (154, 275), (208, 288), (199, 304), (220, 310), (225, 304), (236, 291), (216, 299), (184, 289)
    },
    {
    (214, 244), (181, 248), (166, 243), (140, 252), (165, 246), (166, 235), (167, 242), (187, 235),
    (206, 234), (196, 228), (191, 226), (189, 223), (199, 215), (199, 218), (193, 225), (190, 236),
    (208, 249), (217, 229), (209, 211), (219, 200), (211, 202), (259, 203), (233, 224), (237, 241),
    (198, 245), (192, 232), (206, 213), (228, 200), (218, 194), (213, 209), (191, 228), (212, 238),
    (194, 240), (189, 233), (194, 210), (214, 200), (214, 194), (192, 207), (195, 223), (203, 233),
    (199, 242), (211, 223), (205, 217), (196, 215), (202, 219), (207, 213), (213, 221), (220, 232),
    (211, 250), (211, 221), (213, 214), (194, 226), (202, 225), (215, 217), (235, 221), (215, 227),
    (203, 235), (218, 248), (199, 251), (192, 241), (200, 237), (197, 254), (211, 233), (225, 211)
    },
    {
    (313, 447), (291, 453), (291, 461), (281, 455), (299, 447), (293, 454), (294, 456), (339, 440),
    (322, 444), (324, 448), (330, 451), (345, 438), (335, 439), (351, 436), (348, 440), (367, 432),
    (308, 446), (329, 439), (323, 438), (317, 430), (340, 426), (354, 422), (377, 426), (354, 428),
    (307, 450), (317, 438), (313, 444), (316, 436), (325, 423), (330, 425), (328, 431), (333, 436),
    (292, 451), (294, 445), (295, 444), (301, 440), (313, 434), (300, 432), (314, 428), (307, 436),
    (290, 447), (288, 441), (294, 438), (295, 441), (309, 432), (311, 421), (334, 405), (320, 419),
    (290, 444), (293, 440), (301, 441), (302, 440), (309, 431), (320, 423), (327, 414), (304, 429),
    (307, 456), (308, 441), (310, 445), (318, 438), (326, 431), (322, 445), (324, 426), (308, 443)
    },
    {
    (498, 1047), (516, 1051), (544, 1065), (572, 1056), (568, 1062), (599, 1021), (596, 997), (542, 1033),
    (527, 1045), (514, 1075), (528, 1095), (527, 1102), (540, 1114), (556, 1091), (550, 1065), (571, 1053),
    (523, 1044), (530, 1058), (538, 1093), (548, 1104), (547, 1113), (598, 1091), (593, 1045), (574, 1056),
    (508, 1052), (512, 1078), (527, 1089), (526, 1114), (533, 1124), (537, 1109), (534, 1104), (536, 1076),
    (496, 1061), (509, 1073), (504, 1093), (515, 1115), (521, 1107), (516, 1100), (523, 1078), (521, 1067),
    (497, 1027), (501, 1062), (505, 1084), (502, 1087), (510, 1083), (511, 1082), (524, 1049), (520, 1030),
    (490, 1030), (503, 1035), (509, 1043), (512, 1050), (507, 1058), (522, 1022), (530, 983), (535, 952),
    (476, 1025), (482, 1034), (489, 1046), (496, 1075), (496, 1045), (482, 1017), (508, 982), (500, 976)
    },
    {
    (-75, -59), (-71, -21), (-44, -11), (-92, 11), (-65, -1), (-10, 7), (39, 3), (105, -81),
    (-88, -7), (-23, 19), (-73, 27), (8, 11), (-30, 27), (-25, 37), (44, 33), (16, 15),
    (-87, -2), (25, 14), (-59, 31), (-69, 40), (-49, 44), (21, 40), (6, 38), (-26, 8),
    (-40, -20), (-49, 8), (-69, 28), (-143, 46), (-126, 48), (-87, 44), (-71, 31), (-104, 12),
    (-46, -28), (-45, -4), (-93, 22), (-130, 39), (-128, 42), (-85, 27), (-78, 15), (-110, 4),
    (-23, -28), (-3, -10), (-47, 7), (-67, 21), (-63, 21), (-63, 16), (-24, 1), (-47, -6),
    (50, -28), (21, -14), (3, -4), (-28, 2), (-35, 9), (-12, 5), (20, -6), (28, -25),
    (31, -56), (66, -47), (40, -25), (-39, -13), (13, -24), (-29, -8), (46, -37), (43, -68)
    }
    };
    public static EvalPair[] passedPawnBonuses = {(0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (65, 139), (70, 118), (54, 140), (65, 121), (50, 124), (51, 124), (33, 134), (20, 142), (16, 105), (27, 108), (8, 88), (-3, 64), (-11, 67), (-20, 79), (-44, 86), (-56, 106), (11, 40), (11, 38), (21, 12), (13, 11), (-2, 8), (2, 16), (-23, 39), (-23, 42), (-5, 7), (-7, -5), (-18, -15), (-3, -28), (-13, -27), (-13, -14), (-19, 3), (-13, 5), (-5, -38), (-13, -32), (-12, -43), (-0, -57), (-5, -59), (-10, -54), (-17, -29), (10, -46), (-10, -39), (-2, -49), (1, -60), (5, -75), (17, -86), (-0, -74), (13, -67), (3, -56), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)};
    public static EvalPair[] isolatedPawnPenalty = {(7, 11), (-0, 5), (-3, -3), (-12, -6), (-14, -15), (-5, -36), (-15, -45), (35, -69), (49, 49)};
    public static EvalPair doubledPawnPenalty = (0, -32);
    public static EvalPair protectedPawn = (12, 8);
    public static EvalPair isolatedExposed = (-11, -5);
    public static EvalPair[] pawnThreats = {(0, 0), (18, -4), (62, 14), (49, 41), (75, 8), (91, -46), (0, 0)};
    public static EvalPair[] friendlyKingDistPasser = {(0, 0), (-11, 54), (-16, 40), (-10, 22), (-3, 11), (5, 7), (24, 3), (11, 4)};
    public static EvalPair[] enemyKingDistPasser = {(0, 0), (-54, -36), (24, 1), (9, 26), (5, 35), (-3, 42), (-7, 45), (-29, 48)};
    public static EvalPair bishopPairBonus = (40, 45);
    public static EvalPair bishopMobility = (9, 14);
    public static EvalPair[] bishopThreats = {(0, 0), (-7, 16), (18, 30), (0, 0), (51, 21), (82, 26), (0, 0)};
    public static EvalPair[] knightThreats = {(0, 0), (-11, 8), (0, 0), (33, 23), (67, 12), (69, -88), (0, 0)};
    public static EvalPair rookOpenFile = (37, -10);
    public static EvalPair rookSemiOpenFile = (20, -7);
    public static EvalPair rookMobility = (0, 13);
    public static EvalPair rookKingRingAttack = (14, -6);
    public static EvalPair[] rookThreats = {(0, 0), (-14, 28), (0, 28), (9, 27), (0, 0), (91, -28), (0, 0)};
    public static EvalPair kingOpenFile = (-38, 3);
    public static EvalPair kingPawnShield = (19, -10);
    public static EvalPair tempo = (30, 18);
    


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

        Bitboard whiteKnights = board.GetPieces(Board.WhiteIndex, Piece.Knight);
        Bitboard blackKnights = board.GetPieces(Board.BlackIndex, Piece.Knight);

        Bitboard whiteRooks = board.GetPieces(Board.WhiteIndex, Piece.Rook);
        Bitboard blackRooks = board.GetPieces(Board.BlackIndex, Piece.Rook);

        while (!whiteRooks.Empty())
        {
            int index = whiteRooks.PopLSB();
            eval += EvaluateRook(board, index, Board.WhiteIndex);
        }

        while (!blackRooks.Empty())
        {
            int index = blackRooks.PopLSB();
            eval -= EvaluateRook(board, index, Board.BlackIndex);
        }

        while (!whiteBishops.Empty())
        {
            int index = whiteBishops.PopLSB();
            eval += EvaluateBishop(board, index, Board.WhiteIndex);
        }
        while (!blackBishops.Empty())
        {
            int index = blackBishops.PopLSB();
            eval -= EvaluateBishop(board, index, Board.BlackIndex);
        }

        while (!whiteKnights.Empty())
        {
            int index = whiteKnights.PopLSB();
            eval += EvaluateKnight(board, index, Board.WhiteIndex);
        }
        while (!blackKnights.Empty())
        {
            int index = blackKnights.PopLSB();
            eval -= EvaluateKnight(board, index, Board.BlackIndex);
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
        
        Bitboard ourAttacks = BitboardHelper.GetAllPawnAttacks(ourPawns, currentColor);
        Bitboard threats = ourAttacks & board.sideBitboard[oppositeColorIndex];
        while (!threats.Empty())
        {
            int index = threats.PopLSB();
            int pieceType = board.PieceAt(index);
            score += pawnThreats[pieceType];
        }
        int defended = (ourAttacks & ourPawns).PopCount();
        score.mg += defended * protectedPawn.mg;
        score.eg += defended * protectedPawn.eg;
        return score;
    }

    EvalPair EvaluateBishop(Board board, int pieceIndex, int colorIndex)
    {
        Bitboard simpleBishopMoves = BitboardHelper.GetBishopAttacks(pieceIndex, board.allPiecesBitboard);
        Bitboard threats = simpleBishopMoves & board.sideBitboard[1 - colorIndex];
        int numMoves = simpleBishopMoves.PopCount();
        EvalPair score = new EvalPair(numMoves * bishopMobility.mg, numMoves * bishopMobility.eg);
        while (!threats.Empty())
        {
            int index = threats.PopLSB();
            int pieceType = board.PieceAt(index);
            score += bishopThreats[pieceType];
        }
        return score;
    }

    EvalPair EvaluateRook(Board board, int pieceIndex, int colorIndex)
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

    EvalPair EvaluateKnight(Board board, int pieceIndex, int colorIndex)
    {
        EvalPair score = new EvalPair();
        Bitboard threats = BitboardHelper.knightAttacks[pieceIndex] & board.sideBitboard[1 - colorIndex];
        while (!threats.Empty())
        {
            int index = threats.PopLSB();
            int pieceType = board.PieceAt(index);
            score += knightThreats[pieceType];
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