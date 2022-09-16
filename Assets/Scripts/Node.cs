using System.Collections.Generic;

[System.Serializable]
public class Node
{
    public int value;
    public Point index;
    NodePiece piece;

    public Node(TileType tileType, Point i)
    {
        value = (int)tileType;
        index = i;
    }

    public void SetPiece(NodePiece p)
    {
        piece = p;
        value = (piece == null) ? 0 : piece.value;
        if (piece == null) 
            return;
        piece.SetIndex(index);
    }

    public NodePiece getPiece()
    {
        return piece;
    }
}

public enum TileType
{
    Blank = 0,
    Cube = 1,
    Sphere = 2,
    Cylinder = 3,
    Pryamid = 4,
    Diamond = 5,
    Hole = -1
}
