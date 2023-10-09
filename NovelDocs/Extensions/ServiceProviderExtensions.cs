using Microsoft.Extensions.DependencyInjection;
using System;

namespace NovelDocs.Extensions; 

internal static class ServiceProviderExtensions {
    public static T CreateInstance<T>(this IServiceProvider serviceProvider) {
        return ActivatorUtilities.CreateInstance<T>(serviceProvider);
    }

    public static object CreateInstance(this IServiceProvider serviceProvider, Type type) {
        return ActivatorUtilities.CreateInstance(serviceProvider, type);
    }
}