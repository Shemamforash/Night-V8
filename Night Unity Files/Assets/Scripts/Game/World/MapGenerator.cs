using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    public GameObject mapObject;
    public GameObject tileTemplate;
    private int width = 20, height = 20;
    private float maxTilesScreenWidth = 75f;
    //bigger = smoother
    private float noiseScale = 10f;
    private float tileWidth;
    private MapTile[,] mapTiles;
    private float seed;
    private Texture2D mapTexture;
    private bool navigationActive = false;
    public int persistence = 1, lacunarity = 1, octaves = 1;

    public void SetPersistence(Slider s)
    {
        int value = (int)s.value;
        persistence = value;
        Regenerate();
    }

    public void SetLacunarity(Slider s)
    {
        int value = (int)s.value;
        lacunarity = value;
        Regenerate();
    }

    public void SetOctaves(Slider s)
    {
        int value = (int)s.value;
        octaves = value;
        Regenerate();
    }

    private void Regenerate()
    {
        PerlinNoise.Generate(width / noiseScale, persistence, lacunarity, octaves);
        // float[,] heightMap = GenerateHeightMap(GenerateMountainRange2(5));
        GenerateMapMesh(GenerateMountainRange2(3));
        GenerateMapImage();
    }

    private float[,] GenerateMountainRange2(int noRanges)
    {
        float[,] mountainMask = new float[mapWidthPixels, mapHeightPixels];
        List<Vector2> mountainPoints = new List<Vector2>();
        while (noRanges > 0)
        {
            int maxLength = UnityEngine.Random.Range(2000, 5000);
            int currentLength = 0;
            int maxX = mapWidthPixels;
            int maxY = mapHeightPixels;
            float currentX = UnityEngine.Random.Range(0, maxX);
            float currentY = UnityEngine.Random.Range(0, maxY);
            Vector2 currentDir = new Vector2();
            currentDir.x = UnityEngine.Random.Range(-1.00f, 1.00f);
            currentDir.y = UnityEngine.Random.Range(-1.00f, 1.00f);
            currentDir.Normalize();
            while (currentLength < maxLength)
            {
                if (currentX < 0 || currentX > mapWidthPixels || currentY < 0 || currentY > mapHeightPixels)
                {
                    break;
                }
                mountainPoints.Add(new Vector2(currentX, currentY));
                currentX += currentDir.x;
                currentY += currentDir.y;
                bool changeDir = UnityEngine.Random.Range(0f, 1f) < 0.02f;
                if (changeDir)
                {
                    float theta = UnityEngine.Random.Range(-30f, 30f) * Mathf.Deg2Rad;
                    float newX = currentDir.x * Mathf.Cos(theta) - currentDir.y * Mathf.Sin(theta);
                    float newY = currentDir.x * Mathf.Sin(theta) + currentDir.y * Mathf.Cos(theta);
                    currentDir.x = newX;
                    currentDir.y = newY;
                    currentDir.Normalize();
                }
                ++currentLength;
            }
            --noRanges;
        }
        float maxDistance = Mathf.Max(mapHeightPixels, mapWidthPixels);
        for (int x = 0; x < mapWidthPixels; ++x)
        {
            for (int y = 0; y < mapHeightPixels; ++y)
            {
                Vector2 nearestMountain;
                float nearestMountainDistance = 10000000;
                foreach (Vector2 mountainPoint in mountainPoints)
                {
                    float tempDistance = Vector2.Distance(new Vector2(x, y), mountainPoint);
                    if (tempDistance < nearestMountainDistance)
                    {
                        nearestMountainDistance = tempDistance;
                        nearestMountain = mountainPoint;
                    }
                }
                float weight = 1 - nearestMountainDistance / maxDistance;
                mountainMask[x, y] = weight;
            }
        }
        return mountainMask;
    }

    private float[,] GenerateMountainRange(int noRanges)
    {
        float[,] mountainMask = new float[mapWidthPixels, mapHeightPixels];
        while (noRanges > 0)
        {
            int maxLength = UnityEngine.Random.Range(2000, 5000);
            int currentLength = 0;
            int maxX = mapWidthPixels;
            int maxY = mapHeightPixels;
            float currentX = UnityEngine.Random.Range(0, maxX);
            float currentY = UnityEngine.Random.Range(0, maxY);
            Vector2 currentDir = new Vector2();
            currentDir.x = UnityEngine.Random.Range(-1.00f, 1.00f);
            currentDir.y = UnityEngine.Random.Range(-1.00f, 1.00f);
            currentDir.Normalize();
            while (currentLength < maxLength)
            {
                if (currentX < 0 || currentX > mapWidthPixels || currentY < 0 || currentY > mapHeightPixels)
                {
                    break;
                }
                mountainMask[(int)currentX, (int)currentY] = 1;
                // Vector2 perpendicularDir = new Vector2(currentDir.x, currentDir.y);
                // perpendicularDir.x = currentDir.x * Mathf.Cos(90 * Mathf.Deg2Rad) - currentDir.y * Mathf.Sin(90 * Mathf.Deg2Rad);
                // perpendicularDir.y = currentDir.x * Mathf.Sin(90 * Mathf.Deg2Rad) + currentDir.y * Mathf.Cos(90 * Mathf.Deg2Rad);
                // float perpXPos = currentX, perpYPos = currentY, perpXNeg = currentX, perpYNeg = currentY;
                // float opacity = 1f;
                // for (int i = 0; i < 99; ++i)
                // {
                //     opacity -= 0.01f;
                //     perpXPos += perpendicularDir.x;
                //     perpYPos += perpendicularDir.y;
                //     perpXNeg -= perpendicularDir.x;
                //     perpYNeg -= perpendicularDir.y;
                //     if (perpXPos >= 0 && perpXPos <= mapWidthPixels && perpYPos >= 0 && perpYPos < mapHeightPixels)
                //     {
                //         mountainMask[(int)perpXPos, (int)perpYPos] = opacity;
                //     }
                //     if (perpXNeg >= 0 && perpXNeg <= mapWidthPixels && perpYNeg >= 0 && perpYNeg < mapHeightPixels)
                //     {
                //         mountainMask[(int)perpXNeg, (int)perpYNeg] = opacity;
                //     }
                // }
                currentX += currentDir.x;
                currentY += currentDir.y;
                bool changeDir = UnityEngine.Random.Range(0f, 1f) < 0.02f;
                if (changeDir)
                {
                    float theta = UnityEngine.Random.Range(-30f, 30f) * Mathf.Deg2Rad;
                    float newX = currentDir.x * Mathf.Cos(theta) - currentDir.y * Mathf.Sin(theta);
                    float newY = currentDir.x * Mathf.Sin(theta) + currentDir.y * Mathf.Cos(theta);
                    currentDir.x = newX;
                    currentDir.y = newY;
                    currentDir.Normalize();
                }
                ++currentLength;
            }
            --noRanges;
        }
        return mountainMask;
    }

    private void GenerateMapMesh(float[,] mask)
    {
        int meshComplexity = 4;
        GameObject meshObject = GameObject.Find("MapMesh");
        MeshFilter mf = meshObject.GetComponent<MeshFilter>();
        Mesh m = mf.mesh;
        List<Vector3> vertices = new List<Vector3>();
        List<int> tris = new List<int>();

        int meshWidth = mapWidthPixels / meshComplexity;
        int meshHeight = mapHeightPixels / meshComplexity;

        float scale = 40f / (float)meshWidth;
        meshObject.transform.position = new Vector3(-meshWidth / 2 * scale, -meshHeight / 2 * scale);
        meshObject.transform.localScale = new Vector3(scale, scale, 1);


        //all interior vertices need to exists 4 times
        //all exterior non-corner vertices need to exist 2 times
        //all exterior corner vertices need to exist once
        for (int x = 0; x < meshWidth; ++x)
        {
            for (int y = 0; y < meshHeight; ++y)
            {
                float height = heightMap[x * meshComplexity, y * meshComplexity];// * Mathf.Pow(mask[x * meshComplexity, y * meshComplexity], 8f);
                vertices.Add(new Vector3(x, y, -height));
            }
        }

        for (int x = 0; x < meshWidth - 1; ++x)
        {
            for (int y = 0; y < meshHeight - 1; ++y)
            {
                int v1Tri = y * meshWidth + x;
                int v2Tri = v1Tri + 1;
                int v3Tri = (y + 1) * meshWidth + x;
                int v4Tri = v3Tri + 1;
                tris.Add(v1Tri);
                tris.Add(v2Tri);
                tris.Add(v3Tri);
                tris.Add(v2Tri);
                tris.Add(v4Tri);
                tris.Add(v3Tri);
            }
        }
        m.vertices = vertices.ToArray();
        m.triangles = tris.ToArray();
        m.RecalculateNormals();
    }

    private void ParallelHeightMapLine(int i, int mapHeightPixels)
    {
        float x = i / (float)(width * tileWidth);
        for (int j = 0; j < mapHeightPixels; ++j)
        {
            float y = j / (float)(height * tileWidth);
            float tileHeight = (float)PerlinNoise.GetValue(x + seed, y + seed);
            // tileHeight = ApplySimple(tileHeight, x);
            // tileHeight = ApplyPolarisation(tileHeight);
            // tileHeight = ApplyPlateau(tileHeight);
            // tileHeight = ApplyRadial(tileHeight, i, j, width * tileWidth / 2f, height * tileWidth / 2f, 2f);

            heightMap[i, j] = tileHeight;
        }
    }

    private float[,] GenerateHeightMap(float[,] mask)
    {
        List<Thread> activeThreads = new List<Thread>();

        for (int i = 0; i < mapWidthPixels; ++i)
        {
            // Thread t = new Thread(() => ParallelHeightMapLine(i, mapHeightPixels));
            // activeThreads.Add(t);
            // t.Start();
            float x = i / (float)(width * tileWidth);
            for (int j = 0; j < mapHeightPixels; ++j)
            {
                float y = j / (float)(height * tileWidth);
                float tileHeight = (float)PerlinNoise.GetValue(x + seed, y + seed);
                // tileHeight = ApplySimple(tileHeight, x);
                // tileHeight = ApplyPolarisation(tileHeight);
                // tileHeight = ApplyPlateau(tileHeight);
                // tileHeight = ApplyRadial(tileHeight, i, j, width * tileWidth / 2f, height * tileWidth / 2f, 2f);

                // heightMap[i, j] = (float)Math.Pow(tileHeight, mask[i, j]);
                heightMap[i, j] = tileHeight * Mathf.Pow(mask[i, j], 10f);
                // heightMap[i, j] = mask[i, j];
            }
        }
        // while (activeThreads.Count != 0)
        // {
        //     for (int i = activeThreads.Count - 1; i >= 0; --i)
        //     {
        //         Thread t = activeThreads[i];
        //         if (!t.IsAlive)
        //         {
        //             activeThreads.RemoveAt(i);
        //         }
        //     }
        // }
        return heightMap;
    }

    private void GenerateMapImage()
    {
        mapTexture = new Texture2D(mapWidthPixels, mapHeightPixels);
        GameObject mapTextureObject = new GameObject();
        RectTransform mapTransform = mapTextureObject.AddComponent<RectTransform>();
        mapTextureObject.AddComponent<Image>();
        mapTextureObject.transform.SetParent(mapObject.transform.Find("Canvas"));
        mapTransform.sizeDelta = new Vector2(mapWidthPixels, mapHeightPixels);
        mapTransform.anchoredPosition = new Vector2(0, 0);
        for (int i = 0; i < mapWidthPixels; ++i)
        {
            for (int j = 0; j < mapHeightPixels; ++j)
            {
                float tileHeight = heightMap[i, j];
                mapTexture.SetPixel(i, j, new Color(tileHeight, tileHeight, tileHeight, 1));
            }
        }
        mapTexture.Apply();
        Sprite mapSprite = Sprite.Create(mapTexture, new Rect(0.0f, 0.0f, mapTexture.width, mapTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
        mapTextureObject.GetComponent<Image>().sprite = mapSprite;
    }

    private void GenerateNavigationLayer()
    {
        navigationActive = true;
        int midWidth = width / 2;
        int midHeight = height / 2;
        for (int i = 0; i < width; ++i)
        {
            float x = (i - midWidth) * tileWidth;
            for (int j = 0; j < height; ++j)
            {
                float y = (j - midHeight) * tileWidth;
                GameObject tileObject = Instantiate(tileTemplate, Vector3.zero, mapObject.transform.rotation);
                tileObject.transform.SetParent(mapObject.transform);
                RectTransform tileTransform = tileObject.GetComponent<RectTransform>();
                tileTransform.anchoredPosition = new Vector2(x, y);
                tileTransform.sizeDelta = new Vector2(tileWidth, tileWidth);


                // MapTile.TileType tileType = GetTypeHere(tileHeight);
                MapTile newTile = new MapTile(i, j, MapTile.TileType.NOTHING, tileObject, 0);
                if (x == 0 && y == 0)
                {
                    newTile.Select();
                    selectedTile = newTile;
                }
                mapTiles[i, j] = newTile;
            }
        }
    }

    private float[,] heightMap;
    private int mapWidthPixels, mapHeightPixels;

    public void Awake()
    {
        //frequency, lacunarity, persistence, octaves
        seed = UnityEngine.Random.Range(0f, 1000000f);
        noiseScale = width / 50f * noiseScale;
        PerlinNoise.Generate(width / noiseScale, 2, 2, 6);
        mapTiles = new MapTile[width, height];
        tileWidth = 1920f / maxTilesScreenWidth;
        // tileWidth = 20f;

        // float[,] heightMap = GenerateHeightMap();
        mapWidthPixels = (int)(width * tileWidth);
        mapHeightPixels = (int)(height * tileWidth);
        heightMap = new float[mapWidthPixels, mapHeightPixels];
        for (int i = 0; i < mapWidthPixels; ++i)
        {
            for (int j = 0; j < mapHeightPixels; ++j)
            {
                heightMap[i, j] = 0;
            }
        }
        float[,] mountainMask = GenerateMountainRange2(5);
        heightMap = GenerateHeightMap(mountainMask);
        GenerateMapMesh(heightMap);
        // GenerateMapImage();
        // GenerateNavigationLayer();
    }

    // public List<Tuple<float, float>> CreateMountainMask(int mapWidth, int mapHeight){
    //     float[,] mask = new float[mapWidth, mapHeight];
    //     int xPosition = Random.Range(0, mapWidth);
    //     int yPosition = Random.Range(0, mapHeight);
    //     int maxLength = 500, currentLength = 0;
    //     while(currentLength < maxLength){
    //         int xMove = 0, yMove = 0;
    //         if(Random.Range(0.0f, 1.0f) > 0.5f){
    //             xMove = 1;
    //         } else {
    //             yMove = 1;
    //         }
    //         xPosition += xMove;
    //         yPosition += yMove;
    //         mask[xMove, yMove] = 1;
    //         ++currentLength;
    //     }
    //     return mask;
    // }

    private MapTile selectedTile = null;

    public void Update()
    {
        if (navigationActive)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            int x = selectedTile.x;
            int y = selectedTile.y;
            int xMove = 0, yMove = 0;
            if (horizontal < 0)
            {
                --x;
                xMove = 1;
            }
            else if (horizontal > 0)
            {
                ++x;
                xMove = -1;
            }
            if (vertical < 0)
            {
                --y;
                yMove = 1;
            }
            else if (vertical > 0)
            {
                ++y;
                yMove = -1;
            }
            if (x >= 0 && x < width && y >= 0 && y < height && (xMove != 0 || yMove != 0))
            {
                selectedTile.Deselect();
                selectedTile = mapTiles[x, y];
                selectedTile.Select();
                RectTransform mapRect = mapObject.GetComponent<RectTransform>();
                Vector2 mapPosition = mapRect.anchoredPosition;
                mapPosition.x += xMove * tileWidth;
                mapPosition.y += yMove * tileWidth;
                mapRect.anchoredPosition = mapPosition;
            }
        }
    }

    private float ApplyPolarisation(float noise)
    {
        float polarisation = 3f;
        if (noise < 0.5f)
        {
            noise = Mathf.Pow(noise, polarisation);
            noise *= Mathf.Pow(2f, polarisation - 1f);
        }
        else
        {
            float noiseDiff = 1f - noise;
            noiseDiff = Mathf.Pow(noiseDiff, polarisation);
            noiseDiff *= Mathf.Pow(2f, polarisation - 1f);
            noise = 1f - noiseDiff;
        }
        return noise;
    }

    private float ApplyRadial(float noise, int i, int j, float midWidth, float midHeight, float radialness)
    {
        float distanceWeight = Vector2.Distance(new Vector2(i, j), new Vector2(midWidth, midHeight)) / (float)midWidth;
        noise = Mathf.Pow(distanceWeight, radialness) * noise;
        return noise;
    }

    private float ApplyPlateau(float noise)
    {
        noise *= noise;
        if (noise < 0.7f)
        {
            noise *= 0.5f;
        }
        else if (noise >= 0.7f)
        {
            noise = 0.7f + (float)Mathf.Pow(1 - noise, 2);
        }
        return noise;
    }

    private float ApplySimple(float noise, float x)
    {
        noise = 1 + (Mathf.Sin((x + noise * 50) / 2)) / 2;
        // noise = Mathf.Pow(noise, 2);
        return noise;
    }

    private MapTile.TileType GetTypeHere(float height)
    {
        if (height > 0.5f)
        {
            return MapTile.TileType.WATER;
        }
        return MapTile.TileType.NOTHING;
    }

    private class MapTile
    {
        public enum TileType { NOTHING, WATER, FOOD, FUEL, AMMO, CAMP };
        public int x, y;
        private TileType tileTypeHere;
        private bool discovered = false;
        private Text tileText;
        private Button b;
        private float height;
        private GameObject tileObject;

        public MapTile(int x, int y, TileType tileTypeHere, GameObject g, float height)
        {
            this.x = x;
            this.y = y;
            this.tileTypeHere = tileTypeHere;
            this.tileObject = g;
            // tileText = g.transform.Find("Text").GetComponent<Text>();
            g.GetComponent<Image>().color = new Color(1, 1, 1, 0f);
            // tileText.text = "~";
            this.height = height;
            // b = g.GetComponent<Button>();
            // b.onClick.AddListener(Discover);
            // Discover();
        }

        public void Select()
        {
            tileObject.GetComponent<Image>().color = Color.white;
        }

        public void Deselect()
        {
            tileObject.GetComponent<Image>().color = new Color(1, 1, 1, 0f);
        }

        private string GetLetterHere()
        {
            switch (tileTypeHere)
            {
                case TileType.AMMO:
                    return "A";
                case TileType.CAMP:
                    return "C";
                case TileType.FOOD:
                    return "F";
                case TileType.FUEL:
                    return "P";
                case TileType.NOTHING:
                    return "\"";
                case TileType.WATER:
                    return "W";
                default:
                    return "?";
            }
        }

        public void Discover()
        {
            if (!discovered)
            {
                tileText.text = GetLetterHere();
                discovered = true;
            }
        }
    }
}
