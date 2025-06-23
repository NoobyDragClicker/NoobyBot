public class Move
{
    public int oldIndex;
    public int newIndex;
    bool capture;

    //0 = none, 1 = promote to queen, 2 = promote to bishop, 3 = promote to knight, 4 = promote to rook, 5 = castle, 6 = double pawn push, 7 = enpeasent
    public int flag;
    
    public Move(int prevIndex, int currIndex, bool capture, int flag){
        oldIndex = prevIndex;
        newIndex = currIndex;
        this.capture = capture;
        this.flag = flag;
    }
    public Move(int prevIndex, int currIndex, bool capture){
        oldIndex = prevIndex;
        newIndex = currIndex;
        flag = 0;
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


    public bool isCapture(){
        return capture;
    }

    public int GetIntValue(){
        return oldIndex + (newIndex << 6) + (flag << 12) + ((capture ? 1 : 0) << 15);
    }

    public bool isPromotion(){
        return (flag > 0 && flag < 5) ? true : false;
    }

    public void printMove(){
        //UnityEngine.Debug.Log("Old position: " + Coord.GetNotationFromIndex(oldIndex) + "  New position: " + Coord.GetNotationFromIndex(newIndex) + "  Is Capture: " + capture.ToString() + "  Flag: " + flag.ToString());
    }
}
