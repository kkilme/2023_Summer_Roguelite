using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum ROTATION_TYPE
{
    TOP,
    RIGHT
}

public class Inventory : NetworkBehaviour
{
    private int sizeX, sizeY; // 인벤토리창 크기
    public Item[,] InventorySpace { get; private set; } // 인벤토리 공간을 Item 타입의 이차원 배열로 저장해 해당 좌표의 공간이 비어있는지 채워져 있는지 판단

    private List<Item> items = new List<Item>(); // 현재 갖고있는 아이템들
    private Dictionary<Item, ROTATION_TYPE> itemRotationDic = new Dictionary<Item, ROTATION_TYPE>(); // 해당 아이템의 회전 정보를 저장. 기본값은 RIGHT
    private Dictionary<Item, Vector2Int> itemPositionDic = new Dictionary<Item, Vector2Int>(); // 해당 아이템의 기준점을 저장

    public event EventHandler<InventoryEventHandlerArgs> OnInventoryChanged;
    public class InventoryEventHandlerArgs
    {
        public List<Item> Items { get; private set; }
        public Item[,] InventorySpace { get; private set; }
        public Dictionary<Item, Vector2Int> ItemPositionDic { get; private set; }
        public Dictionary<Item, ROTATION_TYPE> ItemRotationDic { get; private set; }

        public InventoryEventHandlerArgs(List<Item> items, Item[,] inventorySpace, Dictionary<Item, Vector2Int> itemPositionDic, Dictionary<Item, ROTATION_TYPE> itemRotationDic)
        {
            Items = items;
            InventorySpace = inventorySpace;
            ItemPositionDic = itemPositionDic;
            ItemRotationDic = itemRotationDic;
        }
    }
    private InventoryEventHandlerArgs inventoryEventHandlerArgs;

    public override void OnNetworkSpawn()
    {
        this.sizeX = 10;
        this.sizeY = 12;
        InventorySpace = new Item[sizeX, sizeY];
        inventoryEventHandlerArgs = new InventoryEventHandlerArgs(items, InventorySpace, itemPositionDic, itemRotationDic);
    }

