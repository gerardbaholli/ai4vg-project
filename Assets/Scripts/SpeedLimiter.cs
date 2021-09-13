using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedLimiter : MonoBehaviour
{
    public enum Sensor { entranceSpeedLimiter, exitSpeedLimiter };

    [SerializeField] Sensor speedLimiter;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CarController collisionCarController = collision.gameObject.GetComponentInParent<CarController>();
        CarStatus collisionCarStatus = collision.gameObject.GetComponentInParent<CarStatus>();
        if (collisionCarController != null)
        {
            if (speedLimiter == Sensor.entranceSpeedLimiter){
                collisionCarController.SetSpeed(1.2f);
                collisionCarStatus.SetActualLocation(CarStatus.ActualLocation.Pitlane);
            }
            else if (speedLimiter == Sensor.exitSpeedLimiter)
            {
                collisionCarController.SetSpeed(2f);
                collisionCarStatus.SetActualLocation(CarStatus.ActualLocation.Track);
            }
        }
    }

}
