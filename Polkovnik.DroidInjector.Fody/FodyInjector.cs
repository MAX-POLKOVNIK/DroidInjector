using System;
using System.Linq;
using Mono.Cecil;
using Polkovnik.DroidInjector.Fody.Harvesters;

namespace Polkovnik.DroidInjector.Fody
{
    internal class FodyInjector
    {
        

        private readonly ModuleDefinition _moduleDefinition;
        private readonly ReferencesAndDefinitionsProvider _referencesAndDefinitionsProvider;
        
        public FodyInjector(ModuleDefinition moduleDefinition, IAssemblyResolver assemblyResolver)
        {
            _moduleDefinition = moduleDefinition ?? throw new ArgumentNullException(nameof(moduleDefinition));
            _referencesAndDefinitionsProvider = new ReferencesAndDefinitionsProvider(moduleDefinition, assemblyResolver);
        }
        
        public void Execute()
        {
            var viewHarvester = new ViewHarvester(_moduleDefinition);
            var requiredToInject = viewHarvester.Harvest();
            
            foreach (var type in requiredToInject)
            {
                var viewInjectionImplementor = new ViewInjectionImplementor(type.Key, type.Value, _moduleDefinition, _referencesAndDefinitionsProvider);
                viewInjectionImplementor.Execute();

                var activityGetViewMethodImplementor = new ActivityGetViewMethodImplementor(type.Key, _referencesAndDefinitionsProvider);

                var injectorCallReplacer = new InjectorCallReplacer(type.Key, Consts.GeneratedMethodNames.InjectViewsGeneratedMethodName, 
                    _referencesAndDefinitionsProvider.ActivityInjectViewsMethodDefinition, activityGetViewMethodImplementor);
                injectorCallReplacer.Execute();

                injectorCallReplacer = new InjectorCallReplacer(type.Key, Consts.GeneratedMethodNames.InjectViewsGeneratedMethodName, 
                    _referencesAndDefinitionsProvider.InjectViewsMethodReference, activityGetViewMethodImplementor);
                injectorCallReplacer.Execute();
            }

            var menuItemsHarvester = new MenuItemHarvester(_moduleDefinition);
            requiredToInject = menuItemsHarvester.Harvest();

            foreach (var type in requiredToInject)
            {
                var menuItemInjectionImplementor = new MenuItemInjectionImplementor(type.Key, type.Value, _moduleDefinition, _referencesAndDefinitionsProvider);
                menuItemInjectionImplementor.Execute();

                var activityGetViewMethodImplementor = new ActivityGetViewMethodImplementor(type.Key, _referencesAndDefinitionsProvider);

                var injectorCallReplacer = new InjectorCallReplacer(type.Key, Consts.GeneratedMethodNames.InjectMenuItemsGeneratedMethodName,
                    _referencesAndDefinitionsProvider.InjectMenuItemsMethodDefinition, activityGetViewMethodImplementor);
                injectorCallReplacer.Execute();
            }

            var subscriptionHarverster = new ViewEventHarvester(_moduleDefinition);
            var requiredToSubscribe = subscriptionHarverster.Harvest();

            foreach (var type in requiredToSubscribe)
            {
                var methodSubscriptionImplementor = new MethodSubscriptionImplementor(type.Key, type.Value.Cast<MethodDefinition>().ToArray(), _moduleDefinition,
                    _referencesAndDefinitionsProvider);

                methodSubscriptionImplementor.Execute();

                var activityGetViewMethodImplementor = new ActivityGetViewMethodImplementor(type.Key, _referencesAndDefinitionsProvider);

                var injectorCallReplacer = new InjectorCallReplacer(type.Key, Consts.GeneratedMethodNames.BindViewEventsGeneratedMethodName, 
                    _referencesAndDefinitionsProvider.ActivityBindViewEventsMethodDefinition,
                    activityGetViewMethodImplementor);
                injectorCallReplacer.Execute();

                injectorCallReplacer = new InjectorCallReplacer(type.Key, Consts.GeneratedMethodNames.BindViewEventsGeneratedMethodName, 
                    _referencesAndDefinitionsProvider.BindViewEventsMethodDefinition, activityGetViewMethodImplementor);
                injectorCallReplacer.Execute();
            }
        }
    }
}
