using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RogueNaraka.UnitScripts;
using RogueNaraka.Escapeable;
using RogueNaraka.UIScripts.Shop;
using RogueNaraka.RollScripts;

public partial class SoulShopManager : MonoBehaviour
{
    public GameObject shopPnl;
    public GameObject statPnl;
    public GameObject skillPnl;
    public GameObject soulPnl;
    public GameObject diamondPnl;
    public GameObject preparingPnl;

    public GameObject TitleManager;

    public Button[] menuBtns;

    public Button statBtn;

    public int shopStage
    { get { return _shopStage; } }
    [SerializeField]
    private int _shopStage;

    public static SoulShopManager instance = null;
    private void Awake()
    {
        if (instance == null) {
            instance = this;
        }     
    }

    string lastMusic = string.Empty;

    /// <summary>
    /// Soul 상점 패널을 열거나 닫는 함수
    /// 외부에서 접근 가능함
    /// </summary>
    /// <param name="value"></param>
    public void SetSoulShop(bool value, int menu = 0)
    {
        if (value)
        {
            shopPnl.SetActive(true);
            
            GameManager.instance.moneyManager.Load();

            //AudioManager.
            AudioManager.instance.PlayMusic("cave");
            
            menuBtns[menu].onClick.Invoke();

            if(TitleManager.activeSelf) {
                GameManager.instance.SetSettingBtn(false);
            }
        }
        else
        {
            shopPnl.SetActive(false);

            if (StatManager.instance.statPnl.activeSelf)
                StatManager.instance.SyncStatUpgradeTxt();
            if(DeathManager.instance.deathPnl.gameObject.activeSelf) {
                AudioManager.instance.PlayMusic(AudioManager.instance.currentDeathMusic);
            } else if(RollManager.instance.IsFirstRoll) {

            } else {
                AudioManager.instance.PlayMusic(AudioManager.instance.currentMainMusic);
            }

            if(TitleManager.activeSelf) {
                GameManager.instance.SetSettingBtn(true);
            }
        }
    }

    public void SetSoulShop(bool value)
    {
        SetSoulShop(value, 0);
    }

    public void OpenSoulShop(int menu)
    {
        SetSoulShop(true, menu);
    }

    /// <summary>
    /// 스탯 패널만 여는 함수
    /// </summary>
    public void StatPnlOpen()
    {
        shopPnl.SetActive(true);
        skillPnl.SetActive(false);
        weaponPnl.SetActive(false);
        soulPnl.SetActive(false);
        diamondPnl.SetActive(false);
        preparingPnl.SetActive(false);

        statPnl.SetActive(true);
        SyncStatUpgradeTxt();
        TutorialManager.instance.StartTutorial(3);
    }

    public void SkillPnlOpen()
    {
        shopPnl.SetActive(true);
        statPnl.SetActive(false);
        weaponPnl.SetActive(false);
        soulPnl.SetActive(false);
        diamondPnl.SetActive(false);

        //InitSkillPnl();
        skillPnl.SetActive(true);
        preparingPnl.SetActive(true);
    }

    public void WeaponPnlOpen()
    {
        shopPnl.SetActive(true);
        statPnl.SetActive(false);
        skillPnl.SetActive(false);
        soulPnl.SetActive(false);
        diamondPnl.SetActive(false);

        weaponPnl.SetActive(true);

        WeaponPnlUpdate(PlayerPrefs.GetInt("exp"), true);
        preparingPnl.SetActive(false);

        weaponId = -1;
        _weaponId = -1;
    }

    public void SoulPnlOpen()
    {
        shopPnl.SetActive(true);
        statPnl.SetActive(false);
        skillPnl.SetActive(false);
        weaponPnl.SetActive(false);
        diamondPnl.SetActive(false);

        soulPnl.SetActive(true);

        preparingPnl.SetActive(false);
        RefiningRateTxtNameUpdate();
        RefiningRateTxtUpdate();
        UpdateBuyRefiningRateForJewelPanel();
        ADLanguageUpdate();
        UpdateSoulPurchaseText();

        //정제율 제한
        if (MoneyManager.instance.refiningRate >= 2f)
        {
            soulRefRateBtnTxt.text = "Max";
            soulRefRateBtn.interactable = false;
        }
        else
        {
            float rate = MoneyManager.instance.refiningRate;
            rate = Mathf.Round(rate * 100) * 0.01f;
            this.soulRefRateBtnTxt.text = string.Format("<sprite=0> {0}", GetRefiningRateCost(rate));
            soulRefRateBtn.interactable = true;
        }
        TutorialManager.instance.StartTutorial(4);
    }

