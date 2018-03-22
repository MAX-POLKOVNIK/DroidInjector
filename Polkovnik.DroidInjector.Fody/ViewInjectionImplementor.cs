using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Polkovnik.DroidInjector.Fody
{
    internal class ViewInjectionImplementor
    {
        private readonly ReferencesAndDefinitionsProvider _referencesAndDefinitionsProvider;
        private readonly ModuleDefinition _moduleDefinition;
        private readonly IMemberDefinition[] _memberDefinitions;
        private readonly TypeDefinition _typeDefinition;

        public ViewInjectionImplementor(TypeDefinition typeDefinition, IMemberDefinition[] memberDefinitions, ModuleDefinition moduleDefinition, 
            ReferencesAndDefinitionsProvider referencesAndDefinitionsProvider)
        {
            _referencesAndDefinitionsProvider = referencesAndDefinitionsProvider ?? throw new ArgumentNullException(nameof(referencesAndDefinitionsProvider));
            _moduleDefinition = moduleDefinition ?? throw new ArgumentNullException(nameof(moduleDefinition));
            _memberDefinitions = memberDefinitions ?? throw new ArgumentNullException(nameof(memberDefinitions));
            _typeDefinition = typeDefinition ?? throw new ArgumentNullException(nameof(typeDefinition));
        }

        public void Execute()
        {
            var methodDefinition = new MethodDefinition(Consts.GeneratedMethodNames.InjectViewsGeneratedMethodName, MethodAttributes.Private | MethodAttributes.HideBySig, _moduleDefinition.TypeSystem.Void);
            methodDefinition.Parameters.Add(new ParameterDefinition("view", ParameterAttributes.None, _referencesAndDefinitionsProvider.AndroidViewTypeReference));
            
            _typeDefinition.Methods.Add(methodDefinition);

            var ilProcessor = methodDefinition.Body.GetILProcessor();
            ilProcessor.Emit(OpCodes.Nop);

            foreach (var memberDefinition in _memberDefinitions)
            {
                var attribute = memberDefinition.CustomAttributes.First(x => x.AttributeType.FullName == Consts.InjectorAttributes.ViewAttributeTypeName);
                var resourceId = (int)attribute.ConstructorArguments[0].Value;
                if (resourceId == 0)
                {
                    resourceId = GetResourceIdByName(memberDefinition);
                }
                var shouldThrowIfNull = !(bool)attribute.ConstructorArguments[1].Value;

                switch (memberDefinition)
                {
                    case FieldDefinition fieldDefinition:
                        AddInjectViewInstructionsForField(ilProcessor, resourceId, fieldDefinition.FieldType, fieldDefinition, shouldThrowIfNull);
                        break;
                    case PropertyDefinition propertyDefinition:
                        var propertyHasSetter = propertyDefinition.SetMethod != null;
                        if (!propertyHasSetter)
                        {
                            var propertySetterImplementor = new PropertySetterImplementor(propertyDefinition, _moduleDefinition);
                            propertySetterImplementor.Execute();
                        }
                        AddInjectViewInstructionsForProperty(ilProcessor, resourceId, propertyDefinition.PropertyType, propertyDefinition.SetMethod, shouldThrowIfNull);
                        break;
                }
            }

            ilProcessor.Emit(OpCodes.Ret);
        }

        private void AddInjectViewInstructionsForProperty(ILProcessor ilProcessor, int resourceId, TypeReference targetPropertyType, 
            MethodReference setterMethodDefinition, bool shouldThrowIfNull)
        {
            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.Emit(OpCodes.Ldarg_1);
            ilProcessor.Emit(OpCodes.Ldc_I4, resourceId);
            ilProcessor.Emit(OpCodes.Callvirt, _referencesAndDefinitionsProvider.FindViewByIdMethodReference);
            ilProcessor.Emit(OpCodes.Castclass, targetPropertyType);

            var callInstruction = Instruction.Create(OpCodes.Call, setterMethodDefinition);

            if (shouldThrowIfNull)
            {
                ilProcessor.Emit(OpCodes.Dup);
                ilProcessor.Emit(OpCodes.Brtrue_S, callInstruction);
                ilProcessor.Emit(OpCodes.Pop);
                ilProcessor.Emit(OpCodes.Ldstr, $"Can't find view with ID {resourceId}");
                ilProcessor.Emit(OpCodes.Newobj, _referencesAndDefinitionsProvider.InjectorExceptionCtor);
                ilProcessor.Emit(OpCodes.Throw);
            }

            ilProcessor.Append(callInstruction);
            ilProcessor.Emit(OpCodes.Nop);
        }

        private void AddInjectViewInstructionsForField(ILProcessor ilProcessor, int resourceId, TypeReference targetTypeReference, FieldDefinition fieldDefinition, 
            bool shouldThrowIfNull)
        {
            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.Emit(OpCodes.Ldarg_1);
            ilProcessor.Emit(OpCodes.Ldc_I4, resourceId);
            ilProcessor.Emit(OpCodes.Callvirt, _referencesAndDefinitionsProvider.FindViewByIdMethodReference);
            ilProcessor.Emit(OpCodes.Castclass, targetTypeReference);

            var stfldInstruction = Instruction.Create(OpCodes.Stfld, fieldDefinition);

            if (shouldThrowIfNull)
            {
                ilProcessor.Emit(OpCodes.Dup);
                ilProcessor.Emit(OpCodes.Brtrue_S, stfldInstruction);
                ilProcessor.Emit(OpCodes.Pop);
                ilProcessor.Emit(OpCodes.Ldstr, $"Can't find view with ID {resourceId}");
                ilProcessor.Emit(OpCodes.Newobj, _referencesAndDefinitionsProvider.InjectorExceptionCtor);
                ilProcessor.Emit(OpCodes.Throw);
            }

            ilProcessor.Append(stfldInstruction);
        }

        private int GetResourceIdByName(IMemberDefinition fieldDefinition)
        {
            var constName = fieldDefinition.Name.Trim('_');
            var field = _referencesAndDefinitionsProvider.ResourceIdClassType.Fields.FirstOrDefault(x => x.Name == constName);

            if (field == null)
                throw new WeavingException($"Can't find id for member {fieldDefinition.FullName}.");

            return (int)field.Constant;
        }
    }
}
