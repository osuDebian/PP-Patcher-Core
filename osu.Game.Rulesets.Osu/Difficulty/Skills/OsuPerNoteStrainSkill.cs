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
    public abstract class OsuPerNoteStrainSkill : PerNoteStrainSkill
    {
        protected override double SkillMultiplier => 1;

        protected override double StrainDecayBase => 0.5;
        //protected readonly OsuPerNoteDatabase database;
        public OsuPerNoteStrainSkill(IBeatmap beatmap, Mod[] mods, double clockRate) : base(beatmap, mods, clockRate)
        {

        }

        public override double DifficultyValue()
        {
            //var track = new TrackVirtual(10000);
            //mods.OfType<IApplicableToTrack>().ForEach(m => m.ApplyToTrack(track));

            //for (int i = 1; i < beatmap.HitObjects.Count; i++)
            //{
            //    var lastLast = i > 1 ? beatmap.HitObjects[i - 2] : null;
            //    var last = beatmap.HitObjects[i - 1];
            //    var current = beatmap.HitObjects[i];

            //    ProcessInternal(new OsuDifficultyHitObject(current, lastLast, last, clockRate));
            //}

            var totalDifficulty = base.DifficultyValue();

            //var list = new List<double>();
            //list.AddRange(strainPeaks);



            return totalDifficulty;
        }

    }
}
