using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChecksBoard : MonoBehaviour
{
    public Piece[,] pieces = new Piece[8, 8];
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;

    private Vector3 boardOffset = new Vector3(-4.0f, 0, -4.0f);
    private Vector3 pieceOffset = new Vector3(0.5f, 0, 0.5f); //puts the pieces on the squares properly

    public bool isWhite;
    private bool isWhiteTurn;
    private bool captured;
    private bool isVictory;

    private Piece selectedPiece;
    private Move currentMove;
    private List<Piece> forcedPieces;

    private Vector2 mouseOver;
    private Vector2 startDrag;
    private Vector2 endDrag;

    private int isvictorystate;
    private int state;
    private int[] blackLoc = new int[8];
    private int[] whiteLoc = new int[8];


    private void Start()
    {
        isWhiteTurn = true;
        forcedPieces = new List<Piece>();
        GenerateBoard();
        currentMove = new Move();
        isVictory = false;
        isvictorystate = -100;
        state = 0;
        startDrag = Vector2.zero;
    }

    private void Update()
    {
        UpdateMouseOver();
        isvictorystate = CheckVictory();   //checks for victory state.

        //computers turn
        if (!isWhite && (!isVictory || isvictorystate == 0))
        {
            currentMove = GetMove();
            TryMove(currentMove.piece1.Row, currentMove.piece1.Column, currentMove.piece2.Row, currentMove.piece2.Column);
        }

        // players turn
        if (isvictorystate == 0 || !isVictory)
        {
            int x = (int)mouseOver.x;
            int y = (int)mouseOver.y;

            if (selectedPiece != null)
            {
                UpdatePieceDrag(selectedPiece);
            }

            if (Input.GetMouseButtonDown(0))
            {
                SelectPiece(x, y);// when mouse button pressed, piece is selected
            }

            if (Input.GetMouseButtonUp(0))
            {
                TryMove((int)startDrag.x, (int)startDrag.y, x, y); // trys to drop piece to new location
            }
        }

        // says who won
        if (isvictorystate != 0 && state == 0)
        {
            if (isvictorystate == -1)
                Debug.Log("White team wins");
            else if (isvictorystate == 1)
                Debug.Log("Black team wins");
            state = 1;
        }
    }

    private void UpdateMouseOver() // sets the camera and max height it can go, detects where mouse is
    {
        if (!Camera.main)
        {
            Debug.Log("Can't find main camera!");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            mouseOver.x = (int)(hit.point.x - boardOffset.x);
            mouseOver.y = (int)(hit.point.z - boardOffset.z);
        }
        else
        {
            mouseOver.x = -1;
            mouseOver.y = -1;
        }
    }

    private void UpdatePieceDrag(Piece p) // drags pieces
    {
        if (!Camera.main)
        {
            Debug.Log("Can't find main camera!");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            p.transform.position = hit.point + Vector3.up;
        }

    }

    private void SelectPiece(int x, int y)
    {
        Piece p = pieces[x, y]; // checks for piece under array set

        if (x < 0 || x >= 8 || y < 0 || y >= 8)// out of bounds
            return;


        if (p != null && p.isWhite == isWhite)
        {
            if (forcedPieces.Count == 0) // if theres nothing under the forced list, do this
            {
                selectedPiece = p;
                startDrag = mouseOver;
            }
            else
            {
                if (forcedPieces.Find(pieceForced => pieceForced == p) == null)//look for piece under forced list
                {
                    return;
                }
                selectedPiece = p;
                startDrag = mouseOver;

            }
        }
    }

    private void TryMove(int x1, int y1, int x2, int y2)
    {
        forcedPieces = PossibleMove();

        //startDrag = Vector2.zero;
        endDrag = new Vector2(x2, y2);
        selectedPiece = pieces[x1, y1];


        if (x2 < 0 || x2 > 8 || y2 < 0 || y2 > 8) // if its out of bounds, resets to originally posititon
        {
            if (selectedPiece != null)
            {
                MovePiece(selectedPiece, x1, y1);
            }

            startDrag = Vector2.zero;
            selectedPiece = null;
            return;
        }

        if (selectedPiece != null)
        {
            if (endDrag == startDrag) // hasnt moved
            {
                MovePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }
            if (x2 == 0 && y2 == 0)
            {
                Debug.Log("pos");
            }
            //checks for valid move
            if (selectedPiece.ValidMove(pieces, x1, y1, x2, y2))
            {
                //any captures?
                if (Mathf.Abs(x2 - x1) == 2)
                {
                    Piece p = pieces[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (p != null)
                    {
                        pieces[(x1 + x2) / 2, (y1 + y2) / 2] = null;
                        Destroy(p.gameObject);
                        captured = true;
                    }
                }

                //suppose to capture
                if (forcedPieces.Count != 0 && !captured)
                {
                    MovePiece(selectedPiece, x1, y1);
                    startDrag = Vector2.zero;
                    selectedPiece = null;
                    return;
                }

                pieces[x2, y2] = selectedPiece;
                pieces[x1, y1] = null;
                MovePiece(selectedPiece, x2, y2);
                EndTurn();
            }
            else
            {
                MovePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }
        }
    }

    private void EndTurn()
    {
        int x = (int)endDrag.x;
        int y = (int)endDrag.y;

        //becomes king
        if (selectedPiece != null)
        {
            if (selectedPiece.isWhite && !selectedPiece.isKing && y == 0)
            {
                selectedPiece.isKing = true;
                selectedPiece.transform.Rotate(Vector3.right * 180);
            }
            else if (!selectedPiece.isWhite && !selectedPiece.isKing && y == 7)
            {
                selectedPiece.isKing = true;
                selectedPiece.transform.Rotate(Vector3.right * 180);
            }
        }

        selectedPiece = null;
        startDrag = Vector2.zero;

        if (PossibleMove(selectedPiece, x, y).Count != 0 && captured) //checks for double jump for piece that just captured
        {
            return;
        }

        isWhiteTurn = !isWhiteTurn;
        isWhite = !isWhite;
        captured = false;
    }

    //if computer's wins return -1, if players win return 1, draw state return 0.
    private int CheckVictory()
    {
        var pieces = FindObjectsOfType<Piece>();
        bool white = false;
        bool black = false;

        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i].isWhite)
            {
                white = true;
            }
            else
            {
                black = true;
            }
        }
        if (!white)
        {
            return 1;
        }
        if (!black)
        {
            return -1;
        }
        isVictory = !isVictory;
        return 0;
    }

    private List<Piece> PossibleMove(Piece p, int x, int y)
    {
        forcedPieces = new List<Piece>();

        if (pieces[x, y].IsForcedToMove(pieces, x, y))
        {
            forcedPieces.Add(pieces[x, y]);
        }
        return forcedPieces;
    }

    private List<Piece> PossibleMove()
    {
        forcedPieces = new List<Piece>();

        for (int i = 0; i < 8; i++) // checks all pieces
        {
            for (int j = 0; j < 8; j++)
            {
                if (pieces[i, j] != null && pieces[i, j].isWhite == isWhiteTurn)
                {
                    if (pieces[i, j].IsForcedToMove(pieces, i, j))
                    {
                        forcedPieces.Add(pieces[i, j]); // list of moves that must be made
                    }
                }
            }
        }
        return forcedPieces;
    }

    private void GenerateBoard()
    {
        // Generate White Pieces
        for (int y = 0; y < 3; y++)
        {
            bool oddRow = (y % 2 == 0); // checks for odd row
            for (int x = 0; x < 8; x += 2)
            {
                // Generate Piece
                GeneratePiece((oddRow) ? x : x + 1, y);
            }
        }

        // Generate Black Pieces
        for (int y = 7; y > 4; y--)
        {
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2)
            {
                // Generate Piece
                GeneratePiece((oddRow) ? x : x + 1, y);
            }
        }
    }

    private void GeneratePiece(int x, int y)
    {
        bool isPieceWhite = (y < 3) ? false : true;
        GameObject go = Instantiate((isPieceWhite) ? whitePiecePrefab : blackPiecePrefab) as GameObject;

        // shows add position.
        if (isPieceWhite == false)
        {
            go.AddComponent<Animation>();
            go.gameObject.name = "Blackpiece" + x + "_" + y;
        }
        go.transform.SetParent(transform);
        Piece p = go.GetComponent<Piece>();
        pieces[x, y] = p;
        MovePiece(p, x, y);
    }

    private void MovePiece(Piece p, int x, int y)
    {
        if (!p.isWhite)
        {
            p.gameObject.name = "Blackpiece" + x + "_" + y;
        }
        else
        {
            p.gameObject.name = "Whitepiece" + x + "_" + y;
        }
        p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + boardOffset + pieceOffset;
    }

    //returns weight value of each pieces
    private int WeightFuntion(int x, int y, int x1, int y1)
    {
        //sets board value, outside is safest
        int[,] weight = {
                { 4,0,4,0,4,0,4,0 },
                { 0,3,0,3,0,3,0,4 },
                { 4,0,2,0,2,0,3,0 },
                { 0,3,0,1,0,2,0,4 },
                { 4,0,2,0,1,0,3,0 },
                { 0,3,0,2,0,2,0,4 },
                { 4,0,3,0,3,0,3,0 },
                { 0,4,0,4,0,4,0,4 }};
        return weight[x, y];
    }

    //0 is none, 1 is black piece, 2 is white piece, 3 is black king, 4 is white king
    private int GetStatePiece(int x, int y)
    {

        if (x >= 0 && x < 8 && y >= 0 && y < 8)
        {
            if (pieces[x, y] != null)
            {
                if (pieces[x, y].isWhite)
                {
                    if (pieces[x, y].isKing)
                    {
                        return 4;
                    }
                    else
                        return 2;
                }
                else
                {
                    if (pieces[x, y].isKing)
                    {
                        return 3;
                    }
                    else
                        return 1;
                }
            }
            return 0;
        }
        else
        {
            return -1;
        }
    }

    //returns a list of pieces which captured enemy after move
    private List<Move> CheckJumps()
    {
        List<Move> jumps = new List<Move>();

        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {

                if (GetStatePiece(r, c) == 1)
                {
                    if ((GetStatePiece(r - 2, c + 2) == 0) && ((GetStatePiece(r - 1, c + 1) == 2) || (GetStatePiece(r - 1, c + 1) == 4)))
                    {
                        jumps.Add(new Move(new Piece(r, c), new Piece(r - 2, c + 2)));
                    }
                    if ((GetStatePiece(r + 2, c + 2) == 0) && ((GetStatePiece(r + 1, c + 1) == 2) || (GetStatePiece(r + 1, c + 1) == 4)))
                    {
                        jumps.Add(new Move(new Piece(r, c), new Piece(r + 2, c + 2)));
                    }
                }

                if (GetStatePiece(r, c) == 3)
                {
                    if ((GetStatePiece(r - 2, c - 2) == 0) && ((GetStatePiece(r - 1, c - 1) == 2) || (GetStatePiece(r - 1, c - 1) == 4)))
                    {
                        jumps.Add(new Move(new Piece(r, c), new Piece(r - 2, c - 2)));
                    }
                    if ((GetStatePiece(r - 2, c + 2) == 0) && ((GetStatePiece(r - 1, c + 1) == 2) || (GetStatePiece(r - 1, c + 1) == 4)))
                    {
                        jumps.Add(new Move(new Piece(r, c), new Piece(r - 2, c + 2)));
                    }
                    if ((GetStatePiece(r + 2, c - 2) == 0) && ((GetStatePiece(r + 1, c - 1) == 2) || (GetStatePiece(r + 1, c - 1) == 4)))
                    {
                        jumps.Add(new Move(new Piece(r, c), new Piece(r + 2, c - 2)));
                    }
                    if ((GetStatePiece(r + 2, c + 2) == 0) && ((GetStatePiece(r + 1, c + 1) == 2) || (GetStatePiece(r + 1, c + 1) == 4)))
                    {
                        jumps.Add(new Move(new Piece(r, c), new Piece(r + 2, c + 2)));
                    }
                }
            }
        }
        return jumps;
    }

    //best piece select.
    private Move GetMove()
    {
        List<Move> avaliableMoves = new List<Move>();
        avaliableMoves = GetAvaliableMoves();

        int index = new int();

        if (avaliableMoves.Count < 1)
        {
            isVictory = true;
            return null;
        }
        else if (avaliableMoves.Count == 1)
            return avaliableMoves[0];
        else
        {
            int[] orderIndex = new int[avaliableMoves.Count];
            for (int i = 0; i < avaliableMoves.Count; i++)
            {
                orderIndex[i] = GetWeightValue(avaliableMoves[i]);
            }
            index = GetMaxIndex(orderIndex);
            return avaliableMoves[index];
        }
    }

    //return list of available pieces(jumps and no jumps)
    private List<Move> GetAvaliableMoves()
    {
        List<Piece> currentPieces = new List<Piece>();
        List<Move> avaliableMoves = new List<Move>();
        List<Move> jumpMoves = CheckJumps();
        if (jumpMoves.Count > 0)
        {
            avaliableMoves = jumpMoves;//jump list
        }
        else
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    if ((GetStatePiece(r, c) == 1) || (GetStatePiece(r, c) == 3))
                    {
                        currentPieces.Add(new Piece(r, c));
                    }
                }
            }
            for (int i = 0; i < currentPieces.Count; i++)
            {
                List<Move> forMoves = CheckForMoves(currentPieces[i]);
                if (forMoves.Count > 0)
                {
                    for (int j = 0; j < forMoves.Count; j++)
                    {
                        avaliableMoves.Add(forMoves[j]);//no jumps list
                    }
                }
            }
        }
        return avaliableMoves;
    }

    //move available piece list(no jump)
    private List<Move> CheckForMoves(Piece piece)
    {
        List<Move> moves = new List<Move>();
        if (GetStatePiece(piece.Row, piece.Column) == 3)
        {
            if (GetStatePiece(piece.Row - 1, piece.Column - 1) == 0)
                moves.Add(new Move(new Piece(piece.Row, piece.Column), new Piece(piece.Row - 1, piece.Column - 1)));

            if (GetStatePiece(piece.Row - 1, piece.Column + 1) == 0)
                moves.Add(new Move(new Piece(piece.Row, piece.Column), new Piece(piece.Row - 1, piece.Column + 1)));

            if (GetStatePiece(piece.Row + 1, piece.Column - 1) == 0)
                moves.Add(new Move(new Piece(piece.Row, piece.Column), new Piece(piece.Row + 1, piece.Column - 1)));

            if (GetStatePiece(piece.Row + 1, piece.Column + 1) == 0)
                moves.Add(new Move(new Piece(piece.Row, piece.Column), new Piece(piece.Row + 1, piece.Column + 1)));

        }
        else if (GetStatePiece(piece.Row, piece.Column) == 1)
        {
            if (GetStatePiece(piece.Row - 1, piece.Column + 1) == 0)
                moves.Add(new Move(new Piece(piece.Row, piece.Column), new Piece(piece.Row - 1, piece.Column + 1)));

            if (GetStatePiece(piece.Row + 1, piece.Column + 1) == 0)
                moves.Add(new Move(new Piece(piece.Row, piece.Column), new Piece(piece.Row + 1, piece.Column + 1)));
        }
        return moves;
    }
 
    //piece's weight value.
    private int GetWeightValue(Move setMoves)
    {
        int x1 = setMoves.piece1.Row;
        int y1 = setMoves.piece1.Column;
        int x2 = setMoves.piece2.Row;
        int y2 = setMoves.piece2.Column;

        int valueright = 0;
        int valueleft = 0;
        int valueKing = 4;
        int returnvalue = 0;

        //danger state
        if ((x1 + 1 != x2 && y1 + 1 != y2 && (GetStatePiece(x1 + 1, y1 + 1) == 2 || GetStatePiece(x1 + 1, y1 + 1) == 4) && GetStatePiece(x1 - 1, y1 - 1) == 0)
            || (x1 - 1 != x2 && y1 + 1 != y2 && (GetStatePiece(x1 - 1, y1 + 1) == 2 || GetStatePiece(x1 - 1, y1 + 1) == 4) && GetStatePiece(x1 + 1, y1 - 1) == 0)
            || (x1 + 1 != x2 && y1 - 1 != y2 && GetStatePiece(x1 + 1, y1 - 1) == 4 && GetStatePiece(x1 - 1, y1 + 1) == 0)
            || (x1 - 1 != x2 && y1 - 1 != y2 && GetStatePiece(x1 - 1, y1 - 1) == 4 && GetStatePiece(x1 + 1, y1 + 1) == 0))
        {
            returnvalue = returnvalue + valueKing;
        }
        //safe state(normal piece)
        if (GetStatePiece(x1, y1) == 1)
        {
            if (x2 == 7 || x2 == 0)
                returnvalue = returnvalue + 4;
            else if (y2 < 8)
            {
                if (y2 == 7)
                    returnvalue = returnvalue + valueKing;
                else
                {
                    if (GetStatePiece(x2 + 1, y2 + 1) == 0)
                    {
                        if (GetStatePiece(x2 - 1, y2 - 1) == 4)
                            valueleft = -valueKing;
                        else if (GetStatePiece(x2 - 1, y2 - 1) != -1)
                            valueleft = WeightFuntion(x2, y2, x1, y1);
                    }

                    else if (GetStatePiece(x2 + 1, y2 + 1) != -1)
                    {
                        if (GetStatePiece(x2 + 1, y2 + 1) == 1 || GetStatePiece(x2 + 1, y2 + 1) == 3)
                            valueleft = WeightFuntion(x2, y2, x1, y1);
                        if (GetStatePiece(x2 + 1, y2 + 1) == 2 || GetStatePiece(x2 + 1, y2 + 1) == 4)
                        {
                            if (GetStatePiece(x2 - 1, y2 - 1) == 0 || (x2 - 1 == x1 && y2 - 1 == y1))
                                valueleft = -valueKing;

                            else if (GetStatePiece(x2 - 1, y2 - 1) != -1)
                                valueleft = WeightFuntion(x2, y2, x1, y1);
                        }
                    }


                    if (GetStatePiece(x2 - 1, y2 + 1) == 0)
                    {
                        if (GetStatePiece(x2 + 1, y2 - 1) == 4)
                            valueright = -valueKing;


                        else if (GetStatePiece(x2 + 1, y2 - 1) != -1)
                            valueright = WeightFuntion(x2, y2, x1, y1);
                    }


                    else if (GetStatePiece(x2 - 1, y2 + 1) != -1)
                    {
                        if (GetStatePiece(x2 - 1, y2 + 1) == 1 || GetStatePiece(x2 - 1, y2 + 1) == 3)
                            valueright = WeightFuntion(x2, y2, x1, y1);
                        if (GetStatePiece(x2 - 1, y2 + 1) == 2 || GetStatePiece(x2 - 1, y2 + 1) == 4)
                        {
                            if (GetStatePiece(x2 + 1, y2 - 1) == 0 || (x2 + 1 == x1 && y2 - 1 == y1))
                                valueright = -valueKing;


                            else if (GetStatePiece(x2 + 1, y2 - 1) != -1)
                                valueright = WeightFuntion(x2, y2, x1, y1);
                        }
                    }
                    if (valueleft < valueright)
                    {
                        returnvalue = returnvalue + valueleft;
                    }
                    else
                    {
                        returnvalue = returnvalue + valueright;
                    }
                }
            }
        }
        //safe state definition (king)
        else if (GetStatePiece(x1, y1) == 3)
        {
            if (x2 == 7 || x2 == 0)
                returnvalue = returnvalue + 4;
            else if (y2 < 8)
            {
                if (y2 == 7)
                    returnvalue = returnvalue + valueKing;
                else
                {
                    if (GetStatePiece(x2 + 1, y2 + 1) == 0 || (x2 + 1 == x1 && y2 + 1 == y1))
                    {
                        if (GetStatePiece(x2 - 1, y2 - 1) == 4)
                            valueleft = -valueKing;
                        else if (GetStatePiece(x2 - 1, y2 - 1) != -1)
                            valueleft = WeightFuntion(x2, y2, x1, y1);
                    }


                    else if (GetStatePiece(x2 + 1, y2 + 1) != -1)
                    {
                        if (GetStatePiece(x2 + 1, y2 + 1) == 1 || GetStatePiece(x2 + 1, y2 + 1) == 3)
                            valueleft = WeightFuntion(x2, y2, x1, y1);
                        if (GetStatePiece(x2 + 1, y2 + 1) == 2 || GetStatePiece(x2 + 1, y2 + 1) == 4)
                        {
                            if (GetStatePiece(x2 - 1, y2 - 1) == 0 || (x2 - 1 == x1 && y2 - 1 == y1))
                                valueleft = -valueKing;

                            else if (GetStatePiece(x2 - 1, y2 - 1) != -1)
                                valueleft = WeightFuntion(x2, y2, x1, y1);
                        }
                    }


                    if (GetStatePiece(x2 - 1, y2 + 1) == 0 || (x2 - 1 == x1 && y2 + 1 == y1))
                    {
                        if (GetStatePiece(x2 + 1, y2 - 1) == 4)
                            valueright = -valueKing;


                        else if (GetStatePiece(x2 + 1, y2 - 1) != -1)
                            valueright = WeightFuntion(x2, y2, x1, y1);
                    }


                    else if (GetStatePiece(x2 - 1, y2 + 1) != -1)
                    {
                        if (GetStatePiece(x2 - 1, y2 + 1) == 1 || GetStatePiece(x2 - 1, y2 + 1) == 3)
                            valueright = WeightFuntion(x2, y2, x1, y1);
                        if (GetStatePiece(x2 - 1, y2 + 1) == 2 || GetStatePiece(x2 - 1, y2 + 1) == 4)
                        {
                            if (GetStatePiece(x2 + 1, y2 - 1) == 0 || (x2 + 1 == x1 && y2 - 1 == y1))
                                valueright = -valueKing;


                            else if (GetStatePiece(x2 + 1, y2 - 1) != -1)
                                valueright = WeightFuntion(x2, y2, x1, y1);
                        }
                    }
                    if (valueleft < valueright)
                    {
                        returnvalue = returnvalue + valueleft;
                    }
                    else
                    {
                        returnvalue = returnvalue + valueright;
                    }
                }
            }
        }


        return returnvalue;
    }

    //return index max value of list values.
    private int GetMaxIndex(int[] values)
    {
        int getMaxIndex = 0;
        int temp = values[0];
        for (int i = 1; i < values.Length; i++)
        {
            if (temp < values[i])
                temp = values[i];
        }
        for (int i = 0; i < values.Length; i++)
        {
            if (temp == values[i])
                getMaxIndex = i;
        }
        return getMaxIndex;
    }

}

