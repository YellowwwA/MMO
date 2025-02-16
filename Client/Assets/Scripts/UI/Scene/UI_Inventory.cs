using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_Inventory :UI_Base
{
    public List<UI_Inventory_Item> Items { get; } = new List<UI_Inventory_Item>();

    public override void Init()
    {
        Items.Clear();

        GameObject grid = transform.Find("ItemGrid").gameObject;
        foreach (Transform child in grid.transform)
            Destroy(child.gameObject);

        for(int i = 0; i<20; i++)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Inventory_Item", grid.transform);
            UI_Inventory_Item item = go.GetOrAddComponent<UI_Inventory_Item>();
            Items.Add(item);
        }

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (Items.Count == 0)
            return;

        //내가 들고있는 모든 아이템목록에 하나하나 접근하면서 
        List<Item> items = Managers.Inven.Items.Values.ToList();
        items.Sort((left, right) => { return left.Slot - right.Slot; });

        foreach (Item item in items)
        {
            if (item.Slot < 0 || item.Slot >= 20)
                continue;
            Items[item.Slot].SetItem(item); //각 아이템의 슬롯 자리에 이미지를 세팅해줌
        }


    }
}
