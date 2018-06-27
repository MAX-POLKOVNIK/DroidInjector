using System;
using System.Collections.Generic;
using System.Linq;
using Fody;
using Polkovnik.DroidInjector.Fody.Loggers;

namespace Polkovnik.DroidInjector.Fody
{
    public class ModuleWeaver : BaseModuleWeaver
    {
        public override void Execute()
        {
            var logLevelAttribute = Config.Attributes().FirstOrDefault(x => x.Name == nameof(LogLevel));
            var level = logLevelAttribute == null
                ? LogLevel.Info
                : Enum.TryParse<LogLevel>(logLevelAttribute.Value, true, out var parsed)
                    ? parsed
                    : LogLevel.Info;

            Logger.Init(level, LogInfo);

            new FodyInjector(ModuleDefinition, this).Execute();
        }
        
        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield return "mscorlib";
            yield return "System";
            yield return "Polkovnik.DroidInjector";
            yield return "Mono.Android";
        }
    }
}
