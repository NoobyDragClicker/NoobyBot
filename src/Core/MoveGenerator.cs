using System;
using System.Collections.Generic;
using System.Linq;


public static class MoveGenerator
{
    public static int GenerateLegalMoves(Board board, ref Span<Move> legalMoves, bool isCapturesOnly = false)
    {
        int currMoveIndex = 0;
        board.GenerateMoveGenInfo();
        currMoveIndex = GenerateKingMoves(legalMoves, currMoveIndex, board, isCapturesOnly);
        //Double check
        if (!board.gameStateHistory[board.fullMoveClock].isCurrentPlayerInDoubleCheck)
        {
            currMoveIndex = GenerateBishopMoves(legalMoves, currMoveIndex, board, isCapturesOnly);
            currMoveIndex = GenerateRookMoves(legalMoves, currMoveIndex, board, isCapturesOnly);
            currMoveIndex = GenerateKnightMoves(legalMoves, currMoveIndex, board, isCapturesOnly);
            currMoveIndex = GeneratePawnMoves(legalMoves, currMoveIndex, board, isCapturesOnly);
        }
        legalMoves = legalMoves.Slice(0, currMoveIndex);
        
        return currMoveIndex;
        
    }
    
    public static Bitboard GenerateAttackedSquares(Board board, int attackingPieceColor)
    {
        int colorIndex = (attackingPieceColor == Piece.White) ? Board.WhiteIndex : Board.BlackIndex;
        int oppositeColorIndex = (colorIndex == Board.WhiteIndex) ? Board.BlackIndex : Board.WhiteIndex;

        Bitboard attacks = BitboardHelper.GetAllPawnAttacks(board.GetPieces(colorIndex, Piece.Pawn), attackingPieceColor);
        attacks |= BitboardHelper.kingAttacks[board.GetPieces(colorIndex, Piece.King).GetLSB()];
        

        Bitboard piecesExceptEnemyKing = board.allPiecesBitboard ^ board.GetPieces(oppositeColorIndex, Piece.King);
        Bitboard queens = board.GetPieces(colorIndex, Piece.Queen);
        Bitboard rooks = board.GetPieces(colorIndex, Piece.Rook);
        Bitboard bishops = board.GetPieces(colorIndex, Piece.Bishop);
        Bitboard knights = board.GetPieces(colorIndex, Piece.Knight);

        while (queens != 0)
        {
            int index = queens.PopLSB();
            attacks |= BitboardHelper.GetBishopAttacks(index, piecesExceptEnemyKing);
            attacks |= BitboardHelper.GetRookAttacks(index, piecesExceptEnemyKing);
        }

        while (rooks != 0)
        {
            int index = rooks.PopLSB();
            attacks |= BitboardHelper.GetRookAttacks(index, piecesExceptEnemyKing);
        }

        while (bishops != 0)
        {
            int index = bishops.PopLSB();
            attacks |= BitboardHelper.GetBishopAttacks(index, piecesExceptEnemyKing);
        }
        while (knights != 0)
        {
            int index = knights.PopLSB();
            attacks |= BitboardHelper.knightAttacks[index];
        }
        return attacks;
    }

