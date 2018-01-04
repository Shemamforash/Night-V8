using System;
using System.Collections.Generic;
using Facilitating.MenuNavigation;
using Game.Characters;
using Game.Characters.CharacterActions;
using Game.Combat;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.World.Region
{
    public static class MapGenerator
    {
        private const int MaxGenerationDistance = 5;
        private static string _graphString;
        private const int TargetNodeNumber = 50;
        private static readonly List<Region> _nonFullNodes = new List<Region>();
        private const float RegionChance = 0.5f, CacheChance = 0.6f, CombatChance = 0.8f;

        public static Region Generate()
        {
            int _totalNodes = 0;
            Region initialRegionNode = RegionManager.GenerateNewRegion(null, _totalNodes);
            for(int i = 0; i < Random.Range(3, 5); ++i) _nonFullNodes.Add(initialRegionNode);
            while (_totalNodes < TargetNodeNumber)
            {
                Region originRegion = _nonFullNodes[Random.Range(0, _nonFullNodes.Count)];
                ++_totalNodes;
                Region newRegion = RegionManager.GenerateNewRegion(originRegion, _totalNodes);
                originRegion.AddConnection(newRegion);
                float maxConnections = newRegion.Distance > MaxGenerationDistance ? 0 : 4;
//                float maxConnections = MaxGenerationDistance - newRegion.Distance + 1;
                for (int i = 0; i < maxConnections; ++i) _nonFullNodes.Add(newRegion);
                _nonFullNodes.Remove(originRegion);
                _graphString += GetNodeName(originRegion) + "->" + GetNodeName(newRegion) + "\n";
            }
            Debug.Log(_graphString);
            return initialRegionNode;
        }

        private static string GetNodeName(Region n)
        {
            return "\"Id:" + n.RegionNumber + " PReq:" + n.PerceptionRequirement + "\"";
//                return "\"Id:" + n.NodeNumber + " Type:" + n.NodeType + " PReq:" + n.PerceptionRequirement+"\"";
        }
    }
}