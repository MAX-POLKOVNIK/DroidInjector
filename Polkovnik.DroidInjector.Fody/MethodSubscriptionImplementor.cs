﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Polkovnik.DroidInjector.Fody
{
    public class MethodSubscriptionImplementor
    {
        private readonly MethodReference _injectorExceptionCtor;
        private readonly MethodReference _findViewByIdMethodReference;
        private readonly TypeReference _androidViewTypeReference;
        private readonly ModuleDefinition _moduleDefinition;
        private readonly MethodDefinition[] _methodsToSubscribe;
        private readonly TypeDefinition _typeDefinition;

        private const string BindViewEventsGeneratedMethodName = "Polkovnik_DroidInjector_BindViewEvents";

        public MethodSubscriptionImplementor(TypeDefinition typeDefinition, MethodDefinition[] methodsToSubscribe, ModuleDefinition moduleDefinition,
            TypeReference androidViewTypeReference, MethodReference injectorExceptionCtor, MethodReference findViewByIdMethodDefinition)
        {
            _injectorExceptionCtor = injectorExceptionCtor ?? throw new ArgumentNullException(nameof(injectorExceptionCtor));
            _findViewByIdMethodReference = findViewByIdMethodDefinition ?? throw new ArgumentNullException(nameof(findViewByIdMethodDefinition));
            _androidViewTypeReference = androidViewTypeReference ?? throw new ArgumentNullException(nameof(androidViewTypeReference));
            _moduleDefinition = moduleDefinition ?? throw new ArgumentNullException(nameof(moduleDefinition));
            _methodsToSubscribe = methodsToSubscribe ?? throw new ArgumentNullException(nameof(methodsToSubscribe));
            _typeDefinition = typeDefinition ?? throw new ArgumentNullException(nameof(typeDefinition));
        }

        public void Execute()
        {
            if (_methodsToSubscribe.Length == 0)
            {
                Logger.Debug($"Nothing to subscribe in {_typeDefinition}");
                return;
            }

            var subscriptionsInitMethod = new MethodDefinition(BindViewEventsGeneratedMethodName, MethodAttributes.Private | MethodAttributes.HideBySig, _moduleDefinition.TypeSystem.Void);
            subscriptionsInitMethod.Parameters.Add(new ParameterDefinition("view", ParameterAttributes.None, _androidViewTypeReference));
            
            subscriptionsInitMethod.Body.Variables.Add(new VariableDefinition(_androidViewTypeReference));
            subscriptionsInitMethod.Body.Variables.Add(new VariableDefinition(_moduleDefinition.TypeSystem.Boolean));

            Logger.Debug($"Add subscription init method {subscriptionsInitMethod} into {_typeDefinition}");

            _typeDefinition.Methods.Add(subscriptionsInitMethod);
            
            var ilProcessor = subscriptionsInitMethod.Body.GetILProcessor();
            Instruction lastInstruction = null;

            var dict = new Dictionary<int, List<MethodDefinition>>();

            foreach (var method in _methodsToSubscribe)
            {
                var attribute = method.CustomAttributes.First(x => x.AttributeType.FullName == InjectorAttributes.ViewEventAttributeTypeName);
                var resourceId = (int)attribute.ConstructorArguments[0].Value;

                if (dict.TryGetValue(resourceId, out var methodsByResourceId))
                {
                    methodsByResourceId.Add(method);
                }
                else
                {
                    dict.Add(resourceId, new List<MethodDefinition> { method });
                }
            }

            foreach (var methodsWithResourceId in dict)
            {
                var resourceId = methodsWithResourceId.Key;
                var methodsToSubscribe = methodsWithResourceId.Value;
                var listMs = new List<MethodSubscriptionInfo>();
                var shouldCheckIfNull = false;

                foreach (var methodToSubscribe in methodsToSubscribe)
                {
                    var attributeArguments = methodToSubscribe.CustomAttributes
                        .First(x => x.AttributeType.FullName == InjectorAttributes.ViewEventAttributeTypeName).ConstructorArguments;

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

                    listMs.Add(new MethodSubscriptionInfo(addHandlerMethod, importedHandlerCtor, methodToSubscribe, viewType));
                }

                lastInstruction = AddSubscribtionInstructions(lastInstruction, ilProcessor, resourceId, listMs, shouldCheckIfNull);
            }
        }

        private Instruction AddSubscribtionInstructions(Instruction lastInstruction, ILProcessor ilProcessor,
            int resourceId, List<MethodSubscriptionInfo> methodsToSubscribe, bool shouldThrowExceptionIfNull)
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
                InsertBefore(ref li, Instruction.Create(OpCodes.Newobj, _injectorExceptionCtor));
                InsertBefore(ref li, Instruction.Create(OpCodes.Ldstr, $"Can't find view with ID {resourceId}"));
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
            InsertBefore(ref li, Instruction.Create(OpCodes.Callvirt, _findViewByIdMethodReference));
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
    }
}
