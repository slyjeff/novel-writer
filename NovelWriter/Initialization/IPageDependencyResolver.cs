using System;
using NovelWriter.Extensions;
using NovelWriter.PageControls;

namespace NovelWriter.Initialization; 

internal sealed class PageDependencyResolver : IPageDependencyResolver {
    private readonly IServiceProvider _serviceProvider;

    public PageDependencyResolver(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public object? Resolve(Type type) {
        var service = _serviceProvider.GetService(type);
        return service == null 
            ? _serviceProvider.CreateInstance(type) 
            : null;
    }
}