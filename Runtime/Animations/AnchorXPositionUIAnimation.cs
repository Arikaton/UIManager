using DG.Tweening;
using UIManagement.Core;
using UnityEngine;

namespace UIManagement.Animations
{
    public class AnchorXPositionUIAnimation : ModifiableUIAnimation
    {
        [SerializeField] private RectTransform _target;
        [SerializeField] private float _targetValueX;
        [SerializeField] private float _duration;
        [SerializeField] private Ease _ease;
        
        protected override Tween StartAnimationInternal(float durationPercent)
        {
            return _target.DOAnchorPosX(_targetValueX, _duration * durationPercent).SetEase(_ease);
        }

        protected override void StartInstantAnimationInternal()
        {
            _target.anchoredPosition = new Vector2(_targetValueX, _target.anchoredPosition.y);
        }
    }
}