using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RogueNaraka.PopUpScripts;


public partial class SoulShopManager : MonoBehaviour {
    const int SKILL_SLOT_JEWEL_COST_1 = 35;
    const int SKILL_SLOT_JEWEL_COST_2 = 70;
    const int SKILL_SLOT_SOUL_COST_1 = 5000;
    const int SKILL_SLOT_SOUL_COST_2 = 10000;

    [Header("Skill Slot Panel Setting")]
    public GameObject SkillSlotShopPanel;
    public TextMeshProUGUI SkillSlotShopTitleText;
    public TextMeshProUGUI SkillSlotShopContext;
    public Button SkillSlotBuyByJewelButton;
    public Button SkillSlotBuyBySoulButton;
    public TextMeshProUGUI SkillSlotJewelCostText;
    public TextMeshProUGUI SkillSlotSoulCostText;

    public void OpenSkillSlotShop() {
        GameManager.instance.SetPause(true);
        this.SkillSlotShopPanel.SetActive(true);
        UpdateSkillSlotShopText();
        if(!SkillManager.instance.skills[3].IsLockedSkill && !SkillManager.instance.skills[4].IsLockedSkill) {
            DeactiveBuyButtons();
        }
    }

    public void CloseSkillSlotShop() {
        GameManager.instance.SetPause(false);
        this.SkillSlotShopPanel.SetActive(false);
    }

    public void BuySkillSlotByJewel() {
        if(!SkillManager.instance.skills[3].IsLockedSkill && !SkillManager.instance.skills[4].IsLockedSkill) {
            DeactiveBuyButtons();
            return;
        }

        bool isCompleteToBuySkillSlot = false;
        if(SkillManager.instance.skills[3].IsLockedSkill) {
            isCompleteToBuySkillSlot = MoneyManager.instance.UseJewel(SKILL_SLOT_JEWEL_COST_1);
            if(isCompleteToBuySkillSlot) {
                SkillManager.instance.UnlockSKillSlot(3);
            }
        } else if(SkillManager.instance.skills[4].IsLockedSkill) {
            isCompleteToBuySkillSlot = MoneyManager.instance.UseJewel(SKILL_SLOT_JEWEL_COST_2);
            if(isCompleteToBuySkillSlot) {
                SkillManager.instance.UnlockSKillSlot(4);
            }
        }

        if(isCompleteToBuySkillSlot) {
            StartCoroutine(DeactiveBuyByButtonForMoment(true, this.SkillSlotBuyByJewelButton, this.SkillSlotJewelCostText));
        } else {
            StartCoroutine(DeactiveBuyByButtonForMoment(false, this.SkillSlotBuyByJewelButton, this.SkillSlotJewelCostText));
        }
    }

    public void BuySkillSlotBySoul() {
        if(!SkillManager.instance.skills[3].IsLockedSkill && !SkillManager.instance.skills[4].IsLockedSkill) {
            DeactiveBuyButtons();
            return;
        }

        bool isCompleteToBuySkillSlot = false;
        if(SkillManager.instance.skills[3].IsLockedSkill) {
            isCompleteToBuySkillSlot = MoneyManager.instance.UseSoul(SKILL_SLOT_SOUL_COST_1);
            if(isCompleteToBuySkillSlot) {
                SkillManager.instance.UnlockSKillSlot(3);
            }
        } else if(SkillManager.instance.skills[4].IsLockedSkill) {
            isCompleteToBuySkillSlot = MoneyManager.instance.UseSoul(SKILL_SLOT_SOUL_COST_2);
            if(isCompleteToBuySkillSlot) {
                SkillManager.instance.UnlockSKillSlot(4);
            }
        }

        if(isCompleteToBuySkillSlot) {
            StartCoroutine(DeactiveBuyByButtonForMoment(true, this.SkillSlotBuyBySoulButton, this.SkillSlotSoulCostText));
        } else {
            StartCoroutine(DeactiveBuyByButtonForMoment(false, this.SkillSlotBuyBySoulButton, this.SkillSlotSoulCostText));
        }
    }