    public static int GeneratePawnMoves(Span<Move> legalMoves, int currMoveIndex,  Board board, bool isCapturesOnly = false)
    {
        Bitboard pawns = board.GetPieces(board.currentColorIndex, Piece.Pawn);
        Bitboard blockers = board.allPiecesBitboard;
        Bitboard diagPins = board.gameStateHistory[board.fullMoveClock].diagPins;
        Bitboard checkIndexes = board.gameStateHistory[board.fullMoveClock].checkIndexes;
        Bitboard straightPins = board.gameStateHistory[board.fullMoveClock].straightPins;

        while (!pawns.Empty())
        {
            int index = pawns.PopLSB();
            int rank = Coord.IndexToRank(index);
            Bitboard moves = 0;
            bool isDiagPinned = diagPins.ContainsSquare(index);

            if (board.colorTurn == Piece.White)
            {
                bool isMovePromotion = rank == 7;
                //Captures
                if (!straightPins.ContainsSquare(index))
                {
                    moves = BitboardHelper.wPawnAttacks[index] & board.sideBitboard[board.oppositeColorIndex];
                    if(isDiagPinned) { moves &= diagPins; }
                    if (board.gameStateHistory[board.fullMoveClock].isInCheck) { moves &= checkIndexes; }
                    while (!moves.Empty())
                    {
                        if (!isMovePromotion){ legalMoves[currMoveIndex++] = new Move(index, moves.PopLSB(), true); }
                        else
                        {
                            int newIndex = moves.PopLSB();
                            for (int flag = 1; flag < 5; flag++){ legalMoves[currMoveIndex++] = new Move(index, newIndex, true, flag); }
                        }
                        
                    }

                    //EnPassant
                    if (board.enPassantIndex != -1)
                    {
                        moves = BitboardHelper.wPawnAttacks[index] & (1ul << board.enPassantIndex);
                        if(isDiagPinned) { moves &= diagPins; }
                        if (board.gameStateHistory[board.fullMoveClock].isInCheck & !checkIndexes.ContainsSquare(board.enPassantIndex + 8)) { moves &= checkIndexes; }
                        if (moves != 0)
                        {
                            if (isLegalEP(board.allPiecesBitboard ^ ((1ul << index) | (1ul << (board.enPassantIndex + 8)) | moves)))
                            {
                                legalMoves[currMoveIndex++] = new Move(index, moves.PopLSB(), true, Move.EnPassant);
                            }
                        }
                    }
                }
                if (!isCapturesOnly)
                {
                    //Double pawn push
                    if ((BitboardHelper.wPawnDoubleMask[index] & blockers).Empty() && !isDiagPinned)
                    {
                        moves = BitboardHelper.wPawnDouble[index];
                        if (board.gameStateHistory[board.fullMoveClock].isInCheck) { moves &= checkIndexes; }
                        if(straightPins.ContainsSquare(index)){moves &= straightPins;}
                    }

                    while (!moves.Empty()) { legalMoves[currMoveIndex++] = new Move(index, moves.PopLSB(), false, Move.DoublePawnPush); }

                    moves = !isDiagPinned ? BitboardHelper.wPawnMoves[index] : 0;
                    moves &= ~blockers;
                    if (board.gameStateHistory[board.fullMoveClock].isInCheck) { moves &= checkIndexes; }
                    if(straightPins.ContainsSquare(index)){moves &= straightPins;}

                    while (!moves.Empty())
                    {
                        if (!isMovePromotion) { legalMoves[currMoveIndex++] = new Move(index, moves.PopLSB(), false); }
                        else
                        {
                            int newIndex = moves.PopLSB();
                            for (int flag = 1; flag < 5; flag++) { legalMoves[currMoveIndex++] = new Move(index, newIndex, false, flag); }
                        }
                    }
                }
            }
            else
            {
                bool isMovePromotion = rank == 2;
                //Captures
                if (!straightPins.ContainsSquare(index))
                {
                    moves = BitboardHelper.bPawnAttacks[index] & board.sideBitboard[board.oppositeColorIndex];
                    if (board.gameStateHistory[board.fullMoveClock].isInCheck) { moves &= checkIndexes; }
                    if(isDiagPinned) { moves &= diagPins; }
                    while (!moves.Empty())
                    {
                        if (!isMovePromotion) { legalMoves[currMoveIndex++] = new Move(index, moves.PopLSB(), true); }
                        else
                        {
                            int newIndex = moves.PopLSB();
                            for (int flag = 1; flag < 5; flag++) { legalMoves[currMoveIndex++] = new Move(index, newIndex, true, flag); }
                        }

                    }

                    //EnPassant
                    if (board.enPassantIndex != -1)
                    {
                        moves = BitboardHelper.bPawnAttacks[index] & (1ul << board.enPassantIndex);
                        if(isDiagPinned) { moves &= diagPins; }
                        if (board.gameStateHistory[board.fullMoveClock].isInCheck & !checkIndexes.ContainsSquare(board.enPassantIndex - 8)) { moves &= checkIndexes; }
                        if (!moves.Empty())
                        {
                            if (isLegalEP(board.allPiecesBitboard ^ ((1ul << index) | (1ul << (board.enPassantIndex - 8)) | moves)))
                            {
                                legalMoves[currMoveIndex++] = new Move(index, moves.PopLSB(), true, Move.EnPassant);
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
                        if (board.gameStateHistory[board.fullMoveClock].isInCheck) { moves &= checkIndexes; }
                        if(straightPins.ContainsSquare(index)){moves &= straightPins;}
                    }

                    while (!moves.Empty()) { legalMoves[currMoveIndex++] = new Move(index, moves.PopLSB(), false, Move.DoublePawnPush); }

                    moves = !isDiagPinned ? BitboardHelper.bPawnMoves[index] : 0;
                    moves &= ~blockers;
                    if (board.gameStateHistory[board.fullMoveClock].isInCheck) { moves &= checkIndexes; }
                    if(straightPins.ContainsSquare(index)){moves &= straightPins;}

                    while (!moves.Empty())
                    {
                        if (!isMovePromotion) { legalMoves[currMoveIndex++] = new Move(index, moves.PopLSB(), false); }
                        else
                        {
                            int newIndex = moves.PopLSB();
                            for (int flag = 1; flag < 5; flag++) { legalMoves[currMoveIndex++] = new Move(index, newIndex, false, flag); }
                        }
                    }
                }
            }
            
        }
        return currMoveIndex;

        bool isLegalEP(Bitboard boardMinusPawnsInvolved)
        {
            int kingIndex = GetKingIndex(board.colorTurn, board);
            Bitboard attackingPiecesMask = BitboardHelper.GetRookAttacks(kingIndex, boardMinusPawnsInvolved);
            return (attackingPiecesMask & (board.GetPieces(board.oppositeColorIndex, Piece.Rook) | board.GetPieces(board.oppositeColorIndex, Piece.Queen))).Empty();
        }
    }
    public static int GenerateKnightMoves(Span<Move> legalMoves, int currMoveIndex, Board board, bool isCapturesOnly = false)
    {
        Bitboard knights = board.GetPieces(board.currentColorIndex, Piece.Knight);

        Bitboard diagPins = board.gameStateHistory[board.fullMoveClock].diagPins;
        Bitboard straightPins = board.gameStateHistory[board.fullMoveClock].straightPins;
        Bitboard checkIndexes = board.gameStateHistory[board.fullMoveClock].checkIndexes;

        while (!knights.Empty())
        {
            int index = knights.PopLSB();
            if (diagPins.ContainsSquare(index) | straightPins.ContainsSquare(index)){ continue; }
            Bitboard attackedSquares = BitboardHelper.knightAttacks[index] & ~board.sideBitboard[board.currentColorIndex];
            
            if (board.gameStateHistory[board.fullMoveClock].isInCheck) { attackedSquares &= checkIndexes; }
            Bitboard captures = attackedSquares & board.sideBitboard[board.oppositeColorIndex];
            attackedSquares &= ~board.sideBitboard[board.oppositeColorIndex];

            while (!captures.Empty())
            {
                int newIndex = captures.PopLSB();
                legalMoves[currMoveIndex++] =  new Move(index, newIndex, true);
            }

            if (!isCapturesOnly)
            {
                while (!attackedSquares.Empty())
                {
                    int newIndex = attackedSquares.PopLSB();
                    legalMoves[currMoveIndex++] =  new Move(index, newIndex, false);
                }
            }
        }
        return currMoveIndex;
    }
    public static int GenerateBishopMoves(Span<Move> legalMoves, int currMoveIndex, Board board, bool isCapturesOnly = false)
    {
        Bitboard bishops = board.GetPieces(board.currentColorIndex, Piece.Bishop) | board.GetPieces(board.currentColorIndex, Piece.Queen);
        Bitboard diagPins = board.gameStateHistory[board.fullMoveClock].diagPins;
        Bitboard straightPins = board.gameStateHistory[board.fullMoveClock].straightPins;
        Bitboard checkIndexes = board.gameStateHistory[board.fullMoveClock].checkIndexes;

        while (!bishops.Empty())
        {
            int index = bishops.PopLSB();
            Bitboard attackedSquares = BitboardHelper.GetBishopAttacks(index, board.allPiecesBitboard) & ~board.sideBitboard[board.currentColorIndex];
            if (straightPins.ContainsSquare(index)) { continue; }
            else if (diagPins.ContainsSquare(index)){ attackedSquares &= diagPins; }

            if (board.gameStateHistory[board.fullMoveClock].isInCheck) { attackedSquares &= checkIndexes; }

            Bitboard captures = attackedSquares & board.sideBitboard[board.oppositeColorIndex];
            attackedSquares &= ~board.sideBitboard[board.oppositeColorIndex];

            while (!captures.Empty())
            {
                int newIndex = captures.PopLSB();
                legalMoves[currMoveIndex++] =  new Move(index, newIndex, true);
            }

            if (!isCapturesOnly)
            {
                while (!attackedSquares.Empty())
                {
                    int newIndex = attackedSquares.PopLSB();
                    legalMoves[currMoveIndex++] =  new Move(index, newIndex, false);
                }
            }
        }
        return currMoveIndex;
    }
    public static int GenerateRookMoves(Span<Move> legalMoves, int currMoveIndex, Board board, bool isCapturesOnly=false){
        Bitboard rooks = board.GetPieces(board.currentColorIndex, Piece.Rook) | board.GetPieces(board.currentColorIndex, Piece.Queen);
        Bitboard diagPins = board.gameStateHistory[board.fullMoveClock].diagPins;
        Bitboard straightPins = board.gameStateHistory[board.fullMoveClock].straightPins;
        Bitboard checkIndexes = board.gameStateHistory[board.fullMoveClock].checkIndexes;

        while (!rooks.Empty())
        {
            int index = rooks.PopLSB();
            Bitboard attackedSquares = BitboardHelper.GetRookAttacks(index, board.allPiecesBitboard) & ~board.sideBitboard[board.currentColorIndex];
            if (diagPins.ContainsSquare(index)) { continue; }
            else if (straightPins.ContainsSquare(index)){ attackedSquares &= straightPins; }
            if (board.gameStateHistory[board.fullMoveClock].isInCheck){ attackedSquares &= checkIndexes; }

            Bitboard captures = attackedSquares & board.sideBitboard[board.oppositeColorIndex];
            attackedSquares &= ~board.sideBitboard[board.oppositeColorIndex];

            while (!captures.Empty())
            {
                int newIndex = captures.PopLSB();
                legalMoves[currMoveIndex++] =  new Move(index, newIndex, true);
            }

            if (!isCapturesOnly)
            {
                while (!attackedSquares.Empty())
                {
                    int newIndex = attackedSquares.PopLSB();
                    legalMoves[currMoveIndex++] =  new Move(index, newIndex, false);
                }
            }
        }
        return currMoveIndex;
    }
    public static int GenerateKingMoves(Span<Move> moves, int currMoveIndex, Board board, bool isCapturesOnly = false)
    {
        int index = board.GetPieces(board.currentColorIndex, Piece.King).GetLSB();

        //Removing squares attacked by the enemy
        Bitboard kingMoves = BitboardHelper.kingAttacks[index];
        kingMoves &= ~board.gameStateHistory[board.fullMoveClock].attackedSquares[board.oppositeColorIndex];
        kingMoves &= ~board.sideBitboard[board.currentColorIndex];

        Bitboard kingCaptures = kingMoves & board.sideBitboard[board.oppositeColorIndex];
        kingMoves &= ~board.sideBitboard[board.oppositeColorIndex];

        while (!kingCaptures.Empty())
        {
            moves[currMoveIndex++] =  new Move(index, kingCaptures.PopLSB(), true);
        }
        if (isCapturesOnly)
        {
            return currMoveIndex;
        }

        while (!kingMoves.Empty())
        {
            moves[currMoveIndex++] = new Move(index, kingMoves.PopLSB(), false);
        }

        //Add castling
        Bitboard piecesAndAttackedSquares = board.gameStateHistory[board.fullMoveClock].attackedSquares[board.oppositeColorIndex] | board.allPiecesBitboard;
        if (board.HasKingsideRight(board.colorTurn) && !board.gameStateHistory[board.fullMoveClock].isInCheck)
        {
            //No pieces/attacked squares between castling points
            if (board.colorTurn == Piece.White && (piecesAndAttackedSquares & BitboardHelper.whiteKingsideCastleMask) == 0) { moves[currMoveIndex++] =  new Move(index, 62, false, Move.Castle); }
            else if (board.colorTurn == Piece.Black && (piecesAndAttackedSquares & BitboardHelper.blackKingsideCastleMask) == 0) { moves[currMoveIndex++] = new Move(index, 6, false, Move.Castle); }
        }
        if (board.HasQueensideRight(board.colorTurn) && !board.gameStateHistory[board.fullMoveClock].isInCheck)
        {
            if (board.colorTurn == Piece.White && (board.gameStateHistory[board.fullMoveClock].attackedSquares[board.oppositeColorIndex] & BitboardHelper.whiteQueensideAttackCastleMask) == 0 && (board.allPiecesBitboard & BitboardHelper.whiteQueensidePieceCastleMask) == 0) { moves[currMoveIndex++] =  new Move(index, 58, false, Move.Castle); }
            else if (board.colorTurn == Piece.Black && (board.gameStateHistory[board.fullMoveClock].attackedSquares[board.oppositeColorIndex] & BitboardHelper.blackQueensideAttackCastleMask) == 0 && (board.allPiecesBitboard & BitboardHelper.blackQueensidePieceCastleMask) == 0) { moves[currMoveIndex++] =  new Move(index, 2, false, Move.Castle); }
        }
        return currMoveIndex;
    }

    public static void UpdateChecksAndPins(Board board)
    {
        int colorTurn = board.colorTurn;
        int kingIndex = GetKingIndex(colorTurn, board);

        Bitboard checkRays = 0;
        Bitboard straightPins = 0;
        Bitboard diagPins = 0;

        Bitboard straightAttackers = board.GetPieces(board.oppositeColorIndex, Piece.Rook) | board.GetPieces(board.oppositeColorIndex, Piece.Queen);
        Bitboard diagAttackers = board.GetPieces(board.oppositeColorIndex, Piece.Bishop) | board.GetPieces(board.oppositeColorIndex, Piece.Queen);

        Bitboard friendlyPieces = board.sideBitboard[board.currentColorIndex];
        Bitboard straightBlockers = board.sideBitboard[board.oppositeColorIndex] & ~straightAttackers;
        Bitboard diagBlockers = board.sideBitboard[board.oppositeColorIndex] & ~diagAttackers;

        int x = Coord.IndexToFile(kingIndex) - 1;
        int y = 8 - Coord.IndexToRank(kingIndex);
        int numCheckingPieces = 0;

        SlidersDetection(true, straightBlockers, friendlyPieces, straightAttackers, ref checkRays, ref straightPins);
        SlidersDetection(false, diagBlockers, friendlyPieces, diagAttackers, ref checkRays, ref diagPins);

        Bitboard knightCheck = BitboardHelper.knightAttacks[kingIndex] & board.GetPieces(board.oppositeColorIndex, Piece.Knight);
        //Knight checks
        checkRays |= knightCheck;
        if(!knightCheck.Empty()) { numCheckingPieces++; }

        Bitboard pawnCheck = ((colorTurn == Piece.White) ? BitboardHelper.wPawnAttacks[kingIndex] : BitboardHelper.bPawnAttacks[kingIndex]) & board.GetPieces(board.oppositeColorIndex, Piece.Pawn);
        //Pawn checks
        checkRays |= pawnCheck;
        if(!pawnCheck.Empty()) { numCheckingPieces++; }

        board.gameStateHistory[board.fullMoveClock].checkIndexes = checkRays;
        board.gameStateHistory[board.fullMoveClock].diagPins = diagPins;
        board.gameStateHistory[board.fullMoveClock].straightPins = straightPins;
        board.gameStateHistory[board.fullMoveClock].isCurrentPlayerInDoubleCheck  = numCheckingPieces > 1;
        

        void SlidersDetection(bool rook, Bitboard blocker, Bitboard friendly, Bitboard enemy, ref Bitboard checkRays, ref Bitboard pins)
        {
            foreach ((int x, int y) direction in rook ? BitboardHelper.rookDirections: BitboardHelper.bishopDirections)
            {
                Bitboard currentRay = 0;
                bool hasHitFriendly = false;

                for (int dst = 1; dst < 8; dst++)
                {
                    int index;
                    if (BitboardHelper.ValidSquareIndex(x + (direction.x * dst), y + (direction.y * dst), out index))
                    {
                        currentRay.SetSquare(index);
                        if (enemy.ContainsSquare(index))
                        {
                            //Pin
                            if (hasHitFriendly) { pins |= currentRay; break; }
                            //Check
                            else { checkRays |= currentRay; numCheckingPieces++; break; }
                        }
                        else if (friendly.ContainsSquare(index))
                        {
                            //Two friendly pieces, no pin possible
                            if (hasHitFriendly) { break; }
                            else { hasHitFriendly = true; }
                        }
                        //Non attacking enemy piece
                        else if (blocker.ContainsSquare(index)){break;}
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

    }

    public static bool DetermineCheckStatus(Board board)
    {
        int colorTurn = board.colorTurn;
        int kingIndex = GetKingIndex(colorTurn, board);

        Bitboard straightAttackers = board.GetPieces(board.oppositeColorIndex, Piece.Rook) | board.GetPieces(board.oppositeColorIndex, Piece.Queen);
        Bitboard diagAttackers = board.GetPieces(board.oppositeColorIndex, Piece.Bishop) | board.GetPieces(board.oppositeColorIndex, Piece.Queen);

        //Any piece that can block that check, excludes the sliding pieces that would be checking
        Bitboard straightBlockers = (board.sideBitboard[board.oppositeColorIndex] | board.sideBitboard[board.currentColorIndex]) & ~straightAttackers;
        Bitboard diagBlockers = (board.sideBitboard[board.oppositeColorIndex] | board.sideBitboard[board.currentColorIndex]) & ~diagAttackers;

        Bitboard straightCheckers = straightAttackers & BitboardHelper.GetRookAttacks(kingIndex, straightBlockers);
        if (!straightCheckers.Empty()) { return true; }

        Bitboard diagonalCheckers = diagAttackers & BitboardHelper.GetBishopAttacks(kingIndex, diagBlockers);
        if (!diagonalCheckers.Empty()) { return true; }


        //Knight checks
        Bitboard knightCheck = BitboardHelper.knightAttacks[kingIndex] & board.GetPieces(board.oppositeColorIndex, Piece.Knight);
        if (!knightCheck.Empty()) { return true; }

        //Pawn checks
        Bitboard pawnCheck = ((colorTurn == Piece.White) ? BitboardHelper.wPawnAttacks[kingIndex] : BitboardHelper.bPawnAttacks[kingIndex]) & board.GetPieces(board.oppositeColorIndex, Piece.Pawn);
        if (!pawnCheck.Empty()) { return true; }
        return false;
    }
    public static int GetKingIndex(int kingColor, Board board)
    {
        int colorIndex = (kingColor == Piece.White) ? Board.WhiteIndex : Board.BlackIndex;
        return board.GetPieces(colorIndex, Piece.King).GetLSB();
    }
}