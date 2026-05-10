using UnityEngine;

public class TrashItem : MonoBehaviour
{
    public enum TrashColor { Red, Blue, Green }
    public TrashColor trashColor;
    public int basePoints;

    void Start()
    {
        basePoints = (trashColor == TrashColor.Green) ? 20 : 10;
    }
}