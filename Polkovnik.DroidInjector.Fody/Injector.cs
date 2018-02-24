using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Polkovnik.DroidInjector.Fody
{
    internal class Injector
    {
        private readonly IAssemblyResolver _assemblyResolver;

        private const string ViewAttributeTypeName = "Polkovnik.DroidInjector.ViewAttribute";
        private const string InjectViewsGeneratedMethodName = "Polkovnik_DroidInjector_InjectViews";
        private readonly ModuleDefinition _moduleDefinition;

        private TypeReference _androidViewTypeReference;
        private MethodReference _findViewByIdMethodDefinition;

        public event Action<string> DebugEvent;

        public Injector(ModuleDefinition moduleDefinition, IAssemblyResolver assemblyResolver)
        {
            _assemblyResolver = assemblyResolver ?? throw new ArgumentNullException(nameof(assemblyResolver));
            _moduleDefinition = moduleDefinition ?? throw new ArgumentNullException(nameof(moduleDefinition));
        }

        

        public void Debug(string message)
        {
            DebugEvent?.Invoke(message);
        }

        public void Execute()
        {
            Debug("Injector started");

            FindRequiredTypesAndMethods();

            var types = _moduleDefinition.Types.Where(t => t.Fields.SelectMany(y => y.CustomAttributes).Any(x => x.AttributeType.FullName == ViewAttributeTypeName));

            var typesAndFields = types.ToDictionary(k => k, v => v.Fields.Where(x => x.CustomAttributes.Any(y => y.AttributeType.FullName == ViewAttributeTypeName)).ToArray());
            
            foreach (var typesAndField in typesAndFields)
            {
                Debug($"IN :: {typesAndField.Key} :: {typesAndField.Value.Length}");
            }
            
            AddInjectViewMethodInTypeForFields(typesAndFields);

            foreach (var type in typesAndFields.Keys)
            {
                ReplaceInjectMethodCall(type);
            }
        }

        private void FindRequiredTypesAndMethods()
        {
            var assemblyNameReference = _moduleDefinition.AssemblyReferences.First(x => x.Name == "Mono.Android");
            var monoAndroidAssembly = _assemblyResolver.Resolve(assemblyNameReference);

            var androidViewTypeDefinition = monoAndroidAssembly.MainModule.GetType("Android.Views.View");
            _androidViewTypeReference = _moduleDefinition.ImportReference(androidViewTypeDefinition);
            _findViewByIdMethodDefinition = _moduleDefinition.ImportReference(androidViewTypeDefinition.Methods.First(x => x.Name == "FindViewById" && !x.HasGenericParameters));
        }

        private void AddInjectViewMethodInTypeForFields(IDictionary<TypeDefinition, FieldDefinition[]> typesWithFields)
        {
            var methodDefinition = new MethodDefinition(InjectViewsGeneratedMethodName, MethodAttributes.Private | MethodAttributes.HideBySig, _moduleDefinition.TypeSystem.Void);
            methodDefinition.Parameters.Add(new ParameterDefinition("view", ParameterAttributes.None, _androidViewTypeReference));
            foreach (var typesAndField in typesWithFields)
            {
                typesAndField.Key.Methods.Add(methodDefinition);

                var ilProcessor = methodDefinition.Body.GetILProcessor();
                ilProcessor.Emit(OpCodes.Nop);

                foreach (var fieldDefinition in typesAndField.Value)
                {
                    AddInjectViewInstrictions(ilProcessor, GetResourceId(fieldDefinition), fieldDefinition.FieldType, fieldDefinition);
                }

                ilProcessor.Emit(OpCodes.Ret);
            }
        }

        private void AddInjectViewInstrictions(ILProcessor ilProcessor, int resourceId, TypeReference targetTypeReference, FieldDefinition injectingField)
        {
            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.Emit(OpCodes.Ldarg_1);
            ilProcessor.Emit(OpCodes.Ldc_I4, resourceId);
            ilProcessor.Emit(OpCodes.Callvirt, _findViewByIdMethodDefinition);
            ilProcessor.Emit(OpCodes.Castclass, targetTypeReference);
            ilProcessor.Emit(OpCodes.Stfld, injectingField);
        }

        private int GetResourceId(FieldDefinition fieldDefinition)
        {
            var attribute = fieldDefinition.CustomAttributes.First(x => x.AttributeType.FullName == ViewAttributeTypeName);
            var resourceId = attribute.ConstructorArguments[0].Value;
            return (int)resourceId;
        }

        private void ReplaceInjectMethodCall(TypeDefinition definition)
        {
            var value = "Polkovnik.DroidInjector.Injector::InjectViews";

            foreach (var method in definition.Methods)
            {
                var callInjectorInstruction = method.Body.Instructions.FirstOrDefault(x => x.OpCode == OpCodes.Call && x.Operand.ToString().Contains(value));
                
                if (callInjectorInstruction == null)
                    continue;

                Debug($"FOUND CALL {callInjectorInstruction.Operand.ToString()} in {method.FullName}");

                var ilProcessor = method.Body.GetILProcessor();

                ilProcessor.InsertBefore(callInjectorInstruction.Previous, Instruction.Create(OpCodes.Ldarg_0));

                var generatedMethod = method.DeclaringType.Methods.First(x => x.Name == InjectViewsGeneratedMethodName);

                ilProcessor.Replace(callInjectorInstruction, Instruction.Create(OpCodes.Call, generatedMethod));
            }
        }
    }
}
