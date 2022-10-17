using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InventorySelection : MonoBehaviour
{
    public static Action<TypeTrap> OnSendTypeSelected;
    public TypeTrap Type;

    public void SendTypeSelected() => OnSendTypeSelected?.Invoke(Type);

}
