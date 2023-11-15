using NovelDocs.Entity;

namespace NovelDocs.Managers;

public interface IMsWordManager {
    void Compile(Novel novel);
}

internal sealed class MsWordManager : IMsWordManager {
    public void Compile(Novel novel) {
    }
}