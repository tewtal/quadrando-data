namespace Randomizer.Graph.Combo.SuperMetroid;

using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Text.Json;
using Combo.SuperMetroid.Model;
using static global::Randomizer.Graph.Combo.Zelda.ZeldaYamlReader;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security;

internal class SMJsonReader
{
    public List<Room> Rooms { get; set; } = new();
    public List<ConnectionCollection> Connections { get; set; } = new();
    public TechCollection Techs { get; set; } = new TechCollection([]);
    public HelperCollection Helpers { get; set; } = new HelperCollection([]);
    public List<EnemyCollection> Enemies { get; set; } = new();
    public List<BossScenarioCollection> BossScenarios { get; set; } = new();

    private Dictionary<string, Dictionary<string, object>> vertices = new();
    private Dictionary<Requirement, DirectedUndirectedPair> edges = new();

    private HashSet<(string, string, string)> _edgeExists = new();

    private Dictionary<string, object> CreateNode(Dictionary<string, object> nodeData)
    {
        vertices.Add((string)nodeData["name"], nodeData);
        return nodeData;
    }

    private Dictionary<string, object>? FindNode(string name)
    {
        if (vertices.TryGetValue(name, out var vertexData))
        {
            return vertexData;
        }

        return null;
    }

    private Dictionary<string, object> FindOrCreateNode(string name, string? item = null)
    {
        if (!vertices.TryGetValue(name, out var vertexData))
        {
            vertexData = new Dictionary<string, object>
            {
                { "name", name },
                { "type", VertexType.Meta },
                { "item", item! },
            };
            vertices.Add(name, vertexData);
        }

        return vertexData;
    }

    private bool EdgeExists(Dictionary<string, object> from, Dictionary<string, object> to, Strat strat)
    {
        return _edgeExists.Contains(((string)from["name"], (string)to["name"], strat.Name));
    }

    private void AddEdge(Dictionary<string, object> from, Dictionary<string, object> to, Requirement requirement, bool undirected = false)
    {
        if (!edges.TryGetValue(requirement, out var edgePair))
        {
            edgePair = new DirectedUndirectedPair();
            edges.Add(requirement, edgePair);
        }

        if (undirected)
        {
            edgePair.Undirected.Add([(string)from["name"], (string)to["name"]]);
        }
        else
        {
            edgePair.Directed.Add([(string)from["name"], (string)to["name"]]);
        }

    }

    private void AddDirectedEdge(Dictionary<string, object> from, Dictionary<string, object> to, Requirement requirement)
    {
        AddEdge(from, to, requirement);
    }

    private void AddUndirectedEdge(Dictionary<string, object> from, Dictionary<string, object> to, Requirement requirement)
    {
        AddEdge(from, to, requirement, true);
    }

    private List<T> LoadFiles<T>(string path)
    {
        var jsonData = new List<T>();
        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();

        foreach (var fileName in Directory.GetFiles(path, "*.json", SearchOption.AllDirectories))
        {
            if (fileName.Contains("roomDiagram"))
            {
                continue;
            }

            var fileContents = File.ReadAllText(fileName);
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                var data = JsonSerializer.Deserialize<T>(fileContents, options);
                if (data != null)
                {
                    jsonData.Add(data);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while deserializing: {fileName}, {e.Message} ({e.InnerException?.Source ?? ""}");
            }
        }

        return jsonData;
    }

    private T? LoadFile<T>(string path)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            var data = JsonSerializer.Deserialize<T>(File.ReadAllText(path), options);
            return data ?? default;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while deserializing: {path}, {e.Message} ({e.InnerException?.Source ?? ""}");
            return default;
        }
    }

    public Dictionary<Requirement, DirectedUndirectedPair> GetEdges(World world)
    {
        if (Rooms.Count == 0)
        {
            Load();
        }

        return edges.Select(e => { e.Value.Directed = e.Value.Directed.Select(d => { d[0] = $"SM - {d[0]}"; d[1] = $"SM - {d[1]}"; return d; }).ToList(); e.Value.Undirected = e.Value.Undirected.Select(d => { d[0] = $"SM - {d[0]}"; d[1] = $"SM - {d[1]}"; return d; }).ToList(); return e; }).ToDictionary(e => e.Key, e => e.Value);
        //return new();
    }

