using System.Collections;
using System.Collections.Generic;
using Unity.Services.Economy.Model;
using Unity.Services.Economy;
using UnityEngine;
using System;

public class Store : MonoBehaviour
{
    private StoreUI storeUI;
    private List<PlayersInventoryItem> storageItems = new List<PlayersInventoryItem>();

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
        storeUI = GetComponent<StoreUI>();

        GetInventoryResult inventoryResult = await EconomyService.Instance.PlayerInventory.GetInventoryAsync();

        for (int i = 0; i < inventoryResult.PlayersInventoryItems.Count; i++)
        {
            //await EconomyService.Instance.PlayerInventory.DeletePlayersInventoryItemAsync(inventoryResult.PlayersInventoryItems[i].PlayersInventoryItemId);
            //continue;
            if (!inventoryResult.PlayersInventoryItems[i].InstanceData.GetAs<Storage.StorageItemData>().inInventory)
                storageItems.Add(inventoryResult.PlayersInventoryItems[i]);
        }

        storeUI.Init(storageItems);
    }

    // 아이템의 현재 갯수가 최대 갯수일 때만 팔 수 있음
    public async void SellItem(PlayersInventoryItem item)
    {
        var itemData = item.InstanceData.GetAs<Storage.StorageItemData>();

        if (itemData.currentCount == itemData.maxCount)
        {
            MakeVirtualPurchaseOptions options = new MakeVirtualPurchaseOptions()
            {
                PlayersInventoryItemIds = new List<string>() { item.PlayersInventoryItemId }
            };
            MakeVirtualPurchaseResult result = await EconomyService.Instance.Purchases.MakeVirtualPurchaseAsync($"SELL_{item.InventoryItemId}", options);

            OnStorageChanged?.Invoke(this, new StorageItemEventHandlerArgs(item, StorageItemEventHandlerArgs.ChangedType.Removed));
        }

    }

    public async void BuyItem(string itemId)
    {
        MakeVirtualPurchaseResult result = await EconomyService.Instance.Purchases.MakeVirtualPurchaseAsync($"BUY_{itemId}");
        var data = EconomyService.Instance.Configuration.GetInventoryItem(itemId).CustomDataDeserializable.GetAs<Storage.StorageItemData>();
        data.currentCount = data.maxCount;
        var newItem = await EconomyService.Instance.PlayerInventory.UpdatePlayersInventoryItemAsync(result.Rewards.Inventory[0].PlayersInventoryItemIds[0], data);

        OnStorageChanged?.Invoke(this, new StorageItemEventHandlerArgs(newItem, StorageItemEventHandlerArgs.ChangedType.Added));
    }
}
