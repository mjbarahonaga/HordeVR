using UnityEngine;

[CreateAssetMenu(fileName = "Trap.asset", menuName = "HordeVR/Create Scriptable Trap")]
public class DataTrap : ScriptableObject
{

    public int Damage = 20;
    public float CoolDown = 2f;
    public float Price = 100f;

}
