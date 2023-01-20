using System;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    public class PrimerPlayable<TTrackBind> : PrimerPlayable
    {
        protected TTrackBind trackTarget { get; set; }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            preventInitialization = true;
            base.ProcessFrame(playable, info, playerData);
            preventInitialization = false;

            if (playerData is null)
                return;

            if (playerData is not TTrackBind boundObject || boundObject is null) {
                throw new Exception(
                    $"Expected track target to be {typeof(TTrackBind).Name} but {playerData?.GetType().Name} found"
                );
            }

            trackTarget = boundObject;
            RunStart();
        }
    }
}