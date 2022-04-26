using DG.Tweening;

namespace UIManagement.Core.Modificators
{
    public class JoinModificator : Modificator
    {
        public override void AddTweenToSequence(Sequence sequence, Tween tween)
        {
            sequence.Join(tween);
        }
    }
}