using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public bool isWhite;
    public bool isKing;

    public int Row { get; set; }   //piece's row number.
    public int Column { get; set; }   //piece's column number.


    //sets piece's row and column number.
    public Piece(int row, int col)
    {
        this.Row = row;
        this.Column = col;
    }

    public bool IsForcedToMove(Piece[,] board, int x, int y)
    {
        if (!isWhite || isKing)
        {
            //capture top left
            if (x >= 2 && y <= 5)
            {
                Piece p = board[x - 1, y + 1];
                if (p != null && p.isWhite != isWhite) // if theres a piece and its not the same colour
                {
                    if (board[x - 2, y + 2] == null)// can it land when it jumps
                    {
                        return true;
                    }
                }
            }

            //capture top right
            if (x <= 5 && y <= 5)
            {
                Piece p = board[x + 1, y + 1];
                if (p != null && p.isWhite != isWhite)
                {
                    if (board[x + 2, y + 2] == null)// can it land when it jumps
                    {
                        return true;
                    }
                }
            }
        }
        if (isWhite || isKing)
        {
            //capture bottom left
            if (x >= 2 && y >= 2)
            {
                Piece p = board[x - 1, y - 1];
                if (p != null && p.isWhite != isWhite)
                {
                    if (board[x - 2, y - 2] == null)// can it land when it jumps
                    {
                        return true;
                    }
                }
            }

            //capture bottom right
            if (x <= 5 && y >= 2)
            {
                Piece p = board[x + 1, y - 1];
                if (p != null && p.isWhite != isWhite)
                {
                    if (board[x + 2, y - 2] == null)// can it land when it jumps
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public bool ValidMove(Piece[,] board, int x1, int y1, int x2, int y2)
    {
        //move onto another piece
        if (board[x2, y2] != null)
        {
            return false;
        }

        int deltaMove = Mathf.Abs(x1 - x2);
        int deltaMoveY = y2 - y1;

        if (!isWhite || isKing)
        {
            //standard move
            if (deltaMove == 1)// moving 1 in x
            {
                if (deltaMoveY == 1)// moving 1 in y
                {
                    return true;
                }
            }

            //capture move
            else if (deltaMove == 2)
            {
                if (deltaMoveY == 2)
                {
                    Piece p = board[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (p != null && p.isWhite != isWhite)
                    {
                        return true;
                    }
                }
            }
        }

        if (isWhite || isKing)
        {
            //standard move
            if (deltaMove == 1)
            {
                if (deltaMoveY == -1)
                {
                    return true;
                }
            }

            //capture move
            else if (deltaMove == 2)
            {
                if (deltaMoveY == -2)
                {
                    Piece p = board[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (p != null && p.isWhite != isWhite)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}