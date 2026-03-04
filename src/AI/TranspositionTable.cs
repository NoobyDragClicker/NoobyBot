using System.Runtime.Intrinsics.X86;
using System.Runtime.InteropServices;


public unsafe class TranspositionTable
{
    public const int Exact = 0;
    public const int LowerBound = 1;
    public const int UpperBound = 2;
    public ulong numStored;

    Board board;
    public unsafe Bucket* entries;
    //How many can be stored    
    public readonly ulong count;

    public TranspositionTable(Board board, int sizeMB)
    {
        this.board = board;

        ulong desiredTableSizeInBytes = (ulong)sizeMB * 1024ul * 1024ul;
        ulong numEntries = desiredTableSizeInBytes / (ulong)sizeof(Bucket);

        count = numEntries;

        entries = (Bucket*)NativeMemory.Alloc((nuint)numEntries, (nuint)sizeof(Bucket));
        NativeMemory.Clear(entries, (nuint)numEntries * (nuint)sizeof(Bucket));
    }

    public unsafe void PrefetchBucket()
    {
        if (Sse.IsSupported)
        {
            Bucket* ptr = &entries[Index];
            Sse.Prefetch0(ptr);
        }
    }

    public bool ProbeTT(out Entry ttentry)
	{
        Bucket* bucket = &entries[Index];
        Entry* e = (Entry*)bucket;

        if (e[0].key == board.zobristKey)
        {
            ttentry = e[0];
            return true;
        }
        else if (e[1].key == board.zobristKey)
        {
            ttentry = e[1];
            return true;
        }
        ttentry = new Entry();
        return false; 
    }

    public unsafe Entry GetEntryForPos()
    {
        ulong index = Index;

        Bucket* bucket = &entries[index];
        Entry* e = (Entry*)bucket;

        if (e[0].key == board.zobristKey)
        {
            return e[0];
        }
        else
        {
            return e[1];
        }
    }
    
    public unsafe void StoreEvaluation(int depth, int numPlySearched, int eval, int evalType, Move move)
    {
        ulong index = Index;
        Bucket* bucket = &entries[index];
        Entry* e = (Entry*)bucket;

        int replaceIndex = -1;
        int currQuality = int.MaxValue;
        //Taken from Sirius :)
        for (int i = 0; i < 2; i++)
        {
            if (e[i].key == board.zobristKey)
            {
                replaceIndex = i;
                break;
            }

            if(e[i].depth < currQuality)
            {
                replaceIndex = i;
                currQuality = e[i].depth;
            }
        }

        Entry* replace = &e[replaceIndex];
        replace->key = board.zobristKey;
        replace->depth = (byte)depth;
        replace->eval = (short)CorrectMateEvalForStorage(eval, numPlySearched);
        replace->move = (ushort)move;
        replace->nodeType = (byte)evalType;
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


    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct Entry
    {
        [FieldOffset(0)]  public ulong key;      // 8
        [FieldOffset(8)]  public ushort move;    // 2
        [FieldOffset(10)] public short eval;     // 2
        [FieldOffset(12)] public byte depth;     // 1
        [FieldOffset(13)] public byte nodeType;  // 1
        [FieldOffset(14)] private ushort _pad;   // 2 (pad to 16)

        public Entry(ulong key, int evaluation, byte depth, byte nodeType, Move move)
        {
            this.key = key;
            eval = (short)evaluation;
            this.depth = depth;
            this.nodeType = nodeType;
            this.move = move;
        }
    }
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public unsafe struct Bucket
    {
        [FieldOffset(0)]  private Entry _elem0;
        [FieldOffset(16)] private Entry _elem1;
    }

    public void DeleteEntries()
    {
        if (entries != null)
        {
            NativeMemory.Free(entries);
            entries = null;
        }
    }   
}



