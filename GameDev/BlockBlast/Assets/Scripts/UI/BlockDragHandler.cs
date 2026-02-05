using BlockBlast.Core;
using BlockBlast.Managers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BlockBlast.UI
{
    public class BlockDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Settings")]
        public float dragScale = 1.1f;
        public float dragAlpha = 0.8f;
        public float returnDuration = 0.3f;

        private RectTransform rectTransform;
        private Canvas canvas;
        private CanvasGroup canvasGroup;
        private Vector2 originalPosition;
        private int blockIndex;
        private BlockShape blockShape;
        private bool isDragging = false;

        public void Initialize(int index, BlockShape shape, Canvas parentCanvas)
        {
            blockIndex = index;
            blockShape = shape;
            canvas = parentCanvas;
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            originalPosition = rectTransform.anchoredPosition;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (GameManager.Instance.CurrentState != GameManager.GameState.Idle) return;

            isDragging = true;
            rectTransform.DOScale(dragScale, 0.1f).SetEase(Ease.OutQuad);
            canvasGroup.DOFade(dragAlpha, 0.1f);
            canvasGroup.blocksRaycasts = false;

            GameManager.Instance.OnBlockDragStart(blockIndex);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out localPoint
            );

            rectTransform.anchoredPosition = localPoint;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            isDragging = false;

            canvasGroup.DOFade(1f, 0.1f);
            canvasGroup.blocksRaycasts = true;

            bool placed = GameManager.Instance.TryPlaceBlock(blockShape, eventData.position);

            if (placed)
            {
                gameObject.SetActive(false);
            }
            else
            {
                rectTransform.DOScale(1f, 0.15f);
                rectTransform.DOAnchorPos(originalPosition, returnDuration)
                    .SetEase(Ease.OutElastic);
            }

            GameManager.Instance.OnBlockDragEnd(blockIndex, eventData.position, placed);
        }

        public void ResetPosition()
        {
            rectTransform.anchoredPosition = originalPosition;
            rectTransform.localScale = Vector3.one;
            gameObject.SetActive(true);
        }
    }
}
