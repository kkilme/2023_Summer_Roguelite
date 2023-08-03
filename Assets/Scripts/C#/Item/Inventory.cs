using Mono.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Unity.Netcode;
using UnityEngine;

public enum ROTATION_TYPE
{
    TOP,
    RIGHT
}

public class Inventory : NetworkBehaviour
{
    public NetworkVariable<int> sizeX = new NetworkVariable<int>();
    public NetworkVariable<int> sizeY = new NetworkVariable<int>();
    private NetworkList<InventoryItem> items;
    private List<GettableItem> nearItems = new List<GettableItem>();

    public event EventHandler<InventoryEventHandlerArgs> OnInventoryChanged;
    public class InventoryEventHandlerArgs
    {
        public NetworkList<InventoryItem> InventoryItems { get; private set; }

        public InventoryEventHandlerArgs(NetworkList<InventoryItem> inventoryItems)
        {
            InventoryItems = inventoryItems;
        }
    }
    public event EventHandler<NearItemEventHandlerArgs> OnNearItemChanged;
    public class NearItemEventHandlerArgs
    {
        public enum ChangedType
        {
            Added,
            Removed
        }

        public GettableItem GettableItem { get; private set; }
        public ChangedType changedType { get; private set; }
        
        public NearItemEventHandlerArgs(GettableItem gettableItem, ChangedType changedType)
        {
            GettableItem = gettableItem;
            this.changedType = changedType;
        }
    }
    private InventoryEventHandlerArgs inventoryEventHandlerArgs;

    public void OnItemChanged(NetworkListEvent<InventoryItem> changeEvent)
    {
        OnInventoryChanged?.Invoke(this, inventoryEventHandlerArgs);
    }

    private GameObject inventoryUI;

    private void Awake()
    {
        items = new NetworkList<InventoryItem>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            sizeX.Value = 10;
            sizeY.Value = 12;
        }

