using UnityEngine;
using System.Collections.Generic;

public class DungeonWrapper : MonoBehaviour
{
    public enum DungeonStatus { Empty, RoomsCompleted, DoorsCompleted }
    public List<RoomWrapper> rooms = new();
    public List<RectInt> doors = new();
    public DungeonStatus dungeonStatus = DungeonStatus.Empty;

    public void ChangeDungeonStatus(DungeonStatus dungeonStatus)
    {
        this.dungeonStatus = dungeonStatus;
        switch (dungeonStatus)
        {
            case DungeonStatus.RoomsCompleted:
                if (TryGetComponent(out DoorGenerator doorGenerator))
                {
                    if (doorGenerator.autoGenerate)
                    {
                        doorGenerator.StartGeneration();
                    }
                }
                break;
            case DungeonStatus.DoorsCompleted:
                Debug.Log("Dungeon generation complete");
                break;
        }
    }
}
