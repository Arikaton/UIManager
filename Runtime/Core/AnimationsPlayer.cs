using System;
using DG.Tweening;
using UnityEngine;

namespace UIManagement.Core
{
    [Serializable]
    public class AnimationsPlayer
    {
        private Sequence _sequence;
        
        public Transform transform;
        
        public event Action<bool> AnimationFinished;
        public float Progress => _sequence.ElapsedPercentage();

        public void StartAnimation(float durationPercent = 1)
        {
            if (_sequence.IsActive())
                return;
            _sequence = DOTween.Sequence();
            _sequence.OnComplete(OnAnimationComplete);
            StartAnimationInternal(_sequence, Mathf.Clamp01(durationPercent));
        }

        public void StartInstantAnimation()
        {
            if (_sequence.IsActive())
                _sequence.Kill();
            StartInstantAnimationInternal();
        }

        public void ForceStopAnimation()
        {
            _sequence?.Kill();
            AnimationFinished?.Invoke(false);
        }
        
        private void StartAnimationInternal(Sequence sequence, float durationPercent)
        {
            var animations = transform.GetComponents<UIAnimation>();
            foreach (var animation in animations)
            {
                animation.StartAnimation(_sequence);
            }
        }

        protected void StartInstantAnimationInternal()
        {
            var animations = transform.GetComponents<UIAnimation>();
            foreach (var animation in animations)
            {
                animation.StartInstantAnimation();
            }
        }

        private void OnAnimationComplete()
        {
            _sequence?.Kill();
            AnimationFinished?.Invoke(true);
        }
        
        public static implicit operator bool(AnimationsPlayer animationsPlayer)
        {
            return !object.ReferenceEquals(animationsPlayer, null) && animationsPlayer.transform;
        }
    }
}