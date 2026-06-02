using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ScriptableObject que guarda qué dispositivo le corresponde a cada personaje.
/// Persiste entre escenas porque vive en el proyecto como asset, no en la escena.
///
/// Cómo usarlo:
///  1. Click derecho en Project > Create > CharacterSelect > PlayerAssignment
///  2. Asigná ese asset tanto en CharacterSelectManager como en GameStarter.
/// </summary>
[CreateAssetMenu(menuName = "CharacterSelect/PlayerAssignment")]
public class PlayerAssignment : ScriptableObject
{
    //TODO: (Fix this) They must be static, otherwise the data gets lost between scenes.
    public static InputDevice DragonDevice { get; set; }
    public static InputDevice MechaDevice { get; set; }
    public bool BothAssigned => DragonDevice != null && MechaDevice != null;

    public void Reset()
    {
        DragonDevice = null;
        MechaDevice = null;
    }

    ///// <summary>
    ///// Devuelve el InputDevice guardado para el Dragon, o null si no está asignado.
    ///// </summary>
    //public InputDevice GetDragonDevice()
    //{
    //    return GetDevice(DragonDeviceIndex);
    //}

    ///// <summary>
    ///// Devuelve el InputDevice guardado para el Mecha, o null si no está asignado.
    ///// </summary>
    //public InputDevice GetMechaDevice()
    //{
    //    return GetDevice(MechaDeviceIndex);
    //}

    //private InputDevice GetDevice(int index)
    //{
    //    if (index < 0) return null;
    //    var devices = InputSystem.devices;
    //    if (index >= devices.Count) return null;
    //    return devices[index];
    //}
}
