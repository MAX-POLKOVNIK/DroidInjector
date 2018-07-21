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

            var autoInjectionAttribute = Config.Attributes().FirstOrDefault(x => x.Name == "EnableAutoInjection");
            var autoInjectionEnabled = autoInjectionAttribute != null && bool.TryParse(autoInjectionAttribute.Value, out var enabled) && enabled;

            new FodyInjector(ModuleDefinition, this, autoInjectionEnabled).Execute();
        }
        
        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield return "mscorlib";
            yield return "System";
            yield return "Polkovnik.DroidInjector";
            yield return "Mono.Android";
            yield return "Xamarin.Android.Support.v7.RecyclerView";
            yield return "Android.Support.V4.App.Fragment";
        }
    }
}
