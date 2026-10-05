namespace RiverReel.Windows.Models;

/// <summary>The playable toons — the Project Delta sprite roster.</summary>
public sealed class Toon
{
    public Toon(string id, string name) { Id = id; Name = name; }
    public string Id { get; }
    public string Name { get; }
}

public static class Roster
{
    public static readonly IReadOnlyList<Toon> All = new List<Toon>
    {
        new("popeye", "Popeye"),
        new("felix", "Felix the Cat"),
        new("oswald", "Oswald"),
        new("koko", "Koko"),
        new("bimbo", "Bimbo"),
        new("pooh", "Winnie the Pooh"),
        new("olive", "Olive Oyl"),
        new("bosko", "Bosko"),
        new("pete", "Peg-Leg Pete"),
    };

    public static Toon? ById(string id) => All.FirstOrDefault(t => t.Id == id);
}
