using HexGameEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using HexGameEngine.Audio;
using DG.Tweening;
using HexGameEngine.UCM;
using HexGameEngine.TownFeatures;
using HexGameEngine.Characters;

namespace HexGameEngine.Items
{
    public class InventoryItemView : MonoBehaviour
    {
        // Properties + Components
        #region
        [Header("Ability Book Components")]
        [SerializeField] private GameObject bookVisualParent;
        [SerializeField] private Image bookImage;

        [Header("Item Components")]
        [SerializeField] private GameObject weaponVisualParent;
        [SerializeField] private Image weaponImage;
        [SerializeField] private Image rarityOutline;

        private InventoryItem myItemRef;
        public static InventoryItemView itemDragged { get; private set; }

        // Drag values
        [HideInInspector] public bool currentlyBeingDragged = false;
        private Canvas dragCanvas;
        private RectTransform dragTransform;
        #endregion

        // Getters + Accessors
        #region
        public GameObject BookVisualParent
        {
            get { return bookVisualParent; }
        }
        public Image BookImage
        {
            get { return bookImage; }
        }
        public GameObject WeaponVisualParent
        {
            get { return weaponVisualParent; }
        }
        public Image WeaponImage
        {
            get { return weaponImage; }
        }
        public Image RarityOutline
        {
            get { return rarityOutline; }
        }
        public InventoryItem MyItemRef
        {
            get { return myItemRef; }
        }
        #endregion

        // Input
        #region
        public void Click()
        {
            Debug.Log("InventoryItemView.Click()");
            InventoryController.Instance.OnItemViewClicked(this);
        }
        public void MouseEnter()
        {          
            Debug.Log("InventoryItemView.MouseEnter()");
            if (itemDragged != null) return;
            if (MyItemRef.abilityData != null)
                AbilityPopupController.Instance.OnAbilityBookItemMousedOver(this);
            if (MyItemRef.itemData != null)
            {
                ItemPopupController.Instance.OnInventoryItemMousedOver(this);
            }
        }
        public void MouseExit()
        {
            Debug.Log("InventoryItemView.MouseExit()");
            if (itemDragged != null) return;
            AbilityPopupController.Instance.HidePanel();
            ItemPopupController.Instance.HidePanel();
        }
        #endregion

