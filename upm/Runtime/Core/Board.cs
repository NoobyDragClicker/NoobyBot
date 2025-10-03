using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;


public class Board
{
    public const string startPos = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public const int WhiteIndex = 0;
    public const int BlackIndex = 1;

    public ulong[,] pieceBitboards = new ulong[2, 7];
    public ulong[] sideBitboard = new ulong[2];
    public ulong[] attackedSquares = new ulong[2];
    public ulong allPiecesBitboard;

    //Ray of squares in the pin, including the attacking piece
    public ulong diagPins = 0;
    public ulong straightPins = 0;
     //Ray of squares in the check, including the attacking piece
    public ulong checkIndexes = 0;
    public int[,] pieceCounts = new int[2, 7];

    public bool isCurrentPlayerInCheck;
    public bool isCurrentPlayerInDoubleCheck;
    
    public int colorTurn;
    public int numCheckingPieces;

    //Saves the index where a pawn can capture
    public int enPassantIndex;
    public int fiftyMoveCounter;
    public int[] board = new int[64];

    //Saves info about the game
    Stack<GameState> gameStateHistory = new Stack<GameState>();

    public GameState currentGameState = new GameState();
    public Stack<Move> gameMoveHistory = new Stack<Move>();

    public Stack<ulong> zobristHistory = new Stack<ulong>();
    public ulong zobristKey;
    public int plyFromStart;
    SearchLogger logger;
    bool isMoveGenUpdated = false;
    bool isSimpleCheckStatusUpdated = false;
    bool areAttacksUpdated = false;

    public void setPosition(string fenPosition, SearchLogger logger)
    {
        this.logger = logger;
        zobristHistory.Clear();
        gameMoveHistory.Clear();
        gameStateHistory.Clear();
        currentGameState = new GameState();
        board = ConvertFromFEN(fenPosition);
        zobristKey = Zobrist.CalculateZobrist(this);
        zobristHistory.Push(zobristKey);
        isMoveGenUpdated = false;
        isSimpleCheckStatusUpdated = false;
        areAttacksUpdated = false;
        GenerateMoveGenInfo();
    }


