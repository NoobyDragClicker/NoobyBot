using Microsoft.VisualBasic;

public class Tuner
{
    const int numParams = 6*64;
    const int K = 300;
    Random rng = new Random();
    
    Param[] weights = new Param[numParams];
    int[] features = new int[numParams];
    (string, double)[] data;

    public void Tune(string path, int numEpoches, int batchSize)
    {
        LoadData(path);
        float learningRate = 0.01f;
        Console.WriteLine($"Data loaded: {data.Length} positions found");
        SearchLogger logger = new SearchLogger("tune", SearchLogger.LoggingLevel.Deadly);

        for (int epoch = 0; epoch < numEpoches; epoch++)
        {
            (string, double)[] batch = GetBatch(batchSize);
            Board board = new Board();
            foreach ((string, double) fenAndResult in batch)
            {
                board.setPosition(fenAndResult.Item1, logger);
                features = new int[numParams];
                ExtractFeatures(board);
                float eval = Evaluate();
                float pred = Sigmoid(eval);
                float error = pred - (float)fenAndResult.Item2;
                float sigmoidDeriv = pred * (1-pred);

                for(int paramIndex = 0; paramIndex < numParams; paramIndex++)
                {
                    weights[paramIndex].gradient += 2 * error * sigmoidDeriv * features[paramIndex];
                }

            } 
            for (int paramIndex = 0; paramIndex < numParams; paramIndex++)
            {
                weights[paramIndex].weight -= learningRate * weights[paramIndex].gradient;
                weights[paramIndex].gradient = 0;
            }
            Console.WriteLine($"Epoch {epoch} complete");
        }
        PrintSinglePSQT(0);
        PrintSinglePSQT(64);
        PrintSinglePSQT(128);
        PrintSinglePSQT(192);
        PrintSinglePSQT(256);
        PrintSinglePSQT(320);
    }

    (string,double)[] GetBatch(int batchSize)
    {
        (string,double)[] batch = new (string, double)[batchSize];
        for(int index = 0; index < batchSize; index++)
        {
            batch[index] = data[rng.Next(0, data.Length - 1)];
        }
        return batch;   
    }
    void ExtractFeatures(Board board)
    {
        for(int index = 0; index < 64; index++)
        {
            int piece = board.board[index];
            if(piece != 0)
            {
                int relativeIndex = Piece.Color(piece) == Piece.White ? index : index ^ 56;
                features[PSQTIndex(Piece.PieceType(piece), relativeIndex)] += Piece.Color(piece) == Piece.White ? 1 : -1;
            }
        } 
    }

    float Evaluate()
    {
        float eval = 0;
        for(int index = 0; index < numParams; index++)
        {
            eval += weights[index].weight * features[index];
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