        // Drag Logic
        #region
        private void OnMouseUp()
        {
            if (currentlyBeingDragged)
            {
                currentlyBeingDragged = false;
                itemDragged = null;
                HexCharacterData character = CharacterRosterViewController.Instance.CharacterCurrentlyViewing;

                // Stop dragging SFX
                AudioManager.Instance.FadeOutSound(Sound.Card_Dragging, 0.2f);

                // Check equipping 2h item with two 1h items already equip without enough inventory space.
                if(myItemRef.itemData != null &&
                    myItemRef.itemData.handRequirement == HandRequirement.TwoHanded &&
                    character.itemSet.mainHandItem != null &&
                    character.itemSet.offHandItem != null &&
                    !InventoryController.Instance.HasFreeInventorySpace(2)
                    )
                {
                    Debug.Log("InventoryItemView() warning, not enough inventory space to equip a 2H item while holding two 1H items, cancelling...");
                   
                    // Move item back towards slot position
                    Sequence s = DOTween.Sequence();
                    InventorySlot slot = InventoryController.Instance.AllInventorySlots[InventoryController.Instance.Inventory.IndexOf(MyItemRef)];
                    s.Append(transform.DOMove(slot.transform.position, 0.25f));

                    // Re-parent self on arrival
                    s.OnComplete(() => transform.SetParent(slot.transform));
                    return;
                }

                // Was the drag succesful?
                if (DragSuccessful())
                {
                    // Card added SFX
                    AudioManager.Instance.PlaySound(Sound.GUI_Chime_1);

                    // Snap drag item view back to inventory slot position
                    InventorySlot slot = InventoryController.Instance.AllInventorySlots[InventoryController.Instance.Inventory.IndexOf(MyItemRef)];

                    Sequence s = DOTween.Sequence();
                    s.Append(transform.DOMove(slot.transform.position, 0f));
                    s.OnComplete(() => transform.SetParent(slot.transform));

                    if(myItemRef.itemData != null)
                    {
                        // add item to player
                        ItemController.Instance.HandleGiveItemToCharacterFromInventory
                            (character, MyItemRef, RosterItemSlot.SlotMousedOver);

                        // re build roster, inventory and model views
                        CharacterRosterViewController.Instance.HandleRedrawRosterOnCharacterUpdated();
                        InventoryController.Instance.RebuildInventoryView();
                    }
                    else if(myItemRef.abilityData != null)
                    {
                        TownController.Instance.LibraryAbilitySlot.BuildFromAbility(myItemRef.abilityData);
                    }
                }

                else
                {
                    // Move item back towards slot position
                    Sequence s = DOTween.Sequence();
                    InventorySlot slot = InventoryController.Instance.AllInventorySlots[InventoryController.Instance.Inventory.IndexOf(MyItemRef)];
                    s.Append(transform.DOMove(slot.transform.position, 0.25f));

                    // Re-parent self on arrival
                    s.OnComplete(() => transform.SetParent(slot.transform));
                }
            }
        }
        public void OnMouseDrag()
        {
            Debug.Log("InventoryItemView.Drag()");
            //if (myItemRef == null || myItemRef.itemData == null) return;
            if (myItemRef == null) return;

            // On drag start logic
            if (currentlyBeingDragged == false)
            {
                currentlyBeingDragged = true;
                itemDragged = this;

                // Play dragging SFX
                AudioManager.Instance.FadeInSound(Sound.Card_Dragging, 0.2f);

                // Hide pop up window
                ItemPopupController.Instance.HidePanel();
            }

            // Unparent from vert fitter, so it wont be masked while dragging
            transform.SetParent(InventoryController.Instance.DragParent);

            // Get the needed components, if we dont have them already
            if (dragCanvas == null)
                dragCanvas = InventoryController.Instance.MainVisualParent.GetComponent<Canvas>();

            if (dragTransform == null)
                dragTransform = InventoryController.Instance.MainVisualParent.transform as RectTransform;

            // Weird hoki poki magic for dragging in local space on a non screen overlay canvas
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(dragTransform, Input.mousePosition,
                dragCanvas.worldCamera, out pos);

            // Follow the mouse
            transform.position = dragCanvas.transform.TransformPoint(pos);

        }       
        private bool DragSuccessful()
        {
            bool bRet = false;
            
            if(MyItemRef.itemData != null)
            {
                var mainHandItem = CharacterRosterViewController.Instance.CharacterCurrentlyViewing.itemSet.mainHandItem;
                // Cant equip an off hand if main hand is empty
                if (RosterItemSlot.SlotMousedOver.SlotType == RosterSlotType.OffHand &&
                    mainHandItem == null)
                    return false;

                // Cant equip off hand if main hand weapon is 2 handed
                if (RosterItemSlot.SlotMousedOver.SlotType == RosterSlotType.OffHand &&
                   mainHandItem != null &&
                   mainHandItem.handRequirement == HandRequirement.TwoHanded)
                    return false;


                if (ItemController.Instance.IsItemValidOnSlot(MyItemRef.itemData, RosterItemSlot.SlotMousedOver,
                CharacterRosterViewController.Instance.CharacterCurrentlyViewing))
                {
                    bRet = true;
                }
            }

            else if(MyItemRef.abilityData != null)
            {
                bRet = true;
            }
           
            
            return bRet;
        }
        #endregion


        // Misc
        #region
        public void Reset()
        {
            gameObject.SetActive(false);
            bookVisualParent.SetActive(false);
            weaponVisualParent.SetActive(false);
            myItemRef = null;
        }
        public void SetMyItemRef(InventoryItem item)
        {
            myItemRef = item;
        }

        #endregion
    }
}