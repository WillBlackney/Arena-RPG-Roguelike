using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexGameEngine.Items
{
    public class InventorySlot : MonoBehaviour
    {
        [SerializeField] InventoryItemView myItemView;

        public InventoryItemView MyItemView
        {
            get { return myItemView; }
        }
        public void Reset()
        {
            gameObject.SetActive(false);
        }
        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}