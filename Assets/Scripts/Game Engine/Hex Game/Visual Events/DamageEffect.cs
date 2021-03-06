using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HexGameEngine.Utilities;

namespace HexGameEngine.VisualEvents
{
    public class DamageEffect : MonoBehaviour
    {
        [Header("Component References")]
        public TextMeshProUGUI amountText;
        public GameObject myImageParent;
        public Image heartImage;
        public Image shieldImage;
        public Animator myAnim;

        // Initialization + Setup
        #region        
        public void InitializeSetup(int damage, bool critical = false, bool heal = false)
        {
            transform.position = new Vector2(transform.position.x - 0.2f, transform.position.y);

            if (heal)
            {
                amountText.text = TextLogic.ReturnColoredText(damage.ToString(), TextLogic.lightGreen);
            }
            else if (critical)
                amountText.text = TextLogic.ReturnColoredText(damage.ToString(), TextLogic.neutralYellow);
            else
                amountText.text = damage.ToString();
            ChooseRandomDirection();

        }
        
        #endregion

        // Logic
        #region
        public void DestroyThis()
        {
            Destroy(gameObject);
        }

        public void ChooseRandomDirection()
        {
            int randomNumber = Random.Range(0, 100);
            if (randomNumber < 50)
            {
                myAnim.SetTrigger("Right");
            }
            else
            {
                myAnim.SetTrigger("Left");
            }
        }
        #endregion
    }


}