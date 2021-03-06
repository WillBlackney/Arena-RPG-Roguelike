using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexGameEngine.Abilities;
using TMPro;
using UnityEngine.UI;
using HexGameEngine.Libraries;
using HexGameEngine.Player;
using HexGameEngine.Utilities;
using HexGameEngine.UI;
using HexGameEngine.Items;
using DG.Tweening;

namespace HexGameEngine.TownFeatures
{
    public class ItemShopSlot : MonoBehaviour
    {
        // Properties + Components
        #region
        [Header("Components")]
        [SerializeField] TextMeshProUGUI itemNameText;
        [SerializeField] TextMeshProUGUI goldCostText;
        [SerializeField] Image itemImage;
        [SerializeField] Transform scaleParent;

        // Non inspector fields
        private ItemShopData myData;
        #endregion

        // Getters + Accessors
        #region
        public ItemShopData MyData
        {
            get { return myData; }
        }
        #endregion

        // Input
        #region
        public void MouseClick()
        {            
            if (AbleToBuy())
                TownController.Instance.HandleBuyItemFromArmoury(MyData);            
        }
        public void MouseEnter()
        {            
            if (myData != null)            
                ItemPopupController.Instance.OnShopItemMousedOver(this);         
        }
        public void MouseExit()
        {            
            if (myData != null)            
                ItemPopupController.Instance.HidePanel();          
        }
        #endregion

        // Logic
        #region
        private bool AbleToBuy()
        {
            bool ret = false;

            if (MyData.goldCost <= PlayerDataController.Instance.CurrentGold &&
                InventoryController.Instance.HasFreeInventorySpace())
                ret = true;
            return ret;
        }
        public void BuildFromItemShopData(ItemShopData data)
        {
            gameObject.SetActive(true);
            myData = data;
            itemNameText.text = data.item.itemName;
            itemImage.sprite = data.item.ItemSprite;

            // Color cost text red if not enough gold
            string col = "<color=#FFFFFF>";
            if (PlayerDataController.Instance.CurrentGold < data.goldCost) col = TextLogic.lightRed;
            goldCostText.text = TextLogic.ReturnColoredText(data.goldCost.ToString(), col);
        }
        public void Reset()
        {
            gameObject.SetActive(false);
            myData = null;
        }
        public void Enlarge()
        {
            // Calculate enlargement scale and convert it to to a vector 3
            Vector3 endScale = new Vector3(1.15f,1.15f, 1f);

            // Scale the transform to its new size
            scaleParent.DOKill();
            scaleParent.DOScale(endScale, 0.2f);
        }
        public void Shrink(float speed = 0.2f)
        {
            // Calculate enlargement scale and convert it to to a vector 3
            Vector3 endScale = new Vector3(1f, 1f, 1f);

            // Scale the transform to its new size
            scaleParent.DOKill();
            scaleParent.DOScale(endScale, speed);
        }
        #endregion
    }

    public class ItemShopData
    {
        public ItemData item;
        public int goldCost;

        public ItemShopData(ItemData item, int goldCost)
        {
            this.item = item;
            this.goldCost = goldCost;
        }
    }
}