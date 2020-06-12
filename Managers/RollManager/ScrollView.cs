using EasingCore;
using FancyScrollView;
using RogueNaraka.RollScripts;
using RogueNaraka.TimeScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RogueNaraka.RollScripts
{
    public class ScrollView : FancyScrollView<RollManager.RollData, Context>
    {
        public Scroller Scroller { get { return scroller; } }
        [SerializeField] Scroller scroller = default;
        [SerializeField] GameObject cellPrefab = default;
        public Image BackPnl { get { return backPnl; } }
        [SerializeField] Image backPnl;
        Action<int> onSelectionChanged;

        protected override GameObject CellPrefab => cellPrefab;

        private void Awake()
        {
            Context.OnCellClicked = OnCellClicked;
            Context.OnCellDowned = RollManager.instance.OnDown;
            Context.OnCellDragged = RollManager.instance.OnDrag;
            Context.OnCellUpped = RollManager.instance.OnUp;
        }

        public void OnCellClicked(int index)
        {
            Debug.Log(index + " CellClicked");

            RollManager.instance.OnClick(index);
        }

        void Start()
        {
            scroller.OnValueChanged(OnValueChanged);
            scroller.OnSelectionChanged(UpdateSelection);
        }

        void OnValueChanged(float value)
        {
            UpdatePosition(value);
            CheckTick(value);
        }

        float t = -1;
        void CheckTick(float value)
        {
            if (t == -1)
                t = value;
            else if (Mathf.Abs(value - t) >= 1)
            {
                t = value;
                AudioManager.instance.PlaySFX("skillRoulette");
            }
        }

        void UpdateSelection(int index)
        {
            if (Context.SelectedIndex == index)
            {
                return;
            }

            Context.SelectedIndex = index;
            Refresh();

            onSelectionChanged?.Invoke(index);
        }

        public void UpdateData(IList<RollManager.RollData> items)
        {
            UpdateContents(items);
            scroller.SetTotalCount(items.Count);
        }

        public void OnSelectionChanged(Action<int> callback)
        {
            onSelectionChanged = callback;
        }

        public void SelectNextCell()
        {
            SelectCell(Context.SelectedIndex + 1);
        }

        public void SelectPrevCell()
        {
            SelectCell(Context.SelectedIndex - 1);
        }

        public void SelectCell(int index)
        {
            //if (index < 0 || index >= ItemsSource.Count || index == Context.SelectedIndex)
            //{
            //    return;
            //}
            index %= 10;
            UpdateSelection(index);

            scroller.ScrollTo(index, 2, 2f, Ease.InOutBack, RollManager.instance.OnSelect);
            Debug.Log("SelectCell " + index);
        }
    }
}