    //Moves the pieces
    public void Move(Move move, bool isSearch)
    {
        isMoveGenUpdated = false;
        isSimpleCheckStatusUpdated = false;
        areAttacksUpdated = false;
        gameMoveHistory.Push(move);
        int oldCastlingRights = currentGameState.castlingRights;
        int castlingRights = oldCastlingRights;

        int oldEPFile;
        if (enPassantIndex != -1)
        {
            oldEPFile = Coord.IndexToFile(enPassantIndex);
        }
        else
        {
            oldEPFile = 0;
        }
        int enPassantFile = 0;

        int startPos = move.oldIndex;
        int newPos = move.newIndex;
        int movedPiece;
        int newPiece; //Used for promotions
        int capturedPiece = 0;

        int currentColorIndex = (colorTurn == Piece.White) ? WhiteIndex : BlackIndex;
        int oppositeColorIndex = 1 - currentColorIndex;

        //Set to none 
        enPassantIndex = -1;
        movedPiece = board[startPos];
        if (move.isPromotion())
        {
            fiftyMoveCounter = 0;
            //Sets the new piece to be the same color but whatever the new piece type is
            newPiece = Piece.Color(board[startPos]) | move.PromotedPieceType();
            pieceCounts[currentColorIndex, move.PromotedPieceType()] += 1;
            pieceCounts[currentColorIndex, Piece.Pawn] -= 1;
        }
        else
        {
            newPiece = movedPiece;
        }

        int movedPieceType = Piece.PieceType(movedPiece);
        int newPieceType = Piece.PieceType(newPiece);

        //castle
        if (move.flag == 5)
        {
            int oldRookIndex = 0;
            int newRookIndex = 0;
            fiftyMoveCounter++;
            //Once castles, you can't castle again
            if (colorTurn == Piece.White)
            {
                //Removing castling rights
                castlingRights &= 0b1100;
                //Short castles
                if (newPos == 62)
                {
                    oldRookIndex = 63;
                    newRookIndex = 61;
                    int rook = board[oldRookIndex];

                    BitboardHelper.ClearSquare(ref pieceBitboards[WhiteIndex, Piece.Rook], oldRookIndex);
                    BitboardHelper.SetSquare(ref pieceBitboards[WhiteIndex, Piece.Rook], newRookIndex);
                    board[newRookIndex] = rook;
                    board[oldRookIndex] = 0;
                }
                //Long castles
                else if (newPos == 58)
                {
                    oldRookIndex = 56;
                    newRookIndex = 59;
                    int rook = board[oldRookIndex];

                    BitboardHelper.ClearSquare(ref pieceBitboards[WhiteIndex, Piece.Rook], oldRookIndex);
                    BitboardHelper.SetSquare(ref pieceBitboards[WhiteIndex, Piece.Rook], newRookIndex);
                    board[newRookIndex] = rook;
                    board[oldRookIndex] = 0;
                }

                BitboardHelper.ClearSquare(ref pieceBitboards[WhiteIndex, movedPieceType], startPos);
                BitboardHelper.SetSquare(ref pieceBitboards[WhiteIndex, movedPieceType], newPos);
                board[newPos] = movedPiece;
                board[startPos] = 0;
            }
            else if (colorTurn == Piece.Black)
            {
                //Removing castling rights
                castlingRights &= 0b0011;
                //Short castles
                if (newPos == 6)
                {
                    oldRookIndex = 7;
                    newRookIndex = 5;
                    int rook = board[oldRookIndex];

                    BitboardHelper.ClearSquare(ref pieceBitboards[BlackIndex, Piece.Rook], oldRookIndex);
                    BitboardHelper.SetSquare(ref pieceBitboards[BlackIndex, Piece.Rook], newRookIndex);
                    board[newRookIndex] = rook;
                    board[oldRookIndex] = 0;
                }
                //Long castles
                else if (newPos == 2)
                {
                    oldRookIndex = 0;
                    newRookIndex = 3;
                    int rook = board[oldRookIndex];

                    BitboardHelper.ClearSquare(ref pieceBitboards[BlackIndex, Piece.Rook], oldRookIndex);
                    BitboardHelper.SetSquare(ref pieceBitboards[BlackIndex, Piece.Rook], newRookIndex);
                    board[newRookIndex] = rook;
                    board[oldRookIndex] = 0;
                }

                BitboardHelper.ClearSquare(ref pieceBitboards[BlackIndex, movedPieceType], startPos);
                BitboardHelper.SetSquare(ref pieceBitboards[BlackIndex, movedPieceType], newPos);
                board[newPos] = movedPiece;
                board[startPos] = 0;
            }
            zobristKey ^= Zobrist.piecesArray[Piece.Rook, currentColorIndex, oldRookIndex];
            zobristKey ^= Zobrist.piecesArray[Piece.Rook, currentColorIndex, newRookIndex];
        }
        //en passant
        else if (move.flag == 7)
        {

            BitboardHelper.ClearSquare(ref pieceBitboards[currentColorIndex, Piece.Pawn], startPos);
            BitboardHelper.SetSquare(ref pieceBitboards[currentColorIndex, Piece.Pawn], newPos);
            //capture
            board[newPos] = movedPiece;
            board[startPos] = 0;
            int attackedPawnIndex = (colorTurn == Piece.White) ? newPos + 8 : newPos - 8;
            capturedPiece = board[attackedPawnIndex];

            BitboardHelper.ClearSquare(ref pieceBitboards[oppositeColorIndex, Piece.Pawn], attackedPawnIndex);
            board[attackedPawnIndex] = 0;
            zobristKey ^= Zobrist.piecesArray[Piece.Pawn, oppositeColorIndex, attackedPawnIndex];
            fiftyMoveCounter = 0;
            pieceCounts[oppositeColorIndex, Piece.Pawn] -= 1;
        }
        else
        {
            //Double pawn push
            if (move.flag == 6)
            {
                //Set to the square behind the spot moved to
                enPassantIndex = (colorTurn == Piece.White) ? (newPos + 8) : (newPos - 8);
                enPassantFile = IndexToFile(startPos);
            }

            //Once the king has been moved, you can't castle
            if (Piece.PieceType(movedPiece) == Piece.King)
            {
                if (colorTurn == Piece.White) { castlingRights &= 0b1100; }
                else if (colorTurn == Piece.Black) { castlingRights &= 0b0011; }
            }

            //If it's a rook move, check if its from the starting square to remove castling perms
            if (colorTurn == Piece.White)
            {
                if (Piece.PieceType(movedPiece) == Piece.Rook)
                {
                    if (startPos == 56) { castlingRights &= 0b1101; }
                    //Short side
                    else if (startPos == 63) { castlingRights &= 0b1110; }
                }
            }
            else if (colorTurn == Piece.Black)
            {
                if (Piece.PieceType(movedPiece) == Piece.Rook)
                {
                    if (startPos == 0) { castlingRights &= 0b0111; }
                    //Short side
                    else if (startPos == 7) { castlingRights &= 0b1011; }
                }
            }

            //Make the move
            if (move.isCapture())
            {
                //capture
                capturedPiece = board[newPos];
                int capturedPieceType = Piece.PieceType(capturedPiece);
                BitboardHelper.ClearSquare(ref pieceBitboards[oppositeColorIndex, Piece.PieceType(capturedPiece)], newPos);
                //Removing castling rights 
                if (capturedPieceType == Piece.Rook)
                {
                    if (colorTurn == Piece.Black)
                    {
                        if (newPos == 56) { castlingRights &= 0b1101; }
                        if (newPos == 63) { castlingRights &= 0b1110; }
                    }
                    else if (colorTurn == Piece.White)
                    {
                        if (newPos == 0) { castlingRights &= 0b0111; }
                        if (newPos == 7) { castlingRights &= 0b1011; }
                    }
                }

                zobristKey ^= Zobrist.piecesArray[capturedPieceType, oppositeColorIndex, newPos];

                BitboardHelper.ClearSquare(ref pieceBitboards[currentColorIndex, movedPieceType], startPos);
                BitboardHelper.SetSquare(ref pieceBitboards[currentColorIndex, newPieceType], newPos);
                board[newPos] = newPiece;
                board[startPos] = 0;

                pieceCounts[oppositeColorIndex, capturedPieceType] -= 1;

                if (Piece.PieceType(capturedPiece) == Piece.King)
                {
                    logger.AddToLog("King captured", SearchLogger.LoggingLevel.Deadly);
                    logger.AddToLog(ConvertToFEN(), SearchLogger.LoggingLevel.Deadly);
                }

                //Capture resets counter
                fiftyMoveCounter = 0;
            }
            else
            {
                BitboardHelper.ClearSquare(ref pieceBitboards[currentColorIndex, movedPieceType], startPos);
                BitboardHelper.SetSquare(ref pieceBitboards[currentColorIndex, newPieceType], newPos);
                board[newPos] = newPiece;
                board[startPos] = 0;

                //Pawn push resets counter
                if (Piece.PieceType(movedPiece) == Piece.Pawn)
                {
                    fiftyMoveCounter = 0;
                }
                else
                {
                    fiftyMoveCounter++;
                }
            }
        }

        colorTurn = (colorTurn == Piece.White) ? Piece.Black : Piece.White;

        sideBitboard[WhiteIndex] = pieceBitboards[WhiteIndex, Piece.Pawn] | pieceBitboards[WhiteIndex, Piece.Knight] | pieceBitboards[WhiteIndex, Piece.Bishop] | pieceBitboards[WhiteIndex, Piece.Rook] | pieceBitboards[WhiteIndex, Piece.Queen] | pieceBitboards[WhiteIndex, Piece.King];
        sideBitboard[BlackIndex] = pieceBitboards[BlackIndex, Piece.Pawn] | pieceBitboards[BlackIndex, Piece.Knight] | pieceBitboards[BlackIndex, Piece.Bishop] | pieceBitboards[BlackIndex, Piece.Rook] | pieceBitboards[BlackIndex, Piece.Queen] | pieceBitboards[BlackIndex, Piece.King];
        allPiecesBitboard = sideBitboard[WhiteIndex] | sideBitboard[BlackIndex];

        //Update gamestate
        currentGameState.capturedPiece = capturedPiece;
        currentGameState.enPassantFile = enPassantFile;
        currentGameState.fiftyMoveCounter = fiftyMoveCounter;
        currentGameState.castlingRights = castlingRights;
        gameStateHistory.Push(currentGameState);

        //Moving friendly piece
        zobristKey ^= Zobrist.piecesArray[Piece.PieceType(movedPiece), currentColorIndex, startPos];
        zobristKey ^= Zobrist.piecesArray[Piece.PieceType(newPiece), currentColorIndex, newPos];

        //Castling, ep, side to move
        zobristKey ^= Zobrist.castlingRights[oldCastlingRights];
        zobristKey ^= Zobrist.castlingRights[castlingRights];
        zobristKey ^= Zobrist.sideToMove;
        zobristKey ^= Zobrist.enPassantFile[oldEPFile];
        zobristKey ^= Zobrist.enPassantFile[enPassantFile];
        zobristHistory.Push(zobristKey);
        plyFromStart++;
    }
    public void UndoMove(Move move)
    {
        isMoveGenUpdated = false;
        isSimpleCheckStatusUpdated = false;
        areAttacksUpdated = false;

        gameMoveHistory.Pop();

        colorTurn = (colorTurn == Piece.White) ? Piece.Black : Piece.White;
        int currentColorIndex = (colorTurn == Piece.White) ? WhiteIndex : BlackIndex;
        int oppositeColorIndex = 1 - currentColorIndex;
        //Removing the current one and getting the required info
        GameState oldGameStateHistory = gameStateHistory.Pop();

        //Getting the game state from the current move
        currentGameState = gameStateHistory.Peek();
        int capturedPiece = oldGameStateHistory.capturedPiece;
        int enPassantFile = currentGameState.enPassantFile;
        fiftyMoveCounter = currentGameState.fiftyMoveCounter;

        zobristHistory.Pop();
        zobristKey = zobristHistory.Peek();

        //Setting the ep index to what it used to be
        if (enPassantFile != 0)
        {
            enPassantIndex = (colorTurn == Piece.White) ? (15 + enPassantFile) : (39 + enPassantFile);
        }
        else
        {
            enPassantIndex = -1;
        }

        int startPos = move.oldIndex;
        int newPos = move.newIndex;
        int movedPiece = board[newPos];
        int movedPieceType = Piece.PieceType(movedPiece);
        int pieceTypeBeforeMove = movedPieceType;

        if (move.isPromotion())
        {
            fiftyMoveCounter = 0;
            //Sets the new piece to be the same color but whatever the new piece type is
            movedPiece = Piece.Color(board[newPos]) | Piece.Pawn;
            pieceTypeBeforeMove = Piece.Pawn;
            pieceCounts[currentColorIndex, Piece.Pawn] += 1;
            pieceCounts[currentColorIndex, movedPieceType] -= 1;
        }

        //castle
        if (move.flag == 5)
        {
            //Undo the castles
            if (colorTurn == Piece.White)
            {
                //short castles
                if (newPos == 62)
                {
                    int rook = board[61];
                    board[63] = rook;
                    board[61] = 0;
                    BitboardHelper.ClearSquare(ref pieceBitboards[WhiteIndex, Piece.Rook], 61);
                    BitboardHelper.SetSquare(ref pieceBitboards[WhiteIndex, Piece.Rook], 63);
                }
                //Long castles
                else if (newPos == 58)
                {
                    int rook = board[59];
                    board[56] = rook;
                    board[59] = 0;
                    BitboardHelper.ClearSquare(ref pieceBitboards[WhiteIndex, Piece.Rook], 59);
                    BitboardHelper.SetSquare(ref pieceBitboards[WhiteIndex, Piece.Rook], 56);
                }
                board[startPos] = movedPiece;
                board[newPos] = 0;

                BitboardHelper.ClearSquare(ref pieceBitboards[WhiteIndex, Piece.King], newPos);
                BitboardHelper.SetSquare(ref pieceBitboards[WhiteIndex, Piece.King], startPos);

            }
            else if (colorTurn == Piece.Black)
            {
                //Short castles
                if (newPos == 6)
                {
                    int rook = board[5];
                    board[7] = rook;
                    board[5] = 0;
                    BitboardHelper.ClearSquare(ref pieceBitboards[BlackIndex, Piece.Rook], 5);
                    BitboardHelper.SetSquare(ref pieceBitboards[BlackIndex, Piece.Rook], 7);
                }
                //Long castles
                else if (newPos == 2)
                {
                    int rook = board[3];
                    board[0] = rook;
                    board[3] = 0;
                    BitboardHelper.ClearSquare(ref pieceBitboards[BlackIndex, Piece.Rook], 3);
                    BitboardHelper.SetSquare(ref pieceBitboards[BlackIndex, Piece.Rook], 0);
                }
                board[startPos] = movedPiece;
                board[newPos] = 0;
                BitboardHelper.ClearSquare(ref pieceBitboards[BlackIndex, Piece.King], newPos);
                BitboardHelper.SetSquare(ref pieceBitboards[BlackIndex, Piece.King], startPos);
            }

        }
        //en passant
        else if (move.flag == 7)
        {
            //capture
            board[startPos] = movedPiece;
            board[newPos] = 0;

            BitboardHelper.ClearSquare(ref pieceBitboards[currentColorIndex, Piece.Pawn], newPos);
            BitboardHelper.SetSquare(ref pieceBitboards[currentColorIndex, Piece.Pawn], startPos);


            //Replacing the captured pawn
            int attackedPawnIndex = (colorTurn == Piece.White) ? newPos + 8 : newPos - 8;
            board[attackedPawnIndex] = capturedPiece;
            BitboardHelper.SetSquare(ref pieceBitboards[oppositeColorIndex, Piece.Pawn], attackedPawnIndex);
            pieceCounts[oppositeColorIndex, Piece.Pawn] += 1;
        }
        else
        {
            //Undo the move
            if (move.isCapture())
            {
                int capturedPieceType = Piece.PieceType(capturedPiece);
                //capture
                board[startPos] = movedPiece;
                BitboardHelper.ClearSquare(ref pieceBitboards[currentColorIndex, movedPieceType], newPos);
                BitboardHelper.SetSquare(ref pieceBitboards[currentColorIndex, pieceTypeBeforeMove], startPos);


                board[newPos] = capturedPiece;
                BitboardHelper.SetSquare(ref pieceBitboards[oppositeColorIndex, capturedPieceType], newPos);
                pieceCounts[oppositeColorIndex, capturedPieceType] += 1;
            }
            else
            {
                board[startPos] = movedPiece;
                board[newPos] = 0;
                BitboardHelper.ClearSquare(ref pieceBitboards[currentColorIndex, movedPieceType], newPos);
                BitboardHelper.SetSquare(ref pieceBitboards[currentColorIndex, pieceTypeBeforeMove], startPos);
            }
        }
        plyFromStart--;
        sideBitboard[WhiteIndex] = pieceBitboards[WhiteIndex, Piece.Pawn] | pieceBitboards[WhiteIndex, Piece.Knight] | pieceBitboards[WhiteIndex, Piece.Bishop] | pieceBitboards[WhiteIndex, Piece.Rook] | pieceBitboards[WhiteIndex, Piece.Queen] | pieceBitboards[WhiteIndex, Piece.King];
        sideBitboard[BlackIndex] = pieceBitboards[BlackIndex, Piece.Pawn] | pieceBitboards[BlackIndex, Piece.Knight] | pieceBitboards[BlackIndex, Piece.Bishop] | pieceBitboards[BlackIndex, Piece.Rook] | pieceBitboards[BlackIndex, Piece.Queen] | pieceBitboards[BlackIndex, Piece.King];
        allPiecesBitboard = sideBitboard[WhiteIndex] | sideBitboard[BlackIndex];
    }

