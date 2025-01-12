using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Board
{

    public bool isCurrentPlayerInCheck;
    public bool isCurrentPlayerInDoubleCheck;

    public List<int> checkingPieces = new List<int>();
    public List<int> blockableIndexes = new List<int>();
    public List<PinnedPair> pinnedIndexes = new List<PinnedPair>();
    public List<int> pinnedPieceIndexes = new List<int>();
    public int colorTurn;
    

    //TESTING PURPOSES ONLY
    Stack<Move> moveHistory = new Stack<Move>();
    public Move lastMove;

    //Saves the index where a pawn can capture
    public int enPassantIndex;
    public int fiftyMoveCounter;
    public MoveGenerator moveGenerator;
    BoardManager boardManager;
    public int[] board;
    //White = 1, Black = 2

    //Stores castling rights, en passant square, and captured piece
    //Necessary to be able to unmake moves multiple times
    //Bits from L to R: whiteKingside, whiteQueenside, blackKingside, blackQueenside, en passant file (next 4), 50 move counter for the rest
    Stack<uint> gameStateHistory = new Stack<uint>();
    public uint currentGameState;

    //Constructor
    public Board(string fenPosition, MoveGenerator generator){
        moveGenerator = generator;
        board = ConvertFromFEN(fenPosition);
        UpdateCheckingInfo();
        UpdatePinnedInfo();
    }

    //Moves the pieces
    public void Move(Move move, bool isSearch){
        int castlingRights = GetCastlingRights(gameStateHistory.Peek());
        int enPassantFile = 0;
        //Captured piece gets removed, ep file gets removed, need to save castling rights and fiftymoverule
        currentGameState = 0;

        int startPos = move.oldIndex;
        int newPos = move.newIndex;
        int movedPiece;
        int capturedPiece = 0;


        //Testing only
        lastMove = move;
        moveHistory.Push(lastMove);

        //Set to none 
        enPassantIndex = -1;

        if(move.isPromotion()){
            fiftyMoveCounter = 0;
            //Sets the new piece to be the same color but whatever the new piece type is
            movedPiece = Piece.Color(board[startPos]) | move.PromotedPieceType();
        } else {
            movedPiece = board[startPos];
        }

        //castle
        if(move.flag == 5){
            fiftyMoveCounter++;
            //Once castles, you can't castle again
            if(colorTurn == Piece.White) {
                //Removing castling rights
                castlingRights &= 0b1100;
                //Short castles
                if(newPos == 62){
                    int rook = board[63];
                    board[61] = rook;
                    board[63] = 0;
                } 
                //Long castles
                else if(newPos == 58){
                    int rook = board[56];
                    board[59] = rook;
                    board[56] = 0;
                }
                board[newPos] = movedPiece;
                board[startPos] = 0;
            }
            else if(colorTurn == Piece.Black){
                //Removing castling rights
                castlingRights &= 0b0011;
                //Short castles
                if(newPos == 6){
                    int rook = board[7];
                    board[5] = rook;
                    board[7] = 0;
                } 
                //Long castles
                else if(newPos == 2){
                    int rook = board[0];
                    board[3] = rook;
                    board[0] = 0;
                }
                board[newPos] = movedPiece;
                board[startPos] = 0;
            }

        }
        //en passant
        else if(move.flag == 7){
            
            //capture
            board[newPos] = movedPiece;
            board[startPos] = 0;
            int attackedPawnIndex = (colorTurn == Piece.White) ? newPos+8:newPos-8;
            capturedPiece = board[attackedPawnIndex];
            board[attackedPawnIndex] = 0;
            fiftyMoveCounter = 0;
        }
        else{
            
            //Double pawn push
            if(move.flag == 6){
                //Set to the square behind the spot moved to
                enPassantIndex = (colorTurn == Piece.White) ? (newPos + 8):(newPos - 8);
                enPassantFile = IndexToFile(startPos);
            }
            
            //Once the king has been moved, you can't castle
            if(Piece.PieceType(movedPiece) == Piece.King){
                if(colorTurn == Piece.White) {castlingRights &= 0b1100;}
                else if(colorTurn == Piece.Black){castlingRights &= 0b0011;}
            }

            //If it's a rook move, check if its from the starting square to remove castling perms
            if(colorTurn == Piece.White){
                if(Piece.PieceType(movedPiece) == Piece.Rook){
                    if(startPos == 56){castlingRights &=  0b1101;}
                    //Short side
                    else if(startPos == 63) {castlingRights &= 0b1110;}
                }
            } 
            else if(colorTurn == Piece.Black){
                if(Piece.PieceType(movedPiece) == Piece.Rook){
                    if(startPos == 0){castlingRights &=  0b0111;}
                    //Short side
                    else if(startPos == 7) {castlingRights &=  0b1011;}
                }
            }
            
            //Make the move
            if(move.isCapture()){
                //capture
                capturedPiece = board[newPos];
                
                board[newPos] = movedPiece;
                board[startPos] = 0;

                if(Piece.PieceType(capturedPiece) == Piece.King){
                    Debug.Log("King captured");

                    Debug.Log(move.oldIndex + " " + move.newIndex + " " + move.isCapture());
                }

                //Capture resets counter
                fiftyMoveCounter = 0;
            } else{
                board[newPos] = movedPiece;
                board[startPos] = 0;
                //Pawn push resets counter
                if(Piece.PieceType(movedPiece) == Piece.Pawn){
                    fiftyMoveCounter = 0;
                } else{
                    fiftyMoveCounter ++;
                }
            }
        }

        int oppositeColor = colorTurn;
        colorTurn = (colorTurn == Piece.White)? Piece.Black : Piece.White;
        //Adds captured piece to gamestate
        currentGameState = currentGameState | (uint) castlingRights;
        currentGameState = currentGameState | (uint) enPassantFile  << 4;
        currentGameState = currentGameState | (uint) capturedPiece << 8;
        currentGameState = currentGameState | (uint) fiftyMoveCounter << 13;
        
        gameStateHistory.Push(currentGameState);

        UpdateCheckingInfo();
        UpdatePinnedInfo();   
    }

    public void UndoMove(Move move){
        colorTurn = (colorTurn == Piece.White)? Piece.Black : Piece.White;
        //Removing the current one and getting the required info
        uint oldGameStateHistory = gameStateHistory.Pop();

        //Setting the game state to the previous one without removing it
        currentGameState = gameStateHistory.Peek();
        int capturedPiece = (int)((oldGameStateHistory >> 8) & 0b011111);
        int enPassantFile = (int)((currentGameState >> 4) & 0b01111);

        fiftyMoveCounter = (int) (currentGameState >> 13);

        //Setting the ep index to what it used to be
        enPassantIndex = (colorTurn == Piece.White) ? (15 + enPassantFile) : (39 + enPassantFile);

        //Testing only
        moveHistory.Pop();
        if(moveHistory.Count == 0){
            lastMove = null;
        }
        else{
            lastMove = moveHistory.Peek();
        }

        int startPos = move.oldIndex;
        int newPos = move.newIndex;
        int movedPiece;

        if(move.isPromotion()){
            fiftyMoveCounter = 0;
            //Sets the new piece to be the same color but whatever the new piece type is
            movedPiece = Piece.Color(board[newPos]) | Piece.Pawn;
        } else {
            movedPiece = board[newPos];
        }

        //castle
        if(move.flag == 5){
            //Undo the castles
            if(colorTurn == Piece.White) {
                //short castles
                if(newPos == 62){
                    int rook = board[61];
                    board[63] = rook;
                    board[61] = 0;
                } 
                //Long castles
                else if(newPos == 58){
                    int rook = board[59];
                    board[56] = rook;
                    board[59] = 0;
                }
                board[startPos] = movedPiece;
                board[newPos] = 0;
            }
            else if(colorTurn == Piece.Black){
                //Short castles
                if(newPos == 6){
                    int rook = board[5];
                    board[7] = rook;
                    board[5] = 0;
                } 
                //Long castles
                else if(newPos == 2){
                    int rook = board[3];
                    board[0] = rook;
                    board[3] = 0;
                }
                board[startPos] = movedPiece;
                board[newPos] = 0;
            }

        }
        //en passant
        else if(move.flag == 7){
            //capture
            board[startPos] = movedPiece;
            board[newPos] = 0;
            
            //Replacing the captured pawn
            int attackedPawnIndex = (colorTurn == Piece.White) ? newPos+8:newPos-8;
            board[attackedPawnIndex] = capturedPiece;
        }
        else{
            //Undo the move
            if(move.isCapture()){
                //capture
                board[startPos] = movedPiece;
                board[newPos] = capturedPiece;
            } else{
                board[startPos] = movedPiece;
                board[newPos] = 0;
            }
        }
        UpdateCheckingInfo();
        UpdatePinnedInfo();
    }

    public int[] ConvertFromFEN(string fenPosition){
        currentGameState = 0;
        Dictionary<char, int> pieceTypeFromSymbol = new Dictionary<char, int>(){
            ['k'] = Piece.King, ['p'] = Piece.Pawn, ['n'] = Piece.Knight, ['b'] = Piece.Bishop, ['r'] = Piece.Rook, ['q'] = Piece.Queen
        };
        int[] position = new int[64];

        //Part denoting position of each piece
        string posString = fenPosition.Split(' ')[0];
        int index = 0;
        foreach(char c in posString){
            if(c == '/'){
                //ignore
            } 
            else{
                
                if(Char.IsLetter(c)){
                    int pieceColour = Char.IsUpper(c) ? Piece.White : Piece.Black;
                    int pieceType = pieceTypeFromSymbol[char.ToLower(c)];
                    position[index] = pieceType | pieceColour;
                    index ++;
                } 
                else if(Char.IsNumber(c)){
                    for(int x = 0; x< Char.GetNumericValue(c); x ++){
                        position[index] = 0;
                        index ++;
                    }
                }
            }
            
        }

        //Loads who's move it is
        string sidetoMove = fenPosition.Split(' ')[1];  
        if(sidetoMove == "w"){colorTurn = Piece.White;}
        else if(sidetoMove == "b"){colorTurn = Piece.Black;}

        string castling = fenPosition.Split(' ')[2];

        //TODO
        int castlingRights = 0;
        if(castling.Contains("K")){castlingRights += 1;}
        if(castling.Contains("Q")){castlingRights += 2;}
        if(castling.Contains("k")){castlingRights += 4;}
        if(castling.Contains("q")){castlingRights += 8;}
        currentGameState |= (uint) castlingRights;
        gameStateHistory.Push(currentGameState);
        return position;
    }
    
    //TODO: repetition
    public bool IsDraw(){
        //Stalemate
        if(moveGenerator.GenerateLegalMoves(this, colorTurn).Count == 0 && !isCurrentPlayerInCheck){return true;}
        //50 move rule
        if(fiftyMoveCounter >= 100){return true;}

        //Can be improved with dedicated function
        int numQueens = moveGenerator.GetPosByPieceType(Piece.Queen, Piece.Black, this).Count + moveGenerator.GetPosByPieceType(Piece.Queen, Piece.White, this).Count;
        int numWhiteBishop = moveGenerator.GetPosByPieceType(Piece.Bishop, Piece.White, this).Count;
        int numBlackBishop = moveGenerator.GetPosByPieceType(Piece.Bishop, Piece.Black, this).Count;
        int numWhiteKnight = moveGenerator.GetPosByPieceType(Piece.Knight, Piece.White, this).Count;
        int numBlackKnight = moveGenerator.GetPosByPieceType(Piece.Knight, Piece.Black, this).Count;
        int numRook = moveGenerator.GetPosByPieceType(Piece.Rook, Piece.Black, this).Count + moveGenerator.GetPosByPieceType(Piece.Rook, Piece.White, this).Count;
        int numPawn = moveGenerator.GetPosByPieceType(Piece.Pawn, Piece.Black, this).Count + moveGenerator.GetPosByPieceType(Piece.Pawn, Piece.White, this).Count;

        //Insufficient material
        if(numPawn > 0 || numQueens >0 || numRook > 0){return false;}
        else if((numWhiteBishop + numWhiteKnight) > 1 || (numBlackBishop + numBlackKnight) > 1){return false;}
        else{
            return true;
        }
    }

    public bool IsCheckmate(int color){
        int kingIndex = moveGenerator.GetKingIndex(color, this);
        if(isCurrentPlayerInCheck){
            //If there are any valid king moves
            if(moveGenerator.GenerateKingMoves(kingIndex, color, this, false).Count != 0){
                return false;
            }
            //check if there are any blocks
            else{
                if(moveGenerator.GenerateLegalMoves(this, color).Count == 0){return true;}
                else{return false;} 
            }
        } else {
            return false;
        }
    }

    //More efficient than checking multiple times for checks
    public void UpdateCheckingInfo(){
        checkingPieces = moveGenerator.KingCheckIndexes(colorTurn, this);
        if(checkingPieces.Count > 1)
        {
            blockableIndexes.Clear();
            isCurrentPlayerInCheck = true;
            isCurrentPlayerInDoubleCheck = true;
        } else if(checkingPieces.Count == 1){
            isCurrentPlayerInCheck = true;
            isCurrentPlayerInDoubleCheck = false;
            blockableIndexes = moveGenerator.BlockableIndexes(moveGenerator.GetKingIndex(colorTurn, this), checkingPieces[0], this);
        } else{
            isCurrentPlayerInCheck = false;
            isCurrentPlayerInDoubleCheck = false;
            blockableIndexes.Clear();
        }
        
    }
    
    //More efficient than checking multiple times for pinned pieces
    public void UpdatePinnedInfo(){
        pinnedIndexes.Clear();
        pinnedIndexes = moveGenerator.PinnedIndexes(moveGenerator.GetKingIndex(colorTurn, this), this);
        pinnedPieceIndexes.Clear();
        for(int x = 0; x < pinnedIndexes.Count; x++){
            pinnedPieceIndexes.Add(pinnedIndexes[x].PinnedPiece);
        }
    }
    
    //Utilities
    public int IndexToRank(int index){
        if(index <=7 && index >= 0){
            return 8;
        } else if(index <=15 && index > 7){
            return 7;
        } else if(index <=23 && index > 15){
            return 6;
        } else if(index <=31 && index > 23){
            return 5;
        } else if(index <=39 && index > 31){
            return 4;
        } else if(index <=47 && index > 39){
            return 3;
        } else if(index <=55 && index > 47){
            return 2;
        } else if(index <=63 && index > 55){
            return 1;
        } else{
            Debug.Log("index out of range ");
            return 0;
        }
    }

    public int IndexToFile(int index){
        int rank = IndexToRank(index);
        int file = index - ((8 - rank)*8) + 1;
        return file;
    }

    public int RankFileToIndex(int file, int rank){
        int index = ((8 - rank) * 8) + file-1;
        return index;
    }

    void DebugGameState(uint gameState){
        Debug.Log("Castling: " + (gameState & 0b1111).ToString());
        Debug.Log("Captured piece type: " + ((gameState & 0b1111100000000)>>8).ToString());
        Debug.Log("EP file: " + ((gameState & 0b11110000) >>4).ToString());
        Debug.Log("50 move rule: " + ((gameState & 0b11111111111110000000000000) >>13).ToString());
    }

    //Gets them from the current gamestate
    public bool HasKingsideRight(int color){
        if(color == Piece.White && (currentGameState & 0b0000000000001) == 1){
            return true;
        } else if(color == Piece.Black && (currentGameState & 0b0000000000100) == 4){
            return true;
        } else{return false;}
    }
    public bool HasQueensideRight(int color){
        if(color == Piece.White && (currentGameState & 0b000010) == 2){
            return true;
        } else if(color == Piece.Black && (currentGameState & 0b001000) == 8){
            return true;
        } else{return false;}
    }

    //Returns the int in the form of the gamestate
    public int GetCastlingRights(uint gameState){
        return (int) (gameState & 0b001111);
    }
    public int EnPassantFileToIndex(int pieceColor, int epFile){
        if(pieceColor == Piece.White){
            return 39 + epFile;
        } else{
            return 15 + epFile;
        }
    }
}
