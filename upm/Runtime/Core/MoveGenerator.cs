using System;
using System.Collections.Generic;
using System.Linq;


public static class MoveGenerator
{

    //Used for a player when clicking on a piece
    public static List<Move> GeneratePieceMove(int piece, int index, Board board)
    {
        Span<Move> legalMoves = new Move[256];
        GenerateLegalMoves(board, ref legalMoves, board.colorTurn);
        List<Move> correctMoves = new List<Move>();
        foreach (Move move in legalMoves)
        {
            if (move.oldIndex == index)
            {
                correctMoves.Add(move);
            }
        }
        
        return correctMoves;
    }

    //Returns all legal moves in a position
    public static int GenerateLegalMoves(Board board, ref Span<Move> legalMoves, int pieceColor, bool isCapturesOnly = false)
    {

        board.GenerateMoveGenInfo();
        int currMoveIndex = 0;
        currMoveIndex = GenerateKingMoves(legalMoves, currMoveIndex, pieceColor, board, isCapturesOnly);
        //Double check
        if (!board.isCurrentPlayerInDoubleCheck)
        {
            currMoveIndex = GenerateBishopMoves(legalMoves, currMoveIndex, pieceColor, board, isCapturesOnly);
            currMoveIndex = GenerateRookMoves(legalMoves, currMoveIndex, pieceColor, board, isCapturesOnly);
            currMoveIndex = GenerateKnightMoves(legalMoves, currMoveIndex, pieceColor, board, isCapturesOnly);
            currMoveIndex = GeneratePawnMoves(legalMoves, currMoveIndex, pieceColor, board, isCapturesOnly);
        }
        legalMoves = legalMoves.Slice(0, currMoveIndex);
        return currMoveIndex;
    }
    
    public static ulong GenerateAttackedSquares(Board board, int attackingPieceColor)
    {
        int colorIndex = (attackingPieceColor == Piece.White) ? Board.WhiteIndex : Board.BlackIndex;
        int oppositeColorIndex = (colorIndex == Board.WhiteIndex) ? Board.BlackIndex : Board.WhiteIndex;

        ulong attacks = BitboardHelper.GetAllPawnAttacks(board.pieceBitboards[colorIndex, Piece.Pawn], attackingPieceColor);
        ulong king = board.pieceBitboards[colorIndex, Piece.King];
        attacks |= BitboardHelper.kingAttacks[BitboardHelper.PopLSB(ref king)];
        

        ulong piecesExceptEnemyKing = board.allPiecesBitboard ^ board.pieceBitboards[oppositeColorIndex, Piece.King];
        ulong queens = board.pieceBitboards[colorIndex, Piece.Queen];
        ulong rooks = board.pieceBitboards[colorIndex, Piece.Rook];
        ulong bishops = board.pieceBitboards[colorIndex, Piece.Bishop];
        ulong knights = board.pieceBitboards[colorIndex, Piece.Knight];

        while (queens != 0)
        {
            int index = BitboardHelper.PopLSB(ref queens);
            attacks |= BitboardHelper.GetBishopAttacks(index, piecesExceptEnemyKing);
            attacks |= BitboardHelper.GetRookAttacks(index, piecesExceptEnemyKing);
        }

        while (rooks != 0)
        {
            int index = BitboardHelper.PopLSB(ref rooks);
            attacks |= BitboardHelper.GetRookAttacks(index, piecesExceptEnemyKing);
        }

        while (bishops != 0)
        {
            int index = BitboardHelper.PopLSB(ref bishops);
            attacks |= BitboardHelper.GetBishopAttacks(index, piecesExceptEnemyKing);
        }
        while (knights != 0)
        {
            int index = BitboardHelper.PopLSB(ref knights);
            attacks |= BitboardHelper.knightAttacks[index];
        }
        return attacks;
    }