    //TODO: fix 
    public void MakeNullMove()
    {
        fiftyMoveCounter += 1;
        plyFromStart += 1;
        colorTurn = (colorTurn == Piece.White) ? Piece.Black : Piece.White;
        int oldEPFile = currentGameState.enPassantFile;
        currentGameState.fiftyMoveCounter = fiftyMoveCounter;
        currentGameState.enPassantFile = 0;
        currentGameState.capturedPiece = 0;
        gameStateHistory.Push(currentGameState);

        enPassantIndex = -1;

        zobristKey ^= Zobrist.sideToMove;
        zobristKey ^= Zobrist.enPassantFile[oldEPFile];
        zobristKey ^= Zobrist.enPassantFile[currentGameState.enPassantFile];
        zobristHistory.Push(zobristKey);
    }
    public void UnmakeNullMove()
    {
        fiftyMoveCounter -= 1;
        plyFromStart -= 1;
        colorTurn = (colorTurn == Piece.White) ? Piece.Black : Piece.White;
        zobristHistory.Pop();
        zobristKey = zobristHistory.Peek();
        gameStateHistory.Pop();
        currentGameState = gameStateHistory.Peek();
        enPassantIndex = EnPassantFileToIndex(colorTurn, currentGameState.enPassantFile);
    }

