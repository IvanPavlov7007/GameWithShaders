using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveCreator : MonoBehaviour
{
    public Shader waveShader;

    [SerializeField]
    private float maxWaveForce, waveSpeed, waveThickDuration, distortionDist;
    [SerializeField]
    private Material curMaterial;

    private float pixelSize;
    private Rigidbody2D[] objectsInWater;
    private Camera cam;
    Vector2? currentMousePosition = null;

    private Material material
    {
        get
        {
            if (curMaterial == null)
            {
                curMaterial = new Material(waveShader);
                curMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            return curMaterial;
        }
    }

    private void Start()
    {
        cam = GetComponent<Camera>();
        material.SetFloat("_Duration", waveThickDuration);
        var objects = GameObject.FindGameObjectsWithTag("InWater");
        objectsInWater = new Rigidbody2D[objects.Length];
        for(int i = 0; i < objects.Length;i++)
        {
            objectsInWater[i] = objects[i].GetComponent<Rigidbody2D>();
        }
    }

    float time = 0f;
    private void Update()
    {
        time += Time.deltaTime;
        if (Input.GetMouseButtonDown(0))
        {
            time = 0f;
            float longestDist;
            material.SetTexture("_RadianDistMap", distMap(out longestDist));
            material.SetFloat("_MaxTime", longestDist / waveSpeed);
            currentMousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
        }
        material.SetFloat("_CurrentTime", time);
        pixelSize = (cam.ScreenToWorldPoint(Vector3.right) - cam.ScreenToWorldPoint(Vector3.zero)).magnitude;
    }

    const float objectsRadius = 1.2f;
    const float cosPeriod = Mathf.PI / (2f * objectsRadius); 

    private void FixedUpdate()
    {
        if (currentMousePosition == null)
            return;
        float outerRange = pixelSize * waveSpeed * time;
        float innerRange = outerRange -  waveSpeed * pixelSize * waveThickDuration;
        float closestPoint, furthestPoint,x;
        Vector2 direction;
        for (int i = 0; i < objectsInWater.Length; i++)
        {
            direction = objectsInWater[i].position - currentMousePosition.Value;
            closestPoint = direction.magnitude;
            furthestPoint = closestPoint + objectsRadius;
            closestPoint -= objectsRadius;
            if(outerRange > closestPoint && outerRange < furthestPoint)
            {
                x = outerRange - closestPoint;
                //objectsInWater[i].AddForce(direction.normalized * -Mathf.Cos(x * cosPeriod) * maxWaveForce);
                objectsInWater[i].AddForce(direction.normalized * maxWaveForce);
            }
        }
    }

    private Texture distMap(out float longestDist)
    {
        Vector2 mousePosition = Input.mousePosition;
        int width = Screen.width;
        int height = Screen.height;
        longestDist = findLongestDistInRect(mousePosition, width, height);
        Texture2D texture = new Texture2D(Screen.width, Screen.height,TextureFormat.RGBAHalf,false,true);
        Color[] pixels = new Color[width * height];
        int i;
        Vector2 direction;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                i = x + width * y;
                direction = new Vector2(x, y) - mousePosition;
                pixels[i].r = direction.magnitude / longestDist;
                direction = direction.normalized * distortionDist;
                pixels[i].g = direction.x / width;
                pixels[i].b = direction.y / height;
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private float findLongestDistInRect(Vector2 point, float width, float height)
    {
        float maxDist = point.magnitude;
        Vector2 vec = new Vector2(width - point.x, point.y);
        float vecDist = vec.magnitude;
        if (vecDist > maxDist) maxDist = vecDist;
        vec = new Vector2(point.x, height - point.y);
        vecDist = vec.magnitude;
        if (vecDist > maxDist) maxDist = vecDist;
        vec = new Vector2(width - point.x, height - point.y);
        vecDist = vec.magnitude;
        if (vecDist > maxDist) maxDist = vecDist;
        return maxDist;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, material);
    }
}
