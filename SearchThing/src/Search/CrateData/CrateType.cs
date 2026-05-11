namespace SearchThing.Search.CrateData;

public enum CrateType
{
    Prop,
    Avatar,
    Level,
    Invalid
}

public enum CrateSubType
{
    None,

    // Spawnable subtypes
    Gun,
    Melee,
    Throwable,
    Vehicle,

    // Avatar subtypes
    Large,
    Medium,

    Small
    // Level subtypes
    // TODO
}