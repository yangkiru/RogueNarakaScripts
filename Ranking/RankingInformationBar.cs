using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RogueNaraka.TierScripts;

namespace RogueNaraka.RankingScripts {
    public class RankingInformationBar : MonoBehaviour {
        public TextMeshProUGUI Ranking;
        public TextMeshProUGUI Name;
        public TextMeshProUGUI Score;

        public void SetInformation(int _ranking, string _name, int _score) {
            this.Ranking.text = _ranking.ToString();
            this.Name.text = _name;
            this.Score.text = _score.ToString();
        }
    }
}