    public List<Dictionary<string, object>> GetVertices(World world)
    {
        if (Rooms.Count == 0)
        {
            Load();
        }

        return vertices.Values.Select(v => { v["name"] = $"SM - {v["name"]}"; return v; }).ToList();
    }

    // Load all JSON data into memory
    public void Load()
    {
        var path = Path.Combine(YamlReader.DataRoot, "../Combo/Data/SuperMetroid/sm-json-data/");
        Rooms = LoadFiles<Room>(Path.Combine(path, "region"));
        Connections = LoadFiles<ConnectionCollection>(Path.Combine(path, "connection"));
        Techs = LoadFile<TechCollection>(Path.Combine(path, "tech.json")) ?? new TechCollection([]);
        Helpers = LoadFile<HelperCollection>(Path.Combine(path, "helpers.json")) ?? new HelperCollection([]);
        Enemies = LoadFiles<EnemyCollection>(Path.Combine(path, "enemies")).Where(x => x.Enemies != null).ToList();
        BossScenarios = LoadFiles<BossScenarioCollection>(Path.Combine(path, "enemies")).Where(x => x.Scenarios != null).ToList();
    }

    public void BuildGraph(World world)
    {
        // Build all the rooms
        foreach(var room in Rooms)
        {
            BuildRoom(room);
        }
        
        // Connect all the connections
        foreach (var connection in Connections.SelectMany(c => c.Connections))
        {
            BuildConnection(connection);
        }

    }

    private void BuildConnection(Connection connection)
    { 
        var firstConnectionNode = connection.Nodes[0];
        var secondConnectionNode = connection.Nodes[1];

        var firstRoom = Rooms.Find(r => r.Id == firstConnectionNode.RoomId);
        var secondRoom = Rooms.Find(r => r.Id == secondConnectionNode.RoomId);

        if (firstRoom == null || secondRoom == null)
        {
            Console.WriteLine($"Error: Could not find rooms for connection {firstConnectionNode.Area} - {firstConnectionNode.RoomName} and {secondConnectionNode.Area} - {secondConnectionNode.RoomName}");
            return;
        }

        var firstNode = firstRoom.Nodes.First(n => n.Id == firstConnectionNode.NodeId);
        var secondNode = secondRoom.Nodes.First(n => n.Id == secondConnectionNode.NodeId);

        if (firstNode == null || secondNode == null)
        {
            Console.WriteLine($"Error: Could not find nodes for connection {firstConnectionNode.Area} - {firstConnectionNode.RoomName} - {firstConnectionNode.NodeName} and {secondConnectionNode.Area} - {secondConnectionNode.RoomName} - {secondConnectionNode.NodeName}");
            return;
        }

        // Connect first to second
        var firstNodeData = FindNode($"{firstRoom.Area} - {firstRoom.Name} - {firstNode.Name} - Out")!;
        var secondNodeData = FindNode($"{secondRoom.Area} - {secondRoom.Name} - {secondNode.Name} - In")!;
        AddDirectedEdge(firstNodeData, secondNodeData, new Requirement.Always());
        //Console.WriteLine($"Adding edge from {firstNodeData["name"]} to {secondNodeData["name"]}");

        if (connection.Direction.ToLower() == "bidirectional")
        {
            // Connect second to first
            var firstNodeRevData = FindNode($"{secondRoom.Area} - {secondRoom.Name} - {secondNode.Name} - Out")!;
            var secondNodeRevData = FindNode($"{firstRoom.Area} - {firstRoom.Name} - {firstNode.Name} - In")!;
            AddDirectedEdge(firstNodeRevData, secondNodeRevData, new Requirement.Always());
            //Console.WriteLine($"Adding reverse edge from {firstNodeRevData["name"]} to {secondNodeRevData["name"]}");
        }
        
    }

