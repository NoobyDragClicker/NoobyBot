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
    (67, 137), (79, 115), (59, 137), (70, 118), (59, 119), (53, 122), (43, 130), (22, 139),
    (74, 125), (76, 131), (110, 111), (108, 110), (120, 104), (158, 103), (136, 128), (104, 122),
    (56, 118), (65, 113), (69, 107), (76, 93), (96, 96), (89, 97), (80, 102), (76, 100),
    (42, 105), (53, 108), (59, 100), (76, 99), (76, 99), (75, 95), (62, 95), (56, 86),
    (38, 103), (41, 101), (50, 102), (54, 105), (66, 108), (65, 102), (73, 88), (60, 85),
    (42, 111), (51, 109), (46, 111), (52, 114), (56, 127), (86, 108), (80, 96), (56, 90),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)
    },
    {
    (151, 236), (158, 283), (202, 305), (238, 287), (251, 301), (204, 277), (172, 280), (187, 212),
    (231, 288), (265, 297), (282, 312), (299, 311), (276, 301), (331, 292), (250, 296), (274, 266),
    (241, 304), (287, 313), (320, 325), (321, 326), (354, 311), (346, 314), (315, 301), (276, 286),
    (246, 302), (266, 327), (291, 343), (320, 342), (299, 343), (323, 337), (278, 326), (286, 298),
    (231, 313), (248, 325), (273, 343), (277, 342), (288, 345), (278, 334), (273, 320), (247, 294),
    (211, 293), (240, 310), (257, 322), (264, 338), (280, 332), (267, 313), (265, 303), (232, 294),
    (197, 291), (211, 305), (229, 314), (250, 310), (249, 310), (253, 306), (233, 292), (233, 291),
    (154, 269), (208, 281), (197, 298), (220, 302), (225, 297), (235, 285), (215, 292), (182, 284)
    },
    {
    (207, 243), (169, 249), (156, 246), (127, 255), (155, 251), (155, 237), (158, 243), (180, 235),
    (203, 232), (214, 223), (194, 225), (188, 225), (217, 212), (196, 220), (210, 222), (185, 236),
    (202, 248), (215, 226), (213, 210), (223, 197), (205, 203), (250, 205), (222, 226), (230, 241),
    (198, 243), (196, 229), (205, 211), (223, 203), (217, 195), (207, 210), (198, 223), (203, 241),
    (193, 238), (188, 229), (184, 216), (210, 202), (206, 200), (184, 209), (190, 222), (207, 230),
    (198, 241), (202, 227), (198, 219), (187, 219), (193, 222), (200, 217), (206, 222), (218, 232),
    (205, 253), (207, 223), (208, 215), (187, 226), (195, 226), (209, 217), (230, 223), (209, 229),
    (198, 237), (214, 248), (195, 247), (186, 238), (195, 234), (193, 251), (206, 232), (218, 213)
    },
    {
    (316, 456), (294, 460), (294, 468), (284, 461), (303, 454), (293, 464), (296, 466), (339, 451),
    (315, 464), (313, 472), (324, 475), (338, 462), (326, 464), (349, 455), (339, 459), (362, 448),
    (299, 467), (319, 463), (316, 463), (311, 458), (334, 451), (340, 448), (368, 447), (344, 447),
    (296, 469), (307, 459), (303, 465), (308, 457), (315, 445), (318, 449), (316, 453), (322, 454),
    (284, 462), (286, 457), (287, 457), (295, 451), (305, 446), (291, 447), (306, 442), (301, 449),
    (286, 454), (283, 448), (289, 444), (290, 447), (304, 437), (305, 429), (329, 415), (315, 427),
    (288, 447), (290, 444), (299, 443), (300, 443), (307, 434), (318, 426), (325, 418), (303, 432),
    (307, 458), (307, 444), (308, 448), (317, 441), (325, 434), (321, 446), (323, 428), (309, 444)
    },
    {
    (482, 1049), (499, 1054), (526, 1069), (554, 1060), (556, 1062), (587, 1021), (585, 997), (526, 1035),
    (520, 1041), (502, 1075), (517, 1093), (515, 1102), (529, 1113), (548, 1087), (539, 1063), (565, 1049),
    (517, 1040), (521, 1055), (529, 1091), (537, 1103), (539, 1110), (588, 1089), (582, 1044), (569, 1052),
    (502, 1048), (503, 1075), (518, 1087), (516, 1112), (522, 1123), (528, 1107), (526, 1101), (528, 1074),
    (491, 1058), (502, 1069), (497, 1089), (508, 1110), (513, 1103), (508, 1096), (516, 1076), (516, 1061),
    (493, 1023), (497, 1057), (499, 1078), (499, 1079), (505, 1076), (506, 1076), (520, 1043), (516, 1025),
    (488, 1025), (501, 1028), (507, 1036), (511, 1041), (506, 1050), (521, 1014), (527, 976), (533, 946),
    (475, 1017), (479, 1029), (487, 1040), (496, 1065), (494, 1038), (481, 1011), (507, 975), (498, 971)
    },
    {
    (-77, -59), (-77, -21), (-48, -11), (-98, 12), (-70, -1), (-11, 7), (38, 3), (103, -80),
    (-88, -7), (-27, 19), (-76, 26), (3, 11), (-34, 27), (-29, 37), (43, 33), (18, 15),
    (-90, -2), (22, 14), (-62, 31), (-74, 40), (-51, 44), (16, 40), (4, 37), (-28, 8),
    (-43, -20), (-52, 8), (-71, 28), (-147, 46), (-129, 47), (-91, 44), (-74, 31), (-106, 12),
    (-47, -28), (-47, -4), (-95, 22), (-134, 39), (-131, 42), (-88, 28), (-80, 14), (-112, 4),
    (-23, -29), (-4, -11), (-48, 6), (-69, 21), (-64, 21), (-64, 16), (-24, 0), (-46, -6),
    (51, -28), (22, -14), (4, -5), (-27, 2), (-33, 9), (-10, 4), (21, -6), (29, -25),
    (31, -55), (66, -47), (40, -25), (-40, -13), (15, -25), (-30, -8), (47, -36), (43, -67)
    }
    };
    public static EvalPair[] passedPawnBonuses = {(0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (67, 137), (79, 115), (59, 137), (70, 118), (59, 119), (53, 122), (43, 130), (22, 139), (17, 104), (28, 107), (8, 88), (-3, 62), (-13, 67), (-18, 78), (-43, 86), (-54, 105), (10, 41), (10, 38), (21, 12), (13, 12), (-5, 10), (3, 16), (-25, 39), (-23, 42), (-6, 8), (-8, -4), (-18, -15), (-2, -27), (-14, -25), (-11, -15), (-21, 3), (-11, 5), (-6, -36), (-13, -32), (-13, -42), (-0, -57), (-5, -59), (-11, -53), (-16, -29), (10, -46), (-12, -39), (-2, -50), (1, -60), (3, -75), (16, -87), (-0, -74), (13, -68), (4, -57), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)};
    public static EvalPair[] isolatedPawnPenalty = {(8, 11), (0, 5), (-3, -3), (-13, -6), (-16, -15), (-8, -37), (-19, -45), (33, -69), (49, 49)};
    public static EvalPair doubledPawnPenalty = (0, -32);
    public static EvalPair protectedPawn = (11, 8);
    public static EvalPair isolatedExposed = (-10, -5);
    public static EvalPair[] friendlyKingDistPasser = {(0, 0), (-11, 53), (-16, 40), (-10, 21), (-2, 11), (6, 6), (25, 2), (12, 3)};
    public static EvalPair[] enemyKingDistPasser = {(0, 0), (-59, -34), (24, 1), (10, 25), (6, 34), (-1, 41), (-5, 44), (-28, 47)};
    public static EvalPair bishopPairBonus = (41, 51);
    public static EvalPair bishopMobility = (9, 14);
    public static EvalPair rookOpenFile = (40, -7);
    public static EvalPair rookSemiOpenFile = (12, 15);
    public static EvalPair rookMobility = (0, 13);
    public static EvalPair rookKingRingAttack = (13, -6);
    public static EvalPair kingOpenFile = (-38, 4);
    public static EvalPair kingPawnShield = (19, -10);
    public static EvalPair tempo = (21, 17);
    


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
        Bitboard rookAttacks = simpleRookMoves & BitboardHelper.kingRing[1 - colorIndex, board.GetPieces(1 - colorIndex, Piece.King).GetLSB()];
        int numMoves = simpleRookMoves.PopCount();
        int numAttacks = rookAttacks.PopCount();

        score.mg += numMoves * rookMobility.mg + numAttacks * rookKingRingAttack.mg;
        score.eg += numMoves * rookMobility.eg + numAttacks * rookKingRingAttack.eg;
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