using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileControl : MonoBehaviour
{
    ////////////////////////////////////////////// TouchMode (Control) ////////////////////////////////////////////////////////////////////

    public void CarAccelForward(float amount)
    {
        AIControl.CurrentVehicle.accelFwd = amount;
        Debug.Log("CarAccelForward" + amount);
    }
    public void CarAccelBack(float amount)
    {
        AIControl.CurrentVehicle.accelBack = amount;
        Debug.Log("CarAccelBack" + amount);
    }
    public void CarSteer(float amount)
    {
        Debug.Log("CarSteer" + amount);
        AIControl.CurrentVehicle.steerAmount = amount;
    }
    public void CarHandBrake(bool HBrakeing)
    {
        Debug.Log("CarHandBrake" + HBrakeing);
        AIControl.CurrentVehicle.brake = HBrakeing;
    }
    public void CarShift(bool Shifting)
    {
        Debug.Log("CarShift" + Shifting);
        AIControl.CurrentVehicle.shift = Shifting;
    }

}
