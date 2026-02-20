using UnityEngine;
using System.Collections.Generic;

public class DungeonWrapper : MonoBehaviour
{
    public enum DungeonStatus { Empty, RoomsCompleted }

    public BTEntry origin;
    public List<BTEntry> rooms = new();
    public DungeonStatus dungeonStatus = DungeonStatus.Empty;
}
