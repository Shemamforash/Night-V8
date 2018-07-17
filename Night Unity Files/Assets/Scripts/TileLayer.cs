using System.Collections.Generic;
using Game.Combat.Generation;
using UnityEngine;

public class TileLayer : MonoBehaviour
{
    private static GameObject _heavyTile, _heavyMediumTile, _mediumTile, _mediumLightTile, _lightTile;
    private static Transform _tileParent;

    public void Awake()
    {
        _tileParent = GameObject.Find("Tiles").transform;
        if (_heavyTile != null) return;
        _heavyTile = Resources.Load<GameObject>("Prefabs/Floor Tiles/Heavy");
        _heavyMediumTile = Resources.Load<GameObject>("Prefabs/Floor Tiles/HeavyMedium");
        _mediumTile = Resources.Load<GameObject>("Prefabs/Floor Tiles/Medium");
        _mediumLightTile = Resources.Load<GameObject>("Prefabs/Floor Tiles/MediumLight");
        _lightTile = Resources.Load<GameObject>("Prefabs/Floor Tiles/Light");
    }

    private class Tile
    {
        public Tile Left, Right, Up, Down;
        private readonly int Size;
        private readonly Vector2Int _position;

        public Tile(Vector2Int position, int size)
        {
            _position = position;
            Size = size;
        }

        public void CreateObject()
        {
            GameObject tileObject = null;
            int newSize = Size + Random.Range(-1, 2);
            if (newSize > 5) newSize = 5;
            switch (newSize)
            {
                case 5:
                    tileObject = Instantiate(_heavyTile);
                    break;
                case 4:
                    tileObject = Instantiate(_heavyMediumTile);
                    break;
                case 3:
                    tileObject = Instantiate(_mediumTile);
                    break;
                case 2:
                    tileObject = Instantiate(_mediumLightTile);
                    break;
                case 1:
                    tileObject = Instantiate(_lightTile);
                    break;
            }

            if (tileObject == null) return;
            tileObject.transform.SetParent(_tileParent, false);
            Vector3 position = _position + Vector2.one * Random.Range(-0.1f, 0.1f);
            position.z = 5;
            tileObject.transform.position = position;
            tileObject.transform.Rotate(0, 0, Random.Range(0,360));
            tileObject.transform.localScale = Vector3.one * Random.Range(0.9f, 1.1f) * 1.5f;
        }
    }

    private readonly Dictionary<Vector2Int, Tile> _existingTiles = new Dictionary<Vector2Int, Tile>();

    private void GrowTiles(Vector2Int startPosition, int radius)
    {
        for (int x = -radius; x <= radius; ++x)
        {
            for (int y = -radius; y <= radius; ++y)
            {
                float distance = radius - Mathf.Sqrt(x * x + y * y);
                int size = Mathf.FloorToInt(distance / radius * 5);
                if (size <= 0) continue;
                Vector2Int tilePosition = new Vector2Int(x,y) + startPosition;
                Tile newTile = new Tile(tilePosition, size);
                if (_existingTiles.ContainsKey(tilePosition))
                {
                    if (Random.Range(0, 2) == 0)
                    {
                        _existingTiles[tilePosition] = newTile;
                        return;
                    }
                }

                _existingTiles[tilePosition] = newTile;
            }
        }
    }
    
    public void Start()
    {
        //todo me
        return;
        int width = PathingGrid.CombatAreaWidth / 4;
        for (int i = 0; i < Random.Range(2, 4); ++i)
        {
            int randomX = Random.Range(-width, width);
            int randomY = Random.Range(-width, width);
            GrowTiles(new Vector2Int(randomX, randomY), Random.Range(3, 6));
        }

        foreach (Tile tile in _existingTiles.Values)
            tile.CreateObject();
    }
}