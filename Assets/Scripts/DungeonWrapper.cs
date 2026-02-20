using UnityEngine;

public class DungeonWrapper : MonoBehaviour
{
    public enum DungeonStatus { Empty, RoomsCompleted }

    public BTEntry origin;
    public DungeonStatus dungeonStatus = DungeonStatus.Empty;
}
