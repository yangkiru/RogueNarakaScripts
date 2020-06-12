﻿//#define DELAY
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GoogleMobileAds.Api;
using RogueNaraka.UnitScripts;
using RogueNaraka.TierScripts;
using TMPro;
using System.Linq;
using RogueNaraka.RollScripts;
using RogueNaraka.TitleScripts;

public class GameManager : MonoBehaviour {

    public static Language language;

    [SerializeField][ReadOnly]
    public static GameManager instance = null;
    public BoardManager boardManager;
    public MoneyManager moneyManager;
    public LevelUpManager levelUpManager;
    public SoulShopManager soulShopManager;
    public SkillManager skillManager;
    public TitleManager titleManager;
    public DeathEffectManager deathEffectPool;
    public GameObject settingPnl;
    public GameObject PauseCanvas;
    public Button PauseBtn;
    public Button settingBtn;
    public Button RemoveAdButton;

    public Camera mainCamera;

    public Button[] languageBtns;

    public Unit player
    {
        get { return boardManager.player; }
    }
    
    public Button cancelBtn;//stat Upgrade

    public float autoSaveTime = 1;

    public bool isPause
    { get { return _isPause; } }
    [SerializeField][ReadOnly]
    private bool _isPause;

    public bool IsFirstGame { get { return PlayerPrefs.GetInt("isFirstGame") == 1; } set { PlayerPrefs.SetInt("isFirstGame", value ? 1 : 0); } }

    private IEnumerator autoSaveCoroutine;

    private void Awake()
    {
#if !UNITY_EDITOR
        Debug.unityLogger.logEnabled = false;
#endif
        instance = this;

        string lang = PlayerPrefs.GetString("language");
        try
        {
            if (lang == string.Empty)
            {
                Debug.Log("Language Empty");
                language = (Language)System.Enum.Parse(typeof(Language), Application.systemLanguage.ToString());
            }
            else
            {
                Debug.Log("Language " + lang);
                language = (Language)System.Enum.Parse(typeof(Language), StripAlpha(lang));
            }
            SetLanguage((int)language);
        }
        catch
        {
            Debug.Log("Can't not find language : " + lang);
            SetLanguage(0);
        }
        Application.targetFrameRate = 60;

        //Set AdButton
        if(PlayerPrefs.GetInt("isRemoveAds") == 1) {
            this.RemoveAdButton.interactable = false;
        }
    }

    public void SetLanguage(int num = -1)
    {
        if (num != -1)
            language = (Language)num;
        string str = language.ToString();
        PlayerPrefs.SetString("language", str);
        Debug.Log(language);
        int lang = (int)language;
        for (int i = 0; i < languageBtns.Length; i++)
        {
            Image languageBtnImage = languageBtns[i].GetComponent<Image>();
            if (i == lang) {
                languageBtnImage.color = new Color(languageBtnImage.color.r, languageBtnImage.color.g, languageBtnImage.color.b, 1f);
            } else {
                languageBtnImage.color = new Color(languageBtnImage.color.r, languageBtnImage.color.g, languageBtnImage.color.b, 0.4f);
            }
        }

        this.titleManager.UpdateText();
    }

    public string StripAlpha(string self)
    {
        return new string(self.Where(c => char.IsLetter(c)).ToArray());
    }

    private void RandomStat(Stat stat)
    {
        int statPoint = stat.statPoints;
        Debug.Log("RandomStat " + stat.statPoints);
        while(statPoint > 0)
        {
            int type = Random.Range(0, (int)STAT.MR + 1);
            if (stat.AddOrigin((STAT)type, 1))
            {
                //Debug.Log(((STAT)type).ToString());
                statPoint--;
            }
            else
            {
                int origin = (int)stat.sumOrigin;
                int max = (int)stat.sumMax;
                Debug.Log(string.Format("origin:{0},max:{1}", origin, max));
                if (origin >= max)
                    break;
            }
        }
    }

