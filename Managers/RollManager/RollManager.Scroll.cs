using FancyScrollView;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RogueNaraka.RollScripts
{
    public partial class RollManager : MonoBehaviour
    {
        [SerializeField] ScrollView scrollView = default;
        public bool isSelected;
        void InitScroll()
        {
            scrollView.OnSelectionChanged(OnSelectionChanged);

            var items = Enumerable.Range(0, 10)
                .Select(i => RollManager.instance.datas[i])
                .ToArray();

            scrollView.UpdateData(items);
        }

        void OnSelectionChanged(int index)
        {
            this.selected = index;
            if(isClickable)
                this.Select(this.selected);
        }

        public void OnSelect()
        {
            if (!this.isSelected)
            {
                this.isClickable = true;
                this.Select(this.selected);
                AudioManager.instance.PlaySFX("skillStop");
                this.isSelected = true;
                //this.scrollView.BackPnl.raycastTarget = true;
                //Cell.stoppedCell.Arrow.gameObject.SetActive(true);
                Cell.stoppedCell.Image.raycastTarget = true;
            }
            switch (datas[stopped % 10].type)
            {
                case ROLL_TYPE.SKILL:
                    TutorialManager.instance.StartTutorial(2);
                    break;
                case ROLL_TYPE.ITEM:
                    TutorialManager.instance.StartTutorial(5);
                    break;
            }
        }
    }
}