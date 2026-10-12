using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

namespace UIManagement.Core
{
    internal static class TweenExtensions
    {
        public static TweenerCore<float, float, FloatOptions> DOFade(this CanvasGroup target, float endValue, float duration)
        {
            var tween = DOTween.To(() => target.alpha, x => target.alpha = x, endValue, duration);
            tween.SetTarget(target);
            return tween;
        }

        public static TweenerCore<Color, Color, ColorOptions> DOFade(this Image target, float endValue, float duration)
        {
            var tween = DOTween.ToAlpha(() => target.color, x => target.color = x, endValue, duration);
            tween.SetTarget(target);
            return tween;
        }

        public static TweenerCore<Vector2, Vector2, VectorOptions> DOAnchorPos(this RectTransform target, Vector2 endValue, float duration, bool snapping = false)
        {
            var tween = DOTween.To(() => target.anchoredPosition, x => target.anchoredPosition = x, endValue, duration);
            tween.SetOptions(snapping).SetTarget(target);
            return tween;
        }

        public static TweenerCore<Vector2, Vector2, VectorOptions> DOAnchorPosX(this RectTransform target, float endValue, float duration, bool snapping = false)
        {
            var tween = DOTween.To(() => target.anchoredPosition, x => target.anchoredPosition = x, new Vector2(endValue, 0), duration);
            tween.SetOptions(AxisConstraint.X, snapping).SetTarget(target);
            return tween;
        }

        public static TweenerCore<Vector2, Vector2, VectorOptions> DOAnchorPosY(this RectTransform target, float endValue, float duration, bool snapping = false)
        {
            var tween = DOTween.To(() => target.anchoredPosition, x => target.anchoredPosition = x, new Vector2(0, endValue), duration);
            tween.SetOptions(AxisConstraint.Y, snapping).SetTarget(target);
            return tween;
        }
    }
}
