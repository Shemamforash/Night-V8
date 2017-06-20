using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MapGenerator : MonoBehaviour
{
    public GameObject mapObject;
    public GameObject tileTemplate;
    private int width = 100, height = 100;
    private float maxTilesScreenWidth = 75f;
    //bigger = smoother
    private float noiseScale = 100f;
    private float tileWidth;
    private MapTile[,] mapTiles;
    private float seed;
    private Texture2D mapTexture;

    public void Awake()
    {
        //frequency, lacunarity, persistence, octaves
        seed = UnityEngine.Random.Range(0f, 1000000f);
        noiseScale = width / 50f * noiseScale;
        PerlinNoise.Generate(width / noiseScale, 2, 2, 3);
        mapTiles = new MapTile[width, height];
        tileWidth = 1920f / maxTilesScreenWidth;
        int midWidth = width / 2;
        int midHeight = height / 2;

        mapTexture = new Texture2D((int)(width * tileWidth), (int)(height * tileWidth));
        GameObject mapTextureObject = new GameObject();
        RectTransform mapTransform = mapTextureObject.AddComponent<RectTransform>();
        mapTextureObject.AddComponent<Image>();
        mapTextureObject.transform.SetParent(mapObject.transform);
        mapTransform.sizeDelta = new Vector2(tileWidth * width, tileWidth * height);
        mapTransform.anchoredPosition = new Vector2(0, 0);

        for (int i = 0; i < width * tileWidth; ++i)
        {
            float x = i / (float)(width * tileWidth);
            for (int j = 0; j < height * tileWidth; ++j)
            {
                float y = j / (float)(height * tileWidth);
                float tileHeight = (float)PerlinNoise.GetValue(x + seed, y + seed);
                // tileHeight = ApplySimple(tileHeight, x);
                // tileHeight = ApplyPolarisation(tileHeight);
                // tileHeight = ApplyPlateau(tileHeight);
                // tileHeight = ApplyRadial(tileHeight, i, j, width * tileWidth / 2f, height * tileWidth / 2f, 2f);
                float remainder = tileHeight % 0.1f;
                if ((remainder <= 0.01f || remainder >= 0.09f) && (tileHeight > 0.01f && tileHeight < 0.99f))
                {
                    tileHeight = 1;
                }
                else
                {
                    tileHeight = 0;
                }
                mapTexture.SetPixel(i, j, new Color(tileHeight, tileHeight, tileHeight, 1));
            }
        }
        mapTexture.Apply();
        Sprite mapSprite = Sprite.Create(mapTexture, new Rect(0.0f, 0.0f, mapTexture.width, mapTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
        mapTextureObject.GetComponent<Image>().sprite = mapSprite;

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
