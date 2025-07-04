using System;

namespace NovelWriter.PageControls; 

public static class PageControllerConfiguration {
    public static IPageDependencyResolver PageDependencyResolver { get; set; } = new DefaultPageDependencyResolver(); 
}

internal sealed class DefaultPageDependencyResolver : IPageDependencyResolver {
    public object? Resolve(Type type) {
        return Activator.CreateInstance(type);
    }
}