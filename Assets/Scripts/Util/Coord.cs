using System;
using System.Collections.Generic;


public static class Coord
{
    public static String GetNotationFromIndex(int index){
        string notation;
        int rank = IndexToRank(index);
        int fileInt = IndexToFile(index);
        char file;
        switch (fileInt){
            case 1: file = 'a'; break;
            case 2: file = 'b'; break;
            case 3: file = 'c'; break;
            case 4: file = 'd'; break;
            case 5: file = 'e'; break;
            case 6: file = 'f'; break;
            case 7: file = 'g'; break;
            case 8: file = 'h'; break;
            default: file = 'x'; break;
        }
        notation = file.ToString() + rank.ToString();
        return notation;
    }

    public static String GetMoveNotation(int index1, int index2){
        string start = GetNotationFromIndex(index1);
        string end = GetNotationFromIndex(index2);

        return start + " " + end;
    }

    public static int IndexToRank(int index){
        return 8 - ((index - (index % 8)) / 8);
    }

    public static int IndexToFile(int index){
        int file = index % 8 + 1;
        return file;
    }

    public static int NotationToIndex(string notation){
        int rank = (int)Char.GetNumericValue(notation[1]);
        int file = LetterToFile(notation[0]);
        return ((8 - rank) * 8) + (file - 1);
    }

    public static int LetterToFile(char letter){
        int file = 0;
        switch (letter){
            case 'a': file = 1; break;
            case 'b': file = 2; break;
            case 'c': file = 3; break;
            case 'd': file = 4; break;
            case 'e': file = 5; break;
            case 'f': file = 6; break;
            case 'g': file = 7; break;
            case 'h': file = 8; break;
            default:  break;
        }
        return file;
    }

