using DG.Tweening;

namespace UIManagement.Core
{
    public abstract class UIAnimationBase : UIAnimation
    {
        public override void StartAnimation(Sequence sequence, float durationPercent = 1)
        {
            var tween = StartAnimationInternal(durationPercent);
            sequence.Append(tween);
        }

        public override void StartInstantAnimation()
        {
            StartInstantAnimationInternal();
        }

        protected abstract Tween StartAnimationInternal(float durationPercent);

        protected abstract void StartInstantAnimationInternal();
    }
}