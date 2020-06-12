using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RogueNaraka.SingletonPattern;
using RogueNaraka.TheBackendScripts;
using RogueNaraka.TierScripts;
using RogueNaraka.RankingScripts;
using RogueNaraka.RollScripts;

namespace RogueNaraka.PlayerInformation {
    public class PlayerInformationManager : MonoSingleton<PlayerInformationManager> {
        public GameObject BlackPanel;
        public GameObject CloseButton;
        public GameObject InformationBoard;

        [Header("Stat Information")]
        public GameObject StatInfo;
        public TextMeshProUGUI[] StatNumTextArray;

        [Header("Ranking Information")]
        public GameObject RankingInfo;

        public Image TierEmblem;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI LevelText;
        public TextMeshProUGUI TierText;
        public TextMeshProUGUI TopPercentText;
        public TextMeshProUGUI TopRankText;

        public Sprite[] TierEmbelmArray;
        public RankingInformationBar[] TopRankerInformationBar;
        public RankingInformationBar PlayerRankingInformationBar;

        private void RefreshStatNumTxt() {
            this.StatNumTextArray[(int)STAT.DMG].text = GameManager.instance.player.data.stat.dmg.ToString();
            this.StatNumTextArray[(int)STAT.SPD].text = GameManager.instance.player.data.stat.spd.ToString();
            this.StatNumTextArray[(int)STAT.TEC].text = GameManager.instance.player.data.stat.tec.ToString();
            this.StatNumTextArray[(int)STAT.HP].text = GameManager.instance.player.data.stat.hp.ToString();
            this.StatNumTextArray[(int)STAT.HR].text = GameManager.instance.player.data.stat.hpRegen.ToString();
            this.StatNumTextArray[(int)STAT.MP].text = GameManager.instance.player.data.stat.mp.ToString();
            this.StatNumTextArray[(int)STAT.MR].text = GameManager.instance.player.data.stat.mpRegen.ToString();
        }

        //Click On Button Method
        public void ClickOnPlayerInformationButton() {
            //DeActive Any Object
            GameManager.instance.SetPause(true);
            RollManager.instance.isPause = true;
            //
            //Active Player Information UI
            this.BlackPanel.SetActive(true);
            this.CloseButton.SetActive(true);
            this.InformationBoard.SetActive(true);
            RefreshStatNumTxt();
            StartCoroutine(RefreshTopRankerInformationBoard());
            //
        }

        public void ClickOnCloseButton() {
            //Active Any Object
            GameManager.instance.SetPause(false);
            RollManager.instance.isPause = false;
            //
            //DeActive Player Information UI
            this.BlackPanel.SetActive(false);
            this.CloseButton.SetActive(false);
            this.InformationBoard.SetActive(false);
            //
        }
        //

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

            this.TierEmblem.sprite = TierManager.Instance.CurrentTier.emblem;
            this.NameText.text = TheBackendManager.Instance.UserNickName;
            this.LevelText.text = string.Format("Lv.{0}", TierManager.Instance.PlayerLevel);
            this.TierText.text = string.Format("{0} {1} Tier"
                , TierManager.Instance.CurrentTier.type
                , TierManager.Instance.CurrentTier.tier_num != 0 ? TierManager.Instance.CurrentTier.tier_num.ToString() : "");
            this.TopPercentText.text = string.Format("{0}%", Mathf.Floor(TheBackendManager.Instance.TopPercentToClearStageForRank * 100) * 0.01f);
            switch(GameManager.language) {
                case Language.English:
                    this.TopRankText.text = string.Format("Top {0}", TheBackendManager.Instance.TopRankingOfPlayer);
                break;
                case Language.Korean:
                    this.TopRankText.text = string.Format("상위 {0} 위", TheBackendManager.Instance.TopRankingOfPlayer);
                break;
            }

            this.PlayerRankingInformationBar.SetInformation(
                TheBackendManager.Instance.TopRankingOfPlayer
                , TheBackendManager.Instance.UserNickName
                , TheBackendManager.Instance.ClearedStageForRank);
        }
        //
    }
}