    public void DiamondPnlOpen()
    {
        shopPnl.SetActive(true);
        statPnl.SetActive(false);
        skillPnl.SetActive(false);
        weaponPnl.SetActive(false);
        soulPnl.SetActive(false);
        preparingPnl.SetActive(false);
        UpdateDiamondPurchaseText();

        diamondPnl.SetActive(true);
    }

    #region stat

    [Header("About StatShop")]
    public MaxStatUpgradePanel[] MaxStatUpgradePanelArray;

    /// <summary>
    /// 업그레이드 버튼을 누르면 upgrading에 값이 전달됨
    /// </summary>
    /// <param name="type"></param>
    public void SelectStatUpgrade(int type)
    {
        int required = GetRequiredSoul((STAT)type);
        if (MoneyManager.instance.soul >= required)
        {
            Debug.Log("Max Stat Upgraded");
            
            Stat stat = Stat.JsonToStat(PlayerPrefs.GetString("stat"));
            stat.AddMax((STAT)type, 1);
            if(BoardManager.instance.player != null) {
                BoardManager.instance.player.stat.AddMax((STAT)type, 1);
            }
            PlayerPrefs.SetString("stat", Stat.StatToJson(stat));
            MoneyManager.instance.AddSoul(-required);
            MoneyManager.instance.SaveSoul();
            SyncStatUpgradeTxt();
            MaxStatUpgradePanelArray[type].UpdateSuccessOrFailure(true);
        }
        else
        {
            Debug.Log("Not Enough Soul");
            MaxStatUpgradePanelArray[type].UpdateSuccessOrFailure(false);
        }
    }

    /// <summary>
    /// 스탯 업그레이드 값 구하는 함수
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private int GetRequiredSoul(STAT type)
    {
        Stat stat = (Stat)Stat.JsonToStat(PlayerPrefs.GetString("stat")).Clone();
        float target = 0;
        switch(type)
        {
            case STAT.DMG:
                target = stat.dmgMax;
                break;
            case STAT.SPD:
                target = stat.spdMax;
                break;
            case STAT.TEC:
                target = stat.tecMax;
                break;
            case STAT.HP:
                target = stat.hpMax;
                break;
            case STAT.MP:
                target = stat.mpMax;
                break;
            case STAT.HR:
                target = stat.hpRegenMax;
                break;
            case STAT.MR:
                target = stat.mpRegenMax;
                break;
            case STAT.SP:
                target = stat.statPoints;
                break;
        }

        return GetStatCost((int)target);
    }

    /// <summary>
    /// 맥스 스탯 값 업데이트
    /// </summary>
    private void SyncStatUpgradeTxt()
    {
        Stat stat = (Stat)Stat.JsonToStat(PlayerPrefs.GetString("stat")).Clone();

        for (int i = 0; i < MaxStatUpgradePanelArray.Length; i++) {
            MaxStatUpgradePanelArray[i].SetStatPoint((int)stat.GetMax((STAT)i));
            MaxStatUpgradePanelArray[i].SetUpgradeButtonText(GetRequiredSoul((STAT)i).ToString());
        }
    }

    public int GetStatCost(int sp, int cost = 0) {
        if (sp <= 5)
            return 3 + cost;
        else if (sp < 10)
            return GetStatCost(sp - 1, cost + 1);
        else if (sp < 15)
            return GetStatCost(sp - 1, cost + 2);
        else if (sp < 20)
            return GetStatCost(sp - 1, cost + 4);
        else if (sp < 25)
            return GetStatCost(sp - 1, cost + 6);
        else if (sp < 30)
            return GetStatCost(sp - 1, cost + 8);
        else if (sp < 35)
            return GetStatCost(sp - 1, cost + 10);
        else if (sp < 40)
            return GetStatCost(sp - 1, cost + 12);
        else if (sp < 45)
            return GetStatCost(sp - 1, cost + 14);
        else if (sp < 50)
            return GetStatCost(sp - 1, cost + 16);
        else if (sp < 55)
            return GetStatCost(sp - 1, cost + 18);
        else
            return GetStatCost(sp - 1, cost + 20);
    }

