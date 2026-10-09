using ImageCampus.ToolBox.Services;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Condition/OffCamera")]
public class OffCameraCondition : Condition<IEnemyContext>
{
    private CoopCameraController CameraController => ServiceProvider.Instance.GetService<CoopCameraController>();

    public override bool Evaluate(IEnemyContext context)
    {
        return !CameraController.IsInCameraBounds(context.Transform.position);
    }
}


