﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match3 : MonoBehaviour, IMatchHandler
{
    [SerializeField] private Gravity _gravity;
    [SerializeField] private MatchConnected _matchConnected;
    private FlippedPieces _flip;

    public ArrayLayout boardLayout;
    [Header("UI Elements")]
    public Sprite[] pieces;
    public RectTransform gameBoard;
    public RectTransform killedBoard;

    [Header("Prefabs")]
    public GameObject nodePiece;
    public GameObject killedPiece;

    int width = 9;
    int height = 14;
    int[] fills;
    Node[,] board;

    public List<NodePiece> update;
    public List<FlippedPieces> flipped;
    public List<NodePiece> dead;
    public List<KilledPiece> killed;

    System.Random random;

    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        fills = new int[width];
        string seed = getRandomSeed();
        random = new System.Random(seed.GetHashCode());
        update = new List<NodePiece>();
        flipped = new List<FlippedPieces>();
        dead = new List<NodePiece>();
        killed = new List<KilledPiece>();

        InitializeBoard();
        _matchConnected.Initialize(this);
        _gravity.Initialize(this, width, height, update, dead, pieces, gameBoard, nodePiece, fills);
        VerifyBoard();
        InstantiateBoard();

    }

    void Update()
    {
        List<NodePiece> finishedUpdating = new List<NodePiece>();
        for (int i = 0; i < update.Count; i++)
        {
            NodePiece piece = update[i];
            if (!piece.UpdatePiece())
            {
                finishedUpdating.Add(piece);
            }
        }

        for (int i = 0; i < finishedUpdating.Count; i++)
        {
            NodePiece piece = finishedUpdating[i];
            FlippedPieces flip = _flip =  getFlipped(piece);
            NodePiece flippedPiece = null;

            int x = (int)piece.index.x;
            fills[x] = Mathf.Clamp(fills[x] - 1, 0, width);

            List<Point> connected = _matchConnected.IsConnected(piece.index, true);

            if (CheckFlipped()) //If we flipped to make this update
            {  
                flippedPiece = flip.getOtherPiece(piece);
                _matchConnected.AddPoints(ref connected, _matchConnected.IsConnected(flippedPiece.index, true));
            }

            CheckMatchThree(connected, piece, flippedPiece);
           
            flipped.Remove(flip); //Remove the flip after update
            update.Remove(piece);
        }
    }

    private void CheckMatchThree(List<Point> connected, NodePiece piece, NodePiece flippedPiece)
    {
        if (connected.Count == 0) //If we didn't make a match
        {
            if (CheckFlipped()) //If we flipped
                FlipPieces(piece.index, flippedPiece.index, false); //Flip back
        }
        else //If we made a match
        {
            MadeMatch(connected);
        }
    }

    private bool CheckFlipped()
    {
        if (_flip == null)
            return false;
        return true;
    }

    private void MadeMatch(List<Point> connected)
    {
        RemoveNodeConnected(connected);
        _gravity.ApplyGravityToBoard();
    }

    private void RemoveNodeConnected(List<Point> connected)
    {
        Debug.Log(connected.Count);
        foreach (Point pnt in connected) //Remove the node pieces connected
        {
            KillPiece(pnt);
            Node node = GetNodeAtPoint(pnt);
            NodePiece nodePiece = node.getPiece();
            if (nodePiece != null)
            {
                nodePiece.gameObject.SetActive(false);
               dead.Add(nodePiece);
            }
            node.SetPiece(null);
        }
    }

    private FlippedPieces getFlipped(NodePiece p)
    {
        FlippedPieces flip = null;
        for (int i = 0; i < flipped.Count; i++)
        {
            if (flipped[i].getOtherPiece(p) != null)
            {
                flip = flipped[i];
                break;
            }
        }
        return flip;
    }



    void InitializeBoard()
    {
        board = new Node[width, height];
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                board[x, y] = new Node((boardLayout.rows[y].row[x] == ObstacleType.Hole) ? 
                    TileType.Hole : (TileType)fillPiece(),
                    new Point(x, y));
            }
        }
    }

    void VerifyBoard()
    {
        List<int> remove;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Point p = new Point(x, y);
                int val = getValueAtPoint(p);
                if (val <= 0) continue;

                remove = new List<int>();
                while (_matchConnected.IsConnected(p, true).Count > 0)
                {
                    val = getValueAtPoint(p);
                    if (!remove.Contains(val))
                        remove.Add(val);
                    setValueAtPoint(p, newValue(ref remove));
                }
            }
        }
    }

    void InstantiateBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Node node = GetNodeAtPoint(new Point(x, y));

                int val = node.value;
                if (val <= 0) continue;
                GameObject p = Instantiate(nodePiece, gameBoard);
                NodePiece piece = p.GetComponent<NodePiece>();
                RectTransform rect = p.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(32 + (64 * x), -32 - (64 * y));
                piece.Initialize(val, new Point(x, y), pieces[val - 1]);
                node.SetPiece(piece);
            }
        }
    }
     
    public void ResetPiece(NodePiece piece)
    {
        piece.ResetPosition();
        update.Add(piece);
    }

    public void FlipPieces(Point one, Point two, bool main)
    {
        if (getValueAtPoint(one) < 0) return;

        Node nodeOne = GetNodeAtPoint(one);
        NodePiece pieceOne = nodeOne.getPiece();
        if (getValueAtPoint(two) > 0)
        {
            Node nodeTwo = GetNodeAtPoint(two);
            NodePiece pieceTwo = nodeTwo.getPiece();
            nodeOne.SetPiece(pieceTwo);
            nodeTwo.SetPiece(pieceOne);

            if(main)
                flipped.Add(new FlippedPieces(pieceOne, pieceTwo));

            update.Add(pieceOne);
            update.Add(pieceTwo);
        }
        else
            ResetPiece(pieceOne);
    }

    void KillPiece(Point p)
    {
        List<KilledPiece> available = new List<KilledPiece>();

        for (int i = 0; i < killed.Count; i++)
        {
            if (!killed[i].falling)
                available.Add(killed[i]);
        }      

        KilledPiece set = null;
        if (available.Count > 0)
            set = available[0];
        else
        {
            GameObject kill = GameObject.Instantiate(killedPiece, killedBoard);
            KilledPiece kPiece = kill.GetComponent<KilledPiece>();
            set = kPiece;
            killed.Add(kPiece);
        }

        int val = getValueAtPoint(p) - 1;
        if (set != null && val >= 0 && val < pieces.Length)
            set.Initialize(pieces[val], getPositionFromPoint(p));
    }

    public int fillPiece()
    {
        int val = 1;
        val = (random.Next(0, 100) / (100 / pieces.Length)) + 1;
        return val;
    }

    public int getValueAtPoint(Point p)
    {
        if (p.x < 0 || p.x >= width || p.y < 0 || p.y >= height) return -1;
        {
            return board[p.x, p.y].value;
        }
       
    }

    void setValueAtPoint(Point p, int v)
    {
        board[p.x, p.y].value = v;
    }

    public Node GetNodeAtPoint(Point p)
    {
        return board[p.x, p.y];
    }

    int newValue(ref List<int> remove)
    {
        List<int> available = new List<int>();
        for (int i = 0; i < pieces.Length; i++)
            available.Add(i + 1);
        foreach (int i in remove)
            available.Remove(i);

        if (available.Count <= 0) return 0;
        return available[random.Next(0, available.Count)];
    }

    string getRandomSeed()
    {
        string seed = "";
        string acceptableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdeghijklmnopqrstuvwxyz1234567890!@#$%^&*()";
        for (int i = 0; i < 20; i++)
            seed += acceptableChars[Random.Range(0, acceptableChars.Length)];
        return seed;
    }

    public Vector2 getPositionFromPoint(Point p)
    {
        return new Vector2(32 + (64 * p.x), -32 - (64 * p.y));
    }
}
