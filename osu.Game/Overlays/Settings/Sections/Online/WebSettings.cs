// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Online
{
    public class WebSettings : SettingsSubsection
    {
        protected override LocalisableString Header => "Web";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Warn about opening external links",
                    Current = config.GetBindable<bool>(OsuSetting.ExternalLinkWarning)
                },
                new SettingsCheckbox
                {
                    LabelText = "Prefer downloads without video",
                    Keywords = new[] { "no-video" },
                    Current = config.GetBindable<bool>(OsuSetting.PreferNoVideo)
                },
                new SettingsCheckbox
                {
                    LabelText = "Automatically download beatmaps when spectating",
                    Keywords = new[] { "spectator" },
                    Current = config.GetBindable<bool>(OsuSetting.AutomaticallyDownloadWhenSpectating),
                },
                new SettingsCheckbox
                {
                    LabelText = "Show explicit content in search results",
                    Keywords = new[] { "nsfw", "18+", "offensive" },
                    Current = config.GetBindable<bool>(OsuSetting.ShowOnlineExplicitContent),
                }
            };
        }
    }
}
