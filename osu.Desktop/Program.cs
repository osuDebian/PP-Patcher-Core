// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Development;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.IPC;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Tournament;
using osu.Game.Rulesets.Osu;
using osu.Framework.Graphics.Textures;
using osu.Framework.Audio.Track;
using osu.Game.Skinning;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Difficulty;
//using osu.Game.Tests.Beatmaps;

namespace osu.Desktop
{


    public static class Program
    {
        private const string base_game_name = @"osu";

        [STAThread]
        public static int Main(string[] args)
        {
            List<string> routes = new List<string>
            {
                "maps/SYU (from GALNERYUS) - REASON (BarkingMadDog) [A THOUSAND SWORDS].osu",
                "maps/xi - Ascension to Heaven (Kroytz) [Final Moment].osu",
                "maps/Wake Up, May'n! - One In A Billion (A r M i N) [Fantasy].osu",
                "maps/Aitsuki Nakuru - Presenter (Hanazawa Kana) [Gift].osu",
                "maps/katagiri - Sendan Life (katagiri Bootleg) (Settia) [Destroy the World].osu",
                "maps/ryu5150 - Louder than steel (ParkourWizard) [ok this is epic].osu",
                "maps/GYZE - HONESTY (Bibbity Bill) [DISHONEST].osu",
                "maps/Okazaki Taiiku - Kimi no Bouken (TV Size) (fieryrage) [New Adventure!].osu"
            };

            Mod[] modsNone = new Mod[]
            {
                //new OsuModDoubleTime(),
            };

            Mod[] modsDT = new Mod[]
            {
                new OsuModDoubleTime(),
                new OsuModHardRock(),
            };

            foreach (var route in routes)
            {
                var beatmap = GetBeatmap(route);
                var ruleset = new OsuRuleset();
                var diffCalculator = new OsuDifficultyCalculator(ruleset, new DummyConversionBeatmap(beatmap));

                var result = diffCalculator.Calculate(modsNone);
                var perfectScoreInfo = GetPerfectScoreInfo(modsNone, beatmap, result);

                var resultDT = diffCalculator.Calculate(modsDT);
                var perfectScoreInfoDT = GetPerfectScoreInfo(modsDT, beatmap, resultDT);

                var ppCalculator = new OsuPerformanceCalculator(ruleset, result, perfectScoreInfo);
                var ppCalculatorDT = new OsuPerformanceCalculator(ruleset, resultDT, perfectScoreInfoDT);

                Console.WriteLine("  == " + route + " ==  ");
                Console.WriteLine("bancho: " + ppCalculator.Calculate());
                Console.WriteLine("relax: " + ppCalculator.CalculateRelax());
                Console.WriteLine("relaxDT: " + ppCalculatorDT.CalculateRelax());
            }

            return 0;
        }

        public static ScoreInfo GetPerfectScoreInfo(Mod[] mods, Beatmap beatmap, DifficultyAttributes result)
        {
            var perfectScoreInfo = new ScoreInfo()
            {
                Mods = mods,
                Accuracy = 1,
                Combo = result.MaxCombo,
                MaxCombo = result.MaxCombo,
            };
            perfectScoreInfo.Statistics.Add(Game.Rulesets.Scoring.HitResult.Great, beatmap.HitObjects.Count);
            perfectScoreInfo.Statistics.Add(Game.Rulesets.Scoring.HitResult.Ok, 0);
            perfectScoreInfo.Statistics.Add(Game.Rulesets.Scoring.HitResult.Meh, 0);
            perfectScoreInfo.Statistics.Add(Game.Rulesets.Scoring.HitResult.Miss, 0);

            return perfectScoreInfo;
        }

        public static Beatmap GetBeatmap(string route)
        {
            var decoder = new LegacyBeatmapDecoder();

            //using (var resStream = TestResources.OpenResource("Soleily - Renatus (Gamu) [Insane].osu"))
            using (var stream = new LineBufferedReader(File.OpenRead(route)))
            {
                var beatmap = decoder.Decode(stream);

                return beatmap;
            }
        }



        //[STAThread]
        //public static int Main(string[] args)
        //{
        //    // Back up the cwd before DesktopGameHost changes it
        //    var cwd = Environment.CurrentDirectory;

        //    string gameName = base_game_name;
        //    bool tournamentClient = false;

        //    foreach (var arg in args)
        //    {
        //        var split = arg.Split('=');

        //        var key = split[0];
        //        var val = split.Length > 1 ? split[1] : string.Empty;

        //        switch (key)
        //        {
        //            case "--tournament":
        //                tournamentClient = true;
        //                break;

        //            case "--debug-client-id":
        //                if (!DebugUtils.IsDebugBuild)
        //                    throw new InvalidOperationException("Cannot use this argument in a non-debug build.");

        //                if (!int.TryParse(val, out int clientID))
        //                    throw new ArgumentException("Provided client ID must be an integer.");

        //                gameName = $"{base_game_name}-{clientID}";
        //                break;
        //        }
        //    }

        //    using (DesktopGameHost host = Host.GetSuitableHost(gameName, true))
        //    {
        //        host.ExceptionThrown += handleException;

        //        if (!host.IsPrimaryInstance)
        //        {
        //            if (args.Length > 0 && args[0].Contains('.')) // easy way to check for a file import in args
        //            {
        //                var importer = new ArchiveImportIPCChannel(host);

        //                foreach (var file in args)
        //                {
        //                    Console.WriteLine(@"Importing {0}", file);
        //                    if (!importer.ImportAsync(Path.GetFullPath(file, cwd)).Wait(3000))
        //                        throw new TimeoutException(@"IPC took too long to send");
        //                }

        //                return 0;
        //            }

        //            // we want to allow multiple instances to be started when in debug.
        //            if (!DebugUtils.IsDebugBuild)
        //                return 0;
        //        }

        //        if (tournamentClient)
        //            host.Run(new TournamentGame());
        //        else
        //            host.Run(new OsuGameDesktop(args));

        //        return 0;
        //    }
        //}

        private static int allowableExceptions = DebugUtils.IsDebugBuild ? 0 : 1;

        /// <summary>
        /// Allow a maximum of one unhandled exception, per second of execution.
        /// </summary>
        /// <param name="arg"></param>
        private static bool handleException(Exception arg)
        {
            bool continueExecution = Interlocked.Decrement(ref allowableExceptions) >= 0;

            Logger.Log($"Unhandled exception has been {(continueExecution ? $"allowed with {allowableExceptions} more allowable exceptions" : "denied")} .");

            // restore the stock of allowable exceptions after a short delay.
            Task.Delay(1000).ContinueWith(_ => Interlocked.Increment(ref allowableExceptions));

            return continueExecution;
        }
    }

    internal class DummyConversionBeatmap : WorkingBeatmap
    {
        private readonly IBeatmap beatmap;

        public DummyConversionBeatmap(IBeatmap beatmap)
            : base(beatmap.BeatmapInfo, null)
        {
            this.beatmap = beatmap;
        }

        protected override IBeatmap GetBeatmap() => beatmap;
        protected override Texture GetBackground() => null;
        protected override Track GetBeatmapTrack() => null;
        protected override ISkin GetSkin() => null;
        public override Stream GetStream(string storagePath) => null;
    }
}
