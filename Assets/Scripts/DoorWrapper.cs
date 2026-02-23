using System.Collections.Generic;
using UnityEngine;

public class DoorWrapper
{
    public RectInt door;
    public List<RoomWrapper> connectingRooms = new();

    public DoorWrapper(RectInt door)
    {
        this.door = door;
    }
}
