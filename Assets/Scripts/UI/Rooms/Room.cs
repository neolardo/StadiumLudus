using System.Collections.Generic;
/// <summary>
/// Represents a room.
/// </summary>
public class Room
{
    public string Name { get;  }
    public string Password { get;  }
    public RoomStatus Status { get; set; }
    public List<string> Players { get; } = new List<string>();

    public const int MaximumPlayerCount = 4;

    public bool IsSameAs(Room r)
    {
        return r.Name == Name;
    }

    public Room(string name, string password, string firstPlayerUsername)
    {
        Name = name;
        Password = password;
        Status = RoomStatus.CharacterSelection;
        Players.Add(firstPlayerUsername);
    }
}
