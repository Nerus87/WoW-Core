// Copyright (c) Arctium Emulation.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Arctium.Core.Configuration;
using Arctium.Core.Logging;

namespace Arctium.Manager.Misc
{
    class Config : ConfigBase<Config>
    {
        [ConfigEntry("Log.Level", LogTypes.All)]
        public static LogTypes LogLevel { get; }

        [ConfigEntry("Log.Directory", "Logs/Manager")]
        public static string LogDirectory { get; }

        [ConfigEntry("Log.Console.File", "Console.log")]
        public static string LogConsoleFile { get; }
    }
}
