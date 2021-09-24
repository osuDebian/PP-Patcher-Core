using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class NoteVarianceAngle : OsuPerNoteStrainSkill
    {
        protected override double SkillMultiplier => 1;

        protected override double StrainDecayBase => 0.3;
        public NoteVarianceAngle(IBeatmap beatmap, Mod[] mods, double clockRate) : base(beatmap, mods, clockRate)
        {

        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            // 150bpm을 기준으로 하여
            // 150bpm 미만은 둔각일때 보너스
            // 150bpm 이상은 예각일때 보너스 제공
            var osuCurrent = (OsuDifficultyHitObject)current;

            var deltaTimeToBpm = 15000 / current.DeltaTime;

            var angle = osuCurrent.Angle ?? 0;
            var value = Math.Sin(angle / 2);

            if(deltaTimeToBpm >= 150)
            {
                // 200bpm까지 유효
                value += Math.Sin(Math.Max((Math.PI / 2 - angle), 0)) * Math.Min((deltaTimeToBpm - 150), 50) / 50 * 1.5;
            }

            return value;
        }
    }
}
