using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;


public class BoardManager : MonoBehaviour
{
    //UI:
    public GameObject startScreen;
    public TMP_Dropdown whitePlayerType;
    public TMP_Dropdown blackPlayerType;
    public TMP_InputField clockStartTime;
    public TMP_InputField customPosition;


    //Main board for the game
    public Board board;
    public Board searchBoard;

    //Prefabs
    public Tile tilePrefab;
    public MoveSelected moveSelectedPrefab;
    public GameObject debugPrefab;
    public DisplayPiece displayPiecePrefab;

    //Display for who won
    public SpriteRenderer blackIndicator;
    public SpriteRenderer whiteIndicator;

    public Color green;
    public Color red;
    public Color yellow;


    public Clock whiteClock;
    public Clock blackClock;
    public bool useClock = true;
    public bool useCustomPos = false;


    //Lists of prefabs and moves for promotion, selecting a move and debugging
    List<Move> promotionMoves = new List<Move>();
    List<MoveSelected> possiblePlayerMoves = new List<MoveSelected>();
    List<GameObject> debugPrefabs = new List<GameObject>();
    public List<DisplayPiece> displayPieces = new List<DisplayPiece>();

    public bool isDebugMode = false;
    //Index of the current selected piece by the player
    int selectedPiece;

    public Player whitePlayer;
    public Player blackPlayer;
    public Player playerToMove;

    public bool hasGameStarted = false;
    public bool hasGameEnded = false;

    public const string startingFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public string inputtedCustomStr;

/*****************************************************************************
    Will now handle:
        - spawning squares (complete)
        - displaying pieces (complete)
        - displaying captured pieces (sorted and customizable)
        - displaying move number, chess notation and bot eval (when available)
        - interfacing with move generator to allow a player to move pieces (complete)
        - add UI to take in FEN strings
        - allow arrows to be drawn
        - as much front end stuff as I can fit
    
    dev thoughts: use callback to listen for a move command from the bot and player to update where the pieces are

*****************************************************************************/
    void Start(){
        useClock = true;
    }
    
    public void StartGame(){
        //Reading start time
        int startTime = Convert.ToInt32(clockStartTime.text);
        inputtedCustomStr = customPosition.text;
        hasGameStarted = true;
        
        if(useClock){
            whiteClock.gameObject.SetActive(true);
            blackClock.gameObject.SetActive(true);
            whiteClock.startSeconds = startTime;
            blackClock.startSeconds = startTime;
            whiteClock.StartGame();
            blackClock.StartGame();
            whiteClock.isTurnToMove = false;
            blackClock.isTurnToMove = false;
        }

        if(!useCustomPos){
            //Creates new board
            board = new Board(startingFEN, new MoveGenerator());
            searchBoard = new Board(startingFEN, new MoveGenerator());
        } else{
            board = new Board(inputtedCustomStr, new MoveGenerator());
            searchBoard = new Board(inputtedCustomStr, new MoveGenerator());
        }
        UpdateBoard();
        //Generates actual tiles
        for(int x = 0; x< 8; x++){
            for(int y = 0; y < 8; y++){
                var spawnedTile = Instantiate(tilePrefab, new Vector3(x, y, 0.01f), Quaternion.identity);
                spawnedTile.name = $"tile {x} {y}";

                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.Init(isOffset);
            }
        }
        //Turn off starting UI
        startScreen.SetActive(false);


        bool whiteUsingTest = (whitePlayerType.value == 2)? true : false;
        bool blackUsingTest = (blackPlayerType.value == 2)? true : false;
        //Syncing OnMoveChosen
        whitePlayer = (whitePlayerType.value == 0) ? new HumanPlayer() : new AIPlayer(searchBoard, whiteUsingTest, whiteUsingTest? 6:4);
        whitePlayer.onMoveChosen += OnMoveChosen;
        blackPlayer = (blackPlayerType.value == 0) ? new HumanPlayer() : new AIPlayer(searchBoard, blackUsingTest, blackUsingTest? 6:4);
        blackPlayer.onMoveChosen += OnMoveChosen;
        
        if(board.colorTurn == Piece.Black){playerToMove = blackPlayer;}
        if(board.colorTurn == Piece.White){playerToMove = whitePlayer;}

        playerToMove.NotifyToMove();
    }

