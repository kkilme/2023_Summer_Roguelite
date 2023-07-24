using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct ItemStat
{
    public string name;
    public string description;
    public Image image;
    public int sizeX, sizeY;

    public ItemStat(string name, string description, Image image, int sizeX, int sizeY)
    {
        this.name = name;
        this.description = description;
        this.image = image;
        this.sizeX = sizeX;
        this.sizeY = sizeY;
    }
}
