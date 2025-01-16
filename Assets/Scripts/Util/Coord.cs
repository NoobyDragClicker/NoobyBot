using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public static class Coord
{
    public static String GetNotationFromIndex(int index){
        string notation;
        int rank = IndexToRank(index);
        int fileInt = IndexToFile(index);
        char file;
        switch (fileInt){
            case 1: file = 'a'; break;
            case 2: file = 'b'; break;
            case 3: file = 'c'; break;
            case 4: file = 'd'; break;
            case 5: file = 'e'; break;
            case 6: file = 'f'; break;
            case 7: file = 'g'; break;
            case 8: file = 'h'; break;
            default: Debug.Log("out of range"); file = 'x'; break;
        }
        notation = file.ToString() + rank.ToString();
        return notation;
    }

    public static String GetMoveNotation(int index1, int index2){
        string start = GetNotationFromIndex(index1);
        string end = GetNotationFromIndex(index2);

        return start + " " + end;
    }

    public static int IndexToRank(int index){
        if(index <=7 && index >= 0){return 8;}
        else if(index <=15 && index > 7){return 7;}
        else if(index <=23 && index > 15){return 6;}
        else if(index <=31 && index > 23){return 5;}
        else if(index <=39 && index > 31){return 4;}
        else if(index <=47 && index > 39){return 3;}
        else if(index <=55 && index > 47){return 2;}
        else if(index <=63 && index > 55){return 1;}
        else{
            Debug.Log("out of range");
            return 0;
        }
    }

    public static int IndexToFile(int index){
        int rank = IndexToRank(index);
        int file = index - ((8 - rank)*8) + 1;
        return file;
    }


}
