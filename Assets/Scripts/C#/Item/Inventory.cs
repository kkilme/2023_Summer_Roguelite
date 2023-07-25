using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

public enum ROTATION_TYPE
{
    TOP,
    RIGHT
}

public class Inventory : NetworkBehaviour
{
    private int sizeX, sizeY; // 인벤토리창 크기
    private Item[,] inventorySpace; // 인벤토리 공간을 Item 타입의 이차원 배열로 저장해 해당 좌표의 공간이 비어있는지 채워져 있는지 판단
    private List<Item> items = new List<Item>(); // 현재 갖고있는 아이템들
    private Dictionary<Item, ROTATION_TYPE> itemRotationDic = new Dictionary<Item, ROTATION_TYPE>(); // 해당 아이템의 회전 정보를 저장. 기본값은 RIGHT

    public event EventHandler<InventoryEventHandlerArgs> OnInventoryChanged;
    public class InventoryEventHandlerArgs
    {
        public List<Item> Items { get; private set; }
        public Dictionary<Item, ROTATION_TYPE> ItemRotationDic { get; private set; }

        public InventoryEventHandlerArgs(List<Item> items)
        {
            Items = items;
        }
    }
    private InventoryEventHandlerArgs inventoryEventHandlerArgs;

    public Inventory(int sizeX, int sizeY)
    {
        this.sizeX = sizeX;
        this.sizeY = sizeY;
        inventorySpace = new Item[sizeX, sizeY];
        inventoryEventHandlerArgs = new InventoryEventHandlerArgs(items);
    }

    // 인벤토리안에 아이템을 넣는 함수. 매개변수인 x,y가 기준점으로 좌하단에 위치함
    [ServerRpc]
    public void PutItemServerRPC(ITEMNAME itemName, int x, int y, ROTATION_TYPE rotationType = ROTATION_TYPE.RIGHT, ServerRpcParams serverRpcParams = default)
    {
        // 추후에 Stat database에서 stat만 받아올 수 있도록 구조 변경. (GC 낭비)
        var item = Item.GetItem(itemName);
        var itemStat = item.GetItemStat();

        if (x + itemStat.sizeX < sizeX && y + itemStat.sizeY < sizeY)
        {
            if (CheckEmpty(x,y, itemStat.sizeX, itemStat.sizeY))
            {
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { serverRpcParams.Receive.SenderClientId }
                    }
                };
                PutItemClientRPC(itemName, x, y, rotationType, clientRpcParams);
                if (IsServer)
                {
                    items.Add(item);
                    itemRotationDic.Add(item, rotationType);
                }
            }
        }
    }

    // 인벤토리안에 아이템을 자동으로 넣어주는 함수.
    [ServerRpc]
    public void PutItemServerRPC(ITEMNAME itemName, ROTATION_TYPE rotationType = ROTATION_TYPE.RIGHT, ServerRpcParams serverRpcParams = default)
    {
        var item = Item.GetItem(itemName);
        int x, y;
        var itemStat = item.GetItemStat();
        if (CheckEmpty(itemStat.sizeX, itemStat.sizeY, out x, out y))
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { serverRpcParams.Receive.SenderClientId }
                }
            };
            PutItemClientRPC(itemName, x, y, rotationType, clientRpcParams);
            if (IsServer)
            {
                items.Add(item);
                itemRotationDic.Add(item, rotationType);
            }
        }
    }

    [ClientRpc]
    public void PutItemClientRPC(ITEMNAME itemName, int x, int y, ROTATION_TYPE rotationType = ROTATION_TYPE.RIGHT, ClientRpcParams clientRpcParams = default)
    {
        var item = Item.GetItem(itemName);
        var itemStat = item.GetItemStat();
        // rotationType이 Top일경우 x와 y를 스왑
        if (rotationType.Equals(ROTATION_TYPE.TOP))
            (itemStat.sizeX, itemStat.sizeY) = (itemStat.sizeY, itemStat.sizeX);

        for (int i = 0; i < itemStat.sizeY; i++)
            for (int j = 0; j < itemStat.sizeX; j++)
                inventorySpace[x + j, y + j] = item;

        items.Add(item);
        itemRotationDic.Add(item, rotationType);
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

        if (IsServer)
        {
            var item = inventorySpace[itemPosX, itemPosY];
            var itemStat = item.GetItemStat();
            items.Remove(item);
            itemRotationDic.Remove(item);
        }
    }

    [ClientRpc]
    public void RemoveItemClientRPC(int itemPosX, int itemPosY, ClientRpcParams clientRpcParams = default)
    {
        var item = inventorySpace[itemPosX, itemPosY];
        var itemStat = item.GetItemStat();

        for (int i = 0; i < itemStat.sizeY; i++)
            for (int j = 0; j < itemStat.sizeX; j++)
                inventorySpace[itemPosX + j, itemPosY + i] = null;

        items.Remove(item);
        itemRotationDic.Remove(item);
        OnInventoryChanged?.Invoke(this, inventoryEventHandlerArgs);
    }

    // 인벤토리에 존재하는 아이템의 위치를 바꾸는 함수
    public void MoveItem(Item item, int x, int y)
    {
        var rotationType = itemRotationDic[item];
        int itemPosx, itemPosY;
        ITEMNAME itemName = item.ItemName;
        GetItemPos(item, out itemPosx, out itemPosY);
        RemoveItemServerRPC(itemPosx, itemPosY);
        PutItemServerRPC(itemName, x, y, rotationType);
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

    // 인벤토리에 존재하는 아이템의 기준점을 반환하는 함수
    private void GetItemPos(Item item, out int x, out int y) 
    {
        for (int i = 0; i < sizeY; i++)
            for (int j = 0; j < sizeX; j++)
                if (inventorySpace[j, i].Equals(item))
                {
                    x = j; y = i; 
                    return;
                }

        x = -1; y = -1;
    }

    // 기준점에서 해당 크기의 공간이 비어있는지 확인하는 함수
    private bool CheckEmpty(int x, int y, int sizeX, int sizeY)
    {
        for (int i = 0; i < sizeY; i++)
            for (int j = 0; j < sizeX; j++)
                if (inventorySpace[x + j, y + i] != null)
                    return false;

        return true;
    }

    // 해당 크기의 공간이 존재하는지 확인하는 함수. 해당 공간의 기준점도 반환
    private bool CheckEmpty(int sizeX, int sizeY, out int x, out int y)
    {
        for (int i = 0; i < sizeY; i++)
            for (int j = 0; j < sizeX; j++)
            {
                for (int k = 0; k < sizeY; k++)
                    for (int l = 0; l < sizeX; l++)
                        if (inventorySpace[j + l, i + k] != null)
                            goto FAILED;

                x = j; y = i;
                return true;
                FAILED:;
            }

        x = -1; y = -1;
        return false;
    }
}
