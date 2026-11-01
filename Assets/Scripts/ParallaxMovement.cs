
using UnityEngine;

public sealed class ParallaxMovement : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera _camera;

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

            ParallaxSettings settings =  child.GetComponentInChildren<ParallaxSettings>();

            if (layerRenderer == null || settings == null)
            {
                Debug.LogError(  $"Layer '{child.name}' requires a Renderer and ParallaxSettings.", child);

                enabled = false;
                return;
            }

            Material material = layerRenderer.material;

            int texturePropertyId = material.HasProperty(BaseMapId) ? BaseMapId : MainTexId;

            if (!material.HasProperty(texturePropertyId))
            {
                Debug.LogError(  $"Material on '{child.name}' has no supported texture property.", child);

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
        Vector3 cameraDisplacement =  _camera.transform.position - _initialCameraPosition;

        foreach (Layer layer in _layers)
        {
            ParallaxSettings settings = layer.Settings;

            bool repeatX = settings.RepeatHorizontally;
            bool repeatY = settings.RepeatVertically;

            Vector3 position = layer.InitialPosition;

            if (repeatX || settings.FollowCameraHorizontally)
                position.x += cameraDisplacement.x;

            if (repeatY || settings.FollowCameraVertically)
                position.y += cameraDisplacement.y;

            layer.Transform.position = position;

            Vector2 offset = layer.InitialTextureOffset;
            float factor = settings.ParallaxFactor;

            if (repeatX)
                offset.x -= cameraDisplacement.x * factor;

            if (repeatY)
                offset.y -= cameraDisplacement.y * factor;

            layer.Material.SetTextureOffset( layer.TexturePropertyId, offset);
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
