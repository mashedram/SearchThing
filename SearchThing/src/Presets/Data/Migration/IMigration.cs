namespace SearchThing.Presets.Data.Migration;

public interface IMigration
{
    int FromVersion { get; }
    int ToVersion { get; }
    void Migrate(object data);
}