    private void BuildRoom(Room room)
    {
        var roomVertices = new List<Dictionary<string, object>>();

        // For each obstacle in the room, created a copy of a node for that obstacle state (including a blank state)
        var obstacleCombinations = room.Obstacles?.Combinations().ToList() ?? [];
        string[] obstacleIdStrings = ["", ..obstacleCombinations.Select(c => string.Join(",", c.Select(o => o.Id).OrderBy(c => c))).OrderBy(c => c).ToList()];

        foreach (var obstacleIdString in obstacleIdStrings)
        {
            // Create this rooms nodes
            foreach (var node in room.Nodes)
            {
                var nodeName = obstacleIdString == "" ? $"{room.Area} - {room.Name} - {node.Name}" : $"{room.Area} - {room.Name} - {node.Name} - {obstacleIdString}";

                var nodeType = node.NodeType switch
                {
                    // "door", "entrance", "exit", "event", "item", "junction", "utility"
                    "item" => VertexType.Item,
                    "entrance" => VertexType.Entrance,
                    "exit" => VertexType.Entrance,
                    "door" => VertexType.Entrance,
                    _ => VertexType.Meta
                };

                VertexType? nodeSubType = node.NodeSubType switch
                {
                    "chozo" => VertexType.Chozo,
                    "hidden" => VertexType.Hidden,
                    "visible" => VertexType.Visible,
                    _ => null
                };

                var newNode = CreateNode(new()
                {
                    { "name", nodeName },
                    { "type", VertexType.Meta },
                    { "subtype", VertexType.Meta },
                });
                
                roomVertices.Add(newNode);

                // If this node has a lock on it, we need to resolve the lock states
                bool hasLocks = node.Locks != null && node.Locks.Count() > 0;
                var lockClearedNodeName = $"{room.Area} - {room.Name} - {node.Name} - Lock Cleared";
                Dictionary<string, object>? lockClearedNode = null;

                if (node.Locks != null)
                {
                    lockClearedNode = FindNode(lockClearedNodeName) ?? CreateNode(new()
                        {
                            { "name", lockClearedNodeName },
                            { "type", VertexType.Meta },
                            { "subtype", VertexType.Meta },
                        });

                    foreach (var nodeLock in node.Locks)
                    {  
                        if (nodeLock.Lock != null && nodeLock.LockType.ToLower() == "escapefunnel")
                        {
                            AddDirectedEdge(newNode, lockClearedNode, new Requirement.Always());
                            continue;
                        }

                        foreach (var lockStrat in nodeLock.UnlockStrats)
                        {
                            var lockStratNodeName = $"{room.Area} - {room.Name} - {node.Name} - Lock Strat: {lockStrat.Name}";
                            var lockStratNode = FindNode(lockStratNodeName) ?? CreateNode(new()
                            {
                                { "name", lockStratNodeName },
                                { "type", VertexType.Meta },
                                { "subtype", VertexType.Meta },
                            });

                            var lockRequirement = lockStrat.Requires ?? new Requirement.Always();
                            if (nodeLock.Lock != null)
                            {
                                lockRequirement = new Requirement.And([lockRequirement, nodeLock.Lock]);
                            }

                            lockRequirement = lockRequirement.ModifyObstacleState(obstacleIdString.Split(","));

                            AddDirectedEdge(newNode, lockStratNode, lockRequirement);
                            AddDirectedEdge(lockStratNode, lockClearedNode, new Requirement.Always());
                        }

                        foreach(var lockYields in nodeLock.Yields ?? [])
                        {
                            var lockYieldsNodeName = $"{room.Area} - {room.Name} - {node.Name} - Lock Yields: {lockYields}";
                            var lockYieldsNode = FindNode(lockYieldsNodeName) ?? CreateNode(new()
                            {
                                { "name", lockYieldsNodeName },
                                { "type", VertexType.Meta },
                                { "subtype", VertexType.Meta },
                                { "item", lockYields },
                            });

                            AddDirectedEdge(lockClearedNode, lockYieldsNode, new Requirement.Always());
                        }
                    }
                }

                // If this node is an item, create a new item node that is common for all obstacle states
                if (nodeType == VertexType.Item)
                {
                    var itemNodeName = $"{room.Area} - {room.Name} - {node.Name} - Item";
                    var itemNode = FindNode(itemNodeName) ?? CreateNode(new()
                        {
                            { "name", itemNodeName },
                            { "type", VertexType.Item },
                            { "subtype", nodeSubType! },
                            { "address", Convert.ToInt32(node.NodeAddress ?? "0", 16) },
                            { "itemset", (string[])["supermetroid"] },
                        });

                    if (hasLocks)
                    {
                        AddDirectedEdge(lockClearedNode!, itemNode, new Requirement.Always());
                    }
                    else
                    {
                        AddDirectedEdge(newNode, itemNode, new Requirement.Always());
                    }

                    if (obstacleIdString == "")
                    { 
                        // This is required for backtracking
                        AddDirectedEdge(itemNode, newNode, new Requirement.Single("BacktrackSearch"));
                    }
                 
                }

                // If this node yields flags, add a flag node and connection to it
                if (node.Yields != null)
                {
                    foreach (var yield in node.Yields)
                    {
                        var yieldNodeName = $"{room.Area} - {room.Name} - {node.Name} - Yields: {yield}";
                        var yieldNode = FindNode(yieldNodeName) ?? CreateNode(new()
                            {
                                { "name", yieldNodeName },
                                { "type", VertexType.Meta },
                                { "subtype", VertexType.Meta },
                                { "item", yield },
                            });

                        if (hasLocks)
                        {
                            AddDirectedEdge(lockClearedNode!, yieldNode, new Requirement.Always());
                        }
                        else
                        {
                            AddDirectedEdge(newNode, yieldNode, new Requirement.Always());
                        }
                    }
                }
            }
        }

        foreach (var node in room.Nodes)
        {
            ConnectNode(room, node, "");
        }

        // Scan through all the vertices we created and purge the ones that got no edges connected to it
        foreach (var vertex in roomVertices.ToList())
        {
            if (!edges.Values.Any(e => e.Directed.Any(d => d[0] == (string)vertex["name"] || d[1] == (string)vertex["name"])))
            {
                //Console.WriteLine($"Removing unconnected vertex: {vertex["name"]}");
                vertices.Remove((string)vertex["name"]);
                roomVertices.Remove(vertex);
            }
        }

        // Create entrance/exit nodes for the room
        foreach (var node in room.Nodes.Where(n => n.NodeType == "door" || n.NodeType == "entrance" || n.NodeType == "exit" || n.NodeSubType == "ship"))
        {
            // Create In and Out nodes for this entrance/exit
            var inNodeName = $"{room.Area} - {room.Name} - {node.Name} - In";
            var inNode = CreateNode(new()
            {
                { "name", inNodeName },
                { "type", VertexType.Entrance },
                { "subtype", VertexType.Meta },
            });

            var outNodeName = $"{room.Area} - {room.Name} - {node.Name} - Out";
            var outNode = CreateNode(new()
            {
                { "name", outNodeName },
                { "type", VertexType.Entrance },
                { "subtype", VertexType.Meta },
            });

            // Find the node in a non-obstacle state
            var currentNode = FindNode($"{room.Area} - {room.Name} - {node.Name}")!;
            if (currentNode == null)
            {
                Console.WriteLine($"Error: Could not find node for {room.Area} - {room.Name} - {node.Name}");
                continue;
            }

            // Create a undirected edge from the door node to the current node
            AddUndirectedEdge(inNode, currentNode, new Requirement.Always());
            //Console.WriteLine($"Adding incoming edge from {inNodeName} to {currentNode["name"]}");

            if (node.Locks != null)
            {
                currentNode = FindNode($"{room.Area} - {room.Name} - {node.Name} - Lock Cleared")!;
                if (currentNode == null)
                {
                    Console.WriteLine($"Error: Could not find lock cleared node for {room.Area} - {room.Name} - {node.Name}");
                    continue;
                }
            }


            // Create outgoing edge for the normal obstacle state
            //Console.WriteLine($"Adding outgoing edge from {currentNode["name"]} to {outNodeName}");
            AddDirectedEdge(currentNode, outNode, new Requirement.Always());

            if (node.Locks == null)
            {
                // Create directed edges from the other existing obstacle states for this node to the door node
                foreach (var obstacleNode in roomVertices.Where(v => v["name"].ToString()!.StartsWith($"{room.Area} - {room.Name} - {node.Name} - ")))
                {
                    //Console.WriteLine($"Adding outgoing edge from {obstacleNode["name"]} to {outNodeName}");
                    AddDirectedEdge(obstacleNode, outNode, new Requirement.Always());
                }
            }

        }
    }

