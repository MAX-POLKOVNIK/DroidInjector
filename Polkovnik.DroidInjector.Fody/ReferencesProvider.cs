using System;
using System.Linq;
using Fody;
using Mono.Cecil;

namespace Polkovnik.DroidInjector.Fody
{
    internal class ReferencesProvider
    {
        private readonly ModuleDefinition _moduleDefinition;
        private readonly BaseModuleWeaver _baseModuleWeaver;

        public ReferencesProvider(ModuleDefinition moduleDefinition, BaseModuleWeaver baseModuleWeaver)
        {
            _moduleDefinition = moduleDefinition ?? throw new ArgumentNullException(nameof(moduleDefinition));
            _baseModuleWeaver = baseModuleWeaver ?? throw new ArgumentNullException(nameof(baseModuleWeaver));

            FindRequiredTypesAndMethods();
        }

        private void FindRequiredTypesAndMethods()
        {
            if (_moduleDefinition.AssemblyReferences.FirstOrDefault(x => x.Name == "Mono.Android") == null)
            {
                throw new WeavingException("Unable to find Mono.Android");
            }

            var monoAndroidAssembly = _baseModuleWeaver.ResolveAssembly("Mono.Android");
            
            var androidViewTypeDefinition = monoAndroidAssembly.MainModule.GetType("Android.Views.View");
            AndroidViewTypeReference = _moduleDefinition.ImportReference(androidViewTypeDefinition);
            FindViewByIdMethodReference = _moduleDefinition.ImportReference(androidViewTypeDefinition.Methods.First(x => x.Name == "FindViewById" && !x.HasGenericParameters));

            var activityTypeDefinition = monoAndroidAssembly.MainModule.GetType("Android.App.Activity");
            ActivityFindViewByIdMethodReference = _moduleDefinition.ImportReference(activityTypeDefinition.Methods.First(x => x.Name == "FindViewById" && !x.HasGenericParameters));

            var menuTypeDefinition = monoAndroidAssembly.MainModule.GetType("Android.Views.IMenu");
            AndroidMenuTypeReference = _moduleDefinition.ImportReference(menuTypeDefinition);
            FindItemMethodReference = _moduleDefinition.ImportReference(menuTypeDefinition.Methods.First(x => x.Name == "FindItem"));
            
            if (_moduleDefinition.AssemblyReferences.FirstOrDefault(x => x.Name == "Polkovnik.DroidInjector") == null)
            {
                throw new WeavingException("Unable to find Polkovnik.DroidInjector");
            }

            var droidInjectorAssembly = _baseModuleWeaver.ResolveAssembly("Polkovnik.DroidInjector");

            var injectorTypeDefinition = droidInjectorAssembly.MainModule.GetType("Polkovnik.DroidInjector.Injector");
            ActivityInjectViewsMethodDefinition = injectorTypeDefinition.Methods.First(x => x.Name == "InjectViews" && x.Parameters.Count == 0);
            InjectViewsMethodReference = injectorTypeDefinition.Methods.First(x => x.Name == "InjectViews" && x.Parameters.Count == 1);
            InjectMenuItemsMethodDefinition = injectorTypeDefinition.Methods.First(x => x.Name == "InjectMenuItems");

            var injectorExceptionType = droidInjectorAssembly.MainModule.GetType("Polkovnik.DroidInjector.InjectorException");
            InjectorExceptionCtor = _moduleDefinition.ImportReference(injectorExceptionType.Methods.First(x => x.IsConstructor));

            var resourceClassType = _moduleDefinition.GetType($"{_moduleDefinition.Assembly.Name.Name}.Resource");
            ResourceIdClassType = resourceClassType.NestedTypes.First(x => x.Name == "Id");
        }

        public MethodDefinition InjectMenuItemsMethodDefinition { get; private set; }
        public TypeReference AndroidViewTypeReference { get; private set; }
        public MethodReference FindViewByIdMethodReference { get; private set; }
        public MethodDefinition ActivityInjectViewsMethodDefinition { get; private set; }
        public MethodDefinition InjectViewsMethodReference { get; private set; }
        public MethodReference ActivityFindViewByIdMethodReference { get; private set; }
        public TypeDefinition ResourceIdClassType { get; private set; }
        public MethodReference InjectorExceptionCtor { get; private set; }
        public TypeReference AndroidMenuTypeReference { get; private set; }
        public MethodReference FindItemMethodReference { get; private set; }
    }
}
