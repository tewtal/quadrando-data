namespace Randomizer.Graph.Combo.Zelda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using ItemSet = Dictionary<ItemSetName, /* WeightedSet */ Dictionary<int, List<Item>>>;
using WeightedSet = Dictionary<int, List<Item>>;
using PooledItem = (ItemSetName Set, int Weight, Item Item);

internal class ZeldaData
{
    public ZeldaYamlReader.YamlData? Data { get; set; } = null;
}

internal class ZeldaWorld
{
    // Adjusts the world as needed to randomize Metroid
    public static void AdjustWorld(World world, PRNG prng)
    {
        // Load the Metroid Yaml Data and hook up the world to the current world graph
        var yamlReader = new ZeldaYamlReader();
        yamlReader.Load();
        world.ZeldaData.Data = yamlReader.Data;

        if (world.Config.Z1EntranceShuffle == Z1EntranceShuffleOption.Overworld)
        {
            var entranceShuffler = new ZeldaEntranceShuffler(world, prng);
            entranceShuffler.Shuffle();
        }

        yamlReader.BuildGraph();

        var zeldaVertices = yamlReader.LoadYmlData(world);

        foreach (var vtx in zeldaVertices)
        {
            var name = vtx.TryGetValue("name", out object? nameValue) ? (string)nameValue : throw new InvalidDataException("Zelda vertex without a name");
            var type = vtx.TryGetValue("type", out object? typeValue) ? (VertexType)typeValue : VertexType.Meta;
            var subtype = vtx.TryGetValue("subtype", out object? subtypeValue) ? (VertexType?)subtypeValue : (type == VertexType.Item ? VertexType.Standing : null);
            var item = vtx.TryGetValue("item", out object? itemValue) ? (string)itemValue : null;
            var itemset = vtx.TryGetValue("itemset", out object? itemsetValue) ? (string[])itemsetValue : null;
            var address = vtx.TryGetValue("address", out object? addressValue) ? (int?)addressValue : null;

            var vertex = new Vertex()
            {
                World = world,
                Name = name,
                Type = type,
                SubType = subtype,
                Item = item != null ? world.GetItem("Z1" + item, Game.Zelda) : null,
                ItemSet = itemset?.Select(i => new ItemSetName(i, world)).ToArray() ?? [],
                Addresses = address != null ? [(long)address.Value] : null,
                Game = Game.Zelda
            };

            world.Graph.AddVertex(vertex);
            //Console.WriteLine($"Added vertex {vertex.Name}");
        }

        var zeldaEdges = yamlReader.GetForWorld(world);
        foreach (var edgeCollection in zeldaEdges)
        {
            var edgeCollectionData = edgeCollection.Key.Split(":").First().Split('|');
            var requirementName = edgeCollectionData.First();
            if(!requirementName.StartsWith("fixed"))
            {
                requirementName = "Z1" + requirementName;
            }
            var requirement = world.GetItem(requirementName, Game.Zelda);
            var requirementCount = int.Parse(edgeCollectionData.Skip(1).FirstOrDefault() ?? "1");

            foreach (var edges in edgeCollection.Value.Directed)
            {
                var from = world.GetLocation(edges[0]);
                var to = world.GetLocation(edges[1]);
                if (from is null || to is null)
                {
                    throw new Exception("Name Connection Mismatch: " + $"({edges[0]}, {edges[1]}) => " + $"({from}, {to})");
                }

                world.Graph.AddDirected(from, to, requirement, requirementCount);
                //Console.WriteLine($"Added directed edge from {from.Name} to {to.Name}");
            }

            foreach (var edges in edgeCollection.Value.Undirected)
            {
                var from = world.GetLocation(edges[0]);
                var to = world.GetLocation(edges[1]);
                if (from is null || to is null)
                {
                    throw new Exception("Name Connection Mismatch: " + $"({edges[0]}, {edges[1]}) => " + $"({from}, {to})");
                }

                world.Graph.AddDirected(from, to, requirement, requirementCount);
                world.Graph.AddDirected(to, from, requirement, requirementCount);
                //Console.WriteLine($"Added undirected edge from {from.Name} to {to.Name}");
            }
        }

        // Connect the start edge to the start edge of the Metroid graph
        // This will have to change when we know how we actually want to connect portals and such
        //world.Graph.AddDirected(world.GetLocation("start"), world.GetLocation("Brinstar - Morph Room - Spawn Platform (2) - Spawn Platform"), world.GetItem("fixed"));
        //world.Graph.AddDirected(world.GetLocation("start"), world.GetLocation("Meta - Metroid Meta Locations - Meta (0) - Meta"), world.GetItem("fixed"));

        var startMap = yamlReader.GetStartMap();
        var formattedStartMap = startMap.ToString("X2");
        world.Graph.AddDirected(world.GetLocation("start"), world.GetLocation($"Z1 - Overworld - Map {formattedStartMap} - Left exit"), world.GetItem("fixed"));
        world.Graph.AddDirected(world.GetLocation("start"), world.GetLocation($"Z1 - Overworld - Meta - Meta"), world.GetItem("fixed"));

        // Patch the Level 9 entrance edge to account for different triforce requirements
        var levelEntrance = world.GetLocation("Z1 - Level 9 - Entrance");
        var entranceEdge = levelEntrance.Edges.Find(e => e.Condition.Item.Name == "Z1Triforce")!;
        var newEdge = new Edge(entranceEdge.From, entranceEdge.To, new ItemCondition(entranceEdge.Condition.Item, world.Config.Z1Triforces));
        levelEntrance.Edges.Remove(entranceEdge);
        levelEntrance.Edges.Add(newEdge);

    }

