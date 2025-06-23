public static class Piece
{

    /*************************************************
    Will now keep track of: 
        - piece type(int)
        - piece colour(int either 8 or 16)
        - when viewed in binary, 1st 3 bits will be the piece type, 4th and 5th will be piece colour
        
    *************************************************/
    public const int None = 0;
    public const int Pawn = 1;
    public const int Knight = 2;
    public const int Bishop = 3;
    public const int Rook = 4;
    public const int Queen = 5;
    public const int King = 6;

    //4th and 5th byte in binary
    public const int White = 8;
    public const int Black = 16;

    //Binary masks
    const int typeMask = 0b00111;
	const int blackMask = 0b10000;
	const int whiteMask = 0b01000;
    const int colorMask = whiteMask | blackMask;

    //Utilities
    public static bool IsColour(int piece, int color){
        
        return (piece & colorMask) == color;
    }

    public static int Color(int piece){
        return piece & colorMask;
    }
    public static int PieceType (int piece) {
		return piece & typeMask;
	}


}
