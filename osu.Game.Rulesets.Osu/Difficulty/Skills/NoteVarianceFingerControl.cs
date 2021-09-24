﻿using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class NoteVarianceFingerControl : PerNoteStrainSkill
    {
        public NoteVarianceFingerControl(IBeatmap beatmap, Mod[] mods, double clockRate) : base(beatmap, mods, clockRate)
        {
        }

        protected override double SkillMultiplier => 1;

        protected override double StrainDecayBase => 0.15;

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            // 양타 가능성을 계산한다.
            // bpm이 120일때 0
            // bpm이 200일때 1로 처리
            // 즉 bpm이 200이라면 무조건 이사람은 양타할걸로 보는것이다.
            // 200브픔을 단타로 치는 사람은 극히 드물거고 그정도면 개잘하는것
            var deltaTimeToBpm = 15000 / current.DeltaTime;
            var probablityAlternative = Math.Max((deltaTimeToBpm - 120.0), 0) / (200.0 - 120.0);

            return probablityAlternative;
        }
    }
}
