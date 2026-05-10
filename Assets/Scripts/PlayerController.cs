using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;
    public Transform playerCamera;

    [Header("Pickup")]
    public float pickupRange = 2.5f;
    public Transform holdPoint;

    private float xRotation = 0f;
    private TrashItem carriedTrash = null;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (carriedTrash == null)
                TryPickupNearby();
        }
    }

    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        transform.position += move * moveSpeed * Time.deltaTime;
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * 
                       mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * 
                       mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void TryPickupNearby()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(
                                   transform.position, pickupRange);

        TrashItem closestTrash = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider col in nearbyObjects)
        {
            TrashItem trash = col.GetComponent<TrashItem>();
            if (trash != null)
            {
                float dist = Vector3.Distance(
                             transform.position, col.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestTrash = trash;
                }
            }
        }

        if (closestTrash != null)
        {
            carriedTrash = closestTrash;
            carriedTrash.GetComponent<Collider>().enabled = false;

            carriedTrash.transform.SetParent(holdPoint);
            carriedTrash.transform.localPosition = Vector3.zero;
            carriedTrash.transform.localRotation = Quaternion.identity;

            Rigidbody rb = carriedTrash.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            GameManager.Instance.OnPickupTrash(
                        carriedTrash.trashColor.ToString());
            Debug.Log("Picked up: " + carriedTrash.trashColor + " trash");
        }
        else
        {
            Debug.Log("No trash nearby.");
        }
    }

    public void DropTrashInBin(TrashBin bin)
    {
        if (carriedTrash == null) return;

        if (bin.binColor.ToString() == carriedTrash.trashColor.ToString())
        {
            int points = carriedTrash.basePoints;
            Destroy(carriedTrash.gameObject);
            carriedTrash = null;

            GameManager.Instance.AddScore(points);
            GameManager.Instance.OnCorrectBin();
            Debug.Log("Correct bin!");
        }
        else
        {
            GameManager.Instance.OnWrongBin(
                        carriedTrash.trashColor.ToString(),
                        bin.binColor.ToString());
            Debug.Log("Wrong bin!");
        }
    }

    public bool IsCarryingTrash()
    {
        return carriedTrash != null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}