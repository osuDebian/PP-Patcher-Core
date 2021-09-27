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

            // 하드락은 기본적으로 디스턴스가 넓은 곳에서 강하게 작용한다
            // 그렇지만 어느정도 DT와의 시너지 효과를 주기 위해 약간의 보너스를 넣어준다
            double ScaleBonusDeltaTime = 1 + (osuCurrent.ScalingFactor - 1) * 0.5;
            //Console.WriteLine(index);

            // 타이밍 보너스
            double timingHalf = Math.Abs(database.averageDeltaTime / 2 - osuCurrent.DeltaTime) / (database.averageDeltaTime / 2);
            double timingNormal = Math.Abs(database.averageDeltaTime - osuCurrent.DeltaTime) / (database.averageDeltaTime);
            double timingDouble = Math.Abs(database.averageDeltaTime * 2 - osuCurrent.DeltaTime) / (database.averageDeltaTime * 2);

            double timingVarianceBonus = Math.Min(timingHalf, Math.Min(timingNormal, timingDouble)) * 0.15;

            /* 각 노트별 보너스를 가져와 가중치를 곱한다 */
            // 앵글 보너스
            double angleBonus = database.strainsNoteAngle[index] * 0.1;

            // 핑거 컨트롤 보너스
            // 릴렉스라서 값이 작음
            // 이 값을 0.1정도로 주게 되면 speed value와 비슷한 효과가 난다.
            double fingerControlBonus = database.strainsFingerControl[index] * 0.03;

            // 슬라이더 속도 보너스
            double sliderVelocityBonus = database.strainsSliderVelocity[index] * 0.09;
            index++;

            double totalBonus = Math.Pow(
                (Math.Pow(0.99 + angleBonus, 1.2)) *
                (Math.Pow(0.99 + fingerControlBonus, 1.2)) *
                (Math.Pow(0.99 + sliderVelocityBonus, 1.2)) *
                (Math.Pow(0.99 + timingVarianceBonus, 1.2))
                , 1.0 / 1.2)
                //+ timingVarianceBonus * 0.5
                ;

            
            // 기본적으로 점프에 대해 계산하고, 노트간 텀을 400ms로 고정해 계산한 점프를 더한다.
            // 이렇게 되면 디스턴스는 짧은데 텀도 짧아(dt) 넓은 점프로 간주되는 문제를 해소한다.
            //return calculateForJump(result, jumpDistanceExp, travelDistanceExp, osuCurrent.StrainTime);
            return
                totalBonus *
                (calculateForJump(jumpDistanceExp * ScaleBonusDeltaTime, travelDistanceExp * ScaleBonusDeltaTime, osuCurrent.StrainTime) * 0.5 +
                calculateForJump(jumpDistanceExp * osuCurrent.ScalingFactor, travelDistanceExp * osuCurrent.ScalingFactor, 320));
        }

        private double calculateForJump(double jumpDistanceExp, double travelDistanceExp, double strainTime)
        {
            return
            //Math.Max(
                (jumpDistanceExp + travelDistanceExp + Math.Sqrt(travelDistanceExp * jumpDistanceExp)) / Math.Max(strainTime, timing_threshold),
            //    (jumpDistanceExp + travelDistanceExp + Math.Sqrt(travelDistanceExp * jumpDistanceExp)) / strainTime
            //)
            ;
        }

        private double applyDiminishingExp(double val) => Math.Pow(val, 0.99);
    }
}
