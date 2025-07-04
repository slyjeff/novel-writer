using System.IO;
using System;

namespace NovelDocs.Services; 

internal static class DirectoryService {
    public static string LocalImages {
        get {
            var directory = AppDomain.CurrentDomain.BaseDirectory;
            var imagesDirectory = Path.Combine(directory, "Images");
            if (!Directory.Exists(imagesDirectory)) {
                Directory.CreateDirectory(imagesDirectory);
            }

            return imagesDirectory;
        }
    }
}