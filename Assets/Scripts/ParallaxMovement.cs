using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxMovement : MonoBehaviour
{
    private ParallaxSettings[] parallaxSettings;
    Transform cam;
    Vector3 camStartPos;
    float distance;

    GameObject[] backgrounds;
    Material[] mat;
    float[] backSpeed;

    float farthestBack;

    [Range(0.01f, 1f)]
    public float parallaxSpeed;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main.transform;
        camStartPos = cam.position;

        int backCount = transform.childCount;
        mat = new Material[backCount];
        backSpeed = new float[backCount];
        backgrounds = new GameObject[backCount];

        parallaxSettings = new ParallaxSettings[backCount];

        for (int i = 0; i < backCount; i++)
        {
            backgrounds[i] = transform.GetChild(i).gameObject;
            mat[i] = backgrounds[i].GetComponent<Renderer>().material;
            parallaxSettings[i] = backgrounds[i].GetComponent<ParallaxSettings>();
        }

        BackSpeedCalculate(backCount);
    }

    void BackSpeedCalculate(int backCount)
    {
        for (int i = 0; i < backCount; i++)
        {
            if ((backgrounds[i].transform.position.z - cam.position.z) > farthestBack)
            {
                farthestBack = backgrounds[i].transform.position.z - cam.position.z;
            }
        }

        for (int i = 0; i < backCount; i++)
        {
            backSpeed[i] = 1 - (backgrounds[i].transform.position.z - cam.position.z) / farthestBack;
        }
    }

    private void LateUpdate()
    {
        float horizontalDistance = cam.position.x - camStartPos.x;
        float verticalDistance = cam.position.y - camStartPos.y;
        transform.position = new Vector3(cam.position.x - 1, transform.position.y, 2f);

        for (int i = 0; i < backgrounds.Length; i++)
        {
            if (mat[i] == null)
                continue;
            bool useVerticalParallax = parallaxSettings[i] != null && parallaxSettings[i].verticalParallax;
            float verticalOffset = useVerticalParallax ? verticalDistance : 0f;
            Vector2 offset = new Vector2(horizontalDistance, verticalOffset);
            float speed = backSpeed[i] * parallaxSpeed;
            mat[i].SetTextureOffset("_MainTex", offset * speed);
        }
    }
}
