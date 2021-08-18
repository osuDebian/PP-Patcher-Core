using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class DistanceAverage : Skill
    {
        public DistanceAverage(Mod[] mods)
            :base(mods)
        {

        }

        private double sum = 0;
        private int count = 0;

        public override double DifficultyValue()
        {
            return sum / count;
        }

        protected override void Process(DifficultyHitObject pCurrent)
        {
            var current = (OsuDifficultyHitObject)pCurrent;
            sum += current.JumpDistance + current.TravelDistance;
            
            count++;
        }
    }
}
