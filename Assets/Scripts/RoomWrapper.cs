using System.Collections.Generic;
using UnityEngine;
public class RoomWrapper
{
    public RectInt room;
    public List<DoorWrapper> doors = new();
    public bool pendingDeletion;

    public RoomWrapper(RectInt room)
    {
        this.room = room;
    }
}
