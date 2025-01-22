using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;

public class TournamentManager : MonoBehaviour
{
    const int numBoards = 2;
    const int startTime = 75;
    const int maxGames = 50;
    int gamesPlayed;
    int gamesFinished;
    int testPlayerWins, oldPlayerWins, draws;

    [SerializeField]
    Tile tile;

    [SerializeField]
    AISettings testSettings = new AISettings(true, 8);
    [SerializeField]
    AISettings oldSettings = new AISettings(false, 8);

    BoardManager[] boards = new BoardManager[numBoards];
    bool[] isWhiteTest = new bool[numBoards];

    void Start()
    {    
        Application.targetFrameRate = 144;
        for(int boardNumber = 0; boardNumber < numBoards; boardNumber++){
            if(boardNumber % 2 == 0){isWhiteTest[boardNumber] = true;}
            else{isWhiteTest[boardNumber] = false;}

            BoardManager currentBoard = new BoardManager(boardNumber);
            boards[boardNumber] = currentBoard;
            currentBoard.moveMade += UpdateBoard;
            currentBoard.gameFinished += FinishedGame;
        }
    }
    void Update(){
        for(int x = 0; x < numBoards; x++){
            boards[x].Update();
        }
    }
    public void StartTournament(){
        for(int x = 0; x< numBoards; x++){
            SpawnBoard(x * 12);
            StartGame(x);
        }
    }

    void StartGame(int boardNumber){
        AISettings whiteSettings = isWhiteTest[boardNumber] ? testSettings : oldSettings;
        AISettings blackSettings = isWhiteTest[boardNumber] ? oldSettings : testSettings;
        boards[boardNumber].StartGame(true, startTime, false, "", whiteSettings, blackSettings);
        gamesPlayed ++;
    }

    void SpawnBoard(int offset){
        //Generates actual tiles
        for(int x = 0; x< 8; x++){
            for(int y = 0; y < 8; y++){
                var spawnedTile = Instantiate(tile, new Vector3(x + offset, y, 0.01f), Quaternion.identity);
                spawnedTile.name = $"tile {x + offset} {y}";

                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.Init(isOffset);
            }
        }
    }

    void FinishedGame(BoardManager.ResultStatus result, int boardNumber){
        if(result == BoardManager.ResultStatus.Draw){draws ++;}
        else if(result == BoardManager.ResultStatus.White_Won){
            if(isWhiteTest[boardNumber] == true){
                testPlayerWins++;
            } else{
                oldPlayerWins++;
            }
        } else if(result == BoardManager.ResultStatus.Black_Won){
            if(isWhiteTest[boardNumber] == true){
                oldPlayerWins++;
            } else{
                testPlayerWins++;
            }
        }
        gamesFinished ++;
        if(gamesPlayed < maxGames){
            StartGame(boardNumber);
        }
        if(gamesFinished == maxGames){
            FinishTournament();
        }
        if(gamesPlayed > maxGames){
            FinishTournament();
        }
    }

    void FinishTournament(){
        Debug.Log("Test player wins: " + testPlayerWins);
        Debug.Log("Old player wins: " + oldPlayerWins);
        Debug.Log("Draws: " + draws);

    }

    public void UpdateBoard(int boardNumber){

    }
}
