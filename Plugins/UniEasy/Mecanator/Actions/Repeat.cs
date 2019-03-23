using UnityEngine;
using UniRx;

namespace UniEasy
{
    public class Repeat : StateMachineAction
    {
        [MinMaxRange]
        public RangedFloat Interval;
        private System.IDisposable Repeater;

        public override void Execute(StateMachineActionObject smao)
        {
            TryRepeat(smao);
        }

        private void TryRepeat(StateMachineActionObject smao)
        {
            var delay = System.TimeSpan.FromSeconds(smao.StateInfo.length + Random.Range(Interval.Min, Interval.Max));

            if (Repeater != null)
            {
                Repeater.Dispose();
            }

            Repeater = Observable.Timer(delay).Subscribe(_ =>
            {
                if (smao.Animator.State(smao.LayerIndex) == smao.StateInfo.fullPathHash)
                {
                    smao.Animator.Play(smao.StateInfo.fullPathHash, smao.LayerIndex, 0f);
                    TryRepeat(smao);
                }
                else if (Repeater != null)
                {
                    Repeater.Dispose();
                }
            }).AddTo(smao.Animator);
        }
    }
}
