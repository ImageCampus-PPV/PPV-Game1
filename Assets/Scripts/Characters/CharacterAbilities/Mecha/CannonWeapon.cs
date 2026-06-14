using UnityEngine;
using UnityEngine.InputSystem;


[CreateAssetMenu(menuName = "Abilities/Weapons/Cannon")]
public class CannonWeapon : WeaponStrategy
{
    [Header("Cannon Config")]
    [SerializeField] private float _projectileSpeed = 12f;
    [SerializeField] private Projectile _projectilePrefab;

    public override void OnPressed(InputAction.CallbackContext context, Vector2 aimDir)
    {
        if (!context.started) 
            return;

        if (IsOnCooldown) 
            return;

        if (_projectilePrefab == null)
        {
            Debug.LogWarning("[CannonWeapon] Projectile prefab not assigned.");
            return;
        }

        Fire(aimDir);
    }

    private void Fire(Vector2 aimDir)
    {
        lastFireTime = Time.time;

        Projectile proj = Object.Instantiate(_projectilePrefab, character.transform.position, Quaternion.identity);

        proj.Initialize(damage, _projectileSpeed, range, enemyLayer, aimDir);
    }
}
