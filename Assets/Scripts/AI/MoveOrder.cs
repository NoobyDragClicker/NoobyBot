using System;
using System.Collections.Generic;

public class MoveOrder
{
    const int million = 1000000;
    const int maxMoves = 218;
    float[] moveScores = new float[maxMoves];
    public List<Move> OrderMoves(Board board, List<Move> legalMoves, Move firstMove, Move[,] killerMoves, AISettings aiSettings){
        List<Move> moves = legalMoves;

        for (int x = 0; x < legalMoves.Count; x++)
        {
            Move move = legalMoves[x];
            int score = 0;
            if (firstMove != null && move.GetIntValue() == firstMove.GetIntValue())
            {
                score = 8 * million;
            }
            else if (killerMoves[board.plyFromStart, 0] != null && move.GetIntValue() == killerMoves[board.plyFromStart, 0].GetIntValue())
            {
                score = million + 3;
            }
            else if (killerMoves[board.plyFromStart, 1] != null && move.GetIntValue() == killerMoves[board.plyFromStart, 1].GetIntValue())
            {
                score = million + 2;
            }
            else if (killerMoves[board.plyFromStart, 2] != null && move.GetIntValue() == killerMoves[board.plyFromStart, 2].GetIntValue())
            {
                score = million + 1;
            }
            else
            {
                int movedPieceValue;
                if (move.isCapture())
                {

                    int capturedPieceValue;
                    //en passant
                    if (move.flag == 7)
                    {
                        if (Piece.IsColour(board.board[legalMoves[x].oldIndex], Piece.White))
                        {
                            capturedPieceValue = GetPieceValue(Piece.PieceType(board.board[legalMoves[x].newIndex + 8]));
                        }
                        else
                        {
                            capturedPieceValue = GetPieceValue(Piece.PieceType(board.board[legalMoves[x].newIndex - 8]));
                        }
                    }
                    else
                    {
                        capturedPieceValue = GetPieceValue(Piece.PieceType(board.board[legalMoves[x].newIndex]));
                    }
                    movedPieceValue = GetPieceValue(Piece.PieceType(board.board[legalMoves[x].oldIndex]));

                    //Basically MVV LVA, *10 to give more space for killers 
                    score = million + ((capturedPieceValue - movedPieceValue) * 10);
                }
                else if (move.flag == 5)
                {
                    score = 3;
                }
                else if (move.isPromotion())
                {
                    score = 9;
                }

                //Penalty for moving to attacked square
                if ((Piece.Color(board.board[legalMoves[x].oldIndex]) == Piece.White && board.blackAttackedSquares[move.newIndex] == 1) | (board.colorTurn == Piece.Black && board.whiteAttackedSquares[move.newIndex] == 1))
                {
                    score -= 4;
                }

                //Bonus for developping
                if (Coord.IndexToFile(move.newIndex) >= 3 && Coord.IndexToFile(move.newIndex) <= 6 && Coord.IndexToRank(move.newIndex) >= 3 && Coord.IndexToRank(move.newIndex) <= 6)
                {
                    score += 2;
                }
                else if (Coord.IndexToFile(move.newIndex) >= 2 && Coord.IndexToFile(move.newIndex) <= 7 && Coord.IndexToRank(move.newIndex) >= 2 && Coord.IndexToRank(move.newIndex) <= 7)
                {
                    score += 1;
                }
            }
            moveScores[x] = score;
        }

        moves = Sort(moves);
        Array.Clear(moveScores, 0, moveScores.Length);
        return moves;
    }
    List<Move> Sort(List<Move> moves){
        for(int i = 0; i< moves.Count - 1; i++){
            for(int j = i + 1; j > 0; j--){
                int swapIndex = j -1;
                if(moveScores[swapIndex] < moveScores[j]){
                    (moves[j], moves[swapIndex]) = (moves[swapIndex], moves[j]);
					(moveScores[j], moveScores[swapIndex]) = (moveScores[swapIndex], moveScores[j]);
                }
            }
        }
        return moves;
    }
    static int GetPieceValue (int pieceType) {
		switch (pieceType) {
			case Piece.Queen:
				return Evaluation.queenValue;
			case Piece.Rook:
				return Evaluation.rookValue;
			case Piece.Knight:
				return Evaluation.knightValue;
			case Piece.Bishop:
				return Evaluation.bishopValue;
			case Piece.Pawn:
				return Evaluation.pawnValue;
            //Could change this to prioritise king moves in endgame
            case Piece.King:
                return 1;
			default:
				return 0;
		}
	}
}
