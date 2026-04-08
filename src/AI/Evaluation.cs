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
    (66, 139), (79, 116), (58, 139), (69, 120), (58, 121), (53, 123), (42, 132), (21, 142),
    (73, 126), (76, 133), (110, 113), (108, 112), (118, 106), (157, 104), (135, 130), (103, 123),
    (55, 120), (65, 114), (68, 109), (75, 95), (95, 98), (88, 99), (79, 104), (75, 101),
    (41, 107), (52, 109), (58, 102), (76, 101), (75, 101), (76, 96), (62, 97), (56, 88),
    (36, 105), (42, 102), (50, 103), (53, 107), (66, 109), (66, 103), (74, 89), (59, 87),
    (41, 112), (51, 110), (47, 111), (52, 115), (56, 127), (87, 108), (81, 97), (56, 91),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)
    },
    {
    (155, 242), (163, 288), (213, 309), (249, 290), (259, 304), (215, 280), (175, 282), (190, 216),
    (234, 292), (269, 302), (285, 316), (304, 316), (279, 305), (335, 296), (253, 299), (277, 269),
    (244, 307), (289, 317), (324, 330), (326, 332), (359, 317), (352, 317), (316, 305), (279, 288),
    (248, 307), (268, 332), (296, 348), (325, 348), (306, 347), (328, 341), (279, 330), (288, 301),
    (232, 317), (251, 330), (276, 349), (282, 350), (291, 352), (281, 338), (276, 323), (248, 298),
    (213, 298), (242, 315), (259, 329), (266, 346), (282, 339), (269, 318), (267, 308), (235, 298),
    (199, 296), (213, 311), (230, 319), (251, 317), (250, 317), (254, 313), (235, 297), (233, 296),
    (154, 274), (208, 287), (199, 304), (220, 309), (225, 303), (236, 291), (216, 298), (183, 288)
    },
    {
    (211, 243), (176, 247), (163, 242), (136, 251), (160, 245), (163, 233), (164, 241), (183, 234),
    (199, 233), (190, 227), (185, 224), (181, 222), (193, 214), (193, 217), (188, 224), (185, 235),
    (199, 249), (208, 227), (197, 210), (211, 198), (199, 201), (238, 204), (219, 224), (225, 241),
    (192, 244), (184, 230), (197, 211), (218, 199), (208, 192), (201, 207), (184, 225), (203, 237),
    (188, 238), (181, 231), (188, 207), (207, 197), (206, 192), (186, 203), (186, 221), (197, 232),
    (196, 240), (206, 222), (199, 215), (191, 212), (197, 216), (201, 211), (207, 219), (216, 231),
    (209, 249), (208, 219), (209, 212), (189, 224), (198, 223), (210, 215), (231, 220), (212, 226),
    (201, 234), (216, 246), (197, 249), (189, 240), (196, 236), (195, 252), (208, 232), (221, 211)
    },
    {
    (311, 447), (288, 452), (289, 460), (278, 455), (298, 447), (291, 453), (291, 456), (336, 440),
    (318, 444), (320, 448), (326, 451), (342, 437), (331, 438), (347, 436), (345, 440), (365, 432),
    (304, 446), (325, 439), (318, 438), (313, 430), (333, 427), (345, 423), (369, 427), (349, 428),
    (303, 450), (313, 438), (308, 443), (310, 437), (319, 423), (323, 426), (320, 431), (327, 436),
    (288, 450), (289, 445), (288, 445), (296, 440), (307, 434), (293, 433), (308, 428), (304, 436),
    (288, 447), (284, 440), (289, 437), (291, 441), (305, 431), (306, 421), (330, 405), (318, 418),
    (288, 443), (291, 439), (299, 440), (301, 439), (307, 430), (318, 422), (324, 414), (302, 428),
    (307, 455), (307, 441), (308, 445), (317, 437), (325, 431), (321, 444), (322, 425), (308, 442)
    },
    {
    (496, 1046), (513, 1051), (541, 1065), (569, 1055), (567, 1061), (596, 1020), (595, 995), (541, 1032),
    (524, 1043), (509, 1075), (524, 1094), (523, 1102), (537, 1113), (553, 1089), (546, 1064), (568, 1052),
    (520, 1043), (526, 1057), (535, 1092), (544, 1103), (544, 1111), (593, 1090), (587, 1046), (572, 1054),
    (504, 1052), (507, 1078), (523, 1088), (522, 1114), (528, 1124), (532, 1109), (530, 1103), (531, 1076),
    (493, 1061), (505, 1072), (501, 1092), (512, 1113), (518, 1106), (513, 1099), (519, 1078), (519, 1065),
    (495, 1026), (499, 1061), (502, 1083), (501, 1084), (508, 1081), (509, 1080), (521, 1047), (518, 1029),
    (489, 1029), (503, 1032), (509, 1040), (513, 1047), (508, 1055), (522, 1020), (529, 981), (534, 951),
    (476, 1022), (482, 1032), (489, 1045), (497, 1072), (495, 1043), (483, 1015), (509, 979), (499, 974)
    },
    {
    (-76, -59), (-72, -21), (-45, -11), (-95, 11), (-68, -1), (-11, 7), (38, 3), (104, -81),
    (-88, -7), (-24, 19), (-75, 27), (6, 11), (-32, 27), (-28, 37), (43, 33), (16, 15),
    (-89, -2), (24, 14), (-61, 31), (-71, 40), (-51, 45), (18, 41), (5, 38), (-28, 8),
    (-41, -20), (-50, 8), (-70, 28), (-145, 46), (-127, 48), (-89, 44), (-72, 31), (-105, 12),
    (-47, -28), (-45, -4), (-94, 22), (-132, 39), (-130, 42), (-87, 28), (-79, 15), (-112, 4),
    (-23, -28), (-4, -10), (-47, 6), (-68, 21), (-64, 21), (-63, 16), (-25, 1), (-47, -6),
    (50, -28), (21, -14), (3, -5), (-27, 2), (-34, 9), (-11, 4), (20, -6), (28, -25),
    (31, -56), (66, -47), (40, -26), (-39, -13), (14, -25), (-29, -9), (46, -37), (42, -68)
    }
    };
    public static EvalPair[] passedPawnBonuses = {(0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (66, 139), (79, 116), (58, 139), (69, 120), (58, 121), (53, 123), (42, 132), (21, 142), (17, 105), (28, 107), (8, 89), (-3, 63), (-12, 68), (-18, 79), (-44, 86), (-54, 106), (10, 41), (10, 38), (22, 12), (13, 12), (-5, 10), (3, 16), (-23, 39), (-23, 42), (-5, 7), (-8, -4), (-18, -16), (-2, -28), (-13, -26), (-12, -14), (-21, 3), (-11, 5), (-5, -38), (-13, -32), (-14, -42), (0, -57), (-6, -59), (-12, -53), (-17, -29), (10, -46), (-11, -39), (-3, -49), (1, -59), (4, -75), (16, -87), (-1, -73), (13, -67), (3, -56), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)};
    public static EvalPair[] isolatedPawnPenalty = {(7, 11), (-0, 5), (-3, -3), (-12, -6), (-15, -15), (-7, -36), (-17, -44), (34, -68), (49, 49)};
    public static EvalPair doubledPawnPenalty = (0, -32);
    public static EvalPair protectedPawn = (11, 8);
    public static EvalPair isolatedExposed = (-11, -4);
    public static EvalPair[] friendlyKingDistPasser = {(0, 0), (-11, 54), (-16, 40), (-10, 22), (-2, 11), (6, 7), (25, 3), (11, 4)};
    public static EvalPair[] enemyKingDistPasser = {(0, 0), (-57, -35), (25, 1), (10, 25), (6, 35), (-2, 42), (-6, 45), (-28, 47)};
    public static EvalPair bishopPairBonus = (40, 45);
    public static EvalPair bishopMobility = (9, 15);
    public static EvalPair[] bishopThreats = {(0, 0), (-9, 15), (19, 31), (0, 0), (48, 21), (77, 28), (0, 0)};
    public static EvalPair rookOpenFile = (37, -9);
    public static EvalPair rookSemiOpenFile = (20, -7);
    public static EvalPair rookMobility = (0, 13);
    public static EvalPair rookKingRingAttack = (13, -6);
    public static EvalPair[] rookThreats = {(0, 0), (-14, 28), (3, 28), (10, 26), (0, 0), (87, -27), (0, 0)};
    public static EvalPair kingOpenFile = (-38, 3);
    public static EvalPair kingPawnShield = (19, -10);
    public static EvalPair tempo = (24, 18);
    


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
            eval += EvaluateBishopMobility(board, index, Board.WhiteIndex);
        }
        while (!blackBishops.Empty())
        {
            int index = blackBishops.PopLSB();
            eval -= EvaluateBishopMobility(board, index, Board.BlackIndex);
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

    EvalPair EvaluateBishopMobility(Board board, int pieceIndex, int colorIndex)
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