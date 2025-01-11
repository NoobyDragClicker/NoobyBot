using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MoveOrder
{
    const int maxMoves = 218;
    float[] moveScores = new float[maxMoves];
    public List<Move> OrderMoves(Board board, List<Move> legalMoves){
        List<Move> moves = legalMoves;
        List<Move> bestMoves = new List<Move>();
        for(int x = 0; x< legalMoves.Count; x++){
            Move move = legalMoves[x];
            float score = 0;
            int movedPieceValue = 0;
            if(move.isCapture()){
                int capturedPieceValue = 0;
                //en passant
                if(move.flag == 7){
                    if(Piece.IsColour(board.board[legalMoves[x].oldIndex], Piece.White)){
                        capturedPieceValue = GetPieceValue(Piece.PieceType(board.board[legalMoves[x].newIndex + 8]));
                    } else {
                        capturedPieceValue = GetPieceValue(Piece.PieceType(board.board[legalMoves[x].newIndex - 8]));
                    }
                } else{
                    capturedPieceValue = GetPieceValue(Piece.PieceType(board.board[legalMoves[x].newIndex]));
                }
                
                if(move.isPromotion()){
                    movedPieceValue = GetPieceValue(move.PromotedPieceType());
                } 
                else {
                    movedPieceValue = GetPieceValue(Piece.PieceType(board.board[legalMoves[x].oldIndex]));
                }
                //Adds one to differentiate from the non captures
                score = 1+ capturedPieceValue - movedPieceValue ;
                //Castle
            } else if(move.flag == 5){
                score = 3;
            }/* else{
                if(move.isPromotion()){
                    movedPieceValue = GetPieceValue(move.PromotedPieceType());
                } 
                else {
                    movedPieceValue = GetPieceValue(Piece.PieceType(board.board[legalMoves[x].oldIndex]));
                }
                //Bigger piece value = higher ordering
                score = movedPieceValue / 10;
            }*/
            if(score >= 2){
                bestMoves.Add(move);
            }
        }

        //Remove the moves we are moving to the front
        for(int x = 0; x<bestMoves.Count; x++){
            moves.Remove(bestMoves[x]);
        }

        //Move the best moves to the front
        bestMoves.AddRange(moves);
        
        

        return bestMoves;
    }

    /*List<Move> Sort(List<Move> moves){
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
    }*/

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