    public int GetStatDiff(int sp) {
        int aSum = 0;
        int bSum = 0;
        for(int i = sp - 1; i >= 5; i--) {
            aSum += GetStatCost(i);
            bSum += (i - 4) * 10;
        }
        int sum = aSum - bSum;
        return sum > 0 ? sum : 0;
    }

    public void RewardStatCost() {
        if (PlayerPrefs.GetInt("isRewardStatCost") == 0) {
            PlayerPrefs.SetInt("isRewardStatCost", 1);
            Stat stat = Stat.DataToStat();
            int amount = 0;
            for (int i = 0; i < 8; i++) {
                amount += GetStatDiff((int)stat.GetMax((STAT)i));
            }
            if (amount > 0) {
                MoneyManager.instance.AddSoul(amount);
                Debug.Log("StatCost Rewarded! : " + amount);
            } else
                Debug.Log("StatCost Not Rewarded! : " + amount);
        }
    }

    #endregion

    #region skill

    //public SkillScrollView skillScrollView;

    //void InitSkillPnl()
    //{
    //    var cellData = GetUnBoughtSkills();
    //    if (cellData.Count > 0)
    //    {
    //        skillScrollView.gameObject.SetActive(true);
    //        skillScrollView.UpdateData(cellData);
    //    }
    //    else
    //        skillScrollView.gameObject.SetActive(false);
    //}

    //public List<SkillData> GetUnBoughtSkills()
    //{
    //    List<SkillData> list = new List<SkillData>();

    //    bool[] boughts = SkillData.GetBoughtSkills();
    //    for (int i = 0; i < GameDatabase.instance.skills.Length; i++)
    //    {
    //        if (!boughts[i])
    //            list.Add((SkillData)GameDatabase.instance.skills[i].Clone());
    //    }
    //    return list;
    //}

    //public void BuySkill(Button btn, TextMeshProUGUI txt, int id)
    //{
    //    SkillData skill = GameDatabase.instance.skills[id];
    //    if (txt.text.CompareTo("Buy") == 0)
    //    {
    //        Debug.Log("Buying skill " + skill.GetName() + skill.cost + " soul");
    //        txt.text = skill.cost.ToString();
    //    }
    //    else
    //    {
    //        Debug.Log("bought " + skill.GetName() + " " + skill.cost + " soul");
    //        if (MoneyManager.instance.UseSoul(skill.cost))
    //        {
    //            SkillData.Buy(skill.id);
    //            txt.text = "Done";
    //        }
    //        else
    //            txt.text = "Fail";
    //        StartCoroutine(BtnCorou(btn, txt));
    //    }
    //}

    //IEnumerator BtnCorou(Button btn, TextMeshProUGUI txt)
    //{
    //    btn.interactable = false;
    //    float t = 0;
    //    while(t < 1)
    //    {
    //        yield return null;
    //        t += Time.unscaledDeltaTime;
    //    }

    //    if (txt.text.CompareTo("Done") == 0)
    //        InitSkillPnl();

    //    btn.interactable = true;
    //    txt.text = "Buy";
    //}

    #endregion

    #region weapon
    [Header("About WeaponShop")]
    public GameObject weaponPnl;
    public Image weaponSoulIcon;
    public TextMeshProUGUI weaponSoulTxt;
    public TextMeshProUGUI successOrFailureToUpgradeWeaponTxt;
    public TextMeshProUGUI weaponLevelTxt;
    public TextMeshProUGUI weaponDPSTxt;
    public TextMeshProUGUI weaponDPSFigureTxt;
    public TextMeshProUGUI weaponRangeTxt;
    public TextMeshProUGUI weaponRangeFigureTxt;
    public TextMeshProUGUI weaponNameTxt;
    public Image weaponIconImage;
    public Button weaponExpBtn;
    public GameObject weaponLevelUpBanner;
    int weaponId = -1;
    int _weaponId = -1;

    public void ClickOnWeaponUpgradeButton() {
        int exp = PlayerPrefs.GetInt("exp");
        int remain;
        int currentLevel = GetWeaponLevel(exp, out remain);
        int lastLevel = currentLevel;
        int amount = 1 + currentLevel;

        if(MoneyManager.instance.soul < GameDatabase.instance.playerWeapons[currentLevel].cost) {
            FailToLevelUpWeapon();
        } else {
            SuccessToLevelUpWeapon(currentLevel, exp);
        }
    }

