using System;
using System.IO;

namespace NovelWriter.Services; 

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