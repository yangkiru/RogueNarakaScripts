using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Purchasing;

public class MoneyManager : MonoBehaviour {

    public TextMeshProUGUI soulTxt;
    public TextMeshProUGUI unrefinedSoulTxt;
    public TextMeshProUGUI jewelTxt;

    private Vector2 soulSpawnPosition;

    public int unrefinedSoul
    {
        get { return _unrefinedSoul; }
    }
    [SerializeField][ReadOnly]
    private int _unrefinedSoul;
    public int soul
    {
        get { return _soul; }
    }
    [SerializeField][ReadOnly]
    private int _soul;
    public int jewel
    {
        get { return _jewel; }
    }
    [SerializeField][ReadOnly]
    private int _jewel;
    public float RemainSoul { get { return remainSoul; } set { remainSoul = value; } }
    private float remainSoul = 0;

    public float refiningRate { get { return PlayerPrefs.GetFloat("refiningRate"); } set { PlayerPrefs.SetFloat("refiningRate", value); } }
    public float maxRefiningRate { get { return PlayerPrefs.GetFloat("maxRefiningRate"); } set { PlayerPrefs.SetFloat("maxRefiningRate", value); } }

    public static MoneyManager instance = null;

    private bool loaded = false;
    public bool Loaded { get {return this.loaded; } }

    public void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        this.soulSpawnPosition = new Vector2(-0.1f, 0.0f);
    }

    public void SetUnrefinedSoul(int value)
    {
        _unrefinedSoul = value;
        MoneyUpdate();
    }

    public void AddUnrefinedSoul(int value)
    {
        if (_unrefinedSoul + value >= 0)
            _unrefinedSoul += value;
        else
            _unrefinedSoul = 0;
        MoneyUpdate();
        SaveSoul(true);
        if(value != 0)
            PointTxtManager.instance.TxtOnSoul(value, unrefinedSoulTxt.transform, soulSpawnPosition);
    }

    public void SetSoul(int value)
    {
        _soul = value;
        MoneyUpdate();
    }

    public void AddSoul(int value, bool isSave = true)
    {
        if (_soul + value >= 0)
            _soul += value;
        else
            _soul = 0;
        MoneyUpdate();
        if(value != 0)
            PointTxtManager.instance.TxtOnSoul(value, soulTxt.transform, Vector2.zero);
        if(isSave)
            SaveSoul(false);
    }

    public bool UseSoul(int amount)
    {
        if (IsUseableSoul(amount))
        {
            _soul -= amount;
            MoneyUpdate();
            PointTxtManager.instance.TxtOnSoul(-amount, soulTxt.transform, Vector2.zero);
            SaveSoul(false);
            return true;
        }
        else
            return false;
    }

    public void SetJewel(int value)
    {
        _jewel = value;
        MoneyUpdate();
    }

    public void AddJewel(int value, bool isSave = true)
    {
        if (_jewel + value >= 0)
            _jewel += value;
        else
            _jewel = 0;
        MoneyUpdate();
        //if(value != 0)
            //PointTxtManager.instance.TxtOnSoul(value, soulTxt.transform, Vector2.zero);
        if(isSave)
            SaveJewel();
    }

    public bool UseJewel(int amount)
    {
        if (IsUseableJewel(amount))
        {
            _jewel -= amount;
            MoneyUpdate();
            //PointTxtManager.instance.TxtOnSoul(-amount, soulTxt.transform, Vector2.zero);
            SaveJewel();
            return true;
        }
        else
            return false;
    }

    public bool IsUseableSoul(int amount)
    {
        return _soul - amount >= 0;
    }

    public bool IsUseableJewel(int amount)
    {
        return _jewel - amount >= 0;
    }

    public void RefineSoul(float rate = 1)
    {
        if(_unrefinedSoul > 0)
            AddSoul((int)(_unrefinedSoul * rate));
        SetUnrefinedSoul(0);
        SaveSoul();
        Debug.Log("RefineSoul");
    }

    public float GetRandomRefiningRate()
    {
        float rate = Random.Range(refiningRate, maxRefiningRate);
        return rate;
    }

    public void SaveSoul(bool isUnrefined = true)
    {
        if(isUnrefined)
            PlayerPrefs.SetInt("unrefinedSoul", _unrefinedSoul);
        PlayerPrefs.SetInt("soul", _soul);
    }

    public void SaveJewel() {
        PlayerPrefs.SetInt("jewel", _jewel);
    }

    public void Load()
    {
        SetUnrefinedSoul(PlayerPrefs.GetInt("unrefinedSoul"));
        SetSoul(PlayerPrefs.GetInt("soul"));
        SetJewel(PlayerPrefs.GetInt("jewel"));

        //기존 게이지 남아있던 사람들 소울 되돌려주기
        int exp = PlayerPrefs.GetInt("exp");
        int remain;
        int currentLevel = SoulShopManager.instance.GetWeaponLevel(exp, out remain);
        if(remain != 0) {
            MoneyManager.instance.AddSoul(remain, true);
            exp -= remain;
            PlayerPrefs.SetInt("exp", exp);
        }

        CheckRefiningRate();
        this.loaded = true;
    }

    public void ResetData()
    {
        PlayerPrefs.SetInt("unrefinedSoul", 0);
        PlayerPrefs.SetInt("soul", 0);
    }

    public void MoneyUpdate()
    {
        //Debug.Log("MoneyUpdate");
        soulTxt.text = _soul.ToString();
        unrefinedSoulTxt.text = _unrefinedSoul.ToString();
        jewelTxt.text = _jewel.ToString();
    }

    private void CheckRefiningRate() {
        switch(PlayerPrefs.GetInt("BuyRefiningRateIdxForJewel")) {
            case 0:
                if(this.maxRefiningRate != 2f) {
                    this.maxRefiningRate = 2f;
                    if(this.refiningRate + 1f < 2f) {
                        this.refiningRate = this.refiningRate + 1f;
                    }
                }
            break;
            case 1:
                if(this.maxRefiningRate != 2f) {
                    this.refiningRate = 2f;
                    this.maxRefiningRate = 2f;
                }
            break;
            case 2:
                if(this.maxRefiningRate != 4f) {
                    this.refiningRate = 2f;
                    this.maxRefiningRate = 4f;
                }
            break;
        }
    }

    public bool CheckRightRefiningRate(float _refiningRate) {
        if(_refiningRate >= this.refiningRate && _refiningRate <= this.maxRefiningRate) {
            return true;
        } else {
            return false;
        }
    }
}
