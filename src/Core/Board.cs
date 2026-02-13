using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;


public class Board
{
    public const string startPos = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public const int WhiteIndex = 0;
    public const int BlackIndex = 1;

    public Bitboard[] pieceBitboards = new Bitboard[14];
    public Bitboard[] sideBitboard = new Bitboard[2];
    public Bitboard allPiecesBitboard;

    public int[,] pieceCounts = new int[2, 7];
    
    public int colorTurn;
    public int currentColorIndex;
    public int oppositeColorIndex;

    //Saves the index where a pawn can capture
    public int enPassantIndex;
    public int halfMoveClock;
    public int fullMoveClock;
    public int[] board = new int[64];

    //Saves info about the game
    public GameState[] gameStateHistory = new GameState[Search.MAX_GAME_PLY];

    public ulong[] zobristHistory = new ulong[Search.MAX_GAME_PLY];
    public ulong zobristKey;
    
    SearchLogger logger;
    public string startFen;


    public void setPosition(string fenPosition, SearchLogger logger)
    {
        this.logger = logger;
        board = ConvertFromFEN(fenPosition);
        zobristKey = Zobrist.CalculateZobrist(this);
        zobristHistory[fullMoveClock] = zobristKey;
        startFen = fenPosition;
    }


    //Moves the pieces
    public void MakeMove(Move move, bool isSearch)
    {
        fullMoveClock++;
        int oldCastlingRights = gameStateHistory[fullMoveClock - 1].castlingRights;
        int castlingRights = oldCastlingRights;

        int oldEPFile = (enPassantIndex != -1) ? Coord.IndexToFile(enPassantIndex) : 0;
        int enPassantFile = 0;

        int startPos = move.oldIndex;
        int newPos = move.newIndex;
        int movedPiece;
        int newPiece; //Used for promotions
        int capturedPiece = 0;

        int mgValDifference = 0;
        int egValDifference = 0;


        //Set to none 
        enPassantIndex = -1;
        movedPiece = board[startPos];
        if (move.isPromotion())
        {
            halfMoveClock = 0;
            //Sets the new piece to be the same color but whatever the new piece type is
            newPiece = ColorAt(startPos) | move.PromotedPieceType();
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
        if (move.flag == Move.Castle)
        {
            int oldRookIndex = 0;
            int newRookIndex = 0;
            int rook = 0;
            halfMoveClock++;
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
                    rook = board[oldRookIndex];
                }
                //Long castles
                else if (newPos == 58)
                {
                    oldRookIndex = 56;
                    newRookIndex = 59;
                    rook = board[oldRookIndex];
                }


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
                    rook = board[oldRookIndex];
                }
                //Long castles
                else if (newPos == 2)
                {
                    oldRookIndex = 0;
                    newRookIndex = 3;
                    rook = board[oldRookIndex];
                }
            }
            
            //Updating rook position
            pieceBitboards[PieceBitboardIndex(currentColorIndex, Piece.Rook)].ClearSquare(oldRookIndex);
            pieceBitboards[PieceBitboardIndex(currentColorIndex, Piece.Rook)].SetSquare(newRookIndex);

            sideBitboard[currentColorIndex].ClearSquare(oldRookIndex);
            sideBitboard[currentColorIndex].SetSquare(newRookIndex);

            board[newRookIndex] = rook;
            board[oldRookIndex] = 0;

            //Updating king position
            pieceBitboards[PieceBitboardIndex(currentColorIndex, movedPieceType)].ClearSquare(startPos);
            pieceBitboards[PieceBitboardIndex(currentColorIndex, movedPieceType)].SetSquare(newPos);

            sideBitboard[currentColorIndex].ClearSquare(startPos);
            sideBitboard[currentColorIndex].SetSquare(newPos);

