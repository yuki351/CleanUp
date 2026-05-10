using UnityEngine;

public class VehicleMover : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 5f;

    private int currentWaypoint = 0;

    void Update()
    {
        // Stop if all waypoints are completed
        if (currentWaypoint >= waypoints.Length)
            return;

        // Move towards waypoint
        transform.position = Vector3.MoveTowards(
            transform.position,
            waypoints[currentWaypoint].position,
            speed * Time.deltaTime
        );

        // Rotate towards waypoint
        Vector3 direction = waypoints[currentWaypoint].position - transform.position;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Check if reached waypoint
        if (Vector3.Distance(transform.position, waypoints[currentWaypoint].position) < 0.2f)
        {
            currentWaypoint++;
        }
    }
}