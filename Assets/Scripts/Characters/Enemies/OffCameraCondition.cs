using ImageCampus.ToolBox.Services;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Condition/OffCamera")]
public class OffCameraCondition : Condition<IEnemyContext>
{
    private CoopCameraController CameraController => ServiceProvider.Instance.ContainsService<CoopCameraController>() ? ServiceProvider.Instance.GetService<CoopCameraController>() : null;

    public override bool Evaluate(IEnemyContext context)
    {
        if (!CameraController)
        {
            Debug.LogError("No camera controller found in OffCamera condition for enemy " + context.Transform.name);
            return false;
        }

        return !CameraController.IsInCameraBounds(context.Transform.position);
    }
}


