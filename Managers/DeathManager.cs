using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RogueNaraka.TierScripts;
using RogueNaraka.TimeScripts;
using RogueNaraka.TitleScripts;
using RogueNaraka.TheBackendScripts;
using RogueNaraka.RankingScripts;
using RogueNaraka.RollScripts;

public partial class DeathManager : MonoBehaviour
{
    const float UP_PER_EXP_SPEED = 0.5f;
    const float CHANGE_SCALE_OF_TIER_EMBLEM_SPEED = 2.5f;
    const float MAX_SCALE_OF_TIER = 1.5f;
    const int RESURECT_JEWEL_COST = 5;

    public static DeathManager instance;

    public GameObject deathPnl;
    public RectTransform GameResultPnl;

    public Image SoulRefiningPanel;

    public TextMeshProUGUI GameOverTxt;

    public TextMeshProUGUI soulRefiningRateTxt;
    public TextMeshProUGUI unSoulTxt;
    public TextMeshProUGUI soulTxt;
    public Button ResurectButton;
    public TextMeshProUGUI ResurrectButtonContext;
    public TextMeshProUGUI ResurectjewelCostTxt;

    public TitleManager TitleManager;

    [Header("Tier Object")]
    public Image PlayerTierEmblem;
    public TextMeshProUGUI PlayerTierText;

    [Header("Level And Exp Object")]
    public TextMeshProUGUI curLvTxt;
    public TextMeshProUGUI expNumTxt;
    public Image ExpGauge;

    [Header("Ranking Object")]
    public RankingInformationBar[] TopRankerInformationBar;
    public RankingInformationBar PlayerRankingInformationBar;
    public TextMeshProUGUI PlayerRankingText;
    public TextMeshProUGUI PlayerTopRankingPercentText;

    private List<int> huntedUnitNumList = new List<int>();
    public Canvas stageCanvas;

    private bool isClickableCloseBtn;

    public float tt { get; set; }

    void Awake()
    {
        instance = this;
        
        for(int i = 0; i < GameDatabase.instance.enemies.Count(); ++i) {
            this.huntedUnitNumList.Add(0);
        }

        ResurectjewelCostTxt.text = string.Format("<size=13><voffset=-0.05em><sprite=0></voffset></size> {0}", RESURECT_JEWEL_COST); 
        if(PlayerPrefs.GetInt("IsDeactiveResurectButton") == 1) {
            this.ResurectButton.interactable = false;
        }
    }

    public void SetDeathPnl(bool value)
    {
        deathPnl.SetActive(value);
        //CameraShake.instance.Shake(0.2f, 0.2f, 0.01f);
        if (value)
        {
            StartCoroutine(PumpCorou(this.GameOverTxt.rectTransform, 3, 0.5f));
        }
    }

    public void OnDeath()
    {
        Debug.Log("OnDeath");
        //RankManager.instance.SendPlayerRank();
        if(BoardManager.instance.player)
            GameManager.instance.Save();

        RankManager.instance.SendPlayerRank();

        SetDeathPnl(true);

        StartCoroutine(SoulPnlCorou(1));

        //GameManager.instance.SetSettingBtn(true);

        AudioManager.instance.PlayMusic(AudioManager.instance.GetRandomDeathMusic());

        stageCanvas.sortingLayerName = "UI";
    }