    private void FailToLevelUpWeapon() {
        switch(GameManager.language) {
            case Language.English: 
                this.successOrFailureToUpgradeWeaponTxt.text = "Fail";
            break;
            case Language.Korean:
                this.successOrFailureToUpgradeWeaponTxt.text = "실패";
            break;
        }
        StartCoroutine(DeactiveUpgradeButtonForMoment());
    }

    private void SuccessToLevelUpWeapon(int _level, int _exp) {
        switch(GameManager.language) {
            case Language.English: 
                this.successOrFailureToUpgradeWeaponTxt.text = "Success";
            break;
            case Language.Korean:
                this.successOrFailureToUpgradeWeaponTxt.text = "성공";
            break;
        }
        if(_level >= GameDatabase.instance.playerWeapons.Length-2) {
            SetActiveUpgradeButton(false);
            this.successOrFailureToUpgradeWeaponTxt.text = "MAX";
            Debug.Log("Max Weapon");
        } else {
            StartCoroutine(DeactiveUpgradeButtonForMoment());
        }
        UpgradeWeapon(_level, _exp);
    }

    private IEnumerator DeactiveUpgradeButtonForMoment() {
        SetActiveUpgradeButton(false);
        yield return new WaitForSecondsRealtime(1f);
        SetActiveUpgradeButton(true);
    }

    private void SetActiveUpgradeButton(bool _isActive) {
        this.weaponSoulIcon.gameObject.SetActive(_isActive);
        this.weaponSoulTxt.gameObject.SetActive(_isActive);
        this.successOrFailureToUpgradeWeaponTxt.gameObject.SetActive(!_isActive);
        this.weaponExpBtn.interactable = _isActive;
    }

    private void UpgradeWeapon(int _level, int _exp) {
        MoneyManager.instance.UseSoul(GameDatabase.instance.playerWeapons[_level].cost);
        int afterExp = _exp + GameDatabase.instance.playerWeapons[_level].cost;
        PlayerPrefs.SetInt("exp", afterExp);
        AudioManager.instance.PlaySFX("weaponUpgrade");
        WeaponPnlUpdate(afterExp, true);
    }

    int WeaponPnlUpdate(int exp, bool _isCheckButton)
    {
        int remain;
        PlayerWeaponData data = GameManager.instance.GetPlayerWeapon(exp, out remain);
        if(_isCheckButton) {
            if(data.level == GameDatabase.instance.playerWeapons.Length-1)
            {
                SetActiveUpgradeButton(false);
                this.successOrFailureToUpgradeWeaponTxt.text = "MAX";
                Debug.Log("Max Weapon");
            }
            else
            {
                SetActiveUpgradeButton(true);
                weaponSoulTxt.text = data.cost.ToString();
            }
        }
        
        weaponLevelTxt.text = string.Format("{0} Level", data.level);

        weaponId = data.id;
        if (weaponId != _weaponId)
        {
            _weaponId = weaponId;
            Unit player = BoardManager.instance.player;
            WeaponData weapon = GameDatabase.instance.weapons[weaponId];
            if (player)
            {
                player.data.weapon = data.id;
                player.attackable.Init((WeaponData)GameDatabase.instance.weapons[data.id].Clone());
            }
            switch(GameManager.language)
            {
                default:
                    weaponDPSTxt.text = "DPS";
                    weaponRangeTxt.text = "Range";
                    break;
                case Language.Korean:
                    weaponDPSTxt.text = "피해량";
                    weaponRangeTxt.text = "사거리";
                    break;
            }
            weaponDPSFigureTxt.text = GetWeaponDPS(weapon).ToString("##0.##");
            weaponRangeFigureTxt.text = weapon.attackDistance.ToString("##0.##");
            this.weaponIconImage.sprite = data.Icon;
            weaponNameTxt.text = GetWeaponDescription(data);
        }
        return data.level;
    }

    string GetWeaponDescription(PlayerWeaponData data)
    {
        string description;
        if (data.description.Length > (int)GameManager.language)
            description = data.description[(int)GameManager.language];
        else if (data.description.Length > 1)
            description = data.description[0];
        else
            description = string.Empty;
        return description;
    }

