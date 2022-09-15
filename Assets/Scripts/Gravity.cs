using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    private IMatchHandler _matchHandler;

    private GameObject _nodePiece;

    private Sprite[] _pieces;
    private RectTransform _gameBoard;
    private List<NodePiece> _dead;
    private List<NodePiece> _update;

    private int[] _fills;

    private int _width;
    private int _height;

    public void Initialize(IMatchHandler matchHandler, int width, int height, List<NodePiece> update, 
        List<NodePiece> dead, Sprite[] pieces, RectTransform gameBoard, GameObject nodePiece, int[] fills)
    {
        _pieces = pieces;
        _gameBoard = gameBoard;
           _nodePiece = nodePiece;
        _fills = fills;
        _dead = dead;
        _update = update;
        _matchHandler = matchHandler;
        _width = width;
        _height = height;
    }

    public void ApplyGravityToBoard()
    {
        for (int x = 0; x < _width; x++)
        {
            Debug.Log("tut");
            for (int y = (_height - 1); y >= 0; y--) //Start at the bottom and grab the next
            {
                Point p = new Point(x, y);
                Node node = _matchHandler.GetNodeAtPoint(p);
                int val = _matchHandler.getValueAtPoint(p);
                if(val != 0)
                    continue;
                for (int ny = (y - 1); ny >= -1; ny--)
                {
                    Point next = new Point(x, ny);
                    int nextVal = _matchHandler.getValueAtPoint(next);
                    if (nextVal == 0)
                        continue;
                    if (nextVal != -1)
                    {
                        Node gotten = _matchHandler.GetNodeAtPoint(next);
                        NodePiece piece = gotten.getPiece();

                        //Set the hole
                        node.SetPiece(piece);
                        _update.Add(piece);

                        //Make a new hole
                        gotten.SetPiece(null);
                    }
                    else//Use dead ones or create new pieces to fill holes (hit a -1) only if we choose to
                    {
                        int newVal = _matchHandler.fillPiece();
                        NodePiece piece;
                        Point fallPnt = new Point(x, (-1 - _fills[x]));
                        if (_dead.Count > 0)
                        {
                            NodePiece revived = _dead[0];
                            revived.gameObject.SetActive(true);
                            piece = revived;

                            _dead.RemoveAt(0);
                        }
                        else
                        {
                            GameObject obj = Instantiate(_nodePiece, _gameBoard);
                            NodePiece n = obj.GetComponent<NodePiece>();
                            piece = n;
                        }

                        piece.Initialize(newVal, p, _pieces[newVal - 1]);
                        piece.rect.anchoredPosition = _matchHandler.getPositionFromPoint(fallPnt);

                        Node hole = _matchHandler.GetNodeAtPoint(p);
                        hole.SetPiece(piece);
                        _matchHandler.ResetPiece(piece);
                        _fills[x]++;
                    }
                    break;
                }
            }
        }
    }

    private bool CheckHole(int val)
    {
        if (val != 0)
            return true;
        return false;
    }
}
