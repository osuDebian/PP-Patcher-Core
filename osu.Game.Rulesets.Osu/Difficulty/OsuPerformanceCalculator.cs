// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuPerformanceCalculator : PerformanceCalculator
    {
        public new OsuDifficultyAttributes Attributes => (OsuDifficultyAttributes)base.Attributes;

        private Mod[] mods;

        private double accuracy;
        //private double strainAverage;
        private int scoreMaxCombo;
        private int countGreat;
        private int countOk;
        private int countMeh;
        private int countMiss;

        public OsuPerformanceCalculator(Ruleset ruleset, DifficultyAttributes attributes, ScoreInfo score)
            : base(ruleset, attributes, score)
        {
        }

        public double CalculateRelax(Dictionary<string, double> categoryRatings = null)
        {
            mods = Score.Mods;
            accuracy = Score.Accuracy;
            scoreMaxCombo = Score.MaxCombo;
            countGreat = Score.Statistics.GetValueOrDefault(HitResult.Great);
            countOk = Score.Statistics.GetValueOrDefault(HitResult.Ok);
            countMeh = Score.Statistics.GetValueOrDefault(HitResult.Meh);
            countMiss = Score.Statistics.GetValueOrDefault(HitResult.Miss);

            // Custom multipliers for NoFail and SpunOut.
            double multiplier = 1.12; // This is being adjusted to keep the final pp value scaled around what it used to be when changing things

            if (mods.Any(m => m is OsuModNoFail))
                multiplier *= Math.Max(0.90, 1.0 - 0.02 * countMiss);

            if (mods.Any(m => m is OsuModSpunOut))
                multiplier *= 1.0 - Math.Pow((double)Attributes.SpinnerCount / totalHits, 0.85);

            double aimValue = computeAimValueRelax();
            //double speedValue = computeSpeedValue();
            double accuracyValue = computeAccuracyValueRelax();
            double totalValue =
                Math.Pow(
                    Math.Pow(aimValue, 1.1) +
                    0 +
                    Math.Pow(accuracyValue, 1.1), 1.0 / 1.1
                ) * multiplier;

            if (categoryRatings != null)
            {
                categoryRatings.Add("Aim", aimValue);
                categoryRatings.Add("Speed", 0);
                categoryRatings.Add("Accuracy", accuracyValue);
                categoryRatings.Add("OD", Attributes.OverallDifficulty);
                categoryRatings.Add("AR", Attributes.ApproachRate);
                categoryRatings.Add("Max Combo", Attributes.MaxCombo);
            }

            return totalValue;
        }

        public override double Calculate(Dictionary<string, double> categoryRatings = null)
        {
            mods = Score.Mods;
            accuracy = Score.Accuracy;
            scoreMaxCombo = Score.MaxCombo;
            countGreat = Score.Statistics.GetValueOrDefault(HitResult.Great);
            countOk = Score.Statistics.GetValueOrDefault(HitResult.Ok);
            countMeh = Score.Statistics.GetValueOrDefault(HitResult.Meh);
            countMiss = Score.Statistics.GetValueOrDefault(HitResult.Miss);

            if(mods.Any(it=>it is OsuModRelax))
            {
                return CalculateRelax(categoryRatings);
            }

            // Custom multipliers for NoFail and SpunOut.
            double multiplier = 1.2; // This is being adjusted to keep the final pp value scaled around what it used to be when changing things

            if (mods.Any(m => m is OsuModNoFail))
                multiplier *= Math.Max(0.90, 1.0 - 0.02 * countMiss);

            if (mods.Any(m => m is OsuModSpunOut))
                multiplier *= 1.0 - Math.Pow((double)Attributes.SpinnerCount / totalHits, 0.85);

            double aimValue = computeAimValue();
            //double speedValue = computeSpeedValue();
            double speedValue = 0;
            double accuracyValue = computeAccuracyValue();
            double totalValue =
                Math.Pow(
                    Math.Pow(aimValue, 1.1) +
                    Math.Pow(speedValue, 1.1) +
                    Math.Pow(accuracyValue, 1.1), 1.0 / 1.1
                ) * multiplier;

            if (categoryRatings != null)
            {
                categoryRatings.Add("Aim", aimValue);
                categoryRatings.Add("Speed", speedValue);
                categoryRatings.Add("Accuracy", accuracyValue);
                categoryRatings.Add("OD", Attributes.OverallDifficulty);
                categoryRatings.Add("AR", Attributes.ApproachRate);
                categoryRatings.Add("Max Combo", Attributes.MaxCombo);
            }

            return totalValue;
        }

        public double toRadians(double degree)
        {
            return Math.PI * degree / 180.0;
        }

        private double computeAimValueRelax()
        {
            double rawAim = Attributes.AimStrain;

            if (mods.Any(m => m is OsuModTouchDevice))
                rawAim = Math.Pow(rawAim, 0.8);

            double aimValue = Math.Pow(5.0 * Math.Max(1.0, rawAim / 0.0675) - 4.0, 3.0) / 100000.0;

            // Longer maps are worth more
            // 릴렉스 연타 버프의 큰 원인은 갯수 버프가 크다 잡아야한다
            // 연타를 파악하는 방법이 근본적으로 쉽진 않지만 통계적 방법으로 간단하게 구해볼 수는 있다.
            // 먼저 가장 넓은 노트 점프 거리에 대해 상위 n%의 평균(a)과 모든 노트 점프 거리에 대한 평균을 구한다(b).
            // b를 a로 나누게 되면 반드시 0에서 1 사이 값이 나온다(JumpRate).
            // 이 값이 낮다면 디스턴스가 넓은 부분에 비해 디스턴스가 좁은 부분이 매우 많다는 것으로 해석할 수 있다. 
            // 디스턴스만 본다면 연타가 점프에 비해 좁기 때문에 JumpRate가 낮아진다.

            // stream nerf
            
            double JumpRate = (Attributes.DistanceAverage / Attributes.DistanceTop);
            double StreamThresholdLength = 0.8;
            double StreamFirst = Math.Max(StreamThresholdLength - JumpRate, 0);
            //double StreamFirst = Math.Max((1.0 / 2.0) * Math.Cos(toRadians(180 + (180.0) * (JumpRate - 0.3) * (1 / 0.4))) + 0.5, 0);
            // 0.3의 값일때 cos(180)
            // 0.7의 값일때 cos(360)
            // 1 / 2 * cos(180 + (180) * (x - 0.3) * (1 / 0.4)) + 1
            //double StreamNerfRateLength = 1 - Math.Max(StreamFirst * 1.2, 0);
            //Console.WriteLine("lengthBonusRate: " + StreamFirst + ", " + JumpRate + ", " + (180 + (180.0) * (JumpRate - 0.3) * (1 / 0.4)));
            //if (JumpRate <= 0.3) StreamNerfRateLength = 0;
            //if (JumpRate >= 0.7) StreamNerfRateLength = 1;
            double StreamNerfRateLength = Math.Max(1 - StreamFirst * 2, 0);

            //Console.WriteLine(Attributes.HitCircleCount + ", "
            //    + totalHits + ", "
            //    + JumpRate + ", "
            //    + StreamNerfRateLength);


            //double lengthBonus = 0.95
            //                        + 0.05 * Math.Min(1.0, totalHits / 500) * StreamNerfRateLength
            //                        + 0.3 * Math.Max(Math.Min(1.0, (totalHits - 500) / 500), 0) * StreamNerfRateLength
            //                        + 0.7 * Math.Max(Math.Min(1.0, (totalHits - 1000) / 2000.0), 0) * StreamNerfRateLength;
            //                        ;
            double lengthBonus = 0.95 + (0.4 * Math.Min(1.0, totalHits / 2000.0) +
                     (totalHits > 2000 ? Math.Log10(totalHits / 2000.0) * 0.5 : 0.0)) * StreamNerfRateLength;
            //Console.WriteLine(lengthBonus + ", " + JumpRate);
            //Console.WriteLine(lengthBonus);
            aimValue *= lengthBonus;

            // Penalize misses by assessing # of misses relative to the total # of objects. Default a 3% reduction for any # of misses.
            //if (countMiss > 0)
            //    aimValue *= 0.97 * Math.Pow(1 - Math.Pow((double)countMiss / totalHits, 0.775), countMiss);
            if (countMiss > 0)
                aimValue *= Math.Pow(0.95, countMiss);
            
            // Combo scaling
            if (Attributes.MaxCombo > 0)
                aimValue *= Math.Min(Math.Pow(scoreMaxCombo, 0.8) / Math.Pow(Attributes.MaxCombo, 0.8), 1.0);

            // 기존 고AR 보너스, 저AR보너스 삭제
            // 저AR보너스는 아래에 다시 개발
            //double approachRateFactor = 0.0;
            //if (Attributes.ApproachRate > 10.33)
            //    approachRateFactor = (Attributes.ApproachRate - 10.33) / 4;
            //else if (Attributes.ApproachRate < 8.0)
            //    approachRateFactor = 0.025 * (8.0 - Attributes.ApproachRate);

            //double approachRateTotalHitsFactor = 1.0 / (1.0 + Math.Exp(-(0.007 * (totalHits * StreamNerfRateLength - 400))));

            //double approachRateBonus = 1.0 + (0.03 + 0.37 * approachRateTotalHitsFactor) * approachRateFactor;
            //aimValue *= approachRateBonus;

            // We want to give more reward for lower AR when it comes to aim and HD. This nerfs high AR and buffs lower AR.

            // low AR buff
            // 이지 유저 보완 코드.
            // AR이 낮으면 낮을수록 아주 크게 버프받는다. AR 4까지 보장
            // aimValue *= log(10 + (12 - AR)^(2.5)) / 2
            // hidden multiplier 1.5
            double lowarBonus = Math.Log10(9
                + Math.Pow(Math.Min((12 - Attributes.ApproachRate), 8), 1.5) * 2 // 42.22
                * (mods.Any(h => h is OsuModHidden) ? 1.5 : 1));
            //Console.WriteLine(lowarBonus);
            aimValue *= lowarBonus;

            // aim buff
            // aimValue *= max(DistanceTop - 0.6, 0)
            //aimValue *= Math.Max((Attributes.DistanceTop - 0.5) * 2, 1);
            //aimValue *= 1 + Math.Max(JumpRate - 0.5, 0) / 2;

            // stream nerf
            // aimValue *= 1 - max(0.5 - DistanceTop, 0)
            //double StreamThreshold = 0.7;
            //double StreamNerfRate = 1 - Math.Max(StreamThreshold - JumpRate, 0) * 0.1;
            //aimValue *= StreamNerfRate;


            double flashlightBonus = 1.0;

            if (mods.Any(h => h is OsuModFlashlight))
            {
                // Apply object-based bonus for flashlight.
                
                flashlightBonus = 1.0 + (0.35 * Math.Min(1.0, totalHits / 200.0) +
                                  (totalHits > 200
                                      ? 0.3 * Math.Min(1.0, (totalHits - 200) / 300.0) +
                                        (totalHits > 500 ? (totalHits - 500) / 1200.0 : 0.0)
                                      : 0.0));
            }
            aimValue *= flashlightBonus;

            // 이걸 대체 왜한거지?
            //aimValue *= Math.Max(flashlightBonus, approachRateBonus);

            // Scale the aim value with accuracy _slightly_
            aimValue *= 0.5 + accuracy / 2.0;
            //aimValue *= accuracy;
            // It is important to also consider accuracy difficulty when doing that
            aimValue *= 0.98 + Math.Pow(Attributes.OverallDifficulty, 2) / 2500;

            return aimValue;
        }

        private double computeAimValue()
        {
            double rawAim = Attributes.AimStrain;

            if (mods.Any(m => m is OsuModTouchDevice))
                rawAim = Math.Pow(rawAim, 0.8);

            double aimValue = Math.Pow(5.0 * Math.Max(1.0, rawAim / 0.0675) - 4.0, 3.0) / 100000.0;

            // Longer maps are worth more
            double lengthBonus = 0.95 + 0.4 * Math.Min(1.0, totalHits / 2000.0) +
                                 (totalHits > 2000 ? Math.Log10(totalHits / 2000.0) * 0.5 : 0.0);
            //double lengthBonus = 0.95
            //            + 0.05 * Math.Min(1.0, totalHits / 500)
            //            + 0.2 * Math.Max(Math.Min(1.0, (totalHits - 500) / 500), 0)
            //            + 0.4 * Math.Max(Math.Min(1.0, (totalHits - 1000) / 2000.0), 0);
            //;


            aimValue *= lengthBonus;

            // Penalize misses by assessing # of misses relative to the total # of objects. Default a 3% reduction for any # of misses.
            if (countMiss > 0)
                aimValue *= 0.97 * Math.Pow(1 - Math.Pow((double)countMiss / totalHits, 0.775), countMiss);

            // Combo scaling
            if (Attributes.MaxCombo > 0)
                aimValue *= Math.Min(Math.Pow(scoreMaxCombo, 0.8) / Math.Pow(Attributes.MaxCombo, 0.8), 1.0);

            //double approachRateFactor = 0.0;
            //if (Attributes.ApproachRate > 10.33)
            //    approachRateFactor = Attributes.ApproachRate - 10.33;
            //else if (Attributes.ApproachRate < 8.0)
            //    approachRateFactor = 0.025 * (8.0 - Attributes.ApproachRate);

            //double approachRateTotalHitsFactor = 1.0 / (1.0 + Math.Exp(-(0.007 * (totalHits - 400))));

            //double approachRateBonus = 1.0 + (0.03 + 0.37 * approachRateTotalHitsFactor) * approachRateFactor;

            // We want to give more reward for lower AR when it comes to aim and HD. This nerfs high AR and buffs lower AR.
            //if (mods.Any(h => h is OsuModHidden))
            //    aimValue *= 1.0 + 0.04 * (12.0 - Attributes.ApproachRate);

            double flashlightBonus = 1.0;

            if (mods.Any(h => h is OsuModFlashlight))
            {
                // Apply object-based bonus for flashlight.
                flashlightBonus = 1.0 + 0.35 * Math.Min(1.0, totalHits / 200.0) +
                                  (totalHits > 200
                                      ? 0.3 * Math.Min(1.0, (totalHits - 200) / 300.0) +
                                        (totalHits > 500 ? (totalHits - 500) / 1200.0 : 0.0)
                                      : 0.0);
            }

            aimValue *= flashlightBonus;

            // low AR buff
            // 이지 유저 보완 코드.
            // AR이 낮으면 낮을수록 아주 크게 버프받는다. AR 4까지 보장
            // aimValue *= log(10 + (12 - AR)^(2.5)) / 2
            // hidden multiplier 1.8
            double lowarBonus = Math.Log10(9
                + Math.Pow(Math.Min(12 - Attributes.ApproachRate, 8), 1.5) * 2 // 42.22
                * (mods.Any(h => h is OsuModHidden) ? 1.5 : 1));
            //Console.WriteLine(lowarBonus);
            aimValue *= lowarBonus;

            

            //aimValue *= Math.Max(flashlightBonus, approachRateBonus);

            // Scale the aim value with accuracy _slightly_
            aimValue *= 0.5 + accuracy / 2.0;
            // It is important to also consider accuracy difficulty when doing that
            aimValue *= 0.98 + Math.Pow(Attributes.OverallDifficulty, 2) / 2500;

            return aimValue;
        }

        private double computeSpeedValue()
        {
            double speedValue = Math.Pow(5.0 * Math.Max(1.0, Attributes.SpeedStrain / 0.0675) - 4.0, 3.0) / 100000.0;

            // Longer maps are worth more
            double lengthBonus = 0.95 + 0.4 * Math.Min(1.0, totalHits / 2000.0) +
                                 (totalHits > 2000 ? Math.Log10(totalHits / 2000.0) * 0.5 : 0.0);
            speedValue *= lengthBonus;

            // Penalize misses by assessing # of misses relative to the total # of objects. Default a 3% reduction for any # of misses.
            if (countMiss > 0)
                speedValue *= 0.97 * Math.Pow(1 - Math.Pow((double)countMiss / totalHits, 0.775), Math.Pow(countMiss, .875));

            // Combo scaling
            if (Attributes.MaxCombo > 0)
                speedValue *= Math.Min(Math.Pow(scoreMaxCombo, 0.8) / Math.Pow(Attributes.MaxCombo, 0.8), 1.0);

            double approachRateFactor = 0.0;
            if (Attributes.ApproachRate > 10.33)
                approachRateFactor = Attributes.ApproachRate - 10.33;

            double approachRateTotalHitsFactor = 1.0 / (1.0 + Math.Exp(-(0.007 * (totalHits - 400))));

            speedValue *= 1.0 + (0.03 + 0.37 * approachRateTotalHitsFactor) * approachRateFactor;

            if (mods.Any(m => m is OsuModHidden))
                speedValue *= 1.0 + 0.04 * (12.0 - Attributes.ApproachRate);

            // Scale the speed value with accuracy and OD
            speedValue *= (0.95 + Math.Pow(Attributes.OverallDifficulty, 2) / 750) * Math.Pow(accuracy, (14.5 - Math.Max(Attributes.OverallDifficulty, 8)) / 2);
            // Scale the speed value with # of 50s to punish doubletapping.
            speedValue *= Math.Pow(0.98, countMeh < totalHits / 500.0 ? 0 : countMeh - totalHits / 500.0);

            return speedValue;
        }

        private double computeAccuracyValueRelax()
        {
            // This percentage only considers HitCircles of any value - in this part of the calculation we focus on hitting the timing hit window
            double betterAccuracyPercentage;
            int amountHitObjectsWithAccuracy = Attributes.HitCircleCount;

            if (amountHitObjectsWithAccuracy > 0)
                betterAccuracyPercentage = ((countGreat - (totalHits - amountHitObjectsWithAccuracy)) * 6 + countOk * 2 + countMeh) / (double)(amountHitObjectsWithAccuracy * 6);
            else
                betterAccuracyPercentage = 0;

            // It is possible to reach a negative accuracy with this formula. Cap it at zero - zero points
            if (betterAccuracyPercentage < 0)
                betterAccuracyPercentage = 0;

            // Lots of arbitrary values from testing.
            // Considering to use derivation from perfect accuracy in a probabilistic manner - assume normal distribution
            double accuracyValue = Math.Pow(1.52163, Attributes.OverallDifficulty) * Math.Pow(betterAccuracyPercentage, 26) * 2.83 * 1.1;

            // Bonus for many hitcircles - it's harder to keep good accuracy up for longer
            // acc 스트림 너프
            double JumpRate = (Attributes.DistanceAverage / Attributes.DistanceTop);
            double StreamThresholdLength = 0.7;
            double StreamFirstLength = Math.Max((StreamThresholdLength - JumpRate), 0);
            double StreamNerfRateLength = Math.Max(1 - StreamFirstLength * 2.5, 0.05);
            //Console.WriteLine(StreamNerfRateLength);
            //Console.WriteLine(Math.Min(1.15, Math.Pow(amountHitObjectsWithAccuracy / 2000.0 * StreamNerfRateLength, 0.3)));
            accuracyValue *= Math.Min(1.15, Math.Pow(amountHitObjectsWithAccuracy / 1000.0 * StreamNerfRateLength, 0.3));

            if (mods.Any(m => m is OsuModHidden))
                accuracyValue *= 1.08;
            if (mods.Any(m => m is OsuModFlashlight))
                accuracyValue *= 1.02;

            return accuracyValue;
        }
        private double computeAccuracyValue()
        {
            // This percentage only considers HitCircles of any value - in this part of the calculation we focus on hitting the timing hit window
            double betterAccuracyPercentage;
            int amountHitObjectsWithAccuracy = Attributes.HitCircleCount;

            if (amountHitObjectsWithAccuracy > 0)
                betterAccuracyPercentage = ((countGreat - (totalHits - amountHitObjectsWithAccuracy)) * 6 + countOk * 2 + countMeh) / (double)(amountHitObjectsWithAccuracy * 6);
            else
                betterAccuracyPercentage = 0;

            // It is possible to reach a negative accuracy with this formula. Cap it at zero - zero points
            if (betterAccuracyPercentage < 0)
                betterAccuracyPercentage = 0;

            // Lots of arbitrary values from testing.
            // Considering to use derivation from perfect accuracy in a probabilistic manner - assume normal distribution
            double accuracyValue = Math.Pow(1.52163, Attributes.OverallDifficulty) * Math.Pow(betterAccuracyPercentage, 24) * 2.83;

            // Bonus for many hitcircles - it's harder to keep good accuracy up for longer
            accuracyValue *= Math.Min(1.15, Math.Pow(amountHitObjectsWithAccuracy / 2000.0, 0.3));

            if (mods.Any(m => m is OsuModHidden))
                accuracyValue *= 1.08;
            if (mods.Any(m => m is OsuModFlashlight))
                accuracyValue *= 1.02;

            return accuracyValue;
        }

        private int totalHits => countGreat + countOk + countMeh + countMiss;
        private int totalSuccessfulHits => countGreat + countOk + countMeh;
    }
}
