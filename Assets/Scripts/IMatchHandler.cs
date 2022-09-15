using UnityEngine;

public interface IMatchHandler
{
    Node GetNodeAtPoint(Point p);

    int getValueAtPoint(Point p);
    int fillPiece();
    void ResetPiece(NodePiece piece);
    Vector2 getPositionFromPoint(Point p);
}