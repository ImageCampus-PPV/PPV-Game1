
using UnityEngine;

public sealed class ParallaxSettings : MonoBehaviour
{
    [Header("Texture Repetition")]
    [SerializeField] private bool _repeatHorizontally = true;
    [SerializeField] private bool _repeatVertically = true;

    [Header("Camera Following")]
    [SerializeField] private bool _followCameraVertically;

    [Header("Parallax")]
    [SerializeField, Range(0f, 1f)]
    private float _parallaxFactor = 0.5f;

    public bool RepeatHorizontally => _repeatHorizontally;
    public bool RepeatVertically => _repeatVertically;
    public bool FollowCameraVertically => _followCameraVertically;
    public float ParallaxFactor => _parallaxFactor;
}
