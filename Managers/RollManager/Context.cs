using System;

namespace RogueNaraka.RollScripts
{
    public class Context
    {
        public int SelectedIndex = -1;
        public Action<int> OnCellClicked;
        public Action<int> OnCellDowned;
        public Action<int> OnCellDragged;
        public Action<int> OnCellUpped;
    }
}
