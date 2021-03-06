﻿using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Polkovnik.DroidInjector.Fody.Loggers;
using TypeSystem = Fody.TypeSystem;

namespace Polkovnik.DroidInjector.Fody.Implementors
{
    internal class PropertySetterImplementor
    {
        private readonly PropertyDefinition _propertyDefinition;
        private readonly TypeSystem _typeSystem;

        public PropertySetterImplementor(PropertyDefinition propertyDefinition, TypeSystem typeSystem)
        {
            _propertyDefinition = propertyDefinition;
            _typeSystem = typeSystem;
        }

        public void Execute()
        {
            Logger.LogExecute(this);

            var backingFieldName = GetBackingFieldNameForProperty(_propertyDefinition);
            var backingField = _propertyDefinition.DeclaringType.Fields.FirstOrDefault(x => x.Name == backingFieldName).GetThisFieldReference();

            if (backingField == null)
                throw new WeavingException($"Property: {_propertyDefinition.FullName} hasn't setter and not auto-implemented.");

            var setterMethod = new MethodDefinition($"set_{_propertyDefinition.Name}",
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName, _typeSystem.VoidReference);

            setterMethod.Parameters.Add(new ParameterDefinition(_propertyDefinition.PropertyType));

            _propertyDefinition.DeclaringType.Methods.Add(setterMethod);
            _propertyDefinition.SetMethod = setterMethod;

            var ilProcessor = setterMethod.Body.GetILProcessor();

            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.Emit(OpCodes.Ldarg_1);
            ilProcessor.Emit(OpCodes.Stfld, backingField);
            ilProcessor.Emit(OpCodes.Ret);
        }

        private static string GetBackingFieldNameForProperty(PropertyDefinition propertyDefinition)
        {
            return $"<{propertyDefinition.Name}>k__BackingField";
        }

        public override string ToString()
        {
            return $"{nameof(_propertyDefinition)}: {_propertyDefinition}, {nameof(_typeSystem)}: {_typeSystem}";
        }
    }
}