    float GetWeaponDPS(WeaponData data)
    {
        float DPS = GetBulletDPS(GameDatabase.instance.bullets[data.startBulletId]);
        for (int i = 0; i < data.children.Length; i++)
            DPS += GetBulletDPS(GameDatabase.instance.bullets[data.children[i].bulletId]);
        return DPS;
    }

    float GetBulletDPS(BulletData data)
    {
        float DPS = 0;
        if (data.pierce == 1)
            DPS = data.dmg;
        else
            DPS = 1 / Mathf.Max(data.delay, 0.01f) * data.dmg;
        for (int i = 0; i < data.children.Length; i++)
            DPS += GetBulletDPS(GameDatabase.instance.bullets[data.children[i].bulletId]);
        return DPS;
    }

    public int GetWeaponLevel(int exp, out int remain)
    {
        remain = 0;
        for (int i = 0; i < GameDatabase.instance.playerWeapons.Length; i++)
        {
            exp -= GameDatabase.instance.playerWeapons[i].cost;
            if (exp < 0)
            {
                remain = GameDatabase.instance.playerWeapons[i].cost + exp;
                return GameDatabase.instance.playerWeapons[i].level;
            }
        }
        return -1;
    }
    #endregion

    #region soul
    [Header("About SoulShop")]
    public TextMeshProUGUI soulRefRateNameTxt;
    public TextMeshProUGUI preSoulRefRateValueTxt;
    public TextMeshProUGUI nextSoulRefRateValueTxt;
    public TextMeshProUGUI soulRefRateBtnTxt;

    public TextMeshProUGUI adNameTxt;
    public TextMeshProUGUI adValueTxt;
    public Button soulRefRateBtn;

    public TextMeshProUGUI BuyRefiningRateForJewel_NameText;
    public TextMeshProUGUI BuyRefiningRateForJewel_Context;
    public Button BuyRefiningRateForJewelButton;
    public TextMeshProUGUI BuyRefiningRateForJewelButtonText;
    public TextMeshProUGUI SoulPurchaseText;

    public List<int> BuySoulAmountList = new List<int>();
    public List<int> CostJewelListForBuySoul = new List<int>();
    public List<OnMouseButton> BuySoulButtonList = new List<OnMouseButton>();
    public List<TextMeshProUGUI> BuySoulButtonTextList = new List<TextMeshProUGUI>();

    public void RefiningRateUpgrade()
    {
        float rate = MoneyManager.instance.refiningRate;
        rate = Mathf.Round(rate * 100) * 0.01f;
        int amount = GetRefiningRateCost(rate);
        Debug.Log("rate:" + rate + " amount:" + amount);
        if (MoneyManager.instance.UseSoul(amount))
        {
            float r = rate + 0.01f;
            r = Mathf.Round(r * 100) * 0.01f;
            MoneyManager.instance.refiningRate = r;
            RefiningRateTxtUpdate();
            soulRefRateBtnTxt.text = "Done";
            StartCoroutine(LockSoulRefRateBtn());
        }
        else
        {
            soulRefRateBtnTxt.text = "Fail";
            StartCoroutine(LockSoulRefRateBtn());
        }

        if (MoneyManager.instance.refiningRate >= 2f)
        {
            soulRefRateBtnTxt.text = "Max";
            soulRefRateBtn.interactable = false;
        }
    }

    public float GetNextRefiningRate() {
        float rate = MoneyManager.instance.refiningRate;
        rate = Mathf.Round(rate * 100) * 0.01f;
        float nextRefineRate = rate + 0.01f;
        nextRefineRate = Mathf.Round(nextRefineRate * 100) * 0.01f;

        return nextRefineRate;
    }

    /// <summary>
    /// 소울 정제율 비용을 구하는 함수
    /// </summary>
    /// <param name="rate"></param>
    /// <returns></returns>
    public int GetRefiningRateCost(float rate)
    {
        rate -= 1f;
        if (rate <= 0.3f)
            return 100;
        int delta = 0;
        if (rate <= 0.35f)
            delta = 10;
        else if (rate <= 0.4f)
            delta = 50;
        else if (rate <= 0.45f)
            delta = 100;
        else if (rate <= 0.5f)
            delta = 500;
        else if (rate <= 0.55f)
            delta = 1000;
        else if (rate <= 0.6f)
            delta = 5000;
        else if (rate <= 0.65f)
            delta = 10000;
        else if (rate <= 0.7f)
            delta = 20000;

        rate = Mathf.Round((rate - 0.01f) * 100) * 0.01f;
        return GetRefiningRateCost(rate) + delta;
    }