    IEnumerator SoulPnlCorou(float t)
    {
        this.isClickableCloseBtn = false;
        switch(GameManager.language) {
            case Language.English:
                this.ResurrectButtonContext.text = "Resurrect";
            break;
            case Language.Korean:
                this.ResurrectButtonContext.text = "부활";
            break;
        }

        //lv, 경험치 세팅
        int playerOriginLv = TierManager.Instance.PlayerLevel;
        double curExp = TierManager.Instance.CurrentExp;
        double maxExp = GameDatabase.instance.requiredExpTable[playerOriginLv - 1];
        this.curLvTxt.text = string.Format("Lv.{0}", playerOriginLv);
        this.expNumTxt.text = string.Format("{0}  /  {1}", (int)curExp, (int)maxExp);
        this.ExpGauge.fillAmount = (float)(curExp / maxExp);
        //Tier 세팅
        this.PlayerTierEmblem.sprite = TierManager.Instance.CurrentTier.emblem;
        this.PlayerTierText.text = string.Format("{0}{1} Tier"
            , TierManager.Instance.CurrentTier.type
            , TierManager.Instance.CurrentTier.tier_num != 0 ? TierManager.Instance.CurrentTier.tier_num.ToString() : "");
        //Backend ClearedStage 갱신
        if(TheBackendManager.Instance.gameObject.activeSelf) {
            TheBackendManager.Instance.UpdateRankData(PlayerPrefs.GetInt("stage") - 1);
            StartCoroutine(RefreshTopRankerInformationBoard());
        }
        //
        TierManager.Instance.SaveExp();
        tt = 1.5f;
        do
        {
            yield return null;
            tt -= TimeManager.Instance.UnscaledDeltaTime;
        } while (tt > 0);
        
        if(TheBackendManager.Instance.gameObject.activeSelf) {
            yield return new WaitUntil(() => !TheBackendManager.Instance.IsRefreshingPlayerRank);
        }
        do
        {
            yield return null;
            t -= Time.unscaledDeltaTime;
        } while (t > 0);

        SetSoulPnl(true);

        StartCoroutine(StartGainExpAnimation(playerOriginLv, curExp));
        if(TierManager.Instance.CheckIfTierHaveChanged()) {
            StartCoroutine(StartTierUpAnimation());
        }
        this.isClickableCloseBtn = true;
    }

    public void SetSoulPnl(bool value)
    {
        if(value)
        {
            float lastRefiningRate = PlayerPrefs.GetFloat("lastRefiningRate"); 
            float refiningRate = MoneyManager.instance.CheckRightRefiningRate(lastRefiningRate) ? lastRefiningRate : MoneyManager.instance.GetRandomRefiningRate();
            soulTxt.text = "0";
            unSoulTxt.text = "0";
            PlayerPrefs.SetFloat("lastRefiningRate", refiningRate);
            GameResultPnl.gameObject.SetActive(true);
            rate = refiningRate;
            StartCoroutine(SoulRefiningRateTxtCorou(refiningRate));
            StartCoroutine(PumpCorou(GameResultPnl, 0f, 0.25f));
            AdMobManager.instance.RequestRewardBasedVideo();
        }
        else
        {
            GameResultPnl.gameObject.SetActive(false);
            PlayerPrefs.SetFloat("lastRefiningRate", -1);
        }
    }

    IEnumerator SoulRefiningRateTxtCorou(float rate)
    {
        int intRate = (int)(rate * 100);
        float delay = 1f / intRate;
        for (int i = 0; i <= intRate; i++)
        {
            float t = delay;
            do
            {
                yield return null;
                t -= Time.unscaledDeltaTime;
            } while (t > 0);

            if (Input.anyKey)
                i = intRate;

            soulRefiningRateTxt.text = string.Format("{0}%", i);
        }

        int unSoul = MoneyManager.instance.unrefinedSoul;
        delay = 0.5f / intRate;
        for (int i = 0; i <= unSoul; i++)
        {
            float t = delay;
            do
            {
                yield return null;
                t -= Time.unscaledDeltaTime;
            } while (t > 0);

            if (Input.anyKey)
                i = unSoul;

            unSoulTxt.text = string.Format("{0}<size=12><sprite=0></size>", i);
        }

        int soul = (int)(unSoul * rate);
        delay = 0.5f / intRate;
        for (int i = 0; i <= soul; i++)
        {
            float t = delay;
            do
            {
                yield return null;
                t -= Time.unscaledDeltaTime;
            } while (t > 0);
            if (Input.anyKey)
                i = soul;

            soulTxt.text = string.Format("{0}<size=12><sprite=0></size>", i);
        }
    }

