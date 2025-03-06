using UnityEngine;

public class DestinationSpot : MonoBehaviour
{
    public bool isOccupied = false;

    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;
    }

    public bool IsOccupied()
    {
        return isOccupied;
    }
}
