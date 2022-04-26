using System;
using DG.Tweening;
using UIManagement.Core;
using UnityEngine;
using UnityEngine.Events;

namespace UIManagement.Animations
{
    public class RaiseEventUIAnimation : ModifiableUIAnimation
    {
        [SerializeField] private UnityEvent onAnimationStart;
        [SerializeField] private UnityEvent onAnimationInstantStart;

        protected override Tween StartAnimationInternal(float durationPercent)
        {
            onAnimationStart?.Invoke();
            throw new NotImplementedException();
        }

        protected override void StartInstantAnimationInternal()
        {
            onAnimationInstantStart?.Invoke();
        }
    }
}