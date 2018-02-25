using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Polkovnik.DroidInjector.Fody
{
    internal class FodyInjector
    {
        private const string ViewAttributeTypeName = "Polkovnik.DroidInjector.ViewAttribute";
        private const string ViewEventAttributeTypeName = "Polkovnik.DroidInjector.ViewEventAttribute";
        private const string InjectViewsGeneratedMethodName = "Polkovnik_DroidInjector_InjectViews";
        private const string BindViewEventsGeneratedMethodName = "Polkovnik_DroidInjector_BindViewEvents";
        private const string GetViewGeneratedMethodName = "Polkovnik_DroidInjector_GetRootView";

        private readonly ModuleDefinition _moduleDefinition;
        private readonly IAssemblyResolver _assemblyResolver;

        private TypeReference _androidViewTypeReference;
        private MethodReference _findViewByIdMethodDefinition;
        private MethodDefinition _activityInjectViewsMethodDefinition;
        private MethodDefinition _injectViewsMethodReference;
        private MethodReference _activityFindViewByIdMethodReference;
        private TypeDefinition _resourceIdClassType;
        private MethodReference _injectorExceptionCtor;
        private MethodDefinition _activityBindViewEventsMethodDefinition;
        private MethodDefinition _bindViewEventsMethodDefinition;

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
            FindRequiredTypesAndMethods();

            var viewInjectMembers = GetMembersMarkedWithView();

            AddInjectViewMethodInType(viewInjectMembers);
            AddBindViewEventsMethodInType(viewInjectMembers);

            foreach (var type in viewInjectMembers.Keys)
            {
                ReplaceInjectMethodCallInType(type, InjectViewsGeneratedMethodName, _activityInjectViewsMethodDefinition, true);
                ReplaceInjectMethodCallInType(type, InjectViewsGeneratedMethodName, _injectViewsMethodReference, false);
                ReplaceInjectMethodCallInType(type, BindViewEventsGeneratedMethodName, _activityBindViewEventsMethodDefinition, true);
                ReplaceInjectMethodCallInType(type, BindViewEventsGeneratedMethodName, _bindViewEventsMethodDefinition, false);
            }
        }
        
        private Dictionary<TypeDefinition, List<IMemberDefinition>> GetMembersMarkedWithView()
        {
            var membersDictionary = new Dictionary<TypeDefinition, List<IMemberDefinition>>();
            
            foreach (var type in _moduleDefinition.Types)
            {
                var members = new List<IMemberDefinition>();
                members.AddRange(type.Fields);
                members.AddRange(type.Properties);
                members.AddRange(type.Methods);

                foreach (var member in members)
                {
                    string attribute;

                    switch (member)
                    {
                        case FieldDefinition _:
                            attribute = ViewAttributeTypeName;
                            break;
                        case PropertyDefinition _:
                            attribute = ViewAttributeTypeName;
                            break;
                        case MethodDefinition _:
                            attribute = ViewEventAttributeTypeName;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (member.CustomAttributes.All(x => x.AttributeType.FullName != attribute))
                        continue;

                    if (membersDictionary.TryGetValue(type, out var list))
                    {
                        list.Add(member);
                    }
                    else
                    {
                        membersDictionary.Add(type, new List<IMemberDefinition> {member});
                    }
                }
            }
            return membersDictionary;
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
            _activityBindViewEventsMethodDefinition = injectorTypeDefinition.Methods.First(x => x.Name == "BindViewEvents" && x.Parameters.Count == 0);
            _bindViewEventsMethodDefinition = injectorTypeDefinition.Methods.First(x => x.Name == "BindViewEvents" && x.Parameters.Count == 1);

            var injectorExceptionType = droidInjectorAssembly.MainModule.GetType("Polkovnik.DroidInjector.InjectorException");
            _injectorExceptionCtor = _moduleDefinition.ImportReference(injectorExceptionType.Methods.First(x => x.IsConstructor));

            var resourceClassType = _moduleDefinition.GetType($"{_moduleDefinition.Assembly.Name.Name}.Resource");
            _resourceIdClassType = resourceClassType.NestedTypes.First(x => x.Name == "Id");
        }

        private void AddBindViewEventsMethodInType(Dictionary<TypeDefinition, List<IMemberDefinition>> typesWithMembers)
        {
            foreach (var typeWithMembers in typesWithMembers)
            {
                var subscribionsInitMethod = new MethodDefinition(BindViewEventsGeneratedMethodName, MethodAttributes.Private | MethodAttributes.HideBySig, _moduleDefinition.TypeSystem.Void);
                subscribionsInitMethod.Parameters.Add(new ParameterDefinition("view", ParameterAttributes.None, _androidViewTypeReference));

                var methods = typeWithMembers.Value.Where(x => x is MethodDefinition).ToArray();

                if (methods.Length == 0)
                    continue;

                subscribionsInitMethod.Body.Variables.Add(new VariableDefinition(_androidViewTypeReference));
                subscribionsInitMethod.Body.Variables.Add(new VariableDefinition(_moduleDefinition.TypeSystem.Boolean));

                typeWithMembers.Key.Methods.Add(subscribionsInitMethod);

                Debug($"ADD METHOD {subscribionsInitMethod} INTO {typeWithMembers.Key}");

                var ilProcessor = subscribionsInitMethod.Body.GetILProcessor();
                Instruction lastInstruction = null;

                var dict = new Dictionary<int, List<IMemberDefinition>>();

                foreach (var method in methods)
                {
                    var attribute = method.CustomAttributes.First(x => x.AttributeType.FullName == ViewEventAttributeTypeName);
                    var resourceId = (int)attribute.ConstructorArguments[0].Value;

                    if (dict.TryGetValue(resourceId, out var methodsByResourceId))
                    {
                        methodsByResourceId.Add(method);
                    }
                    else
                    {
                        dict.Add(resourceId, new List<IMemberDefinition> {method});
                    }
                }

                foreach (var methodsWithResourceId in dict)
                {
                    var resourceId = methodsWithResourceId.Key;
                    var methodsToSubscribe = methodsWithResourceId.Value;
                    var listMs = new List<M>();
                    var shouldCheckIfNull = false;

                    foreach (var methodToSubscribe in methodsToSubscribe)
                    {
                        var attributeArguments = methodToSubscribe.CustomAttributes.First(x => x.AttributeType.FullName == ViewEventAttributeTypeName).ConstructorArguments;

                        var argsLength = attributeArguments.Count;

                        shouldCheckIfNull = shouldCheckIfNull || (bool)attributeArguments[argsLength == 4 ? 3 : 2].Value;
                        var eventName = (string)attributeArguments[argsLength == 4 ? 2 : 1].Value;
                        var viewType = argsLength == 4 ? (TypeReference)attributeArguments[1].Value : _androidViewTypeReference;
                        
                        var baseType = viewType;
                        EventDefinition eventDefinition = null;

                        while (baseType != null)
                        {
                            eventDefinition = baseType.Resolve().Events.FirstOrDefault(x => x.Name == eventName);

                            if (eventDefinition == null)
                            {
                                baseType = baseType.Resolve().BaseType;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (eventDefinition == null)
                        {
                            throw new FodyInjectorException($"Can't find event {eventName} in {viewType}");
                        }
                        
                        var eventTypeDefinition = eventDefinition.EventType.Resolve();
                        
                        var addHandlerMethod = _moduleDefinition.ImportReference(eventDefinition.AddMethod);
                        
                        var handlerCtor = eventTypeDefinition.Methods.First(x => x.IsConstructor && x.Parameters.Count == 2);
                        var importedHandlerCtor = _moduleDefinition.ImportReference(handlerCtor);
                        if (eventDefinition.EventType.IsGenericInstance)
                        {
                            var instance = (GenericInstanceType)eventDefinition.EventType;
                            var genericArgs = instance.GenericArguments.Select(x => _moduleDefinition.ImportReference(x)).ToArray();

                            importedHandlerCtor.DeclaringType = importedHandlerCtor.DeclaringType.MakeGenericInstanceType(genericArgs);
                        }
                        
                        listMs.Add(new M(addHandlerMethod, importedHandlerCtor, (MethodReference)methodToSubscribe, viewType));
                    }

                    lastInstruction = AddSubscribtionInstructions(lastInstruction, ilProcessor, resourceId, listMs, shouldCheckIfNull, $"Can't find view with ID {resourceId}", 
                        _injectorExceptionCtor, _findViewByIdMethodDefinition);
                }
            }
        }

        public class M
        {
            public M(MethodReference addHandlerMethod, MethodReference handlerCtor, MethodReference targetMethod, TypeReference ownerType)
            {
                AddHandlerMethod = addHandlerMethod;
                HandlerCtor = handlerCtor;
                TargetMethod = targetMethod;
                OwnerType = ownerType;
            }

            public MethodReference AddHandlerMethod { get; }
            public MethodReference HandlerCtor { get; }
            public MethodReference TargetMethod { get; }
            public TypeReference OwnerType { get; }
        }
        
        private static Instruction AddSubscribtionInstructions(Instruction lastInstruction, ILProcessor ilProcessor,
            int resourceId, List<M> methodsToSubscribe, bool shouldThrowExceptionIfNull, string exceptionMessage, MethodReference exceptionCtor, 
            MethodReference findViewMethodReference)
        {
            Instruction li;

            if (lastInstruction == null)
            {
                lastInstruction = li = Instruction.Create(OpCodes.Ret);
                ilProcessor.Append(li);
                
            }
            else
            {
                li = lastInstruction;
            }

            InsertBefore(ref li, Instruction.Create(OpCodes.Nop));

            foreach (var m in methodsToSubscribe)
            {
                InsertBefore(ref li, Instruction.Create(OpCodes.Nop));
                InsertBefore(ref li, Instruction.Create(OpCodes.Callvirt, m.AddHandlerMethod));
                InsertBefore(ref li, Instruction.Create(OpCodes.Newobj, m.HandlerCtor));
                InsertBefore(ref li, Instruction.Create(OpCodes.Ldftn, m.TargetMethod));
                InsertBefore(ref li, Instruction.Create(OpCodes.Ldarg_0));
                InsertBefore(ref li, Instruction.Create(OpCodes.Castclass, m.OwnerType));
                InsertBefore(ref li, Instruction.Create(OpCodes.Ldloc_0));
            }

            var nopBeforeSubscriptions = Instruction.Create(OpCodes.Nop);

            InsertBefore(ref li, nopBeforeSubscriptions);

            if (shouldThrowExceptionIfNull)
            {
                InsertBefore(ref li, Instruction.Create(OpCodes.Throw));
                InsertBefore(ref li, Instruction.Create(OpCodes.Newobj, exceptionCtor));
                InsertBefore(ref li, Instruction.Create(OpCodes.Ldstr, exceptionMessage));
            }
            else
            {
                InsertBefore(ref li, Instruction.Create(OpCodes.Br_S, lastInstruction));
                InsertBefore(ref li, Instruction.Create(OpCodes.Nop));
            }

            InsertBefore(ref li, Instruction.Create(OpCodes.Nop));
            InsertBefore(ref li, Instruction.Create(OpCodes.Brfalse_S, nopBeforeSubscriptions));
            InsertBefore(ref li, Instruction.Create(OpCodes.Ldloc_1));
            InsertBefore(ref li, Instruction.Create(OpCodes.Stloc_1));
            InsertBefore(ref li, Instruction.Create(OpCodes.Ldloc_1));
            InsertBefore(ref li, Instruction.Create(OpCodes.Stloc_1));
            InsertBefore(ref li, Instruction.Create(OpCodes.Ceq));
            InsertBefore(ref li, Instruction.Create(OpCodes.Ldnull));
            InsertBefore(ref li, Instruction.Create(OpCodes.Ldloc_0));

            InsertBefore(ref li, Instruction.Create(OpCodes.Stloc_0));
            InsertBefore(ref li, Instruction.Create(OpCodes.Callvirt, findViewMethodReference));
            InsertBefore(ref li, Instruction.Create(OpCodes.Ldc_I4, resourceId));
            InsertBefore(ref li, Instruction.Create(OpCodes.Ldarg_1));
            InsertBefore(ref li, Instruction.Create(OpCodes.Nop));

            return li;

            void InsertBefore(ref Instruction firstInstruction, Instruction newInstruction)
            {
                ilProcessor.InsertBefore(firstInstruction, newInstruction);
                firstInstruction = newInstruction;
            }
        }

        private void AddInjectViewMethodInType(IDictionary<TypeDefinition, List<IMemberDefinition>> typesWithMembers)
        {
            var methodDefinition = new MethodDefinition(InjectViewsGeneratedMethodName, MethodAttributes.Private | MethodAttributes.HideBySig, _moduleDefinition.TypeSystem.Void);
            methodDefinition.Parameters.Add(new ParameterDefinition("view", ParameterAttributes.None, _androidViewTypeReference));
            foreach (var typesAndField in typesWithMembers)
            {
                typesAndField.Key.Methods.Add(methodDefinition);

                var ilProcessor = methodDefinition.Body.GetILProcessor();
                ilProcessor.Emit(OpCodes.Nop);

                foreach (var memberDefinition in typesAndField.Value)
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
                                AddSetterForProperty(propertyDefinition);
                            }
                            AddInjectViewInstructionsForProperty(ilProcessor, GetResourceId(propertyDefinition), propertyDefinition.PropertyType, propertyDefinition.SetMethod);
                            break;
                    }
                }

                ilProcessor.Emit(OpCodes.Ret);
            }
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
        
        private void AddSetterForProperty(PropertyDefinition propertyDefinition)
        {
            var backingFieldName = GetBackingFieldNameForProperty(propertyDefinition);
            var backingField = propertyDefinition.DeclaringType.Fields.FirstOrDefault(x => x.Name == backingFieldName);

            if (backingField == null)
                throw new FodyInjectorException($"Property: {propertyDefinition.FullName} hasn't setter and not auto-implemented.");
            
            backingField.Attributes = backingField.Attributes ^ FieldAttributes.InitOnly;

            var setterMethod = new MethodDefinition($"set_{propertyDefinition.Name}", 
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName, _moduleDefinition.TypeSystem.Void);

            setterMethod.Parameters.Add(new ParameterDefinition(propertyDefinition.PropertyType));
            
            propertyDefinition.DeclaringType.Methods.Add(setterMethod);
            propertyDefinition.SetMethod = setterMethod;

            var ilProcessor = setterMethod.Body.GetILProcessor();

            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.Emit(OpCodes.Ldarg_1);
            ilProcessor.Emit(OpCodes.Stfld, backingField);
            ilProcessor.Emit(OpCodes.Ret);
        }

        private string GetBackingFieldNameForProperty(PropertyDefinition propertyDefinition)
        {
            return $"<{propertyDefinition.Name}>k__BackingField";
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
            var attribute = fieldDefinition.CustomAttributes.First(x => x.AttributeType.FullName == ViewAttributeTypeName);

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

        private void ReplaceInjectMethodCallInType(TypeDefinition definition, string methodNameToCall, MethodDefinition methodToRemove, bool methodIsParameterless)
        {
            var generatedMethod = definition.Methods.First(x => x.Name == methodNameToCall);

            var isTypeDerivedFromActivity = IsTypeDerivedFromAndroidActivity(definition);

            MethodDefinition activityGetViewMethodDefinition = null;

            if (isTypeDerivedFromActivity)
            {
                activityGetViewMethodDefinition = AddGetViewMethodInActivity(definition);
            }

            foreach (var method in definition.Methods)
            {
                while (true)
                {
                    var callInstuction = method.Body.Instructions.FirstOrDefault(x => x.OpCode == OpCodes.Call && IsMehodToRemove(x.Operand));
                    if (callInstuction == null)
                        break;

                    Debug($"REPLACE CALL {methodToRemove} in {method.FullName}");

                    if (methodIsParameterless)
                    {
                        ReplaceParameterlessMethodInstructions(callInstuction, method.Body.GetILProcessor(), generatedMethod, activityGetViewMethodDefinition);
                    }
                    else
                    {
                        ReplaceMethodCallInsructions(callInstuction, method.Body.GetILProcessor(), generatedMethod);
                    }
                }
            }
            bool IsMehodToRemove(object operand) => operand is MethodReference methodReference && methodReference.Resolve() == methodToRemove;
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

        private static void ReplaceParameterlessMethodInstructions(Instruction callInjectorInstruction, ILProcessor ilProcessor, MethodReference injectionMethod, MethodReference getViewMethod)
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
        private static void ReplaceMethodCallInsructions(Instruction callInjectorInstruction, ILProcessor ilProcessor, MethodReference injectionMethod)
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
