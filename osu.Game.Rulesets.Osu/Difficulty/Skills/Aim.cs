// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to correctly aim at every object in the map with a uniform CircleSize and normalized distances.
    /// </summary>
    public class Aim : OsuStrainSkill
    {
        private const double angle_bonus_begin = Math.PI / 3;
        private const double timing_threshold = 107;

        private OsuPerNoteDatabase database;
        public Aim(OsuPerNoteDatabase database, Mod[] mods)
            : base(mods)
        {
            this.database = database;
        }

        protected override double SkillMultiplier => 26.25;
        protected override double StrainDecayBase => 0.15;

        private int index = 0;
        protected override double StrainValueOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrent = (OsuDifficultyHitObject)current;

            //double result = 0;

            //if (Previous.Count > 0)
            //{
            //    var osuPrevious = (OsuDifficultyHitObject)Previous[0];

            //    if (osuCurrent.Angle != null && osuCurrent.Angle.Value > angle_bonus_begin)
            //    {
            //        // 이걸 대체 왜하는거지?
            //        const double scale = 0;

            //        var angleBonus = Math.Sqrt(
            //            Math.Max(osuPrevious.JumpDistance - scale, 0)
            //            * Math.Pow(Math.Sin(osuCurrent.Angle.Value - angle_bonus_begin), 2)
            //            * Math.Max(osuCurrent.JumpDistance - scale, 0));
            //        result = 1.4 * applyDiminishingExp(Math.Max(0, angleBonus)) / Math.Max(timing_threshold, osuPrevious.StrainTime);
            //    }
            //}

            double jumpDistanceExp = applyDiminishingExp(osuCurrent.JumpDistance);
            double travelDistanceExp = applyDiminishingExp(osuCurrent.TravelDistance);

            double ScaleBonusDeltaTime = 1 + (osuCurrent.ScalingFactor - 1) * 0.1;
            //Console.WriteLine(index);
            double angleBonus = database.strainsNoteAngle[index] * 0.05;
            double fingerControlBonus = database.strainsFingerControl[index] * 0.01;
            double sliderVelocityBonus = database.strainsSliderVelocity[index] * 0.1;

            double totalBonus = 1 + angleBonus + fingerControlBonus + sliderVelocityBonus;

            index++;
            // 기본적으로 점프에 대해 계산하고, 노트간 텀을 400ms로 고정해 계산한 점프를 더한다.
            // 이렇게 되면 디스턴스는 짧은데 텀도 짧아(dt) 넓은 점프로 간주되는 문제를 해소한다.
            //return calculateForJump(result, jumpDistanceExp, travelDistanceExp, osuCurrent.StrainTime);
            return totalBonus *
                (calculateForJump(0, jumpDistanceExp * ScaleBonusDeltaTime, travelDistanceExp * ScaleBonusDeltaTime, osuCurrent.StrainTime) * 0.6 +
                calculateForJump(0, jumpDistanceExp * osuCurrent.ScalingFactor, travelDistanceExp * osuCurrent.ScalingFactor, 500));
        }

        private double calculateForJump(double result, double jumpDistanceExp, double travelDistanceExp, double strainTime)
        {
            return Math.Max(
                result + (jumpDistanceExp + travelDistanceExp + Math.Sqrt(travelDistanceExp * jumpDistanceExp)) / Math.Max(strainTime, timing_threshold),
                0
                //(Math.Sqrt(travelDistanceExp * jumpDistanceExp) + jumpDistanceExp + travelDistanceExp) / strainTime
            );
        }

        private double applyDiminishingExp(double val) => Math.Pow(val, 0.99);
    }
}
