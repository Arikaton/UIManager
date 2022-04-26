using DG.Tweening;
using UIManagement.Core;
using UnityEngine;

namespace UIManagement.Animations
{
    public class AnchorYPositionUIAnimation : ModifiableUIAnimation
    {
        [SerializeField] private RectTransform _target;
        [SerializeField] private float _targetValueY;
        [SerializeField] private float _duration;
        [SerializeField] private Ease _ease;
        
        protected override Tween StartAnimationInternal(float durationPercent)
        {
            return _target.DOAnchorPosY(_targetValueY, _duration * durationPercent).SetEase(_ease);
        }

        protected override void StartInstantAnimationInternal()
        {
            _target.anchoredPosition = new Vector2(_target.anchoredPosition.x, _targetValueY);
        }
    }
}