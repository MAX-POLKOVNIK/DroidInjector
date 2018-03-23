﻿using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Polkovnik.DroidInjector.Fody.AttributesHolders;
using Polkovnik.DroidInjector.Fody.Loggers;

namespace Polkovnik.DroidInjector.Fody.Implementors
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
            Logger.LogExecute(this);

            var methodDefinition = new MethodDefinition(Consts.GeneratedMethodNames.InjectMenuItemsGeneratedMethodName, MethodAttributes.Private | MethodAttributes.HideBySig, _moduleDefinition.TypeSystem.Void);
            methodDefinition.Parameters.Add(new ParameterDefinition("menu", ParameterAttributes.None, _referencesAndDefinitionsProvider.AndroidMenuTypeReference));

            _typeDefinition.Methods.Add(methodDefinition);

            var ilProcessor = methodDefinition.Body.GetILProcessor();
            ilProcessor.Emit(OpCodes.Nop);

            foreach (var memberDefinition in _memberDefinitions)
            {
                var attribute = memberDefinition.CustomAttributes.First(x => x.AttributeType.FullName == Consts.InjectorAttributes.MenuItemAttributeTypeName);
                var attributeHolder = new MenuItemAttributeHolder(attribute);

                var resourceId = attributeHolder.ResourceId;
                if (resourceId == 0)
                {
                    resourceId = Utils.GetResourceIdByName(memberDefinition.Name, memberDefinition.FullName, _referencesAndDefinitionsProvider.ResourceIdClassType);
                }

                var getResourceIdOperation = memberDefinition.IsInAndroidClassLibrary()
                    ? Instruction.Create(OpCodes.Ldsfld, Utils.GetResourceIdField(attributeHolder.ResourceIdName ?? memberDefinition.Name, _referencesAndDefinitionsProvider.ResourceIdClassType))
                    : Instruction.Create(OpCodes.Ldc_I4, resourceId);

                switch (memberDefinition)
                {
                    case FieldReference fieldReference:
                        fieldReference = fieldReference.GetThisFieldReference();
                        AddInstructionsForField(ilProcessor, getResourceIdOperation, fieldReference, !attributeHolder.AllowMissing);
                        break;
                    case PropertyDefinition propertyDefinition:
                        var propertyHasSetter = propertyDefinition.SetMethod != null;
                        if (!propertyHasSetter)
                        {
                            var propertySetterImplementor = new PropertySetterImplementor(propertyDefinition, _moduleDefinition);
                            propertySetterImplementor.Execute();
                        }
                        var propertySetMethodReference = propertyDefinition.SetMethod;
                        AddForProperty(ilProcessor, getResourceIdOperation, propertyDefinition, propertySetMethodReference, !attributeHolder.AllowMissing);
                        break;
                }
            }

            ilProcessor.Emit(OpCodes.Ret);
        }

        private void AddForProperty(ILProcessor ilProcessor, Instruction getResourceIdInstruction, PropertyReference propertyReference,
            MethodReference setterMethodDefinition, bool shouldThrowIfNull)
        {
            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.Emit(OpCodes.Ldarg_1);
            ilProcessor.Append(getResourceIdInstruction);
            ilProcessor.Emit(OpCodes.Callvirt, _referencesAndDefinitionsProvider.FindItemMethodReference);

            var callInstruction = Instruction.Create(OpCodes.Call, setterMethodDefinition);

            if (shouldThrowIfNull)
            {
                ilProcessor.Emit(OpCodes.Dup);
                ilProcessor.Emit(OpCodes.Brtrue_S, callInstruction);
                ilProcessor.Emit(OpCodes.Pop);
                ilProcessor.Emit(OpCodes.Ldstr, $"Can't find view for {propertyReference.FullName}");
                ilProcessor.Emit(OpCodes.Newobj, _referencesAndDefinitionsProvider.InjectorExceptionCtor);
                ilProcessor.Emit(OpCodes.Throw);
            }

            ilProcessor.Append(callInstruction);
            ilProcessor.Emit(OpCodes.Nop);
        }

        private void AddInstructionsForField(ILProcessor ilProcessor, Instruction getResourceIdInstruction, FieldReference fieldReference, bool shouldThrowIfNull)
        {
            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.Emit(OpCodes.Ldarg_1);
            ilProcessor.Append(getResourceIdInstruction);
            ilProcessor.Emit(OpCodes.Callvirt, _referencesAndDefinitionsProvider.FindItemMethodReference);

            var stfldInstruction = Instruction.Create(OpCodes.Stfld, fieldReference);

            if (shouldThrowIfNull)
            {
                ilProcessor.Emit(OpCodes.Dup);
                ilProcessor.Emit(OpCodes.Brtrue_S, stfldInstruction);
                ilProcessor.Emit(OpCodes.Pop);
                ilProcessor.Emit(OpCodes.Ldstr, $"Can't find view for {fieldReference.FullName}");
                ilProcessor.Emit(OpCodes.Newobj, _referencesAndDefinitionsProvider.InjectorExceptionCtor);
                ilProcessor.Emit(OpCodes.Throw);
            }

            ilProcessor.Append(stfldInstruction);
        }

        public override string ToString()
        {
            return $"{nameof(_referencesAndDefinitionsProvider)}: {_referencesAndDefinitionsProvider}, {nameof(_moduleDefinition)}: {_moduleDefinition}, {nameof(_typeDefinition)}: {_typeDefinition}, {nameof(_memberDefinitions)}: {_memberDefinitions}";
        }
    }
}
