using System.Runtime.CompilerServices;
public struct Move
{
    public byte oldIndex;
    public byte newIndex;
    public byte flag;

    bool capture;

    //0 = none, 1 = promote to queen, 2 = promote to bishop, 3 = promote to knight, 4 = promote to rook, 5 = castle, 6 = double pawn push, 7 = enpeasent

    public const int None = 0;
    public const int QueenPromo = 1;
    public const int BishopPromo = 2;
    public const int KnightPromo = 3;
    public const int RookPromo = 4;
    public const int Castle = 5;
    public const int DoublePawnPush = 6;
    public const int EnPassant = 7;
    
    public Move(ushort bits)
    {
        oldIndex = (byte)(bits & 0b0000000000111111);
        newIndex = (byte)((bits >> 6) & 0b0000000000111111);
        flag = (byte)((bits >> 12) & 0b0000000000000111);
        capture = ((bits >> 15) & 1) != 0;
    }

    public Move(int prevIndex, int currIndex, bool capture, int flag)
    {
        oldIndex = (byte)prevIndex;
        newIndex = (byte)currIndex;
        this.flag = (byte)flag;
        this.capture = capture;
    }
    public Move(int prevIndex, int currIndex, bool capture){
        oldIndex = (byte)prevIndex;
        newIndex = (byte)currIndex;
        flag = None;
        this.capture = capture;
    }

    public int PromotedPieceType(){
        switch(flag){
            case 1: return Piece.Queen;
            case 2: return Piece.Bishop;
            case 3: return Piece.Knight;
            case 4: return Piece.Rook;
            default: return Piece.None;
        }
    }

    public bool isNull()
    {
        return oldIndex == 0 && newIndex == 0;
    }


    public bool isCapture()
    {
        return capture;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ushort(Move move)
    {
        return (ushort)(move.oldIndex | (move.newIndex << 6) | (move.flag << 12) | ((move.capture ? 1 : 0) << 15));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator int(Move move)
    {
        return move.oldIndex | (move.newIndex << 6) | (move.flag << 12) | ((move.capture ? 1 : 0) << 15);
    }

    public bool isPromotion(){
        return (flag > None && flag < Castle) ? true : false;
    }

    public void printMove(){
        //UnityEngine.Debug.Log("Old position: " + Coord.GetNotationFromIndex(oldIndex) + "  New position: " + Coord.GetNotationFromIndex(newIndex) + "  Is Capture: " + capture.ToString() + "  Flag: " + flag.ToString());
    }
}