    public int[] ConvertFromFEN(string fenPosition)
    {
        currentGameState.capturedPiece = 0;
        currentGameState.castlingRights = 0;
        currentGameState.enPassantFile = 0;
        currentGameState.fiftyMoveCounter = 0;

        Dictionary<char, int> pieceTypeFromSymbol = new Dictionary<char, int>()
        {
            ['k'] = Piece.King,
            ['p'] = Piece.Pawn,
            ['n'] = Piece.Knight,
            ['b'] = Piece.Bishop,
            ['r'] = Piece.Rook,
            ['q'] = Piece.Queen
        };
        int[] position = new int[64];

        string[] fenComponents = fenPosition.Split(' ');

        //Part denoting position of each piece
        string posString = fenComponents[0];
        int index = 0;
        foreach (char c in posString)
        {
            if (c == '/')
            {
                //ignore
            }
            else
            {

                if (Char.IsLetter(c))
                {
                    int pieceColour = Char.IsUpper(c) ? Piece.White : Piece.Black;
                    int colorIndex = (pieceColour == Piece.White) ? WhiteIndex : BlackIndex;

                    int pieceType = pieceTypeFromSymbol[char.ToLower(c)];
                    position[index] = pieceType | pieceColour;

                    BitboardHelper.SetSquare(ref pieceBitboards[colorIndex, pieceType], index);
                    pieceCounts[colorIndex, pieceType] += 1;
                    index++;
                }
                else if (Char.IsNumber(c))
                {
                    for (int x = 0; x < Char.GetNumericValue(c); x++)
                    {
                        position[index] = 0;
                        index++;
                    }
                }
            }

        }

        sideBitboard[WhiteIndex] = pieceBitboards[WhiteIndex, Piece.Pawn] | pieceBitboards[WhiteIndex, Piece.Knight] | pieceBitboards[WhiteIndex, Piece.Bishop] | pieceBitboards[WhiteIndex, Piece.Rook] | pieceBitboards[WhiteIndex, Piece.Queen] | pieceBitboards[WhiteIndex, Piece.King];
        sideBitboard[BlackIndex] = pieceBitboards[BlackIndex, Piece.Pawn] | pieceBitboards[BlackIndex, Piece.Knight] | pieceBitboards[BlackIndex, Piece.Bishop] | pieceBitboards[BlackIndex, Piece.Rook] | pieceBitboards[BlackIndex, Piece.Queen] | pieceBitboards[BlackIndex, Piece.King];
        allPiecesBitboard = sideBitboard[WhiteIndex] | sideBitboard[BlackIndex];

        //Loads who's move it is
        string sidetoMove = fenPosition.Split(' ')[1];
        if (sidetoMove == "w") { colorTurn = Piece.White; }
        else if (sidetoMove == "b") { colorTurn = Piece.Black; }

        string castling = fenPosition.Split(' ')[2];

        int castlingRights = 0;
        if (castling.Contains("K")) { castlingRights += 1; }
        if (castling.Contains("Q")) { castlingRights += 2; }
        if (castling.Contains("k")) { castlingRights += 4; }
        if (castling.Contains("q")) { castlingRights += 8; }
        currentGameState.castlingRights = castlingRights;
        
        enPassantIndex = -1;
        if (fenComponents.Length >= 4)
        {
            if (fenComponents[3] != "-")
            {
                enPassantIndex = Coord.NotationToIndex(fenComponents[3]);
            }
        }
        
        gameStateHistory.Push(currentGameState);
        plyFromStart = 0;
        return position;
    }

