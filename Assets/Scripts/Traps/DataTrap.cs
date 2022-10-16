using System;
using UnityEngine;

public enum TypeTrap
{
    None,
    Spikes,
    Arrows,
    SwingBlades
}

[Flags]
public enum TypeSurface
{
    None = 0,
    Floor = 1,
    Ceiling = 2,
    Wall = 4,
    Everything= 15
}

[CreateAssetMenu(fileName = "Trap.asset", menuName = "HordeVR/Create Scriptable Trap")]
public class DataTrap : ScriptableObject
{
    public TypeTrap Type;
    public TypeSurface Surface; 
    public int Damage = 20;
    public float CoolDown = 2f;
    public int Price = 100;

}
