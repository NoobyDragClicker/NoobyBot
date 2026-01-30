using System.Numerics;
public static class SEE
{
    static int[] SEEPieceVals = [0, 100, 300, 300, 500, 900, 0];

    public static bool EvaluateSEE(Board board, Move move, int threshold)
    {
        //Implementation from ethereal
        int nextVictim = move.isPromotion() ? move.PromotedPieceType() : board.MovedPieceType(move);
        int balance = EstimatedCaptureValue(board, move) - threshold;

        //Capture is not worth the threshold
        if(balance < 0){ return false; }
        
        //If the moved piece is captured and we are still better, it is a good capture
        balance -= SEEPieceVals[nextVictim];
        if(balance >= 0){ return true; }

        ulong bishops = board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Bishop)] | board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Bishop)] | board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Queen)] | board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Queen)];
        ulong rooks = board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Rook)] | board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Rook)] | board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Queen)] | board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Queen)];

        //Update occupancy
        ulong allPieces = board.allPiecesBitboard;
        allPieces = (allPieces ^ (1ul<<move.oldIndex)) | (1ul<<move.newIndex);
        if(move.flag == Move.EnPassant){ allPieces ^= 1ul<<board.enPassantIndex; }

        ulong attackers = board.GetAttackersToSquare(move.newIndex, allPieces, rooks, bishops) & allPieces;

        int currentColorIndex = board.oppositeColorIndex;

        ulong myAttackers;
        while (true)
        {
            myAttackers = attackers & board.sideBitboard[currentColorIndex];
            if(myAttackers == 0){ break; }
            for(nextVictim = Piece.Pawn; nextVictim <= Piece.Queen; nextVictim++)
            {
                if((myAttackers & board.pieceBitboards[Board.PieceBitboardIndex(currentColorIndex, nextVictim)]) != 0){ break; }
            }


            //Update occupancy
            allPieces ^= (1ul << BitOperations.TrailingZeroCount(myAttackers & board.pieceBitboards[Board.PieceBitboardIndex(currentColorIndex, nextVictim)]));

            //A diagonal move can reveal a bishop or a queen attacker
            if(nextVictim == Piece.Pawn || nextVictim == Piece.Bishop || nextVictim == Piece.Queen)
            {
                attackers |= BitboardHelper.GetBishopAttacks(move.newIndex, allPieces) & bishops;
            }
            //Rook or queen move can reveal a rook or queen attacker
            if(nextVictim == Piece.Rook || nextVictim == Piece.Queen)
            {
                attackers |= BitboardHelper.GetRookAttacks(move.newIndex, allPieces) & rooks;
            }

            //Remove any already used attacks
            attackers &= allPieces;

            currentColorIndex = 1 - currentColorIndex;

            balance = -balance - 1 - SEEPieceVals[nextVictim];

            if(balance >= 0)
            {
                //If our last attacking piece is a king, and the opponent has attackers, we have lost as the move we followed would be illegal
                if(nextVictim == Piece.King && (attackers & board.sideBitboard[currentColorIndex]) != 0)
                {
                    currentColorIndex = 1 - currentColorIndex;
                }
                break;
            }
        }
        return board.currentColorIndex != currentColorIndex;
    }

    public static int EstimatedCaptureValue(Board board, Move move)
    {
        if(move.flag == Move.EnPassant){ return SEEPieceVals[Piece.Pawn]; }
        else if (move.isPromotion()){
            return SEEPieceVals[board.PieceAt(move.newIndex)] + SEEPieceVals[move.PromotedPieceType()] - SEEPieceVals[Piece.Pawn];
        }
        else
        {
            return SEEPieceVals[board.PieceAt(move.newIndex)];
        }
    }
}