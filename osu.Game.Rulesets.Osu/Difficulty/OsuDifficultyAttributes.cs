// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuDifficultyAttributes : DifficultyAttributes
    {
        public double AimStrainRelax { get; set; }
        public double AimStrain { get; set; }
        public double AimStrainAverage { get; set; }
        public double AimStrainMost { get; set; }
        public double DistanceAverage { get; set; }
        public double DistanceTop { get; set; }
        public double SpeedStrain { get; set; }
        public double ApproachRate { get; set; }
        public double OverallDifficulty { get; set; }
        public int HitCircleCount { get; set; }
        public int SpinnerCount { get; set; }
    }
}
