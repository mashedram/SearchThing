namespace SearchThing.Extensions.Panel.Data.Extensions;

public class NameOverwriteExtension : IItemRenderExtension
{
    public string Name { get; set; }

    public NameOverwriteExtension(string name)
    {
        Name = name;
    }
}