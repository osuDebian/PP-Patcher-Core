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
    public class AimStandard : OsuStrainSkill
    {
        private const double angle_bonus_begin = Math.PI / 3;
        private const double timing_threshold = 107;

        private OsuPerNoteDatabase database;
        public AimStandard(OsuPerNoteDatabase database, Mod[] mods)
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
            // HardRock basically works strongly in a wide distance.
            // However, some bonus is added to give some synergy with DT.
            double ScaleBonusDeltaTime = 1 + (osuCurrent.ScalingFactor - 1) * 0.5;

            // 타이밍 보너스
            // 평균과 차이가 많이 나는 노트에 대해 보너스를 부여한다.
            // Timing Bonus
            // The bonus is given for notes that deviate significantly from the average.
            double timingHalf = Math.Abs(database.averageDeltaTime / 2 - osuCurrent.DeltaTime) / (database.averageDeltaTime / 2);
            double timingNormal = Math.Abs(database.averageDeltaTime - osuCurrent.DeltaTime) / (database.averageDeltaTime);
            double timingDouble = Math.Abs(database.averageDeltaTime * 2 - osuCurrent.DeltaTime) / (database.averageDeltaTime * 2);

            double timingVarianceBonus = Math.Min(timingHalf, Math.Min(timingNormal, timingDouble)) * 0.01;

            /* 각 노트별 보너스를 가져와 가중치를 곱한다 
             * it takes the bonus for each note and multiply by the weight
             */
            // 앵글 변화 보너스
            // angle variance bonus
            double angleBonus = database.strainsNoteAngle[index] * 0.13;

            // 핑거 컨트롤 보너스
            // 릴렉스라서 값이 작음
            // 이 값을 0.1정도로 주게 되면 speed value와 비슷한 효과가 난다.
            double fingerControlBonus = database.strainsFingerControl[index] * 0.065;

            // 슬라이더 속도 보너스
            double sliderVelocityBonus = database.strainsSliderVelocity[index] * 0.075;
            index++;

            double totalBonus = Math.Pow(
                (Math.Pow(0.98 + angleBonus, 1.2)) *
                (Math.Pow(0.99 + fingerControlBonus, 1.2)) *
                (Math.Pow(0.98 + sliderVelocityBonus, 1.2)) *
                (Math.Pow(0.98 + timingVarianceBonus, 1.2))
                , 1.0 / 1.2)
                //+ timingVarianceBonus * 0.5
                ;

            // 기본적으로 점프에 대해 계산하고, 노트간 텀을 400ms로 고정해 계산한 점프를 더한다.
            // 이렇게 되면 디스턴스는 짧은데 텀도 짧아(dt) 넓은 점프로 간주되는 문제를 해소한다.
            //return calculateForJump(result, jumpDistanceExp, travelDistanceExp, osuCurrent.StrainTime);
            return
                totalBonus *
                (calculateForJump(0, jumpDistanceExp * ScaleBonusDeltaTime, travelDistanceExp * ScaleBonusDeltaTime, osuCurrent.StrainTime) * 0.5 +
                calculateForJump(0, jumpDistanceExp * osuCurrent.ScalingFactor, travelDistanceExp * osuCurrent.ScalingFactor, 320));
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
