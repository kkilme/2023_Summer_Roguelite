using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct ItemStat
{
    public string name;
    public string description;
    public Image image;

    public ItemStat(string name, string description, Image image)
    {
        this.name = name;
        this.description = description;
        this.image = image;
    }
}