    //Self explanatory
    public static int GeneratePawnMoves(Span<Move> legalMoves, int currMoveIndex, int pieceColor,  Board board, bool isCapturesOnly = false)
    {
        int colorIndex = (pieceColor == Piece.White) ? Board.WhiteIndex : Board.BlackIndex;
        int oppositeColorIndex = (pieceColor == Piece.White) ? Board.BlackIndex : Board.WhiteIndex;

        ulong pawns = board.pieceBitboards[colorIndex, Piece.Pawn];
        ulong blockers = board.allPiecesBitboard;


        while (pawns != 0)
        {
            int index = BitboardHelper.PopLSB(ref pawns);
            int rank = Coord.IndexToRank(index);
            ulong moves = 0;
            bool isDiagPinned = BitboardHelper.ContainsSquare(board.diagPins, index);

            if (pieceColor == Piece.White)
            {
                bool isMovePromotion = rank == 7;
                //Captures
                if (!BitboardHelper.ContainsSquare(board.straightPins, index))
                {
                    moves = BitboardHelper.wPawnAttacks[index] & board.sideBitboard[oppositeColorIndex];
                    if(isDiagPinned) { moves &= board.diagPins; }
                    if (board.isCurrentPlayerInCheck) { moves &= board.checkIndexes; }
                    while (moves != 0)
                    {
                        if (!isMovePromotion){ legalMoves[currMoveIndex++] = new Move(index, BitboardHelper.PopLSB(ref moves), true); }
                        else
                        {
                            int newIndex = BitboardHelper.PopLSB(ref moves);
                            for (int flag = 1; flag < 5; flag++){ legalMoves[currMoveIndex++] = new Move(index, newIndex, true, flag); }
                        }
                        
                    }

                    //EnPassant
                    if (board.enPassantIndex != -1)
                    {
                        moves = BitboardHelper.wPawnAttacks[index] & (1ul << board.enPassantIndex);
                        if(isDiagPinned) { moves &= board.diagPins; }
                        if (board.isCurrentPlayerInCheck & !BitboardHelper.ContainsSquare(board.checkIndexes, board.enPassantIndex + 8)) { moves &= board.checkIndexes; }
                        if (moves != 0)
                        {
                            if (isLegalEP(board.allPiecesBitboard ^ ((1ul << index) | (1ul << (board.enPassantIndex + 8)) | moves)))
                            {
                                legalMoves[currMoveIndex++] = new Move(index, BitboardHelper.PopLSB(ref moves), true, 7);
                            }
                        }
                    }
                }
                if (!isCapturesOnly)
                {
                    //Double pawn push
                    if ((BitboardHelper.wPawnDoubleMask[index] & blockers) == 0 && !isDiagPinned)
                    {
                        moves = BitboardHelper.wPawnDouble[index];
                        if (board.isCurrentPlayerInCheck) { moves &= board.checkIndexes; }
                        if(BitboardHelper.ContainsSquare(board.straightPins, index)){moves &= board.straightPins;}
                    }

                    while (moves != 0) { legalMoves[currMoveIndex++] = new Move(index, BitboardHelper.PopLSB(ref moves), false, 6); }

                    moves = !isDiagPinned ? BitboardHelper.wPawnMoves[index] : 0;
                    moves &= ~blockers;
                    if (board.isCurrentPlayerInCheck) { moves &= board.checkIndexes; }
                    if(BitboardHelper.ContainsSquare(board.straightPins, index)){moves &= board.straightPins;}

                    while (moves != 0)
                    {
                        if (!isMovePromotion) { legalMoves[currMoveIndex++] = new Move(index, BitboardHelper.PopLSB(ref moves), false); }
                        else
                        {
                            int newIndex = BitboardHelper.PopLSB(ref moves);
                            for (int flag = 1; flag < 5; flag++) { legalMoves[currMoveIndex++] = new Move(index, newIndex, false, flag); }
                        }
                    }
                }
            }
            else
            {
                bool isMovePromotion = rank == 2;
                //Captures
                if (!BitboardHelper.ContainsSquare(board.straightPins, index))
                {
                    moves = BitboardHelper.bPawnAttacks[index] & board.sideBitboard[oppositeColorIndex];
                    if (board.isCurrentPlayerInCheck) { moves &= board.checkIndexes; }
                    if(isDiagPinned) { moves &= board.diagPins; }
                    while (moves != 0)
                    {
                        if (!isMovePromotion) { legalMoves[currMoveIndex++] = new Move(index, BitboardHelper.PopLSB(ref moves), true); }
                        else
                        {
                            int newIndex = BitboardHelper.PopLSB(ref moves);
                            for (int flag = 1; flag < 5; flag++) { legalMoves[currMoveIndex++] = new Move(index, newIndex, true, flag); }
                        }

                    }

                    //EnPassant
                    if (board.enPassantIndex != -1)
                    {
                        moves = BitboardHelper.bPawnAttacks[index] & (1ul << board.enPassantIndex);
                        if(isDiagPinned) { moves &= board.diagPins; }
                        if (board.isCurrentPlayerInCheck & ! BitboardHelper.ContainsSquare(board.checkIndexes, board.enPassantIndex - 8)) { moves &= board.checkIndexes; }
                        if (moves != 0)
                        {
                            if (isLegalEP(board.allPiecesBitboard ^ ((1ul << index) | (1ul << (board.enPassantIndex - 8)) | moves)))
                            {
                                legalMoves[currMoveIndex++] = new Move(index, BitboardHelper.PopLSB(ref moves), true, 7);
                            }
                        }
                    }
                }
                if (!isCapturesOnly)
                {
                    //Double pawn push
                    if ((BitboardHelper.bPawnDoubleMask[index] & blockers) == 0 && !isDiagPinned)
                    {
                        moves = BitboardHelper.bPawnDouble[index];
                        if (board.isCurrentPlayerInCheck) { moves &= board.checkIndexes; }
                        if(BitboardHelper.ContainsSquare(board.straightPins, index)){moves &= board.straightPins;}
                    }

                    while (moves != 0) { legalMoves[currMoveIndex++] = new Move(index, BitboardHelper.PopLSB(ref moves), false, 6); }

                    moves = !isDiagPinned ? BitboardHelper.bPawnMoves[index] : 0;
                    moves &= ~blockers;
                    if (board.isCurrentPlayerInCheck) { moves &= board.checkIndexes; }
                    if(BitboardHelper.ContainsSquare(board.straightPins, index)){moves &= board.straightPins;}

                    while (moves != 0)
                    {
                        if (!isMovePromotion) { legalMoves[currMoveIndex++] = new Move(index, BitboardHelper.PopLSB(ref moves), false); }
                        else
                        {
                            int newIndex = BitboardHelper.PopLSB(ref moves);
                            for (int flag = 1; flag < 5; flag++) { legalMoves[currMoveIndex++] = new Move(index, newIndex, false, flag); }
                        }
                    }
                }
            }
            
        }
        return currMoveIndex;

        bool isLegalEP(ulong boardMinusPawnsInvolved)
        {
            int kingIndex = GetKingIndex(pieceColor, board);
            ulong attackingPiecesMask = BitboardHelper.GetRookAttacks(kingIndex, boardMinusPawnsInvolved);
            return (attackingPiecesMask & (board.pieceBitboards[oppositeColorIndex, Piece.Rook] | board.pieceBitboards[oppositeColorIndex, Piece.Queen])) == 0;
        }
    }
    public static int GenerateKnightMoves(Span<Move> legalMoves, int currMoveIndex, int pieceColor, Board board, bool isCapturesOnly = false)
    {
        int colorIndex = (pieceColor == Piece.White) ? Board.WhiteIndex : Board.BlackIndex;
        int oppositeColorIndex = (pieceColor == Piece.White) ? Board.BlackIndex : Board.WhiteIndex;

        ulong knights = board.pieceBitboards[colorIndex, Piece.Knight];

        ulong diagPins = board.diagPins;
        ulong straightPins = board.straightPins;
        ulong checkIndexes = board.checkIndexes;
        while (knights != 0)
        {
            int index = BitboardHelper.PopLSB(ref knights);
            ulong attackedSquares = BitboardHelper.knightAttacks[index] & ~board.sideBitboard[colorIndex];

            if (BitboardHelper.ContainsSquare(diagPins, index) | BitboardHelper.ContainsSquare(straightPins, index))
            {
                attackedSquares &= 0;
            }
            if (board.isCurrentPlayerInCheck) { attackedSquares &= checkIndexes; }
            ulong captures = attackedSquares & board.sideBitboard[oppositeColorIndex];
            attackedSquares &= ~board.sideBitboard[oppositeColorIndex];

            while (captures != 0)
            {
                int newIndex = BitboardHelper.PopLSB(ref captures);
                legalMoves[currMoveIndex++] =  new Move(index, newIndex, true);
            }

            if (!isCapturesOnly)
            {
                while (attackedSquares != 0)
                {
                    int newIndex = BitboardHelper.PopLSB(ref attackedSquares);
                    legalMoves[currMoveIndex++] =  new Move(index, newIndex, false);
                }
            }
        }
        return currMoveIndex;
    }
    public static int GenerateBishopMoves(Span<Move> legalMoves, int currMoveIndex, int pieceColor, Board board, bool isCapturesOnly = false)
    {
        int colorIndex = (pieceColor == Piece.White) ? Board.WhiteIndex : Board.BlackIndex;
        int oppositeColorIndex = (pieceColor == Piece.White) ? Board.BlackIndex : Board.WhiteIndex;
        ulong bishops = board.pieceBitboards[colorIndex, Piece.Bishop] | board.pieceBitboards[colorIndex, Piece.Queen];
        ulong diagPins = board.diagPins;
        ulong straightPins = board.straightPins;
        ulong checkIndexes = board.checkIndexes;

        while (bishops != 0)
        {
            int index = BitboardHelper.PopLSB(ref bishops);
            ulong attackedSquares = BitboardHelper.GetBishopAttacks(index, board.allPiecesBitboard) & ~board.sideBitboard[colorIndex];
            if (BitboardHelper.ContainsSquare(diagPins, index))
            {
                attackedSquares &= diagPins;
            }
            else if (BitboardHelper.ContainsSquare(straightPins, index)) { attackedSquares &= 0; }
            if (board.isCurrentPlayerInCheck)
            {
                attackedSquares &= checkIndexes;
            }

            ulong captures = attackedSquares & board.sideBitboard[oppositeColorIndex];
            attackedSquares &= ~board.sideBitboard[oppositeColorIndex];

            while (captures != 0)
            {
                int newIndex = BitboardHelper.PopLSB(ref captures);
                legalMoves[currMoveIndex++] =  new Move(index, newIndex, true);
            }

            if (!isCapturesOnly)
            {
                while (attackedSquares != 0)
                {
                    int newIndex = BitboardHelper.PopLSB(ref attackedSquares);
                    legalMoves[currMoveIndex++] =  new Move(index, newIndex, false);
                }
            }
        }
        return currMoveIndex;
    }
    public static int GenerateRookMoves(Span<Move> legalMoves, int currMoveIndex, int pieceColor, Board board, bool isCapturesOnly=false){
        int colorIndex = (pieceColor == Piece.White) ? Board.WhiteIndex : Board.BlackIndex;
        int oppositeColorIndex = (pieceColor == Piece.White) ? Board.BlackIndex : Board.WhiteIndex;
        ulong rooks = board.pieceBitboards[colorIndex, Piece.Rook] | board.pieceBitboards[colorIndex, Piece.Queen];
        ulong diagPins = board.diagPins;
        ulong straightPins = board.straightPins;
        ulong checkIndexes = board.checkIndexes;

        while (rooks != 0)
        {
            int index = BitboardHelper.PopLSB(ref rooks);
            ulong attackedSquares = BitboardHelper.GetRookAttacks(index, board.allPiecesBitboard) & ~board.sideBitboard[colorIndex];
            if (BitboardHelper.ContainsSquare(straightPins, index))
            {
                attackedSquares &= straightPins;
            }
            else if (BitboardHelper.ContainsSquare(diagPins, index)) { attackedSquares &= 0; }
            if (board.isCurrentPlayerInCheck)
            {
                attackedSquares &= checkIndexes;
            }

            ulong captures = attackedSquares & board.sideBitboard[oppositeColorIndex];
            attackedSquares &= ~board.sideBitboard[oppositeColorIndex];

            while (captures != 0)
            {
                int newIndex = BitboardHelper.PopLSB(ref captures);
                legalMoves[currMoveIndex++] =  new Move(index, newIndex, true);
            }

            if (!isCapturesOnly)
            {
                while (attackedSquares != 0)
                {
                    int newIndex = BitboardHelper.PopLSB(ref attackedSquares);
                    legalMoves[currMoveIndex++] =  new Move(index, newIndex, false);
                }
            }
        }
        return currMoveIndex;
    }
    public static int GenerateKingMoves(Span<Move> moves, int currMoveIndex, int pieceColor, Board board, bool isCapturesOnly = false)
    {
        int colorIndex = (pieceColor == Piece.White) ? Board.WhiteIndex : Board.BlackIndex;
        int oppositeColorIndex = (colorIndex == Board.WhiteIndex) ? Board.BlackIndex : Board.WhiteIndex;

        //Getting king index
        ulong kingIndexes = board.pieceBitboards[colorIndex, Piece.King];

        int index = BitboardHelper.PopLSB(ref kingIndexes);

        //Removing squares attacked by the enemy
        ulong kingMoves = BitboardHelper.kingAttacks[index];
        kingMoves &= ~board.attackedSquares[oppositeColorIndex];
        kingMoves &= ~board.sideBitboard[colorIndex];

        ulong kingCaptures = kingMoves & board.sideBitboard[oppositeColorIndex];
        kingMoves &= ~board.sideBitboard[oppositeColorIndex];

        while (kingCaptures != 0)
        {
            moves[currMoveIndex++] =  new Move(index, BitboardHelper.PopLSB(ref kingCaptures), true);
        }
        if (isCapturesOnly)
        {
            return currMoveIndex;
        }

        while (kingMoves != 0)
        {
            moves[currMoveIndex++] = new Move(index, BitboardHelper.PopLSB(ref kingMoves), false);
        }

        //Add castling
        ulong piecesAndAttackedSquares = board.attackedSquares[oppositeColorIndex] | board.allPiecesBitboard;
        if (board.HasKingsideRight(pieceColor) && !board.isCurrentPlayerInCheck)
        {
            //No pieces/attacked squares between castling points
            if (pieceColor == Piece.White && (piecesAndAttackedSquares & BitboardHelper.whiteKingsideCastleMask) == 0) { moves[currMoveIndex++] =  new Move(index, 62, false, 5); }
            else if (pieceColor == Piece.Black && (piecesAndAttackedSquares & BitboardHelper.blackKingsideCastleMask) == 0) { moves[currMoveIndex++] = new Move(index, 6, false, 5); }
        }
        if (board.HasQueensideRight(pieceColor) && !board.isCurrentPlayerInCheck)
        {
            if (pieceColor == Piece.White && (board.attackedSquares[oppositeColorIndex] & BitboardHelper.whiteQueensideAttackCastleMask) == 0 && (board.allPiecesBitboard & BitboardHelper.whiteQueensidePieceCastleMask) == 0) { moves[currMoveIndex++] =  new Move(index, 58, false, 5); }
            else if (pieceColor == Piece.Black && (board.attackedSquares[oppositeColorIndex] & BitboardHelper.blackQueensideAttackCastleMask) == 0 && (board.allPiecesBitboard & BitboardHelper.blackQueensidePieceCastleMask) == 0) { moves[currMoveIndex++] =  new Move(index, 2, false, 5); }
        }
        return currMoveIndex;
    }

