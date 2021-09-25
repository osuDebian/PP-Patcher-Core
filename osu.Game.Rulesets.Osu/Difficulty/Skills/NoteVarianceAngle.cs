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
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class NoteVarianceAngle : PerNoteStrainSkill
    {
        protected override double SkillMultiplier => 1;

        protected override double StrainDecayBase => 0.3;
        public NoteVarianceAngle(IBeatmap beatmap, Mod[] mods, double clockRate) : base(beatmap, mods, clockRate)
        {

        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            var lastAngle = 0.0;
            if(Previous.Count > 0)
            {
                var osuPast = (OsuDifficultyHitObject)Previous[0];
                lastAngle = osuPast.Angle ?? 0;
            }
            var osuCurrent = (OsuDifficultyHitObject)current;
            
            var deltaTimeToBpm = 15000 / current.DeltaTime;

            var angle = osuCurrent.Angle ?? 0;

            // 각도 변화 값
            var value = Math.Sin(Math.Abs(angle - lastAngle) / 2);

            // 120bpm 미만은 둔각일때 보너스
            value += Math.Sin(angle / 2) / 10;

            // 120bpm 이상은 예각일때 보너스 제공
            if (deltaTimeToBpm >= 120)
            {
                // 스택된 예각 연타 너프
                var cs = 54.4 - 4.48 * ((OsuHitObject)osuCurrent.BaseObject).Radius;
                var distance = osuCurrent.JumpDistance / (cs * 2);
                var multiplier = distance >= 1.0 ? 1.0 : Math.Max(distance, 0);

                // 200bpm까지 유효
                value += Math.Sin(Math.Max((Math.PI / 2 - angle), 0)) *
                    Math.Min((deltaTimeToBpm - 120), 80) / 60 *
                    1 * multiplier
                    ;
            }

            return value;
        }
    }
}