    private void DeactiveBuyButtons() {
        switch(GameManager.language) {
            case Language.English:
                this.SkillSlotJewelCostText.text = "Purchased";
                this.SkillSlotSoulCostText.text = "Purchased";
            break;
            case Language.Korean:
                this.SkillSlotJewelCostText.text = "구매완료";
                this.SkillSlotSoulCostText.text = "구매완료";
            break;
        }
        this.SkillSlotBuyByJewelButton.interactable = false;
        this.SkillSlotBuyBySoulButton.interactable = false;
    }

    private IEnumerator DeactiveBuyByButtonForMoment(bool _isPurchased, Button _button, TextMeshProUGUI _buttonText) {
        _button.interactable = false;

        if(_isPurchased) {
            _buttonText.text = "Done";
        } else {
            _buttonText.text = "Fail";
        }

        yield return new WaitForSecondsRealtime(1f);

        if(!SkillManager.instance.skills[3].IsLockedSkill && !SkillManager.instance.skills[4].IsLockedSkill) {
            DeactiveBuyButtons();
        } else {
            _button.interactable = true;
            if(SkillManager.instance.skills[3].IsLockedSkill) {
                this.SkillSlotJewelCostText.text = string.Format("<sprite=0>  {0}", SKILL_SLOT_JEWEL_COST_1.ToString());
                this.SkillSlotSoulCostText.text = string.Format("<sprite=0>  {0}", SKILL_SLOT_SOUL_COST_1.ToString());
            } else if(SkillManager.instance.skills[4].IsLockedSkill) {
                this.SkillSlotJewelCostText.text = string.Format("<sprite=0>  {0}", SKILL_SLOT_JEWEL_COST_2.ToString());
                this.SkillSlotSoulCostText.text = string.Format("<sprite=0>  {0}", SKILL_SLOT_SOUL_COST_2.ToString());
            } else {
                this.SkillSlotJewelCostText.text = string.Format("<sprite=0>  {0}", SKILL_SLOT_JEWEL_COST_2.ToString());
                this.SkillSlotSoulCostText.text = string.Format("<sprite=0>  {0}", SKILL_SLOT_SOUL_COST_2.ToString());
            }
        }
    }

    private void UpdateSkillSlotShopText() {
        switch(GameManager.language) {
            case Language.English:
                this.SkillSlotShopTitleText.text = "Skill Slot Purchase";
                this.SkillSlotShopContext.text = "Skill Slot\nUnlock";
            break;
            case Language.Korean:
                this.SkillSlotShopTitleText.text = "스킬 슬롯 구매";
                this.SkillSlotShopContext.text = "스킬 슬롯\n잠금 해제";
            break;
        }
        if(SkillManager.instance.skills[3].IsLockedSkill) {
            this.SkillSlotJewelCostText.text = string.Format("<sprite=0>  {0}", SKILL_SLOT_JEWEL_COST_1.ToString());
            this.SkillSlotSoulCostText.text = string.Format("<sprite=0>  {0}", SKILL_SLOT_SOUL_COST_1.ToString());
        } else if(SkillManager.instance.skills[4].IsLockedSkill) {
            this.SkillSlotJewelCostText.text = string.Format("<sprite=0>  {0}", SKILL_SLOT_JEWEL_COST_2.ToString());
            this.SkillSlotSoulCostText.text = string.Format("<sprite=0>  {0}", SKILL_SLOT_SOUL_COST_2.ToString());
        } else {
            this.SkillSlotJewelCostText.text = string.Format("<sprite=0>  {0}", SKILL_SLOT_JEWEL_COST_2.ToString());
            this.SkillSlotSoulCostText.text = string.Format("<sprite=0>  {0}", SKILL_SLOT_SOUL_COST_2.ToString());
        }
    }
}
