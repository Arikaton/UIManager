using DG.Tweening;
using UIManagement.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UIManagement.Animations
{
    public class ImageAlphaUIAnimation : ModifiableUIAnimation
    {
        [SerializeField] private Image target;
        [SerializeField] [Range(0, 1)] private float targetValue;
        [SerializeField] private float duration;
        [SerializeField] private Ease ease;
        [SerializeField] private bool setFromValue;
        [SerializeField] private float fromValue;

        protected override Tween StartAnimationInternal(float durationPercent)
        {
            if(setFromValue)
                target.color = new Color(target.color.r, target.color.g, target.color.b, fromValue);
            return target.DOFade(targetValue, duration).SetEase(ease);
        }

        protected override void StartInstantAnimationInternal()
        {
            target.color = new Color(target.color.r, target.color.g, target.color.b, targetValue);
        }
    }
}