            if(colorTurn == Piece.White)
            {
                mgValDifference += Evaluation.mg_PSQT[Piece.Rook, newRookIndex] + Evaluation.mg_PSQT[Piece.King, newPos] - Evaluation.mg_PSQT[Piece.Rook, oldRookIndex] - Evaluation.mg_PSQT[Piece.King, startPos];
                egValDifference += Evaluation.eg_PSQT[Piece.Rook, newRookIndex] + Evaluation.eg_PSQT[Piece.King, newPos] - Evaluation.eg_PSQT[Piece.Rook, oldRookIndex] - Evaluation.eg_PSQT[Piece.King, startPos];
            }
            else
            {
                mgValDifference -= Evaluation.mg_PSQT[Piece.Rook, newRookIndex ^ 56] + Evaluation.mg_PSQT[Piece.King, newPos ^ 56] - Evaluation.mg_PSQT[Piece.Rook, oldRookIndex ^ 56] - Evaluation.mg_PSQT[Piece.King, startPos ^ 56];
                egValDifference -= Evaluation.eg_PSQT[Piece.Rook, newRookIndex ^ 56] + Evaluation.eg_PSQT[Piece.King, newPos ^ 56] - Evaluation.eg_PSQT[Piece.Rook, oldRookIndex ^ 56] - Evaluation.eg_PSQT[Piece.King, startPos ^ 56];
            }
            board[newPos] = movedPiece;
            board[startPos] = 0;

