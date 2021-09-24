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

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public abstract class PerNoteStrainSkill : Skill
    {
        //protected readonly OsuPerNoteDatabase database;
        protected readonly IBeatmap beatmap;
        protected readonly double clockRate;

        protected readonly List<double> strainPeaks = new List<double>();

        /// <summary>
        /// Strain values are multiplied by this number for the given skill. Used to balance the value of different skills between each other.
        /// </summary>
        protected abstract double SkillMultiplier { get; }

        /// <summary>
        /// Determines how quickly strain decays for the given skill.
        /// For example a value of 0.15 indicates that strain decays to 15% of its original value in one second.
        /// </summary>
        protected abstract double StrainDecayBase { get; }

        public PerNoteStrainSkill(IBeatmap beatmap, Mod[] mods, double clockRate) : base(mods)
        {
            //this.database = database;
            this.beatmap = beatmap;
            strainPeaks.Add(0);
        }

        private double currentStrain = 0;

        protected override void Process(DifficultyHitObject current)
        {
            var value = StrainValueOf(current);

            currentStrain *= strainDecay(current.DeltaTime);
            currentStrain += value * SkillMultiplier;

            strainPeaks.Add(currentStrain);
        }

        public override double DifficultyValue()
        {
            return 0;
        }

        protected abstract double StrainValueOf(DifficultyHitObject current);
        private double strainDecay(double ms) => Math.Pow(StrainDecayBase, ms / 1000);

        public List<double> GetAllStrainPeaks()
        {
            return strainPeaks;
        }

    }
}
