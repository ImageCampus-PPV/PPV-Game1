
using UnityEngine;

public sealed class ParallaxMovement : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera _camera;

    [Tooltip("Horizontal distance between the root and the camera.")]
    [SerializeField] private float _cameraFollowXOffset = -1f;

    private static readonly int MainTexId = Shader.PropertyToID("_MainTex");

    private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");

    private Layer[] _layers;
    private Vector3 _initialCameraPosition;

    private sealed class Layer
    {
        public Transform Transform;
        public ParallaxSettings Settings;
        public Material Material;
        public int TexturePropertyId;
        public Vector3 InitialPosition;
        public Vector2 InitialTextureOffset;
    }

    private void Awake()
    {
        if (_camera == null)
            _camera = Camera.main;

        if (_camera == null)
        {
            Debug.LogError("ParallaxMovement requires a camera.");
            enabled = false;
            return;
        }

        _initialCameraPosition = _camera.transform.position;

        int layerCount = transform.childCount;
        _layers = new Layer[layerCount];

        for (int i = 0; i < layerCount; i++)
        {
            Transform child = transform.GetChild(i);
            Renderer layerRenderer = child.GetComponentInChildren<Renderer>();
            ParallaxSettings settings = child.GetComponentInChildren<ParallaxSettings>();

            if (layerRenderer == null || settings == null)
            {
                Debug.LogError($"Layer '{child.name}' requires a Renderer and ParallaxSettings.", child);

                enabled = false;
                return;
            }

            Material material = layerRenderer.material;

            int texturePropertyId = material.HasProperty(BaseMapId) ? BaseMapId : MainTexId;

            if (!material.HasProperty(texturePropertyId))
            {
                Debug.LogError($"Material on '{child.name}' has no supported main texture property (_MainTex or _BaseMap).");

                Destroy(material);
                enabled = false;
                return;
            }

            _layers[i] = new Layer
            {
                Transform = child,
                Settings = settings,
                Material = material,
                TexturePropertyId = texturePropertyId,
                InitialPosition = child.position,
                InitialTextureOffset = material.GetTextureOffset(texturePropertyId)
            };
        }
    }

    private void LateUpdate()
    {
        Vector3 cameraPosition = _camera.transform.position;

        Vector3 totalCameraDisplacement = cameraPosition - _initialCameraPosition;

        Vector3 rootPosition = transform.position;
        rootPosition.x = cameraPosition.x + _cameraFollowXOffset;
        transform.position = rootPosition;

        foreach (Layer layer in _layers)
        {
            ParallaxSettings settings = layer.Settings;
            float factor = settings.ParallaxFactor;

            Vector2 offset = layer.InitialTextureOffset;

            if (settings.RepeatHorizontally)
            {
                offset.x -= totalCameraDisplacement.x * factor;
            }

            if (settings.RepeatVertically)
            {
                offset.y -= totalCameraDisplacement.y * factor;
            }
            else if (settings.FollowCameraVertically)
            {
                Vector3 position = layer.Transform.position;
                position.y = layer.InitialPosition.y + totalCameraDisplacement.y;

                layer.Transform.position = position;
            }

            layer.Material.SetTextureOffset(layer.TexturePropertyId, offset);
        }
    }

    private void OnDestroy()
    {
        if (_layers == null)
            return;

        foreach (Layer layer in _layers)
        {
            if (layer?.Material != null)
                Destroy(layer.Material);
        }
    }
}
