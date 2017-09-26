using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//move state.

public class Move : MonoBehaviour
{

    public Piece piece1 { get; set; }
    public Piece piece2 { get; set; }

    //set initial state.
    public Move()
    {
        this.piece1 = null;
        this.piece2 = null;
    }

    //set original state.
    public Move(Piece piece1, Piece piece2)
    {
        this.piece1 = piece1;
        this.piece2 = piece2;
    }

}

