using System.Runtime.CompilerServices;

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
    public int[,,] pieceToHistory = new int[2, 7, 64];
    public int[] continuationHistory = new int[7 * 64 * 7 * 64 * 2];
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
        ApplyConthistBonuses(move, bonus); 
    }
    public void UpdateQuietHistories(Move move, int depth)
    {
        int bonus = HISTORY_MULTIPLE * depth - HISTORY_SUB;
        ApplyHistoryBonus(move, bonus);
        ApplyConthistBonuses(move, bonus);
    }

    public void ApplyQuietPenalties(ref Span<Move> moves, int startNum, int depth)
    {
        for (int i = startNum - 1; i >= 0; i--)
        {
            if (!moves[i].isCapture())
            {
                ApplyHistoryBonus(moves[i], -(300 * depth - 250));
                ApplyConthistBonuses(moves[i], -(300 * depth - 250));
            }
        }
    }

    public void ApplyNoisyPenalties(ref Span<Move> moves, int startNum, int depth)
    {
        for (int i = startNum - 1; i >= 0; i--)
        {
            if (moves[i].isCapture())
            {
                ApplyCapthistBonus(moves[i], -(300 * depth - 250));
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
        pieceToHistory[board.currentColorIndex, board.MovedPieceType(move), move.newIndex] = CalculateNewScore(pieceToHistory[board.currentColorIndex, board.MovedPieceType(move), move.newIndex], bonus);
        quietHistory[board.currentColorIndex, move.oldIndex, move.newIndex] = CalculateNewScore(quietHistory[board.currentColorIndex, move.oldIndex, move.newIndex], bonus);
    }

    public void ApplyCapthistBonus(Move move, int bonus)
    {
        captureHistory[board.currentColorIndex, move.newIndex, board.MovedPieceType(move), board.PieceAt(move.newIndex)] = CalculateNewScore(captureHistory[board.currentColorIndex, move.newIndex, board.MovedPieceType(move), board.PieceAt(move.newIndex)], bonus);
    }

    void ApplyConthistBonuses(Move move, int bonus)
    {
        if (board.fullMoveClock > 0)
        {
            int contHistIndex = FlattenConthistIndex(movesAndPieceTypes[board.fullMoveClock - 1].Item2, movesAndPieceTypes[board.fullMoveClock - 1].Item1.newIndex, board.MovedPieceType(move), move.newIndex, board.currentColorIndex);
            continuationHistory[contHistIndex] = CalculateNewScore(continuationHistory[contHistIndex], bonus);
        }
    }

    public int GetConthistScores(Move move)
    {
        if(board.fullMoveClock > 0)
        {
            return continuationHistory[FlattenConthistIndex(movesAndPieceTypes[board.fullMoveClock - 1].Item2, movesAndPieceTypes[board.fullMoveClock - 1].Item1.newIndex, board.MovedPieceType(move), move.newIndex, board.currentColorIndex)];
        }
        return 0;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int FlattenConthistIndex(int prevPiece, int prevTo, int currPiece, int currTo, int currColor)
    {
        return (((prevPiece * 64 + prevTo) * 7 + currPiece) * 64 + currTo) * 2 + currColor;
    }

    
}