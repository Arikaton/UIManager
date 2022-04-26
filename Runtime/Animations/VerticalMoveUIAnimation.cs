using DG.Tweening;
using UIManagement.Core;
using UnityEngine;

namespace UIManagement.Animations
{
    public class VerticalMoveUIAnimation : ModifiableUIAnimation
    {
        [SerializeField] private UIView _uiView;
        [SerializeField] private float _duration;
        [SerializeField] private Ease _ease;
        [SerializeField] private bool _isShowAnimation;
        private float _height;
        private RectTransform _rectTransform;
        private float _destinationY;

        private void Awake()
        {
            _rectTransform = _uiView.GetComponent<RectTransform>();
            _height = _rectTransform.sizeDelta.y;
            _destinationY = _isShowAnimation ? _uiView.StartPosition.y : _uiView.StartPosition.y - _height;
        }

        protected override Tween StartAnimationInternal(float durationPercent)
        {
            return _rectTransform.DOAnchorPosY(_destinationY, _duration * durationPercent).SetEase(_ease);
        }

        protected override void StartInstantAnimationInternal()
        {
            _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, _destinationY);
        }
    }
}