using DG.Tweening;

namespace UIManagement.Core
{
    public interface IUIAnimation
    {
        public void StartAnimation(Sequence sequence, float durationPercent = 1f);
        public void StartInstantAnimation();
    }
}