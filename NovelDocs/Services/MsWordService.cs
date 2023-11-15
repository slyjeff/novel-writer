using NovelDocs.Entity;

namespace NovelDocs.Services;


public interface IMsWordService {
    public void Compile(Novel novel);
}

internal sealed class MsWordService : IMsWordService {
    public MsWordService() {
        
    }

    public void Compile(Novel novel) {
        throw new System.NotImplementedException();
    }
}