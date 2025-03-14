using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int ID { get; private set;}
    public int BoardX { get; private set; }

    public int BoardY { get; private set; }

    public Vector3 pos { get; private set; }
    public void Setup(int cellX, int cellY) {
        this.BoardX = cellX;
        this.BoardY = cellY;
    }

    public void SetID(int id) {
        this.ID = id;
    }

    public void setPos(Vector3 pos)
    {
        this.pos = pos;
    }
}