    bool isADReward;
    //-    public bool isClose { get; set; }
    //-
    //-    IEnumerator SoulAutoCloseCorou(float rate)
    //-    {
    //-        float t = 6;
    //-        do
    //-        {
    //-            yield return null;
    //-            t -= Time.unscaledDeltaTime* (isADActive? 0 : 1);
    //-            if(isADReward)
    //-            {
    //-                MoneyManager.instance.RefineSoul(rate* 2);
    //-                SetSoulPnl(false);
    //-                yield break;
    //-            }
    //-            if(isClose)
    //-            {
    //-                MoneyManager.instance.RefineSoul(rate);
    //-                SetSoulPnl(false);
    //-                isClose = false;
    //-                yield break;
    //-            }
    //-        } while (t > 0);
    //-
    //-        if (soulPnl.gameObject.activeSelf)
    //-        {
    //-            MoneyManager.instance.RefineSoul(rate);
    //-            SetSoulPnl(false);
    //-        }
    //-    }

    IEnumerator soulCorou;
    IEnumerator SoulCorou()
    {
        while(true)
        {
            yield return null;
            if(isADReward)
            {
                yield return null;
                isADReward = false;
                MoneyManager.instance.RefineSoul(rate * 2);
                SetSoulPnl(false);
                break;
            }
        }
        soulCorou = null;
    }

    float rate;

    public void OnSoulRefiningRatePnlClose()
    {
        if(!this.isClickableCloseBtn) {
            return;
        }
        if(soulCorou != null) {
            StopCoroutine(soulCorou);
        }
        MoneyManager.instance.RefineSoul(rate);
        PlayerPrefs.SetInt("isRun", 0);
        PlayerPrefs.SetInt("IsDeactiveResurectButton", 0);
        //SetSoulPnl(false);
    }

    public void ClickOnDeathADButton() {
        if(!this.isClickableCloseBtn) {
            return;
        }

        StartCoroutine(CheckToReciveAdReward());
    }

    public IEnumerator CheckToReciveAdReward() {
        #if !UNITY_EDITOR
        yield return new WaitUntil(() => AdMobManager.instance.Ad_State != AD_State.PLAYING);
        #endif
        yield return null;
        
        if(AdMobManager.instance.Ad_State == AD_State.DONE) {
            MoneyManager.instance.RefineSoul(rate * 2);
        } else {
            MoneyManager.instance.RefineSoul(rate);
        }
        isADReward = false;
        this.SoulRefiningPanel.gameObject.SetActive(false);
        PlayerPrefs.SetInt("IsDeactiveResurectButton", 0);
        PlayerPrefs.SetInt("isRun", 0);
        yield return null;
    }

    public void OnADReward()
    {
        isADReward = true;
    }

    public void ReGame()
    {
        this.GameResultPnl.gameObject.SetActive(false);
        ResurectButton.interactable = true;
        if (StageSkipManager.Instance.GetSkipableStage() != 1) {
            StageSkipManager.Instance.SetStageSkipPnl(true);
        } else {
            EndGame();
            PlayerPrefs.SetInt("isRun", 0);
            GameManager.instance.Load();
        }
    }

    public void EndGame()
    {
        SetDeathPnl(false);
        RollManager.instance.IsFirstRoll = true;

        RageManager.instance.SetActiveSmallRageBtn(false);
        //AudioManager.instance.PlayMusic(AudioManager.instance.GetRandomMainMusic());
        
        SkillManager.instance.ResetSave();
        Item.instance.ResetSave();
        Item.instance.InitItem();

        BoardManager.instance.ClearStage();

        PlayerPrefs.SetFloat("lastRefiningRate", 0);

        Fillable.playerHp.img.fillAmount = 1;
        Fillable.playerMp.img.fillAmount = 1;

        GameManager.instance.SetSettingBtn(true);

        stageCanvas.sortingLayerName = "Wall";
    }

