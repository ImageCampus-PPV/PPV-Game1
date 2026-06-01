using UnityEngine;

public class StickyWall : MonoBehaviour
{
    [SerializeField] private Vector2 _wallNormal = Vector2.left;

    public Vector2 Normal => _wallNormal.normalized;

    private void OnValidate()
    {
        _wallNormal.Normalize();
    }
}