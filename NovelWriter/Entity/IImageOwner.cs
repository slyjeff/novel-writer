using System;

namespace NovelWriter.Entity; 

public interface IImageOwner {
    Guid ImageId { get; set; }
}