    public void ClickOnResurectButton() {
        if(MoneyManager.instance.jewel < RESURECT_JEWEL_COST) {
            StartCoroutine(DeactiveResurrectButtonForMoment());
        } else {
            MoneyManager.instance.UseJewel(RESURECT_JEWEL_COST);
            ResurectButton.interactable = false;
            PlayerPrefs.SetInt("IsDeactiveResurectButton", 1);
            this.GameResultPnl.gameObject.SetActive(false);
            this.SoulRefiningPanel.gameObject.SetActive(false);
            PlayerPrefs.SetFloat("lastRefiningRate", -1);
            SetDeathPnl(false);
            PlayerPrefs.SetInt("isRun", 1);
            Vector2 resurectPos = GameManager.instance.player.transform.position;
            GameManager.instance.ResurrectPlayer();
            GameManager.instance.player.transform.position = resurectPos;
            GameManager.instance.Save();
        }
    }

    public IEnumerator DeactiveResurrectButtonForMoment() {
        this.ResurectButton.interactable = false;
        string originButtonText = ResurectjewelCostTxt.text;
        switch(GameManager.language) {
            case Language.English:
                ResurectjewelCostTxt.text = "Shotrage";
            break;
            case Language.Korean:
                ResurectjewelCostTxt.text = "부족";
            break;
        }

        yield return new WaitForSecondsRealtime(1f);

        this.ResurectButton.interactable = true;
        ResurectjewelCostTxt.text = originButtonText;
    }

    public void ClickOnMainButton() {
        this.GameResultPnl.gameObject.SetActive(false);
        ResurectButton.interactable = true;
        PlayerPrefs.SetInt("IsDeactiveResurectButton", 0);
        SetDeathPnl(false);
        RollManager.instance.IsFirstRoll = true;
        RageManager.instance.SetActiveSmallRageBtn(false);
        SkillManager.instance.ResetSave();
        Item.instance.ResetSave();
        Item.instance.InitItem();

        BoardManager.instance.ClearStage();

        PlayerPrefs.SetFloat("lastRefiningRate", 0);

        Fillable.playerHp.img.fillAmount = 1;
        Fillable.playerMp.img.fillAmount = 1;

        GameManager.instance.SetSettingBtn(true);

        stageCanvas.sortingLayerName = "Wall";

        PlayerPrefs.SetInt("isRun", 0);

        this.TitleManager.gameObject.SetActive(true);
        this.TitleManager.UpdateText();
    }

    public void OpenSoulShop()
    {
        SoulShopManager.instance.SetSoulShop(true);
    }

    IEnumerator PumpCorou(RectTransform rect, float size, float t)
    {
        RectTransform imgRect = rect;
        Vector3 origin = imgRect.localScale;
        imgRect.localScale = new Vector3(size, size, 0);
        float tt = 0;
        while (tt < t)
        {
            yield return null;
            tt += Time.unscaledDeltaTime;
            imgRect.localScale = Vector3.Lerp(imgRect.localScale, origin, tt / t);
        }
        imgRect.localScale = origin;
    }

    public List<int> GetHuntedUnitNumList() {
        return this.huntedUnitNumList.ToList();
    }

    public int GetHuntedUnitNum(int _unitId) {
        if(_unitId < 0 || _unitId >= this.huntedUnitNumList.Count) {
            throw new System.ArgumentException(string.Format("Unit Id is Incorrect! Id : {0}", _unitId));
        }
        return this.huntedUnitNumList[_unitId];
    }

    public void ReceiveHuntedUnit(int _unitId) {
        this.huntedUnitNumList[_unitId] += 1;
    }

    public void ClearHuntedUnitList() {
        for(int i = 0; i < this.huntedUnitNumList.Count; ++i) {
            this.huntedUnitNumList[i] = 0;
        }
    }

