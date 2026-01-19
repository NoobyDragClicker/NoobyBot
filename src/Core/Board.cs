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

    public ulong[] pieceBitboards = new ulong[14];
    public ulong[] sideBitboard = new ulong[2];
    public ulong allPiecesBitboard;

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
    public void Move(Move move, bool isSearch)
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
            BitboardHelper.ClearSquare(ref pieceBitboards[PieceBitboardIndex(currentColorIndex, Piece.Rook)], oldRookIndex);
            BitboardHelper.SetSquare(ref pieceBitboards[PieceBitboardIndex(currentColorIndex, Piece.Rook)], newRookIndex);

            BitboardHelper.ClearSquare(ref sideBitboard[currentColorIndex], oldRookIndex);
            BitboardHelper.SetSquare(ref sideBitboard[currentColorIndex], newRookIndex);

            board[newRookIndex] = rook;
            board[oldRookIndex] = 0;

            //Updating king position
            BitboardHelper.ClearSquare(ref pieceBitboards[PieceBitboardIndex(currentColorIndex, movedPieceType)], startPos);
            BitboardHelper.SetSquare(ref pieceBitboards[PieceBitboardIndex(currentColorIndex, movedPieceType)], newPos);

            BitboardHelper.ClearSquare(ref sideBitboard[currentColorIndex], startPos);
            BitboardHelper.SetSquare(ref sideBitboard[currentColorIndex], newPos);

            if(colorTurn == Piece.White)
            {
                mgValDifference += Evaluation.mg_PSQT[Piece.Rook, newRookIndex] + Evaluation.mg_PSQT[Piece.King, newPos] - Evaluation.mg_PSQT[Piece.Rook, oldRookIndex] - Evaluation.mg_PSQT[Piece.King, startPos];
                egValDifference += Evaluation.eg_PSQT[Piece.Rook, newRookIndex] + Evaluation.eg_PSQT[Piece.King, newPos] - Evaluation.eg_PSQT[Piece.Rook, oldRookIndex] - Evaluation.eg_PSQT[Piece.King, startPos];
            }
            else
            {
                mgValDifference -= Evaluation.mg_PSQT[Piece.Rook, 63 - newRookIndex] + Evaluation.mg_PSQT[Piece.King, 63 - newPos] - Evaluation.mg_PSQT[Piece.Rook, 63 - oldRookIndex] - Evaluation.mg_PSQT[Piece.King, 63 - startPos];
                egValDifference -= Evaluation.eg_PSQT[Piece.Rook, 63 - newRookIndex] + Evaluation.eg_PSQT[Piece.King, 63 - newPos] - Evaluation.eg_PSQT[Piece.Rook, 63 - oldRookIndex] - Evaluation.eg_PSQT[Piece.King, 63 - startPos];
            }
            board[newPos] = movedPiece;
            board[startPos] = 0;

            zobristKey ^= Zobrist.piecesArray[Piece.Rook, currentColorIndex, oldRookIndex];
            zobristKey ^= Zobrist.piecesArray[Piece.Rook, currentColorIndex, newRookIndex];
        }
        //en passant
        else if (move.flag == 7)
        {
            //Moving pawn
            BitboardHelper.ClearSquare(ref pieceBitboards[PieceBitboardIndex(currentColorIndex, Piece.Pawn)], startPos);
            BitboardHelper.SetSquare(ref pieceBitboards[PieceBitboardIndex(currentColorIndex, Piece.Pawn)], newPos);

            BitboardHelper.ClearSquare(ref sideBitboard[currentColorIndex], startPos);
            BitboardHelper.SetSquare(ref sideBitboard[currentColorIndex], newPos);
            board[newPos] = movedPiece;
            board[startPos] = 0;

            //Removing attacked pawn
            int attackedPawnIndex = (colorTurn == Piece.White) ? newPos + 8 : newPos - 8;
            capturedPiece = board[attackedPawnIndex];
            board[attackedPawnIndex] = 0;

            BitboardHelper.ClearSquare(ref pieceBitboards[PieceBitboardIndex(oppositeColorIndex, Piece.Pawn)], attackedPawnIndex);
            BitboardHelper.ClearSquare(ref sideBitboard[oppositeColorIndex], attackedPawnIndex);

            if(colorTurn == Piece.White)
            {
                mgValDifference += Evaluation.mg_PSQT[Piece.Pawn, newPos] - Evaluation.mg_PSQT[Piece.Pawn, startPos] + Evaluation.mg_PSQT[Piece.Pawn, 63 - attackedPawnIndex];
                egValDifference += Evaluation.eg_PSQT[Piece.Pawn, newPos] - Evaluation.eg_PSQT[Piece.Pawn, startPos] + Evaluation.eg_PSQT[Piece.Pawn, 63 - attackedPawnIndex];
            }
            else
            {
                mgValDifference -= Evaluation.mg_PSQT[Piece.Pawn, 63 - newPos] - Evaluation.mg_PSQT[Piece.Pawn, 63 - startPos] + Evaluation.mg_PSQT[Piece.Pawn, attackedPawnIndex];
                egValDifference -= Evaluation.eg_PSQT[Piece.Pawn, 63 - newPos] - Evaluation.eg_PSQT[Piece.Pawn, 63 - startPos] + Evaluation.eg_PSQT[Piece.Pawn, attackedPawnIndex];
            }

            zobristKey ^= Zobrist.piecesArray[Piece.Pawn, oppositeColorIndex, attackedPawnIndex];

            pieceCounts[oppositeColorIndex, Piece.Pawn] -= 1;
            halfMoveClock = 0;
        }
        else
        {
            //Double pawn push
            if (move.flag == 6)
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

                BitboardHelper.ClearSquare(ref pieceBitboards[PieceBitboardIndex(oppositeColorIndex, capturedPieceType)], newPos);
                BitboardHelper.ClearSquare(ref sideBitboard[oppositeColorIndex], newPos);

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
                BitboardHelper.ClearSquare(ref pieceBitboards[PieceBitboardIndex(currentColorIndex, movedPieceType)], startPos);
                BitboardHelper.SetSquare(ref pieceBitboards[PieceBitboardIndex(currentColorIndex, newPieceType)], newPos);

                BitboardHelper.ClearSquare(ref sideBitboard[currentColorIndex], startPos);
                BitboardHelper.SetSquare(ref sideBitboard[currentColorIndex], newPos);

                if(colorTurn == Piece.White)
                {
                    mgValDifference += Evaluation.mg_PSQT[newPieceType, newPos] - Evaluation.mg_PSQT[movedPieceType, startPos] + Evaluation.mg_PSQT[capturedPieceType, 63 - newPos];
                    egValDifference += Evaluation.eg_PSQT[newPieceType, newPos] - Evaluation.eg_PSQT[movedPieceType, startPos] + Evaluation.eg_PSQT[capturedPieceType, 63 - newPos];
                }
                else
                {
                    mgValDifference -= Evaluation.mg_PSQT[newPieceType, 63 - newPos] - Evaluation.mg_PSQT[movedPieceType, 63 - startPos] + Evaluation.mg_PSQT[capturedPieceType, newPos];
                    egValDifference -= Evaluation.eg_PSQT[newPieceType, 63 - newPos] - Evaluation.eg_PSQT[movedPieceType, 63 - startPos] + Evaluation.eg_PSQT[capturedPieceType, newPos];
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
                BitboardHelper.ClearSquare(ref pieceBitboards[PieceBitboardIndex(currentColorIndex, movedPieceType)], startPos);
                BitboardHelper.SetSquare(ref pieceBitboards[PieceBitboardIndex(currentColorIndex, newPieceType)], newPos);

                BitboardHelper.ClearSquare(ref sideBitboard[currentColorIndex], startPos);
                BitboardHelper.SetSquare(ref sideBitboard[currentColorIndex], newPos);

                if (colorTurn == Piece.White)
                {
                    mgValDifference += Evaluation.mg_PSQT[newPieceType, newPos] - Evaluation.mg_PSQT[movedPieceType, startPos];
                    egValDifference += Evaluation.eg_PSQT[newPieceType, newPos] - Evaluation.eg_PSQT[movedPieceType, startPos];
                }
                else
                {
                    mgValDifference -= Evaluation.mg_PSQT[newPieceType, 63 - newPos] - Evaluation.mg_PSQT[movedPieceType, 63 - startPos];
                    egValDifference -= Evaluation.eg_PSQT[newPieceType, 63 - newPos] - Evaluation.eg_PSQT[movedPieceType, 63 - startPos];
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
                    BitboardHelper.ClearSquare(ref pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.Rook)], 61);
                    BitboardHelper.SetSquare(ref pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.Rook)], 63);

                    BitboardHelper.ClearSquare(ref sideBitboard[WhiteIndex], 61);
                    BitboardHelper.SetSquare(ref sideBitboard[WhiteIndex], 63);
                }
                //Long castles
                else if (newPos == 58)
                {
                    int rook = board[59];
                    board[56] = rook;
                    board[59] = 0;
                    BitboardHelper.ClearSquare(ref pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.Rook)], 59);
                    BitboardHelper.SetSquare(ref pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.Rook)], 56);

                    BitboardHelper.ClearSquare(ref sideBitboard[WhiteIndex], 59);
                    BitboardHelper.SetSquare(ref sideBitboard[WhiteIndex], 56);
                }
                board[startPos] = movedPiece;
                board[newPos] = 0;

                BitboardHelper.ClearSquare(ref pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.King)], newPos);
                BitboardHelper.SetSquare(ref pieceBitboards[PieceBitboardIndex(WhiteIndex, Piece.King)], startPos);

                BitboardHelper.ClearSquare(ref sideBitboard[WhiteIndex], newPos);
                BitboardHelper.SetSquare(ref sideBitboard[WhiteIndex], startPos);

            }
            else if (colorTurn == Piece.Black)
            {
                //Short castles
                if (newPos == 6)
                {
                    int rook = board[5];
                    board[7] = rook;
                    board[5] = 0;
                    BitboardHelper.ClearSquare(ref pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.Rook)], 5);
                    BitboardHelper.SetSquare(ref pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.Rook)], 7);

                    BitboardHelper.ClearSquare(ref sideBitboard[BlackIndex], 5);
                    BitboardHelper.SetSquare(ref sideBitboard[BlackIndex], 7);
                }
                //Long castles
                else if (newPos == 2)
                {
                    int rook = board[3];
                    board[0] = rook;
                    board[3] = 0;
                    BitboardHelper.ClearSquare(ref pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.Rook)], 3);
                    BitboardHelper.SetSquare(ref pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.Rook)], 0);

                    BitboardHelper.ClearSquare(ref sideBitboard[BlackIndex], 3);
                    BitboardHelper.SetSquare(ref sideBitboard[BlackIndex], 0);
                }
                board[startPos] = movedPiece;
                board[newPos] = 0;
                BitboardHelper.ClearSquare(ref pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.King)], newPos);
                BitboardHelper.SetSquare(ref pieceBitboards[PieceBitboardIndex(BlackIndex, Piece.King)], startPos);

                BitboardHelper.ClearSquare(ref sideBitboard[BlackIndex], newPos);
                BitboardHelper.SetSquare(ref sideBitboard[BlackIndex], startPos);
            }

        }
        //en passant
        else if (move.flag == 7)
        {
            //capture
            board[startPos] = movedPiece;
            board[newPos] = 0;

            BitboardHelper.ClearSquare(ref pieceBitboards[PieceBitboardIndex(currentColorIndex, Piece.Pawn)], newPos);
            BitboardHelper.SetSquare(ref pieceBitboards[PieceBitboardIndex(currentColorIndex, Piece.Pawn)], startPos);

            BitboardHelper.ClearSquare(ref sideBitboard[currentColorIndex], newPos);
            BitboardHelper.SetSquare(ref sideBitboard[currentColorIndex], startPos);


            //Replacing the captured pawn
            int attackedPawnIndex = (colorTurn == Piece.White) ? newPos + 8 : newPos - 8;
            board[attackedPawnIndex] = capturedPiece;

            BitboardHelper.SetSquare(ref pieceBitboards[PieceBitboardIndex(oppositeColorIndex, Piece.Pawn)], attackedPawnIndex);
            BitboardHelper.SetSquare(ref sideBitboard[oppositeColorIndex], attackedPawnIndex);

            
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
                BitboardHelper.ClearSquare(ref pieceBitboards[PieceBitboardIndex(currentColorIndex, movedPieceType)], newPos);
                BitboardHelper.SetSquare(ref pieceBitboards[PieceBitboardIndex(currentColorIndex, pieceTypeBeforeMove)], startPos);

                BitboardHelper.ClearSquare(ref sideBitboard[currentColorIndex], newPos);
                BitboardHelper.SetSquare(ref sideBitboard[currentColorIndex], startPos);


                board[newPos] = capturedPiece;
                BitboardHelper.SetSquare(ref pieceBitboards[PieceBitboardIndex(oppositeColorIndex, capturedPieceType)], newPos);
                BitboardHelper.SetSquare(ref sideBitboard[oppositeColorIndex], newPos);

                pieceCounts[oppositeColorIndex, capturedPieceType] += 1;
            }
            else
            {
                board[startPos] = movedPiece;
                board[newPos] = 0;
                BitboardHelper.ClearSquare(ref pieceBitboards[PieceBitboardIndex(currentColorIndex, movedPieceType)], newPos);
                BitboardHelper.SetSquare(ref pieceBitboards[PieceBitboardIndex(currentColorIndex, pieceTypeBeforeMove)], startPos);

                BitboardHelper.ClearSquare(ref sideBitboard[currentColorIndex], newPos);
                BitboardHelper.SetSquare(ref sideBitboard[currentColorIndex], startPos);
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
        pieceBitboards = new ulong[14];
        pieceCounts = new int[2, 7];
        sideBitboard = new ulong[2];
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

                    BitboardHelper.SetSquare(ref pieceBitboards[PieceBitboardIndex(colorIndex, pieceType)], index);
                    pieceCounts[colorIndex, pieceType] += 1;

                    if(pieceColour == Piece.White)
                    {
                        mgVal += Evaluation.mg_PSQT[pieceType, index];
                        egVal += Evaluation.eg_PSQT[pieceType, index];
                    }
                    else
                    {
                        mgVal -= Evaluation.mg_PSQT[pieceType, 63 - index];
                        egVal -= Evaluation.eg_PSQT[pieceType, 63 - index];
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

    public void GenerateMoveGenInfo()
    {
        if(!gameStateHistory[fullMoveClock].isMoveGenUpdated)
        {
            if(gameStateHistory[fullMoveClock].attackedSquares == null)
            {
                gameStateHistory[fullMoveClock].attackedSquares = new ulong[2];
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
        return Piece.PieceType(board[move.oldIndex]);
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
    public static int PieceBitboardIndex(int colorIndex, int pieceType)
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

    public ulong GetAttackersToSquare(int square, ulong occupancy, ulong rooks, ulong bishops)
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
    public ulong[] attackedSquares;
    public ulong diagPins;
    public ulong straightPins;
    public ulong checkIndexes;
    public bool isInCheck;
    public bool isMoveGenUpdated;
    public bool isCurrentPlayerInDoubleCheck;
    public int mgPSQTVal;
    public int egPSQTVal;
}
