using System;
using System.Collections.Generic;
using Facilitating.MenuNavigation;
using Game.Characters;
using Game.Characters.CharacterActions;
using Game.Combat;
using Random = UnityEngine.Random;

namespace Game.World.Region
{
    public static class MapGenerator
    {
        private const int MaxGenerationDistance = 5;
        private static string _graphString;
        private const int TargetNodeNumber = 25;
        private static readonly List<Node> _nonFullNodes = new List<Node>();
        private const float RegionChance = 0.5f, CacheChance = 0.6f, CombatChance = 0.8f;

        public static Node Generate()
        {
            int _totalNodes = 0;
            Node initialNode = new Node(0);
            while (_totalNodes < TargetNodeNumber)
            {
                Node randomNode = _nonFullNodes[Random.Range(0, _nonFullNodes.Count)];
                ++_totalNodes;
                randomNode.AddEdge();
                _nonFullNodes.Remove(randomNode);
            }
            return initialNode;
        }

        public class Node
        {
            private readonly Action<Player> _enterNodeAction;
            public readonly List<Edge> Connections = new List<Edge>();
            private readonly Edge _connector;
            public readonly float TotalDistance;
            public string NodeType;
            private Region _regionHere;

            public Node(float totalDistance, Edge connector = null)
            {
                _connector = connector;
                TotalDistance = totalDistance;
                float maxConnections = MaxGenerationDistance - TotalDistance;
                for (int i = 0; i < maxConnections; ++i) _nonFullNodes.Add(this);
                NodeType = GenerateType();
                _enterNodeAction = player =>
                {
                    Popup p = new Popup("Reached Node " + NodeType);
                    foreach (Edge e in Connections)
                    {
                        p.AddButton("Explore " + e.Target.NodeType,
                            () => player.StartExploration(() => e.Target.Discover(player), 1), true, true);
                    }
                    if (_regionHere != null)
                    {
                        if (_regionHere.GetCombatScenario() == null)
                        {
                            p.AddButton("Look around " + _regionHere.Name, () =>
                                {
                                    ((CollectResources) player.States.GetState("Collect Resources")).SetTargetRegion(_regionHere);
                                    player.States.NavigateToState("Collect Resources");
                                    RegionManager.DiscoverRegion(_regionHere);
                                }
                                , true, true);
                        }
                        else
                        {
                            p.AddButton("Fight at " + _regionHere.Name, () =>
                            {
                                CombatManager.EnterCombat(player, _regionHere.GetCombatScenario());
                            }, true, true);
                        }
                    }
                    p.AddButton("Return", () => { player.States.NavigateToState(nameof(Return)); }, true, true);
                };
            }

            public void Discover(Player p)
            {
                _enterNodeAction(p);
                if (HasNext()) return;
                Delete();
            }

            private bool HasNext()
            {
                return Connections.Count != 0;
            }

            private void Delete()
            {
                _connector.Origin?.RemoveConnection(_connector);
            }

            private void RemoveConnection(Edge connection)
            {
                Connections.Remove(connection);
                if (Connections.Count == 0) Delete();
            }

            private string GenerateType()
            {
                if (TotalDistance != 0)
                {
                    float rand = Random.Range(0f, 1f);
                    if (rand < RegionChance)
                    {
                        _regionHere = RegionManager.GenerateNewRegion();
                        return "Region";
                    }
                    if (rand < CacheChance)
                        return "Cache";
                    if (rand < CombatChance)
                    {
                        _regionHere = RegionManager.GenerateNewRegion();
                        _regionHere.GenerateCombatScenario();
                        return "Combat";
                    }
                }
                return "Nothing";
            }

            public void AddEdge()
            {
                Edge e = new Edge(this);
                Connections.Add(e);
            }
        }

        public class Edge
        {
            public readonly Node Target;
            public readonly Node Origin;
            public string ChoiceString;

            public Edge(Node origin)
            {
                Origin = origin;
                Target = new Node(1 + Origin.TotalDistance, this);
                _graphString += Origin.NodeType + "->" + Target.NodeType + "\n";
            }
        }
    }
}