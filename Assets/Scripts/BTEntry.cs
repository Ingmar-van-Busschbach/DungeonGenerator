using UnityEngine;
public class BTEntry
{
    public enum BTEntryStatus { Complete, MissingLeft, MissingRight, MissingBoth }

    public RectInt room;
    public BTEntryStatus complete = BTEntryStatus.MissingBoth;
    [Tooltip("Leafs are the rooms without any child rooms")]
    public bool leaf = false;
    public BTEntry parent;
    public BTEntry left;
    public BTEntry right;

    public BTEntry(BTEntry parent, RectInt room)
    {
        this.parent = parent;
        this.room = room;
        if (parent == null)
        {
            return;
        }
        if (parent.right == null)
        {
            parent.right = this;
        }
        else if (parent.left == null)
        {
            parent.left = this;
        }
    }
    public void CheckComplete()
    {
        bool completeRight = false;
        bool completeLeft = false;

        if(right != null)
        {
            if(right.complete == BTEntryStatus.Complete)
            {
                completeRight = true;
            }
        }
        if (left != null)
        {
            if (left.complete == BTEntryStatus.Complete)
            {
                completeLeft = true;
            }
        }

        if (completeRight && completeLeft)
        {
            complete = BTEntryStatus.Complete;
        }
        else if (completeLeft)
        {
            complete = BTEntryStatus.MissingRight;
        }
        else if (completeRight)
        {
            complete = BTEntryStatus.MissingLeft;
        }
        else
        {
            complete = BTEntryStatus.MissingBoth;
        }
    }
}
