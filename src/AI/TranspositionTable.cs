using System.Transactions;

public class TranspositionTable
{
    //For when we don't have a usable transposition in the table
    public const int LookupFailed = -1;
    public const int Exact = 0;
    public const int LowerBound = 1;
    public const int UpperBound = 2;
    public ulong numStored;

    Board board;
    public Bucket[] entries;
    //How many can be stored    
    public readonly ulong count;

    public TranspositionTable(Board board, int sizeMB)
    {
        this.board = board;
        int ttEntrySizeBytes = System.Runtime.InteropServices.Marshal.SizeOf<Bucket>();
        int desiredTableSizeInBytes = sizeMB * 1024 * 1024;
        int numEntries = desiredTableSizeInBytes / ttEntrySizeBytes;
        count = (ulong)numEntries;
        entries = new Bucket[numEntries];
    }

    public int LookupEvaluation(int depth, int plyFromRoot, int alpha, int beta)
	{
        Bucket bucket = entries[Index];

        if (bucket.depthPreferred.key == board.zobristKey)
        {
            //Don't use the stored eval if it's a lower depth
            if (bucket.depthPreferred.depth >= depth)
            {
                int eval = RetrieveEval(bucket.depthPreferred.eval, plyFromRoot);
                //The exact eval
                if (bucket.depthPreferred.nodeType == Exact)
                {
                    return eval;
                }

                //We know the upper bound of the position, if it's less than our current best score it is unimportant
                if (bucket.depthPreferred.nodeType == UpperBound && eval <= alpha)
                {
                    return eval;
                }

                //Stored the lower bound, only return if it causes a beta cutoff
                if (bucket.depthPreferred.nodeType == LowerBound && eval >= beta)
                {
                    return eval;
                }
            }
        }
        else if (bucket.alwaysReplace.key == board.zobristKey)
        {
            //Don't use the stored eval if it's a lower depth
            if (bucket.alwaysReplace.depth >= depth)
            {
                int eval = RetrieveEval(bucket.alwaysReplace.eval, plyFromRoot);
                //The exact eval
                if (bucket.alwaysReplace.nodeType == Exact)
                {
                    return eval;
                }

                //We know the upper bound of the position, if it's less than our current best score it is unimportant
                if (bucket.alwaysReplace.nodeType == UpperBound && eval <= alpha)
                {
                    return eval;
                }

                //Stored the lower bound, only return if it causes a beta cutoff
                if (bucket.alwaysReplace.nodeType == LowerBound && eval >= beta)
                {
                    return eval;
                }
            }
        }
        return LookupFailed;
    }

    public Entry GetEntryForPos()
    {
        ulong index = Index;
        if (board.zobristKey == entries[index].depthPreferred.key) { return entries[index].depthPreferred; }
        else{ return entries[index].alwaysReplace; }
    }
    
    public void StoreEvaluation(int depth, int numPlySearched, int eval, int evalType, Move move)
    {
        ulong index = Index;
        if(entries[index].depthPreferred.depth <= depth)
        {
            entries[index].depthPreferred = new Entry(board.zobristKey, CorrectMateEvalForStorage(eval, numPlySearched), (byte)depth, (byte)evalType, move);
        }
        else
        {
            entries[index].alwaysReplace = new Entry(board.zobristKey, CorrectMateEvalForStorage(eval, numPlySearched), (byte)depth, (byte)evalType, move);
        }
    }

    //Returning it to its new mate value, based on how far away this mate is
    int RetrieveEval(int eval, int numPlySearched){
        if(Search.IsMateScore(eval)){
			int sign = System.Math.Sign(eval);
            return (eval * sign - numPlySearched) * sign;
        }
        //If not a mate, just return the eval
        return eval;
    }

    //Returning it to either 99999 or -99999 
    int CorrectMateEvalForStorage(int eval, int numPlySearched){
        if (Search.IsMateScore(eval)){
			int sign = System.Math.Sign(eval);
			return (eval * sign + numPlySearched) * sign;
		}
		return eval;
    }

    ulong Index{
        get{
	        return board.zobristKey % count;
        }
	}

    public Move GetStoredMove()
    {
        if (entries[Index].depthPreferred.key == board.zobristKey) { return entries[Index].depthPreferred.move; }
        else if (entries[Index].alwaysReplace.key == board.zobristKey) { return entries[Index].alwaysReplace.move; }
        else{ return Search.nullMove; }
    }

    
    public struct Entry
    {
        public readonly ulong key;
        public readonly byte depth;
        public readonly int eval;
        public readonly byte nodeType;
        public readonly Move move;

        public Entry(ulong key, int evaluation, byte depth, byte nodeType, Move move)
        {
            this.key = key;
            eval = evaluation;
            this.depth = depth;
            this.nodeType = nodeType;
            this.move = move;
        }
    }
    public struct Bucket
    {
        public Entry depthPreferred;
        public Entry alwaysReplace;
    }
    
    

    public void DeleteEntries(){
        entries = null;
    }
}



