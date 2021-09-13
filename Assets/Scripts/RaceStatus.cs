using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RaceStatus : MonoBehaviour
{

    [SerializeField] public bool start = false;
    [SerializeField] public bool saferyCar = false;
    [SerializeField] public float safetyCarMaxSpeed = 1.0f;
    [SerializeField] public bool redFlag = false;

    public void DeliverSafetyCar()
    {
        Debug.Log("DeliverSafetyCar");
        saferyCar = true;
    }

    public void BringBackSafetyCar()
    {
        Debug.Log("BringBackSafetyCar");
        saferyCar = false;
    }

    public void SendRedFlag()
    {
        Debug.Log("SendRedFlag");
        DeliverSafetyCar();
        redFlag = true;
    }

    public bool IsSafetyCarDelivered()
    {
        Debug.Log("IsSafetyCarDelivered");
        return saferyCar;
    }
}
