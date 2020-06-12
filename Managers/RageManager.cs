﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RageManager : MonoBehaviour
{
    public static RageManager instance;

    public int rageLevel;
    public float enemiesDmg = 1;
    public float enemiesHp = 1;
    public float soul = 1;
    public bool isRage { get; set; }

    public Button rageBtn;
    public GameObject rageSmallPnl;
    public GameObject rageBanner;
    public ParticleSystem[] fireParticles;

    public TextMeshProUGUI rageLevelTxt;
    public TextMeshProUGUI contentTxt;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    public void ResetSave()
    {
        PlayerPrefs.SetInt("rageLevel", 0);
    }

    public void Rage()
    {
        Debug.Log("Rage");

        Rage(PlayerPrefs.GetInt("rageLevel") + 1);
        //StartCoroutine(EndCorou());
    }

    public void Rage(int level)
    {
        rageLevel = level;
        PlayerPrefs.SetInt("rageLevel", rageLevel);
        //ragePnl.SetActive(true);

        //PlayerPrefs.SetInt("isRun", 0);

        ResetRage();
        RageFunction(rageLevel);
        isRage = true;

        if (rageLevel > 0)
            SetActiveSmallRageBtn(true);
        else
            rageBtn.gameObject.SetActive(false);
        TxtUpdate();
    }

    public void SetActiveSmallRageBtn(bool value)
    {
        if (value)
        {
            rageBtn.interactable = true;
            rageBtn.gameObject.SetActive(true);
            rageBtn.targetGraphic.color = Color.white;
            fireParticles[0].gameObject.SetActive(true);
            fireParticles[1].gameObject.SetActive(true);
        }
        else
            rageBtn.gameObject.SetActive(false);

        if (rageLevel > 1)
        {
            rageLevelTxt.text = string.Format("x{0}", rageLevel);
            rageLevelTxt.gameObject.SetActive(true);
        }
        else
            rageLevelTxt.gameObject.SetActive(false);
    }

    public void BigPnlOpen()
    {
        StartCoroutine(BigPnlOpenActive());
    }

    private IEnumerator BigPnlOpenActive() {
        yield return new WaitForSecondsRealtime(0.5f);

        rageBanner.SetActive(true);
    }

    public void CheckRage()
    {
        rageLevel = PlayerPrefs.GetInt("rageLevel");
        ResetRage();
        RageFunction(rageLevel);
        if (rageLevel > 0)
            SetActiveSmallRageBtn(true);
        TxtUpdate();
    }

    public void ResetRage()
    {
        enemiesDmg = 1;
        enemiesHp = 1;
        soul = 1;
        contentTxt.text = string.Empty;
    }

    public void TxtUpdate()
    {
        contentTxt.text = string.Empty;
        if(enemiesDmg != 0)
        {
            string str = string.Empty;
            switch (GameManager.language)
            {
                case Language.English:
                    str = string.Format("Enemies' Dmg {0} times Up \n", enemiesDmg-1);
                    break;
                case Language.Korean:
                    str = string.Format("적들의 공격력 {0}배 증가\n", enemiesDmg-1);
                    break;
            }
            contentTxt.text += str;
        }
        if(enemiesHp != 0)
        {
            string str = string.Empty;
            switch (GameManager.language)
            {
                case Language.English:
                    str = string.Format("Enemies' Hp {0} times Up\n", enemiesHp - 1);
                    break;
                case Language.Korean:
                    str = string.Format("적들의 체력 {0}배 증가\n", enemiesHp - 1);
                    break;
            }
            contentTxt.text += str;
        }

        if(soul != 0)
        {
            string str = string.Empty;
            switch (GameManager.language)
            {
                case Language.English:
                    str = string.Format("Soul Get {0} times Up\n", soul-1);
                    break;
                case Language.Korean:
                    str = string.Format("얻는 영혼 {0}배 증가\n", soul-1);
                    break;
            }
            contentTxt.text += str;
        }
    }

    public void RageFunction(int level)
    {
        switch(level)
        {
            case 0:
                return;
            //case 1:
            //    DmgUp(5f, level);
            //    HpUp(5f, level);
            //    SoulUp(0.5f, level);
            //    break;
            //case 2:
            //    DmgUp(2f, level);
            //    HpUp(2f, level);
            //    break;
            default:
                DmgUp(5f, level);
                HpUp(5f, level);
                SoulUp(0.5f, level);
                break;

        }
        RageFunction(level - 1);
        return;
    }

    void DmgUp(float amount, int level)
    {
        //Debug.Log("DmgUp" + amount);
        enemiesDmg += amount;
    }

    void HpUp(float amount, int level)
    {
        //Debug.Log("HpUp" + amount);
        enemiesHp += amount;
    }

    void SoulUp(float amount, int level)
    {
        //Debug.Log("SoulUp" + amount);
        soul += amount;
    }
}
