using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Zobrist
{

    static Zobrist(){
        //Read the random numbers to an array for each piece and color
        //Then sets the random numbers for castling rights, en passant file, and side to move
    }

    //Only used at start of game, all other changes done in Move and Unmove
    static ulong CalculateZobrist(Board board){
        return 0;
    }
    //Writes all random numbers to a file
    static void WriteRandomNumbers(){

    }


    //Reads in random numbers (if the file doesnt exist, call WriteRandomNumbers), done at start of the game
    static void ReadRandomNumbers(){

    }
    //Returns a random 64 bit number
    static ulong RandomUnsigned64BitNumber(){
        return 0;
    }

    

}