        if (IsOwner)
        {
            inventoryEventHandlerArgs = new InventoryEventHandlerArgs(items);
            inventoryUI = FindObjectOfType<InventoryUI>(true).gameObject;
            items.OnListChanged += OnItemChanged;
        }
    }

    // 인벤토리안에 아이템을 넣는 함수. 매개변수인 x,y가 기준점으로 좌하단에 위치함
    [ServerRpc]
    public void PutItemServerRPC(NetworkObjectReference item, int posX, int posY, ServerRpcParams serverRpcParams = default)
    {
        NetworkObject getItem = item;
        var t = getItem.GetComponent<GettableItem>();
        InventoryItem inventoryItem = Item.GetInventoryItem(t.ItemName, t.ItemCount);
        inventoryItem.posX = posX;
        inventoryItem.posY = posY;

        if (CheckEmpty(inventoryItem))
        {
            items.Add(inventoryItem);
            getItem.Despawn();
        }
    }

    // 인벤토리안에 아이템을 자동으로 넣어주는 함수.
    //[ServerRpc]
    //public void PutItemServerRPC(ITEMNAME itemName, ROTATION_TYPE rotationType = ROTATION_TYPE.RIGHT, int itemCount = 1, ServerRpcParams serverRpcParams = default)
    //{
    //    var item = Item.GetItem(itemName, itemCount);
    //    int x, y;
    //    var itemStat = item.ItemStat;

    //    if (CheckEmpty(itemStat.sizeX, itemStat.sizeY, out x, out y, rotationType))
    //    {
    //        ClientRpcParams clientRpcParams = new ClientRpcParams
    //        {
    //            Send = new ClientRpcSendParams
    //            {
    //                TargetClientIds = new ulong[] { serverRpcParams.Receive.SenderClientId }
    //            }
    //        };
    //        PutItemClientRPC(itemName, x, y, rotationType, itemCount, clientRpcParams);
    //        if (IsServer && !IsHost)
    //        {
    //            items.Add(item);
    //            itemRotationDic.Add(item, rotationType);
    //            itemPositionDic.Add(item, new Vector2Int(x, y));
    //        }
    //    }
    //}

    // 인벤토리에있는 아이템을 제거함
    [ServerRpc]
    public void RemoveItemServerRPC(InventoryItem item, ServerRpcParams serverRpcParams = default)
    {
        items.Remove(item);
    }

    // 인벤토리에 존재하는 아이템의 위치를 바꾸는 함수
    [ServerRpc]
    public void MoveItemServerRPC(InventoryItem item, int x, int y, ServerRpcParams serverRpcParams = default)
    {
        // 아이템의 종류가 같다면 합치기
        InventoryItem receiveItem;
        if (CheckSameItemType(x, y, item.itemName, out receiveItem))
        {
            TransferItemCount(item, receiveItem, serverRpcParams);
            return;
        }

        // 해당 공간이 비어있는제 확인
        if (!CheckEmpty(item))
        {
            return;
        }

        item.posX = x; item.posY = y;
        items[FindIndex(item)] = item;
    }

    private void TransferItemCount(InventoryItem item, InventoryItem receiveItem, ServerRpcParams serverRpcParams)
    {
        if (!IsServer)
        {
            return;
        }

        int sendingCount = Mathf.Min(item.currentCount, receiveItem.maxCount - receiveItem.currentCount);

        item.currentCount -= sendingCount;
        receiveItem.currentCount += sendingCount;

        items[FindIndex(receiveItem)] = receiveItem;

        if (item.currentCount <= 0)
            items.Remove(item);
        else
            items[FindIndex(item)] = item;
    }

    // 아이템을 회전시키는 함수
    [ServerRpc]
    public void RotateItemServerRPC(InventoryItem item, ServerRpcParams serverRpcParams = default)
    {
        if (item.rotationType.Equals(ROTATION_TYPE.RIGHT))
            item.rotationType = ROTATION_TYPE.TOP;
        else
            item.rotationType = ROTATION_TYPE.RIGHT;

        items[FindIndex(item)] = item;
    }

    // 기준점에서 해당 크기의 공간이 비어있는지 확인하는 함수
    public bool CheckEmpty(InventoryItem item)
    {
        if (!IsServer)
            return false;

        if (item.posX < 0 || item.posY < 0 || item.posX >= sizeX.Value || item.posY >= sizeY.Value)
            return false;

        int itemSizeX, itemSizeY;

        if (item.rotationType == ROTATION_TYPE.RIGHT)
        {
            itemSizeX = item.posX;
            itemSizeY = item.posY;
        }
        else
        {
            itemSizeX = item.posY;
            itemSizeY = item.posX;
        }

        bool isX, isY;

        for (int i = 0; i < items.Count; i++)
        {
            isX = false;
            isY = false;

            // 좌하단 검사
            if (items[i].rotationType == ROTATION_TYPE.RIGHT)
            {
                if (items[i].posX >= item.posX && item.posX < items[i].posX + items[i].sizeX)
                    isX = true;
                else if (items[i].posY >= item.posY && item.posY < items[i].posY + items[i].sizeY)
                    isY = true;
            }
            // 우상단 검사
            else
            {
                if (items[i].posX >= item.posX + itemSizeX && item.posX + itemSizeX < items[i].posX + items[i].sizeY)
                    isX = true;
                else if (items[i].posY >= item.posY + itemSizeY && item.posY + itemSizeY < items[i].posY + items[i].sizeX)
                    isY = true;
            }

            if (isX && isY)
            {
                return false;
            }
        }

        return true;
    }

    // 해당 크기의 공간이 존재하는지 확인하는 함수. 해당 공간의 기준점도 반환
    //private bool CheckEmpty(int itemSizeX, int itemSizeY, out int x, out int y, ROTATION_TYPE rotationType)
    //{
    //    if (rotationType.Equals(ROTATION_TYPE.TOP))
    //        (itemSizeX, itemSizeY) = (itemSizeY, itemSizeX);

    //    for (int i = 0; i < sizeY; i++)
    //        for (int j = 0; j < sizeX; j++)
    //        {
    //            for (int k = 0; k < itemSizeY; k++)
    //                for (int l = 0; l < itemSizeX; l++)
    //                {
    //                    if (j + l < sizeX && i + k < sizeY)
    //                    { 
    //                        if (InventorySpace[j + l, i + k] != null)
    //                            goto FAILED;
    //                    }
    //                    else
    //                        goto FAILED;
    //                }

    //            x = j; y = i;
    //            return true;
    //            FAILED:;
    //        }

    //    x = -1; y = -1;
    //    return false;
    //}

    private bool CheckSameItemType(int x, int y, ITEMNAME itemName, out InventoryItem item)
    {
        item = new InventoryItem();

        if (x < 0 || y < 0 || x >= sizeX.Value || y >= sizeY.Value)
            return false;

        for (int i = 0; i < items.Count; i++)
            if (items[i].itemName == itemName)
            {
                if (items[i].rotationType == ROTATION_TYPE.RIGHT)
                {
                    if (items[i].posX >= x && x < items[i].posX + items[i].sizeX && items[i].posY >= y && y < items[i].posY + items[i].sizeY)
                    {
                        item = items[i];
                        return true;
                    }
                }
                else
                {
                    if (items[i].posX >= x && x < items[i].posX + items[i].sizeY && items[i].posY >= y && y < items[i].posY + items[i].sizeX)
                    {
                        item = items[i];
                        return true;
                    }
                }
            }

        return false;
    }

    public InventoryItem SelectItem(int x, int y)
    {
        if (x < 0 || y < 0 || x >= sizeX.Value || y >= sizeY.Value)
            return new InventoryItem();

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].rotationType == ROTATION_TYPE.RIGHT)
                if (items[i].posX >= x && x < items[i].posX + items[i].sizeX && items[i].posY >= y && y < items[i].posY + items[i].sizeY)                
                    return items[i];
            else
                if (items[i].posX >= x && x < items[i].posX + items[i].sizeY && items[i].posY >= y && y < items[i].posY + items[i].sizeX)
                    return items[i];
        }

        return new InventoryItem();
    }

    public bool SwitchInventoryPanel()
    {
        bool state = inventoryUI.activeSelf;
        inventoryUI.SetActive(!state);
        return !state;
    }

    public void AddNearItem(GettableItem item)
    {
        nearItems.Add(item);
        OnNearItemChanged?.Invoke(this, new NearItemEventHandlerArgs(item, NearItemEventHandlerArgs.ChangedType.Added));
    }

    public void RemoveNearItem(GettableItem item)
    {
        nearItems.Remove(item);
        OnNearItemChanged?.Invoke(this, new NearItemEventHandlerArgs(item, NearItemEventHandlerArgs.ChangedType.Removed));
    }

    public System.Collections.ObjectModel.ReadOnlyCollection<GettableItem> GetNearItems()
    {
        return nearItems.AsReadOnly();
    }

    [ServerRpc]
    public void DropItemServerRPC(InventoryItem item, ServerRpcParams serverRpcParams = default)
    {
        var networkObj = Instantiate(GettableItem.GetItemPrefab(item.itemName), transform.position + transform.forward, Quaternion.identity).GetComponent<NetworkObject>();
        networkObj.Spawn();
        RemoveItemServerRPC(item , serverRpcParams);
    }

    public bool hasItem(ITEMNAME itemName)
    {
        for (int i = 0; i < items.Count; i++)
            if (items[i].itemName == itemName)
                return true;

        return false;
    }

    private int FindIndex(InventoryItem item)
    {
        for (int i = 0; i < items.Count; i++)
            if (items[i].Equals(item))
                return i;

        return -1;
    }
}
