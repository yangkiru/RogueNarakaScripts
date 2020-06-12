using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RogueNaraka.UIScripts.Shop {
    public class MaxStatUpgradePanel : MonoBehaviour {
        public TextMeshProUGUI CurrentStatPointText;
        public TextMeshProUGUI NextStatPointText;
        public Button StatUpgradeButton;
        public TextMeshProUGUI SoulIcon;
        public TextMeshProUGUI StatUpgradeButtonText;
        public TextMeshProUGUI SuccessOrFailureText;

        public void SetUpgradeButtonText(string _text) {
            this.StatUpgradeButtonText.text = _text;
        }

        public void SetStatPoint(int _statPoint) {
            this.CurrentStatPointText.text = _statPoint.ToString();
            this.NextStatPointText.text = (_statPoint + 1).ToString();
        }

        public void UpdateSuccessOrFailure(bool _isSuccess) {
            if(_isSuccess) {
                this.SuccessOrFailureText.text = "Done";
            } else {
                this.SuccessOrFailureText.text = "Fail";
            }
            StartCoroutine(WaitIsButtonClickable());
        }

        public IEnumerator WaitIsButtonClickable() {
            this.StatUpgradeButton.interactable = false;
            this.SoulIcon.gameObject.SetActive(false);
            this.StatUpgradeButtonText.gameObject.SetActive(false);
            this.SuccessOrFailureText.gameObject.SetActive(true);

            yield return new WaitForSecondsRealtime(0.25f);

            this.StatUpgradeButton.interactable = true;
            this.SoulIcon.gameObject.SetActive(true);
            this.StatUpgradeButtonText.gameObject.SetActive(true);
            this.SuccessOrFailureText.gameObject.SetActive(false);
        }
    }
}