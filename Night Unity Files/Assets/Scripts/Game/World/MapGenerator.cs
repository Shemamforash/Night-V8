using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    public GameObject mapObject;
    public GameObject tileTemplate;
    private int width = 10, height = 10;
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
        GenerateMapMesh(GenerateMountainRange(3));
        // GenerateMapImage();
    }

    private void CreateMountainLine(Vector2 currentPosition, HashSet<Vector2> mountainPoints, int targetLength, Vector2 currentDir, int lineLevel)
    {
        int currentLength = 0;
        currentDir = OffsetDirectionInRange(currentDir, 5f);
        float dirChangeProbability = 0.02f / Mathf.Pow(0.25f, lineLevel);
        float branchProbability = 0.25f;
        int dirChangeDirection = 1;
        while (currentLength < targetLength)
        {
            float x = currentPosition.x;
            float y = currentPosition.y;
            x += currentDir.x;
            y += currentDir.y;
            currentPosition.x = x;
            currentPosition.y = y;
            if ((int)x < 0 || (int)x >= mapWidthPixels || (int)y < 0 || (int)y >= mapHeightPixels)
            {
                break;
            }
            ++currentLength;
            mountainPoints.Add(currentPosition);
            float rand = Random.Range(0.00f, 1.00f);
            if (rand < dirChangeProbability)
            {
                currentDir = OffsetDirectionInRange(currentDir, 0, 15 * dirChangeDirection);
                dirChangeDirection = -dirChangeDirection;
                rand = Random.Range(0.0f, 1.0f);
                if (rand < branchProbability && lineLevel < 3)
                {
                    Vector2 perpendicular = OffsetDirection(currentDir, 90f);
                    rand = Random.Range(0.0f, 1.0f);
                    int targetLengthMin = targetLength / 8;
                    int targetLengthMax = targetLength / 4;
                    if (rand > 0.66f)
                    {
                        // CreateMountainLine(currentPosition, mountainPoints, Random.Range(targetLengthMin, targetLengthMax), perpendicular, lineLevel + 1);
                    }
                    else if (rand < 0.33)
                    {
                        // CreateMountainLine(currentPosition, mountainPoints, Random.Range(targetLengthMin, targetLengthMax), -perpendicular, lineLevel + 1);
                    }
                    else
                    {
                        // CreateMountainLine(currentPosition, mountainPoints, Random.Range(targetLengthMin, targetLengthMax), perpendicular, lineLevel + 1);
                        // CreateMountainLine(currentPosition, mountainPoints, Random.Range(targetLengthMin, targetLengthMax), -perpendicular, lineLevel + 1);
                    }
                }
            }
        }
    }

    private Vector2 OffsetDirectionInRange(Vector2 direction, float offsetAmount)
    {
        return OffsetDirectionInRange(direction, -offsetAmount, offsetAmount);
    }

    private Vector2 OffsetDirectionInRange(Vector2 direction, float offsetMin, float offsetMax)
    {
        float theta = Random.Range(offsetMin, offsetMax);
        if (theta < 0f)
        {
            theta = 360f + theta;
        }
        return OffsetDirection(direction, theta);
    }

    private Vector2 OffsetDirection(Vector2 direction, float theta)
    {
        bool negativeTheta = theta < 0;
        if (negativeTheta)
        {
            theta = -theta;
        }
        theta *= Mathf.Deg2Rad;
        float xCos = direction.x * Mathf.Cos(theta);
        float xSin = direction.x * Mathf.Sin(theta);
        float yCos = direction.y * Mathf.Cos(theta);
        float ySin = direction.y * Mathf.Sin(theta);
        if (negativeTheta)
        {
            xSin = -xSin;
        }
        else
        {
            ySin = -ySin;
        }
        direction.x = xCos + ySin;
        direction.y = xSin + yCos;
        return direction;
    }

    private float[,] GenerateMountainRange(int noRanges)
    {
        float[,] mountainMask = new float[mapWidthPixels, mapHeightPixels];
        HashSet<Vector2> mountainPoints = new HashSet<Vector2>();
        while (noRanges > 0)
        {
            int maxLength = UnityEngine.Random.Range(2000, 5000);
            // float currentX = UnityEngine.Random.Range(0, mapWidthPixels);
            // float currentY = UnityEngine.Random.Range(0, mapHeightPixels);
            float currentX = mapWidthPixels / 2;
            float currentY = mapHeightPixels / 2;
            Vector2 currentPosition = new Vector2(currentX, currentY);
            Vector2 currentDir = new Vector2();
            currentDir.x = UnityEngine.Random.Range(-1.00f, 1.00f);
            currentDir.y = UnityEngine.Random.Range(-1.00f, 1.00f);
            currentDir.Normalize();
            CreateMountainLine(currentPosition, mountainPoints, maxLength, currentDir, 0);
            --noRanges;
        }

        float maxDistance = Mathf.Max(mapHeightPixels, mapWidthPixels) / 2;
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
                if (weight < 0)
                {
                    weight = 0;
                }
                mountainMask[x, y] = weight;
                // mountainMask[x, y] = 0;
            }
        }
        // foreach (Vector2 mountainPoint in mountainPoints)
        // {
        //     mountainMask[(int)mountainPoint.x, (int)mountainPoint.y] = 1;
        // }
        return mountainMask;
    }

    private void GenerateMapMesh(float[,] mask)
    {
        int meshComplexity = 2;
        GameObject meshObject = GameObject.Find("MapMesh");
        MeshFilter mf = meshObject.GetComponent<MeshFilter>();
        Mesh m = mf.mesh;
        List<Vector3> vertices = new List<Vector3>();
        List<int> tris = new List<int>();

        int meshWidth = mapWidthPixels / meshComplexity;
        int meshHeight = mapHeightPixels / meshComplexity;

        float scale = 40f / ((float)meshWidth * 3);
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

    private float[,] GenerateHeightMap(float[,] mask)
    {
        for (int i = 0; i < mapWidthPixels; ++i)
        {
            float x = i / (float)(width * tileWidth);
            for (int j = 0; j < mapHeightPixels; ++j)
            {
                float y = j / (float)(height * tileWidth);
                float tileHeight = (float)PerlinNoise.GetValue(x + seed, y + seed, true);
                // tileHeight = ApplySimple(tileHeight, x);
                // tileHeight = ApplyPolarisation(tileHeight);
                // tileHeight = ApplyPlateau(tileHeight);
                // tileHeight = ApplyRadial(tileHeight, i, j, width * tileWidth / 2f, height * tileWidth / 2f, 2f);

                heightMap[i, j] = tileHeight;
                // tileHeight = (tileHeight - 0.5f) / 0.5f;
                // heightMap[i, j] = tileHeight * Mathf.Pow(mask[i, j], 10f);
            }
        }
        return heightMap;
    }

    private void GenerateMapImage(float[,] mask)
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
                float tileHeight = mask[i, j];
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

    // public void Awake()
    // {
    //     //frequency, lacunarity, persistence, octaves
    //     seed = UnityEngine.Random.Range(0f, 1000000f);
    //     noiseScale = width / 50f * noiseScale;
    //     PerlinNoise.Generate(width / noiseScale, 1f, 1f, 3);
    //     mapTiles = new MapTile[width, height];
    //     tileWidth = 1920f / maxTilesScreenWidth;
    //     // tileWidth = 20f;

    //     // float[,] heightMap = GenerateHeightMap();
    //     mapWidthPixels = (int)(width * tileWidth);
    //     mapHeightPixels = (int)(height * tileWidth);
    //     heightMap = new float[mapWidthPixels, mapHeightPixels];
    //     for (int i = 0; i < mapWidthPixels; ++i)
    //     {
    //         for (int j = 0; j < mapHeightPixels; ++j)
    //         {
    //             heightMap[i, j] = 0;
    //         }
    //     }
    //     float[,] mountainMask = GenerateMountainRange(1);
    //     heightMap = GenerateHeightMap(mountainMask);
    //     GenerateMapMesh(heightMap);
    //     // GenerateMapMesh(mountainMask);
    //     // GenerateMapImage(mountainMask);
    //     // GenerateNavigationLayer();
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
