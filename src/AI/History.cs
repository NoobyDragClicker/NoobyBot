public class History
{
    const int HISTORY_MAX = 32768;
    const int HISTORY_MULTIPLE = 300;
    const int HISTORY_SUB = 250;

    public (Move, int)[] movesAndPieceTypes = new (Move, int)[Search.MAX_GAME_PLY];

    public Move[] killers = new Move[Search.MAX_GAME_PLY];
    Board board;
    
    //Color turn, from, to
    public int[,,] quietHistory = new int[2, 64, 64];
    public int[] continuationHistory = new int[2 * 7 * 64 * 2 * 7 * 64];
    //Color turn, to, moved piece, captured piece
    public int[,,,] captureHistory = new int[2, 64, 7, 7];

    public History(Board board)
    {
        this.board = board;
    }

    public void UpdateQuietTables(Move move, int depth)
    {
        killers[board.fullMoveClock] = move;

        int bonus = HISTORY_MULTIPLE * depth - HISTORY_SUB;
        ApplyHistoryBonus(move, bonus);

        if(board.fullMoveClock > 0)
        {
            ApplyContHistBonus(move, bonus);
        }
    }

    public void ApplyQuietPenalties(ref Span<Move> moves, int startNum, int depth)
    {
        for (int i = startNum - 1; i >= 0; i--)
        {
            if (!moves[i].isCapture())
            {
                ApplyHistoryBonus(moves[i], -(300 * depth - 250));
                if(board.fullMoveClock > 0)
                {
                    ApplyContHistBonus(moves[i], -(300 * depth - 250));
                }
            }
        }
    }

    public void ApplyNoisyPenalties(ref Span<Move> moves, int startNum, int depth)
    {
        for (int i = startNum - 1; i >= 0; i--)
        {
            if (moves[i].isCapture())
            {
                ApplyCaptHistBonus(moves[i], -(300 * depth - 250));
            }
        }
    }

    int CalculateNewScore(int score, int bonus)
    {
        int clampedBonus = Math.Clamp(bonus, -HISTORY_MAX, HISTORY_MAX);
        return score + clampedBonus - score * Math.Abs(clampedBonus) / HISTORY_MAX;
    }

    void ApplyHistoryBonus(Move move, int bonus)
    {
        quietHistory[board.currentColorIndex, move.oldIndex, move.newIndex] = CalculateNewScore(quietHistory[board.currentColorIndex, move.oldIndex, move.newIndex], bonus);
    }

    public void ApplyCaptHistBonus(Move move, int bonus)
    {
        captureHistory[board.currentColorIndex, move.newIndex, board.MovedPieceType(move), board.PieceAt(move.newIndex)] = CalculateNewScore(captureHistory[board.currentColorIndex, move.newIndex, board.MovedPieceType(move), board.PieceAt(move.newIndex)], bonus);
    }

    void ApplyContHistBonus(Move move, int bonus)
    {
        int contHistIndex = FlattenConthistIndex(board.oppositeColorIndex, movesAndPieceTypes[board.fullMoveClock - 1].Item2, movesAndPieceTypes[board.fullMoveClock - 1].Item1.newIndex, board.currentColorIndex, board.MovedPieceType(move), move.newIndex);
        continuationHistory[contHistIndex] = CalculateNewScore(continuationHistory[contHistIndex], bonus);
    }
    
    public int FlattenConthistIndex(int prevColor, int prevPiece, int prevTo, int currColor, int currPiece, int currTo)
    {
        return ((((prevColor * 7 + prevPiece) * 64 + prevTo) * 2 + currColor) * 7 + currPiece) * 64 + currTo;
    }

    
}