    void OnMoveChosen(Move move){
        board.Move(move, false);
        searchBoard.Move(move, true);
        if(board.colorTurn == Piece.Black){playerToMove = blackPlayer;}
        if(board.colorTurn == Piece.White){playerToMove = whitePlayer;}
        //Updates the main board display
        UpdateBoard();

        //Checking if there is a draw or mate
        if(board.IsCheckmate(board.colorTurn)){
            if(board.colorTurn == Piece.White){EndGame(Piece.Black, false);}
            if(board.colorTurn == Piece.Black){EndGame(Piece.White, false);}
        }
        if(board.IsDraw()){EndGame(0, true);}


        if(!hasGameEnded){
            playerToMove.NotifyToMove();
        }
    }

    void Update(){
        if(whiteClock.hasLost == true && !hasGameEnded){EndGame(Piece.Black, false);}
        else if(blackClock.hasLost == true && !hasGameEnded){EndGame(Piece.White, false);}
        if(useClock && hasGameStarted && !hasGameEnded){
            whiteClock.isTurnToMove = (board.colorTurn == Piece.White)? true:false;
            blackClock.isTurnToMove = (board.colorTurn == Piece.Black)? true:false;
        }
        if(hasGameStarted && !hasGameEnded){playerToMove.Update();}
        
    }

    public void UpdateBoard(){ 
           
        //Removing all displayed pieces
        for(int y = 0; y < displayPieces.Count; y++){
            Destroy(displayPieces[y].gameObject);
        }
        displayPieces.Clear();

        //Destroy current moveselectors
        for(int z = 0; z< possiblePlayerMoves.Count; z++){
            Destroy(possiblePlayerMoves[z].gameObject);
        }
        possiblePlayerMoves.Clear();

        //Destroy current debug prefabs
        for(int z = 0; z< debugPrefabs.Count; z++){
            Destroy(debugPrefabs[z]);
        }
        debugPrefabs.Clear();
        
        
        //Spawning them in based on board
        for(int x = 0; x<64; x++){
            if(board.board[x] != 0){
                int pieceType = Piece.PieceType(board.board[x]);
                int rank = board.IndexToRank(x);
                int file = board.IndexToFile(x);
                DisplayPiece displayPiece = Instantiate(displayPiecePrefab, new Vector3(file-1, rank-1), Quaternion.identity);
                displayPiece.Init(pieceType, Piece.Color(board.board[x]));
                displayPieces.Add(displayPiece);
            }
        }
        ShowDebugSquares(board.pinnedPieceIndexes);
        if(isDebugMode){ShowAttackedSquares();}
    }

    public void SelectPiece(int x, int y){

        //Can't select a piece for an AI
        if(playerToMove is AIPlayer){ return;}

        //Destroy current moveselectors
        for(int z = 0; z< possiblePlayerMoves.Count; z++){
            Destroy(possiblePlayerMoves[z].gameObject);
        }
        possiblePlayerMoves.Clear();
        promotionMoves.Clear();

        //checking if it is already selected, and if yes, deselect
        if (selectedPiece == board.RankFileToIndex(x, y)){
            selectedPiece = -1;
            return;
        }

        //Getting possible moves and displaying
        selectedPiece = board.RankFileToIndex(x, y);
        List<Move> possibleMoves = new List<Move>();

        //If there is a piece on that square
        if(board.board[selectedPiece] != 0){
            possibleMoves = board.moveGenerator.GeneratePieceMove(board.board[selectedPiece], board.RankFileToIndex(x, y), board);
        }

        for(int z = 0; z< possibleMoves.Count; z++){
            var movePrefab = Instantiate(moveSelectedPrefab, new Vector3(board.IndexToFile(possibleMoves[z].newIndex) -1, board.IndexToRank(possibleMoves[z].newIndex) -1, -0.02f), Quaternion.identity);
            movePrefab.move = possibleMoves[z];
            //If its a promotion piece
            if(movePrefab.move.isPromotion() && movePrefab.move.flag == 1){
                movePrefab.name = "promotion piece";
                movePrefab.isPromoManager = true;
                possiblePlayerMoves.Add(movePrefab);
                promotionMoves.Add(possibleMoves[z]);
            } else if(movePrefab.move.isPromotion() && movePrefab.move.flag != 1){
                promotionMoves.Add(possibleMoves[z]);
                Destroy(movePrefab.gameObject);
            //Else
            } else{
                possiblePlayerMoves.Add(movePrefab);
            }
        }
    }

