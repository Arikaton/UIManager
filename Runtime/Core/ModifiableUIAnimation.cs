using System;
using DG.Tweening;
using UIManagement.Core.Modificators;
using UnityEngine;

namespace UIManagement.Core
{
    public abstract class ModifiableUIAnimation : UIAnimationBase
    {
        public override void StartAnimation(Sequence sequence, float durationPercent = 1)
        {
            var tween = StartAnimationInternal(durationPercent);
            AddTweenToSequenceWithModificators(sequence, tween);
        }

        private void AddTweenToSequenceWithModificators(Sequence sequence, Tween tween)
        {
            var allComponents = GetComponents<Component>();
            var myIndex = Array.IndexOf(allComponents, this);
            var upperComponent = allComponents[myIndex - 1];

            if (upperComponent is Modificator modificator)
                modificator.AddTweenToSequence(sequence, tween);
            else
                sequence.Insert(0.0f, tween);
        }
    }
}