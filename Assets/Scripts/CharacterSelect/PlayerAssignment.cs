using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "CharacterSelect/PlayerAssignment")]
public class PlayerAssignment : ScriptableObject
{
    public int DragonDeviceId = -1;
    public int MechaDeviceId = -1;

    public bool BothAssigned => DragonDeviceId >= 0 && MechaDeviceId >= 0;

    public void Reset()
    {
        DragonDeviceId = -1;
        MechaDeviceId = -1;
    }

    public InputDevice GetDragonDevice() => GetDeviceById(DragonDeviceId);
    public InputDevice GetMechaDevice() => GetDeviceById(MechaDeviceId);

    private InputDevice GetDeviceById(int id)
    {
        if (id < 0) return null;
        return InputSystem.GetDeviceById(id);
    }
}
