using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Services.Economy;
using Unity.Services.Economy.Model;
using UnityEngine;
using UnityEngine.UI;

public class StoreUI : MonoBehaviour
{
    private Store store;
    private ScrollRect scrollRect;
    private ItemUI selectedStorageItem;
    private Dictionary<PlayersInventoryItem, ItemUI> storageDic = new Dictionary<PlayersInventoryItem, ItemUI>();
    private Stack<ItemUI> storageItemUIStack;
    [SerializeField] private TextMeshProUGUI scrapText;

    private void Awake()
    {
        store = GetComponent<Store>();
        storageItemUIStack = new Stack<ItemUI>(transform.GetChild(1).GetComponentsInChildren<ItemUI>(true));
        storageItemUIStack.ToList().ForEach(x => x.action += SelectStorageItem);
        scrollRect = GetComponentInChildren<ScrollRect>(true);
    }

    public void Init(List<PlayersInventoryItem> storageItems)
    {
        var removedItems = storageDic.Keys.Except(storageItems).ToArray();

        for (int i = 0; i < removedItems.Length; i++)
        {
            storageItemUIStack.Push(storageDic[removedItems[i]]);
            storageDic[removedItems[i]].gameObject.SetActive(false);
            storageDic.Remove(removedItems[i]);
        }

        for (int i = 0; i < storageItems.Count; i++)
        {
            if (!storageDic.ContainsKey(storageItems[i]))
            {
                storageDic.Add(storageItems[i], storageItemUIStack.Pop());
            }

            storageDic[storageItems[i]].gameObject.SetActive(true);

            var data = storageItems[i].InstanceData.GetAs<Storage.StorageItemData>();
            storageDic[storageItems[i]].image.rectTransform.sizeDelta = new Vector2(data.sizeX, data.sizeY) * 64;
            storageDic[storageItems[i]].text.text = data.currentCount.ToString();
        }
    }

    private async void OnEnable()
    {
        store.OnStorageChanged += DisplayStorageItem;
        var balances = await EconomyService.Instance.PlayerBalances.GetBalancesAsync();
        int amount = (int)balances.Balances[0].Balance;
        scrapText.text = $"Scrap: {amount}";
    }

    private void OnDisable()
    {
        store.OnStorageChanged -= DisplayStorageItem;
    }

    private void Update()
    {
        if (selectedStorageItem != null)
        {
            var item = storageDic.ToList().Find(x => x.Value == selectedStorageItem).Key;
            store.SellItem(item);
            selectedStorageItem = null;
        }
    }

    public async void DisplayStorageItem(object sender, Store.StorageItemEventHandlerArgs e)
    {
        if (e.changedType == Store.StorageItemEventHandlerArgs.ChangedType.Added)
        {
            if (!storageDic.ContainsKey(e.item))
            {
                storageDic.Add(e.item, storageItemUIStack.Pop());
            }

            storageDic[e.item].gameObject.SetActive(true);
            var data = e.item.InstanceData.GetAs<Storage.StorageItemData>();
            storageDic[e.item].image.rectTransform.sizeDelta = new Vector2(data.sizeX, data.sizeY) * 64;
            storageDic[e.item].text.text = data.currentCount.ToString();
        }

        else
        {
            if (storageDic.ContainsKey(e.item))
            {
                storageItemUIStack.Push(storageDic[e.item]);
                storageDic[e.item].gameObject.SetActive(false);
                storageDic.Remove(e.item);
            }
        }

        var balances = await EconomyService.Instance.PlayerBalances.GetBalancesAsync();
        int amount = (int)balances.Balances[0].Balance;
        scrapText.text = $"Scrap: {amount}";
    }

    private void SelectStorageItem(ItemUI itemUI)
    {
        selectedStorageItem = itemUI;
    }
}
