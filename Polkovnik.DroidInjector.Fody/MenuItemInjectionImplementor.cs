﻿using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Polkovnik.DroidInjector.Fody
{
    internal class MenuItemInjectionImplementor
    {
        private readonly ReferencesAndDefinitionsProvider _referencesAndDefinitionsProvider;
        private readonly ModuleDefinition _moduleDefinition;
        private readonly TypeDefinition _typeDefinition;
        private readonly IMemberDefinition[] _memberDefinitions;

        public MenuItemInjectionImplementor(TypeDefinition typeDefinition, IMemberDefinition[] memberDefinitions, ModuleDefinition moduleDefinition,
            ReferencesAndDefinitionsProvider referencesAndDefinitionsProvider)
        {
            _referencesAndDefinitionsProvider = referencesAndDefinitionsProvider ?? throw new ArgumentNullException(nameof(referencesAndDefinitionsProvider));
            _moduleDefinition = moduleDefinition ?? throw new ArgumentNullException(nameof(moduleDefinition));
            _memberDefinitions = memberDefinitions ?? throw new ArgumentNullException(nameof(memberDefinitions));
            _typeDefinition = typeDefinition ?? throw new ArgumentNullException(nameof(typeDefinition));
        }

        public void Execute()
        {
            var methodDefinition = new MethodDefinition(Consts.GeneratedMethodNames.InjectMenuItemsGeneratedMethodName, MethodAttributes.Private | MethodAttributes.HideBySig, _moduleDefinition.TypeSystem.Void);
            methodDefinition.Parameters.Add(new ParameterDefinition("menu", ParameterAttributes.None, _referencesAndDefinitionsProvider.AndroidMenuTypeReference));

            _typeDefinition.Methods.Add(methodDefinition);

            var ilProcessor = methodDefinition.Body.GetILProcessor();
            ilProcessor.Emit(OpCodes.Nop);

            foreach (var memberDefinition in _memberDefinitions)
            {
                var attribute = memberDefinition.CustomAttributes.First(x => x.AttributeType.FullName == Consts.InjectorAttributes.MenuItemAttributeTypeName);
                var resourceId = (int)attribute.ConstructorArguments[0].Value;
                var shouldThrowIfNull = !(bool) attribute.ConstructorArguments[1].Value;

                switch (memberDefinition)
                {
                    case FieldDefinition fieldDefinition:
                        AddInstructionsForField(ilProcessor, resourceId, fieldDefinition, shouldThrowIfNull);
                        break;
                    case PropertyDefinition propertyDefinition:
                        var propertyHasSetter = propertyDefinition.SetMethod != null;
                        if (!propertyHasSetter)
                        {
                            var propertySetterImplementor = new PropertySetterImplementor(propertyDefinition, _moduleDefinition);
                            propertySetterImplementor.Execute();
                        }
                        AddForProperty(ilProcessor, resourceId, propertyDefinition.SetMethod, shouldThrowIfNull);
                        break;
                }
            }

            ilProcessor.Emit(OpCodes.Ret);
        }

        private void AddForProperty(ILProcessor ilProcessor, int resourceId, MethodReference propertyDefinitionSetMethod, bool shouldThrowIfNull)
        {
            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.Emit(OpCodes.Ldarg_1);
            ilProcessor.Emit(OpCodes.Ldc_I4, resourceId);
            ilProcessor.Emit(OpCodes.Callvirt, _referencesAndDefinitionsProvider.FindItemMethodReference);

            var callInstruction = Instruction.Create(OpCodes.Call, propertyDefinitionSetMethod);

            if (shouldThrowIfNull)
            {
                ilProcessor.Emit(OpCodes.Dup);
                ilProcessor.Emit(OpCodes.Brtrue_S, callInstruction);
                ilProcessor.Emit(OpCodes.Pop);
                ilProcessor.Emit(OpCodes.Ldstr, $"Can't find menu item with ID {resourceId}");
                ilProcessor.Emit(OpCodes.Newobj, _referencesAndDefinitionsProvider.InjectorExceptionCtor);
                ilProcessor.Emit(OpCodes.Throw);
            }

            ilProcessor.Append(callInstruction);
            ilProcessor.Emit(OpCodes.Nop);
        }

        private void AddInstructionsForField(ILProcessor ilProcessor, int resourceId, FieldReference fieldDefinition, bool shouldThrowIfNull)
        {
            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.Emit(OpCodes.Ldarg_1);
            ilProcessor.Emit(OpCodes.Ldc_I4, resourceId);
            ilProcessor.Emit(OpCodes.Callvirt, _referencesAndDefinitionsProvider.FindItemMethodReference);

            var stfldInstruction = Instruction.Create(OpCodes.Stfld, fieldDefinition);

            if (shouldThrowIfNull)
            {
                ilProcessor.Emit(OpCodes.Dup);
                ilProcessor.Emit(OpCodes.Brtrue_S, stfldInstruction);
                ilProcessor.Emit(OpCodes.Pop);
                ilProcessor.Emit(OpCodes.Ldstr, $"Can't find menu item with ID {resourceId}");
                ilProcessor.Emit(OpCodes.Newobj, _referencesAndDefinitionsProvider.InjectorExceptionCtor);
                ilProcessor.Emit(OpCodes.Throw);
            }

            ilProcessor.Append(stfldInstruction);
        }
    }
}