    private IEnumerator StartTierUpAnimation() {
        yield return new WaitForSecondsRealtime(0.3f);

        this.PlayerTierEmblem.transform.localScale = new Vector2(MAX_SCALE_OF_TIER, MAX_SCALE_OF_TIER);
        this.PlayerTierEmblem.sprite = TierManager.Instance.CurrentTier.emblem;
        this.PlayerTierText.text = string.Format("{0}{1} Tier"
            , TierManager.Instance.CurrentTier.type
            , TierManager.Instance.CurrentTier.tier_num != 0 ? TierManager.Instance.CurrentTier.tier_num.ToString() : "");
        //Smaller
        WaitForFixedUpdate waitFixedFrame = new WaitForFixedUpdate();
        while(this.PlayerTierEmblem.transform.localScale.x >= 1.0f) {
            this.PlayerTierEmblem.transform.localScale -= new Vector3(1f, 1f, 0f) * CHANGE_SCALE_OF_TIER_EMBLEM_SPEED * TimeManager.Instance.FixedDeltaTime;
            yield return waitFixedFrame;
        }
        this.PlayerTierEmblem.transform.localScale = Vector2.one;
    }

    private IEnumerator StartGainExpAnimation(int _originLv, double _originExp) {
        double upExpPerSecond = (TierManager.Instance.TotalGainExpInGame) * UP_PER_EXP_SPEED;
        double min_upExpPerSecond = upExpPerSecond * 0.08d;
        double maxExp = GameDatabase.instance.requiredExpTable[_originLv - 1];
        double remainUpExp = TierManager.Instance.TotalGainExpInGame;

        WaitForFixedUpdate waitFixedFrame = new WaitForFixedUpdate();

        while(_originLv < TierManager.Instance.PlayerLevel 
            || _originExp < TierManager.Instance.CurrentExp) {
            yield return waitFixedFrame;
            if(remainUpExp <= TierManager.Instance.TotalGainExpInGame * 0.2d) {
                if(upExpPerSecond <= min_upExpPerSecond) {
                    upExpPerSecond = min_upExpPerSecond;
                } else {
                    upExpPerSecond *= 0.95d;
                }
            }
            double upExp = upExpPerSecond * TimeManager.Instance.FixedDeltaTime;
            _originExp += upExp;
            remainUpExp -= upExp;
            if(_originExp >= maxExp) {
                _originLv++;
                _originExp = 0.0d;
                maxExp = GameDatabase.instance.requiredExpTable[_originLv - 1];
                this.curLvTxt.text = _originLv.ToString();
            }
            this.expNumTxt.text = string.Format("{0}  /  {1}", (int)_originExp, (int)maxExp);
            this.ExpGauge.fillAmount = (float)(_originExp / maxExp);
        }
        this.expNumTxt.text = string.Format("{0}  /  {1}", (int)TierManager.Instance.CurrentExp
            , (int)GameDatabase.instance.requiredExpTable[TierManager.Instance.PlayerLevel - 1]);
        this.ExpGauge.fillAmount = (float)(TierManager.Instance.CurrentExp / GameDatabase.instance.requiredExpTable[TierManager.Instance.PlayerLevel - 1]);

        SoulRefiningPanel.gameObject.SetActive(true);
        StartCoroutine(PumpCorou(SoulRefiningPanel.rectTransform, 0f, 0.25f));
    }

    //Coroutine
    private IEnumerator RefreshTopRankerInformationBoard() {
        TheBackendManager.Instance.RefreshTopRankerData(4);
        yield return new WaitUntil(() => !TheBackendManager.Instance.IsRefreshingTopRankerData);

        for(int i = 0; i < this.TopRankerInformationBar.Length; ++i) {
            float topPercent = i / TheBackendManager.Instance.TotalRankedUserNum * 100.0f;
            this.TopRankerInformationBar[i].SetInformation(
                i + 1,
                TheBackendManager.Instance.TopRankerDataList[i].nickName,
                TheBackendManager.Instance.TopRankerDataList[i].score);
        }

        this.PlayerRankingInformationBar.SetInformation(
            TheBackendManager.Instance.TopRankingOfPlayer
            , TheBackendManager.Instance.UserNickName
            , TheBackendManager.Instance.ClearedStageForRank);
        switch(GameManager.language) {
            case Language.English:
                this.PlayerRankingText.text = string.Format("Top  {0} Rank", TheBackendManager.Instance.TopRankingOfPlayer);
            break;
            case Language.Korean:
                this.PlayerRankingText.text = string.Format("상위  {0} 위", TheBackendManager.Instance.TopRankingOfPlayer);
            break;
        }
    }
}