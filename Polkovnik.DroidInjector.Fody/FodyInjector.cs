using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Polkovnik.DroidInjector.Fody
{
    internal class FodyInjector
    {
        private const string ViewAttributeTypeName = "Polkovnik.DroidInjector.ViewAttribute";
        private const string InjectViewsGeneratedMethodName = "Polkovnik_DroidInjector_InjectViews";
        private const string GetViewGeneratedMethodName = "Polkovnik_DroidInjector_GetRootView";

        private readonly ModuleDefinition _moduleDefinition;
        private readonly IAssemblyResolver _assemblyResolver;

        private TypeReference _androidViewTypeReference;
        private MethodReference _findViewByIdMethodDefinition;
        private MethodDefinition _activityInjectViewsMethodDefinition;
        private MethodDefinition _injectViewsMethodReference;
        private MethodReference _activityFindViewByIdMethodReference;
        private TypeDefinition _resourceIdClassType;

        public event Action<string> DebugEvent;

        public FodyInjector(ModuleDefinition moduleDefinition, IAssemblyResolver assemblyResolver)
        {
            _assemblyResolver = assemblyResolver ?? throw new ArgumentNullException(nameof(assemblyResolver));
            _moduleDefinition = moduleDefinition ?? throw new ArgumentNullException(nameof(moduleDefinition));
        }

        private void Debug(string message)
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
                    ReplaceInjectMethodCallInType(type);
            }
        }

        private void FindRequiredTypesAndMethods()
        {
            var assemblyNameReference = _moduleDefinition.AssemblyReferences.First(x => x.Name == "Mono.Android");
            var monoAndroidAssembly = _assemblyResolver.Resolve(assemblyNameReference);

            var androidViewTypeDefinition = monoAndroidAssembly.MainModule.GetType("Android.Views.View");
            _androidViewTypeReference = _moduleDefinition.ImportReference(androidViewTypeDefinition);
            _findViewByIdMethodDefinition = _moduleDefinition.ImportReference(androidViewTypeDefinition.Methods.First(x => x.Name == "FindViewById" && !x.HasGenericParameters));

            var activityTypeDefinition = monoAndroidAssembly.MainModule.GetType("Android.App.Activity");
            _activityFindViewByIdMethodReference = _moduleDefinition.ImportReference(activityTypeDefinition.Methods.First(x => x.Name == "FindViewById" && !x.HasGenericParameters));

            assemblyNameReference = _moduleDefinition.AssemblyReferences.First(x => x.Name == "Polkovnik.DroidInjector");
            var droidInjectorAssembly = _assemblyResolver.Resolve(assemblyNameReference);

            var injectorTypeDefinition = droidInjectorAssembly.MainModule.GetType("Polkovnik.DroidInjector.Injector");
            _activityInjectViewsMethodDefinition = injectorTypeDefinition.Methods.First(x => x.Name == "InjectViews" && x.Parameters.Count == 0);
            _injectViewsMethodReference = injectorTypeDefinition.Methods.First(x => x.Name == "InjectViews" && x.Parameters.Count == 1);

            var resourceClassType = _moduleDefinition.GetType($"{_moduleDefinition.Assembly.Name.Name}.Resource");
            _resourceIdClassType = resourceClassType.NestedTypes.First(x => x.Name == "Id");
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

            if (attribute.HasConstructorArguments)
            {
                var resourceId = attribute.ConstructorArguments[0].Value;
                return (int)resourceId;
            }

            var constName = fieldDefinition.Name.Trim('_');
            var field = _resourceIdClassType.Fields.FirstOrDefault(x => x.Name == constName);

            if (field == null)
                throw new FodyInjectorException($"Can't find id for field {fieldDefinition.FullName}.");
            
            return (int)field.Constant;
        }

        private void ReplaceInjectMethodCallInType(TypeDefinition definition)
        {
            var generatedMethod = definition.Methods.First(x => x.Name == InjectViewsGeneratedMethodName);

            var isTypeDerivedFromActivity = IsTypeDerivedFromAndroidActivity(definition);

            MethodDefinition activityGetViewMethodDefinition = null;

            if (isTypeDerivedFromActivity)
            {
                Debug($"ADDING GETVIEW METHOD TO {definition}");
                activityGetViewMethodDefinition = AddGetViewMethodInActivity(definition);
            }

            foreach (var method in definition.Methods)
            {
                while (true)
                {
                    var callInjectorInstruction = method.Body.Instructions.FirstOrDefault(x => x.OpCode == OpCodes.Call && IsInjectViewsMethod(x.Operand));

                    if (callInjectorInstruction == null)
                        break;
                    
                    Debug($"REPLACE CALL {_injectViewsMethodReference} in {method.FullName}");

                    ReplaceInjectInsructions(callInjectorInstruction, method.Body.GetILProcessor(), generatedMethod);
                }

                while (true)
                {
                    var callActivityInjectorInstuction = method.Body.Instructions.FirstOrDefault(x => x.OpCode == OpCodes.Call && IsActivityInjectMethod(x.Operand));

                    if (callActivityInjectorInstuction == null)
                        break;

                    ReplaceInjectViewParameterlessMethodInstructions(callActivityInjectorInstuction, method.Body.GetILProcessor(), generatedMethod, activityGetViewMethodDefinition);
                }
            }

            bool IsActivityInjectMethod(object operand) => operand is MethodReference methodReference && methodReference.Resolve() == _activityInjectViewsMethodDefinition;
            bool IsInjectViewsMethod(object operand) => operand is MethodReference methodReference && methodReference.Resolve() == _injectViewsMethodReference;
        }

        private bool IsTypeDerivedFromAndroidActivity(TypeDefinition typeDefinition)
        {
            var baseType = typeDefinition.BaseType;

            while (baseType != null)
            {
                if (baseType.FullName == "Android.App.Activity")
                {
                    return true;
                }

                baseType = baseType.Resolve().BaseType;
            }

            return false;
        }

        private static void ReplaceInjectViewParameterlessMethodInstructions(Instruction callInjectorInstruction, ILProcessor ilProcessor, MethodReference injectionMethod, MethodReference getViewMethod)
        {
            ilProcessor.InsertBefore(callInjectorInstruction, Instruction.Create(OpCodes.Ldarg_0));
            ilProcessor.InsertBefore(callInjectorInstruction, Instruction.Create(OpCodes.Ldarg_0));
            ilProcessor.InsertBefore(callInjectorInstruction, Instruction.Create(OpCodes.Call, getViewMethod));
            ilProcessor.Replace(callInjectorInstruction, Instruction.Create(OpCodes.Call, injectionMethod));
        }

        /// <summary>
        /// Replaces call Polkovnik.DroidInjector.Injector::InjectViews(View).
        /// </summary>
        /// <param name="callInjectorInstruction">Instruction with calling Polkovnik.DroidInjector.Injector::InjectViews(View).</param>
        /// <param name="ilProcessor">Method body's ILProcessor.</param>
        /// <param name="injectionMethod">Activty's generated method for injecting.</param>
        private static void ReplaceInjectInsructions(Instruction callInjectorInstruction, ILProcessor ilProcessor, MethodReference injectionMethod)
        {
            var targetInstruction = callInjectorInstruction;

            while (targetInstruction.Previous.OpCode.Name.StartsWith("call"))
            {
                targetInstruction = targetInstruction.Previous;
            }

            while (targetInstruction.Previous.OpCode.Name.StartsWith("ld"))
            {
                targetInstruction = targetInstruction.Previous;
            }

            ilProcessor.InsertBefore(targetInstruction, Instruction.Create(OpCodes.Ldarg_0));
            
            ilProcessor.Replace(callInjectorInstruction, Instruction.Create(OpCodes.Call, injectionMethod));
        }

        private MethodDefinition AddGetViewMethodInActivity(TypeDefinition typeDefinition)
        {
            var methodDefinition = new MethodDefinition(GetViewGeneratedMethodName, MethodAttributes.Private | MethodAttributes.HideBySig, _androidViewTypeReference);

            methodDefinition.Body.Variables.Add(new VariableDefinition(_androidViewTypeReference));

            typeDefinition.Methods.Add(methodDefinition);

            var ilProcessor = methodDefinition.Body.GetILProcessor();

            ilProcessor.Emit(OpCodes.Nop);
            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.Emit(OpCodes.Ldc_I4, 0x01020002);
            ilProcessor.Emit(OpCodes.Callvirt, _activityFindViewByIdMethodReference);
            ilProcessor.Emit(OpCodes.Stloc_0);

            var ldloc0 = Instruction.Create(OpCodes.Ldloc_0);

            ilProcessor.Emit(OpCodes.Br_S, ldloc0);
            ilProcessor.Append(ldloc0);
            ilProcessor.Emit(OpCodes.Ret);

            return methodDefinition;
        }
    }
}
