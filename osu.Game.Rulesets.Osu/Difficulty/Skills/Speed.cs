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
    /// Represents the skill required to press keys with regards to keeping up with the speed at which objects need to be hit.
    /// </summary>
    public class Speed : OsuStrainSkill
    {
        private const double single_spacing_threshold = 125;

        private const double angle_bonus_begin = 5 * Math.PI / 6;
        private const double pi_over_4 = Math.PI / 4;
        private const double pi_over_2 = Math.PI / 2;

        protected override double SkillMultiplier => 1400;
        protected override double StrainDecayBase => 0.3;
        protected override int ReducedSectionCount => 5;
        protected override double DifficultyMultiplier => 1.04;

        private const double min_speed_bonus = 75; // ~200BPM
        private const double max_speed_bonus = 45; // ~330BPM
        private const double speed_balancing_factor = 40;

        private const double min_doubletap_nerf = 0.5; // minimum value (eventually on stacked)
        private const double max_doubletap_nerf = 1.0; // maximum value 
        private const double threshold_doubletap_contributing = 1.5; // minimum distance not influenced (2.0 means it is not stacked at least)


        private const double min_acute_stream_spam_nerf = 0.0; // minimum value (eventually on stacked)
        private const double max_acute_stream_spam_nerf = 1.0; // maximum value 
        private const double threshold_acute_stream_spam_contributing = 2.0; // minimum distance not influenced (2.0 means it is not stacked at least)
        public Speed(Mod[] mods)
            : base(mods)
        {
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrent = (OsuDifficultyHitObject)current;

            double distance = Math.Min(single_spacing_threshold, osuCurrent.TravelDistance + osuCurrent.JumpDistance);
            double deltaTime = Math.Max(max_speed_bonus, current.DeltaTime);
            double radius = ((OsuHitObject)osuCurrent.BaseObject).Radius;

            double speedBonus = 1.0;
            if (deltaTime < min_speed_bonus)
            {
                double multiplierSpeedBonus = min_doubletap_nerf +
                    Math.Max(Math.Min(distance / (radius * threshold_doubletap_contributing), 1.0), 0.0)
                     * (max_doubletap_nerf - min_doubletap_nerf);
                speedBonus = 1 + Math.Pow((min_speed_bonus - deltaTime) / speed_balancing_factor, 2)
                                    * multiplierSpeedBonus
                                    ;
            }

            double angleBonus = 1.0;

            if (osuCurrent.Angle != null && osuCurrent.Angle.Value < angle_bonus_begin)
            {
                angleBonus = 1 + Math.Pow(Math.Sin(1.5 * (angle_bonus_begin - osuCurrent.Angle.Value)), 2) / 3.57;

                if (osuCurrent.Angle.Value < pi_over_2)
                {

                    double multiplierAngleBonus = min_acute_stream_spam_nerf +
                        Math.Max(Math.Min(distance / (radius * threshold_acute_stream_spam_contributing), 1.0), 0.0)
                        * (max_acute_stream_spam_nerf - min_acute_stream_spam_nerf)
                        ;

                    if (distance < 90)
                        if (osuCurrent.Angle.Value < pi_over_4)
                            angleBonus = (1.28 + (1 - 1.28) * Math.Min((90 - distance) / 10, 1)) * multiplierAngleBonus;
                        else
                            angleBonus = (1.28 + (1 - 1.28) * Math.Min((90 - distance) / 10, 1)
                            * Math.Sin((pi_over_2 - osuCurrent.Angle.Value) / pi_over_4)) * multiplierAngleBonus;
                    else
                        angleBonus = 1.28;

                    // angleBonus = 1.28;
                    //if (distance < 90 && osuCurrent.Angle.Value < pi_over_4)
                    //    angleBonus += (1 - angleBonus) * Math.Min((90 - distance) / 10, 1);
                    //else if (distance < 90)
                    //    angleBonus += (1 - angleBonus) * Math.Min((90 - distance) / 10, 1)
                    //        * Math.Sin((pi_over_2 - osuCurrent.Angle.Value) / pi_over_4);
                }
            }

            


            return (1 + (speedBonus - 1) * 0.75) * angleBonus * (0.95 + speedBonus * Math.Pow(distance / single_spacing_threshold, 3.5)) / osuCurrent.StrainTime;
        }
    }
}