    public static PooledItem[] GetItemSet(World world)
    {
        return
        [
            new PooledItem(new ItemSetName("z1d1", world), 1, world.GetItem("Z1Map", Game.Zelda)),
            new PooledItem(new ItemSetName("z1d1", world), 1, world.GetItem("Z1Compass", Game.Zelda)),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("z1d1", world), 1, world.GetItem("Z1Key", Game.Zelda)), 4),

            new PooledItem(new ItemSetName("z1d2", world), 1, world.GetItem("Z1Map", Game.Zelda)),
            new PooledItem(new ItemSetName("z1d2", world), 1, world.GetItem("Z1Compass", Game.Zelda)),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("z1d2", world), 1, world.GetItem("Z1Key", Game.Zelda)), 3),

            new PooledItem(new ItemSetName("z1d3", world), 1, world.GetItem("Z1Map", Game.Zelda)),
            new PooledItem(new ItemSetName("z1d3", world), 1, world.GetItem("Z1Compass", Game.Zelda)),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("z1d3", world), 1, world.GetItem("Z1Key", Game.Zelda)), 4),

            new PooledItem(new ItemSetName("z1d4", world), 1, world.GetItem("Z1Map", Game.Zelda)),
            new PooledItem(new ItemSetName("z1d4", world), 1, world.GetItem("Z1Compass", Game.Zelda)),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("z1d4", world), 1, world.GetItem("Z1Key", Game.Zelda)), 3),

            new PooledItem(new ItemSetName("z1d5", world), 1, world.GetItem("Z1Map", Game.Zelda)),
            new PooledItem(new ItemSetName("z1d5", world), 1, world.GetItem("Z1Compass", Game.Zelda)),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("z1d5", world), 1, world.GetItem("Z1Key", Game.Zelda)), 5),

            new PooledItem(new ItemSetName("z1d6", world), 1, world.GetItem("Z1Map", Game.Zelda)),
            new PooledItem(new ItemSetName("z1d6", world), 1, world.GetItem("Z1Compass", Game.Zelda)),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("z1d6", world), 1, world.GetItem("Z1Key", Game.Zelda)), 4),

            new PooledItem(new ItemSetName("z1d7", world), 1, world.GetItem("Z1Map", Game.Zelda)),
            new PooledItem(new ItemSetName("z1d7", world), 1, world.GetItem("Z1Compass", Game.Zelda)),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("z1d7", world), 1, world.GetItem("Z1Key", Game.Zelda)), 3),

            new PooledItem(new ItemSetName("z1d8", world), 1, world.GetItem("Z1Map", Game.Zelda)),
            new PooledItem(new ItemSetName("z1d8", world), 1, world.GetItem("Z1Compass", Game.Zelda)),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("z1d8", world), 1, world.GetItem("Z1Key", Game.Zelda)), 4),

            new PooledItem(new ItemSetName("z1d9", world), 1, world.GetItem("Z1Map", Game.Zelda)),
            new PooledItem(new ItemSetName("z1d9", world), 1, world.GetItem("Z1Compass", Game.Zelda)),
            .. Enumerable.Repeat(new PooledItem(new ItemSetName("z1d9", world), 1, world.GetItem("Z1Key", Game.Zelda)), 2),

            new PooledItem(new ItemSetName("lw", world), 4, world.GetItem("Z1SwordL1", Game.Zelda)),

            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Z1Bombs", Game.Zelda)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Z1StepLadder", Game.Zelda)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Z1Raft", Game.Zelda)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Z1Recorder", Game.Zelda)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Z1SwordL2", Game.Zelda)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Z1SwordL3", Game.Zelda)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Z1BlueCandle", Game.Zelda)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Z1RedCandle", Game.Zelda)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Z1SilverArrows", Game.Zelda)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Z1Bow", Game.Zelda)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Z1Arrows", Game.Zelda)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Z1MagicalKey", Game.Zelda)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Z1Rod", Game.Zelda)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Z1Book", Game.Zelda)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Z1BlueRing", Game.Zelda)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Z1RedRing", Game.Zelda)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Z1PowerBracelet", Game.Zelda)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Z1Letter", Game.Zelda)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Z1MagicShield", Game.Zelda)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Z1Boomerang", Game.Zelda)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Z1MagicBoomerang", Game.Zelda)),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("Z1HeartContainer", Game.Zelda)), 9),

            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("Z1HeartContainer", Game.Zelda)), 4),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("Z1Bombs", Game.Zelda)), 20),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("Z1Key", Game.Zelda)), 9),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("Z1Rupee", Game.Zelda)), 6),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("Z1Rupee5", Game.Zelda)), 11),
        ];
    }
}
