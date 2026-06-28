using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Abilities/Weapons/Gatling")]
public class GatlingWeapon : WeaponStrategy
{
    [Header("Gatling Config")]
    [SerializeField] private float _projectileSpeed = 18f;
    [SerializeField] private float _fireRate = 0.08f;
    [SerializeField] private int _magazineSize = 50;
    [SerializeField] private float _reloadTime = 5f;
    [SerializeField] private Projectile _projectilePrefab;

    private int _currentAmmo;
    private bool _isReloading;
    private float _reloadTimer;
    private float _fireTimer;
    private bool _isTriggerHeld;
    private Vector2 _currentAimDir = Vector2.right;

    public int CurrentAmmo => _currentAmmo;
    public int MagazineSize => _magazineSize;
    public bool IsReloading => _isReloading;

    public override float CooldownProgress =>
        _isReloading && _reloadTime > 0f ? 1f - (_reloadTimer / _reloadTime) : 1f;

    public override bool HasCharge => true;

    public override float ChargeProgress =>
        _magazineSize > 0 ? (float)_currentAmmo / _magazineSize : 1f;

    public override void Initialize(Character character)
    {
        base.Initialize(character);
        if (_currentAmmo == 0)
            _currentAmmo = _magazineSize;
    }

    public override void OnPressed(InputAction.CallbackContext context, Vector2 aimDir)
    {
        if (context.started || context.performed)
            _isTriggerHeld = true;
    }

    public void UpdateAimDir(Vector2 aimDir)
    {
        if (aimDir.sqrMagnitude > 0.01f)
            _currentAimDir = aimDir;
    }

    public override void OnReleased(InputAction.CallbackContext context, Vector2 aimDir)
    {
        _isTriggerHeld = false;
        if (!_isReloading)
            StartReload();
    }

    public override void Cancel()
    {
        _isTriggerHeld = false;
    }

    public override void Tick()
    {
        if (_isReloading)
        {
            _reloadTimer -= Time.deltaTime;
            if (_reloadTimer <= 0f)
            {
                _isReloading = false;
                _currentAmmo = _magazineSize;
                //Debug.Log("[GatlingWeapon] Recarga completa.");
            }
            return;
        }

        if (!_isTriggerHeld)
            return;

        _fireTimer += Time.deltaTime;
        while (_fireTimer >= _fireRate)
        {
            _fireTimer -= _fireRate;
            if (_currentAmmo <= 0)
            {
                StartReload();
                return;
            }
            Fire();
        }
    }

    private void Fire()
    {
        if (_projectilePrefab == null)
        {
            Debug.LogWarning("[GatlingWeapon] Projectile prefab not assigned.");
            return;
        }

        _currentAmmo--;

        float spread = Random.Range(-3f, 3f);
        Vector2 dir = Quaternion.Euler(0, 0, spread) * _currentAimDir;
        Projectile proj = Object.Instantiate(_projectilePrefab, character.transform.position, Quaternion.identity);
        proj.Initialize(damage, _projectileSpeed, range, enemyLayer, dir);

        if (_currentAmmo <= 0)
            StartReload();
    }

    private void StartReload()
    {
        _isTriggerHeld = false;
        _isReloading = true;
        _fireTimer = 0f;

        float ammoMissing = (float)(_magazineSize - _currentAmmo) / _magazineSize;
        _reloadTimer = _reloadTime * ammoMissing;

        //.Log($"[GatlingWeapon] Recargando {_magazineSize - _currentAmmo} balas... ({_reloadTimer:F1}s)");
    }
}
