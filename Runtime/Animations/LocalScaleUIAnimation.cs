using DG.Tweening;
using UIManagement.Core;
using UnityEngine;

namespace UIManagement.Animations
{
    public class LocalScaleUIAnimation : ModifiableUIAnimation
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _targetValue;
        [SerializeField] private float _duration;
        [SerializeField] private Ease _ease;
        
        protected override Tween StartAnimationInternal(float durationPercent)
        {
            return _target.DOScale(_targetValue, _duration * durationPercent).SetEase(_ease);
        }

        protected override void StartInstantAnimationInternal()
        {
            _target.localScale = new Vector3(_targetValue, _targetValue, 1);
        }
    }
}