using DG.Tweening;
using UnityEngine;

namespace UIManagement.Core.Modificators
{
    public abstract class Modificator : MonoBehaviour
    {
        public abstract void AddTweenToSequence(Sequence sequence, Tween tween);
    }
}