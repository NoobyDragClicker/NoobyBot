using System;
using System.Collections.Generic;
using System.Linq;


public static class Coord
{
    //Return alphanum notation for a square
    public static String GetNotationFromIndex(int index)
    {
        string notation;
        int rank = IndexToRank(index);
        int fileInt = IndexToFile(index);
        char file;
        switch (fileInt)
        {
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

    //Get the UCI move notation from start and end pos
    public static String GetUCIMoveNotation(Move move)
    {
        string start = GetNotationFromIndex(move.oldIndex);
        string end = GetNotationFromIndex(move.newIndex);
        string promotion = "";

        if(move.flag == Move.QueenPromo){ promotion = "q"; }
        else if (move.flag == Move.BishopPromo){ promotion = "b"; }
        else if (move.flag == Move.KnightPromo){ promotion = "n"; }
        else if (move.flag == Move.RookPromo){ promotion = "r"; }


        return start + "" + end + promotion;
    }

    public static int DistToEdge(int index)
    {
        int file = index % 8;
        return Math.Min(file, file ^ 7);
    }

    public static int ChebyshevDist(int squareA, int squareB)
    {
        int rankDist = Math.Abs(IndexToRank(squareA) - IndexToRank(squareB));
        int fileDist = Math.Abs(IndexToFile(squareA) - IndexToFile(squareB));
        return Math.Max(rankDist, fileDist);
    }

    public static int IndexToRank(int index)
    {
        return 8 - ((index - (index % 8)) / 8);
    }

    public static int IndexToFile(int index){
        int file = index % 8 + 1;
        return file;
    }

    //From alphanum square notation to an index
    public static int NotationToIndex(string notation)
    {
        int rank = (int)Char.GetNumericValue(notation[1]);
        int file = LetterToFile(notation[0]);
        return ((8 - rank) * 8) + (file - 1);
    }

    public static Move convertUCIMove(Board board, string move)
    {
        int startPos = 0;
        int endPos = 0;
        try
        {
            startPos = NotationToIndex(move.Substring(0, 2));
            endPos = NotationToIndex(move.Substring(2, 2));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine(move);
        }
        
        bool isPromotion = (move.Count() == 5) ? true : false;
        int flag = 0;
        bool isCapture = false;

        if (board.board[endPos] != 0)
        {
            isCapture = true;
        }
        if (board.PieceAt(startPos) == Piece.Pawn)
        {
            int distance = Math.Abs(startPos - endPos);
            //Double pawn push
            if (distance == 16)
            {
                flag = 6;
            }
            //En passant
            else if ((distance == 7 || distance == 9) & !isCapture)
            {
                flag = 7;
            }
            else if (isPromotion)
            {
                if (move[4] == 'q') { flag = 1; }
                else if (move[4] == 'b') { flag = 2; }
                else if (move[4] == 'n') { flag = 3; }
                else if (move[4] == 'r') { flag = 4; }
            }
        }
        else if (board.PieceAt(startPos) == Piece.King)
        {
            int distance = Math.Abs(startPos - endPos);
            //Castling
            if (distance == 2)
            {
                flag = 5;
            }
        }
        return new Move(startPos, endPos, isCapture, flag);
    }

    public static int LetterToFile(char letter)
    {
        int file = 0;
        switch (letter)
        {
            case 'a': file = 1; break;
            case 'b': file = 2; break;
            case 'c': file = 3; break;
            case 'd': file = 4; break;
            case 'e': file = 5; break;
            case 'f': file = 6; break;
            case 'g': file = 7; break;
            case 'h': file = 8; break;
            default: break;
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
                return new Move(60, 62, false, Move.Castle);
            } else{
                return new Move(4, 6, false, Move.Castle);
            }

        } 
        else if (strMove == "O-O-O"){
            if(board.colorTurn == Piece.White){
                return new Move(60, 58, false, Move.Castle);
            } else{
                return new Move(4, 2, false, Move.Castle);
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
                    Span<Move> validMoves = new Move[256];
                    switch (pieceType){
                        case 'N': MoveGenerator.GenerateKnightMoves(validMoves, 0, board); pieceNum = Piece.Knight; break;
                        case 'B': MoveGenerator.GenerateBishopMoves(validMoves, 0, board) ; pieceNum = Piece.Bishop; break;
                        case 'Q':
                            int moveIndex = MoveGenerator.GenerateBishopMoves(validMoves, 0,  board);
                            MoveGenerator.GenerateRookMoves(validMoves, moveIndex, board);
                            pieceNum = Piece.Queen;
                            break;
                        case 'R': MoveGenerator.GenerateRookMoves(validMoves, 0, board); pieceNum = Piece.Rook; break;
                        case 'K': MoveGenerator.GenerateKingMoves(validMoves, 0, board); pieceNum = Piece.King; break;
                    }
                    List<int> possibleStartIndexes = new List<int>();

                    for (int x = 0; x< validMoves.Length; x++){
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
                            if (((board.gameStateHistory[board.fullMoveClock].diagPins | board.gameStateHistory[board.fullMoveClock].straightPins) & (1ul << possibleStartIndexes[x])) != 0){
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