    public static Move convertPGNMove(Board board, string strMove, int lineNum){
        string ogMove = strMove;
        //Removing check/mate indicator
        if (strMove[strMove.Length - 1] == '+' || strMove[strMove.Length - 1] == '#'){
            strMove = strMove.Substring(0, strMove.Length - 1);
        }
        //Castling
        if (strMove == "O-O"){
            if(board.colorTurn == Piece.White){
                return new Move(60, 62, false, 5);
            } else{
                return new Move(4, 6, false, 5);
            }

        } 
        else if (strMove == "O-O-O"){
            if(board.colorTurn == Piece.White){
                return new Move(60, 58, false, 5);
            } else{
                return new Move(4, 2, false, 5);
            }
        }
        else {
            //Extracting the end point
            int endIndex = NotationToIndex(strMove.Substring(strMove.Length - 2, 2));
            bool isCapture = false;
            int flag = 0;
            int startIndex = 0;
            //Not just a basic pawn move
            if (strMove.Length != 2){
                strMove = strMove.Substring(0, strMove.Length - 2);
                //Capture
                if (strMove[strMove.Length - 1] == 'x'){
                    isCapture = true;
                    //Remove the 'x' from the capture
                    strMove = strMove.Substring(0, strMove.Length - 1);
                }

                //Not a pawn capture
                if(Char.IsUpper(strMove[0])){
                    char pieceType = strMove[0];
                    int pieceNum = 0; 
                    List<Move> validMoves = new List<Move>();
                    switch (pieceType){
                        case 'N': validMoves = board.moveGenerator.GenerateKnightMoves(endIndex, board.colorTurn, board, true, false); pieceNum = Piece.Knight; break;
                        case 'B': validMoves = board.moveGenerator.GenerateBishopMoves(endIndex, board.colorTurn, board, true, false); pieceNum = Piece.Bishop; break;
                        case 'Q': validMoves = board.moveGenerator.GenerateQueenMoves(endIndex, board.colorTurn, board, true, false); pieceNum = Piece.Queen; break;
                        case 'R': validMoves = board.moveGenerator.GenerateRookMoves(endIndex, board.colorTurn, board, true, false); pieceNum = Piece.Rook; break;
                        case 'K': validMoves = board.moveGenerator.GenerateKingMoves(endIndex, board.colorTurn, board, true); pieceNum = Piece.King; break;
                    }
                    List<int> possibleStartIndexes = new List<int>();

                    for (int x = 0; x< validMoves.Count; x++){
                        //There is the correct piece + color at that square, this was a possible starting point
                        if(board.board[validMoves[x].newIndex] == (board.colorTurn | pieceNum)){
                            possibleStartIndexes.Add(validMoves[x].newIndex);
                        }
                    }

                    //Just the piece type remaining
                    if(strMove.Length == 1 && possibleStartIndexes.Count == 1){
                        startIndex = possibleStartIndexes[0];
                    } else if (strMove.Length > 1){
                        //Remove the piece type from the start, all that is left is the extra info for which piece it is
                        strMove = strMove.Substring(1, strMove.Length - 1);

                        //Doubley disambiguated, only one square it can be at
                        if (strMove.Length == 2){
                            startIndex = NotationToIndex(strMove);
                        } 
                        //Rank disambiguated
                        else if (Char.IsDigit(strMove[0])){
                            for (int x = 0; x< possibleStartIndexes.Count; x++){
                                if (IndexToRank(possibleStartIndexes[x]) == (int)Char.GetNumericValue(strMove[0])){
                                    if (startIndex == 0){
                                        startIndex = possibleStartIndexes[x];
                                    } else{
                                        //Debug.Log("Rank Disambiguated but multiple options");
                                    } 
                                }
                            }
                        } 
                        //File disambiguated
                        else if (!Char.IsDigit(strMove[0])){
                            for (int x = 0; x< possibleStartIndexes.Count; x++){
                                if (IndexToFile(possibleStartIndexes[x]) == LetterToFile(strMove[0])){
                                    if (startIndex == 0){
                                        startIndex = possibleStartIndexes[x];
                                    } else{
                                        //Debug.Log("File Disambiguated but multiple options: " + ogMove + " " + strMove + "Line: " + lineNum.ToString());
                                    } 
                                }
                            }
                        }

                    } else if (strMove.Length == 1 && possibleStartIndexes.Count > 1){
                        for(int x = 0; x< possibleStartIndexes.Count; x++){
                            if (board.pinnedPieceIndexes.Contains(possibleStartIndexes[x])){
                                possibleStartIndexes.RemoveAt(x);
                            }
                        }
                        if (possibleStartIndexes.Count == 1) {
                            startIndex = possibleStartIndexes[0];
                        } else {
                            //Debug.Log("Disambiguated but no extra info: " + ogMove + " " + strMove + "Line: " + lineNum.ToString());
                            //Debug.Log(possibleStartIndexes.Count);
                        }
                    } 
                    else{
                        //Debug.Log("No Valid Moves found: " + ogMove + " " + strMove + "Line: " + lineNum.ToString());
                        //Debug.Log(possibleStartIndexes.Count);
                    }
                }

                //Pawn Capture
                else {
                    //No piece at the square it captured, en passant
                    if (board.board[endIndex] == 0){
                        flag = 7;
                    }
                    int oldFile = LetterToFile(strMove[0]);
                    int newFile = IndexToFile(endIndex);
                    if (board.colorTurn == Piece.White){
                        if (oldFile > newFile){
                            startIndex = endIndex + 9;
                        }
                        else {
                            startIndex = endIndex + 7;
                        }
                    } 
                    else {
                        if (oldFile > newFile){
                            startIndex = endIndex - 7;
                        }
                        else {
                            startIndex = endIndex - 9;
                        }
                    }
                }
            } 
            //Basic pawn move
            else{
                if(board.colorTurn == Piece.White){
                    if (board.board[endIndex + 8]  == (Piece.Pawn | Piece.White)){
                        startIndex = endIndex + 8;
                    }
                    //Double pawn push
                    else if (board.board[endIndex + 16]  == (Piece.Pawn | Piece.White)){
                        startIndex = endIndex + 16;
                        flag = 6;
                    } else {
                        //Debug.Log("No pawn detected for pawn push");
                    }
                } else {
                    if (board.board[endIndex - 8]  == (Piece.Pawn | Piece.Black)){
                        startIndex = endIndex - 8;
                    } 
                    //Double pawn push
                    else if (board.board[endIndex - 16]  == (Piece.Pawn | Piece.Black)){
                        startIndex = endIndex - 16;
                        flag = 6;
                    } else {
                        //Debug.Log("No pawn detected for pawn push");
                    }
                }
            }

            return new Move(startIndex, endIndex, isCapture, flag);
        }
        
    }

    public static Move getMoveFromIntValue(int value)
    {
        int startIndex = value & 0b111111;
        int endIndex = (value >> 6) & 0b111111;
        int flag = (value >> 12) & 0b111;
        bool isCapture = ((value >> 15) & 0b1) == 1 ? true : false;
        return new Move(startIndex, endIndex, isCapture, flag);


    }
}
