using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
public class Tuner
{
    const int psqtOffset = 2*6*64;
    const int passedPawnOffset = 2*64;
    const int isolatedPawnOffset = 9;
    const int numParams = psqtOffset + passedPawnOffset + isolatedPawnOffset + 3;
    
    int K = 142;
    const int MAX_PHASE = 24;
    const int maxGrad = 10;
    const float lambda = 1e-3f;
    Random rng = new Random();
    
    Param[] weights = new Param[numParams];
    Entry[] entries;
    Entry[] test;
    (string, double)[] data;

    public void Tune(string path, int numEpoches, int batchSize)
    {
        LoadData(path);
        int dataSize = data.Length;
        Console.WriteLine($"Data loaded: {dataSize} positions found");
        ConvertEntries();
        InitWeights();
        Console.WriteLine("Entries loaded, starting tuning");
        float learningRate = 0.05f;
        for (int epoch = 0; epoch < numEpoches; epoch++)
        {
            //Full cycle through the data
            for(int batchNum = 0; batchNum < dataSize/batchSize; batchNum++)
            {
                Entry[] batch = GetBatch(batchSize);
                for (int entryNum = 0; entryNum < batchSize; entryNum++)
                {
                    Entry entry = batch[entryNum];
                    float eval = Evaluate(entry);
                    float pred = Sigmoid(eval);
                    float error = pred - (float)entry.result;

                    for(int featureIndex = 0; featureIndex < entry.features.Length; featureIndex++)
                    {
                        int weightIndex = entry.features[featureIndex].Item1;
                        weights[weightIndex].gradient += 2 * error * entry.features[featureIndex].Item2 
                        * (weights[weightIndex].mg ? ((float)entry.phase)/MAX_PHASE : (float)(MAX_PHASE-entry.phase)/MAX_PHASE); //Middlegame/endgame adjusted
                    }

                } 
                for (int paramIndex = 0; paramIndex < numParams; paramIndex++)
                {
                    if (Math.Abs(weights[paramIndex].gradient) > maxGrad)
                    {
                        weights[paramIndex].gradient = Math.Sign(weights[paramIndex].gradient) * maxGrad;
                    }
                    weights[paramIndex].gradient += lambda * weights[paramIndex].weight;
                    weights[paramIndex].weight -= learningRate * weights[paramIndex].gradient;
                    weights[paramIndex].gradient = 0;
                }
            }
            Console.WriteLine($"Epoch {epoch} complete: Training loss: {CalculateLoss()} Test loss: {CalculateTestLoss()}");
        }
        Console.WriteLine("mg:");
        PrintSpan(0, 64);
        PrintSpan(64, 64);
        PrintSpan(128, 64);
        PrintSpan(192, 64);
        PrintSpan(256, 64);
        PrintSpan(320, 64);
        Console.WriteLine("eg");
        PrintSpan(384, 64);
        PrintSpan(448, 64);
        PrintSpan(512, 64);
        PrintSpan(576, 64);
        PrintSpan(640, 64);
        PrintSpan(704, 64);
        Console.WriteLine("Passed pawn bonuses mg:");
        PrintSpan(psqtOffset, 64);
        Console.WriteLine("Passed pawn bonuses eg:");
        PrintSpan(psqtOffset + 64, 64);
        Console.WriteLine("Isolated pawn bonuses");
        PrintSpan(psqtOffset + passedPawnOffset, 9);
        Console.WriteLine($"Doubled pawn penalty {Math.Round(weights[psqtOffset + passedPawnOffset + isolatedPawnOffset].weight)}");
        Console.WriteLine($"Bishop pair: mg: {Math.Round(weights[psqtOffset + passedPawnOffset + isolatedPawnOffset + 1].weight)} eg: {Math.Round(weights[psqtOffset + passedPawnOffset + isolatedPawnOffset + 2].weight)}");
    }

