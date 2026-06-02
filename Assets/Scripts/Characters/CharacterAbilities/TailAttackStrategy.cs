using ImageCampus.ToolBox.Services;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Attacks/Ground Tail (Y)")]
public class TailAttackStrategy : AttackStrategy
{
    [SerializeField] private float _hitboxSizeX = 3f; 
    [SerializeField] private float _hitboxSizeY = 1.5f;
    [SerializeField] private float _knockBackXForce = 10f;
    [SerializeField] private float _knockBackYForce = 2f;
    private float _currentAttackTimer;

    private RuntimeDebugVisual _debugVisual;

    public override void Execute(Vector2 aimDir)
    {
        if (!character.IsGrounded)
            return;

        isExecuting = true;

        _currentAttackTimer = attackSpeed;

        character.IsIgnoringInput = true;
        character.IsBlockingRotation = true;
        character.ApplyHVelocity(0);

        Vector2 attackPos = (Vector2)character.transform.position + (Vector2.up * (_hitboxSizeY / 2f));

        if(!_debugVisual)
            _debugVisual = ServiceProvider.Instance.GetService<RuntimeDebugVisual>();

        _debugVisual.DrawBox(attackPos, new(_hitboxSizeX, _hitboxSizeY), Color.magenta, attackSpeed);

        Collider2D[] hits = Physics2D.OverlapBoxAll(attackPos, new(_hitboxSizeX, _hitboxSizeY), enemyLayer);

        foreach (var hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            damageable?.TakeDamage(damage);

            if (hit.TryGetComponent<Rigidbody2D>(out var enemyRb))
            {
                float dir = Mathf.Sign(hit.transform.position.x - character.transform.position.x);
                enemyRb.linearVelocity = Vector2.zero;
                enemyRb.AddForce(new(dir * _knockBackXForce, _knockBackYForce), ForceMode2D.Impulse);
            }
        }
    }

    public override void Tick()
    {
        if (!isExecuting)
            return;

        _currentAttackTimer -= Time.deltaTime;

        if (_currentAttackTimer < 0f)
        {
            character.IsIgnoringInput = false;
            character.IsBlockingRotation = false;
            isExecuting = false;
        }
    }
}

