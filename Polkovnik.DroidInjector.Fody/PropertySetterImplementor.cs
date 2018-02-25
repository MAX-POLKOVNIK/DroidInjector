﻿using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Polkovnik.DroidInjector.Fody
{
    internal class PropertySetterImplementor
    {
        private readonly PropertyDefinition _propertyDefinition;
        private readonly ModuleDefinition _moduleDefinition;

        public PropertySetterImplementor(PropertyDefinition propertyDefinition, ModuleDefinition moduleDefinition)
        {
            _propertyDefinition = propertyDefinition;
            _moduleDefinition = moduleDefinition;
        }

        public void Execute()
        {
            var backingFieldName = GetBackingFieldNameForProperty(_propertyDefinition);
            var backingField = _propertyDefinition.DeclaringType.Fields.FirstOrDefault(x => x.Name == backingFieldName);

            if (backingField == null)
                throw new FodyInjectorException($"Property: {_propertyDefinition.FullName} hasn't setter and not auto-implemented.");

            backingField.Attributes = backingField.Attributes ^ FieldAttributes.InitOnly;

            var setterMethod = new MethodDefinition($"set_{_propertyDefinition.Name}",
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName, _moduleDefinition.TypeSystem.Void);

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
    }
}
