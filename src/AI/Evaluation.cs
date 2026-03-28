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
    (68, 137), (80, 116), (60, 138), (70, 118), (59, 120), (55, 123), (44, 132), (22, 140),
    (74, 126), (76, 134), (111, 114), (109, 112), (121, 105), (159, 107), (137, 133), (105, 123),
    (56, 119), (66, 115), (70, 108), (77, 94), (97, 97), (90, 100), (81, 105), (77, 100),
    (42, 106), (53, 110), (59, 101), (76, 98), (76, 98), (76, 97), (63, 98), (57, 87),
    (38, 104), (42, 101), (50, 102), (54, 106), (66, 108), (65, 102), (74, 90), (60, 85),
    (42, 112), (51, 111), (46, 112), (52, 116), (56, 128), (86, 109), (80, 98), (56, 91),
    (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)
    },
    {
    (151, 236), (158, 283), (202, 305), (238, 287), (251, 301), (204, 277), (171, 280), (187, 212),
    (231, 288), (265, 297), (282, 311), (299, 311), (276, 301), (331, 291), (250, 295), (274, 265),
    (241, 304), (287, 313), (320, 325), (321, 326), (354, 311), (346, 314), (315, 301), (276, 285),
    (247, 302), (266, 327), (291, 343), (320, 342), (299, 343), (323, 337), (278, 326), (286, 298),
    (231, 313), (248, 325), (273, 343), (277, 342), (288, 346), (278, 334), (273, 320), (247, 294),
    (211, 294), (240, 310), (257, 323), (264, 339), (280, 333), (267, 313), (265, 304), (232, 295),
    (198, 291), (212, 306), (229, 314), (250, 310), (249, 311), (254, 306), (233, 293), (233, 292),
    (154, 270), (208, 281), (197, 299), (220, 303), (225, 297), (235, 285), (215, 292), (182, 284)
    },
    {
    (207, 243), (169, 250), (157, 246), (127, 256), (155, 251), (155, 237), (158, 244), (180, 235),
    (203, 232), (214, 223), (194, 225), (188, 225), (217, 212), (196, 220), (210, 222), (185, 236),
    (202, 248), (215, 226), (213, 210), (224, 197), (206, 203), (250, 205), (222, 226), (230, 241),
    (198, 243), (196, 229), (205, 212), (223, 203), (217, 195), (208, 210), (198, 224), (203, 241),
    (193, 238), (189, 230), (184, 216), (211, 203), (207, 200), (184, 210), (190, 223), (207, 230),
    (198, 242), (202, 227), (198, 220), (187, 219), (193, 222), (200, 217), (206, 222), (218, 233),
    (206, 253), (207, 224), (208, 216), (187, 226), (195, 226), (209, 218), (230, 224), (210, 230),
    (198, 237), (214, 248), (195, 247), (186, 238), (195, 235), (193, 251), (206, 233), (219, 214)
    },
    {
    (316, 455), (295, 460), (294, 468), (284, 461), (303, 454), (293, 464), (295, 466), (339, 451),
    (315, 464), (313, 471), (324, 475), (338, 462), (326, 463), (349, 455), (339, 459), (362, 448),
    (299, 467), (319, 463), (316, 463), (311, 458), (334, 451), (340, 447), (368, 447), (344, 447),
    (296, 468), (307, 459), (303, 465), (308, 457), (316, 445), (318, 448), (316, 453), (322, 454),
    (285, 461), (286, 457), (287, 456), (295, 450), (305, 446), (291, 446), (306, 442), (301, 448),
    (286, 454), (283, 447), (289, 443), (290, 446), (305, 437), (305, 429), (329, 415), (316, 426),
    (288, 447), (290, 444), (299, 443), (301, 443), (307, 434), (318, 426), (325, 418), (303, 431),
    (308, 458), (307, 444), (308, 448), (317, 441), (325, 434), (322, 446), (323, 429), (309, 444)
    },
    {
    (482, 1050), (500, 1054), (527, 1070), (555, 1060), (556, 1062), (587, 1021), (585, 997), (526, 1036),
    (520, 1040), (503, 1074), (517, 1093), (515, 1102), (528, 1113), (548, 1087), (539, 1063), (565, 1049),
    (517, 1040), (521, 1055), (529, 1091), (537, 1103), (539, 1110), (588, 1089), (582, 1044), (569, 1052),
    (502, 1048), (503, 1075), (518, 1087), (516, 1112), (522, 1123), (528, 1107), (526, 1101), (529, 1074),
    (491, 1058), (502, 1069), (497, 1089), (508, 1109), (513, 1103), (508, 1096), (516, 1076), (517, 1061),
    (493, 1024), (498, 1057), (499, 1078), (499, 1079), (505, 1077), (506, 1076), (520, 1044), (516, 1025),
    (488, 1025), (501, 1028), (507, 1036), (511, 1042), (506, 1050), (521, 1014), (527, 976), (533, 946),
    (476, 1017), (479, 1029), (487, 1041), (496, 1066), (494, 1038), (481, 1011), (508, 975), (498, 971)
    },
    {
    (-75, -59), (-75, -21), (-47, -11), (-98, 11), (-69, -1), (-10, 6), (39, 3), (102, -80),
    (-86, -7), (-26, 19), (-75, 26), (3, 11), (-33, 27), (-29, 37), (44, 32), (19, 15),
    (-89, -2), (22, 14), (-62, 31), (-73, 40), (-51, 44), (16, 40), (5, 37), (-28, 8),
    (-41, -20), (-51, 8), (-70, 28), (-146, 46), (-129, 47), (-90, 44), (-73, 31), (-106, 12),
    (-46, -28), (-45, -4), (-95, 22), (-133, 39), (-131, 42), (-88, 28), (-80, 14), (-112, 4),
    (-22, -29), (-4, -10), (-47, 6), (-69, 21), (-64, 21), (-64, 16), (-25, 0), (-46, -6),
    (51, -28), (22, -14), (4, -5), (-27, 2), (-33, 9), (-10, 5), (20, -6), (29, -25),
    (31, -55), (66, -47), (40, -26), (-40, -13), (14, -25), (-30, -8), (47, -36), (43, -67)
    }
    };
    public static EvalPair[] passedPawnBonuses = {(0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (68, 137), (80, 116), (60, 138), (70, 118), (59, 120), (55, 123), (44, 132), (22, 140), (17, 105), (29, 106), (8, 87), (-2, 62), (-13, 67), (-17, 77), (-43, 85), (-54, 105), (10, 41), (11, 37), (21, 12), (13, 13), (-4, 11), (4, 15), (-24, 39), (-23, 42), (-6, 8), (-8, -4), (-18, -15), (-2, -26), (-14, -24), (-11, -15), (-20, 2), (-11, 5), (-7, -37), (-13, -32), (-13, -42), (-0, -57), (-6, -59), (-12, -53), (-16, -30), (10, -46), (-12, -39), (-2, -50), (0, -59), (3, -75), (16, -86), (-1, -75), (12, -68), (3, -57), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)};
    public static EvalPair[] isolatedPawnPenalty = {(8, 10), (0, 4), (-3, -3), (-13, -4), (-16, -11), (-9, -31), (-19, -38), (34, -60), (49, 49)};
    public static EvalPair doubledPawnPenalty = (0, -23);
    public static EvalPair protectedPawn = (11, 8);
    public static EvalPair isolatedExposed = (-10, -5);
    public static EvalPair[] friendlyKingDistPasser = {(0, 0), (-10, 53), (-16, 40), (-10, 21), (-2, 11), (6, 6), (25, 2), (12, 3)};
    public static EvalPair[] enemyKingDistPasser = {(0, 0), (-57, -35), (25, 1), (10, 25), (6, 34), (-2, 41), (-5, 44), (-28, 47)};
    public static EvalPair bishopPairBonus = (41, 51);
    public static EvalPair bishopMobility = (9, 14);
    public static EvalPair rookOpenFile = (40, -7);
    public static EvalPair rookSemiOpenFile = (12, 16);
    public static EvalPair rookMobility = (0, 13);
    public static EvalPair rookKingRingAttack = (13, -6);
    public static EvalPair kingOpenFile = (-38, 4);
    public static EvalPair kingPawnShield = (19, -10);
    public static EvalPair tempo = (21, 16);
    


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

            //Passed pawn
            if (passer) { 
                int psqtIndex = currentColorIndex == Board.WhiteIndex ? index : index ^ 56;
                score += passedPawnBonuses[psqtIndex]; 
                score += friendlyKingDistPasser[Coord.ChebyshevDist(ourKing, index)]; 
                score += enemyKingDistPasser[Coord.ChebyshevDist(theirKing, index)]; 
            }

            if (!(ourPawns & BitboardHelper.pawnForwardFill[currentColorIndex, index]).Empty()) { score.eg += doubledPawnPenalty.eg; }
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