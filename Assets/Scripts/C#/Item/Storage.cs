using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.Core;
using Unity.Services.Economy.Model;
using Unity.Services.Economy;
using UnityEngine;
using System;
using System.Reflection;
using Unity.VisualScripting;

public class Storage : MonoBehaviour
{
    public class StorageItemData
    {
        public bool inInventory;

        public int currentCount;
        public int maxCount;
        public int sizeX;
        public int sizeY;
        public int posX;
        public int posY;
        public bool isRight;
    }

    public int sizeX;
    public int sizeY;

    private StorageUI storageUI;
    private List<PlayersInventoryItem> storageItems = new List<PlayersInventoryItem>();
    private List<PlayersInventoryItem> inventoryItems = new List<PlayersInventoryItem>();

    public event EventHandler<InventoryEventHandlerArgs> OnInventoryChanged;
    private InventoryEventHandlerArgs inventoryEventHandlerArgs;
    public class InventoryEventHandlerArgs
    {
        public List<PlayersInventoryItem> InventoryItems { get; private set; }

        public InventoryEventHandlerArgs(List<PlayersInventoryItem> inventoryItems)
        {
            InventoryItems = inventoryItems;
        }
    }
    public event EventHandler<StorageItemEventHandlerArgs> OnStorageChanged;
    public class StorageItemEventHandlerArgs
    {
        public enum ChangedType
        {
            Added,
            Removed
        }

        public PlayersInventoryItem item;
        public ChangedType changedType { get; private set; }

        public StorageItemEventHandlerArgs(PlayersInventoryItem item, ChangedType changedType)
        {
            this.item = item;
            this.changedType = changedType;
        }
    }

