using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills.Pre
{
    public class AverageTiming : PerNoteStrainSkill
    {
        public AverageTiming(IBeatmap beatmap, Mod[] mods, double clockRate) : base(beatmap, mods, clockRate)
        {
        }

        protected override double SkillMultiplier => 1;

        protected override double StrainDecayBase => 0.25;

        private double sum = 0;
        private int count = 0;

        public override double DifficultyValue()
        {

            return count != 0 ? sum / count : 0;
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            sum += current.DeltaTime;
            count++;

            return 1000.0 / current.DeltaTime;
        }
    }
}
