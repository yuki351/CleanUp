using UnityEngine;

public class TrashBin : MonoBehaviour
{
    public enum BinColor { Red, Blue, Green }
    public BinColor binColor;

    private PlayerController player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")
                           .GetComponent<PlayerController>();
    }

    void OnMouseDown()
    {
        if (player != null && player.IsCarryingTrash())
        {
            player.DropTrashInBin(this);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null && pc.IsCarryingTrash())
            {
                pc.DropTrashInBin(this);
            }
        }
    }
}