using System;

namespace NovelDocs.PageControls; 

internal static class PageDependencyResolverExtensions {
    // ReSharper is complaining about this, but it works (I tried to use "ReSharper disable All", but it still complained)
    public static T? Resolve<T>(this IPageDependencyResolver pageDependencyResolver) where T : class {
        return (T?) pageDependencyResolver.Resolve(typeof(T));
    }
}

public interface IPageDependencyResolver {
    object? Resolve(Type type);
}