    public static void UpdateChecksAndPins(Board board)
    {
        int colorTurn = board.colorTurn;
        int colorIndex = (colorTurn == Piece.White) ? Board.WhiteIndex : Board.BlackIndex;
        int oppositeColorIndex = (colorTurn == Piece.White) ? Board.BlackIndex : Board.WhiteIndex;
        int kingIndex = GetKingIndex(colorTurn, board);

        ulong checkRays = 0;
        ulong straightPins = 0;
        ulong diagPins = 0;

        ulong straightAttackers = board.pieceBitboards[oppositeColorIndex, Piece.Rook] | board.pieceBitboards[oppositeColorIndex, Piece.Queen];
        ulong diagAttackers = board.pieceBitboards[oppositeColorIndex, Piece.Bishop] | board.pieceBitboards[oppositeColorIndex, Piece.Queen];

        ulong friendlyPieces = board.sideBitboard[colorIndex];
        ulong straightBlockers = board.sideBitboard[oppositeColorIndex] & ~straightAttackers;
        ulong diagBlockers = board.sideBitboard[oppositeColorIndex] & ~diagAttackers;

        int x = Coord.IndexToFile(kingIndex) - 1;
        int y = 8 - Coord.IndexToRank(kingIndex);
        int numCheckingPieces = 0;

        SlidersDetection(true, straightBlockers, friendlyPieces, straightAttackers, ref checkRays, ref straightPins);
        SlidersDetection(false, diagBlockers, friendlyPieces, diagAttackers, ref checkRays, ref diagPins);

        ulong knightCheck = BitboardHelper.knightAttacks[kingIndex] & board.pieceBitboards[oppositeColorIndex, Piece.Knight];
        //Knight checks
        checkRays |= knightCheck;
        if(knightCheck != 0) { numCheckingPieces++; }

        ulong pawnCheck = ((colorTurn == Piece.White) ? BitboardHelper.wPawnAttacks[kingIndex] : BitboardHelper.bPawnAttacks[kingIndex]) & board.pieceBitboards[oppositeColorIndex, Piece.Pawn];
        //Pawn checks
        checkRays |= pawnCheck;
        if(pawnCheck != 0) { numCheckingPieces++; }

        board.checkIndexes = checkRays;
        board.diagPins = diagPins;
        board.straightPins = straightPins;
        board.numCheckingPieces = numCheckingPieces;
        

        void SlidersDetection(bool rook, ulong blocker, ulong friendly, ulong enemy, ref ulong checkRays, ref ulong pins)
        {
            foreach ((int x, int y) direction in rook ? BitboardHelper.rookDirections: BitboardHelper.bishopDirections)
            {
                ulong currentRay = 0;
                bool hasHitFriendly = false;

                for (int dst = 1; dst < 8; dst++)
                {
                    int index;
                    if (BitboardHelper.ValidSquareIndex(x + (direction.x * dst), y + (direction.y * dst), out index))
                    {
                        currentRay |= 1ul << index;
                        if (BitboardHelper.ContainsSquare(enemy, index))
                        {
                            //Pin
                            if (hasHitFriendly) { pins |= currentRay; break; }
                            //Check
                            else { checkRays |= currentRay; numCheckingPieces++; break; }
                        }
                        else if (BitboardHelper.ContainsSquare(friendly, index))
                        {
                            //Two friendly pieces, no pin possible
                            if (hasHitFriendly) { break; }
                            else { hasHitFriendly = true; }
                        }
                        //Non attacking enemy piece
                        else if (BitboardHelper.ContainsSquare(blocker, index)){break;}
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

    }
    //Returns king's position
    public static int GetKingIndex(int kingColor, Board board)
    {
        int colorIndex = (kingColor == Piece.White) ? Board.WhiteIndex : Board.BlackIndex;
        ulong kingBitboard = board.pieceBitboards[colorIndex, Piece.King];
        return (kingBitboard != 0) ? BitboardHelper.PopLSB(ref kingBitboard) : 0;
    }

    

}