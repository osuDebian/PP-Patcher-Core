// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Gameplay
{
    public class ModsSettings : SettingsSubsection
    {
        protected override LocalisableString Header => "Mods";

        public override IEnumerable<string> FilterTerms => base.FilterTerms.Concat(new[] { "mod" });

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Increase visibility of first object when visual impairment mods are enabled",
                    Current = config.GetBindable<bool>(OsuSetting.IncreaseFirstObjectVisibility),
                },
            };
        }
    }
}
