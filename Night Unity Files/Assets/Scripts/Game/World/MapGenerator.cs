using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    public GameObject mapObject;
    public GameObject tileTemplate;
    private int width = 50, height = 50;
    private float maxTilesScreenWidth = 50f;
    private float noiseScale = 5f;
    private float tileWidth;
    private MapTile[,] mapTiles;

    public void Awake()
    {
        mapTiles = new MapTile[width, height];
        tileWidth = 1920f / maxTilesScreenWidth;
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

                float normalisedX = (x + (midWidth * tileWidth)) / (width * tileWidth) * noiseScale;
                float normalisedY = (y + (midHeight * tileWidth)) / (height * tileWidth) * noiseScale;
                float tileHeight = GenerateSimpleEnvironment(normalisedX, normalisedY);
                tileHeight = Mathf.Sin(tileHeight);
                tileHeight = Mathf.Pow(tileHeight, 2);
                MapTile.TileType tileType = GetTypeHere(tileHeight);

                MapTile newTile = new MapTile(i, j, tileType, tileObject, tileHeight);
                mapTiles[i, j] = newTile;
            }
        }
    }

    private float GenerateSimpleEnvironment(float x, float y)
    {
        float noise = Mathf.PerlinNoise(x, y);
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
        private int x, y;
        private TileType tileTypeHere;
        private bool discovered = false;
        private Text tileText;
        private Button b;
        private float height;

        public MapTile(int x, int y, TileType tileTypeHere, GameObject g, float height)
        {
            this.x = x;
            this.y = y;
            this.tileTypeHere = tileTypeHere;
            tileText = g.transform.Find("Text").GetComponent<Text>();
            g.GetComponent<Image>().color = new Color(1, 1, 1, height);
            tileText.text = "~";
            this.height = height;
            // b = g.GetComponent<Button>();
            // b.onClick.AddListener(Discover);
            Discover();
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
