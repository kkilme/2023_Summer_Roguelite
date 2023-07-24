using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    private void Start()
    {
        FindObjectOfType<Player>().Inventory.OnInventoryChanged += DisplayInventoryUI;
    }

    private void DisplayInventoryUI(object sender, Inventory.InventoryEventHandlerArgs e)
    {
        
    }
}