    [ContextMenu("Test")]
    public void Test()
    {
        //for (float rate = 0.3f; rate + 0.01f <= 0.7f; rate += 0.01f)
        //{
        //    int result = GetRefiningRateCost(rate);
        //    Debug.Log(rate + " " + result);
        //}
        PlayerPrefs.SetFloat("exp", 0);
    }

    ///// <summary>
    ///// 소울 정제율 초기화
    ///// </summary>
    //public void ResetRefiningRateUpgrade()
    //{
    //    if (PlayerPrefs.GetInt("isRefiningRateReset") == 1)
    //        return;

    //    float rate = MoneyManager.instance.refiningRate;
    //    int amount = (int)(Mathf.Round(rate * 1000));
    //    int result = 0;
    //    for(int i = 300; i < amount; i += 10)
    //    {
    //        result += i;
    //    }

    //    Debug.Log("rate:" + rate + " result:" + result);

    //    //MoneyManager.instance.AddSoul(result);
    //    MoneyManager.instance.refiningRate = 1f;
    //    PlayerPrefs.SetInt("isRefiningRateReset", 1);
        
    //}    

    IEnumerator LockSoulRefRateBtn()
    {
        soulRefRateBtn.interactable = false;
        float t = 1;
        do
        {
            yield return null;
            t -= Time.unscaledDeltaTime;
        } while (t > 0);

        if (MoneyManager.instance.refiningRate < 2f)
        {
            soulRefRateBtn.interactable = true;
            float rate = MoneyManager.instance.refiningRate;
            rate = Mathf.Round(rate * 100) * 0.01f;
            this.soulRefRateBtnTxt.text = string.Format("<sprite=0> {0}", GetRefiningRateCost(rate));
        }
    }

    public void BuyRefiningRateForJewel() {
        switch(PlayerPrefs.GetInt("BuyRefiningRateIdxForJewel")) {
            case 0:
                if(MoneyManager.instance.UseJewel(100)) {
                    MoneyManager.instance.refiningRate = 2f;
                    PlayerPrefs.SetInt("BuyRefiningRateIdxForJewel", 1);
                    StartCoroutine(LockBuyRefiningRateForJewelButton(true));
                    soulRefRateBtnTxt.text = "Max";
                    soulRefRateBtn.interactable = false;
                } else {
                    StartCoroutine(LockBuyRefiningRateForJewelButton(false));
                }
            break;
            case 1:
                if(MoneyManager.instance.UseJewel(100)) {
                    MoneyManager.instance.maxRefiningRate = 4f;
                    PlayerPrefs.SetInt("BuyRefiningRateIdxForJewel", 2);
                    StartCoroutine(LockBuyRefiningRateForJewelButton(true));
                } else {
                    StartCoroutine(LockBuyRefiningRateForJewelButton(false));
                }
            break;
        }
        UpdateBuyRefiningRateForJewelPanel();
        RefiningRateTxtUpdate();
    }

    public IEnumerator LockBuyRefiningRateForJewelButton(bool _isPurchased) {
        if(_isPurchased) {
            this.BuyRefiningRateForJewelButtonText.text = "Done";
        } else {
            this.BuyRefiningRateForJewelButtonText.text = "Fail";
        }

        this.BuyRefiningRateForJewelButton.interactable = false;

        yield return new WaitForSecondsRealtime(1f);

        if(PlayerPrefs.GetInt("BuyRefiningRateIdxForJewel") != 2) {
            this.BuyRefiningRateForJewelButton.interactable = true;
            this.BuyRefiningRateForJewelButtonText.text = string.Format("<sprite=0> 100");
        } else {
            this.BuyRefiningRateForJewelButtonText.text = "Max";
        }
    }

    public void RefiningRateTxtUpdate()
    {
        this.preSoulRefRateValueTxt.text = (MoneyManager.instance.refiningRate * 100).ToString();
        int nextRefineRate = (int)(GetNextRefiningRate() * 100f);
        if(nextRefineRate < 200) {
            this.nextSoulRefRateValueTxt.text = (GetNextRefiningRate() * 100).ToString();
        } else {
            this.nextSoulRefRateValueTxt.text = this.preSoulRefRateValueTxt.text;
        }

        float rate = MoneyManager.instance.refiningRate;
        rate = Mathf.Round(rate * 100) * 0.01f;
        this.soulRefRateBtnTxt.text = string.Format("<sprite=0> {0}", GetRefiningRateCost(rate));
    }

