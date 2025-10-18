using System;
using System.Collections.Generic;
using System.IO;


public static class Zobrist
{
    const int seed = 73448693;
    static Random prng = new Random (seed);
    //const string RandomNumberFile = Engine.chessRoot + "/RandomNumbers.txt";
    public static readonly ulong[, , ] piecesArray = new ulong[8, 2, 64];
	public static readonly ulong[] castlingRights = new ulong[16];
	/// ep file (0 = no ep).
	public static readonly ulong[] enPassantFile = new ulong[9];
	public static readonly ulong sideToMove;

    static Zobrist(){
        var randomNumbers = GenerateRandomNumbers();

		for (int squareIndex = 0; squareIndex < 64; squareIndex++) {
			for (int pieceIndex = 0; pieceIndex < 8; pieceIndex++) {
				piecesArray[pieceIndex, Board.WhiteIndex, squareIndex] = randomNumbers.Dequeue ();
				piecesArray[pieceIndex, Board.BlackIndex, squareIndex] = randomNumbers.Dequeue ();
			}
		}

		for (int i = 0; i < 16; i++) {
			castlingRights[i] = randomNumbers.Dequeue();
		}

		for (int i = 0; i < enPassantFile.Length; i++) {
			enPassantFile[i] = randomNumbers.Dequeue();
		}

		sideToMove = randomNumbers.Dequeue();
    }

	//Only used at start of game, all other changes done in Move and Unmove
	public static ulong CalculateZobrist(Board board)
	{
		ulong zobristKey = 0;

		//XORing
		for (int squareIndex = 0; squareIndex < 64; squareIndex++)
		{
			if (board.board[squareIndex] != 0)
			{
				int pieceType = Piece.PieceType(board.board[squareIndex]);
				int pieceColour = Piece.Color(board.board[squareIndex]);

				zobristKey ^= piecesArray[pieceType, (pieceColour == Piece.White) ? Board.WhiteIndex : Board.BlackIndex, squareIndex];
			}
		}

		int epIndex = board.currentGameState.enPassantFile;
		zobristKey ^= enPassantFile[epIndex];
		if (board.colorTurn == Piece.Black)
		{
			zobristKey ^= sideToMove;
		}
		zobristKey ^= castlingRights[board.currentGameState.castlingRights];
		return zobristKey;
	}

    
    //Utils
    //Writes all random numbers to a file
	/*
    static void WriteRandomNumbers(){
        prng = new System.Random(seed);
        string randomNumberString = "";
        //Squares * Piece Types * Color + castling + en passant file + side to move
		int numRandomNumbers = 64 * 8 * 2 + castlingRights.Length + 9 + 1;

		for (int i = 0; i < numRandomNumbers; i++) {
			randomNumberString += RandomUnsigned64BitNumber ();
			if (i != numRandomNumbers - 1) {
				randomNumberString += ',';
			}
		}
		var writer = new StreamWriter (RandomNumberFile);
		writer.Write (randomNumberString);
		writer.Close ();
    }
	//Reads in random numbers (if the file doesnt exist, call WriteRandomNumbers), done at start of the game
	static Queue<ulong> ReadRandomNumbers()
	{
		if (!File.Exists(RandomNumberFile)) { WriteRandomNumbers(); }
		Queue<ulong> randomNumbers = new Queue<ulong>();

		var reader = new StreamReader(RandomNumberFile);
		string numbersString = reader.ReadToEnd();
		reader.Close();

		string[] numberStrings = numbersString.Split(',');
		for (int i = 0; i < numberStrings.Length; i++)
		{
			ulong number = ulong.Parse(numberStrings[i]);
			randomNumbers.Enqueue(number);
		}

		return randomNumbers;
	}*/
	
	static Queue<ulong> GenerateRandomNumbers()
	{
		//Squares * Piece Types * Color + castling + en passant file + side to move 
		int numRandomNumbers = 64 * 8 * 2 + castlingRights.Length + 9 + 1;

		Queue<ulong> randomNumbers = new Queue<ulong>();
		for(int i = 0; i < numRandomNumbers; i++)
        {
            randomNumbers.Enqueue(RandomUnsigned64BitNumber());
        }
		return randomNumbers;
    }
    //Returns a random 64 bit number
    static ulong RandomUnsigned64BitNumber(){
        byte[] buffer = new byte[8];
		prng.NextBytes (buffer);
		return BitConverter.ToUInt64 (buffer, 0);
    }
}
