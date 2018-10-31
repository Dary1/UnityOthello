using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class OthelloBoard : MonoBehaviour {
    public int CurrentTurn = 0;
    public GameObject ScoreBoard;
    public Text ScoreBoardText;
    public GameObject Template;
    public int BoardSize = 8;
    public List<Color> PlayerChipColors;    
    public List<Vector2> DirectionList;
    static OthelloBoard instance;
    public static OthelloBoard Instance { get { return instance; } }
    OthelloCell[,] OthelloCells;
    public int EnemyID { get { return (CurrentTurn+1) % 2; } }
    void Start()
    {
        instance = this;
        OthelloBoardIsSquareSize();
       
        OthelloCells = new OthelloCell[BoardSize, BoardSize];
        float cellAnchorSize = 1.0f / BoardSize;
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                CreateNewCell(x,y, cellAnchorSize);
            }
        }
        ScoreBoard.GetComponent<RectTransform>().SetSiblingIndex(BoardSize*BoardSize+1);
        GameObject.Destroy(Template);
        InitializeGame();
    }
    private void CreateNewCell(int x, int y, float cellAnchorSize)
    {
        GameObject go = GameObject.Instantiate(Template, this.transform);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(x * cellAnchorSize, y * cellAnchorSize);
        r.anchorMax = new Vector2((x + 1) * cellAnchorSize, (y + 1) * cellAnchorSize);
        OthelloCell oc = go.GetComponent<OthelloCell>();
        OthelloCells[x, y] = oc;
        oc.Location.x = x;
        oc.Location.y = y;
    }
    private void OthelloBoardIsSquareSize()
    {
        RectTransform rect = this.GetComponent<RectTransform>();
        if (Screen.width > Screen.height)
        {
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.height);
        }
        else
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.width);
    }
    public void InitializeGame()
    {
        ScoreBoard.gameObject.SetActive(false);
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                OthelloCells[x, y].OwnerID = -1;
            }
        }
        OthelloCells[3, 3].OwnerID = 0;
        OthelloCells[4, 4].OwnerID = 0;
        OthelloCells[4, 3].OwnerID = 1;
        OthelloCells[3, 4].OwnerID = 1;
    }
    internal bool CanPlaceHere(Vector2 location)
    {
        if (OthelloCells[(int)location.x, (int)location.y].OwnerID != -1)
            return false;

        for (int direction = 0; direction < DirectionList.Count; direction++)
        {
            Vector2 directionVector = DirectionList[direction];
            if (FindAllyChipOnOtherSide(directionVector, location, false) != null)
            {
                return true;
            }
        }
        return false;
    }
    internal void PlaceHere(OthelloCell othelloCell)
    {
        for (int direction = 0; direction < DirectionList.Count; direction++)
        {
            Vector2 directionVector = DirectionList[direction];
            OthelloCell onOtherSide = FindAllyChipOnOtherSide(directionVector, othelloCell.Location, false);
            if (onOtherSide != null)
            {
                ChangeOwnerBetween(othelloCell, onOtherSide, directionVector);
            }
        }
        OthelloCells[(int)othelloCell.Location.x, (int)othelloCell.Location.y].OwnerID = CurrentTurn;
    }
    private OthelloCell FindAllyChipOnOtherSide(Vector2 directionVector, Vector2 from, bool EnemyFound)
    {
        Vector2 to = from + directionVector;
        if (IsInRangeOfBoard(to) && OthelloCells[(int)to.x, (int)to.y].OwnerID != -1)
        {
            if (OthelloCells[(int)to.x, (int)to.y].OwnerID == OthelloBoard.Instance.CurrentTurn)
            {
                if (EnemyFound)
                    return OthelloCells[(int)to.x, (int)to.y];
                return null;
            }
            else
                return FindAllyChipOnOtherSide(directionVector, to, true);
        }
        return null;
    }
    private bool IsInRangeOfBoard(Vector2 point)
    {
        return point.x >= 0 && point.x < BoardSize && point.y >= 0 && point.y < BoardSize;
    }
    private void ChangeOwnerBetween(OthelloCell from, OthelloCell to, Vector2 directionVector)
    {
        for (Vector2 location = from.Location + directionVector; location != to.Location; location += directionVector)
        {
            OthelloCells[(int)location.x, (int)location.y].OwnerID = CurrentTurn;
        }
    }
    internal void EndTurn(bool isAlreadyEnded)
    {        
        CurrentTurn = EnemyID;
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                if (CanPlaceHere(new Vector2(x, y)))
                {
                    return;
                }
            }
        }
        if (isAlreadyEnded)
            GameOver();
        else {
            EndTurn(true);
        }            
    }
    public void GameOver()
    {
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                OthelloCells[x, y].GetComponent<Button>().interactable = false;
            }
        }
        int white = CountScoreFor(0);
        int black = CountScoreFor(1);
        if (white > black)
            ScoreBoardText.text = "White wins " + white + ":" + black;
        else if (black > white)
            ScoreBoardText.text = "Black wins " + black + ":" + white;
        else
            ScoreBoardText.text = "Draw! " + white + ":" + black;
        ScoreBoard.gameObject.SetActive(true);
    }
    private int CountScoreFor(int owner)
    {
        int count = 0;
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                if (OthelloCells[x, y].OwnerID == owner) {
                    count++;
                }
            }
        }
        return count;
    }
}