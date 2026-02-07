using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.VisualBasic;
public class Tuner
{
    int K = 142;
    const int MAX_PHASE = 24;
    Random rng = new Random(123123123);
    

    enum Tunables {PSQT, PASSER, ISOLATED, DOUBLED, BISHOP};

    TuningInfo[] infos =
    {
        new TuningInfo(64*6, true, true),
        new TuningInfo(64, true, true),
        new TuningInfo(9, false, true),
        new TuningInfo(1, false, true),
        new TuningInfo(1, true, true)
    };
    
    TPair[] weights;
    Entry[] entries;
    Entry[] test;
    (string, double)[] data;

    public Tuner()
    {
        int currentStartIndex = 0;
        for (int infoIndex = 0; infoIndex < infos.Length; infoIndex++)
        {
            infos[infoIndex].startIndex = currentStartIndex;
            currentStartIndex += infos[infoIndex].length;
        }
        weights = new TPair[currentStartIndex];

        //Initialise with a weight of 0
        for (int infoIndex = 0; infoIndex < infos.Length; infoIndex++)
        {
            for(int index = infos[infoIndex].startIndex; index < infos[infoIndex].startIndex + infos[infoIndex].length; index++)
            {
                weights[index].mg = new Param(0f, infos[infoIndex].tuneMG);
                weights[index].eg = new Param(0f, infos[infoIndex].tuneEG);
            }
        }
    }

