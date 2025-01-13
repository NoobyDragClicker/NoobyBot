using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class MoveGenerator
{
    /*
    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    stopwatch.Start();
    UnityEngine.Debug.Log(stopwatch.Elapsed);
    stopwatch.Stop();   
    */

    public MoveGenerator(){}
    //Used for a player when clicking on a piece
    public List<Move> GeneratePieceMove(int piece, int index, Board board){
        int pieceType = Piece.PieceType(piece);
        int pieceColor = Piece.Color(piece);
        int oppositeColor = (pieceColor == Piece.White)? Piece.Black : Piece.White;
        List<Move> legalMoves = new List<Move>();

        if(board.isCurrentPlayerInDoubleCheck){
            if(pieceType == Piece.King){
                legalMoves = GenerateKingMoves(index, pieceColor, board, false);
            }
            return legalMoves;
        }

        //Only if its the right turn
        if(board.colorTurn == pieceColor){
            if(pieceType == Piece.Pawn){
                legalMoves = GeneratePawnMoves(index, pieceColor, board, false, board.isCurrentPlayerInCheck);
            } else if(pieceType == Piece.Knight){
                legalMoves = GenerateKnightMoves(index, pieceColor, board, false, board.isCurrentPlayerInCheck);
            } else if(pieceType == Piece.Bishop){
                legalMoves = GenerateBishopMoves(index, pieceColor, board, false, board.isCurrentPlayerInCheck);
            } else if(pieceType == Piece.Rook){
                legalMoves = GenerateRookMoves(index, pieceColor, board, false, board.isCurrentPlayerInCheck);
            } else if(pieceType == Piece.Queen){
                legalMoves = GenerateQueenMoves(index, pieceColor, board, false, board.isCurrentPlayerInCheck);
            } else if(pieceType == Piece.King){
                legalMoves = GenerateKingMoves(index, pieceColor, board, false);
            }
        }
        return legalMoves;
    }

    //Returns all legal moves in a position
    public List<Move> GenerateLegalMoves(Board board, int pieceColor){
        
        List<Move> legalMoves = new List<Move>();
        int kingIndex = GetKingIndex(pieceColor, board);
        
        //Double check
        if(board.isCurrentPlayerInDoubleCheck){
            legalMoves = GenerateKingMoves(kingIndex, pieceColor, board, false);
            return legalMoves;
        }

        List<int> pawnIndexes = GetPosByPieceType(Piece.Pawn, pieceColor, board);
        List<int> bishopIndexes = GetPosByPieceType(Piece.Bishop, pieceColor, board);
        List<int> knightIndexes = GetPosByPieceType(Piece.Knight, pieceColor, board);
        List<int> rookIndexes = GetPosByPieceType(Piece.Rook, pieceColor, board);
        List<int> queenIndexes = GetPosByPieceType(Piece.Queen, pieceColor, board);
        
        for(int x = 0; x< pawnIndexes.Count; x++){legalMoves.AddRange(GeneratePawnMoves(pawnIndexes[x], pieceColor, board, false, board.isCurrentPlayerInCheck));}
        for(int x = 0; x< knightIndexes.Count; x++){ legalMoves.AddRange(GenerateKnightMoves(knightIndexes[x], pieceColor, board, false, board.isCurrentPlayerInCheck));}
        for(int x = 0; x< bishopIndexes.Count; x++){ legalMoves.AddRange(GenerateBishopMoves(bishopIndexes[x], pieceColor, board, false, board.isCurrentPlayerInCheck));}
        for(int x = 0; x< rookIndexes.Count; x++){ legalMoves.AddRange(GenerateRookMoves(rookIndexes[x], pieceColor, board, false, board.isCurrentPlayerInCheck));}
        for(int x = 0; x< queenIndexes.Count; x++){ legalMoves.AddRange(GenerateQueenMoves(queenIndexes[x], pieceColor, board, false, board.isCurrentPlayerInCheck));}
        legalMoves.AddRange(GenerateKingMoves(kingIndex, pieceColor, board, false));

        return legalMoves;
    }
    
    //Returns a 64 int long array, with 0 being safe and 1 being attacked
    public int[] GenerateAttackedSquares(int oppositeColor, Board board){
        int[] attackedSquares = new int[64];
        List<int> pawnIndexes = GetPosByPieceType(Piece.Pawn, oppositeColor, board);
        List<int> knightIndexes = GetPosByPieceType(Piece.Knight, oppositeColor, board);
        List<int> bishopIndexes = GetPosByPieceType(Piece.Bishop, oppositeColor, board);
        List<int> rookIndexes = GetPosByPieceType(Piece.Rook, oppositeColor, board);
        List<int> queenIndexes = GetPosByPieceType(Piece.Queen, oppositeColor, board);

        List<Move> possibleMoves = new List<Move>();
        for(int x = 0; x<pawnIndexes.Count; x++){
            possibleMoves.AddRange(GeneratePawnMoves(pawnIndexes[x], oppositeColor, board, true, false));
        }
        for(int x = 0; x<knightIndexes.Count; x++){
            possibleMoves.AddRange(GenerateKnightMoves(knightIndexes[x], oppositeColor, board, true, false));
        }
        for(int x = 0; x<bishopIndexes.Count; x++){
            possibleMoves.AddRange(GenerateBishopMoves(bishopIndexes[x], oppositeColor, board, true, false));
        }
        for(int x = 0; x<rookIndexes.Count; x++){
            possibleMoves.AddRange(GenerateRookMoves(rookIndexes[x], oppositeColor, board, true, false));
        }
        for(int x = 0; x<queenIndexes.Count; x++){
            possibleMoves.AddRange(GenerateQueenMoves(queenIndexes[x], oppositeColor, board, true, false));
        }
        possibleMoves.AddRange(GenerateKingMoves(GetKingIndex(oppositeColor, board), oppositeColor, board, true));
        
        for(int x = 0; x< possibleMoves.Count; x++){
            //If not already attacked
            if(attackedSquares[possibleMoves[x].newIndex] == 0){
                attackedSquares[possibleMoves[x].newIndex] = 1;
            }
        }
        

        return attackedSquares;
    }

    //Self explanatory
    public List<Move> GeneratePawnMoves(int index, int pieceColor, Board board, bool squaresAttacked, bool isInCheck){
        List<Move> legalMoves = new List<Move>();
        int dirVal = (pieceColor == Piece.White)? -1 : 1;
        
        if(!squaresAttacked){
            //1 square forward
            if(board.board[index + (8*dirVal)] == 0){
                legalMoves.Add(new Move(index, index  + (8*dirVal), false));
            }
            
            //2 moves forward
            if(dirVal == -1 && board.IndexToRank(index) == 2){
                if(board.board[index + (16*dirVal)] == 0 && board.board[index + (8*dirVal)] == 0){
                    legalMoves.Add(new Move(index, index  + (16*dirVal), false, 6));   
                }
            } else if (dirVal == 1 && board.IndexToRank(index) == 7){
                if(board.board[index + (16*dirVal)] == 0 && board.board[index + (8*dirVal)] == 0){
                    legalMoves.Add(new Move(index, index  + (16*dirVal), false, 6));
                }
            }
        }


        //Captures
        List<int> attackedIndexes = new List<int>();
        if(pieceColor == Piece.White){
            //On the right side
            if(board.IndexToFile(index) == 8){
                attackedIndexes.Add(index - 9);
            } 
            //On the right side
            else if(board.IndexToFile(index) == 1){
                attackedIndexes.Add(index -7);
            }
            //Not on the side
            else {
                attackedIndexes.Add(index -7);
                attackedIndexes.Add(index - 9);
            }

        } else if(pieceColor == Piece.Black){
            //On the right side
            if(board.IndexToFile(index) == 8){
                attackedIndexes.Add(index + 7);
            } 
            //On the right side
            else if(board.IndexToFile(index) == 1){
                attackedIndexes.Add(index + 9);
            }
            //Not on the side
            else {
                attackedIndexes.Add(index +7);
                attackedIndexes.Add(index + 9);
            }
        }

        //Loops through the squares where the pawn can capture
        for(int x = 0; x< attackedIndexes.Count; x++){
            //If there's a piece or we are getting the attacked squares
            if(board.board[attackedIndexes[x]] != 0 || squaresAttacked){
                //Regular capturing
                if(Piece.Color(board.board[attackedIndexes[x]]) != pieceColor || squaresAttacked){
                    legalMoves.Add(new Move(index, attackedIndexes[x], true, 0));
                }
            } //En passant
            else if(attackedIndexes[x] == board.enPassantIndex){
                int enPassantAttackedIndex = (pieceColor == Piece.White) ? (board.enPassantIndex + 8) : (board.enPassantIndex - 8);
                if(!InCheckAfterEnPassant(board, index, attackedIndexes[x], enPassantAttackedIndex)){
                    legalMoves.Add(new Move(index, attackedIndexes[x], true, 7));
                }
            }
        }

        if(isInCheck){
            legalMoves = PruneIllegalMoves(legalMoves, board.blockableIndexes);
        }

        if(board.pinnedPieceIndexes.Contains(index) && !squaresAttacked){
            int pinningPieceIndex = GetPinningPiece(index, board.pinnedIndexes);
            legalMoves = PruneIllegalMoves(legalMoves, BlockableIndexes(GetKingIndex(pieceColor, board), pinningPieceIndex, board));
        }

        //If this piece's next move is about to promote
        if(pieceColor == Piece.White && board.IndexToRank(index) == 7){List<Move> possibleMoves = AddPromotionsToList(legalMoves); legalMoves = possibleMoves;}
        else if(pieceColor == Piece.Black && board.IndexToRank(index) == 2){List<Move> possibleMoves = AddPromotionsToList(legalMoves); legalMoves = possibleMoves;}
        
        return legalMoves;
    }
    public List<Move> GenerateKnightMoves(int index, int pieceColor, Board board, bool squaresAttacked, bool isInCheck){
        List<Move> legalMoves = new List<Move>();
        List<int> legalMoveIndexes = new List<int>();
        int file = board.IndexToFile(index);
        int rank = board.IndexToRank(index);


        //Checks starting at bottom leftmost and going clockwise
        //Left edge check
        if(file > 2){
            if(rank > 1){
                legalMoveIndexes.Add(index + 6);
            }
            if(rank < 8){
                legalMoveIndexes.Add(index - 10);
            }
        }
        //Top check
        if(rank < 7){
            if(file > 1){
                legalMoveIndexes.Add(index - 17);
            }
            if(file < 8){
                legalMoveIndexes.Add(index - 15);
            }
        }
        //Right edge checks
        if(file < 7){
            if(rank < 8){
                legalMoveIndexes.Add(index - 6);
            }
            if(rank > 1){
                legalMoveIndexes.Add(index + 10);
            }
        }
        //Bottom check
        if(rank > 2){
            if(file < 8){
                legalMoveIndexes.Add(index + 17);
            }
            if(file > 1){
                legalMoveIndexes.Add(index + 15);
            }
        }
        for(int x = 0; x< legalMoveIndexes.Count; x++){
            bool isCapture = !Piece.IsColour(board.board[legalMoveIndexes[x]], pieceColor);
            //Not a capture
            if(board.board[legalMoveIndexes[x]] == 0 || squaresAttacked){legalMoves.Add(new Move(index, legalMoveIndexes[x], false)); }
            //Is a capture
            else if(isCapture || squaresAttacked){legalMoves.Add(new Move(index, legalMoveIndexes[x], isCapture));}
            //Other possibility: not a capture, but not an empty square = friendly piece
        }
        if(isInCheck){
            legalMoves = PruneIllegalMoves(legalMoves, board.blockableIndexes);
        }
        if(board.pinnedPieceIndexes.Contains(index) && !squaresAttacked){
            int pinningPieceIndex = GetPinningPiece(index, board.pinnedIndexes);
            legalMoves = PruneIllegalMoves(legalMoves, BlockableIndexes(GetKingIndex(pieceColor, board), pinningPieceIndex, board));
        }
        return legalMoves;
    }
    public List<Move> GenerateBishopMoves(int index, int pieceColor, Board board, bool squaresAttacked, bool isInCheck){
        List<Move> legalMoves = new List<Move>();
        //bottom right diag
        for(int currentIndex = index + 9; currentIndex < 64 && currentIndex > -1 && board.IndexToFile(currentIndex) > board.IndexToFile(index); currentIndex += 9){
            if(board.board[currentIndex] == 0){
                legalMoves.Add(new Move(index, currentIndex, false));
            //Capture
            } else if(!Piece.IsColour(board.board[currentIndex], pieceColor) || squaresAttacked){
                legalMoves.Add(new Move(index, currentIndex, true));
                //When checking attacked squares, control piece behind king
                if(!(squaresAttacked && Piece.PieceType(board.board[currentIndex]) == Piece.King && !Piece.IsColour(board.board[currentIndex], pieceColor))){
                    break;
                }
            //Friendly piece
            } else{break;}
        }
        
        //Bottom left diag
        for(int currentIndex = index + 7; currentIndex < 64 && currentIndex > -1 && board.IndexToFile(currentIndex) < board.IndexToFile(index); currentIndex += 7){
            if(board.board[currentIndex] == 0){
                legalMoves.Add(new Move(index, currentIndex, false));
            //Capture
            } else if(!Piece.IsColour(board.board[currentIndex], pieceColor) || squaresAttacked){
                legalMoves.Add(new Move(index, currentIndex, true));
                //When checking attacked squares, control piece behind king
                if(!(squaresAttacked && Piece.PieceType(board.board[currentIndex]) == Piece.King && !Piece.IsColour(board.board[currentIndex], pieceColor))){
                    break;
                }
            //Friendly piece
            } else{break;}
        }
        //Top right diag
        for(int currentIndex = index - 7; currentIndex < 64 && currentIndex > -1 && board.IndexToFile(currentIndex) > board.IndexToFile(index); currentIndex -= 7){
            if(board.board[currentIndex] == 0){
                legalMoves.Add(new Move(index, currentIndex, false));
            //Capture
            } else if(!Piece.IsColour(board.board[currentIndex], pieceColor) || squaresAttacked){
                legalMoves.Add(new Move(index, currentIndex, true));
                //When checking attacked squares, control piece behind king
                if(!(squaresAttacked && Piece.PieceType(board.board[currentIndex]) == Piece.King && !Piece.IsColour(board.board[currentIndex], pieceColor))){
                    break;
                }
            //Friendly piece
            } else{break;}
        }
        //Top left diag
        for(int currentIndex = index - 9; currentIndex < 64 && currentIndex > -1 && board.IndexToFile(currentIndex) < board.IndexToFile(index); currentIndex -= 9){
            if(board.board[currentIndex] == 0){
                legalMoves.Add(new Move(index, currentIndex, false));
            //Capture
            } else if(!Piece.IsColour(board.board[currentIndex], pieceColor) || squaresAttacked){
                legalMoves.Add(new Move(index, currentIndex, true));
                //When checking attacked squares, control piece behind king
                if(!(squaresAttacked && Piece.PieceType(board.board[currentIndex]) == Piece.King && !Piece.IsColour(board.board[currentIndex], pieceColor))){
                    break;
                }
            //Friendly piece
            } else{break;}
        }
        if(isInCheck){
            legalMoves = PruneIllegalMoves(legalMoves, board.blockableIndexes);
        }
        if(board.pinnedPieceIndexes.Contains(index) && !squaresAttacked){
            int pinningPieceIndex = GetPinningPiece(index, board.pinnedIndexes);
            legalMoves = PruneIllegalMoves(legalMoves, BlockableIndexes(GetKingIndex(pieceColor, board), pinningPieceIndex, board));
        }
        return legalMoves;
    }
    public List<Move> GenerateRookMoves(int index, int pieceColor, Board board, bool squaresAttacked, bool isInCheck){
        List<Move> legalMoves = new List<Move>();

        //Right
        if(board.IndexToFile(index) !=8){
            for(int currentIndex = index + 1; currentIndex < 64 && board.IndexToFile(currentIndex) != 1; currentIndex++){
                if(board.board[currentIndex] == 0){
                    legalMoves.Add(new Move(index, currentIndex, false));
                //Capture
                } else if(!Piece.IsColour(board.board[currentIndex], pieceColor) || squaresAttacked){
                    legalMoves.Add(new Move(index, currentIndex, true));
                    //When checking attacked squares, control piece behind king
                    if(!(squaresAttacked && Piece.PieceType(board.board[currentIndex]) == Piece.King && !Piece.IsColour(board.board[currentIndex], pieceColor))){
                        break;
                    }
                //Friendly piece
                } else{break;}
            }
        }
        //Left
        if(board.IndexToFile(index) != 1){
            for(int currentIndex = index - 1; board.IndexToFile(index) != 1 && currentIndex >-1 && board.IndexToFile(currentIndex) != 8 ; currentIndex--){
                if(board.board[currentIndex] == 0){
                    legalMoves.Add(new Move(index, currentIndex, false));
                //Capture
                } else if(!Piece.IsColour(board.board[currentIndex], pieceColor) || squaresAttacked){
                    legalMoves.Add(new Move(index, currentIndex, true));
                    //When checking attacked squares, control piece behind king
                    if(!(squaresAttacked && Piece.PieceType(board.board[currentIndex]) == Piece.King && !Piece.IsColour(board.board[currentIndex], pieceColor))){
                        break;
                    }
                //Friendly piece
                } else{break;}
            }
        }
        //Up
        if(board.IndexToRank(index) != 8){
            for(int currentIndex = index - 8;  currentIndex > -1; currentIndex -= 8){
                if(board.board[currentIndex] == 0){
                    legalMoves.Add(new Move(index, currentIndex, false));
                //Capture
                } else if(!Piece.IsColour(board.board[currentIndex], pieceColor) || squaresAttacked){
                    legalMoves.Add(new Move(index, currentIndex, true));
                    //When checking attacked squares, control piece behind king
                    if(!(squaresAttacked && Piece.PieceType(board.board[currentIndex]) == Piece.King && !Piece.IsColour(board.board[currentIndex], pieceColor)) ){
                        break;
                    }
                //Friendly piece
                } else{break;}
            }
        }
        //Down
        if(board.IndexToRank(index) != 1){
            for(int currentIndex = index + 8;  currentIndex < 64; currentIndex += 8){
                if(board.board[currentIndex] == 0){
                    legalMoves.Add(new Move(index, currentIndex, false));
                //Capture
                } else if(!Piece.IsColour(board.board[currentIndex], pieceColor) || squaresAttacked){
                    legalMoves.Add(new Move(index, currentIndex, true));
                    //When checking attacked squares, control piece behind opposite king
                    if(!(squaresAttacked && Piece.PieceType(board.board[currentIndex]) == Piece.King && !Piece.IsColour(board.board[currentIndex], pieceColor))){
                        break;
                    }
                //Friendly piece
                } else{break;}
            }
        }

        if(isInCheck){
            legalMoves = PruneIllegalMoves(legalMoves, board.blockableIndexes);
        }
        if(board.pinnedPieceIndexes.Contains(index) && !squaresAttacked){
            int pinningPieceIndex = GetPinningPiece(index, board.pinnedIndexes);
            legalMoves = PruneIllegalMoves(legalMoves, BlockableIndexes(GetKingIndex(pieceColor, board), pinningPieceIndex, board));
        }
        return legalMoves;
    }
    public List<Move> GenerateQueenMoves(int index, int pieceColor, Board board, bool squaresAttacked, bool isInCheck){
        List<Move> legalMoves = GenerateBishopMoves(index, pieceColor, board, squaresAttacked, isInCheck);
        List<Move> legalRookMoves = GenerateRookMoves(index, pieceColor, board, squaresAttacked, isInCheck);
        legalMoves.AddRange(legalRookMoves);
        return legalMoves;
    }
    public List<Move> GenerateKingMoves(int index, int pieceColor, Board board, bool squaresAttacked){
        List<int> potentialIndexes = new List<int>();
        List<Move> legalMoves = new List<Move>();
        int[] illegalSquares = new int[64];
        int oppositeColor = (pieceColor == Piece.White) ? Piece.Black : Piece.White;

        if(!squaresAttacked){
            illegalSquares = GenerateAttackedSquares(oppositeColor, board);
        }
        


        //Loads generic castling data
        bool canCastleShort = board.HasKingsideRight(pieceColor);
        bool canCastleLong = board.HasQueensideRight(pieceColor);

        //Castling
        if(!squaresAttacked && (canCastleLong || canCastleShort)){
            //Empty squares to the right and rook of the same color and not in check and square about to be moved to is not in check
            if(canCastleShort && board.board[index + 1] == 0 && board.board[index + 2] == 0 && board.board[index + 3] == (pieceColor | Piece.Rook) && !board.isCurrentPlayerInCheck && illegalSquares[index + 2] == 0 && illegalSquares[index + 1] == 0){
                legalMoves.Add(new Move(index, index + 2, false, 5));
            }
            //Empty squares to the left and rook of the same color and not in check and square about to be moved to is not in check
            if(canCastleShort && board.board[index - 1] == 0 && board.board[index - 2] == 0 && board.board[index - 3] == 0 && board.board[index - 4] == (pieceColor | Piece.Rook) && !board.isCurrentPlayerInCheck && illegalSquares[index - 2] == 0 && illegalSquares[index - 1] == 0){
                legalMoves.Add(new Move(index, index - 2, false, 5));
            }
        }
        

        int file = board.IndexToFile(index);
        int rank = board.IndexToRank(index);
        //Left side check
        if(file > 1){
            if(rank > 1){
                potentialIndexes.Add(index + 7);
            }
            if(rank < 8 ){
                potentialIndexes.Add(index - 9);
            }
            potentialIndexes.Add(index - 1);
        }
        //Right side check
        if(file < 8){
            if(rank > 1){
                potentialIndexes.Add(index + 9);
            }
            if(rank < 8 ){
                potentialIndexes.Add(index - 7);
            }
            potentialIndexes.Add(index + 1);
        }
        if(rank < 8){potentialIndexes.Add(index - 8);}
        if(rank > 1){potentialIndexes.Add(index + 8);}
        
        if(squaresAttacked){
            for(int x = 0; x< potentialIndexes.Count; x++){
                legalMoves.Add(new Move(index, potentialIndexes[x], false));
            }
            return legalMoves;

        }
        

        
        for(int x = 0; x< potentialIndexes.Count; x++){
            if(board.board[potentialIndexes[x]] == 0 && illegalSquares[potentialIndexes[x]] == 0){
                legalMoves.Add(new Move(index, potentialIndexes[x], false));
            } else if(!Piece.IsColour(board.board[potentialIndexes[x]], pieceColor) && illegalSquares[potentialIndexes[x]] == 0){
                legalMoves.Add(new Move(index, potentialIndexes[x], true));
            }

        }
        return legalMoves;
    }
    
    //Outputs list of indexes where said piece type is    
    public List<int> GetPosByPieceType(int pieceType, int pieceColor, Board board){
        List<int> pieces = new List<int>();
        for(int index = 0; index<64; index++){
            if(Piece.IsColour(board.board[index], pieceColor) && Piece.PieceType(board.board[index]) == pieceType){
                pieces.Add(index);
            }
        }
        return pieces;
    }

    //Changes the board to see what it would be like after en passant
    bool InCheckAfterEnPassant (Board board, int startSquare, int targetSquare, int epCapturedPawnSquare) {

		// Update board to reflect en-passant capture
        int movedPiece = board.board[startSquare];
		board.board[targetSquare] = board.board[startSquare];
		board.board[startSquare] = Piece.None;
        int capturedPiece = board.board[epCapturedPawnSquare];
		board.board[epCapturedPawnSquare] = Piece.None;

		bool inCheckAfterEpCapture = false;
        //Check if there are pieces checking the king
		if (KingCheckIndexes(Piece.Color(movedPiece), board).Count != 0) {
			inCheckAfterEpCapture = true;
		}

		// Undo change to board
		board.board[targetSquare] = Piece.None;
		board.board[startSquare] = movedPiece;
		board.board[epCapturedPawnSquare] = capturedPiece;
		return inCheckAfterEpCapture;
	}

    //Returns king's position
    public int GetKingIndex(int kingColor, Board board){
        for(int index = 0; index<64; index++){
            if(Piece.Color(board.board[index]) == kingColor && Piece.PieceType(board.board[index]) == Piece.King){
                return index;
            }
        }
        return 0;
    }

    //Returns list of pieces checking the king
    public List<int> KingCheckIndexes(int kingColor, Board board){
        int kingIndex = GetKingIndex(kingColor, board);
        List<int> kingCheckIndexes = new List<int>();
        List<Move> bishopChecks = GenerateBishopMoves(kingIndex, kingColor, board, false, false);
        List<Move> rookChecks = GenerateRookMoves(kingIndex, kingColor, board, false, false);
        List<Move> knightChecks = GenerateKnightMoves(kingIndex, kingColor, board, false, false);
        List<int> pawnCheckIndexes = new List<int>();
        if(kingColor == Piece.White){
            if(board.IndexToFile(kingIndex) != 1 && (kingIndex -7 > -1)){
                pawnCheckIndexes.Add(kingIndex - 7);
            }if(board.IndexToFile(kingIndex) != 8 && (kingIndex - 9 > -1)){
                pawnCheckIndexes.Add(kingIndex - 9);
            }
            
        } else{
            if(board.IndexToFile(kingIndex) != 1 && (kingIndex + 7 < 64)){
                pawnCheckIndexes.Add(kingIndex + 7);
            }if(board.IndexToFile(kingIndex) != 8 && (kingIndex + 9 <64)){
                pawnCheckIndexes.Add(kingIndex + 9);
            }
        }
        //Bishop
        for(int x = 0; x < bishopChecks.Count; x++)
        {
            int index = bishopChecks[x].newIndex;
            //Checks if there is a opposite color piece of type bishop or queen
            if(board.board[index] !=0 && !Piece.IsColour(board.board[index], kingColor) && (Piece.PieceType(board.board[index]) == Piece.Bishop || Piece.PieceType(board.board[index]) == Piece.Queen)){
                kingCheckIndexes.Add(index);
            }
        }
        //Rook
        for(int x = 0; x< rookChecks.Count; x++)
        {
            int index = rookChecks[x].newIndex;
            //Checks if there is a opposite color piece of type rook or queen
            if(board.board[index] !=0 && !Piece.IsColour(board.board[index], kingColor) && (Piece.PieceType(board.board[index]) == Piece.Rook || Piece.PieceType(board.board[index]) == Piece.Queen)){
                kingCheckIndexes.Add(index);
            }
        }
        //Knight
        for(int x = 0; x< knightChecks.Count; x++)
        {
            int index = knightChecks[x].newIndex;
            //Checks if there is a opposite color piece of type knight
            if(board.board[index] !=0 && !Piece.IsColour(board.board[index], kingColor) && Piece.PieceType(board.board[index]) == Piece.Knight){
                kingCheckIndexes.Add(index);
            }
        }
        //Pawn
        for(int x = 0; x< pawnCheckIndexes.Count; x++)
        {
            int index = pawnCheckIndexes[x];
            //Checks if there is a opposite color piece of type knight
            if(board.board[index] !=0 && !Piece.IsColour(board.board[index], kingColor) && Piece.PieceType(board.board[index]) == Piece.Pawn){
                kingCheckIndexes.Add(index);
            }
        }

        return kingCheckIndexes;
    }

    //Removes moves that don't block or remove a check if they are in check
    public List<Move> PruneIllegalMoves(List<Move> startingMoves, List<int> allowedIndexes){
        List<Move> allowedMoves = new List<Move>();
        for(int x = 0; x< startingMoves.Count; x++){
            if(allowedIndexes.Contains(startingMoves[x].newIndex)){
                allowedMoves.Add(startingMoves[x]);
            }
        }
        return allowedMoves; 
    }
    
    //Returns indexes that can be used to block a check or a pin
    public List<int> BlockableIndexes(int kingIndex, int attackingPieceIndex, Board board){
        List<int> blockableIndexes = new List<int> {attackingPieceIndex};  //checking piece can be captured
        int kingRank = board.IndexToRank(kingIndex);
        int kingFile = board.IndexToFile(kingIndex);
        int rank = board.IndexToRank(attackingPieceIndex);
        int file = board.IndexToFile(attackingPieceIndex);
        //Knight cant be blocked for checks, cant pin
        if(Piece.PieceType(board.board[attackingPieceIndex]) == Piece.Knight){
            return blockableIndexes;
        }
        else if(kingRank == rank){
            //Which direction to loop through
            if(kingIndex > attackingPieceIndex){
                for(int x = kingIndex - 1; x > attackingPieceIndex; x--){blockableIndexes.Add(x);}
            } else{
                for(int x = kingIndex + 1; x < attackingPieceIndex; x++){blockableIndexes.Add(x);}
            }
        }
        else if(kingFile == file){
            //Which direction to loop through
            if(kingIndex > attackingPieceIndex){
                for(int x = kingIndex - 8; x > attackingPieceIndex; x-=8){blockableIndexes.Add(x);}
            } else{
                for(int x = kingIndex + 8; x < attackingPieceIndex; x+=8){blockableIndexes.Add(x);}
            }
        }
        //Iterate diagonally on the king's right
        else if(kingFile < file){
            //Down
            if(kingRank < rank){
                for(int x = kingIndex - 7; x>attackingPieceIndex; x -= 7){blockableIndexes.Add(x);}
            }
            //Up
            if(kingRank > rank){
                for(int x = kingIndex + 9; x<attackingPieceIndex; x += 9){blockableIndexes.Add(x);}
            }

        //Diagonally to the kings left
        }else if(kingFile > file){
            //Down
            if(kingRank < rank){
                for(int x = kingIndex - 9; x>attackingPieceIndex; x -= 9){blockableIndexes.Add(x);}
            }
            //Up
            if(kingRank > rank){
                for(int x = kingIndex +7; x<attackingPieceIndex; x += 7){blockableIndexes.Add(x);}
            }
        }
        return blockableIndexes;
    }
    
    //Returns the indexes of all pinned pieces of the king's color
    public List<PinnedPair> PinnedIndexes(int kingIndex, Board board){
        List<PinnedPair> pinnedIndexes = new List<PinnedPair>();
        int file = board.IndexToFile(kingIndex);
        int rank = board.IndexToRank(kingIndex);
        int pieceIndex = -1;

        int friendlyColor = Piece.Color(board.board[kingIndex]); 
        int enemyColor = (friendlyColor == Piece.White) ? Piece.Black : Piece.White;

        //Iterating left
        if(file != 1){
            //Stores the friendly piece
            pieceIndex = -1;
            for(int checkedIndex = kingIndex - 1; checkedIndex > -1 && board.IndexToFile(checkedIndex) != 8; checkedIndex--){
                //if there is a piece
                if(board.board[checkedIndex] != 0){
                    //No piece already in this line
                    if(Piece.Color(board.board[checkedIndex]) == friendlyColor && pieceIndex == -1){
                        pieceIndex = checkedIndex;
                    } else if(Piece.Color(board.board[checkedIndex]) == friendlyColor && pieceIndex != -1){
                        break; // 2 friendly pieces found, not pinned
                    } else if(Piece.Color(board.board[checkedIndex]) == enemyColor && (Piece.PieceType(board.board[checkedIndex]) == Piece.Rook || Piece.PieceType(board.board[checkedIndex]) == Piece.Queen) && pieceIndex != -1){
                        pinnedIndexes.Add(new PinnedPair(pieceIndex, checkedIndex)); //Found a pinned piece on this line, added to the list
                        break;
                    } else if(Piece.Color(board.board[checkedIndex]) == enemyColor){
                        break;
                    }
                }
            }
        }

        //Iterating Right
        if(file != 8){
            //Stores the friendly piece
            pieceIndex = -1;
            for(int checkedIndex = kingIndex + 1; checkedIndex < 64 && board.IndexToFile(checkedIndex) != 1; checkedIndex++){
                //if there is a piece
                if(board.board[checkedIndex] != 0){
                    //No piece already in this line
                    if(Piece.Color(board.board[checkedIndex]) == friendlyColor && pieceIndex == -1){
                        pieceIndex = checkedIndex;
                    } else if(Piece.Color(board.board[checkedIndex]) == friendlyColor && pieceIndex != -1){
                        break; // 2 friendly pieces found, not pinned
                    } else if(Piece.Color(board.board[checkedIndex]) == enemyColor && (Piece.PieceType(board.board[checkedIndex]) == Piece.Rook || Piece.PieceType(board.board[checkedIndex]) == Piece.Queen) && pieceIndex != -1){
                        pinnedIndexes.Add(new PinnedPair(pieceIndex, checkedIndex)); //Found a pinned piece on this line, added to the list
                        break;
                    }else if(Piece.Color(board.board[checkedIndex]) == enemyColor){
                        break;
                    }
                }
            }
        }

        //Iterating up
        if(rank != 8){
            //Stores the friendly piece
            pieceIndex = -1;
            for(int checkedIndex = kingIndex - 8; checkedIndex > -1; checkedIndex -= 8){
                //if there is a piece
                if(board.board[checkedIndex] != 0){
                    //No piece already in this line
                    if(Piece.Color(board.board[checkedIndex]) == friendlyColor && pieceIndex == -1){
                        pieceIndex = checkedIndex;
                    } else if(Piece.Color(board.board[checkedIndex]) == friendlyColor && pieceIndex != -1){
                        break; // 2 friendly pieces found, not pinned
                    } else if(Piece.Color(board.board[checkedIndex]) == enemyColor && (Piece.PieceType(board.board[checkedIndex]) == Piece.Rook || Piece.PieceType(board.board[checkedIndex]) == Piece.Queen) && pieceIndex != -1){
                        pinnedIndexes.Add(new PinnedPair(pieceIndex, checkedIndex)); //Found a pinned piece on this line, added to the list
                        break;
                    }else if(Piece.Color(board.board[checkedIndex]) == enemyColor){
                        break;
                    }
                }
            }
        }

        //Iterating down
        if(rank != 1){
            //Stores the friendly piece
            pieceIndex = -1;
            for(int checkedIndex = kingIndex + 8; checkedIndex < 64; checkedIndex += 8){
                //if there is a piece
                if(board.board[checkedIndex] != 0){
                    //No piece already in this line
                    if(Piece.Color(board.board[checkedIndex]) == friendlyColor && pieceIndex == -1){
                        pieceIndex = checkedIndex;
                    } else if(Piece.Color(board.board[checkedIndex]) == friendlyColor && pieceIndex != -1){
                        break; // 2 friendly pieces found, not pinned
                    } else if(Piece.Color(board.board[checkedIndex]) == enemyColor && (Piece.PieceType(board.board[checkedIndex]) == Piece.Rook || Piece.PieceType(board.board[checkedIndex]) == Piece.Queen) && pieceIndex != -1){
                        pinnedIndexes.Add(new PinnedPair(pieceIndex, checkedIndex)); //Found a pinned piece on this line, added to the list
                        break;
                    } else if(Piece.Color(board.board[checkedIndex]) == enemyColor){
                        break;
                    }
                }
            }
        }


        pieceIndex = -1;
        //ALL STOLEN FROM BISHOP CODE, BLAME PAST SPENCER WHEN IT DOESNT WORK
        //Bottom right diag
        for(int checkedIndex = kingIndex + 9; checkedIndex < 64 && board.IndexToFile(checkedIndex) > board.IndexToFile(kingIndex); checkedIndex += 9){
            //if there is a piece
            if(board.board[checkedIndex] != 0){
                //No piece already in this line
                if(Piece.Color(board.board[checkedIndex]) == friendlyColor && pieceIndex == -1){
                    pieceIndex = checkedIndex;
                } else if(Piece.Color(board.board[checkedIndex]) == friendlyColor && pieceIndex != -1){
                    break; // 2 friendly pieces found, not pinned
                } else if(Piece.Color(board.board[checkedIndex]) == enemyColor && (Piece.PieceType(board.board[checkedIndex]) == Piece.Bishop || Piece.PieceType(board.board[checkedIndex]) == Piece.Queen) && pieceIndex != -1){
                    pinnedIndexes.Add(new PinnedPair(pieceIndex, checkedIndex)); //Found a pinned piece on this line, added to the list
                    break;
                } else if(Piece.Color(board.board[checkedIndex]) == enemyColor){
                        break;
                }
            }
        }
        
        pieceIndex = -1;
        //Bottom left diag
        for(int checkedIndex = kingIndex + 7; checkedIndex < 64 && board.IndexToFile(checkedIndex) < board.IndexToFile(kingIndex); checkedIndex += 7){
            //if there is a piece
            if(board.board[checkedIndex] != 0){
                //No piece already in this line
                if(Piece.Color(board.board[checkedIndex]) == friendlyColor && pieceIndex == -1){
                    pieceIndex = checkedIndex;
                } else if(Piece.Color(board.board[checkedIndex]) == friendlyColor && pieceIndex != -1){
                    break; // 2 friendly pieces found, not pinned
                } else if(Piece.Color(board.board[checkedIndex]) == enemyColor && (Piece.PieceType(board.board[checkedIndex]) == Piece.Bishop || Piece.PieceType(board.board[checkedIndex]) == Piece.Queen) && pieceIndex != -1){
                    pinnedIndexes.Add(new PinnedPair(pieceIndex, checkedIndex)); //Found a pinned piece on this line, added to the list
                    break;
                } else if(Piece.Color(board.board[checkedIndex]) == enemyColor){
                        break;
                }
            }   
        }

        pieceIndex = -1;
        //Top right diag
        for(int checkedIndex = kingIndex - 7; checkedIndex > -1 && board.IndexToFile(checkedIndex) > board.IndexToFile(kingIndex); checkedIndex -= 7){
            //if there is a piece
            if(board.board[checkedIndex] != 0){
                //No piece already in this line
                if(Piece.Color(board.board[checkedIndex]) == friendlyColor && pieceIndex == -1){
                    pieceIndex = checkedIndex;
                } else if(Piece.Color(board.board[checkedIndex]) == friendlyColor && pieceIndex != -1){
                    break; // 2 friendly pieces found, not pinned
                } else if(Piece.Color(board.board[checkedIndex]) == enemyColor && (Piece.PieceType(board.board[checkedIndex]) == Piece.Bishop || Piece.PieceType(board.board[checkedIndex]) == Piece.Queen) && pieceIndex != -1){
                    pinnedIndexes.Add(new PinnedPair(pieceIndex, checkedIndex)); //Found a pinned piece on this line, added to the list
                    break;
                } else if(Piece.Color(board.board[checkedIndex]) == enemyColor){
                        break;
                }
            }   
        }
        
        pieceIndex = -1;
        //Top left diag
        for(int checkedIndex = kingIndex - 9; checkedIndex > -1 && board.IndexToFile(checkedIndex) < board.IndexToFile(kingIndex); checkedIndex -= 9){
            //if there is a piece
            if(board.board[checkedIndex] != 0){
                //No piece already in this line
                if(Piece.Color(board.board[checkedIndex]) == friendlyColor && pieceIndex == -1){
                    pieceIndex = checkedIndex;
                } else if(Piece.Color(board.board[checkedIndex]) == friendlyColor && pieceIndex != -1){
                    break; // 2 friendly pieces found, not pinned
                } else if(Piece.Color(board.board[checkedIndex]) == enemyColor && (Piece.PieceType(board.board[checkedIndex]) == Piece.Bishop || Piece.PieceType(board.board[checkedIndex]) == Piece.Queen) && pieceIndex != -1){
                    pinnedIndexes.Add(new PinnedPair(pieceIndex, checkedIndex)); //Found a pinned piece on this line, added to the list
                    break;
                } else if(Piece.Color(board.board[checkedIndex]) == enemyColor){
                        break;
                }
            }   
        }
        
        return pinnedIndexes;
    }
    
    //Adds all of the different types of promotions (knight, rook etc)
    public List<Move> AddPromotionsToList(List<Move> moves){
        List<Move> promoteMoves = new List<Move>();
        Move promoteToKnight;
        Move promoteToQueen;
        Move promoteToBishop;
        Move promoteToRook;

        //Loops through each move and creates a move for each piece type
        for(int x = 0; x< moves.Count; x++){
            promoteToKnight = new Move(moves[x].oldIndex, moves[x].newIndex, moves[x].isCapture(), 3);
            promoteToQueen = new Move(moves[x].oldIndex, moves[x].newIndex, moves[x].isCapture(), 1);
            promoteToBishop = new Move(moves[x].oldIndex, moves[x].newIndex, moves[x].isCapture(), 2);
            promoteToRook = new Move(moves[x].oldIndex, moves[x].newIndex, moves[x].isCapture(), 4);
            promoteMoves.Add(promoteToKnight);
            promoteMoves.Add(promoteToBishop);
            promoteMoves.Add(promoteToQueen);
            promoteMoves.Add(promoteToRook);
        }
        //Returns the promotion options
        return promoteMoves;

    }

    //Takes the pinned piece index and the list of pairs and returns the corresponding pinning piece
    public int GetPinningPiece(int pinnedPieceIndex, List<PinnedPair> pinnedPairs){
        for(int x = 0; x< pinnedPairs.Count(); x++){
            if(pinnedPairs[x].PinnedPiece == pinnedPieceIndex){
                return pinnedPairs[x].PinningPiece;
            }
        }
        return -1;
    }
}


//Stores the pinned piece and which piece is pinning it
public struct PinnedPair{
    public PinnedPair(int pinnedPieceIndex, int pinningPieceIndex){
        PinnedPiece = pinnedPieceIndex;
        PinningPiece = pinningPieceIndex;
    }
    public int PinnedPiece{ get;}
    public int PinningPiece{ get;}
}