    public void TuneKValue(string path)
    {
        LoadData(path);
        int dataSize = data.Length;
        Console.WriteLine($"Data loaded: {dataSize} positions found");
        ConvertEntries();
        LoadWeights();
        Console.WriteLine("Entries loaded, starting tuning");
        int delta = 1;

        double lastLoss = CalculateLoss();
        K += delta;
        double currentLoss = CalculateLoss();
        if(currentLoss > lastLoss){ 
            delta *= -1; 
            K += delta * 2;
        }
        currentLoss = CalculateLoss();
        while(lastLoss > currentLoss)
        {
            K += delta;
            lastLoss = currentLoss;
            currentLoss = CalculateLoss();
            Console.WriteLine(K);
        }
        Console.WriteLine("Final K:" + (K - delta));
    }
    Entry[] GetBatch(int batchSize)
    {
        Entry[] batch = new Entry[batchSize];
        for(int index = 0; index < batchSize; index++)
        {
            batch[index] = entries[rng.Next(0, entries.Length - 1)];
        }
        return batch;   
    }

    //Gets the current evaluation of a position
    float Evaluate(Entry entry)
    {
        float eval = 0;
        for(int index = 0; index < entry.features.Length; index++)
        {
            int weightIndex = entry.features[index].Item1;
            eval += weights[weightIndex].weight * entry.features[index].Item2 * (weights[weightIndex].mg ? (float)entry.phase/MAX_PHASE : ((float)MAX_PHASE-entry.phase)/MAX_PHASE);
        }
        return eval;
    }


    float Sigmoid(float x) {
        return 1.0f / (1.0f + (float)Math.Exp(-x / K));
    }

    void LoadData(string path)
    {
        string[] rawData = File.ReadAllLines(path);
        data = new (string, double)[rawData.Length];
        for (int index = 0; index < rawData.Length; index++)
        {
            string[] split = rawData[index].Split("[");
            data[index].Item1 = split[0];
            data[index].Item2 = split[1].Contains("1.0") ? 1.0 : split[1].Contains("0.5") ? 0.5 : 0.0;
        }
    }

