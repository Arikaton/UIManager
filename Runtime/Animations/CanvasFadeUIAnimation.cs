using DG.Tweening;
using UIManagement.Core;
using UnityEngine;

namespace UIManagement.Animations
{
    public class CanvasFadeUIAnimation : ModifiableUIAnimation
    {
        [SerializeField] private CanvasGroup _target;
        [SerializeField] [Range(0, 1)] protected float _targetValue;
        [SerializeField] private float _duration;
        [SerializeField] private Ease _ease;
        
        protected override Tween StartAnimationInternal(float durationPercent)
        {
            return _target.DOFade(_targetValue, _duration * durationPercent).SetEase(_ease);
        }

        protected override void StartInstantAnimationInternal()
        {
            _target.alpha = _targetValue;
        }
    }
}