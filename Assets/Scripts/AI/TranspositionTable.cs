using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TranspositionTable
{
    //For when we don't have a usable transposition in the table
    public const int LookupFailed = -1;
    public const int Exact = 0;
    public const int LowerBound = 1;
    public const int UpperBound = 2;
    public ulong numStored;

    Board board;
    public Entry[] entries;
    //How many can be stored    
    public readonly ulong count;

    public TranspositionTable(Board board, int sizeMB){
        this.board = board;
        int ttEntrySizeBytes = System.Runtime.InteropServices.Marshal.SizeOf<Entry>();
        int desiredTableSizeInBytes = sizeMB * 1024 * 1024;
		int numEntries = desiredTableSizeInBytes / ttEntrySizeBytes;
        count = (ulong) numEntries;
        entries = new Entry[numEntries];
    }

    public int LookupEvaluation(int depth, int plyFromRoot, int alpha, int beta)
	{
        Entry entry = entries[Index];

        if(entry.key == board.zobristKey){
            //Don't use the stored eval if it's a lower depth
            if(entry.depth >= depth){
                int eval = RetrieveEval(entry.eval, plyFromRoot);
                //The exact eval
                if(entry.nodeType == Exact){
                    return eval;
                }

                //We know the upper bound of the position, if it's less than our current best score it is unimportant
                if(entry.nodeType == UpperBound && eval <= alpha){
                    return eval;
                }

                //Stored the lower bound, only return if it causes a beta cutoff
                if(entry.nodeType == LowerBound && eval >= beta){
                    return eval;
                }
            }
        }
        return LookupFailed;
    }

    public void StoreEvaluation(int depth, int numPlySearched, int eval, int evalType, Move move){
        numStored++;
        ulong index = Index;
		Entry entry = new Entry(board.zobristKey, CorrectMateEvalForStorage(eval, numPlySearched), (byte)depth, (byte)evalType, move);
		entries[index] = entry;
    }

    //Returning it to its new mate value, based on how far away this mate is
    public int RetrieveEval(int eval, int numPlySearched){
        if(Search.IsMateScore(eval)){
			int sign = System.Math.Sign(eval);
            return (eval * sign - numPlySearched) * sign;
			
        }
        //If not a mate, just return the eval
        return eval;
    }

    //Returning it to either 99999 or -99999 
    public int CorrectMateEvalForStorage(int eval, int numPlySearched){
        if (Search.IsMateScore(eval)){
			int sign = System.Math.Sign(eval);
			return (eval * sign + numPlySearched) * sign;
		}
		return eval;
    }

    public ulong Index{
        get{
	        return board.zobristKey % count;
        }
	}

    public Move GetStoredMove()
	{
        return entries[Index].move;	
	}

    public struct Entry
    {
        public readonly ulong key;
        public readonly int depth;
        public readonly int eval;
        public readonly int nodeType;
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

    public void DeleteEntries(){
        entries = null;
    }
}



