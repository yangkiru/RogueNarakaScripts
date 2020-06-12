using FancyScrollView;
using UnityEngine;
using UnityEngine.UI;

namespace RogueNaraka.RollScripts
{
    public class Cell : FancyScrollViewCell<RollManager.RollData, Context>
    {
        [SerializeField] Animator animator = default;
        public Image Image { get { return image; } }
        [SerializeField] Image image = default;
        //public Image Arrow { get { return arrow; } }
        //[SerializeField] Image arrow = default;
        [SerializeField] OnMouseButton button = default;
        [SerializeField] RectTransform cachedRectTransform = default;
        public static Cell stoppedCell = default;
        public static Cell selectedCell = default;

        static class AnimatorHash
        {
            public static readonly int Scroll = Animator.StringToHash("scroll");
        }

        private void Start()
        {
            button.onClick.AddListener(() => Context.OnCellClicked?.Invoke(Index));
            button.onDown.AddListener(() => Context.OnCellDowned?.Invoke(Index));
            button.onDrag.AddListener(() => Context.OnCellDragged?.Invoke(Index));
            button.onUp.AddListener(() => Context.OnCellUpped?.Invoke(Index));
        }

        public override void UpdateContent(RollManager.RollData itemData)
        {
            int id = itemData.id;
            int lang = (int)GameManager.language;
            image.sprite = RollManager.instance.GetSprite(itemData);
            bool isStopped = Index == RollManager.instance.stopped;
            bool isSelected = Index == Context.SelectedIndex;
            bool isClickable = RollManager.instance.isClickable;
            image.raycastTarget = isSelected && isClickable;
            //arrow.gameObject.SetActive(isStopped && isClickable);
                
            if (isStopped)
                stoppedCell = this;
        }

        public override void UpdatePosition(float position)
        {
            currentPosition = position;

            animator.Play(AnimatorHash.Scroll, -1, position);
            animator.speed = 0;
        }

        public void SetAsLastSibling()
        {
            cachedRectTransform.SetAsLastSibling();
        }

        public void SetAsFirstSibling()
        {
            cachedRectTransform.SetAsFirstSibling();
        }

        float currentPosition = 0;

        void OnEnable() => UpdatePosition(currentPosition);
    }
}
