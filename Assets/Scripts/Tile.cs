using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    int isSelected = 0;
    [SerializeField] private GameObject highlight;
    [SerializeField] private Color white, black, green, red;
    [SerializeField] private SpriteRenderer render;
    private UIManager uiManager;

    public void Init(bool isOffset){
        uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
        render.color = isOffset ? white : black;
    }

    //Code to change color - do not touch
    void OnMouseOver() {
        if(Input.GetMouseButtonDown(1)){
            if(isSelected == 0){
                highlight.SetActive(true);
                highlight.GetComponent<SpriteRenderer>().color = red;
                isSelected = 1;
            } else if(isSelected == 1){
                highlight.GetComponent<SpriteRenderer>().color = green;
                isSelected = 2;
            } else{
                highlight.SetActive(false);
                isSelected = 0;
            }
        }
    }


    void OnMouseDown(){
        uiManager.SelectPiece(Mathf.RoundToInt(transform.position.x) + 1, Mathf.RoundToInt(transform.position.y) + 1);
    }   
}
