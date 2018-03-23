using System;
using System.Linq;
using Mono.Cecil;
using Polkovnik.DroidInjector.Fody.Harvesters;
using Polkovnik.DroidInjector.Fody.Implementors;
using Polkovnik.DroidInjector.Fody.Loggers;

namespace Polkovnik.DroidInjector.Fody
{
    internal class FodyInjector
    {
        private readonly IAssemblyResolver _assemblyResolver;
        private readonly ModuleDefinition _moduleDefinition;
        private ReferencesProvider _referencesProvider;
        
        public FodyInjector(ModuleDefinition moduleDefinition, IAssemblyResolver assemblyResolver)
        {
            _assemblyResolver = assemblyResolver ?? throw new ArgumentNullException(nameof(assemblyResolver));
            _moduleDefinition = moduleDefinition ?? throw new ArgumentNullException(nameof(moduleDefinition));
        }
        
        public void Execute()
        {
            if (_moduleDefinition.AssemblyReferences.All(x => x.Name != "Polkovnik.DroidInjector"))
            {
                Logger.Info("Skip injecting library due to reference to Polkovnik.DroidInjector not found");
                return;
            }

            _referencesProvider = new ReferencesProvider(_moduleDefinition, _assemblyResolver);

            var viewHarvester = new ViewHarvester(_moduleDefinition);
            var requiredToInject = viewHarvester.Harvest();
            
            foreach (var type in requiredToInject)
            {
                var viewInjectionImplementor = new ViewInjectionImplementor(type.Key, type.Value, _moduleDefinition, _referencesProvider);
                viewInjectionImplementor.Execute();

                var activityGetViewMethodImplementor = new ActivityGetViewMethodImplementor(type.Key, _referencesProvider);

                var injectorCallReplacer = new InjectorCallReplacer(type.Key, Consts.GeneratedMethodNames.InjectViewsGeneratedMethodName, 
                    _referencesProvider.ActivityInjectViewsMethodDefinition, activityGetViewMethodImplementor);
                injectorCallReplacer.Execute();

                injectorCallReplacer = new InjectorCallReplacer(type.Key, Consts.GeneratedMethodNames.InjectViewsGeneratedMethodName, 
                    _referencesProvider.InjectViewsMethodReference, activityGetViewMethodImplementor);
                injectorCallReplacer.Execute();
            }

            var menuItemsHarvester = new MenuItemHarvester(_moduleDefinition);
            requiredToInject = menuItemsHarvester.Harvest();

            foreach (var type in requiredToInject)
            {
                var menuItemInjectionImplementor = new MenuItemInjectionImplementor(type.Key, type.Value, _moduleDefinition, _referencesProvider);
                menuItemInjectionImplementor.Execute();

                var activityGetViewMethodImplementor = new ActivityGetViewMethodImplementor(type.Key, _referencesProvider);

                var injectorCallReplacer = new InjectorCallReplacer(type.Key, Consts.GeneratedMethodNames.InjectMenuItemsGeneratedMethodName,
                    _referencesProvider.InjectMenuItemsMethodDefinition, activityGetViewMethodImplementor);
                injectorCallReplacer.Execute();
            }

            var subscriptionHarverster = new ViewEventHarvester(_moduleDefinition);
            var requiredToSubscribe = subscriptionHarverster.Harvest();

            foreach (var type in requiredToSubscribe)
            {
                var methodSubscriptionImplementor = new MethodSubscriptionImplementor(type.Key, type.Value.Cast<MethodDefinition>().ToArray(), _moduleDefinition,
                    _referencesProvider);

                methodSubscriptionImplementor.Execute();

                var activityGetViewMethodImplementor = new ActivityGetViewMethodImplementor(type.Key, _referencesProvider);

                var injectorCallReplacer = new InjectorCallReplacer(type.Key, Consts.GeneratedMethodNames.BindViewEventsGeneratedMethodName, 
                    _referencesProvider.ActivityBindViewEventsMethodDefinition,
                    activityGetViewMethodImplementor);
                injectorCallReplacer.Execute();

                injectorCallReplacer = new InjectorCallReplacer(type.Key, Consts.GeneratedMethodNames.BindViewEventsGeneratedMethodName, 
                    _referencesProvider.BindViewEventsMethodDefinition, activityGetViewMethodImplementor);
                injectorCallReplacer.Execute();
            }
        }
    }
}
