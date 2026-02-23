using System.Collections.Generic;
using UnityEngine;
public class RoomWrapper
{
    public RectInt room;
    public List<RectInt> doors = new();
    public List<RoomWrapper> connectingRooms = new();

    public RoomWrapper(RectInt room)
    {
        this.room = room;
    }
}
