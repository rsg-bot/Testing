﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Moq;

namespace Rocket.Surgery.Extensions.Testing
{
    class RemoveMockFromEnumerableRegistrationSource : IRegistrationSource
    {
        public IEnumerable<IComponentRegistration> RegistrationsFor(
            Service service,
            Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor
        )
        {
            // This is only designed to work with IEnumerable, not array or list.
            if (service is TypedService typedService && typedService.ServiceType.IsGenericType &&
                typedService.ServiceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var registrations = registrationAccessor(service);
                foreach (var registration in registrations)
                {
                    registration.Activating += (sender, args) =>
                    {
                        var method = typeof(RemoveMockFromEnumerableRegistrationSource)
                           .GetMethod(nameof(ReplaceInstance), BindingFlags.Static | BindingFlags.NonPublic)
                           .MakeGenericMethod(typedService.ServiceType.GenericTypeArguments[0]);

                        if (args.Instance is IEnumerable<object> enumerable &&
                            enumerable.Any(x => x is IMocked))
                        {
                            method.Invoke(null, new object[] { args, enumerable });
                        }
                    };
                }
            }

            yield break;
        }

        public bool IsAdapterForIndividualComponents { get; } = true;

        static void ReplaceInstance<T>(ActivatingEventArgs<object> args, IEnumerable<T> items)
        {
            args.ReplaceInstance(items.Where(z => !(z is IMocked)).ToArray().AsEnumerable());
        }
    }
}