﻿using System;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Polkovnik.DroidInjector.Fody.Harvesters;
using Polkovnik.DroidInjector.Fody.Implementors;
using Polkovnik.DroidInjector.Fody.Loggers;

namespace Polkovnik.DroidInjector.Fody
{
    internal class FodyInjector
    {
        private readonly bool _autoInjectionEnabled;
        private readonly BaseModuleWeaver _baseModuleWeaver;
        private readonly ModuleDefinition _moduleDefinition;
        private ReferencesProvider _referencesProvider;
        
        public FodyInjector(ModuleDefinition moduleDefinition, BaseModuleWeaver baseModuleWeaver, bool autoInjectionEnabled)
        {
            _autoInjectionEnabled = autoInjectionEnabled;
            _baseModuleWeaver = baseModuleWeaver ?? throw new ArgumentNullException(nameof(baseModuleWeaver));
            _moduleDefinition = moduleDefinition ?? throw new ArgumentNullException(nameof(moduleDefinition));
        }
        
        public void Execute()
        {
            if (_moduleDefinition.AssemblyReferences.All(x => x.Name != "Polkovnik.DroidInjector"))
            {
                Logger.Info("Skip injecting library due to reference to Polkovnik.DroidInjector not found");
                return;
            }

            _referencesProvider = new ReferencesProvider(_moduleDefinition, _baseModuleWeaver);

            var viewHarvestQuery = new ViewHarvestQuery();
            var menuItemHarvestQuery = new MenuItemHarvestQuery();

            var queries = new MemberInfoHarvestQuery[] { viewHarvestQuery, menuItemHarvestQuery };
            var newHarvester = new MemberInfoHarvester(_moduleDefinition, queries);
            newHarvester.Execute();

            foreach (var query in queries)
            {
                Logger.Debug($"QUERY: {query} harvested: {query.QueryResult.Keys.Count}");
            }

            var injectorCallsHarvester = new InjectorCallsHarverster(_moduleDefinition);
            var harvestedInstructions = injectorCallsHarvester.Execute();

            foreach (var harvestedInstruction in harvestedInstructions)
            {
                if (harvestedInstruction.Instruction.OpCode != OpCodes.Call)
                    throw new WeavingException($"Injector.InjectViews must be called. You can't pass it as delegate. {harvestedInstruction.MethodDefinition}");
            }

            foreach (var type in viewHarvestQuery.QueryResult)
            {
                var viewInjectionImplementor = new ViewInjectionImplementor(type.Key, type.Value, _moduleDefinition, _referencesProvider, _baseModuleWeaver);
                viewInjectionImplementor.Execute();

                var activityGetViewMethodImplementor = new ActivityGetViewMethodImplementor(type.Key, _referencesProvider);

                Logger.Debug($"-- CHECK AUTOINJECT FOR {type.Key}. HAS MANUAL CALL = {harvestedInstructions.Any(x => x.MethodDefinition.DeclaringType == type.Key)}");
                
                var injectorCallReplacer = new InjectorCallReplacer(type.Key, Consts.GeneratedMethodNames.InjectViewsGeneratedMethodName, 
                    _referencesProvider.ActivityInjectViewsMethodDefinition, activityGetViewMethodImplementor, _baseModuleWeaver);
                injectorCallReplacer.Execute();

                injectorCallReplacer = new InjectorCallReplacer(type.Key, Consts.GeneratedMethodNames.InjectViewsGeneratedMethodName, 
                    _referencesProvider.InjectViewsMethodDefinition, activityGetViewMethodImplementor, _baseModuleWeaver);
                injectorCallReplacer.Execute();
            }
            
            foreach (var type in menuItemHarvestQuery.QueryResult)
            {
                var menuItemInjectionImplementor = new MenuItemInjectionImplementor(type.Key, type.Value, _moduleDefinition, _referencesProvider, _baseModuleWeaver);
                menuItemInjectionImplementor.Execute();

                var activityGetViewMethodImplementor = new ActivityGetViewMethodImplementor(type.Key, _referencesProvider);

                var injectorCallReplacer = new InjectorCallReplacer(type.Key, Consts.GeneratedMethodNames.InjectMenuItemsGeneratedMethodName,
                    _referencesProvider.InjectMenuItemsMethodDefinition, activityGetViewMethodImplementor, _baseModuleWeaver);
                injectorCallReplacer.Execute();
            }

            harvestedInstructions = injectorCallsHarvester.Execute();
            
            foreach (var harvestedInstruction in harvestedInstructions)
            {
                throw new WeavingException($"Injector.InjectViews not removed. Are you call it in async method or in class which doesn't contains [View] attribute? {harvestedInstruction.MethodDefinition}");
            }
        }
    }
}
