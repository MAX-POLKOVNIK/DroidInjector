using System;
using System.Linq;
using Mono.Cecil;
using Polkovnik.DroidInjector.Fody.Harvesters;

namespace Polkovnik.DroidInjector.Fody
{
    internal class FodyInjector
    {
        private const string InjectViewsGeneratedMethodName = "Polkovnik_DroidInjector_InjectViews";
        private const string BindViewEventsGeneratedMethodName = "Polkovnik_DroidInjector_BindViewEvents";

        private readonly ModuleDefinition _moduleDefinition;
        private readonly IAssemblyResolver _assemblyResolver;

        private TypeReference _androidViewTypeReference;
        private MethodReference _findViewByIdMethodDefinition;
        private MethodDefinition _activityInjectViewsMethodDefinition;
        private MethodDefinition _injectViewsMethodReference;
        private MethodReference _activityFindViewByIdMethodReference;
        private TypeDefinition _resourceIdClassType;
        private MethodReference _injectorExceptionCtor;
        private MethodDefinition _activityBindViewEventsMethodDefinition;
        private MethodDefinition _bindViewEventsMethodDefinition;
        
        public FodyInjector(ModuleDefinition moduleDefinition, IAssemblyResolver assemblyResolver)
        {
            _assemblyResolver = assemblyResolver ?? throw new ArgumentNullException(nameof(assemblyResolver));
            _moduleDefinition = moduleDefinition ?? throw new ArgumentNullException(nameof(moduleDefinition));
        }
        
        public void Execute()
        {
            FindRequiredTypesAndMethods();
            
            var viewHarvester = new ViewHarvester(_moduleDefinition);
            var requiredToInject = viewHarvester.Harvest();
            
            foreach (var type in requiredToInject)
            {
                var viewInjectionImplementor = new ViewInjectionImplementor(type.Key, type.Value, _moduleDefinition, _androidViewTypeReference,
                    _findViewByIdMethodDefinition, _resourceIdClassType);

                viewInjectionImplementor.Execute();

                var activityGetViewMethodImplementor = new ActivityGetViewMethodImplementor(type.Key, _androidViewTypeReference, _activityFindViewByIdMethodReference);

                var injectorCallReplacer = new InjectorCallReplacer(type.Key, InjectViewsGeneratedMethodName, _activityInjectViewsMethodDefinition, 
                    activityGetViewMethodImplementor);
                injectorCallReplacer.Execute();

                injectorCallReplacer = new InjectorCallReplacer(type.Key, InjectViewsGeneratedMethodName, _injectViewsMethodReference, activityGetViewMethodImplementor);
                injectorCallReplacer.Execute();
            }
            
            var subscriptionHarverster = new ViewEventHarvester(_moduleDefinition);
            var requiredToSubscribe = subscriptionHarverster.Harvest();

            foreach (var type in requiredToSubscribe)
            {
                var methodSubscriptionImplementor = new MethodSubscriptionImplementor(type.Key, type.Value.Cast<MethodDefinition>().ToArray(), _moduleDefinition,
                    _androidViewTypeReference, _injectorExceptionCtor, _findViewByIdMethodDefinition);

                methodSubscriptionImplementor.Execute();

                var activityGetViewMethodImplementor = new ActivityGetViewMethodImplementor(type.Key, _androidViewTypeReference, _activityFindViewByIdMethodReference);

                var injectorCallReplacer = new InjectorCallReplacer(type.Key, BindViewEventsGeneratedMethodName, _activityBindViewEventsMethodDefinition,
                    activityGetViewMethodImplementor);
                injectorCallReplacer.Execute();

                injectorCallReplacer = new InjectorCallReplacer(type.Key, BindViewEventsGeneratedMethodName, _bindViewEventsMethodDefinition, activityGetViewMethodImplementor);
                injectorCallReplacer.Execute();
            }
        }
        

        private void FindRequiredTypesAndMethods()
        {
            var assemblyNameReference = _moduleDefinition.AssemblyReferences.First(x => x.Name == "Mono.Android");
            var monoAndroidAssembly = _assemblyResolver.Resolve(assemblyNameReference);

            var androidViewTypeDefinition = monoAndroidAssembly.MainModule.GetType("Android.Views.View");
            _androidViewTypeReference = _moduleDefinition.ImportReference(androidViewTypeDefinition);
            _findViewByIdMethodDefinition = _moduleDefinition.ImportReference(androidViewTypeDefinition.Methods.First(x => x.Name == "FindViewById" && !x.HasGenericParameters));

            var activityTypeDefinition = monoAndroidAssembly.MainModule.GetType("Android.App.Activity");
            _activityFindViewByIdMethodReference = _moduleDefinition.ImportReference(activityTypeDefinition.Methods.First(x => x.Name == "FindViewById" && !x.HasGenericParameters));

            assemblyNameReference = _moduleDefinition.AssemblyReferences.First(x => x.Name == "Polkovnik.DroidInjector");
            var droidInjectorAssembly = _assemblyResolver.Resolve(assemblyNameReference);

            var injectorTypeDefinition = droidInjectorAssembly.MainModule.GetType("Polkovnik.DroidInjector.Injector");
            _activityInjectViewsMethodDefinition = injectorTypeDefinition.Methods.First(x => x.Name == "InjectViews" && x.Parameters.Count == 0);
            _injectViewsMethodReference = injectorTypeDefinition.Methods.First(x => x.Name == "InjectViews" && x.Parameters.Count == 1);
            _activityBindViewEventsMethodDefinition = injectorTypeDefinition.Methods.First(x => x.Name == "BindViewEvents" && x.Parameters.Count == 0);
            _bindViewEventsMethodDefinition = injectorTypeDefinition.Methods.First(x => x.Name == "BindViewEvents" && x.Parameters.Count == 1);

            var injectorExceptionType = droidInjectorAssembly.MainModule.GetType("Polkovnik.DroidInjector.InjectorException");
            _injectorExceptionCtor = _moduleDefinition.ImportReference(injectorExceptionType.Methods.First(x => x.IsConstructor));

            var resourceClassType = _moduleDefinition.GetType($"{_moduleDefinition.Assembly.Name.Name}.Resource");
            _resourceIdClassType = resourceClassType.NestedTypes.First(x => x.Name == "Id");
        }
    }
}
