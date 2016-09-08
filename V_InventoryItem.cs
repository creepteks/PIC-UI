﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
public class V_InventoryItem : V_UIElement, IPointerEnterHandler, IPointerDownHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    // fileds
    V_Inventory_UI Inventory;

    public ItemTypes itemType;
    public ItemClass itemClass;
    public float itemTime;
    public GameObject itemPrfb;


    // buttons and UI refs
    public bool isAnInGameItem = false, isEquipped = false;
    public int indexOfItemInList = 0;
    public Image icon;
    public Button donateBtn, deleteBtn, customizeBtn;
    public Text itemNameTxt, timeTxt;
    public Image level;



    // Double click vars
    bool mouseClickStarted = false;
    int mouseClickNumber = 0;
    [SerializeField] float mouseDoubleClickLimit = .25f; // tweak from inspector

    // methods
    public new void Awake()
    {
        base.Awake();
        Inventory = FindObjectOfType<V_Inventory_UI>();
        indexOfItemInList = transform.GetSiblingIndex();

        UIController.IfClick_GoTo(donateBtn, ()=> Inventory.DonateItem(this));
        // #revision: Save the deleted item so it doesnt show up anymore
        UIController.IfClick_GoTo(deleteBtn, () =>
        {
            UIController.AskYesNoQuestion("Do you want to delete this item?",
            () =>
            {
                // #revision
                Destroy(this.gameObject);
                UIController.CloseYesNoQ();
            },
            () =>
            {
                UIController.CloseYesNoQ();
            });
        });

        if (customizeBtn != null)
        {
            UIController.IfClick_GoTo(customizeBtn, ()=> CustomizeWeapon(this));
        }
    }
    new void OnEnable()
    {
        // do not call base.OnEnable on this type of objects, so we keep it seperate from UI panels which need to call base.OnEnable() while hiding it
    }
    void CustomizeWeapon(V_InventoryItem item)
    {
        print("CustomizeWeapon is enabled");
        UIController.Enable_DisableUI(Inventory.weaponCustomizePanel);
        Inventory.weaponToCustomize = item.itemPrfb.gameObject;
        // print(Inventory.weapon_to_customize_placeholder.name);

    }

    public void Initialize(GameObject prfb)
    {
        // #revision: big revision!!! 
        itemPrfb = prfb;
        V_Weapon someWeapon = itemPrfb.GetComponent<V_Weapon>();
        if (someWeapon)
        {
            if(someWeapon.type == V_Weapon.weaponType.rifle)
            {
                itemClass = ItemClass.WEAPON;
                itemType = ItemTypes.W_ASSAULT;
            }
            if(someWeapon.type == V_Weapon.weaponType.pistol)
            {
                itemClass = ItemClass.WEAPON;
                itemType = ItemTypes.W_PISTOL;
            }


            switch (itemClass)
            {
                case ItemClass.WEAPON:
                    icon.sprite = itemPrfb.GetComponent<V_Weapon>().icon;
                    itemNameTxt.text = itemPrfb.GetComponent<V_Weapon>().name;
                    timeTxt.text = itemPrfb.GetComponent<V_Weapon>().lifeTime.ToString();

                    break;

                case ItemClass.GEAR:
                    icon.sprite = itemPrfb.GetComponent<V_Gear>().icon;
                    itemNameTxt.text = itemPrfb.GetComponent<V_Gear>().name;
                    // #revision: does Gear have lifeTime?
                    // timeTxt.text = itemPrfb.GetComponent<V_Gear>().lifeTime.ToString();

                    break;

                case ItemClass.CHARACTER:
                    // icon = itemPrfb.GetComponent<V_Character>().icon;
                    break;

                default:
                    UIController.ThrowError("V_InventoryItem: Item type is not set properly", UIController.CloseError);
                    break;
            }
        }
    }
    public void OnPointerEnter(PointerEventData data)
    {
        if(Inventory.selectedItemByMouseHover == null && Inventory.selectedItem == null)
        {
            Inventory.selectedItemByMouseHover = this;
            return;
        }
        if (Inventory.selectedItem == null)
        {
            return;
        }
        if (Inventory.selectedItem != this && Inventory.selectedItem.itemType == this.itemType)
        {
            Inventory.compareeItem = this;
        }
        else
        {
            Inventory.compareeItem = null;
        }
    }
    public void OnPointerDown(PointerEventData data)
    {
        // detecting Double click
        mouseClickNumber++;
        if (mouseClickStarted)
        {
            return;
        }        
        mouseClickStarted = true;
        // checking for double clicks, and calling some funcs to 1. adding to ingames, 2. equippingItem, 3. deleting from ingames
        StartCoroutine(OnDoubleClick());

        if (data.button == PointerEventData.InputButton.Left)
        {
            EventSystem.current.SetSelectedGameObject(gameObject, data);
        }
        if (Inventory.compareeItem == this)
        {
            Inventory.compareeItem = null;
        }

    }
    IEnumerator OnDoubleClick()
    {
        yield return new WaitForSeconds(mouseDoubleClickLimit);
        if (mouseClickNumber > 1)
        {
            V_InventoryItem tmpItem = this;
            if (isAnInGameItem && !isEquipped)
            {
                Inventory.EquipItem(ref tmpItem);
                Debug.LogAssertion("Equipped item " + this.name);
            }
            else if (isAnInGameItem && isEquipped)
            {
                Inventory.UnEquipItem(ref tmpItem);
                Debug.LogAssertion("Unequipped item " + this.name);
            }
            else if (!isAnInGameItem)
            {
                Inventory.AddToInGameInventory(ref tmpItem);
                Debug.LogAssertion("added item " + this.name + " to ingame inventory");
            }
        }
        mouseClickStarted = false;
        mouseClickNumber = 0;
    }
    public void OnPointerExit(PointerEventData data)
    {
        if(Inventory.selectedItemByMouseHover == this)
		{
			Inventory.selectedItemByMouseHover = null;
		}
    }

    public void OnSelect(BaseEventData data)
    {
        try
        {
            Inventory.selectedItem = data.selectedObject.GetComponent<V_InventoryItem>();
        }
        catch (System.Exception err)
        {
            UIController.ThrowError("V_InventoryItem: OnSelect(): " + err.Message, UIController.CloseError);
            throw;
        }
    }

    public void OnDeselect(BaseEventData data)
    {
        Inventory.selectedItem = null;
        Inventory.compareeItem = null;
    }
}