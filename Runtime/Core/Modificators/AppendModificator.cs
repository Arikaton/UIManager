using DG.Tweening;

namespace UIManagement.Core.Modificators
{
    public class AppendModificator : Modificator
    {
        public override void AddTweenToSequence(Sequence sequence, Tween tween)
        {
            sequence.Append(tween);
        }
    }
}