using System;
using Mono.Cecil;
using Polkovnik.DroidInjector.Fody.AttributesHolders;

namespace Polkovnik.DroidInjector.Fody.Implementors
{
    internal class MenuItemInjectionImplementor : InjectionImplementor
    {
        private readonly ReferencesProvider _referencesProvider;

        public MenuItemInjectionImplementor(TypeDefinition typeDefinition, IMemberDefinition[] memberDefinitions, ModuleDefinition moduleDefinition,
            ReferencesProvider referencesProvider) : base(typeDefinition, memberDefinitions, moduleDefinition, referencesProvider)
        {
            _referencesProvider = referencesProvider ?? throw new ArgumentNullException(nameof(referencesProvider));
        }

        protected override string GeneratedMethodName => Consts.GeneratedMethodNames.InjectMenuItemsGeneratedMethodName;
        protected override TypeReference GeneratedMethodParameterTypeReference => _referencesProvider.AndroidMenuTypeReference;
        protected override string AttributeTypeName => Consts.InjectorAttributes.MenuItemAttributeTypeName;
        protected override InjectAttributeHolder GetInjectHolder(CustomAttribute customAttribute) => new MenuItemAttributeHolder(customAttribute);
        protected override MethodReference FindMethodReference => _referencesProvider.FindItemMethodReference;
        protected override bool CastToTargetType => true;
    }
}