    // 인벤토리안에 아이템을 넣는 함수. 매개변수인 x,y가 기준점으로 좌하단에 위치함
    [ServerRpc]
    public void PutItemServerRPC(ITEMNAME itemName, int x, int y, ROTATION_TYPE rotationType = ROTATION_TYPE.RIGHT, int itemCount = 1, ServerRpcParams serverRpcParams = default)
    {
        // 추후에 Stat database에서 stat만 받아올 수 있도록 구조 변경. (GC 낭비)
        var item = Item.GetItem(itemName, itemCount);
        var itemStat = item.ItemStat;

        if (x + itemStat.sizeX < sizeX && y + itemStat.sizeY < sizeY)
        {
            if (CheckEmpty(x,y, itemStat.sizeX, itemStat.sizeY, rotationType))
            {
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { serverRpcParams.Receive.SenderClientId }
                    }
                };
                PutItemClientRPC(itemName, x, y, rotationType, itemCount, clientRpcParams);
                if (IsServer && !IsHost)
                {
                    items.Add(item);
                    itemRotationDic.Add(item, rotationType);
                    itemPositionDic.Add(item, new Vector2Int(x, y));
                }
            }
        }
    }

    // 인벤토리안에 아이템을 자동으로 넣어주는 함수.
    [ServerRpc]
    public void PutItemServerRPC(ITEMNAME itemName, ROTATION_TYPE rotationType = ROTATION_TYPE.RIGHT, int itemCount = 1, ServerRpcParams serverRpcParams = default)
    {
        var item = Item.GetItem(itemName, itemCount);
        int x, y;
        var itemStat = item.ItemStat;

        if (CheckEmpty(itemStat.sizeX, itemStat.sizeY, out x, out y, rotationType))
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { serverRpcParams.Receive.SenderClientId }
                }
            };
            PutItemClientRPC(itemName, x, y, rotationType, itemCount, clientRpcParams);
            if (IsServer && !IsHost)
            {
                items.Add(item);
                itemRotationDic.Add(item, rotationType);
                itemPositionDic.Add(item, new Vector2Int(x, y));
            }
        }
    }

    [ClientRpc]
    public void PutItemClientRPC(ITEMNAME itemName, int x, int y, ROTATION_TYPE rotationType, int itemCount, ClientRpcParams clientRpcParams = default)
    {
        var item = Item.GetItem(itemName, itemCount);
        var itemStat = item.ItemStat;
        // rotationType이 Top일경우 x와 y를 스왑
        if (rotationType.Equals(ROTATION_TYPE.TOP))
            (itemStat.sizeX, itemStat.sizeY) = (itemStat.sizeY, itemStat.sizeX);

        for (int i = 0; i < itemStat.sizeY; i++)
            for (int j = 0; j < itemStat.sizeX; j++)
                InventorySpace[x + j, y + i] = item;

        items.Add(item);
        itemRotationDic.Add(item, rotationType);
        itemPositionDic.Add(item, new Vector2Int(x, y));
        OnInventoryChanged?.Invoke(this, inventoryEventHandlerArgs);
    }

    // 인벤토리에있는 아이템을 제거함
    [ServerRpc]
    public void RemoveItemServerRPC(int itemPosX, int itemPosY, ServerRpcParams serverRpcParams = default)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { serverRpcParams.Receive.SenderClientId }
            }
        };

        RemoveItemClientRPC(itemPosX, itemPosY, clientRpcParams);

        if (IsServer && !IsHost)
        {
            var item = InventorySpace[itemPosX, itemPosY];
            var itemStat = item.ItemStat;
            items.Remove(item);
            itemRotationDic.Remove(item);
            itemPositionDic.Remove(item);
        }
    }

    [ClientRpc]
    public void RemoveItemClientRPC(int itemPosX, int itemPosY, ClientRpcParams clientRpcParams = default)
    {
        var item = InventorySpace[itemPosX, itemPosY];
        var itemStat = item.ItemStat;

        for (int i = 0; i < sizeY; i++)
            for (int j = 0; j < sizeX; j++)
                if (InventorySpace[j, i] == item)
                    InventorySpace[j, i] = null;

        items.Remove(item);
        itemRotationDic.Remove(item);
        itemPositionDic.Remove(item);

        OnInventoryChanged?.Invoke(this, inventoryEventHandlerArgs);
    }

    // 인벤토리에 존재하는 아이템의 위치를 바꾸는 함수
    public void MoveItem(Item item, int x, int y)
    {
        if (CheckSameItemType(x, y, item.ItemName))
        {
            TransferItemCountServerRPC(itemPositionDic[item], x, y);
            return;
        }

        if (!CheckEmpty(x, y, item.ItemStat.sizeX, item.ItemStat.sizeY, itemRotationDic[item], item))
        {
            OnInventoryChanged?.Invoke(this, inventoryEventHandlerArgs);
            return;
        }

        var rotationType = itemRotationDic[item];
        ITEMNAME itemName = item.ItemName;
        int itemCount = item.ItemStat.currentCount;
        RemoveItemServerRPC(itemPositionDic[item].x, itemPositionDic[item].y);
        PutItemServerRPC(itemName, x, y, rotationType, itemCount);
    }

    [ServerRpc]
    private void TransferItemCountServerRPC(Vector2Int sendingPos, int receivedX, int receivedY, ServerRpcParams serverRpcParams = default)
    {
        if (receivedX < 0 || receivedY < 0 || receivedX >= sizeX || receivedY >= sizeY)
            return;

        var sendingItem = InventorySpace[sendingPos.x, sendingPos.y];
        var receivedItem = InventorySpace[receivedX, receivedY];

        int sendingCount = Mathf.Min(sendingItem.ItemStat.currentCount, receivedItem.ItemStat.maxCount - receivedItem.ItemStat.currentCount);

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { serverRpcParams.Receive.SenderClientId }
            }
        };

        TransferItemCountClientRPC(sendingCount, sendingPos, receivedX, receivedY, clientRpcParams);

        if (IsServer && !IsHost)
        {
            sendingItem.AddCount(-sendingCount);
            receivedItem.AddCount(sendingCount);
        }
    }

    [ClientRpc]
    private void TransferItemCountClientRPC(int sendingCount, Vector2Int sendingPos, int receivedX, int receivedY, ClientRpcParams clientRpcParams = default)
    {
        var sendingItem = InventorySpace[sendingPos.x, sendingPos.y];
        var receivedItem = InventorySpace[receivedX, receivedY];

        sendingItem.AddCount(-sendingCount);
        receivedItem.AddCount(sendingCount);

        if (sendingItem.ItemStat.currentCount == 0)
        {
            RemoveItemServerRPC(itemPositionDic[sendingItem].x, itemPositionDic[sendingItem].y);
        }

        OnInventoryChanged?.Invoke(this, inventoryEventHandlerArgs);
    }

    // 아이템을 회전시키는 함수
    public void RotateItem(Item item)
    {
        if (itemRotationDic[item].Equals(ROTATION_TYPE.RIGHT))
            itemRotationDic[item] = ROTATION_TYPE.TOP;
        else
            itemRotationDic[item] = ROTATION_TYPE.RIGHT;

        OnInventoryChanged?.Invoke(this, inventoryEventHandlerArgs);
    }

    // 기준점에서 해당 크기의 공간이 비어있는지 확인하는 함수
    private bool CheckEmpty(int x, int y, int itemSizeX, int itemSizeY, ROTATION_TYPE rotationType, Item item = null)
    {
        if (x < 0 || y < 0 || x >= sizeX || y >= sizeY)
            return false;

        if (rotationType.Equals(ROTATION_TYPE.TOP))
            (itemSizeX, itemSizeY) = (itemSizeY, itemSizeX);

        for (int i = 0; i < itemSizeY; i++)
            for (int j = 0; j < itemSizeX; j++)
            {
                if (x + j < sizeX && y + i < sizeY)
                {
                    if (InventorySpace[x + j, y + i] != null)
                    {
                        if (item != null)
                        {
                            if (InventorySpace[x + j, y + i] != item)
                                return false;
                        }
                        else
                            return false;
                    }
                }
                else
                    return false;
            }

        return true;
    }

    // 해당 크기의 공간이 존재하는지 확인하는 함수. 해당 공간의 기준점도 반환
    private bool CheckEmpty(int itemSizeX, int itemSizeY, out int x, out int y, ROTATION_TYPE rotationType)
    {
        if (rotationType.Equals(ROTATION_TYPE.TOP))
            (itemSizeX, itemSizeY) = (itemSizeY, itemSizeX);

        for (int i = 0; i < sizeY; i++)
            for (int j = 0; j < sizeX; j++)
            {
                for (int k = 0; k < itemSizeY; k++)
                    for (int l = 0; l < itemSizeX; l++)
                    {
                        if (j + l < sizeX && i + k < sizeY)
                        { 
                            if (InventorySpace[j + l, i + k] != null)
                                goto FAILED;
                        }
                        else
                            goto FAILED;
                    }

                x = j; y = i;
                return true;
                FAILED:;
            }

        x = -1; y = -1;
        return false;
    }

    private bool CheckSameItemType(int x, int y, ITEMNAME itemName)
    {
        if (InventorySpace[x, y] == null) 
            return false;

        return InventorySpace[x, y].ItemName == itemName;
    }

}
