using DG.Tweening;
using UIManagement.Core;
using UnityEngine;

namespace UIManagement.Animations
{
    public class AnchoredPositionAnimation : ModifiableUIAnimation
    {
        [SerializeField] private RectTransform _target;
        [SerializeField] private Vector2 targetValue;
        [SerializeField] private float _duration;
        [SerializeField] private Ease _ease;
        [SerializeField] private bool setStartValue;
        [SerializeField] private Vector2 startValue;

        protected override Tween StartAnimationInternal(float durationPercent)
        {
            if (setStartValue)
                _target.anchoredPosition = startValue;
            return _target.DOAnchorPos(targetValue, _duration * durationPercent).SetEase(_ease);
        }

        protected override void StartInstantAnimationInternal()
        {
            _target.anchoredPosition = targetValue;
        }
    }
}