    public void EndGame(int winningColor, bool isDraw){
        hasGameEnded = true;
        whiteIndicator.gameObject.SetActive(true);
        blackIndicator.gameObject.SetActive(true);

        if(winningColor == Piece.White){ Debug.Log("White won"); whiteIndicator.color = green; blackIndicator.color = red;}
        else if(winningColor == Piece.Black){ Debug.Log("Black won"); whiteIndicator.color = red; blackIndicator.color = green;}
        else if(isDraw){ Debug.Log("Draw"); whiteIndicator.color = yellow; blackIndicator.color = yellow;}
        Debug.Log("Black: ");
        blackPlayer.NotifyGameOver();
        Debug.Log("White: ");
        whitePlayer.NotifyGameOver();
    }

    //Slightly bad system, but is only for front end, no effect on bot performance
    public void SpawnPromotionPieces(Move move){

        //Destroy current moveselectors
        for(int z = 0; z< possiblePlayerMoves.Count; z++){
            Destroy(possiblePlayerMoves[z].gameObject);
        }
        possiblePlayerMoves.Clear();

        for(int x = 0; x< promotionMoves.Count; x++){
            //For cases where there are multiple ways to promote, only spawn them if they are the selected move
            if(promotionMoves[x].newIndex == move.newIndex){
                var movePrefab = Instantiate(moveSelectedPrefab, new Vector3(board.IndexToFile(promotionMoves[x].newIndex) -1, board.IndexToRank(promotionMoves[x].newIndex) -1, -0.02f), Quaternion.identity);
                movePrefab.move = promotionMoves[x];
                movePrefab.SetPromotionRender();
                possiblePlayerMoves.Add(movePrefab);
            }

        }
    }

    //Debugging purposes
    void ShowAttackedSquares(){
        
        //Destroy current debug things
        for(int z = 0; z< debugPrefabs.Count; z++){
            Destroy(debugPrefabs[z]);
        }
        debugPrefabs.Clear();

        int[] attackedSquares = board.moveGenerator.GenerateAttackedSquares(board.colorTurn, board);
        for(int x = 0; x<64; x++){
            int rank = board.IndexToRank(x);
            int file = board.IndexToFile(x);
            if(attackedSquares[x] == 1){
                
                var debug = Instantiate(debugPrefab, new Vector3(file-1, rank-1, -0.02f), Quaternion.identity);
                debugPrefabs.Add(debug);
            }
        }

    }

    //takes in indexes
    public void ShowDebugSquares(List<int> index){

        //Destroy current debug prefabs
        for(int z = 0; z< debugPrefabs.Count; z++){
            Destroy(debugPrefabs[z]);
        }
        debugPrefabs.Clear();
        for(int x = 0; x<64; x++){
            int rank = board.IndexToRank(x);
            int file = board.IndexToFile(x);
            if(index.Contains(x)){
                var debug = Instantiate(debugPrefab, new Vector3(file-1, rank-1, -0.02f), Quaternion.identity);
                debugPrefabs.Add(debug);
            }
        }

    }
    //UI
    public void UpdateUseClock(bool input){
        useClock = input;
    }
    public void UpdateCustomPos(bool input){
        useCustomPos = input;
    }

    //Testing
    public void UseUndoMoveButton(){
        board.UndoMove(board.lastMove);
        UpdateBoard();
    }

}