    public IEnumerator AutoSave(float time)
    {
        while (true)
        {
            Save();
            float t = 0;
            while (t < time)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }

    public void Save()
    {
        moneyManager.SaveSoul();
        moneyManager.SaveJewel();

        EffectData[] effectDatas = new EffectData[player.effectable.effects.Count];
        for (int i = 0; i < effectDatas.Length; i++)
        {
            effectDatas[i] = (EffectData)(player.effectable.effects[i].data.Clone());
        }
        PlayerPrefs.SetString("effect", JsonHelper.ToJson<EffectData>(effectDatas));

        PlayerPrefs.SetString("stat", Stat.StatToJson(player.data.stat));
        boardManager.Save();
        PlayerPrefs.SetInt("weapon", player.attackable.weapon.id);
        SkillManager.instance.Save();
        Item.instance.Save();

        //Debug.Log("Saved");
    }

    public void ResetData()
    {
        //UnityEngine.PlayerPrefs.DeleteAll();

        PlayerPrefs.SetInt(Application.version, 1);

        moneyManager.ResetData();
        SkillManager.instance.ResetSave();
        Item.instance.ResetSave();

        UnitData playerBase = GameDatabase.instance.playerBase;

        Stat dbStat = (Stat)playerBase.stat.Clone();
        Stat.StatToData(dbStat);
       
        PlayerPrefs.SetString("stat", Stat.StatToJson(dbStat));
        PlayerPrefs.SetString("randomStat", string.Empty);
        
        PlayerPrefs.SetString("effect", string.Empty);
        PlayerPrefs.SetInt("isRun", 0);
        LevelUpManager.IsLevelUp = false;
        MoneyManager.instance.refiningRate = 1f;

        PlayerPrefs.SetInt("exp", 0);

        PlayerPrefs.SetInt("stage", 1);

        RollManager.instance.ResetData();

        PlayerPrefs.SetInt("isReset", 1);

        PlayerPrefs.SetInt("isFirstGame", 1);
    }

    public void RunGame(Stat stat)
    {
        Debug.Log("RunGame");
        moneyManager.Load();
        //DeathManager.instance.LoadRefiningData();
        
        UnitData playerData = (UnitData)GameDatabase.instance.playerBase.Clone();
        playerData.weapon = GetPlayerWeapon(PlayerPrefs.GetInt("exp")).id;
        playerData.stat = stat;
        string effect = PlayerPrefs.GetString("effect");
        if (effect != string.Empty)
            playerData.effects = JsonHelper.FromJson<EffectData>(effect);

        boardManager.SetStage(PlayerPrefs.GetInt("stage"));
        
        boardManager.SpawnPlayer(playerData);

        SkillManager.instance.Load();
        Item.instance.Load();

        RageManager.instance.CheckRage();

        if (LevelUpManager.IsLevelUp)
        {
            levelUpManager.LevelUp();
        }
        else if (RollManager.instance.IsFirstRoll)
        {
            SetPause(true);
            if(IsFirstGame)
                RollManager.instance.FirstGame();
            else
                RollManager.instance.FirstRoll();
            //AudioManager.instance.PlayMusic(AudioManager.instance.GetRandomMainMusic());
        }
        else
        {
            boardManager.InitBoard();
        }

        if (playerData.stat.currentHp <= 0)
        {
            player.gameObject.SetActive(false);
            DeathManager.instance.OnDeath();
            SetSettingBtn(false);
        }
    }

    public void ResurrectPlayer() {
        UnitData playerData = (UnitData)GameDatabase.instance.playerBase.Clone();
        Stat stat = Stat.JsonToStat(PlayerPrefs.GetString("stat"));
        stat.currentHp = stat.hpMax;
        playerData.weapon = GetPlayerWeapon(PlayerPrefs.GetInt("exp")).id;
        playerData.stat = stat;
        string effect = PlayerPrefs.GetString("effect");
        if (effect != string.Empty)
            playerData.effects = JsonHelper.FromJson<EffectData>(effect);
        AudioManager.instance.PlayMusic(AudioManager.instance.currentMainMusic);
        boardManager.SpawnPlayer(playerData);
    }

    public void LoadInit(int stage = 1)
    {
        UnitData playerBase = (UnitData)GameDatabase.instance.playerBase.Clone();
        Stat stat = Stat.JsonToStat(PlayerPrefs.GetString("stat"));
        stat.SetOrigin(playerBase.stat);
        Stat randomStat = new Stat();
        randomStat.SetMax(stat);
        for (int i = 0; i < (int)STAT.MR + 1; i++)
            randomStat.AddMax((STAT)i, -playerBase.stat.GetOrigin((STAT)i));
        randomStat.statPoints = stat.statPoints;
        RandomStat(randomStat);

        Stat.StatToData(randomStat, "randomStat");
        Stat.StatToData(stat);
        RageManager.instance.ResetSave();

        moneyManager.Load();

        BoardManager.instance.Stage = stage;
  
        skillManager.ResetSave();
        Item.instance.ResetSave();

        RollManager.instance.IsFirstRoll = true;
    }

    public void Load()
    {            
        if (PlayerPrefs.GetInt("isReset") == 0)//reset
        {
            Debug.Log("Load First");
            ResetData();
        }
        if (PlayerPrefs.GetInt("isRun") == 1)
        {
            Debug.Log("Open Run");
            Stat stat = Stat.JsonToStat(PlayerPrefs.GetString("stat"));
            Stat randomStat = Stat.DataToStat("randomStat");

            if (randomStat != null)
            {
                StatOrbManager.instance.SetActive(true, randomStat, stat);
                AudioManager.instance.PlayMusic(AudioManager.instance.GetRandomMainMusic());
                moneyManager.Load();
                //StatOrbManager.OnLastOverflow에서 RunGame() 호출
            }
            else
                RunGame(stat);
        }
        else//Init Run
        {
            Debug.Log("RunReset");
            PlayerPrefs.SetInt("isRun", 1);
            if (StageSkipManager.Instance.GetSkipableStage() != 1) {
                StageSkipManager.Instance.SetStageSkipPnl(true);
            } else {
                LoadInit();
                Load();
            }
        }
    }

    public PlayerWeaponData GetPlayerWeapon(int exp)
    {
        for (int i = 0; i < GameDatabase.instance.playerWeapons.Length; i++)
        {
            exp -= GameDatabase.instance.playerWeapons[i].cost;
            if (exp < 0 || i == GameDatabase.instance.playerWeapons.Length - 1)
            {
                return GameDatabase.instance.playerWeapons[i];
            }
        }
        return null;
    }

    public PlayerWeaponData GetPlayerWeapon(int exp, out int remain)
    {
        int i;
        remain = 0;
        for(i = 0; i < GameDatabase.instance.playerWeapons.Length; i++)
        {
            exp -= GameDatabase.instance.playerWeapons[i].cost;
            if (exp < 0)
            {
                remain = GameDatabase.instance.playerWeapons[i].cost + exp;
                return GameDatabase.instance.playerWeapons[i];
            }
        }
        return GameDatabase.instance.playerWeapons[i - 1];
    }

    public void SetPause(bool value)
    {
        Debug.Log(string.Format("SetPause {0}", value));
        _isPause = value;
        if (value)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }

    public void SetSettingBtn(bool value)
    {
        settingBtn.gameObject.SetActive(value);
        PauseCanvas.gameObject.SetActive(!value);
    }

    public void SetPauseBtn(bool value)
    {
        settingBtn.gameObject.SetActive(!value);
        PauseCanvas.gameObject.SetActive(value);
    }

    public void SetSettingPnl(bool value)
    {
        settingPnl.SetActive(value);

        TutorialManager.instance.isPause = value;
        if (StatOrbManager.instance.pnl.activeSelf && !TutorialManager.instance.isPlaying)
        {
            GameManager.instance.SetPause(value);
        }
    }

    public Vector2 GetMousePosition()
    {
        var mousePos = Input.mousePosition;
        if (mousePos.x < 0 || mousePos.x >= Screen.width || mousePos.y < 0 || mousePos.y >= Screen.height)
        {
            return boardManager.player.cachedTransform.position - new Vector3(0, Pointer.instance.offset);
        }

        Vector2 vec = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        return vec;
    }

    public void InitDropdown(TMP_Dropdown dropdown)
    {
        dropdown.value = (int)language;
    }

    public void Resume()
    {
        if (!RollManager.instance.rollPnl.activeSelf)
            SetPause(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

#if !UNITY_EDITOR
    public void OnApplicationFocus(bool focus)
    {
        if (!focus && !this.isPause && !this.titleManager.gameObject.activeSelf && !DeathManager.instance.deathPnl.gameObject.activeSelf) {
            this.PauseBtn.onClick.Invoke();
        }
    }
#endif
}