    void ConvertEntries()
    {
        Board board = new Board();
        SearchLogger logger = new SearchLogger("tune", SearchLogger.LoggingLevel.Deadly);
        int numTrainingData = (int) (data.Length * 0.9);
        entries = new Entry[numTrainingData];
        test = new Entry[data.Length - numTrainingData];


        for(int posIndex = 0; posIndex < data.Length; posIndex++)
        {
            (string, double) pos = data[posIndex];
            board.setPosition(pos.Item1, logger);
            int phase = 0;
            Dictionary<int, int> features = new Dictionary<int, int>();
            int numBlackIsolatedPawns = 0;
            int numWhiteIsolatedPawns = 0;

            for(int index = 0; index < 64; index++)
            {
                int piece = board.board[index];
                
                if(piece != 0)
                {
                    int pieceType = Piece.PieceType(piece);
                    switch (pieceType)
                    {
                        case Piece.Knight:
                            phase += 1;
                            break;
                        case Piece.Bishop:
                            phase += 1;
                            break;
                        case Piece.Rook:
                            phase += 2;
                            break;
                        case Piece.Queen:
                            phase += 4;
                            break;
                        default: 
                            break;
                    }

                    int relativeIndex = Piece.Color(piece) == Piece.White ? index : index ^ 56;
                    //Add middlegame and endgame psqt weight
                    AddFeature(PSQTIndex(Piece.PieceType(piece), relativeIndex, true), Piece.Color(piece), features);
                    AddFeature(PSQTIndex(Piece.PieceType(piece), relativeIndex, false), Piece.Color(piece), features);

                    if(pieceType == Piece.Pawn)
                    {
                        if (Piece.Color(piece) == Piece.White)
                        {
                            //Passed pawn
                            if ((board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Pawn)] & BitboardHelper.wPawnPassedMask[index]) == 0) { 
                                AddFeature(psqtOffset + index, Piece.White, features); 
                                AddFeature(psqtOffset + 64 + index, Piece.White, features); //EG
                            }
                            //Doubled pawn penalty
                            if (board.PieceAt(index - 8) == Piece.Pawn && board.ColorAt(index - 8) == Piece.White) { AddFeature(psqtOffset + isolatedPawnOffset + passedPawnOffset, Piece.White, features); }
                            if ((BitboardHelper.isolatedPawnMask[index] & board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Pawn)]) == 0) { numWhiteIsolatedPawns++; }
                        }
                        else
                        {
                            //Passed pawn
                            if ((board.pieceBitboards[Board.PieceBitboardIndex(Board.WhiteIndex, Piece.Pawn)] & BitboardHelper.bPawnPassedMask[index]) == 0) { 
                                AddFeature(psqtOffset + (index ^ 56), Piece.Black, features); 
                                AddFeature(psqtOffset + 64 + (index ^ 56), Piece.Black, features); //EG
                            }
                            //Doubled pawn penalty
                            if (board.PieceAt(index + 8) == Piece.Pawn && board.ColorAt(index + 8) == Piece.Black) { AddFeature(psqtOffset + isolatedPawnOffset + passedPawnOffset, Piece.Black, features); }
                            if((BitboardHelper.isolatedPawnMask[index] & board.pieceBitboards[Board.PieceBitboardIndex(Board.BlackIndex, Piece.Pawn)]) == 0){ numBlackIsolatedPawns++; }
                        }
                    }
                }
            }

            //Isolated pawns
            AddFeature(psqtOffset + passedPawnOffset + numBlackIsolatedPawns, Piece.Black, features);
            AddFeature(psqtOffset + passedPawnOffset + numWhiteIsolatedPawns, Piece.White, features);

            //Bishop Pair
            if(board.pieceCounts[Board.WhiteIndex, Piece.Bishop] >= 2)
            {
                AddFeature(psqtOffset + passedPawnOffset + isolatedPawnOffset + 1, Piece.White, features);
                AddFeature(psqtOffset + passedPawnOffset + isolatedPawnOffset + 2, Piece.White, features);
            }
            //Bishop Pair
            if(board.pieceCounts[Board.BlackIndex, Piece.Bishop] >= 2)
            {
                AddFeature(psqtOffset + passedPawnOffset + isolatedPawnOffset + 1, Piece.Black, features);
                AddFeature(psqtOffset + passedPawnOffset + isolatedPawnOffset + 2, Piece.Black, features);
            }

            //Remove zeroed features
            foreach (KeyValuePair<int, int> pair in features)
            {
                if(pair.Value == 0){features.Remove(pair.Key);}
            }

            //Finally convert to the array
            (int, int)[] featuresArray = new (int, int)[features.Count];
            int i = 0;
            foreach (var kvp in features)
            {
                featuresArray[i++] = (kvp.Key, kvp.Value);
            }

            phase = Math.Min(phase, 24);
            if(posIndex < numTrainingData)
            {
                entries[posIndex] = new Entry(phase, featuresArray, pos.Item2);
            }
            else
            {
                test[posIndex - numTrainingData] = new Entry(phase, featuresArray, pos.Item2);
            }
            
        }
    }

    int PSQTIndex(int pieceType, int index, bool mg)
    {
        pieceType--;
        return (pieceType * 64) + index + (mg ? 0 : 6*64);
    }


    void PrintSpan(int startIndex, int numItems)
    {
        string msg = "{";
        for(int index = startIndex; index < startIndex + numItems; index++)
        {
            msg += Math.Round(weights[index].weight);
            if(startIndex + numItems - index != 1) {msg += ", ";}
        }
        msg += "},";
        Console.WriteLine(msg);
    }

    void AddFeature(int key, int color, Dictionary<int, int> features)
    {
        //Store the feature
        if (features.TryGetValue(key, out int existing))
        {
            features[key] = existing + (color == Piece.White ? 1 : -1);
        }
        else
        {
            features[key] = color == Piece.White ? 1 : -1;
        }
    }

    void InitWeights()
    {
        //PSQT
        for(int index = 0; index < 6*64; index++)
        {
            weights[index] = new Param(0, true);
        }
        for(int index = 6*64; index < psqtOffset; index++)
        {
            weights[index] = new Param(0, false);
        }
        int currentOffset = psqtOffset;
        
        //passed pawn
        for(int index = psqtOffset; index < psqtOffset + 64; index++)
        {
            weights[index] = new Param(0, true);
        }
        for(int index = psqtOffset + 64; index < psqtOffset + passedPawnOffset; index++)
        {
            weights[index] = new Param(0, false);
        }
        currentOffset += passedPawnOffset;

        //Isolated pawn
        for(int index = psqtOffset + passedPawnOffset; index < currentOffset + isolatedPawnOffset ; index++)
        {
            weights[index] = new Param(0, false);
        }
        currentOffset += isolatedPawnOffset;

        //Doubled pawn penalty
        weights[currentOffset++] = new Param(0, false);
        //Bishop pair
        weights[currentOffset++] = new Param(0, true);
        weights[currentOffset++] = new Param(0, false);
    }

    void LoadWeights()
    {
        try{
        for(int pieceIndex = 1; pieceIndex < 7; pieceIndex++)
        {
            for(int index = 0; index < 64; index++)
            {
                weights[PSQTIndex(pieceIndex, index, true)] = new Param(Evaluation.mg_PSQT[pieceIndex, index], true);
                weights[PSQTIndex(pieceIndex, index, false)] = new Param(Evaluation.eg_PSQT[pieceIndex, index], false);
            }
        }
        //passed pawn
        for(int index = 0; index < 64; index++)
        {
            weights[psqtOffset + index] = new Param(Evaluation.passedPawnBonuses[0, index], true);
        }
        for(int index = 0; index < 64; index++)
        {
            weights[psqtOffset + 64 + index] = new Param(Evaluation.passedPawnBonuses[1, index], false);
        }

        for(int index = 0; index < isolatedPawnOffset; index++)
        {
            weights[psqtOffset + passedPawnOffset + index] = new Param(Evaluation.isolatedPawnPenalty[index], false);
        }
        weights[psqtOffset + passedPawnOffset + isolatedPawnOffset] = new Param(Evaluation.doubledPawnPenalty, false);
        weights[psqtOffset + passedPawnOffset + isolatedPawnOffset + 1] = new Param(Evaluation.bishopPairBonusMG, true);
        weights[psqtOffset + passedPawnOffset + isolatedPawnOffset + 2] = new Param(Evaluation.bishopPairBonusEG, false);

        }
        catch(Exception e)
        {
            Console.WriteLine(e);
        }
    }

    double CalculateLoss()
    {
        double totalLoss = 0;
        int count = 0;

        foreach (Entry entry in entries)
        {
            float eval = Evaluate(entry);
            float p = Sigmoid(eval);

            float y = (float)entry.result;

            p = Math.Clamp(p, 1e-7f, 1 - 1e-7f);

            double loss =
                -(y * Math.Log(p) + (1 - y) * Math.Log(1 - p));

            totalLoss += loss;
            count++;
        }
        return totalLoss / count;
    }
    double CalculateTestLoss()
    {
        double totalLoss = 0;
        int count = 0;

        foreach (Entry entry in test)
        {
            float eval = Evaluate(entry);
            float p = Sigmoid(eval);

            float y = (float)entry.result;

            p = Math.Clamp(p, 1e-7f, 1 - 1e-7f);

            double loss =
                -(y * Math.Log(p) + (1 - y) * Math.Log(1 - p));

            totalLoss += loss;
            count++;
        }
        return totalLoss / count;
    }
}

struct Param
{
    public float weight;
    public float gradient;
    public bool mg;
    public Param(int weight, bool mg)
    {
        this.weight = weight;
        this.mg = mg;
    }
}

struct Entry
{
    public int phase;
    public (int, int)[] features;
    public double result;
    public Entry(int phase, (int, int)[] features, double result)
    {
        this.phase = phase;
        this.features = features;
        this.result = result;
    }
}