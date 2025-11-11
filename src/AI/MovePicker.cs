public class MovePicker
{
    public enum Stage { TTMove, Other, Finished }
    public Stage currentStage;

    int maxIndexInStage;
    int currentIndexInStage;
    Move ttMove;
    Board board;
    MoveOrder moveOrder;
    int[] moveScores = new int[1];
    bool genNextStage;
    bool isQuiescence;
    bool isTTMoveNull;

    public MovePicker(Move ttMove, Board board, MoveOrder moveOrder, bool isQuiescence)
    {
        this.ttMove = ttMove;
        this.board = board;
        this.moveOrder = moveOrder;
        this.isQuiescence = isQuiescence;
        genNextStage = true;
        isTTMoveNull = ttMove.isNull();
        currentStage = isTTMoveNull ? Stage.Other : Stage.TTMove;
    }

    public Move GetNextMove(ref Span<Move> moves)
    {
        if (currentStage == Stage.TTMove)
        {
            currentStage++;
            genNextStage = true;
            return ttMove;
        }

        if (currentStage == Stage.Other && genNextStage)
        {
            GenerateAllMoves(ref moves);
            genNextStage = false;
            if (maxIndexInStage <= -1) {currentStage++; return Search.nullMove; }
        }

        Move bestMove = GetMoveFromList(ref moves);

        currentIndexInStage++;
        if (currentIndexInStage >= maxIndexInStage)
        {
            currentStage++;
        }
        
        return bestMove;
    }

    Move GetMoveFromList(ref Span<Move> moves)
    {
        
        //Take the index the search is currently at
        int highest = currentIndexInStage;
        for (int i = highest + 1; i < moveScores.Length; i++)
        {
            //Find the next highest score
            if (moveScores[i] > moveScores[highest]) { highest = i; }
        }

        //Swap the next highest move into the spot that is about to be searched, hoping for a quick beta cutoff
        Move bestMove = moves[highest];
        int tempScore = moveScores[highest];
        moves[highest] = moves[currentIndexInStage];
        moveScores[highest] = moveScores[currentIndexInStage];
        moves[currentIndexInStage] = bestMove;
        moveScores[currentIndexInStage] = tempScore;
        return bestMove;
    }
    
    void GenerateAllMoves(ref Span<Move> moves)
    {
        currentIndexInStage = 0;
        maxIndexInStage = MoveGenerator.GenerateLegalMoves(board, ref moves, board.colorTurn, currentIndexInStage, isQuiescence) - 1;
        //if(!isTTMoveNull && !isQuiescence) { maxIndexInStage--; }
        if (isQuiescence)
        {
            moveScores = moveOrder.ScoreCaptures(board, moves);
        }
        else
        {
            moveScores = moveOrder.ScoreMoves(board, moves, ttMove);
        }
    }
        
    
}