using UnityEngine;
using System.Collections.Generic;

public class DungeonWrapper : MonoBehaviour
{
    public enum DungeonStatus { Empty, RoomsCompleted, DoorsCompleted, ConnectionsCompleted }
    public List<RoomWrapper> rooms = new();
    public List<RoomWrapper> reducedRooms = new();
    public List<DoorWrapper> doors = new();
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
                if (TryGetComponent(out ConnectionGenerator connectionGenerator))
                {
                    if (connectionGenerator.autoGenerate)
                    {
                        connectionGenerator.StartGeneration();
                    }
                }
                break;
            case DungeonStatus.ConnectionsCompleted:
                Debug.Log("Dungeon generation complete");
                break;
        }
    }
}
