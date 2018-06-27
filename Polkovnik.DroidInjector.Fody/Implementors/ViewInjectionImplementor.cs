using System;
using Fody;
using Mono.Cecil;
using Polkovnik.DroidInjector.Fody.AttributesHolders;

namespace Polkovnik.DroidInjector.Fody.Implementors
{
    internal class ViewInjectionImplementor : InjectionImplementor
    {
        private readonly ReferencesProvider _referencesProvider;

        public ViewInjectionImplementor(TypeDefinition typeDefinition, IMemberDefinition[] memberDefinitions, ModuleDefinition moduleDefinition, 
            ReferencesProvider referencesProvider, BaseModuleWeaver baseModuleWeaver) : base(typeDefinition, memberDefinitions, moduleDefinition, referencesProvider, baseModuleWeaver)
        {
            _referencesProvider = referencesProvider ?? throw new ArgumentNullException(nameof(referencesProvider));
        }
        
        protected override string GeneratedMethodName => Consts.GeneratedMethodNames.InjectViewsGeneratedMethodName;
        protected override TypeReference GeneratedMethodParameterTypeReference => _referencesProvider.AndroidViewTypeReference;
        protected override string AttributeTypeName => Consts.InjectorAttributes.ViewAttributeTypeName;
        protected override InjectAttributeHolder GetInjectHolder(CustomAttribute customAttribute) => new ViewAttributeHolder(customAttribute);
        protected override MethodReference FindMethodReference => _referencesProvider.FindViewByIdMethodReference;
        protected override bool CastToTargetType => true;
    }
}
