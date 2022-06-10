using GameCreator.Core;
using System.Collections.Generic;
using UnityEngine;
using GameCreator.Inventory;

[AddComponentMenu("")]
public class ActionSortInventory : IAction
{
    public GameObject container;
    public enum SortBy
    {
        Name,
        Weight,
        Price
    }
    public enum SortOrder
    {
        Ascending,
        Decending
    }

    public SortBy sortby = SortBy.Name;
    public SortOrder sortOrder = SortOrder.Ascending;

    public override bool InstantExecute(GameObject target, IAction[] actions, int index)
    {
        List<ItemUI> itemUIlist = new List<ItemUI>();
        ItemUI[] itemUIs = container.GetComponentsInChildren<ItemUI>();
        if (itemUIs != null) { foreach (ItemUI i in itemUIs) { itemUIlist.Add(i); } }



        switch (sortby)
        {
            case SortBy.Name:
                switch (sortOrder)
                {
                    case SortOrder.Ascending:
                        itemUIlist.Sort((p1, p2) => p1.item.itemName.content.CompareTo(p2.item.itemName.content));
                        break;
                    case SortOrder.Decending:
                        itemUIlist.Sort((p1, p2) => p2.item.itemName.content.CompareTo(p1.item.itemName.content));
                        break;
                }
                        break; 

            case SortBy.Weight:
                switch (sortOrder)
                {
                    case SortOrder.Ascending:
                        itemUIlist.Sort((p1, p2) => p1.item.weight.CompareTo(p2.item.weight));
                        break;
                    case SortOrder.Decending:
                        itemUIlist.Sort((p1, p2) => p2.item.weight.CompareTo(p1.item.weight));
                        break;
                }
                break;

            case SortBy.Price:
                switch (sortOrder)
                {
                    case SortOrder.Ascending:
                        itemUIlist.Sort((p1, p2) => p1.item.price.CompareTo(p2.item.price));
                        break;
                    case SortOrder.Decending:
                        itemUIlist.Sort((p1, p2) => p2.item.price.CompareTo(p1.item.price));
                        break;
                }
                break;
        }
        foreach (ItemUI i in itemUIlist) { i.gameObject.transform.SetSiblingIndex(itemUIlist.IndexOf(i)); }
        
        return true;
    }

#if UNITY_EDITOR

    public static new string NAME = "Custom/Sort Inventory";
    private const string NODE_TITLE = "Sort Inventory";

#endif
}
