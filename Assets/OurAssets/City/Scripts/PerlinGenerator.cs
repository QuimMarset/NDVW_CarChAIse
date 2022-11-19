using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerlinGenerator : MonoBehaviour
{
    public static PerlinGenerator instance = null;

    public int perlinTextureSizeX;
    public int perlinTextureSizeY;
    public bool randomizeNoiseOffset;
    public Vector2 perlinOffset;
    public float noiseScale = 1f;
    public int perlinGridStepSizeX = 4;
    public int perlinGridStepSizeY = 4;

    private Texture2D perlinTexture;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    public void Generate()
    {
        GenerateNoise();
    }

    void GenerateNoise()
    {
        if (randomizeNoiseOffset)
        {
            perlinOffset = new Vector2(Random.Range(0, 99999), Random.Range(0, 99999));
        }

        perlinTexture = new Texture2D(perlinTextureSizeX, perlinTextureSizeY);

        for (int x = 0; x < perlinTextureSizeX; x++)
        {
            for (int y = 0; y < perlinTextureSizeY; y++)
            {
                perlinTexture.SetPixel(x, y, SampleNoise(x, y));
            }
        }

        perlinTexture.Apply();
    }

    Color SampleNoise(int x, int y)
    {
        float xCoord = (float)x / perlinTextureSizeX * noiseScale + perlinOffset.x;
        float yCoord = (float)y / perlinTextureSizeY * noiseScale + perlinOffset.y;

        float sample = Mathf.PerlinNoise(xCoord, yCoord);
        Color perlinColor = new Color(sample, sample, sample);

        return perlinColor;
    }

    public float SampleStepped(int x, int y)
    {
        int gridStepSizeX = perlinTextureSizeX / perlinGridStepSizeX;
        int gridStepSizeY = perlinTextureSizeY / perlinGridStepSizeY;

        float sampledFloat = perlinTexture.GetPixel
                   ((Mathf.FloorToInt(x * gridStepSizeX)), (Mathf.FloorToInt(y * gridStepSizeX))).grayscale;

        return sampledFloat;
    }
    
    public float PerlinSteppedPosition(Vector3 worldPosition)
    {
        int xToSample = Mathf.FloorToInt(worldPosition.x + perlinGridStepSizeX * .5f);
        int yToSample = Mathf.FloorToInt(worldPosition.z + perlinGridStepSizeY * .5f);

        xToSample = xToSample % perlinGridStepSizeX;
        yToSample = yToSample % perlinGridStepSizeY;

        float sampledValue = SampleStepped(xToSample,yToSample);

        return sampledValue;
    }
    
    public void SetNoiseScaleFromSlider(Slider slider)
    {
        noiseScale = slider.value;
    }

}