    // This will recursively connect all the nodes in the room, switching obstacle states as needed until the full room is connected starting from the obstacle-free state
    public void ConnectNode(Room room, Node from, string obstacleState)
    {
        var fromNodeName = obstacleState == "" ? $"{room.Area} - {room.Name} - {from.Name}" : $"{room.Area} - {room.Name} - {from.Name} - {obstacleState}";
        var fromNodeData = FindNode(fromNodeName)!;

        // Get all the nodes that are connected from this one (all edges are directed)
        var links = room.Links.Where(l => l.From == from.Id).ToList();

        foreach (var link in links)
        {
            foreach (var linkTo in link.To)
            {
                var toNode = room.Nodes.Where(n => n.Id == linkTo.Id).FirstOrDefault()!;
                var linkStrats = room.Strats.Where(s => s.Link[0] == link.From && s.Link[1] == linkTo.Id).ToList();

                foreach (var strat in linkStrats)
                {
                    // TODO: This should be mostly safe in a vanilla world to skip for now, but this needs proper handling
                    if (strat.EntranceCondition != null)
                    {
                        if (strat.EntranceCondition is not EntranceCondition.ComeInNormally &&
                            strat.EntranceCondition is not EntranceCondition.ComeInRunning &&
                            strat.EntranceCondition is not EntranceCondition.ComeInJumping
                            )
                        {
                            // Let's not deal with any entrance conditions for now
                            continue;
                        }
                    }

                    // Figure out the target obstacle state after executing this strat
                    string newObstacleState = obstacleState;
                    
                    if (strat.ClearsObstacles != null)
                    {
                        string[] currentObstacles = obstacleState.Split(",");
                        newObstacleState = string.Join(",", currentObstacles.Union(strat.ClearsObstacles).OrderBy(c => c)).Trim(',');
                    }

                    if (strat.ResetsObstacles != null)
                    {
                        string[] currentObstacles = newObstacleState.Split(",");
                        newObstacleState = string.Join(",", currentObstacles.Except(strat.ResetsObstacles).OrderBy(c => c)).Trim(',');
                    }

                    var stratNodeName = $"{fromNodeName} - {string.Join(",", strat.Link)} - Strat: {strat.Name}";
                    var stratNode = FindOrCreateNode(stratNodeName);

                    var toNodeName = newObstacleState == "" ? $"{room.Area} - {room.Name} - {toNode.Name}" : $"{room.Area} - {room.Name} - {toNode.Name} - {newObstacleState}";
                    var toNodeData = FindNode(toNodeName)!;

                    var requirement = strat.Requires == new Requirement.And([]) ? new Requirement.Always() : strat.Requires;

                    // Adjust the requirement based on the previous obstacle state, turning the obstacle check into a static always/never check
                    requirement = requirement.ModifyObstacleState(obstacleState.Split(","));

                    if (!EdgeExists(fromNodeData, stratNode, strat))
                    {
                        _edgeExists.Add((fromNodeName, stratNodeName, strat.Name));
                        AddDirectedEdge(fromNodeData, stratNode, requirement);
                        AddDirectedEdge(stratNode, toNodeData, new Requirement.Always());
                        //Console.WriteLine($"Added edge from {fromNodeName} to {stratNodeName} with requirement {requirement}");
                        ConnectNode(room, toNode, newObstacleState);
                    }

                }
            }
        }
    }
}
