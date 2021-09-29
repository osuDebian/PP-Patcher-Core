﻿using System;
using System.Collections.Generic;
using System.Text;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills.Pre
{
    public class SpeedBonus : PerNoteStrainSkill
    {
        private const double angle_bonus_begin = 5 * Math.PI / 6;
        private const double pi_over_4 = Math.PI / 4;
        private const double pi_over_2 = Math.PI / 2;

        private const double single_spacing_threshold = 125;
        private const double min_speed_bonus = 75; // ~200BPM
        private const double speed_balancing_factor = 40;

        private const double min_doubletap_nerf = 0.75; // minimum value (eventually on stacked)
        private const double max_doubletap_nerf = 1.0; // maximum value 
        private const double threshold_doubletap_contributing = 1.5; // minimum distance not influenced (2.0 means it is not stacked at least)

        private const double min_acute_stream_spam_nerf = 0.0; // minimum value (eventually on stacked)
        private const double max_acute_stream_spam_nerf = 1.0; // maximum value 
        private const double threshold_acute_stream_spam_contributing = 2.0; // minimum distance not influenced (2.0 means it is not stacked at least)


        private readonly double greatWindow;
        public SpeedBonus(IBeatmap beatmap, Mod[] mods, double clockRate, double hitWindowGreat) : base(beatmap, mods, clockRate)
        {
            greatWindow = hitWindowGreat;
        }

        protected override double SkillMultiplier => 1;

        protected override double StrainDecayBase => 0.3;


        protected override double StrainValueOf(DifficultyHitObject current) {
            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrent = (OsuDifficultyHitObject)current;
            var osuPrevious = Previous.Count > 0 ? (OsuDifficultyHitObject)Previous[0] : null;

            double radius = ((OsuHitObject)osuCurrent.BaseObject).Radius;
            double distance = Math.Min(single_spacing_threshold, (osuCurrent.TravelDistance + osuCurrent.JumpDistance) * osuCurrent.ScalingFactor);
            double strainTime = osuCurrent.StrainTime;

            double greatWindowFull = greatWindow * 2;
            double speedWindowRatio = strainTime / greatWindowFull;

            //Aim to nerf cheesy rhythms(Very fast consecutive doubles with large deltatimes between)
            //if (osuPrevious != null && strainTime<greatWindowFull && osuPrevious.StrainTime> strainTime)
            //    strainTime = Interpolation.Lerp(osuPrevious.StrainTime, strainTime, speedWindowRatio);

            ////Cap deltatime to the OD 300 hitwindow.
            ////0.93 is derived from making sure 260bpm OD8 streams aren't nerfed harshly, whilst 0.92 limits the effect of the cap.
            //strainTime /= Math.Clamp((strainTime / greatWindowFull) / 0.93, 0.92, 1);

            double speedBonus = 1.0;
            if (strainTime < min_speed_bonus)
            {
                double multiplierSpeedBonus = min_doubletap_nerf +
                    Math.Max(Math.Min(distance / (radius * threshold_doubletap_contributing), 1.0), 0.0)
                    * (max_doubletap_nerf - min_doubletap_nerf);

                speedBonus = 1 + Math.Pow((min_speed_bonus - strainTime) / speed_balancing_factor, 2)
                            * multiplierSpeedBonus;
            }

            double angleBonus = 1.0;

            if (osuCurrent.Angle != null && osuCurrent.Angle.Value < angle_bonus_begin)
            {
                //angleBonus = 1 + Math.Pow(Math.Sin(1.5 * (angle_bonus_begin - osuCurrent.Angle.Value)), 2) / 3.57;

                //if (osuCurrent.Angle.Value < pi_over_2)
                //{
                //    // nerf anglebonus on stacked acute stream spam

                //    double multiplierAngleBonus = min_acute_stream_spam_nerf +
                //        Math.Max(Math.Min(distance / (radius * threshold_acute_stream_spam_contributing), 1.0), 0.0)
                //        * (max_acute_stream_spam_nerf - min_acute_stream_spam_nerf)
                //        ;

                //    if (distance < 90)
                //        if (osuCurrent.Angle.Value < pi_over_4)
                //            angleBonus = (1.28 + (1 - 1.28) * Math.Min((90 - distance) / 10, 1)) * multiplierAngleBonus;
                //        else
                //            angleBonus = (1.28 + (1 - 1.28) * Math.Min((90 - distance) / 10, 1)
                //            * Math.Sin((pi_over_2 - osuCurrent.Angle.Value) / pi_over_4)) * multiplierAngleBonus;
                //    else
                //        angleBonus = 1.28;
                //    //angleBonus *= 1.5;
                //}
            }


            return (1 + (speedBonus - 1) * 0.75)
                   * angleBonus
                   * (0.95 + speedBonus * Math.Pow(distance / single_spacing_threshold, 3.5))
                   / strainTime;
        }
    }
}
