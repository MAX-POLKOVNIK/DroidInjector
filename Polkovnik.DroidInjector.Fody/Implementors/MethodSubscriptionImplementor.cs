using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Polkovnik.DroidInjector.Fody.Loggers;

namespace Polkovnik.DroidInjector.Fody.Implementors
{
    internal class MethodSubscriptionImplementor
    {
        private readonly ReferencesProvider _referencesProvider;
        private readonly ModuleDefinition _moduleDefinition;
        private readonly MethodDefinition[] _methodsToSubscribe;
        private readonly TypeDefinition _typeDefinition;
        
        public MethodSubscriptionImplementor(TypeDefinition typeDefinition, MethodDefinition[] methodsToSubscribe, ModuleDefinition moduleDefinition,
            ReferencesProvider referencesProvider)
        {
            _referencesProvider = referencesProvider ?? throw new ArgumentNullException(nameof(referencesProvider));
            _moduleDefinition = moduleDefinition ?? throw new ArgumentNullException(nameof(moduleDefinition));
            _methodsToSubscribe = methodsToSubscribe ?? throw new ArgumentNullException(nameof(methodsToSubscribe));
            _typeDefinition = typeDefinition ?? throw new ArgumentNullException(nameof(typeDefinition));
        }

        public void Execute()
        {
            Logger.LogExecute(this);

            if (_methodsToSubscribe.Length == 0)
            {
                Logger.Debug($"Nothing to subscribe in {_typeDefinition}");
                return;
            }

            var subscriptionsInitMethod = new MethodDefinition(Consts.GeneratedMethodNames.BindViewEventsGeneratedMethodName, MethodAttributes.Private | MethodAttributes.HideBySig, _moduleDefinition.TypeSystem.Void);
            subscriptionsInitMethod.Parameters.Add(new ParameterDefinition("view", ParameterAttributes.None, _referencesProvider.AndroidViewTypeReference));
            
            subscriptionsInitMethod.Body.Variables.Add(new VariableDefinition(_referencesProvider.AndroidViewTypeReference));
            subscriptionsInitMethod.Body.Variables.Add(new VariableDefinition(_moduleDefinition.TypeSystem.Boolean));

            Logger.Debug($"Add subscription init method {subscriptionsInitMethod} into {_typeDefinition}");

            _typeDefinition.Methods.Add(subscriptionsInitMethod);
            
            var ilProcessor = subscriptionsInitMethod.Body.GetILProcessor();
            Instruction lastInstruction = null;

            var dict = new Dictionary<int, List<MethodDefinition>>();

            foreach (var method in _methodsToSubscribe)
            {
                var attribute = method.CustomAttributes.First(x => x.AttributeType.FullName == Consts.InjectorAttributes.ViewEventAttributeTypeName);
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
                var shouldThrowExceptionIfNull = false;

                foreach (var methodToSubscribe in methodsToSubscribe)
                {
                    var attributeArguments = methodToSubscribe.CustomAttributes
                        .First(x => x.AttributeType.FullName == Consts.InjectorAttributes.ViewEventAttributeTypeName).ConstructorArguments;

                    var argsLength = attributeArguments.Count;

                    shouldThrowExceptionIfNull = shouldThrowExceptionIfNull || !(bool)attributeArguments[argsLength == 4 ? 3 : 2].Value;

                    var eventName = (string)attributeArguments[argsLength == 4 ? 2 : 1].Value;
                    var viewType = argsLength == 4 ? (TypeReference)attributeArguments[1].Value : _referencesProvider.AndroidViewTypeReference;

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
                        throw new WeavingException($"Can't find event {eventName} in {viewType}");
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

                lastInstruction = AddSubscribtionInstructions(lastInstruction, ilProcessor, resourceId, listMs, shouldThrowExceptionIfNull);
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
                InsertBefore(ref li, Instruction.Create(OpCodes.Newobj, _referencesProvider.InjectorExceptionCtor));
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
            InsertBefore(ref li, Instruction.Create(OpCodes.Ceq));
            InsertBefore(ref li, Instruction.Create(OpCodes.Ldnull));
            InsertBefore(ref li, Instruction.Create(OpCodes.Ldloc_0));

            InsertBefore(ref li, Instruction.Create(OpCodes.Stloc_0));
            InsertBefore(ref li, Instruction.Create(OpCodes.Callvirt, _referencesProvider.FindViewByIdMethodReference));
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

        public override string ToString()
        {
            return $"{nameof(_referencesProvider)}: {_referencesProvider}, {nameof(_moduleDefinition)}: {_moduleDefinition}, {nameof(_methodsToSubscribe)}: {_methodsToSubscribe}, {nameof(_typeDefinition)}: {_typeDefinition}";
        }
    }
}