    private async void Awake()
    {
        sizeX = 10; sizeY = 12;

        storageUI = GetComponent<StorageUI>();

        GetInventoryResult inventoryResult = await EconomyService.Instance.PlayerInventory.GetInventoryAsync();

        for (int i = 0; i < inventoryResult.PlayersInventoryItems.Count; i++)
        {
            //await EconomyService.Instance.PlayerInventory.DeletePlayersInventoryItemAsync(inventoryResult.PlayersInventoryItems[i].PlayersInventoryItemId);
            //continue;
            if (inventoryResult.PlayersInventoryItems[i].InstanceData.GetAs<StorageItemData>().inInventory)
                inventoryItems.Add(inventoryResult.PlayersInventoryItems[i]);
            else
                storageItems.Add(inventoryResult.PlayersInventoryItems[i]);
        }

        inventoryEventHandlerArgs = new InventoryEventHandlerArgs(inventoryItems);

        storageUI.Init(storageItems);
        storageUI.DisplayInventoryUI(this, inventoryEventHandlerArgs);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            TestAdd();
        }
    }

    private async void TestAdd()
    {
        var data = EconomyService.Instance.Configuration.GetInventoryItem("CANNED_FOOD").CustomDataDeserializable.GetAs<StorageItemData>();

        AddInventoryItemOptions options = new AddInventoryItemOptions
        {
            InstanceData = data
        };

        var newItem = await EconomyService.Instance.PlayerInventory.AddInventoryItemAsync("CANNED_FOOD", options);
        storageItems.Add(newItem);
        OnStorageChanged?.Invoke(this, new StorageItemEventHandlerArgs(newItem, StorageItemEventHandlerArgs.ChangedType.Added));
    }

    // 창고에 있는 아이템을 인벤토리로 옮기는 함수
    public async void PutItem(PlayersInventoryItem item, int posX, int posY)
    {
        if (CheckEmpty(item, posX, posY))
        {
            var data = item.InstanceData.GetAs<StorageItemData>();
            data.posX = posX;
            data.posY = posY;
            data.inInventory = true;
            
            var newItem = await EconomyService.Instance.PlayerInventory.UpdatePlayersInventoryItemAsync(item.PlayersInventoryItemId, data);
            storageItems.Remove(item);
            inventoryItems.Add(newItem);
            OnInventoryChanged?.Invoke(this, inventoryEventHandlerArgs);
            OnStorageChanged?.Invoke(this , new StorageItemEventHandlerArgs(item, StorageItemEventHandlerArgs.ChangedType.Removed));
        }
    }

    // 인벤토리에 있는 아이템을 창고로 옮기는 함수
    public async void RemoveItem(PlayersInventoryItem item)
    {
        var instanceData = item.InstanceData.GetAs<StorageItemData>();
        instanceData.inInventory = false;

        var newItem = await EconomyService.Instance.PlayerInventory.UpdatePlayersInventoryItemAsync(item.PlayersInventoryItemId, instanceData);
        inventoryItems.Remove(item);
        storageItems.Add(newItem);
        OnInventoryChanged?.Invoke(this, inventoryEventHandlerArgs);
        OnStorageChanged?.Invoke(this, new StorageItemEventHandlerArgs(newItem, StorageItemEventHandlerArgs.ChangedType.Added));
    }

    public async void MoveItem(PlayersInventoryItem item, int x, int y)
    {
        // 아이템의 종류가 같다면 합치기
        if (CheckSameItemType( x, y, item, out PlayersInventoryItem receiveItem))
        {
            TransferItemCount(item, receiveItem);
            return;
        }

        // 해당 공간이 비어있는제 확인
        if (CheckEmpty(item, x, y))
        {
            var data = item.InstanceData.GetAs<StorageItemData>();
            data.posX = x;
            data.posY = y;

            var newItem = await EconomyService.Instance.PlayerInventory.UpdatePlayersInventoryItemAsync(item.PlayersInventoryItemId, data);
            inventoryItems.Remove(item);
            inventoryItems.Add(newItem);
            OnInventoryChanged?.Invoke(this, inventoryEventHandlerArgs);
            return;
        }

        OnInventoryChanged?.Invoke(this, inventoryEventHandlerArgs);
    }

    public async void RotateItem(PlayersInventoryItem item)
    {
        var data = item.InstanceData.GetAs<StorageItemData>();

        if (data.isRight)
        {
            data.isRight = false;
        }
        else
        {
            data.isRight = true;
        }

        var newItem = await EconomyService.Instance.PlayerInventory.UpdatePlayersInventoryItemAsync(item.PlayersInventoryItemId, data);
        inventoryItems.Remove(item);
        inventoryItems.Add(newItem);
        storageUI.selectedInventoryItem = newItem;
        OnInventoryChanged?.Invoke(this, inventoryEventHandlerArgs);
    }

    // 기준점에서 해당 크기의 공간이 비어있는지 확인하는 함수
    private bool CheckEmpty(PlayersInventoryItem playerItem, int posX, int posY)
    {
        var item = playerItem.InstanceData.GetAs<StorageItemData>();

        if (posX < 0 || posY < 0 || posX >= sizeX || posY >= sizeY)
            return false;

        int itemSizeX, itemSizeY;

        if (item.isRight)
        {
            itemSizeX = item.sizeX;
            itemSizeY = item.sizeY;
        }
        else
        {
            itemSizeX = item.sizeY;
            itemSizeY = item.sizeX;
        }

        for (int i = 0; i < inventoryItems.Count; i++)
        {
            var data = inventoryItems[i].InstanceData.GetAs<StorageItemData>();

            if (inventoryItems[i].Equals(playerItem))
                continue;

            if (data.isRight)
            {
                if (posX + itemSizeX > data.posX &&
                    data.posX + data.sizeX > posX &&
                    posY + itemSizeY > data.posY &&
                    data.posY + data.sizeY > posY)
                {
                    return false;
                }
            }
            else
            {
                if (posX + itemSizeX > data.posX &&
                    data.posX + data.sizeY > posX &&
                    posY + itemSizeY > data.posY &&
                    data.posY + data.sizeX > posY)
                {
                    return false;
                }
            }
        }

        return true;
    }

    // 두 아이템이 같은 종류인지 체크하는 함수
    private bool CheckSameItemType(int x, int y, PlayersInventoryItem preItem, out PlayersInventoryItem item)
    {
        item = null;
        if (x < 0 || y < 0 || x >= sizeX || y >= sizeY)
            return false;

        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i] == preItem)
                continue;

            var data = inventoryItems[i].InstanceData.GetAs<StorageItemData>();

            if (inventoryItems[i].InventoryItemId == preItem.InventoryItemId)
            {
                if (data.isRight)
                {
                    if (data.posX <= x && x < data.posX + data.sizeX && data.posY <= y && y < data.posY + data.sizeY)
                    {
                        item = inventoryItems[i];
                        return true;
                    }
                }
                else
                {
                    if (data.posX <= x && x < data.posX + data.sizeY && data.posY <= y && y < data.posY + data.sizeX)
                    {
                        item = inventoryItems[i];
                        return true;
                    }
                }
            }
        }

        return false;
    }

    // 아이템 합치는 함수
    private async void TransferItemCount(PlayersInventoryItem item, PlayersInventoryItem receiveItem)
    {
        var itemData = item.InstanceData.GetAs<StorageItemData>();
        var receiveItemData = receiveItem.InstanceData.GetAs<StorageItemData>();

        int sendingCount = Mathf.Min(itemData.currentCount, receiveItemData.maxCount - receiveItemData.currentCount);

        itemData.currentCount -= sendingCount;
        receiveItemData.currentCount += sendingCount;

        var newReceiveItem = await EconomyService.Instance.PlayerInventory.UpdatePlayersInventoryItemAsync(item.PlayersInventoryItemId, receiveItemData);
        inventoryItems.Remove(receiveItem);
        inventoryItems.Add(newReceiveItem);

        if (itemData.currentCount <= 0)
        {
            inventoryItems.Remove(item);
        }
        else
        {
            var newSendItem = await EconomyService.Instance.PlayerInventory.UpdatePlayersInventoryItemAsync(item.PlayersInventoryItemId, itemData);
            inventoryItems.Remove(item);
            inventoryItems.Add(newSendItem);
        }

        OnInventoryChanged?.Invoke(this, inventoryEventHandlerArgs);
    }

    public PlayersInventoryItem SelectItem(int x, int y)
    {
        if (x < 0 || y < 0 || x >= sizeX || y >= sizeY)
            return null;

        for (int i = 0; i < inventoryItems.Count; i++)
        {
            var data = inventoryItems[i].InstanceData.GetAs<StorageItemData>();

            if (data.isRight)
            {
                if (data.posX <= x && x < data.posX + data.sizeX && data.posY <= y && y < data.posY + data.sizeY)
                    return inventoryItems[i];
            }
            else
            {
                if (data.posX <= x && x < data.posX + data.sizeY && data.posY <= y && y < data.posY + data.sizeX)
                    return inventoryItems[i];
            }
        }

        return null;
    }

    public Dictionary<string, object> ConvertToDictionary(StorageItemData obj)
    {
        Dictionary<string, object> dictionary = new Dictionary<string, object>();

        Type type = obj.GetType();
        PropertyInfo[] properties = type.GetProperties();

        foreach (PropertyInfo property in properties)
        {
            string propertyName = property.Name;
            object propertyValue = property.GetValue(obj);

            dictionary.Add(propertyName, propertyValue);
        }

        return dictionary;
    }
}
