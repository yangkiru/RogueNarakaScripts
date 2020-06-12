using RogueNaraka.SkillScripts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RogueNaraka.RollScripts
{
    public partial class RollManager : MonoBehaviour {
        public Button rejectBtn;
        public GameObject pauseBtn;
        public GameObject rollPnl;
        public GameObject selectPnl;
        public GameObject soulAlertPnl;
        public Image dragImg;
        public TextMeshProUGUI typeTxt;
        public TextMeshProUGUI nameTxt;
        public TextMeshProUGUI descTxt;
        public TextMeshProUGUI soulTxt;
        public TextMeshProUGUI leftRollTxt;

        public Fade fade;
        public ManaUI manaUI;

        public Vector3 restPosition;

        public int selected;
        public int stopped;
        public bool isClickable;
        public bool isPause { get; set; }
        public RollData[] datas;
        /// <summary>
        /// reRoll 횟수
        /// </summary>
        public int RollCount {
            get { return PlayerPrefs.GetInt("rollCount"); }
            set { PlayerPrefs.SetInt("rollCount", value); }
        }
        /// <summary>
        /// roll 실행 횟수
        /// </summary>
        public int LeftRoll {
            get { return PlayerPrefs.GetInt("leftRoll"); }
            set {
                PlayerPrefs.SetInt("leftRoll", value);
                if (value <= 0)
                    MaxLeftRoll = 0;
            }
        }

        public int MaxLeftRoll {
            get {
                int last = PlayerPrefs.GetInt("maxLeftRoll");
                if (last <= 0)
                    MaxLeftRoll = last = LeftRoll;
                return last;
            }
            set { PlayerPrefs.SetInt("maxLeftRoll", value); }
        }

        public bool IsFirstRoll {
            get { return PlayerPrefs.GetInt("isFirstRoll") == 1; }
            set { PlayerPrefs.SetInt("isFirstRoll", value ? 1 : 0); }
        }

        public int ReRollCost {
            get {
                return RollCount * 10;
            }
        }

        private bool isSkillSelected;

        public static RollManager instance;

        public enum ROLL_TYPE { ALL, SKILL, STAT, ITEM }
        ROLL_TYPE[] lastMode;

        public event System.Action onDecided;
        public event System.Action onLeftRoll;
        public event System.Action onFadeOut;

        void Awake()
        {
            instance = this;
            Init();
        }
        public void Init()
        {
            datas = new RollData[10];
            for (int i = 0; i < 10; i++)
            {
                datas[i].id = -1;
            }
            isClickable = false;
            //isPassed = false;
            stopped = -1;
            SetSelectPnl(false);
        }

        /// <summary>
        /// 쇼케이스 설정
        /// </summary>
        public void SetShowCase(params ROLL_TYPE[] mode)
        {
            Init();
            lastMode = mode;
            if (!LoadDatas())
            {
                SkillChangeManager.instance.Levels = 0;
                for (int i = 0; i < 10; i++)
                {
                    RollData rnd;
                    for (int j = 0; j < 50; j++)
                    {
                        rnd = GetRandom(mode);
                        if (IsSetable(i, rnd) || j == 49)
                        {
                            datas[i] = rnd;
                            break;
                        }
                    }
                    //showCases[i].enabled = true;

                    //SetSprite(i, GetSprite(datas[i]));
                }
                string datasJson = JsonHelper.ToJson<RollData>(datas);
                PlayerPrefs.SetString("rollDatas", datasJson);
            }
            else
            {
                //for (int i = 0; i < showCases.Length; i++)
                //{
                //    showCases[i].enabled = true;
                //    SetSprite(i, GetSprite(datas[i]));
                //}
            }
            //SetStatTxt(true);

            this.InitScroll();
            this.Roll();
        }

        public void SetShowCase(RollData selected, params RollData[] datas)
        {
            Init();
            if (!LoadDatas())
            {
                this.datas = datas;
                for (int i = datas.Length; i < 10; i++)
                {
                    this.datas[i] = datas[Random.Range(0, datas.Length)];
                    Debug.Log("RANDOM");
                }

                this.stopped = -1;

                for (int i = 0; i < 10; i++)
                {
                    if (this.datas[i].type == selected.type && this.datas[i].id == selected.id)
                    {
                        this.stopped = i;
                    }
                    //showCases[i].enabled = true;

                    //SetSprite(i, GetSprite(this.datas[i]));
                    Debug.Log("Set:" + datas[i].id);
                }

                if (this.stopped == -1)
                {
                    int rnd = Random.Range(0, 10);
                    this.datas[rnd] = selected;
                    //showCases[rnd].enabled = true;
                    //SetSprite(rnd, GetSprite(this.datas[rnd]));
                    this.stopped = rnd;
                    Debug.Log("No Stopped");
                }

                if (--stopped < 0)
                    stopped = 9;

                string datasJson = JsonHelper.ToJson<RollData>(this.datas);
                PlayerPrefs.SetString("rollDatas", datasJson);

                PlayerPrefs.SetInt("stopped", stopped);
            }
            else
            {
                Debug.Log("RollData, params RollData[] : ShowCase Loaded");
                //for (int i = 0; i < showCases.Length; i++)
                //{
                //    showCases[i].enabled = true;
                //    SetSprite(i, GetSprite(this.datas[i]));
                //}
            }
            //SetStatTxt(true);

            this.InitScroll();
        }

        public void SetRollPnl(bool value)
        {
            if (value)
            {
                GameManager.instance.SetPauseBtn(true);
                rollPnl.SetActive(value);
                fade.FadeIn();
                if (Pointer.instance)
                    Pointer.instance.SetPointer(false);
            }
            else
            {
                ResetData();

                SkillManager.instance.Save();
                //if (!isStageUp)
                //{
                //    PlayerPrefs.SetInt("isFirstRoll", 0);
                //    SkillManager.instance.Save();
                //    Item.instance.Save();
                //}
                //if (isStageUp)
                //{
                //    BoardManager.instance.StageUp();
                //    //BoardManager.instance.Save();
                //    PlayerPrefs.SetInt("isLevelUp", 0);
                //    //LevelUpManager.instance.StartCoroutine(LevelUpManager.instance.EndLevelUp());
                //    GameManager.instance.Save();
                //}

                //StatManager.instance.SetStatPnl(false);
                if (--LeftRoll <= 0)
                {
                    LeftRoll = 0;
                    if (onDecided != null)
                        onDecided.Invoke();
                    fade.FadeOut();
                }
                else
                {
                    if (onLeftRoll == null)
                    {
                        SetShowCase(lastMode);
                        Roll();
                    }
                    else
                        onLeftRoll.Invoke();

                }

            }
        }

        public void FirstRoll()
        {
            if (LeftRoll == 0)
            {
                LeftRoll = 3;
            }
            SetShowCase(ROLL_TYPE.SKILL);
            SetRollPnl(true);
            Roll();
            SetOnPnlClose(delegate ()
            {
                IsFirstRoll = false;
            });

            GameManager.instance.SetPause(true);
            SetOnFadeOut(GameStart);
        }

        public void GameStart()
        {
            Debug.Log("GameStart");
            Stat stat = Stat.DataToStat();
            if (StageSkipManager.Instance.IsSkipStage && StageSkipManager.Instance.SelectedStage != 1)
            {
                StageSkipManager.Instance.selectedStage = StageSkipManager.Instance.SelectedStage;
                StageSkipManager.Instance.AddBook(StageSkipManager.Instance.GetRandomBookAmount());
                StageSkipManager.Instance.AddRandomStat(stat, StageSkipManager.Instance.GetRandomStatAmount());
                Stat.StatToData(stat);
                StageSkipManager.Instance.SetResultPnl(true);
                StageSkipManager.Instance.IsSkipStage = false;
                GameManager.instance.SetPause(true);
            }
            else
            {
                if (stat != null)
                    GameManager.instance.RunGame(stat);
            }
            //else
            //    BoardManager.instance.fade.FadeIn();
        }

        public void FirstGame()
        {
            Debug.Log("FirstGame");
            LeftRoll = LeftRoll;
            if (LeftRoll == 0 || LeftRoll == 3)
            {
                LeftRoll = 3;
                RollData[] datas = new RollData[10];
                for (int i = 0; i < datas.Length; i++)
                {
                    datas[i] = GetRandom(ROLL_TYPE.SKILL);
                }
                RollData thunder = new RollData();
                thunder.type = ROLL_TYPE.SKILL;
                thunder.id = 0;
                SetShowCase(thunder, datas);
            }
            else
            {
                OnLeftRollFirstGame();
            }
            SetRollPnl(true);
            Roll();
            SetOnFadeOut(GameStart);
            SetOnPnlClose(delegate ()
            {
                IsFirstRoll = false;
                GameManager.instance.IsFirstGame = false;
            });
            SetOnLeftRoll(OnLeftRollFirstGame);
        }

        public void OnLeftRollFirstGame()
        {
            Debug.Log("OnLeftRollFirstGame:" + LeftRoll);

            switch (LeftRoll)
            {
                case 2:
                    {
                        RollData[] datas = new RollData[10];
                        for (int i = 0; i < datas.Length; i++)
                        {
                            datas[i] = GetRandom(ROLL_TYPE.SKILL);
                        }

                        Debug.Log("Ice");
                        RollData ice = new RollData();
                        ice.type = ROLL_TYPE.SKILL;
                        ice.id = 1;
                        SetShowCase(ice, datas);
                        Roll();
                        break;
                    }
                case 1:
                    {
                        RollData[] datas = new RollData[10];
                        for (int i = 0; i < datas.Length; i++)
                        {
                            datas[i] = GetRandom(ROLL_TYPE.SKILL);
                        }

                        Debug.Log("DashShoes");
                        RollData dashshoes = new RollData();
                        dashshoes.type = ROLL_TYPE.SKILL;
                        dashshoes.id = 6;
                        SetShowCase(dashshoes, datas);
                        SetOnLeftRoll(null);
                        Roll();
                        break;
                    }
            }
        }

        public void SetOnLeftRoll(System.Action onLeftRoll)
        {
            this.onLeftRoll = onLeftRoll;
        }

        public void SetOnPnlClose(System.Action onPnlClose)
        {
            this.onDecided = onPnlClose;
        }

        public void SetOnFadeOut(System.Action onFadeOut)
        {
            this.onFadeOut = onFadeOut;
        }

        public void OnFadeOut()
        {
            rollPnl.SetActive(false);
            StatManager.instance.statPnl.SetActive(false);
            if (onFadeOut != null)
                onFadeOut.Invoke();
            //if (isStageUp)
            //    BoardManager.instance.InitBoard();
            //else
            //{
            //    Stat stat = Stat.DataToStat();
            //    if (stat != null)
            //        GameManager.instance.RunGame(stat);
            //    //else
            //    //    BoardManager.instance.fade.FadeIn();
            //}
        }

        IEnumerator OnFail()
        {
            float t = 1f;
            do
            {
                yield return null;
                t -= Time.unscaledDeltaTime;
            } while (t > 0);
            soulAlertPnl.SetActive(true);
        }

        //bool isPassed;
        public void Pass()
        {
            SetRollPnl(false);
            //if (!isPassed)
            //{
            //    isPassed = true;
            //}
            //else
            //{
            //    isPassed = false;
            //    SetRollPnl(false);
            //}
        }

        public void SetSelectPnl(bool value)
        {
            selectPnl.SetActive(value);
        }

        /// <summary>
        /// 데이터 로드
        /// </summary>
        /// <returns></returns>
        private bool LoadDatas()
        {
            string datasJson = PlayerPrefs.GetString("rollDatas");
            if (datasJson != string.Empty)
            {
                datas = JsonHelper.FromJson<RollData>(datasJson);
                Debug.Log("RollDatas Loaded:" + datasJson);
                for (int i = 0; i < datas.Length; i++)
                {
                    switch (datas[i].type)
                    {
                        case ROLL_TYPE.ALL:
                            datas[i] = GetRandom(ROLL_TYPE.ALL);
                            break;
                        case ROLL_TYPE.ITEM:
                            if (datas[i].id >= GameDatabase.instance.items.Length)
                                return false;
                            break;
                        case ROLL_TYPE.SKILL:
                            if (datas[i].id >= GameDatabase.instance.skills.Length)
                                return false;
                            break;
                        case ROLL_TYPE.STAT:
                            break;
                            //case ROLL_TYPE.PASSIVE:
                            //    break;
                    }
                }
                return true;
            }
            return false;
        }

        private bool LoadStopped()
        {
            if (PlayerPrefs.GetInt("stopped") != -1)
            {
                stopped = PlayerPrefs.GetInt("stopped");
                Debug.Log("stopped Loaded:" + stopped);
                return true;
            }
            return false;
        }

        public void ResetData()
        {
            PlayerPrefs.SetString("rollDatas", string.Empty);
            RollCount = 0;
            PlayerPrefs.SetInt("stopped", -1);
        }

        public SkillData GetSkillData(RollData data)
        {
            return GameDatabase.instance.skills[data.id];
        }

        public void Select(int position)
        {
            Debug.Log("Select " + position);
            RollData data = datas[position];
            switch (data.type)
            {
                case ROLL_TYPE.SKILL:
                    SkillData skill = GameDatabase.instance.skills[data.id];
                    //selectedImg.sprite = GetSprite(data);
                    typeTxt.text = GameManager.language == Language.Korean ? "기술" : "Skill";
                    nameTxt.text = skill.GetName();
                    descTxt.text = skill.GetDescription();
                    manaUI.SetMana(skill);
                    break;
                case ROLL_TYPE.STAT:
                    //selectedImg.sprite = GetSprite(data);
                    manaUI.gameObject.SetActive(false);
                    typeTxt.text = GameManager.language == Language.Korean ? "능력치" : "Stat";
                    string point = "Point";
                    if (data.id + 1 > 1)
                        point += "s";
                    nameTxt.text = (data.id + 1) + point;
                    switch (GameManager.language)
                    {
                        default:
                            descTxt.text = string.Format("You will get {0} Stat {1}.", (data.id + 1), point);
                            break;
                        case Language.Korean:
                            descTxt.text = string.Format("{0}의 스탯 포인트를 획득한다.", (data.id + 1));
                            break;
                    }
                    break;
                case ROLL_TYPE.ITEM:
                    manaUI.gameObject.SetActive(false);
                    //selectedImg.sprite = GetSprite(data);
                    typeTxt.text = GameManager.language == Language.Korean ? "소모품" : "ITEM";
                    ItemData item = GameDatabase.instance.items[data.id];
                    ItemSpriteData itemSpr = GameDatabase.instance.itemSprites[Item.instance.sprIds[item.id]];
                    //if (Item.instance.isKnown[item.id])
                    //{
                    nameTxt.text = item.GetName();
                    descTxt.text = item.GetDescription();
                    //}
                    //else
                    //{
                    //    nameTxt.text = string.Format(format, item.GetName());
                    //    descTxt.text = string.Format(format, item.GetDescription());
                    //}
                    break;
                    //case ROLL_TYPE.PASSIVE:
                    //    manaUI.gameObject.SetActive(false);
                    //    //selectedImg.sprite = GetSprite(data);
                    //    typeTxt.text = "Passive";
                    //    nameTxt.text = "패시브";
                    //    descTxt.text = "패시브";
                    //    break;
            }

            SetSelectPnl(true);
        }


        public void OnClick(int position)
        {
            if (this.selected != this.stopped)
                return;
            Debug.Log("selected:" + this.selected + " stopped:" + this.stopped + " position:" + position);
            if (datas[stopped].type == ROLL_TYPE.STAT)
                Ok();
        }

        public void OnDown(int position)
        {
            if (this.selected != this.stopped)
                return;
            if (datas[selected].type != ROLL_TYPE.STAT)
            {
                dragImg.sprite = GetSprite(datas[selected]);
                dragImg.gameObject.SetActive(true);
            }
        }

        public void OnDrag(int position)
        {
            if (this.selected != this.stopped)
                return;

            dragImg.sprite = GetSprite(datas[selected]);
            Vector3 pos = GameManager.instance.GetMousePosition();
            dragImg.rectTransform.position = pos;
        }

        public void OnUp(int position)
        {
            dragImg.rectTransform.position = restPosition;
            dragImg.sprite = null;
            dragImg.gameObject.SetActive(false);
            if (this.selected != this.stopped)
                return;
            Ok();
        }

        /// <summary>
        /// 결정되었을 때 호출
        /// </summary>
        public void Ok()
        {
            RollData rollData = datas[stopped];
            switch (rollData.type)
            {
                case ROLL_TYPE.SKILL:
                    if (SkillGUI.pointedSkill == -1)
                        break;
                    SkillData selectedSkill = GameDatabase.instance.skills[rollData.id];//선택된 스킬 데이터
                    Skill slotSkill = SkillManager.instance.skills[SkillGUI.pointedSkill].skill;//해당 슬롯에 장착된 스킬
                    if (slotSkill == null || slotSkill.data.id == -1)//슬롯이 비었으면
                    {
                        SkillManager.instance.SetSkill(selectedSkill, SkillGUI.pointedSkill);
                        SetRollPnl(false);
                    }
                    else if (slotSkill.data.id == selectedSkill.id)//같은 스킬이면
                    {
                        SkillManager.instance.SetSkill(selectedSkill, SkillGUI.pointedSkill);
                        SetRollPnl(false);
                    }
                    else//다른 스킬이면
                    {
                        if (slotSkill.data.level == 1)
                        {
                            SkillManager.instance.SetSkill(selectedSkill, SkillGUI.pointedSkill);
                            SetRollPnl(false);
                        }
                        else
                            SkillChangeManager.instance.OpenChangePnl(selectedSkill, SkillGUI.pointedSkill);//스킬 교체 패널 오픈
                    }
                    //SetRollPnl(false, isStageUp);
                    break;
                case ROLL_TYPE.STAT:
                    StatManager.instance.SetStatPnl(true, rollData.id + 1);
                    break;
                case ROLL_TYPE.ITEM:
                    if (Item.isPointed)
                    {
                        Item.instance.EquipItem(rollData.id);
                        SetRollPnl(false);
                    }
                    break;
                    //case ROLL_TYPE.PASSIVE:
                    //    //selectedImg.sprite = GetSprite(data);
                    //    typeTxt.text = "Passive";
                    //    nameTxt.text = "패시브";
                    //    descTxt.text = "패시브";
                    //    break;
            }
        }

        ///// <summary>
        ///// SkillUI를 가리키면 호출
        ///// </summary>
        ///// <param name="position"></param>
        //public void SelectSkill(int position)
        //{
        //    if (isClickable && selected != -1 && datas[selected].type == ROLL_TYPE.SKILL)
        //    {
        //        //Debug.Log("Skill Added,position:" + position + " id:" + datas[selected].id);
        //        target = position;
        //    }
        //}

        public Sprite GetSprite(RollData data)
        {
            {
                Sprite result = null;
                try
                {
                    switch (data.type)
                    {
                        case ROLL_TYPE.SKILL:
                            result = GameDatabase.instance.skills[data.id].spr;
                            break;
                        case ROLL_TYPE.STAT:
                            result = GameDatabase.instance.statSprite;
                            break;
                        case ROLL_TYPE.ITEM:
                            result = GameDatabase.instance.itemSprites[Item.instance.sprIds[data.id]].spr;
                            break;
                            //case ROLL_TYPE.PASSIVE:
                            //    result = null;//수정 필요
                            //    break;
                    }
                    return result;
                }
                catch
                {
                    return null;
                }
            }
        }
        public RollData GetRandom(params ROLL_TYPE[] modes)
        {
            RollData result = new RollData();
            int rnd = Random.Range(0, modes.Length);

            result.type = modes[rnd];
            switch (modes[rnd])
            {
                case ROLL_TYPE.ALL:
                    int rndMode = Random.Range(1, (int)ROLL_TYPE.ITEM + 1);
                    return GetRandom((ROLL_TYPE)rndMode);
                case ROLL_TYPE.SKILL:
                    do
                    {
                        result.id = Random.Range(0, GameDatabase.instance.skills.Length);
                    } while (!SkillData.IsBought(result.id) && !GameDatabase.instance.skills[result.id].isBasic);
                    break;
                case ROLL_TYPE.STAT:
                    result.id = Random.Range(0, 3);
                    break;
                case ROLL_TYPE.ITEM:
                    result.id = Random.Range(0, GameDatabase.instance.items.Length);
                    break;
                    //case ROLL_TYPE.PASSIVE:
                    //    result = GetRandom();
                    //    //result.id = Random.Range(0, GameDatabase.instance.skills.Length);//패시브의 길이로 수정
                    //    break;
            }
            return result;
        }

        /// <summary>
        /// RandomSelect Skills
        /// </summary>
        public void Roll(int last = -1)
        {
            Debug.Log("RollStart");
            leftRollTxt.text = string.Format("{0}/{1}", LeftRoll, MaxLeftRoll);
            SetSelectPnl(false);
            isClickable = false;
            //this.scrollView.BackPnl.raycastTarget = false;
            if (!LoadStopped())//로드에 실패하면
            {
                Debug.Log("Load Stopped Fail");
                do
                {
                    stopped = Random.Range(0, 10);//새로운 selected
                } while (last != -1 && last == stopped);
                ++RollCount;
                PlayerPrefs.SetInt("stopped", stopped);//저장
            }
            this.isSelected = false;
            this.scrollView.SelectCell(stopped);
            soulTxt.text = string.Format("<size=11><sprite=0></size> {0}", ReRollCost.ToString());
            //StartCoroutine(CheckRollEnd());
        }

        public void ReRoll() {
            if (!isClickable)
                return;
            int amount = ReRollCost;
            if (MoneyManager.instance.soul >= amount) {
                MoneyManager.instance.UseSoul(amount);
                PlayerPrefs.SetInt("stopped", -1);//초기화
                this.Roll(stopped);
            }
        }

        //IEnumerator IconEffectCorou()
        //{
        //    float size = 1.5f;
        //    float t = 0.5f;
        //    RectTransform imgRect = showCases[(stopped + 1) % 10].rectTransform;
        //    imgRect.localScale = new Vector3(size, size, 0);
        //    while (t > 0)
        //    {
        //        yield return null;
        //        t -= Time.unscaledDeltaTime;
        //        imgRect.localScale = Vector3.Lerp(imgRect.localScale, Vector3.one, 1 - t * 2);
        //    }
        //    imgRect.localScale = Vector3.one;
        //}

        //void SetStatTxt(bool active, int position = -1)
        //{
        //    if (position == -1)
        //    {
        //        for (int i = 0; i < statTxts.Length; i++)
        //        {
        //            if (active && datas[i].type == ROLL_TYPE.STAT)
        //            {
        //                statTxts[i].text = string.Format("+{0}", datas[i].id + 1);
        //                statTxts[i].gameObject.SetActive(true);
        //            }
        //            else
        //                statTxts[i].gameObject.SetActive(false);
        //        }
        //    }
        //    else
        //    {
        //        if (active && datas[position].type == ROLL_TYPE.STAT)
        //        {
        //            statTxts[position].text = string.Format("+{0}", datas[position].id + 1);
        //            statTxts[position].gameObject.SetActive(true);
        //        }
        //        else
        //            statTxts[position].gameObject.SetActive(false);
        //    }
        //}

        /// <summary>
        /// 스킬들의 중복검사
        /// </summary>
        /// <param name="position">쇼케이스의 위치</param>
        /// <param name="id">스킬의 ID</param>
        /// <returns></returns>
        public bool IsSetable(int position, RollData data)
        {
            if (position == 0)
                return true;
            else if (position == 1)
                return datas[position - 1] != data;
            else if (position == datas.Length - 2)
                return datas[0] != data && datas[position - 2] != data && datas[position - 1] != data;
            else if (position == datas.Length - 1)
                return datas[0] != data && datas[1] != data && datas[position - 2] != data && datas[position - 1] != data;
            else
                return datas[position - 2] != data && datas[position - 1] != data;
        }

        public SkillData GetSkillData(int id)
        {
            return GameDatabase.instance.skills[id];
        }

        [System.Serializable]
        public struct RollData
        {
            public ROLL_TYPE type;
            public int id;

            public RollData(ROLL_TYPE type, int id)
            {
                this.type = type;
                this.id = id;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is RollData))
                {
                    return false;
                }

                var data = (RollData)obj;
                return type == data.type &&
                       id == data.id;
            }

            public override int GetHashCode()
            {
                var hashCode = 961388853;
                hashCode = hashCode * -1521134295 + type.GetHashCode();
                hashCode = hashCode * -1521134295 + id.GetHashCode();
                return hashCode;
            }

            public static bool operator ==(RollData d1, RollData d2)
            {
                return d1.Equals(d2);
            }

            public static bool operator !=(RollData d1, RollData d2)
            {
                return !d1.Equals(d2);
            }
        }
    }
}