            zobristKey ^= Zobrist.piecesArray[Piece.Rook, currentColorIndex, oldRookIndex];
            zobristKey ^= Zobrist.piecesArray[Piece.Rook, currentColorIndex, newRookIndex];
        }
        //en passant
        else if (move.flag == Move.EnPassant)
        {
            //Moving pawn
            pieceBitboards[PieceBitboardIndex(currentColorIndex, Piece.Pawn)].ClearSquare(startPos);
            pieceBitboards[PieceBitboardIndex(currentColorIndex, Piece.Pawn)].SetSquare(newPos);
            
            sideBitboard[currentColorIndex].ClearSquare(startPos);
            sideBitboard[currentColorIndex].SetSquare(newPos);
            board[newPos] = movedPiece;
            board[startPos] = 0;

            //Removing attacked pawn
            int attackedPawnIndex = (colorTurn == Piece.White) ? newPos + 8 : newPos - 8;
            capturedPiece = board[attackedPawnIndex];
            board[attackedPawnIndex] = 0;

            pieceBitboards[PieceBitboardIndex(oppositeColorIndex, Piece.Pawn)].ClearSquare(attackedPawnIndex);
            sideBitboard[oppositeColorIndex].ClearSquare(attackedPawnIndex);

            if(colorTurn == Piece.White)
            {
                mgValDifference += Evaluation.mg_PSQT[Piece.Pawn, newPos] - Evaluation.mg_PSQT[Piece.Pawn, startPos] + Evaluation.mg_PSQT[Piece.Pawn, attackedPawnIndex ^ 56];
                egValDifference += Evaluation.eg_PSQT[Piece.Pawn, newPos] - Evaluation.eg_PSQT[Piece.Pawn, startPos] + Evaluation.eg_PSQT[Piece.Pawn, attackedPawnIndex ^ 56];
            }
            else
            {
                mgValDifference -= Evaluation.mg_PSQT[Piece.Pawn, newPos ^ 56] - Evaluation.mg_PSQT[Piece.Pawn, startPos ^ 56] + Evaluation.mg_PSQT[Piece.Pawn, attackedPawnIndex];
                egValDifference -= Evaluation.eg_PSQT[Piece.Pawn, newPos ^ 56] - Evaluation.eg_PSQT[Piece.Pawn, startPos ^ 56] + Evaluation.eg_PSQT[Piece.Pawn, attackedPawnIndex];
            }

            zobristKey ^= Zobrist.piecesArray[Piece.Pawn, oppositeColorIndex, attackedPawnIndex];

            pieceCounts[oppositeColorIndex, Piece.Pawn] -= 1;
            halfMoveClock = 0;
        }
        else
        {
            //Double pawn push
            if (move.flag == Move.DoublePawnPush)
            {
                //Set to the square behind the spot moved to
                enPassantIndex = (colorTurn == Piece.White) ? (newPos + 8) : (newPos - 8);
                enPassantFile = Coord.IndexToFile(startPos);
            }

            //Once the king has been moved, you can't castle
            if (movedPieceType == Piece.King && castlingRights > 0)
            {
                if (colorTurn == Piece.White) { castlingRights &= 0b1100; }
                else { castlingRights &= 0b0011; }
            }

            //If it's a rook move, check if its from the starting square to remove castling perms
            if (movedPieceType == Piece.Rook && castlingRights > 0)
            {
                if (colorTurn == Piece.White)
                {
                    if (startPos == 56) { castlingRights &= 0b1101; }
                    //Short side
                    else if (startPos == 63) { castlingRights &= 0b1110; }
                }
                else
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

                pieceBitboards[PieceBitboardIndex(oppositeColorIndex, capturedPieceType)].ClearSquare(newPos);
                sideBitboard[oppositeColorIndex].ClearSquare(newPos);

                //Removing castling rights if rook is captured on starting square
                if (capturedPieceType == Piece.Rook && castlingRights > 0)
                {
                    if (colorTurn == Piece.White)
                    {
                        if (newPos == 0) { castlingRights &= 0b0111; }
                        if (newPos == 7) { castlingRights &= 0b1011; }
                    }
                    else
                    {
                        if (newPos == 56) { castlingRights &= 0b1101; }
                        if (newPos == 63) { castlingRights &= 0b1110; }
                    }
                }

                zobristKey ^= Zobrist.piecesArray[capturedPieceType, oppositeColorIndex, newPos];

                //Moving capturing piece
                pieceBitboards[PieceBitboardIndex(currentColorIndex, movedPieceType)].ClearSquare(startPos);
                pieceBitboards[PieceBitboardIndex(currentColorIndex, newPieceType)].SetSquare(newPos);

                sideBitboard[currentColorIndex].ClearSquare(startPos);
                sideBitboard[currentColorIndex].SetSquare(newPos);

                if(colorTurn == Piece.White)
                {
                    mgValDifference += Evaluation.mg_PSQT[newPieceType, newPos] - Evaluation.mg_PSQT[movedPieceType, startPos] + Evaluation.mg_PSQT[capturedPieceType, newPos ^ 56];
                    egValDifference += Evaluation.eg_PSQT[newPieceType, newPos] - Evaluation.eg_PSQT[movedPieceType, startPos] + Evaluation.eg_PSQT[capturedPieceType, newPos ^ 56];
                }
                else
                {
                    mgValDifference -= Evaluation.mg_PSQT[newPieceType, newPos ^ 56] - Evaluation.mg_PSQT[movedPieceType, startPos ^ 56] + Evaluation.mg_PSQT[capturedPieceType, newPos];
                    egValDifference -= Evaluation.eg_PSQT[newPieceType, newPos ^ 56] - Evaluation.eg_PSQT[movedPieceType, startPos ^ 56] + Evaluation.eg_PSQT[capturedPieceType, newPos];
                }


                board[newPos] = newPiece;
                board[startPos] = 0;

                pieceCounts[oppositeColorIndex, capturedPieceType] -= 1;

                if (capturedPieceType == Piece.King)
                {
                    logger.AddToLog("King captured", SearchLogger.LoggingLevel.Deadly);
                    logger.AddToLog(ConvertToFEN(), SearchLogger.LoggingLevel.Deadly);
                }

                //Capture resets counter
                halfMoveClock = 0;
            }
            else
            {
                pieceBitboards[PieceBitboardIndex(currentColorIndex, movedPieceType)].ClearSquare(startPos);
                pieceBitboards[PieceBitboardIndex(currentColorIndex, newPieceType)].SetSquare(newPos);

                sideBitboard[currentColorIndex].ClearSquare(startPos);
                sideBitboard[currentColorIndex].SetSquare(newPos);

                if (colorTurn == Piece.White)
                {
                    mgValDifference += Evaluation.mg_PSQT[newPieceType, newPos] - Evaluation.mg_PSQT[movedPieceType, startPos];
                    egValDifference += Evaluation.eg_PSQT[newPieceType, newPos] - Evaluation.eg_PSQT[movedPieceType, startPos];
                }
                else
                {
                    mgValDifference -= Evaluation.mg_PSQT[newPieceType, newPos ^ 56] - Evaluation.mg_PSQT[movedPieceType, startPos ^ 56];
                    egValDifference -= Evaluation.eg_PSQT[newPieceType, newPos ^ 56] - Evaluation.eg_PSQT[movedPieceType, startPos ^ 56];
                }
                
                board[newPos] = newPiece;
                board[startPos] = 0;

                //Pawn push resets counter
                if (movedPieceType == Piece.Pawn)
                {
                    halfMoveClock = 0;
                }
                else
                {
                    halfMoveClock++;
                }
            }
        }

        colorTurn = (colorTurn == Piece.White) ? Piece.Black : Piece.White;

        allPiecesBitboard = sideBitboard[WhiteIndex] | sideBitboard[BlackIndex];

        //Update gamestate
        gameStateHistory[fullMoveClock].capturedPiece = capturedPiece;
        gameStateHistory[fullMoveClock].enPassantFile = enPassantFile;
        gameStateHistory[fullMoveClock].halfMoveClock = halfMoveClock;
        gameStateHistory[fullMoveClock].castlingRights = castlingRights;
        gameStateHistory[fullMoveClock].isMoveGenUpdated = false;
        gameStateHistory[fullMoveClock].mgPSQTVal = gameStateHistory[fullMoveClock - 1].mgPSQTVal + mgValDifference;
        gameStateHistory[fullMoveClock].egPSQTVal = gameStateHistory[fullMoveClock - 1].egPSQTVal + egValDifference;
        //Moving friendly piece
        zobristKey ^= Zobrist.piecesArray[movedPieceType, currentColorIndex, startPos];
        zobristKey ^= Zobrist.piecesArray[newPieceType, currentColorIndex, newPos];

        oppositeColorIndex = currentColorIndex;
        currentColorIndex = 1 - currentColorIndex;
        UpdateSimpleCheckStatus();


        if(oldCastlingRights != castlingRights)
        {
            zobristKey ^= Zobrist.castlingRights[oldCastlingRights];
            zobristKey ^= Zobrist.castlingRights[castlingRights];
        }
        if(oldEPFile != enPassantFile)
        {
            zobristKey ^= Zobrist.enPassantFile[oldEPFile];
            zobristKey ^= Zobrist.enPassantFile[enPassantFile];
        }
        
        zobristKey ^= Zobrist.sideToMove;
        zobristHistory[fullMoveClock] = zobristKey;
    }
    public void UndoMove(Move move)
    {
        fullMoveClock--;
        colorTurn = (colorTurn == Piece.White) ? Piece.Black : Piece.White;
        oppositeColorIndex = currentColorIndex;
        currentColorIndex = 1 - currentColorIndex;
        //Removing the current one and getting the required info
        GameState oldGameStateHistory = gameStateHistory[fullMoveClock + 1];

        //Getting the game state from the current move
        int capturedPiece = oldGameStateHistory.capturedPiece;
        int enPassantFile = gameStateHistory[fullMoveClock].enPassantFile;
        halfMoveClock = gameStateHistory[fullMoveClock].halfMoveClock;

        zobristKey = zobristHistory[fullMoveClock];

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
            //Sets the new piece to be the same color but whatever the new piece type is
            movedPiece = ColorAt(newPos) | Piece.Pawn;
            pieceTypeBeforeMove = Piece.Pawn;
            pieceCounts[currentColorIndex, Piece.Pawn] += 1;
            pieceCounts[currentColorIndex, movedPieceType] -= 1;
        }

        //castle
        if (move.flag == Move.Castle)
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
                    pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.Rook)].ClearSquare(61);
                    pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.Rook)].SetSquare(63);

                    sideBitboard[WhiteIndex].ClearSquare(61);
                    sideBitboard[WhiteIndex].SetSquare(63);
                }
                //Long castles
                else if (newPos == 58)
                {
                    int rook = board[59];
                    board[56] = rook;
                    board[59] = 0;
                    pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.Rook)].ClearSquare(59);
                    pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.Rook)].SetSquare(56);

                    sideBitboard[WhiteIndex].ClearSquare(59);
                    sideBitboard[WhiteIndex].SetSquare(56);
                }
                board[startPos] = movedPiece;
                board[newPos] = 0;

                pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.King)].ClearSquare(newPos);
                pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.King)].SetSquare(startPos);

                sideBitboard[WhiteIndex].ClearSquare(newPos);
                sideBitboard[WhiteIndex].SetSquare(startPos);

            }
            else if (colorTurn == Piece.Black)
            {
                //Short castles
                if (newPos == 6)
                {
                    int rook = board[5];
                    board[7] = rook;
                    board[5] = 0;
                    pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.Rook)].ClearSquare(5);
                    pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.Rook)].SetSquare(7);

                    sideBitboard[BlackIndex].ClearSquare(5);
                    sideBitboard[BlackIndex].SetSquare(7);
                }
                //Long castles
                else if (newPos == 2)
                {
                    int rook = board[3];
                    board[0] = rook;
                    board[3] = 0;
                    pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.Rook)].ClearSquare(3);
                    pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.Rook)].SetSquare(0);

                    sideBitboard[BlackIndex].ClearSquare(3);
                    sideBitboard[BlackIndex].SetSquare(0);
                }
                board[startPos] = movedPiece;
                board[newPos] = 0;
                pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.King)].ClearSquare(newPos);
                pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.King)].SetSquare(startPos);

                sideBitboard[BlackIndex].ClearSquare(newPos);
                sideBitboard[BlackIndex].SetSquare(startPos);
            }

        }
        //en passant
        else if (move.flag == Move.EnPassant)
        {
            //capture
            board[startPos] = movedPiece;
            board[newPos] = 0;

            pieceBitboards[PieceBitboardIndex(currentColorIndex, Piece.Pawn)].ClearSquare(newPos);
            pieceBitboards[PieceBitboardIndex(currentColorIndex, Piece.Pawn)].SetSquare(startPos);

            sideBitboard[currentColorIndex].ClearSquare(newPos);
            sideBitboard[currentColorIndex].SetSquare(startPos);


            //Replacing the captured pawn
            int attackedPawnIndex = (colorTurn == Piece.White) ? newPos + 8 : newPos - 8;
            board[attackedPawnIndex] = capturedPiece;

            pieceBitboards[PieceBitboardIndex(oppositeColorIndex, Piece.Pawn)].SetSquare(attackedPawnIndex);
            sideBitboard[oppositeColorIndex].SetSquare(attackedPawnIndex);

            
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
                pieceBitboards[PieceBitboardIndex(currentColorIndex, movedPieceType)].ClearSquare(newPos);
                pieceBitboards[PieceBitboardIndex(currentColorIndex, pieceTypeBeforeMove)].SetSquare(startPos);

                sideBitboard[currentColorIndex].ClearSquare(newPos);
                sideBitboard[currentColorIndex].SetSquare(startPos);


                board[newPos] = capturedPiece;
                pieceBitboards[PieceBitboardIndex(oppositeColorIndex, capturedPieceType)].SetSquare(newPos);
                sideBitboard[oppositeColorIndex].SetSquare(newPos);

                pieceCounts[oppositeColorIndex, capturedPieceType] += 1;
            }
            else
            {
                board[startPos] = movedPiece;
                board[newPos] = 0;
                pieceBitboards[PieceBitboardIndex(currentColorIndex, movedPieceType)].ClearSquare(newPos);
                pieceBitboards[PieceBitboardIndex(currentColorIndex, pieceTypeBeforeMove)].SetSquare(startPos);

                sideBitboard[currentColorIndex].ClearSquare(newPos);
                sideBitboard[currentColorIndex].SetSquare(startPos);
            }
        }
        
        //sideBitboard[WhiteIndex] = pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.Pawn)] | pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.Knight)] | pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.Bishop)] | pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.Rook)] | pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.Queen)] | pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.King)];
        //sideBitboard[BlackIndex] = pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.Pawn)] | pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.Knight)] | pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.Bishop)] | pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.Rook)] | pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.Queen)] | pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.King)];
        allPiecesBitboard = sideBitboard[WhiteIndex] | sideBitboard[BlackIndex];
    }

    public void MakeNullMove()
    {
        halfMoveClock++;
        fullMoveClock++;
        colorTurn = (colorTurn == Piece.White) ? Piece.Black : Piece.White;
        oppositeColorIndex = currentColorIndex;
        currentColorIndex = 1 - currentColorIndex;

        int oldEPFile = gameStateHistory[fullMoveClock - 1].enPassantFile;
        
        gameStateHistory[fullMoveClock].halfMoveClock = halfMoveClock;
        gameStateHistory[fullMoveClock].enPassantFile = 0;
        gameStateHistory[fullMoveClock].capturedPiece = 0;
        gameStateHistory[fullMoveClock].isInCheck = false;
        gameStateHistory[fullMoveClock].isCurrentPlayerInDoubleCheck = false;
        gameStateHistory[fullMoveClock].isMoveGenUpdated = false;
        gameStateHistory[fullMoveClock].castlingRights = gameStateHistory[fullMoveClock - 1].castlingRights;
        gameStateHistory[fullMoveClock].mgPSQTVal = gameStateHistory[fullMoveClock - 1].mgPSQTVal;
        gameStateHistory[fullMoveClock].egPSQTVal = gameStateHistory[fullMoveClock - 1].egPSQTVal;

        enPassantIndex = -1;

        zobristKey ^= Zobrist.sideToMove;
        zobristKey ^= Zobrist.enPassantFile[oldEPFile];
        zobristKey ^= Zobrist.enPassantFile[0];
        zobristHistory[fullMoveClock] = zobristKey;
    }
    public void UnmakeNullMove()
    {
        halfMoveClock--;
        fullMoveClock--;

        zobristKey = zobristHistory[fullMoveClock];
        if (gameStateHistory[fullMoveClock].enPassantFile != 0)
        {
            enPassantIndex = EnPassantFileToIndex(colorTurn, gameStateHistory[fullMoveClock].enPassantFile);
        }
        else
        {
            enPassantIndex = -1;
        }
        colorTurn = (colorTurn == Piece.White) ? Piece.Black : Piece.White;
        oppositeColorIndex = currentColorIndex;
        currentColorIndex = 1 - currentColorIndex;
    }

    public int[] ConvertFromFEN(string fenPosition)
    {

        gameStateHistory = new GameState[Search.MAX_GAME_PLY];
        zobristHistory = new ulong[Search.MAX_GAME_PLY];
        pieceBitboards = new Bitboard[14];
        pieceCounts = new int[2, 7];
        sideBitboard = new Bitboard[2];
        allPiecesBitboard = 0;

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
        int mgVal = 0;
        int egVal = 0;
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
                    
                    pieceBitboards[PieceBitboardIndex(colorIndex, pieceType)].SetSquare(index);
                    pieceCounts[colorIndex, pieceType] += 1;

                    if(pieceColour == Piece.White)
                    {
                        mgVal += Evaluation.mg_PSQT[pieceType, index];
                        egVal += Evaluation.eg_PSQT[pieceType, index];
                    }
                    else
                    {
                        mgVal -= Evaluation.mg_PSQT[pieceType, index ^ 56];
                        egVal -= Evaluation.eg_PSQT[pieceType, index ^ 56];
                    }

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

        sideBitboard[WhiteIndex] = pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.Pawn)] | pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.Knight)] | pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.Bishop)] | pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.Rook)] | pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.Queen)] | pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.King)];
        sideBitboard[BlackIndex] = pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.Pawn)] | pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.Knight)] | pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.Bishop)] | pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.Rook)] | pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.Queen)] | pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.King)];
        allPiecesBitboard = sideBitboard[WhiteIndex] | sideBitboard[BlackIndex];

        //Loads who's move it is
        string sidetoMove = fenPosition.Split(' ')[1];
        if (sidetoMove == "w") { colorTurn = Piece.White; currentColorIndex = WhiteIndex; }
        else if (sidetoMove == "b") { colorTurn = Piece.Black; currentColorIndex = BlackIndex; }
        oppositeColorIndex = 1 - currentColorIndex;
        

        string castling = fenPosition.Split(' ')[2];

        int castlingRights = 0;
        if (castling.Contains("K")) { castlingRights += 1; }
        if (castling.Contains("Q")) { castlingRights += 2; }
        if (castling.Contains("k")) { castlingRights += 4; }
        if (castling.Contains("q")) { castlingRights += 8; }
        
        enPassantIndex = -1;
        if (fenComponents.Length >= 4)
        {
            if (fenComponents[3] != "-")
            {
                enPassantIndex = Coord.NotationToIndex(fenComponents[3]);
            }
            if (fenComponents.Length >= 5)
            {
                try
                {
                    halfMoveClock = int.Parse(fenComponents[4]);
                }
                catch (Exception) { }

            }
            if (fenComponents.Length >= 6)
            {
                try
                {
                    fullMoveClock = int.Parse(fenComponents[5]);
                }
                catch (Exception) { }
            }
        }
        
        UpdateSimpleCheckStatus();
        gameStateHistory[fullMoveClock].capturedPiece = 0;
        gameStateHistory[fullMoveClock].castlingRights = castlingRights;
        gameStateHistory[fullMoveClock].enPassantFile = 0;
        gameStateHistory[fullMoveClock].halfMoveClock = halfMoveClock;
        gameStateHistory[fullMoveClock].mgPSQTVal = mgVal;
        gameStateHistory[fullMoveClock].egPSQTVal = egVal;
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
                if (ColorAt(index) == Piece.White)
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

        if(emptyCounter != 0) {fen += emptyCounter.ToString();}
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
        }
        else { fen += " -"; }
        fen += $" {halfMoveClock}";
        fen += $" {fullMoveClock}";
        return fen;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitboard GetPieces(int colorIndex, int pieceType)
    {
        return pieceBitboards[PieceBitboardIndex(colorIndex, pieceType)];
    }

    public void GenerateMoveGenInfo()
    {
        if(!gameStateHistory[fullMoveClock].isMoveGenUpdated)
        {
            if(gameStateHistory[fullMoveClock].attackedSquares == null)
            {
                gameStateHistory[fullMoveClock].attackedSquares = new Bitboard[2];
            }
            gameStateHistory[fullMoveClock].attackedSquares[WhiteIndex] = MoveGenerator.GenerateAttackedSquares(this, Piece.White);
            gameStateHistory[fullMoveClock].attackedSquares[BlackIndex] = MoveGenerator.GenerateAttackedSquares(this, Piece.Black);
            MoveGenerator.UpdateChecksAndPins(this);
        }    
    }

    public void UpdateSimpleCheckStatus()
    {
        gameStateHistory[fullMoveClock].isInCheck = MoveGenerator.DetermineCheckStatus(this);
    }

    public bool IsRepetitionDraw()
    {
        int repCount = 0;
        for(int index = fullMoveClock; index >= ((fullMoveClock - halfMoveClock) > 0 ? (fullMoveClock - halfMoveClock) : 0); index--)
        {
            if (zobristHistory[index] == zobristKey) { repCount++; }
            if(repCount >= 2){ return true; }
        }
        return false;
    }

    public int MovedPieceType(Move move)
    {
        return PieceAt(move.oldIndex);
    }

    public bool IsSearchDraw()
    {
        if (halfMoveClock >= 100) { return true; }
        else if (IsRepetitionDraw()) { return true; }
        else
        {
            if (pieceCounts[WhiteIndex, Piece.Pawn] + pieceCounts[BlackIndex, Piece.Pawn] +
                pieceCounts[WhiteIndex, Piece.Rook] + pieceCounts[BlackIndex, Piece.Rook] +
                pieceCounts[WhiteIndex, Piece.Queen] + pieceCounts[BlackIndex, Piece.Queen]
                != 0) { return false; }
            else
            {
                int whiteMinorPieces = pieceCounts[WhiteIndex, Piece.Knight] + pieceCounts[WhiteIndex, Piece.Bishop];
                int blackMinorPieces = pieceCounts[BlackIndex, Piece.Knight] + pieceCounts[BlackIndex, Piece.Bishop];
                if ((whiteMinorPieces == 1 && blackMinorPieces == 0) || (whiteMinorPieces == 0 && blackMinorPieces == 1) || (whiteMinorPieces == 0 && blackMinorPieces == 0)) { return true; }
                else { return false; }
            }
        }
    }
    public bool IsCheckmate()
    {
        int kingIndex = MoveGenerator.GetKingIndex(colorTurn, this);
        
        if (gameStateHistory[fullMoveClock].isInCheck)
        {
            GenerateMoveGenInfo();
            Span<Move> legalKingMoves = stackalloc Move[218];
            int currMoveIndex = MoveGenerator.GenerateKingMoves(legalKingMoves, 0, this);
            //If there are any valid king moves
            if (currMoveIndex != 0)
            {
                return false;
            }
            //check if there are any blocks
            else
            {
                Span<Move> legalMoves = new Move[256];
                int moveIndex = MoveGenerator.GenerateLegalMoves(this, ref legalMoves);
                if (moveIndex == 0) { return true; }
                else { return false; }
            }
        }
        else
        {
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int PieceBitboardIndex(int colorIndex, int pieceType)
    {
        return (colorIndex * 7) + pieceType;
    }
    //Gets them from the current gamestate
    public bool HasKingsideRight(int color)
    {
        if (color == Piece.White && (gameStateHistory[fullMoveClock].castlingRights & 0b1) == 1)
        {
            return true;
        }
        else if (color == Piece.Black && (gameStateHistory[fullMoveClock].castlingRights & 0b100) == 4)
        {
            return true;
        }
        else { return false; }
    }
    public bool HasQueensideRight(int color)
    {
        if (color == Piece.White && (gameStateHistory[fullMoveClock].castlingRights & 0b000010) == 2)
        {
            return true;
        }
        else if (color == Piece.Black && (gameStateHistory[fullMoveClock].castlingRights & 0b001000) == 8)
        {
            return true;
        }
        else { return false; }
    }

    public Bitboard GetAttackersToSquare(int square, Bitboard occupancy, Bitboard rooks, Bitboard bishops)
    {
        return(
            (BitboardHelper.GetRookAttacks(square, occupancy) & rooks)
            | (BitboardHelper.GetBishopAttacks(square, occupancy) & bishops) 
            | (BitboardHelper.knightAttacks[square] & (pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.Knight)] | pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.Knight)]))
            | (BitboardHelper.wPawnAttacks[square] & pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.Pawn)])
            | (BitboardHelper.bPawnAttacks[square] & pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.Pawn)])
            | (BitboardHelper.kingAttacks[square] & (pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.King)] | pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.King)]))
            );
    }

    public int PieceAt(int index)
    {
        return Piece.PieceType(board[index]);
    }

    public int ColorAt(int index)
    {
        return Piece.Color(board[index]);
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
    public int halfMoveClock;
    public Bitboard[] attackedSquares;
    public Bitboard diagPins;
    public Bitboard straightPins;
    public Bitboard checkIndexes;
    public bool isInCheck;
    public bool isMoveGenUpdated;
    public bool isCurrentPlayerInDoubleCheck;
    public int mgPSQTVal;
    public int egPSQTVal;
}
