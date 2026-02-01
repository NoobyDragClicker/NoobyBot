using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;

public class Tuner
{
    const int numParams = 6*64;
    const int K = 400;
    Random rng = new Random();
    
    Param[] weights = new Param[numParams];
    Entry[] entries;
    (string, double)[] data;

    public void Tune(string path, int numEpoches, int batchSize)
    {
        LoadData(path);
        ConvertEntries();
        float learningRate = 0.2f;
        Console.WriteLine($"Data loaded: {data.Length} positions found");

        for (int epoch = 0; epoch < numEpoches; epoch++)
        {
            Entry[] batch = GetBatch(batchSize);
            for (int entryNum = 0; entryNum < batchSize; entryNum++)
            {
                Entry entry = batch[entryNum];
                float eval = Evaluate(entry);
                float pred = Sigmoid(eval);
                float error = pred - (float)entry.result;
                float sigmoidDeriv = pred * (1-pred);

                for(int featureIndex = 0; featureIndex < entry.features.Length; featureIndex++)
                {
                    weights[entry.features[featureIndex].Item1].gradient += 2 * error * sigmoidDeriv * entry.features[featureIndex].Item2;
                }

            } 
            for (int paramIndex = 0; paramIndex < numParams; paramIndex++)
            {
                weights[paramIndex].weight -= learningRate * weights[paramIndex].gradient;
                weights[paramIndex].gradient = 0;
            }
            Console.WriteLine($"Epoch {epoch} complete");
            if((epoch % 100) == 0){learningRate *= 0.9f;}
        }
        /*Console.WriteLine($"Pawn {weights[0].weight}");
        Console.WriteLine($"Knight {weights[1].weight}");
        Console.WriteLine($"Bishop {weights[2].weight}");
        Console.WriteLine($"Rook {weights[3].weight}");
        Console.WriteLine($"Queen {weights[4].weight}");
        Console.WriteLine($"King {weights[5].weight}");*/
        PrintSinglePSQT(0);
        PrintSinglePSQT(64);
        PrintSinglePSQT(128);
        PrintSinglePSQT(192);
        PrintSinglePSQT(256);
        PrintSinglePSQT(320);
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
            eval += weights[entry.features[index].Item1].weight * entry.features[index].Item2;
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
        entries = new Entry[data.Length];

        for(int posIndex = 0; posIndex < data.Length; posIndex++)
        {
            (string, double) pos = data[posIndex];
            board.setPosition(pos.Item1, logger);
            int phase = 0;
            Dictionary<int, int> features = new Dictionary<int, int>();

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
                    int key = PSQTIndex(Piece.PieceType(piece), relativeIndex);
                    //Store the feature
                    if (features.TryGetValue(key, out int existing))
                    {
                        features[key] = existing + (Piece.Color(piece) == Piece.White ? 1 : -1);
                    }
                    else
                    {
                        features[key] = Piece.Color(piece) == Piece.White ? 1 : -1;
                    }
                }
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
            entries[posIndex] = new Entry(phase, featuresArray, pos.Item2);
        }
    }

    int PSQTIndex(int pieceType, int index)
    {
        pieceType--;
        return (pieceType * 64) + index;
    }

    void PrintSinglePSQT(int startIndex)
    {
        string msg = "{";
        for(int index = startIndex; index < startIndex + 64; index++)
        {
            msg += Math.Round(weights[index].weight);
            if(startIndex + 64 - index != 1) {msg += ", ";}
        }
        msg += "}";
        Console.WriteLine(msg);
    }
}

struct Param
{
    public float weight;
    public float gradient;
    public Param(int weight)
    {
        this.weight = weight;
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