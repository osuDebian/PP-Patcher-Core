using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class NoteVarianceSliderVelocity : PerNoteStrainSkill
    {
        public NoteVarianceSliderVelocity(IBeatmap beatmap, Mod[] mods, double clockRate) : base(beatmap, mods, clockRate)
        {
        }

        protected override double SkillMultiplier => 1;

        protected override double StrainDecayBase => 0.75;

        private double lastVelocity = -1;

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            var osuCurrent = (OsuDifficultyHitObject)current;
            var result = 0.0;

            if(osuCurrent.LastObject is Slider OsuSlider)
            {
                if(lastVelocity >= 0)
                {
                    result = Math.Abs(OsuSlider.Velocity - lastVelocity);
                }
                lastVelocity = OsuSlider.Velocity;

                result += OsuSlider.Velocity / 10;
            }

            return result;
        }
    }
}
