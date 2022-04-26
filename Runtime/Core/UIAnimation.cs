using DG.Tweening;
using UnityEngine;

namespace UIManagement.Core
{
    public abstract class UIAnimation : MonoBehaviour, IUIAnimation
    {
        public abstract void StartAnimation(Sequence sequence, float durationPercent = 1);
        public abstract void StartInstantAnimation();
    }
}