    public void RefiningRateTxtNameUpdate()
    {
        switch (GameManager.language)
        {
            case Language.English:
                soulRefRateNameTxt.text = "Min Refining Rate";
                break;
            case Language.Korean:
                soulRefRateNameTxt.text = "최소 정제율";
                break;
        }
    }

    public void UpdateBuyRefiningRateForJewelPanel() {
        switch(PlayerPrefs.GetInt("BuyRefiningRateIdxForJewel")) {
            case 0:
                switch(GameManager.language) {
                    case Language.English:
                    this.BuyRefiningRateForJewel_NameText.text = "Refining Rate 200%";
                    break;
                case Language.Korean:
                    this.BuyRefiningRateForJewel_NameText.text = "정제율 200%";
                    break;
                }
                this.BuyRefiningRateForJewel_Context.text = string.Format("{0} <sprite=0> 200 %", (MoneyManager.instance.refiningRate * 100).ToString());
            break;
            case 1:
                switch(GameManager.language) {
                    case Language.English:
                    this.BuyRefiningRateForJewel_NameText.text = "Refining Rate";
                    break;
                case Language.Korean:
                    this.BuyRefiningRateForJewel_NameText.text = "정제율";
                    break;
                }
                this.BuyRefiningRateForJewel_Context.text = "200 ~ 400 %";
            break;
            case 2:
                switch(GameManager.language) {
                    case Language.English:
                    this.BuyRefiningRateForJewel_NameText.text = "Refining Rate";
                    break;
                case Language.Korean:
                    this.BuyRefiningRateForJewel_NameText.text = "정제율";
                    break;
                }
                this.BuyRefiningRateForJewel_Context.text = "200 ~ 400 %";
                this.BuyRefiningRateForJewelButtonText.text = string.Format("Max");
                this.BuyRefiningRateForJewelButton.interactable = false;
            break;
        }
    }

    public void ADLanguageUpdate()
    {
        switch (GameManager.language)
        {
            case Language.English:
                adNameTxt.text = "Refine";
                adValueTxt.text = "Watch Video";
                break;
            case Language.Korean:
                adNameTxt.text = "즉시 정제";
                adValueTxt.text = "광고 시청";
                break;
        }
    }

    public void UpdateSoulPurchaseText() {
        switch (GameManager.language)
        {
            case Language.English:
                SoulPurchaseText.text = "Soul Purchase";
                break;
            case Language.Korean:
                SoulPurchaseText.text = "소울 구매";
                break;
        }
    }

    public void BuySoul(int _idx) {
        int soulAmount = this.BuySoulAmountList[_idx];
        int costJewel = this.CostJewelListForBuySoul[_idx];
        Button buySoulButton = this.BuySoulButtonList[_idx];
        TextMeshProUGUI buySoulButtonText = this.BuySoulButtonTextList[_idx];

        if(MoneyManager.instance.UseJewel(costJewel)) {
            StartCoroutine(LockBuySoulButton(buySoulButton, buySoulButtonText, true));
            MoneyManager.instance.AddSoul(soulAmount);
        } else {
            StartCoroutine(LockBuySoulButton(buySoulButton, buySoulButtonText, false));
        }
    }
    
    public IEnumerator LockBuySoulButton(Button _lockButton, TextMeshProUGUI _buttonTxt, bool _isPurchased) {
        string originButtonTxt = _buttonTxt.text;
        if(_isPurchased) {
            _buttonTxt.text = "Done";
        } else {
            _buttonTxt.text = "Fail";
        }

        _lockButton.interactable = false;

        yield return new WaitForSecondsRealtime(1f);

        _lockButton.interactable = true;
        _buttonTxt.text = originButtonTxt;
    }
    #endregion

    #region diamond
    [Header("About SoulShop")]
    public TextMeshProUGUI DiamondPurchaseText;

    public void UpdateDiamondPurchaseText() {
        switch (GameManager.language)
        {
            case Language.English:
                DiamondPurchaseText.text = "Diamond Purchase";
                break;
            case Language.Korean:
                DiamondPurchaseText.text = "다이아몬드 구매";
                break;
        }
    }
    #endregion
}
