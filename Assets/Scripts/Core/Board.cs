using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEngine;
#endif


public class Board
{
    public const string startPos = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public const int WhiteIndex = 0;
    public const int BlackIndex = 1;


    public bool isCurrentPlayerInCheck;
    public bool isCurrentPlayerInDoubleCheck;

    public List<int> checkingPieces = new List<int>();
    public List<int> blockableIndexes = new List<int>();
    public List<PinnedPair> pinnedIndexes = new List<PinnedPair>();
    public List<int> pinnedPieceIndexes = new List<int>();
    public int[,] attackedSquares = new int[2, 64];
    public int colorTurn;

    //Saves the index where a pawn can capture
    public int enPassantIndex;
    public int fiftyMoveCounter;
    public MoveGenerator moveGenerator;
    public int[] board;

    //Saves info about the game
    Stack<GameState> gameStateHistory = new Stack<GameState>();

    public GameState currentGameState = new GameState();
    public Stack<Move> gameMoveHistory = new Stack<Move>();

    public Stack<ulong> zobristHistory = new Stack<ulong>();
    public ulong zobristKey;
    public int plyFromStart;


    public void setPosition(string fenPosition, MoveGenerator generator)
    {
        zobristHistory.Clear();
        gameMoveHistory.Clear();
        gameStateHistory.Clear();
        currentGameState = new GameState();

        moveGenerator = generator;
        board = ConvertFromFEN(fenPosition);
        try
        {
            zobristKey = Zobrist.CalculateZobrist(this);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        zobristHistory.Push(zobristKey);
        GenerateMoveGenInfo();
    }


    //Moves the pieces
    public void Move(Move move, bool isSearch)
    {
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
        }
        else
        {
            newPiece = movedPiece;
        }

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
                    board[newRookIndex] = rook;
                    board[oldRookIndex] = 0;
                }
                //Long castles
                else if (newPos == 58)
                {
                    oldRookIndex = 56;
                    newRookIndex = 59;
                    int rook = board[oldRookIndex];
                    board[newRookIndex] = rook;
                    board[oldRookIndex] = 0;
                }
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
                    board[newRookIndex] = rook;
                    board[oldRookIndex] = 0;
                }
                //Long castles
                else if (newPos == 2)
                {
                    oldRookIndex = 0;
                    newRookIndex = 3;
                    int rook = board[oldRookIndex];
                    board[newRookIndex] = rook;
                    board[oldRookIndex] = 0;
                }
                board[newPos] = movedPiece;
                board[startPos] = 0;
            }
            zobristKey ^= Zobrist.piecesArray[Piece.Rook, currentColorIndex, oldRookIndex];
            zobristKey ^= Zobrist.piecesArray[Piece.Rook, currentColorIndex, newRookIndex];
        }
        //en passant
        else if (move.flag == 7)
        {
            //capture
            board[newPos] = movedPiece;
            board[startPos] = 0;
            int attackedPawnIndex = (colorTurn == Piece.White) ? newPos + 8 : newPos - 8;
            capturedPiece = board[attackedPawnIndex];
            board[attackedPawnIndex] = 0;
            zobristKey ^= Zobrist.piecesArray[Piece.Pawn, oppositeColorIndex, attackedPawnIndex];
            fiftyMoveCounter = 0;
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
                if (colorTurn == Piece.White) { castlingRights &= 0b1100;}
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
                //Removing capturing rights 
                if (Piece.PieceType(capturedPiece) == Piece.Rook)
                {
                    if (colorTurn == Piece.White)
                    {
                        if (newPos == 56) { castlingRights &= 0b1101; }
                        if (newPos == 63) { castlingRights &= 0b1110; }
                    }
                    else if (colorTurn == Piece.Black)
                    {
                        if (newPos == 0) { castlingRights &= 0b0111; }
                        if (newPos == 7) { castlingRights &= 0b1011; }
                    }
                }

                zobristKey ^= Zobrist.piecesArray[Piece.PieceType(capturedPiece), oppositeColorIndex, newPos];

                board[newPos] = newPiece;
                board[startPos] = 0;

                if (Piece.PieceType(capturedPiece) == Piece.King)
                {
                    #if UNITY_EDITOR
                    UnityEngine.Debug.Log("King captured");
                    #endif
                    GameLogger.LogGame(this, 1010101);
                }

                //Capture resets counter
                fiftyMoveCounter = 0;
            }
            else
            {
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
        gameMoveHistory.Pop();

        colorTurn = (colorTurn == Piece.White) ? Piece.Black : Piece.White;
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
        int movedPiece;

        if (move.isPromotion())
        {
            fiftyMoveCounter = 0;
            //Sets the new piece to be the same color but whatever the new piece type is
            movedPiece = Piece.Color(board[newPos]) | Piece.Pawn;
        }
        else
        {
            movedPiece = board[newPos];
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
                }
                //Long castles
                else if (newPos == 58)
                {
                    int rook = board[59];
                    board[56] = rook;
                    board[59] = 0;
                }
                board[startPos] = movedPiece;
                board[newPos] = 0;
            }
            else if (colorTurn == Piece.Black)
            {
                //Short castles
                if (newPos == 6)
                {
                    int rook = board[5];
                    board[7] = rook;
                    board[5] = 0;
                }
                //Long castles
                else if (newPos == 2)
                {
                    int rook = board[3];
                    board[0] = rook;
                    board[3] = 0;
                }
                board[startPos] = movedPiece;
                board[newPos] = 0;
            }

        }
        //en passant
        else if (move.flag == 7)
        {
            //capture
            board[startPos] = movedPiece;
            board[newPos] = 0;

            //Replacing the captured pawn
            int attackedPawnIndex = (colorTurn == Piece.White) ? newPos + 8 : newPos - 8;
            board[attackedPawnIndex] = capturedPiece;
        }
        else
        {
            //Undo the move
            if (move.isCapture())
            {
                //capture
                board[startPos] = movedPiece;
                board[newPos] = capturedPiece;
            }
            else
            {
                board[startPos] = movedPiece;
                board[newPos] = 0;
            }
        }
        plyFromStart--;
    }

    public void MakeNullMove()
    {
        fiftyMoveCounter += 1;
        plyFromStart += 1;
        colorTurn = (colorTurn == Piece.White) ? Piece.Black : Piece.White;
        currentGameState.fiftyMoveCounter = fiftyMoveCounter;
        currentGameState.enPassantFile = 0;
        currentGameState.capturedPiece = 0;
        gameStateHistory.Push(currentGameState);

        enPassantIndex = -1;

        zobristKey ^= Zobrist.sideToMove;
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

    public bool hasNonPawn(int colorTurn)
    {
        for (int index = 0; index < 64; index++)
        {
            if (board[index] != 0)
            {
                if (Piece.IsColour(board[index], colorTurn) && Piece.PieceType(board[index]) != Piece.Pawn)
                {
                    return true;
                }
            }
        }
        return false;
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

        //Part denoting position of each piece
        string posString = fenPosition.Split(' ')[0];
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
                    int pieceType = pieceTypeFromSymbol[char.ToLower(c)];
                    position[index] = pieceType | pieceColour;
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
        gameStateHistory.Push(currentGameState);
        plyFromStart = 0;
        return position;
    }
    public bool IsDraw()
    {
        //Stalemate
        if (moveGenerator.GenerateLegalMoves(this, colorTurn).Count == 0 && !isCurrentPlayerInCheck) { return true; }
        //50 move rule
        if (fiftyMoveCounter >= 100) { return true; }
        if (IsRepetitionDraw()) { return true; }

        //Can be improved with dedicated function
        int numQueens = moveGenerator.GetPosByPieceType(Piece.Queen, Piece.Black, this).Count + moveGenerator.GetPosByPieceType(Piece.Queen, Piece.White, this).Count;
        int numWhiteBishop = moveGenerator.GetPosByPieceType(Piece.Bishop, Piece.White, this).Count;
        int numBlackBishop = moveGenerator.GetPosByPieceType(Piece.Bishop, Piece.Black, this).Count;
        int numWhiteKnight = moveGenerator.GetPosByPieceType(Piece.Knight, Piece.White, this).Count;
        int numBlackKnight = moveGenerator.GetPosByPieceType(Piece.Knight, Piece.Black, this).Count;
        int numRook = moveGenerator.GetPosByPieceType(Piece.Rook, Piece.Black, this).Count + moveGenerator.GetPosByPieceType(Piece.Rook, Piece.White, this).Count;
        int numPawn = moveGenerator.GetPosByPieceType(Piece.Pawn, Piece.Black, this).Count + moveGenerator.GetPosByPieceType(Piece.Pawn, Piece.White, this).Count;

        //Insufficient material
        if (numPawn > 0 || numQueens > 0 || numRook > 0) { return false; }
        else if ((numWhiteBishop + numWhiteKnight) > 1 || (numBlackBishop + numBlackKnight) > 1) { return false; }
        else
        {
            return true;
        }
    }

    public void GenerateMoveGenInfo()
    {
        attackedSquares = moveGenerator.GenerateAllAttackedSquares(this);
        UpdateCheckingInfo();
        UpdatePinnedInfo();
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

    public bool IsCheckmate(int color)
    {
        int kingIndex = moveGenerator.GetKingIndex(color, this);
        if (isCurrentPlayerInCheck)
        {
            //If there are any valid king moves
            if (moveGenerator.GenerateKingMoves(kingIndex, color, this, false).Count != 0)
            {
                return false;
            }
            //check if there are any blocks
            else
            {
                if (moveGenerator.GenerateLegalMoves(this, color).Count == 0) { return true; }
                else { return false; }
            }
        }
        else
        {
            return false;
        }
    }

    //More efficient than checking multiple times for checks
    public void UpdateCheckingInfo()
    {
        checkingPieces = moveGenerator.KingCheckIndexes(colorTurn, this);
        if (checkingPieces.Count > 1)
        {
            blockableIndexes.Clear();
            isCurrentPlayerInCheck = true;
            isCurrentPlayerInDoubleCheck = true;
        }
        else if (checkingPieces.Count == 1)
        {
            isCurrentPlayerInCheck = true;
            isCurrentPlayerInDoubleCheck = false;
            blockableIndexes = moveGenerator.BlockableIndexes(moveGenerator.GetKingIndex(colorTurn, this), checkingPieces[0], this);
        }
        else
        {
            isCurrentPlayerInCheck = false;
            isCurrentPlayerInDoubleCheck = false;
            blockableIndexes.Clear();
        }

    }

    //More efficient than checking multiple times for pinned pieces
    public void UpdatePinnedInfo()
    {
        pinnedIndexes.Clear();
        pinnedIndexes = moveGenerator.PinnedIndexes(moveGenerator.GetKingIndex(colorTurn, this), this);
        pinnedPieceIndexes.Clear();
        for (int x = 0; x < pinnedIndexes.Count; x++)
        {
            pinnedPieceIndexes.Add(pinnedIndexes[x].PinnedPiece);
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
