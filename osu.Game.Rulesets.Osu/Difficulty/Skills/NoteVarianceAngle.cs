using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class NoteVarianceAngle
    {
        private OsuPerNoteDatabase database;
        public NoteVarianceAngle(OsuPerNoteDatabase database)
        {
            this.database = database;
        }

        public void setDatabase()
        {
            
        }
    }
}
