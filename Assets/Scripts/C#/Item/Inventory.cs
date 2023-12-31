using Mono.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Unity.Netcode;
using Unity.Services.Economy.Model;
using Unity.Services.Economy;
using UnityEngine;
using Unity.Services.Authentication;
using UnityEngine.InputSystem.Processors;
using Unity.Services.Core;
using Unity.Services.CloudCode;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Unity.Services.Lobbies.Models;
using Unity.Collections;

public enum ROTATION_TYPE
{
    TOP,
    RIGHT
}

public partial class Inventory : NetworkBehaviour
{
    private class ResultType
    {
        public InventoryResponse[] inventory;
    }

    private class InventoryResponse
    {
        public JObject created;
        public JObject instanceData;
        public string inventoryItemId;
        public JObject modified;
        public string playersInventoryItemId;
        public string writeLock;
    }

    public NetworkVariable<int> sizeX = new NetworkVariable<int>();
    public NetworkVariable<int> sizeY = new NetworkVariable<int>();
    public NetworkList<InventoryItem> items;
    private List<GettableItem> nearItems = new List<GettableItem>();

    public event Action<InventoryEventHandlerArgs> OnInventoryChanged;
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
        OnInventoryChanged?.Invoke(inventoryEventHandlerArgs);
        if (changeEvent.PreviousValue.Equals(inventoryUI.selectedInventoryItem) && inventoryUI.selectedInventoryItem.itemName != ITEMNAME.NONE)
            inventoryUI.selectedInventoryItem = changeEvent.Value;
    }
    [SerializeField]
    private InventoryUI inventoryUI;
    private Player curPlayer;

    private void Awake()
    {
        items = new NetworkList<InventoryItem>();
        EquipInit();
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
            InitServerRPC(AuthenticationService.Instance.PlayerId);
            inventoryEventHandlerArgs = new InventoryEventHandlerArgs(items);
            items.OnListChanged += OnItemChanged;
            curPlayer = GetComponent<Player>();
        }
    }

    public void InitInventoryUI(InventoryUI ui)
    {
        inventoryUI = ui;
    }

    [ServerRpc]
    private void InitServerRPC(string playerID)
    {
        Init(playerID);
    }

    /// <summary>
    /// 게임 시작 시 플레이어가 창고에서 넣어놨던 인벤토리 아이템들을 불러오는 함수
    /// </summary>
    /// <param name="playerID"></param>
    private async void Init(string playerID)
    {
        var response = await CloudCodeService.Instance.CallEndpointAsync<InventoryResponse[]>("GetPlayerInventory", new Dictionary<string, object>() { { "otherPlayerId", playerID } });
        
        for (int i = 0; i < response.Length; i++)
        {
            var data = JsonConvert.DeserializeObject<Storage.StorageItemData>(response[i].instanceData.ToString());
            if (data.inInventory)
            {
                ROTATION_TYPE rotationType;

                if (data.isRight)
                    rotationType = ROTATION_TYPE.RIGHT;
                else
                    rotationType = ROTATION_TYPE.TOP;

                var item = new InventoryItem((ITEMNAME)Enum.Parse(typeof(ITEMNAME), response[i].inventoryItemId), 
                    rotationType, data.currentCount, data.posX, data.posY);

                items.Add(item);
            }
        }
    }

    /// <summary>
    /// 인벤토리안에 아이템을 넣는 함수. 매개변수인 x,y가 기준점으로 좌하단에 위치함
    /// </summary>
    /// <param name="item">해당 아이템의 NetworkObject</param>
    /// <param name="posX">넣고자 하는 x좌표</param>
    /// <param name="posY">넣고자 하는 y좌표</param>
    /// <param name="serverRpcParams"></param>
    [ServerRpc]
    public void PutItemServerRPC(NetworkObjectReference item, int posX, int posY, ServerRpcParams serverRpcParams = default)
    {
        NetworkObject getItem = item;
        var t = getItem.GetComponent<GettableItem>();
        InventoryItem inventoryItem = Item.GetInventoryItem(t.ItemName, ROTATION_TYPE.RIGHT, t.ItemCount, posX, posY);
        inventoryItem.posX = posX;
        inventoryItem.posY = posY;

        if (CheckEmpty(inventoryItem))
        {
            items.Add(inventoryItem);
            getItem.Despawn();
        }
        else
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { serverRpcParams.Receive.SenderClientId }
                }
            };

            PutItemClientRPC(clientRpcParams);
        }
    }

    [ClientRpc]
    public void PutItemClientRPC(ClientRpcParams clientRpcParams)
    {
        inventoryUI.DisplayNearItemUI();
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

    /// <summary>
    /// 인벤토리에있는 아이템을 제거함
    /// </summary>
    /// <param name="item"></param>
    /// <param name="serverRpcParams"></param>
    public void RemoveItem(InventoryItem item)
    {
        if (!IsServer)
        {
            return;
        }

        items.Remove(item);
    }

    /// <summary>
    /// 인벤토리에 존재하는 아이템의 위치를 바꾸는 함수
    /// </summary>
    /// <param name="item"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="serverRpcParams"></param>
    [ServerRpc]
    public void MoveItemServerRPC(InventoryItem item, int x, int y, ServerRpcParams serverRpcParams = default)
    {
        // 아이템의 종류가 같다면 합치기
        InventoryItem receiveItem;
        if (CheckSameItemType(item.hashCode, x, y, item.itemName, out receiveItem))
        {
            TransferItemCount(item, receiveItem, serverRpcParams);
            return;
        }

        // 해당 공간이 비어있는제 확인
        item.posX = x; item.posY = y;

        if (!CheckEmpty(item))
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { serverRpcParams.Receive.SenderClientId }
                }
            };

            // 인벤토리가 변경되지 않았기에 다시 클라에게 인벤토리 호출을 해 원상태로 복귀 시킴
            MoveItemClientRPC(clientRpcParams);
            return;
        }

        items[FindIndex(item)] = item;
    }

    [ClientRpc]
    public void MoveItemClientRPC(ClientRpcParams clientRpcParams = default)
    {
        OnInventoryChanged?.Invoke(inventoryEventHandlerArgs);
    }

    /// <summary>
    /// 아이템 합치는 함수. 아이템 최대 갯수에 맞춰 아이템 갯수를 옮겨줌.
    /// </summary>
    /// <param name="item">이동시킬려는 아이템</param>
    /// <param name="receiveItem">해당 위치에 있는 아이템. 해당 아이템의 갯수가 증가함</param>
    /// <param name="serverRpcParams"></param>
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

    /// <summary>
    /// 아이템을 회전시키는 함수
    /// </summary>
    /// <param name="item"></param>
    /// <param name="serverRpcParams"></param>
    [ServerRpc]
    public void RotateItemServerRPC(InventoryItem item, ServerRpcParams serverRpcParams = default)
    {
        if (item.rotationType.Equals(ROTATION_TYPE.RIGHT))
            item.rotationType = ROTATION_TYPE.TOP;
        else
            item.rotationType = ROTATION_TYPE.RIGHT;

        items[FindIndex(item)] = item;
    }

    /// <summary>
    /// 기준점에서 해당 크기의 공간이 비어있는지 확인하는 함수
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private bool CheckEmpty(InventoryItem item)
    {
        if (!IsServer)
            return false;

        if (item.posX < 0 || item.posY < 0 || item.posX >= sizeX.Value || item.posY >= sizeY.Value)
            return false;

        if (item.posX + item.sizeX - 1 >= sizeX.Value || item.posY + item.sizeY - 1 >= sizeY.Value)
            return false;

        int itemSizeX, itemSizeY;

        if (item.rotationType == ROTATION_TYPE.RIGHT)
        {
            itemSizeX = item.sizeX;
            itemSizeY = item.sizeY;
        }
        else
        {
            itemSizeX = item.sizeY;
            itemSizeY = item.sizeX;
        }

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].Equals(item))
                continue;

            if (items[i].rotationType == ROTATION_TYPE.RIGHT)
            {
                if (item.posX + itemSizeX > items[i].posX &&
                    items[i].posX + items[i].sizeX > item.posX &&
                    item.posY + itemSizeY > items[i].posY &&
                    items[i].posY + items[i].sizeY > item.posY)
                {
                    return false;
                }
            }
            else
            {
                if (item.posX + itemSizeX > items[i].posX &&
                    items[i].posX + items[i].sizeY > item.posX &&
                    item.posY + itemSizeY > items[i].posY &&
                    items[i].posY + items[i].sizeX > item.posY)
                {
                    return false;
                }
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

    /// <summary>
    /// 같은 종류의 아이템이 있는지 확인하는 함수. 만약에 같은 종류의 아이템이 발견 되면 해당 아이템도 리턴함
    /// </summary>
    /// <param name="hashcode">아이템이 같은 아이템인지 구분하는 매개변수</param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="itemName"></param>
    /// <param name="item">반환되는 같은 종류의 아이템. (기존 아이템과는 다른 아이템임)</param>
    /// <returns></returns>
    private bool CheckSameItemType(FixedString128Bytes hashcode, int x, int y, ITEMNAME itemName, out InventoryItem item)
    {
        item = new InventoryItem();

        if (x < 0 || y < 0 || x >= sizeX.Value || y >= sizeY.Value)
            return false;

        for (int i = 0; i < items.Count; i++)
            if (items[i].itemName == itemName && items[i].hashCode != hashcode)
            {
                if (items[i].rotationType == ROTATION_TYPE.RIGHT)
                {
                    if (items[i].posX <= x && x < items[i].posX + items[i].sizeX && items[i].posY <= y && y < items[i].posY + items[i].sizeY)
                    {
                        item = items[i];
                        return true;
                    }
                }
                else
                {
                    if (items[i].posX <= x && x < items[i].posX + items[i].sizeY && items[i].posY <= y && y < items[i].posY + items[i].sizeX)
                    {
                        item = items[i];
                        return true;
                    }
                }
            }

        return false;
    }

    /// <summary>
    /// 인벤토리 좌표에 해당하는 아이템을 리턴하는 함수
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public InventoryItem SelectItem(int x, int y)
    {
        if (x < 0 || y < 0 || x >= sizeX.Value || y >= sizeY.Value)
            return new InventoryItem();

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].rotationType == ROTATION_TYPE.RIGHT)
            {
                if (items[i].posX <= x && x < items[i].posX + items[i].sizeX && items[i].posY <= y && y < items[i].posY + items[i].sizeY)
                    return items[i];
            }
            else
            {
                if (items[i].posX <= x && x < items[i].posX + items[i].sizeY && items[i].posY <= y && y < items[i].posY + items[i].sizeX)
                    return items[i];
            }
        }

        return new InventoryItem();
    }

    /// <summary>
    /// 인벤토리창 UI 관리 함수
    /// </summary>
    /// <returns></returns>
    public bool SwitchInventoryPanel()
    {
        bool state = inventoryUI.gameObject.activeSelf;
        inventoryUI.gameObject.SetActive(!state);
        return !state;
    }

    /// <summary>
    /// 근처에 GettableItem이 있을시 nearItems 리스트에 추가하는 함수. 이벤트 콜도 진행.
    /// </summary>
    /// <param name="item"></param>
    public void AddNearItem(GettableItem item)
    {
        nearItems.Add(item);
        OnNearItemChanged?.Invoke(this, new NearItemEventHandlerArgs(item, NearItemEventHandlerArgs.ChangedType.Added));
    }

    /// <summary>
    /// 근처에 GettableItem이 없어질 시 nearItems 리스트에 삭제되는 함수. 이벤트 콜도 진행.
    /// </summary>
    /// <param name="item"></param>
    public void RemoveNearItem(GettableItem item)
    {
        nearItems.Remove(item);
        OnNearItemChanged?.Invoke(this, new NearItemEventHandlerArgs(item, NearItemEventHandlerArgs.ChangedType.Removed));
    }

    /// <summary>
    /// nearItems 리스트를 읽기 전용으로 반환하는 함수 (외부에서 수정 방지를 위함)
    /// </summary>
    /// <returns></returns>
    public System.Collections.ObjectModel.ReadOnlyCollection<GettableItem> GetNearItems()
    {
        return nearItems.AsReadOnly();
    }

    /// <summary>
    /// 인벤토리 UI가 Enable 될 시 해당 함수를 호출하여 인벤토리 재정렬 작업을 함
    /// </summary>
    public void EnableInventoryUI()
    {
        OnInventoryChanged?.Invoke(inventoryEventHandlerArgs);
    }

    /// <summary>
    /// 아이템 버리는 함수. GettableItem이 생성
    /// </summary>
    /// <param name="item"></param>
    /// <param name="serverRpcParams"></param>
    [ServerRpc]
    public void DropItemServerRPC(InventoryItem item, ServerRpcParams serverRpcParams = default)
    {
        var networkObj = Instantiate(GettableItem.GetItemPrefab(item.itemName), transform.position + transform.forward, Quaternion.identity).GetComponent<NetworkObject>();
        networkObj.Spawn();
        RemoveItem(item);
    }

    /// <summary>
    /// ItemName에 해당하는 아이템이 인벤토리에 존재하는 지 체크하는 함수
    /// 만약 존재시 해당 아이템도 리턴함
    /// </summary>
    /// <param name="itemName"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool HasItem(ITEMNAME itemName, out InventoryItem item)
    {
        for (int i = 0; i < items.Count; i++)
            if (items[i].itemName == itemName)
            {
                item = items[i];
                return true;
            }

        item = new InventoryItem();
        return false;
    }

    /// <summary>
    /// NetworkList에는 따로 Find함수가 없기 때문에 해당 함수를 제작함.
    /// 매개변수로 들어오는 아이템이 리스트에 있을 시 해당 아이템의 인덱스를 반환해줌.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public int FindIndex(InventoryItem item)
    {
        for (int i = 0; i < items.Count; i++)
            if (items[i].Equals(item))
                return i;

        return -1;
    }

    /// <summary>
    /// 인벤토리 저장 함수. 다음 함수를 통해 게임을 나가더라도 인벤토리와 창고 인벤토리의 아이템이 연동되게 할 수 있음.
    /// </summary>
    /// <param name="playerId"></param>
    [ServerRpc]
    private void SaveInventoryServerRPC(string playerId)
    {
        SaveInventory(playerId);
    }

    private async void SaveInventory(string playerId)
    {
        // 1. 기존에 있던 인벤토리 아이템들을 삭제
        // 2. 현재 가지고있는 인벤토리 아이템들을 추가

        Storage.StorageItemData[] datas = new Storage.StorageItemData[items.Count];
        string[] itemNames = new string[items.Count];

        for (int i = 0; i < items.Count; i++)
        {
            bool isRight = items[i].rotationType == ROTATION_TYPE.RIGHT ? true : false;

            Storage.StorageItemData data = new Storage.StorageItemData()
            {
                inInventory = true,
                isRight = isRight,
                currentCount = items[i].currentCount,
                maxCount = items[i].maxCount,
                posX = items[i].posX,
                posY = items[i].posY,
                sizeX = items[i].sizeX,
                sizeY = items[i].sizeY
            };

            datas[i] = data;
            itemNames[i] = items[i].itemName.ToString();
        }

        await CloudCodeService.Instance.CallEndpointAsync("SaveInventoryItems",
            new Dictionary<string, object>() {
                    { "otherPlayerId", playerId },
                    { "inventoryItemIds", JsonConvert.SerializeObject(itemNames) },
                    { "item", JsonConvert.SerializeObject(datas) } 
            });

        Debug.Log("complete");
    }

    /// <summary>
    /// 아이템 사용 함수 해당 좌표에 해당하는 아이템을 선택후 사용가능한 아이템일 시 아이템 사용 및 아이템 갯수를 줄여줌.
    /// </summary>
    /// <param name="pos"></param>
    [ServerRpc]
    public void UseItemServerRPC(Vector2Int pos)
    {
        var item = SelectItem(pos.x, pos.y);

        if (item.itemName != ITEMNAME.NONE)
        {
            var usableItem = Item.GetUsableItem(item.itemName);

            if (usableItem != null)
            {
                // 아이템 사용 ture 리턴시에만 아이템 카운트 감소
                if (usableItem.Use(curPlayer))
                {
                    item.currentCount -= 1;
                if (item.currentCount <= 0)
                        items.Remove(item);
                    else
                        items[FindIndex(item)] = item;
                }
            }
        }
    }
}
