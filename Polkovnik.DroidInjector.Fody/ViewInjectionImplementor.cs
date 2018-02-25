using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Polkovnik.DroidInjector.Fody
{
    internal class ViewInjectionImplementor
    {
        private readonly TypeDefinition _resourceIdClassType;
        private readonly MethodReference _findViewByIdMethodDefinition;
        private readonly TypeReference _androidViewTypeReference;
        private readonly ModuleDefinition _moduleDefinition;
        private readonly IMemberDefinition[] _memberDefinitions;
        private readonly TypeDefinition _typeDefinition;
        private const string InjectViewsGeneratedMethodName = "Polkovnik_DroidInjector_InjectViews";

        public ViewInjectionImplementor(TypeDefinition typeDefinition, IMemberDefinition[] memberDefinitions, ModuleDefinition moduleDefinition, 
            TypeReference androidViewTypeReference, MethodReference findViewByIdMethodDefinition, TypeDefinition resourceIdClassType)
        {
            _resourceIdClassType = resourceIdClassType ?? throw new ArgumentNullException(nameof(resourceIdClassType));
            _findViewByIdMethodDefinition = findViewByIdMethodDefinition ?? throw new ArgumentNullException(nameof(findViewByIdMethodDefinition));
            _androidViewTypeReference = androidViewTypeReference ?? throw new ArgumentNullException(nameof(androidViewTypeReference));
            _moduleDefinition = moduleDefinition ?? throw new ArgumentNullException(nameof(moduleDefinition));
            _memberDefinitions = memberDefinitions ?? throw new ArgumentNullException(nameof(memberDefinitions));
            _typeDefinition = typeDefinition ?? throw new ArgumentNullException(nameof(typeDefinition));
        }

        public void Execute()
        {
            var methodDefinition = new MethodDefinition(InjectViewsGeneratedMethodName, MethodAttributes.Private | MethodAttributes.HideBySig, _moduleDefinition.TypeSystem.Void);
            methodDefinition.Parameters.Add(new ParameterDefinition("view", ParameterAttributes.None, _androidViewTypeReference));
            
            _typeDefinition.Methods.Add(methodDefinition);

            var ilProcessor = methodDefinition.Body.GetILProcessor();
            ilProcessor.Emit(OpCodes.Nop);

            foreach (var memberDefinition in _memberDefinitions)
            {
                switch (memberDefinition)
                {
                    case FieldDefinition fieldDefinition:
                        AddInjectViewInstructionsForField(ilProcessor, GetResourceId(fieldDefinition), fieldDefinition.FieldType, fieldDefinition);
                        break;
                    case PropertyDefinition propertyDefinition:
                        var propertyHasSetter = propertyDefinition.SetMethod != null;
                        if (!propertyHasSetter)
                        {
                            var propertySetterImplementor = new PropertySetterImplementor(propertyDefinition, _moduleDefinition);
                            propertySetterImplementor.Execute();
                        }
                        AddInjectViewInstructionsForProperty(ilProcessor, GetResourceId(propertyDefinition), propertyDefinition.PropertyType, propertyDefinition.SetMethod);
                        break;
                }
            }

            ilProcessor.Emit(OpCodes.Ret);
        }

        private void AddInjectViewInstructionsForProperty(ILProcessor ilProcessor, int resourceId, TypeReference targetPropertyType, MethodReference setterMethodDefinition)
        {
            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.Emit(OpCodes.Ldarg_1);
            ilProcessor.Emit(OpCodes.Ldc_I4, resourceId);
            ilProcessor.Emit(OpCodes.Callvirt, _findViewByIdMethodDefinition);
            ilProcessor.Emit(OpCodes.Castclass, targetPropertyType);
            ilProcessor.Emit(OpCodes.Call, setterMethodDefinition);
            ilProcessor.Emit(OpCodes.Nop);
        }

        private void AddInjectViewInstructionsForField(ILProcessor ilProcessor, int resourceId, TypeReference targetTypeReference, FieldDefinition injectingField)
        {
            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.Emit(OpCodes.Ldarg_1);
            ilProcessor.Emit(OpCodes.Ldc_I4, resourceId);
            ilProcessor.Emit(OpCodes.Callvirt, _findViewByIdMethodDefinition);
            ilProcessor.Emit(OpCodes.Castclass, targetTypeReference);
            ilProcessor.Emit(OpCodes.Stfld, injectingField);
        }

        private int GetResourceId(IMemberDefinition fieldDefinition)
        {
            var attribute = fieldDefinition.CustomAttributes.First(x => x.AttributeType.FullName == InjectorAttributes.ViewAttributeTypeName);

            if (attribute.HasConstructorArguments)
            {
                var resourceId = attribute.ConstructorArguments[0].Value;
                return (int)resourceId;
            }

            var constName = fieldDefinition.Name.Trim('_');
            var field = _resourceIdClassType.Fields.FirstOrDefault(x => x.Name == constName);

            if (field == null)
                throw new FodyInjectorException($"Can't find id for member {fieldDefinition.FullName}.");

            return (int)field.Constant;
        }
    }
}
