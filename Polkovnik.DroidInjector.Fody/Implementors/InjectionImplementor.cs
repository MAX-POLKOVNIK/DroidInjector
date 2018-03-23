﻿using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Polkovnik.DroidInjector.Fody.AttributesHolders;
using Polkovnik.DroidInjector.Fody.Loggers;

namespace Polkovnik.DroidInjector.Fody.Implementors
{
    internal abstract class InjectionImplementor
    {
        private readonly ReferencesProvider _referencesProvider;
        private readonly ModuleDefinition _moduleDefinition;
        private readonly IMemberDefinition[] _memberDefinitions;
        private readonly TypeDefinition _typeDefinition;

        protected InjectionImplementor(TypeDefinition typeDefinition, IMemberDefinition[] memberDefinitions,
            ModuleDefinition moduleDefinition, ReferencesProvider referencesProvider)
        {
            _referencesProvider = referencesProvider ?? throw new ArgumentNullException(nameof(referencesProvider));
            _moduleDefinition = moduleDefinition ?? throw new ArgumentNullException(nameof(moduleDefinition));
            _memberDefinitions = memberDefinitions ?? throw new ArgumentNullException(nameof(memberDefinitions));
            _typeDefinition = typeDefinition ?? throw new ArgumentNullException(nameof(typeDefinition));
        }

        protected abstract string GeneratedMethodName { get; }
        protected abstract TypeReference GeneratedMethodParameterTypeReference { get; }
        protected abstract string AttributeTypeName { get; }
        protected abstract InjectAttributeHolder GetInjectHolder(CustomAttribute customAttribute);
        protected abstract MethodReference FindMethodReference { get; }
        protected abstract bool CastToTargetType { get; }

        public void Execute()
        {
            Logger.LogExecute(this);

            var methodDefinition = new MethodDefinition(GeneratedMethodName,MethodAttributes.Private | MethodAttributes.HideBySig, _moduleDefinition.TypeSystem.Void);
            methodDefinition.Parameters.Add(new ParameterDefinition("p0", ParameterAttributes.None, GeneratedMethodParameterTypeReference));

            _typeDefinition.Methods.Add(methodDefinition);

            var ilProcessor = methodDefinition.Body.GetILProcessor();
            ilProcessor.Emit(OpCodes.Nop);

            foreach (var memberDefinition in _memberDefinitions)
            {
                var attribute = memberDefinition.CustomAttributes.First(x => x.AttributeType.FullName == AttributeTypeName);
                var attributeHolder = GetInjectHolder(attribute);

                var resourceId = attributeHolder.ResourceId;
                if (resourceId == 0)
                {
                    resourceId = Utils.GetResourceIdByName(memberDefinition.Name, memberDefinition.FullName, _referencesProvider.ResourceIdClassType);
                }

                var getResourceIdOperation = memberDefinition.IsInAndroidClassLibrary()
                    ? Instruction.Create(OpCodes.Ldsfld, Utils.GetResourceIdField(attributeHolder.ResourceIdName ?? memberDefinition.Name, _referencesProvider.ResourceIdClassType))
                    : Instruction.Create(OpCodes.Ldc_I4, resourceId);

                switch (memberDefinition)
                {
                    case FieldReference fieldReference:
                        fieldReference = fieldReference.GetThisFieldReference();
                        AddInstructionsForField(ilProcessor, getResourceIdOperation, fieldReference.FieldType, fieldReference, !attributeHolder.AllowMissing);
                        break;
                    case PropertyDefinition propertyDefinition:
                        var propertyHasSetter = propertyDefinition.SetMethod != null;
                        if (!propertyHasSetter)
                        {
                            var propertySetterImplementor = new PropertySetterImplementor(propertyDefinition, _moduleDefinition);
                            propertySetterImplementor.Execute();
                        }
                        var propertySetMethodReference = propertyDefinition.SetMethod;
                        AddInstructionsForProperty(ilProcessor, getResourceIdOperation, propertyDefinition, propertySetMethodReference, !attributeHolder.AllowMissing);
                        break;
                }
            }

            ilProcessor.Emit(OpCodes.Ret);
        }

        private void AddInstructionsForProperty(ILProcessor ilProcessor, Instruction getResourceIdInstruction, PropertyReference propertyReference,
                MethodReference setterMethodDefinition, bool shouldThrowIfNull)
        {
            AddInstructions(ilProcessor, getResourceIdInstruction, propertyReference.PropertyType, Instruction.Create(OpCodes.Call, setterMethodDefinition),
                shouldThrowIfNull, $"Can't find view for {propertyReference.FullName}", true);
        }

        private void AddInstructionsForField(ILProcessor ilProcessor, Instruction getResourceIdInstruction, TypeReference targetTypeReference, 
            FieldReference fieldReference, bool shouldThrowIfNull)
        {
            AddInstructions(ilProcessor, getResourceIdInstruction, targetTypeReference, Instruction.Create(OpCodes.Stfld, fieldReference),
                shouldThrowIfNull, $"Can't find view for {fieldReference.FullName}", false);
        }

        private void AddInstructions(ILProcessor ilProcessor, Instruction getResourceIdInstruction, TypeReference castTypeReference, Instruction storeObjectInstruction,
            bool shouldThrowIfNull, string exceptionMessage, bool addNopToEnd)
        {
            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.Emit(OpCodes.Ldarg_1);
            ilProcessor.Append(getResourceIdInstruction);
            ilProcessor.Emit(OpCodes.Callvirt, FindMethodReference);

            if (CastToTargetType)
            {
                ilProcessor.Emit(OpCodes.Castclass, castTypeReference);
            }

            if (shouldThrowIfNull)
            {
                ilProcessor.Emit(OpCodes.Dup);
                ilProcessor.Emit(OpCodes.Brtrue_S, storeObjectInstruction);
                ilProcessor.Emit(OpCodes.Pop);
                ilProcessor.Emit(OpCodes.Ldstr, exceptionMessage);
                ilProcessor.Emit(OpCodes.Newobj, _referencesProvider.InjectorExceptionCtor);
                ilProcessor.Emit(OpCodes.Throw);
            }

            ilProcessor.Append(storeObjectInstruction);

            if (addNopToEnd)
            {
                ilProcessor.Emit(OpCodes.Nop);
            }
        }

        public override string ToString()
        {
            return $"{nameof(_referencesProvider)}: {_referencesProvider}, {nameof(_moduleDefinition)}: {_moduleDefinition}, {nameof(_memberDefinitions)}: {_memberDefinitions}, {nameof(_typeDefinition)}: {_typeDefinition}";
        }
    }
}