    public string ConvertToFEN()
    {
        string fen = "";
        int emptyCounter = 0;
        for (int index = 0; index < 64; index++)
        {
            //Add line separators
            if (index % 8 == 0 && index != 0)
            {
                if (emptyCounter != 0) { fen += emptyCounter.ToString(); emptyCounter = 0; }
                fen += "/";
            }

            if (board[index] == 0)
            {
                emptyCounter++;
            }
            else
            {
                //If empties need to be added, add them
                if (emptyCounter != 0) { fen += emptyCounter.ToString(); emptyCounter = 0; }
                int pieceType = Piece.PieceType(board[index]);
                if (Piece.Color(board[index]) == Piece.White)
                {
                    switch (pieceType)
                    {
                        case Piece.Pawn: fen += "P"; break;
                        case Piece.Knight: fen += "N"; break;
                        case Piece.Bishop: fen += "B"; break;
                        case Piece.Rook: fen += "R"; break;
                        case Piece.Queen: fen += "Q"; break;
                        case Piece.King: fen += "K"; break;
                    }
                }
                else
                {
                    switch (pieceType)
                    {
                        case Piece.Pawn: fen += "p"; break;
                        case Piece.Knight: fen += "n"; break;
                        case Piece.Bishop: fen += "b"; break;
                        case Piece.Rook: fen += "r"; break;
                        case Piece.Queen: fen += "q"; break;
                        case Piece.King: fen += "k"; break;
                    }
                }

            }

        }

        fen += (colorTurn == Piece.White) ? " w " : " b ";
        string castleStr = "";
        castleStr += HasKingsideRight(Piece.White) ? "K" : "";
        castleStr += HasQueensideRight(Piece.White) ? "Q" : "";
        castleStr += HasKingsideRight(Piece.Black) ? "k" : "";
        castleStr += HasQueensideRight(Piece.Black) ? "q" : "";
        castleStr = castleStr == "" ? "-" : castleStr;



        fen += castleStr;

        if (enPassantIndex != -1)
        {
            fen += " " + Coord.GetNotationFromIndex(enPassantIndex);
        } else{ fen += " -"; }
        return fen;
    }
    public bool IsDraw()
    {
        //Stalemate
        Span<Move> moves = new Move[256];
        int numMoves = MoveGenerator.GenerateLegalMoves(this, ref moves, colorTurn);
        if (numMoves == 0 && !isCurrentPlayerInCheck) { return true; }
        //50 move rule
        if (fiftyMoveCounter >= 100) { return true; }
        if (IsRepetitionDraw()) { return true; }

        if (pieceCounts[WhiteIndex, Piece.Pawn] + pieceCounts[BlackIndex, Piece.Pawn] +
                pieceCounts[WhiteIndex, Piece.Rook] + pieceCounts[BlackIndex, Piece.Rook] +
                pieceCounts[WhiteIndex, Piece.Queen] + pieceCounts[BlackIndex, Piece.Queen]
                != 0) { return false; }
        else if (pieceCounts[WhiteIndex, Piece.Knight] + pieceCounts[WhiteIndex, Piece.Bishop] <= 1 && pieceCounts[BlackIndex, Piece.Knight] + pieceCounts[BlackIndex, Piece.Bishop] <= 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void GenerateMoveGenInfo()
    {
        if (!areAttacksUpdated)
        {
            attackedSquares[WhiteIndex] = MoveGenerator.GenerateAttackedSquares(this, Piece.White);
            attackedSquares[BlackIndex] = MoveGenerator.GenerateAttackedSquares(this, Piece.Black);
            areAttacksUpdated = true;
        }
        if (!isMoveGenUpdated)
        {
            MoveGenerator.UpdateChecksAndPins(this);
            isCurrentPlayerInCheck = (numCheckingPieces > 0) ? true : false;
            isCurrentPlayerInDoubleCheck = (numCheckingPieces > 1) ? true : false;
            isMoveGenUpdated = true;
            isSimpleCheckStatusUpdated = true;
        }
    }

    public void UpdateSimpleCheckStatus()
    {
        if (!isSimpleCheckStatusUpdated)
        {
            isCurrentPlayerInCheck = MoveGenerator.DetermineCheckStatus(this);
            isSimpleCheckStatusUpdated = true;
        }
        
    }
    public bool IsRepetitionDraw()
    {
        int repCount = zobristHistory.Count(x => x == zobristKey);
        if (repCount >= 3)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    public bool IsSearchDraw()
    {
        if (fiftyMoveCounter >= 100) { return true; }
        else if (IsRepetitionDraw()) { return true; }
        else
        {
            if (pieceCounts[WhiteIndex, Piece.Pawn] + pieceCounts[BlackIndex, Piece.Pawn] +
                pieceCounts[WhiteIndex, Piece.Rook] + pieceCounts[BlackIndex, Piece.Rook] +
                pieceCounts[WhiteIndex, Piece.Queen] + pieceCounts[BlackIndex, Piece.Queen]
                != 0) { return false; }
            else if (pieceCounts[WhiteIndex, Piece.Knight] + pieceCounts[WhiteIndex, Piece.Bishop] <= 1 && pieceCounts[BlackIndex, Piece.Knight] + pieceCounts[BlackIndex, Piece.Bishop] <= 1){ return true; }
            else{ return false; }
        }
    }
    public bool IsCheckmate(int color)
    {
        int kingIndex = MoveGenerator.GetKingIndex(color, this);
        if(!isSimpleCheckStatusUpdated){ UpdateSimpleCheckStatus(); }
        if (isCurrentPlayerInCheck)
        {
            Span<Move> legalKingMoves = stackalloc Move[218];
            int currMoveIndex = MoveGenerator.GenerateKingMoves(legalKingMoves, 0, color, this);
            //If there are any valid king moves
            if (currMoveIndex != 0)
            {
                return false;
            }
            //check if there are any blocks
            else
            {
                Span<Move> legalMoves = new Move[256];
                int moveIndex = MoveGenerator.GenerateLegalMoves(this, ref legalMoves, color);
                if (moveIndex == 0) { return true; }
                else { return false; }
            }
        }
        else
        {
            return false;
        }
    }


    //Utilities
    public int IndexToRank(int index)
    {
        return 8 - ((index - (index % 8)) / 8);
    }

    public int IndexToFile(int index)
    {
        int file = index % 8 + 1;
        return file;
    }

    public int RankFileToIndex(int file, int rank)
    {
        int index = ((8 - rank) * 8) + file - 1;
        return index;
    }


    //Gets them from the current gamestate
    public bool HasKingsideRight(int color)
    {
        if (color == Piece.White && (currentGameState.castlingRights & 0b1) == 1)
        {
            return true;
        }
        else if (color == Piece.Black && (currentGameState.castlingRights & 0b100) == 4)
        {
            return true;
        }
        else { return false; }
    }
    public bool HasQueensideRight(int color)
    {
        if (color == Piece.White && (currentGameState.castlingRights & 0b000010) == 2)
        {
            return true;
        }
        else if (color == Piece.Black && (currentGameState.castlingRights & 0b001000) == 8)
        {
            return true;
        }
        else { return false; }
    }

    public int EnPassantFileToIndex(int pieceColor, int epFile)
    {
        if (pieceColor == Piece.White)
        {
            return 39 + epFile;
        }
        else
        {
            return 15 + epFile;
        }
    }
}
public struct GameState {
    public int castlingRights;
    public int enPassantFile;
    public int capturedPiece;
    public int fiftyMoveCounter;
}
