using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuPerNoteDatabase
    {
        public double averageDeltaTime;
        public List<double> strainsNoteAngle;
        public List<double> strainsFingerControl;
        public List<double> strainsSliderVelocity;
        public List<double> strainsSpeedBonus;
    }
}
