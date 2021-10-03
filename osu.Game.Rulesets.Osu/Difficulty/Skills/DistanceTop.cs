using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class DistanceTop : Skill
    {
        public DistanceTop(Mod[] mods)
            :base(mods)
        {

        }


        private List<double> distances = new List<double>();

        public override double DifficultyValue()
        {
            distances.Sort((a, b) => b.CompareTo(a));
            double sum = 0;

            // average of top 20% notes
            int count = (int) (distances.Count * (20.0 / 100.0));
            for(int i = 0; i < count; i++)
            {
                sum += distances[i];
            }

            return sum / count;
        }

        protected override void Process(int index, DifficultyHitObject pCurrent)
        {
            var current = (OsuDifficultyHitObject)pCurrent;
            distances.Add(current.JumpDistance + current.TravelDistance);
        }
    }
}
