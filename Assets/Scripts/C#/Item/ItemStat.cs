using System;
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
    public int currentCount;
    public int maxCount; // 한 공간에 최대로 들어갈 수 있는 갯수

    public ItemStat(string name, string description, Image image, int sizeX, int sizeY, int currentCount = 1, int maxCount = 1)
    {
        this.name = name;
        this.description = description;
        this.image = image;
        this.sizeX = sizeX;
        this.sizeY = sizeY;
        this.currentCount = currentCount;
        this.maxCount = maxCount;
    }

}