    public void Tune(string path, int numEpoches, int batchSize)
    {
        LoadData(path);
        int dataSize = data.Length;
        Console.WriteLine($"Data loaded: {dataSize} positions found");
        ConvertEntries();
        Console.WriteLine("Entries loaded, starting tuning");
        float learningRate = 0.01f;
        float beta1 = 0.9f;
        float beta2 = 0.99f;
        float weightDecay = 3e-4f;
        int maxGrad = 50;
        float eps = 1e-8f;
        int t = 0;
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
                        if (weights[weightIndex].mg.tune)
                        {
                            weights[weightIndex].mg.gradient += 2 * error * entry.features[featureIndex].Item2 * ((float)entry.phase)/MAX_PHASE;
                        }
                        if (weights[weightIndex].eg.tune)
                        {
                            weights[weightIndex].eg.gradient += 2 * error * entry.features[featureIndex].Item2 * ((float)MAX_PHASE-entry.phase)/MAX_PHASE;
                        }
                    }

                } 
                
                for (int paramIndex = 0; paramIndex < weights.Length; paramIndex++)
                {
                    t++;
                    //MG
                    if (weights[paramIndex].mg.tune)
                    {
                        float g = weights[paramIndex].mg.gradient;
                        if (Math.Abs(g) > maxGrad) { g = Math.Sign(g) * maxGrad; }

                        weights[paramIndex].mg.m = beta1 * weights[paramIndex].mg.m + (1 - beta1) * g;

                        weights[paramIndex].mg.v = beta2 * weights[paramIndex].mg.v + (1 - beta2) * g * g;

                        float mHat = weights[paramIndex].mg.m / (1 - MathF.Pow(beta1, t));
                        float vHat = weights[paramIndex].mg.v / (1 - MathF.Pow(beta2, t));

                        // Adam update
                        weights[paramIndex].mg.weight -= learningRate * mHat / (MathF.Sqrt(vHat) + eps);
                        //weight decay
                        weights[paramIndex].mg.weight -= learningRate * weightDecay * weights[paramIndex].mg.weight;
                    }
                    weights[paramIndex].mg.gradient = 0;

                    //EG
                    if (weights[paramIndex].eg.tune)
                    {
                        float g = weights[paramIndex].eg.gradient;

                        if (Math.Abs(g) > maxGrad) { g = Math.Sign(g) * maxGrad; };

                        weights[paramIndex].eg.m = beta1 * weights[paramIndex].eg.m + (1 - beta1) * g;
                        weights[paramIndex].eg.v = beta2 * weights[paramIndex].eg.v + (1 - beta2) * g * g;

                        float mHat = weights[paramIndex].eg.m / (1 - MathF.Pow(beta1, t));
                        float vHat = weights[paramIndex].eg.v / (1 - MathF.Pow(beta2, t));

                        weights[paramIndex].eg.weight -= learningRate * mHat / (MathF.Sqrt(vHat) + eps);
                        weights[paramIndex].eg.weight -= learningRate * weightDecay * weights[paramIndex].eg.weight;
                    }

                    weights[paramIndex].eg.gradient = 0;
                }
            }
            Console.WriteLine($"Epoch {epoch} complete: Training loss: {CalculateLoss()} Test loss: {CalculateTestLoss()}");
        }
        PrintPSQT();
        Console.WriteLine("Passed pawn");
        PrintSpan(infos[(int)Tunables.PASSER]);
        Console.WriteLine("Isolated pawn");
        PrintSpan(infos[(int)Tunables.ISOLATED]);
        Console.WriteLine("Doubled pawn");
        PrintSpan(infos[(int)Tunables.DOUBLED]);
        Console.WriteLine("Bishop pair");
        PrintSpan(infos[(int)Tunables.BISHOP]);
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
            eval += weights[weightIndex].mg.weight * entry.features[index].Item2 * ((float)entry.phase)/MAX_PHASE ;
            eval += weights[weightIndex].eg.weight * entry.features[index].Item2 * ((float)MAX_PHASE-entry.phase)/MAX_PHASE;
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
            int[] isolatedPawnCount = new int[2];

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
                    AddFeature(PSQTIndex(Piece.PieceType(piece), relativeIndex), Piece.Color(piece), features);

                    if(pieceType == Piece.Pawn)
                    {
                        int currentColor = Piece.Color(piece);
                        int currentColorIndex = currentColor == Piece.White ? Board.WhiteIndex : Board.BlackIndex;
                        int oppositeColorIndex = 1 - currentColorIndex;
                        int pushSquare =  currentColorIndex == Board.WhiteIndex ? index - 8 :  index + 8;

                        //Passed pawn
                        if ((board.pieceBitboards[Board.PieceBitboardIndex(oppositeColorIndex, Piece.Pawn)] & BitboardHelper.pawnPassedMask[currentColorIndex, index]) == 0) { 
                            AddFeature(infos[(int)Tunables.PASSER].startIndex + relativeIndex, currentColor, features); 
                        }
                        //Doubled pawn penalty
                        if (board.PieceAt(pushSquare) == Piece.Pawn && board.ColorAt(pushSquare) == currentColor) { AddFeature(infos[(int)Tunables.DOUBLED].startIndex, currentColor, features); }
                        if ((BitboardHelper.isolatedPawnMask[index] & board.pieceBitboards[Board.PieceBitboardIndex(currentColorIndex, Piece.Pawn)]) == 0) { isolatedPawnCount[currentColorIndex]++; }
                    }
                }
            }

            //Isolated pawns
            AddFeature(infos[(int)Tunables.ISOLATED].startIndex + isolatedPawnCount[Board.BlackIndex], Piece.Black, features);
            AddFeature(infos[(int)Tunables.ISOLATED].startIndex + isolatedPawnCount[Board.WhiteIndex], Piece.White, features);

            //Bishop Pair
            if(board.pieceCounts[Board.WhiteIndex, Piece.Bishop] >= 2)
            {
                AddFeature(infos[(int)Tunables.BISHOP].startIndex, Piece.White, features);
            }
            //Bishop Pair
            if(board.pieceCounts[Board.BlackIndex, Piece.Bishop] >= 2)
            {
                AddFeature(infos[(int)Tunables.BISHOP].startIndex, Piece.Black, features);
            }

            //Remove zeroed features
            foreach (var key in features.Keys.ToArray())
            {
                if (features[key] == 0)
                {
                    features.Remove(key);
                }
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

    int PSQTIndex(int pieceType, int index)
    {
        pieceType--;
        return (pieceType * 64) + index;
    }


    void PrintPSQT()
    {
        string mg = "mg: {";
        string eg = "eg: {";
        int index = 0;
        for(int pieceNum = 0; pieceNum < 6; pieceNum++)
        {
            for(int i = 0; i < 64; i++)
            {
                mg += Math.Round(weights[index].mg.weight);
                eg += Math.Round(weights[index].eg.weight);
                index++;
                if(i != 63) {mg += ", "; eg += ", ";}
            }
            mg += "}";
            eg += "}";
            if(pieceNum != 5){ mg += ", \n{"; eg += ", \n{"; }
        }
        Console.WriteLine(mg);
        Console.WriteLine(eg);
    }

    void PrintSpan(TuningInfo info)
    {
        string mg = "{";
        string eg = "{";
        for(int index = info.startIndex; index < info.startIndex + info.length; index++)
        {
            mg += Math.Round(weights[index].mg.weight);
            eg += Math.Round(weights[index].eg.weight);
            if(index != info.startIndex + info.length - 1){ mg += ", "; eg += ", "; }
        }
        mg += "}, ";
        eg += "}";
        Console.WriteLine(mg);
        Console.WriteLine(eg);
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

struct TPair
{
    public Param mg;
    public Param eg;
}

struct Param
{
    public float weight;
    public float gradient;
    public float m;
    public float v;
    public bool tune;
    public Param(float weight, bool tune)
    {
        this.weight = weight;
        this.tune = tune;
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

struct TuningInfo
{
    public int startIndex;
    public int length;
    public bool tuneMG;
    public bool tuneEG;
    public TuningInfo(int length, bool tuneMG, bool tuneEG)
    {
        this.length = length;
        this.tuneMG = tuneMG;
        this.tuneEG = tuneEG;
    }
}