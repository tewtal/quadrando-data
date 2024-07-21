namespace Randomizer.Graph.Combo.Zelda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class ZeldaYamlReader
{
    private YamlData? data;
    private Dictionary<string, Dictionary<string, object>> vertices = new Dictionary<string, Dictionary<string, object>>();
    private Dictionary<string, DirectedUndirectedPair> edges = new Dictionary<string, DirectedUndirectedPair>();

    public YamlData? Data { get { return data;  } }

    internal class Screen
    {
        public string name;
        public Area area;
        public int screen;
        public NodeCollection nodes;
        public EdgeCollection edges;
    }

    internal class NodeCollection
    {
        public List<Exit> exits;
        public List<Cave> caves;
        public List<Region> regions;
        public List<Meta> meta;
    }

    internal class EdgeCollection
    {
        public Dictionary<string, List<object>> undirected;
        public Dictionary<string, List<object>> directed;
    }

    internal class Exit
    {
        public string name;
        public ExitType type;
        public Direction direction;
    }

    internal class Cave
    {
        public string name;
        public CaveType type;
    }

    internal class Meta
    {
        public string name;
        public MetaType type;
        public string position;
        public string? item;
    }

    internal class Region
    {
        public string name;
        public RegionType type;
        public int[] from;
        public int[] to;
    }

    internal enum MetaType
    {
        Meta,
        Item,
        Armos,
        Fairy,
        Push,
        Stairs
    }

    internal enum CaveType
    {
        Bomb,
        Open,
        Grave,
        Tree,
        Push
    }

    internal enum Area
    {
        Overworld,
        Underworld,
    }

    internal enum ExitType
    {
        Scroll
    }

    internal enum RegionType
    {
        Region
    }

    internal enum Direction
    {
        Up,
        Down,
        Left,
        Right,
    }

    internal class OverworldMap
    {
        public string name;
        public Area area;
        public int map;
        public int screen;
        public int[] palettes;
        public bool zora;
        public bool waves;
        public int enemies;
        public int enemy_id;
        public int enemy_sides;
        public int enemy_mode;
        public int cave;
        public int stairs;
        public int[] secret;
        public int[] exit;
        public int level_info_e;
    }

    internal class UnderworldMap
    {
        public string name;
        public Area area;
        public int map;
        public int screen;
        public int[] palettes;
        public int[] doors;
        public bool passage;
        public int passage_left;
        public int passage_right;
        public int enemies;
        public int enemy_id;
        public int enemy_mode;
        public bool push_block;
        public bool dark_room;
        public int boss_sfx;
        public int room_item;
        public int item_pos;
        public int behaviour;
    }

    internal enum DoorType : int
    {
        Open = 0,
        Wall = 1,
        PassThrough = 2,
        PassThroughNoSound = 3,
        Bombable = 4,
        Locked = 5,
        Locked2 = 6,
        Shutter = 7
    }

    internal enum RoomBehaviour : int
    {
        None = 0,
        KillForItemShutter = 1,
        Leader = 2,
        GetTriforceShutter = 3,
        PushBlockShutter = 4,
        PushBlockStairs = 5,
        LeaderShutters = 6,
        KillForItemShutterBoss = 7
    }

    internal class Level
    {
        public string name;
        public int level;
        public Area area;
        public int[] rooms;
        public int[] enemy_counts;
        public int start_room_id;
        public int start_y;
        public int boss_room_id;
        public int triforce_room_id;
        public int[] shortcut_or_item_pos_array;
        public int[] cellar_room_id_array;
        public int[] world_flags_addr;
        public int[] submenu_map_mask;
        public int submenu_map_rotation;
        public int status_bar_map_x_offset;
        public byte[] status_bar_map_transfer_buf;
        public byte[] palettes_transfer_buf;
        public byte[] palette_cycles;
        public byte[] death_palette_series;
    }

    internal class Special
    {
        public int overworld_item_room;
        public int overworld_item_x;
        public int overworld_item_id;
        public int armos_item_room;
        public int armos_item_x;
        public int armos_item_id;
        public int[] armos_stairs;
        public int[] armos_x_pos;
        public int[] step_ladder;
        public int[] recorder_stairs;
        public int[] recorder_dests;
        public int[] recorder_y_pos;
        public int[] any_road_x_pos;
        public int start;
    }

    internal class CaveData
    {
        public string name;
        public int cave;
        public int[] items;
        public int[] flags;
        public int[] prices;
        public int text;

        public CaveFlags Flag => (CaveFlags)((text & 0xC0) >> 6 | flags[2] >> 4 | flags[1] >> 2 | flags[0]);
    }

    [Flags]
    internal enum CaveFlags : int
    {
        PickItem = 0x01,
        Shop = 0x02,
        ShowItems = 0x04,
        ShowPrices = 0x08,
        MoneyGame = 0x10,
        Hint = 0x20,
        HeartRequirement = 0x40,
        NegativeAmounts = 0x80,
    }

    internal class YamlData
    {
        public List<OverworldMap> overworld_maps;
        public List<Screen> overworld_screens;

        public List<UnderworldMap> underworld_maps;
        public List<Screen> underworld_screens;

        public List<Level> levels;
        public List<CaveData> caves;
        public Special special;
    }

    private List<T> LoadFiles<T>(string path)
    {
        var yamlData = new List<T>();
        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();

        foreach (var fileName in Directory.GetFiles(path, "*.yml", SearchOption.AllDirectories))
        {
            var fileContents = File.ReadAllText(fileName);
            try
            {
                var data = deserializer.Deserialize<T>(fileContents);
                yamlData.Add(data);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while deserializing: {fileName}, {e.Message} ({e.InnerException?.Source ?? ""}");
            }
        }

        return yamlData;
    }

    private T LoadFile<T>(string path)
    {
        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();
        try
        {
            var data = deserializer.Deserialize<T>(File.ReadAllText(path));
            return data;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while deserializing: {path}, {e.Message} ({e.InnerException?.Source ?? ""}");
            return default;
        }
    }

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

    private void AddEdge(Dictionary<string, object> from, Dictionary<string, object> to, string edgeGroup, bool undirected = false)
    {
        if (!edges.TryGetValue(edgeGroup, out var edgePair))
        {
            edgePair = new DirectedUndirectedPair();
            edges.Add(edgeGroup, edgePair);
        }

        // Check if the edge already exists
        var edgeExists = undirected ? edgePair.Undirected.Any(e => e.SequenceEqual(new string[] { (string)from["name"], (string)to["name"] })) :
                                      edgePair.Directed.Any(e => e.SequenceEqual(new string[] { (string)from["name"], (string)to["name"] }));

        if (!edgeExists)
        {
            if (undirected)
            {
                edgePair.Undirected.Add([(string)from["name"], (string)to["name"]]);
            }
            else
            {
                edgePair.Directed.Add([(string)from["name"], (string)to["name"]]);
            }
        }
    }

    private void AddDirectedEdge(Dictionary<string, object> from, Dictionary<string, object> to, string edgeGroup)
    {
        AddEdge(from, to, edgeGroup);
    }

    private void AddUndirectedEdge(Dictionary<string, object> from, Dictionary<string, object> to, string edgeGroup)
    {
        AddEdge(from, to, edgeGroup, true);
    }

    public void Load()
    {
        var path = Path.Combine(YamlReader.DataRoot, "../Combo/Data/Zelda");
        var levels = LoadFiles<Level>(Path.Combine(path, "Levels"));
        
        var overworldMaps = LoadFiles<OverworldMap>(Path.Combine(path, "Maps/Overworld"));
        var underworldMaps = LoadFiles<UnderworldMap>(Path.Combine(path, "Maps/Underworld"));

        var overworldScreens = LoadFiles<Screen>(Path.Combine(path, "Screens/Overworld"));
        var underworldScreens = LoadFiles<Screen>(Path.Combine(path, "Screens/Underworld"));

        var caves = LoadFile<List<CaveData>>(Path.Combine(path, "Caves.yml"));
        var special = LoadFile<Special>(Path.Combine(path, "Special.yml"));

        data = new YamlData()
        {
            levels = levels,
            overworld_maps = overworldMaps,
            underworld_maps = underworldMaps,
            overworld_screens = overworldScreens,
            underworld_screens = underworldScreens,
            special = special,
            caves = caves
        };

    }

    public int GetStartMap()
    {
        return data.special.start;
    }

    public Dictionary<string, DirectedUndirectedPair> GetForWorld(World world)
    {
        return edges.Select(e => { e.Value.Directed = e.Value.Directed.Select(d => { d[0] = $"Z1 - {d[0]}"; d[1] = $"Z1 - {d[1]}"; return d; }).ToList(); e.Value.Undirected = e.Value.Undirected.Select(d => { d[0] = $"Z1 - {d[0]}"; d[1] = $"Z1 - {d[1]}"; return d; }).ToList(); return e; }).ToDictionary(e => $"{e.Key}", e => e.Value);
    }

    public List<Dictionary<string, object>> LoadYmlData(World world)
    {
        return vertices.Values.Select(v => { v["name"] = $"Z1 - {v["name"]}"; return v; }).ToList();
    }

    public void BuildGraph()
    {
        foreach (var map in data.overworld_maps)
        {
            BuildOverworldMap(map);
        }

        foreach(var level in data.levels.Where(l => l.level > 0))
        {
            foreach (var map in data.underworld_maps.Where(m => level.rooms.Contains(m.map)))
            {
                BuildUnderworldMap(map, level);
            }

            // Connect level entrance
            var levelEntranceNode = FindOrCreateNode($"Level {level.level} - Entrance");

            // Get start room from map
            var startMap = data.underworld_maps.Where(m => m.area == Area.Underworld && m.map == level.start_room_id).First();
            var startScreen = data.underworld_screens.Where(s => s.area == startMap.area && s.screen == startMap.screen).First();
            var startNode = startScreen.nodes.exits.Where(e => e.direction == Direction.Down).First();

            // Connect level entrance to start room
            // TODO: Temporary sword requirement for dungeons, remove this
            // TODO: These dungeon entrance logical restrictions are way too restrictive and neds to be fixed so that the restrictions are on the actual places
            //       where the sword, bow and recorder are required, not the entrance to the dungeon
            var levelRequirement = level.level switch
            {
                5 => "CanDefeatDigdogger",
                6 => "CanDefeatGohma",
                7 => "CanDefeatDigdogger",
                8 => "CanDefeatGohma",
                9 => "Triforce|8",
                _ => "CanHurtEnemies"
            };

            AddUndirectedEdge(levelEntranceNode, FindOrCreateNode($"{startMap.area} - {level.name} - {startMap.name} - {startNode.name}"), levelRequirement);
        }

        foreach(var cave in data.caves)
        {
            var caveEntranceNode = FindNode($"Cave {cave.cave:X2} - Entrance");
            var caveNode = FindOrCreateNode($"Cave {cave.cave:X2}");

            AddDirectedEdge(caveEntranceNode, caveNode, "fixed");

            // We don't mess with shops for now, just the take item things, and let's remove the "heart requirement" flag for now
            // TODO: Fix Heart requirement flag

            if (cave.Flag.HasFlag(CaveFlags.PickItem) && cave.Flag.HasFlag(CaveFlags.ShowItems) && !cave.Flag.HasFlag(CaveFlags.Shop) && !cave.Flag.HasFlag(CaveFlags.MoneyGame) && !cave.Flag.HasFlag(CaveFlags.Hint))
            {
                bool takeAny = cave.items.Where(i => i != 0x2F).Count() > 1;
                var caveTypeName = takeAny ? "Take Any Item" : "Take One Item";
                var caveItemSet = takeAny ? "z1takeany" : "z1takeone";
                int caveItemIndex = 0;
                foreach(var item in cave.items)
                {
                    if(item != 0x2F)
                    {
                        string junkItem = null;
                        if (takeAny)
                        {
                            // TODO: Fix this
                            // Pre-fill this take-any with a junk item
                            List<string> junkItems = ["Rupee", "Rupee5", "Heart", "Key", "Bombs", "Arrows"];
                            junkItem = junkItems[new Random().Next(0, junkItems.Count)];
                        }

                        // This is an item we want to add to the cave
                        var itemNode = CreateNode(new()
                        {
                            { "name", $"Cave {cave.cave:X2} - {caveTypeName} - Item {caveItemIndex:X2}" },
                            { "type", VertexType.Item },
                            { "item", junkItem },
                            { "address", 0x650100 + ((cave.cave-0x10)*3) + caveItemIndex },
                            { "itemset", (string[])["zelda", $"z1c{cave.cave:X2}", caveItemSet] },
                        });

                        if (cave.Flag.HasFlag(CaveFlags.HeartRequirement))
                        {
                            var heartRequirement = cave.cave switch
                            {
                                0x12 => "HeartContainer|2",
                                0x13 => "HeartContainer|9",
                                _ => "fixed"
                            };
                            AddDirectedEdge(caveNode, itemNode, heartRequirement);
                        }
                        else
                        {
                            AddDirectedEdge(caveNode, itemNode, "fixed");
                        }

                        
                    }
                    caveItemIndex++;
                }
            }            
        }
    }

    private void BuildOverworldMap(OverworldMap map)
    {
        var level = data.levels.First();
        var screen = data.overworld_screens.Where(s => s.area == map.area && s.screen == map.screen).First();
        var mapName = $"{map.area} - {map.name}";
        
        var exits = new List<Direction>();
        foreach (var exit in screen.nodes.exits ?? [])
        {
            var exitName = $"{mapName} - {exit.name}";
            var exitNode = FindOrCreateNode(exitName);
            exits.Add(exit.direction);
        }

        foreach (var cave in screen.nodes.caves ?? [])
        {
            if (map.cave > 0 && (cave.type == CaveType.Open || cave.type == CaveType.Push || cave.type == CaveType.Bomb || cave.type == CaveType.Tree || cave.type == CaveType.Grave || map.secret[0] == 1))
            {
                var caveName = $"{mapName} - {cave.name}";
                var caveNode = FindOrCreateNode(caveName);
            }
        }

        foreach (var meta in screen.nodes.meta ?? [])
        {
            if (meta.type == MetaType.Armos)
            {
                if (data.special.armos_stairs.Contains(map.map) || data.special.armos_item_room == map.map)
                {
                    var metaName = $"{mapName} - {meta.name}";
                    var metaNode = FindOrCreateNode(metaName);
                }
            } 
            else
            {
                var metaName = $"{mapName} - {meta.name}";
                var metaNode = FindOrCreateNode(metaName, meta.item);
            }
        }

        // Add all the undirected edges in the room
        foreach (var undirected in screen.edges.undirected ?? [])
        {
            var requirement = undirected.Key;
            var edges = undirected.Value;
            foreach (List<object> edge in edges ?? [])
            {
                var fromString = (string)edge[0];
                var toString = (string)edge[1];

                var fromName = $"{mapName} - {fromString}";
                var toName = $"{mapName} - {toString}";

                var fromNode = FindNode(fromName);
                var toNode = FindNode(toName);

                if(fromNode != null && toNode != null)
                {
                    AddUndirectedEdge(fromNode, toNode, requirement);
                }
            }
        }

        // Add all the directed edges in the room
        foreach (var directed in screen.edges.directed ?? [])
        {
            var requirement = directed.Key;
            var edges = directed.Value;
            foreach (List<object> edge in edges ?? [])
            {
                var fromString = (string)edge[0];
                var toString = (string)edge[1];

                var fromName = $"{mapName} - {fromString}";
                var toName = $"{mapName} - {toString}";

                var fromNode = FindNode(fromName);
                var toNode = FindNode(toName);

                if (fromNode != null && toNode != null)
                {
                    AddDirectedEdge(fromNode, toNode, requirement);
                }
            }
        }

        // Connect caves
        if (map.cave > 0)
        {
            var cave = screen.nodes.caves?.FirstOrDefault() ?? null;
            if (cave != null)
            {
                if (cave.type == CaveType.Open || cave.type == CaveType.Push || cave.type == CaveType.Bomb || cave.type == CaveType.Tree || cave.type == CaveType.Grave || map.secret[0] == 1)
                {
                    if (map.cave < 10)
                    {
                        // This is a dungeon entrance
                        var dungeonStartNode = FindOrCreateNode($"Level {map.cave} - Entrance");
                        var caveNode = FindOrCreateNode($"{mapName} - {cave.name}");

                        AddUndirectedEdge(caveNode, dungeonStartNode, "fixed");
                    } 
                    else
                    {
                        var caveEntranceNode = FindOrCreateNode($"Cave {map.cave:X2} - Entrance");
                        var caveNode = FindOrCreateNode($"{mapName} - {cave.name}");

                        AddDirectedEdge(caveNode, caveEntranceNode, "fixed");
                    }
                }
            }
        }

        // Connect stairs
        if (data.special.armos_stairs.Contains(map.map))
        {
            var armosScreenNode = screen.nodes.meta.Where(m => m.type == MetaType.Armos).First();
            var armosNode = FindOrCreateNode($"{mapName} - {armosScreenNode.name}");
            var caveNodeName = map.cave switch
            {
                <= 10 => $"Level {map.cave} - Entrance",
                _ => $"Cave {map.cave:X2} - Entrance"
            };

            var caveNode = FindOrCreateNode(caveNodeName);

            AddDirectedEdge(armosNode, caveNode, "fixed");
            if (map.cave <= 10)
            {
                AddDirectedEdge(caveNode, armosNode, "fixed");
            }            
        }

        // Connect armos item
        if (data.special.armos_item_room == map.map)
        {
            var armosScreenNode = screen.nodes.meta.Where(m => m.type == MetaType.Armos).First();
            var armosNode = FindOrCreateNode($"{mapName} - {armosScreenNode.name}");

            var itemNode = CreateNode(new()
            {
                { "name", $"{mapName} - {armosScreenNode.name} - Item" },
                { "type", VertexType.Item },
                { "item", null },
                { "address", 0x620CF5 },
                { "itemset", (string[])["zelda"] },
            }); 

            AddDirectedEdge(armosNode, itemNode, "fixed");
        }

        // Connect overworld item
        if (data.special.overworld_item_room == map.map)
        {
            var itemScreenNode = screen.nodes.meta.Where(m => m.type == MetaType.Item).First();
            var itemNode = FindOrCreateNode($"{mapName} - {itemScreenNode.name}");

            var overworldNode = CreateNode(new()
            {
                { "name", $"{mapName} - {itemScreenNode.name} - Item" },
                { "type", VertexType.Item },
                { "item", null },
                { "address", 0x62B88A },
                { "itemset", (string[])["zelda"] },
            });

            AddDirectedEdge(itemNode, overworldNode, "fixed");
        }

        // Connect overworld recorder stairs
        if (data.special.recorder_stairs.Contains(map.map))
        {
            var firstExitScreenNode = screen.nodes.exits.First();
            var firstExitNode = FindOrCreateNode($"{mapName} - {firstExitScreenNode.name}");
            var recorderNode = FindOrCreateNode($"{mapName} - Recorder Stairs");

            var caveNodeName = map.cave switch
            {
                <= 10 => $"Level {map.cave} - Entrance",
                _ => $"Cave {map.cave:X2} - Entrance"
            };

            var caveNode = FindOrCreateNode(caveNodeName);

            AddDirectedEdge(firstExitNode, recorderNode, "Recorder");
            AddDirectedEdge(recorderNode, caveNode, "fixed");

            if (map.cave <= 10)
            {
                AddDirectedEdge(caveNode, firstExitNode, "fixed");
            }
        }

        foreach (var exit in exits)
        {
            var offset = exit switch
            {
                Direction.Left => -1,
                Direction.Right => 1,
                Direction.Up => -16,
                Direction.Down => 16,
            };

            var targetMap = data.overworld_maps.Where(m => m.area == map.area && m.map == map.map + offset).FirstOrDefault();
            if (targetMap != null)
            {
                ConnectOWMaps(map, targetMap, exit);
            }
        }
    }

    private void ConnectOWMaps(OverworldMap from, OverworldMap to, Direction direction)
    {
        var oppositeDirection = direction switch
        {
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
        };

        var currentScreen = data.overworld_screens.Where(s => s.area == from.area && s.screen == from.screen).First();
        var currentExits = currentScreen.nodes.exits.Where(e => e.direction == direction).ToList();

        var targetScreen = data.overworld_screens.Where(s => s.area == to.area && s.screen == to.screen).First();
        var targetExits = targetScreen.nodes.exits.Where(e => e.direction == oppositeDirection).ToList();

        // Create undirected connections between all these exits
        foreach (var currentExit in currentExits)
        {
            foreach (var targetExit in targetExits)
            {
                var currentExitNode = FindOrCreateNode($"{from.area} - {from.name} - {currentExit.name}");
                var targetExitNode = FindOrCreateNode($"{to.area} - {to.name} - {targetExit.name}");

                AddUndirectedEdge(currentExitNode, targetExitNode, "fixed");
            }
        }
    }

    private void BuildUnderworldMap(UnderworldMap map, Level level)
    {
        var mapName = $"{map.area} - {level.name} - {map.name}";

        if (!map.passage)
        {
            var screen = data.underworld_screens.Where(s => s.area == map.area && s.screen == map.screen).First();

            // Go through doors and create nodes for them
            for (int i = 0; i <= 3; i++)
            {
                var door = (DoorType)map.doors[i];
                var direction = (Direction)i;

                // Find an exit node that matches this doors
                var exit = screen.nodes.exits.Where(e => e.direction == direction).First();
                var exitName = $"{mapName} - {exit.name}";
                var exitNode = FindOrCreateNode(exitName);

                if (door == DoorType.Wall)
                {
                    continue;
                }

                // Connect this exit to the other room
                ConnectUWMaps(map, exitNode, door, direction, level);
            }

            foreach (var meta in screen.nodes.meta ?? [])
            {
                var metaName = $"{mapName} - {meta.name}";
                var metaNode = FindOrCreateNode(metaName);

                if (meta.name == "Triforce" && level.triforce_room_id == map.map)
                {
                    // Create a Triforce Item Node and link it up
                    var triforceNode = CreateNode(new()
                    {
                        { "name", $"{mapName} - {meta.name} - Triforce" },
                        { "type", VertexType.Meta },
                        { "item", "Triforce" },
                        { "itemset", (string[])["zelda", "z1triforce"] },
                    });

                    AddDirectedEdge(metaNode, triforceNode, "fixed");
                }

                if (meta.name == "Zelda" && level.triforce_room_id == map.map)
                { 
                    // Create a Zelda Item Node and link it up
                    var zeldaNode = CreateNode(new()
                    {
                        { "name", $"{mapName} - {meta.name} - Zelda" },
                        { "type", VertexType.Meta },
                        { "item", "Zelda" },
                        { "itemset", (string[])["zelda"] },
                    });

                    AddDirectedEdge(metaNode, zeldaNode, "fixed");
                }

                if (meta.name == "Ganon")
                {
                    var ganonNode = CreateNode(new()
                    {
                        { "name", $"{mapName} - {meta.name} - Ganon" },
                        { "type", VertexType.Meta },
                        { "item", "Triforce" },
                        { "itemset", (string[])["zelda"] },
                    });

                    AddDirectedEdge(metaNode, ganonNode, "fixed");
                }
            }

            // Create all region nodes
            foreach (var region in screen.nodes.regions)
            {
                var regionName = $"{mapName} - {region.name}";
                var regionNode = FindOrCreateNode(regionName);
            }

            // Does this room have an item? (This should be 2F when writing back combo data)
            if (map.room_item != 0x03 && level.triforce_room_id != map.map && map.screen != 0x28)
            {
                var itemName = (RoomBehaviour)map.behaviour switch
                {
                    RoomBehaviour.KillForItemShutter => $"{mapName} - Kill - Item",
                    RoomBehaviour.KillForItemShutterBoss => (level.boss_room_id == map.map) ? $"{mapName} - Boss - Item" : $"{mapName} - Kill - Item",
                    _ => $"{mapName} - Item"
                };

                var roomItemNode = CreateNode(new()
                {
                    { "name", itemName },
                    { "type", VertexType.Item },
                    { "item", null },
                    { "address", 0x650000 + map.map },
                    { "itemset", (string[])["zelda", $"z1d{level.level}"] },
                });

                
                // Look up item position for this room
                var levelItemPositions = level.shortcut_or_item_pos_array[map.item_pos];
                var itemX = (levelItemPositions >> 4) - 2;
                var itemY = (levelItemPositions & 0x0F) - 6;

                // Find the region on the screen that contains the coordinates of the item
                // A region has a from [x,y] and a to [x,y] coordinate that defines a rectangle

                var itemRegionNode = screen.nodes.regions.Where(r => r.from[0] <= itemX && r.from[1] <= itemY && r.to[0] >= itemX && r.to[1] >= itemY).FirstOrDefault();
                if(itemRegionNode == null)
                {
                    throw new Exception($"Could not find region for item in room {mapName}");
                }
                var itemRegionNodeName = $"{mapName} - {itemRegionNode.name}";
                var itemRegionNodeNode = FindOrCreateNode(itemRegionNodeName);

                var roomItemRequirement = (RoomBehaviour)map.behaviour switch
                {
                    RoomBehaviour.KillForItemShutter => "UseSword",
                    RoomBehaviour.KillForItemShutterBoss => "UseSword",
                    _ => "fixed"
                };

                AddDirectedEdge(itemRegionNodeNode, roomItemNode, roomItemRequirement);
            }

            // Add all undirected edges in the room
            foreach (var undirected in screen.edges.undirected ?? [])
            {
                var requirement = undirected.Key;
                var edges = undirected.Value;
                foreach (List<object> edge in edges ?? [])
                {
                    var fromString = (string)edge[0];
                    var toString = (string)edge[1];

                    var fromName = $"{mapName} - {fromString}";
                    var toName = $"{mapName} - {toString}";

                    var fromNode = FindNode(fromName);
                    var toNode = FindNode(toName);

                    if (fromNode != null && toNode != null)
                    {
                        AddUndirectedEdge(fromNode, toNode, requirement);
                    }
                }
            }

            // Add all directed edges in the room
            foreach (var directed in screen.edges.directed ?? [])
            {
                var requirement = directed.Key;
                var edges = directed.Value;
                foreach (List<object> edge in edges ?? [])
                {
                    var fromString = (string)edge[0];
                    var toString = (string)edge[1];

                    var fromName = $"{mapName} - {fromString}";
                    var toName = $"{mapName} - {toString}";

                    var fromNode = FindNode(fromName);
                    var toNode = FindNode(toName);

                    if (fromNode != null && toNode != null)
                    {
                        AddDirectedEdge(fromNode, toNode, requirement);
                    }
                }
            }
        }
        else        
        {
            if (map.screen == 0x3E)
            {
                // This is a passage
                var left_room = map.passage_left + (level.level >= 7 ? 0x80 : 0x00);
                var right_room = map.passage_right + (level.level >= 7 ? 0x80 : 0x00);

                // Create nodes for left and right
                var leftNode = FindOrCreateNode($"{mapName} - Passage - Left");
                var rightNode = FindOrCreateNode($"{mapName} - Passage - Right");

                // Connect the left and right nodes to the left and right room stairs
                var leftRoom = data.underworld_maps.Where(m => m.area == map.area && m.map == left_room).First();
                var leftScreen = data.underworld_screens.Where(s => s.area == leftRoom.area && s.screen == leftRoom.screen).First();
                var leftStairs = leftScreen.nodes.meta.Where(m => m.type == MetaType.Stairs).FirstOrDefault();
                var leftStairsName = "";

                if(leftStairs == null)
                {
                    // Connect to the region that has the top right coordinate (11,0) defined in its from and to coordinates
                    var leftStairsRegion = leftScreen.nodes.regions.Where(r => r.from[0] <= 11 && r.from[1] >= 0 && r.to[0] >= 11 && r.to[1] >= 0).First();
                    leftStairsName = leftStairsRegion.name;
                } 
                else
                {
                    leftStairsName = leftStairs.name;
                }

                var rightRoom = data.underworld_maps.Where(m => m.area == map.area && m.map == right_room).First();
                var rightScreen = data.underworld_screens.Where(s => s.area == rightRoom.area && s.screen == rightRoom.screen).First();
                var rightStairs = rightScreen.nodes.meta.Where(m => m.type == MetaType.Stairs).FirstOrDefault();
                var rightStairsName = "";

                if (rightStairs == null)
                {
                    var rightStairsRegion = leftScreen.nodes.regions.Where(r => r.from[0] <= 11 && r.from[1] >= 0 && r.to[0] >= 11 && r.to[1] >= 0).First();
                    rightStairsName = rightStairsRegion.name;
                }
                else
                {
                    rightStairsName = rightStairs.name;
                }

                // Connect the left and right nodes to the left and right room stairs
                AddUndirectedEdge(leftNode, FindOrCreateNode($"{map.area} - {level.name} - {leftRoom.name} - {leftStairsName}"), "fixed");
                AddUndirectedEdge(rightNode, FindOrCreateNode($"{map.area} - {level.name} - {rightRoom.name} - {rightStairsName}"), "fixed");

                // Connect the passage nodes
                AddUndirectedEdge(leftNode, rightNode, "fixed");

            } 
            else
            {
                // This is an item room
                var left_room = map.passage_left + (level.level >= 7 ? 0x80 : 0x00);

                // Create nodes for left and the item in the room
                var leftNode = FindOrCreateNode($"{mapName} - Passage - Left");
                var itemNode = CreateNode(new()
                {
                    { "name", $"{mapName} - Passage - Item" },
                    { "type", VertexType.Item },
                    { "item", null },
                    { "address", 0x650000 + map.map },
                    { "itemset", (string[])["zelda", $"z1d{level.level}"] },
                });

                // Connect the left and right nodes to the left
                var leftRoom = data.underworld_maps.Where(m => m.area == map.area && m.map == left_room).First();
                var leftScreen = data.underworld_screens.Where(s => s.area == leftRoom.area && s.screen == leftRoom.screen).First();
                var leftStairs = leftScreen.nodes.meta.Where(m => m.type == MetaType.Stairs).FirstOrDefault();

                if (leftStairs == null)
                {
                    // Connect to the region that has the top right coordinate (11,0) defined in its from and to coordinates
                    var topRightRegionNode = leftScreen.nodes.regions.Where(r => r.from[0] <= 11 && r.from[1] >= 0 && r.to[0] >= 11 && r.to[1] >= 0).First();

                    // Connect the left node to the top right region
                    AddUndirectedEdge(leftNode, FindOrCreateNode($"{map.area} - {level.name} - {leftRoom.name} - {topRightRegionNode.name}"), "fixed");
                } else
                {
                    // Connect the left nodes to the left stairs
                    AddUndirectedEdge(leftNode, FindOrCreateNode($"{map.area} - {level.name} - {leftRoom.name} - {leftStairs.name}"), "fixed");
                }

                // Connect the left node to the item
                AddUndirectedEdge(leftNode, itemNode, "fixed");


            }
        }
    }

    private void ConnectUWMaps(UnderworldMap from, Dictionary<string, object> exitNode, DoorType door, Direction direction, Level level)
    {
        var offset = direction switch
        {
            Direction.Left => -1,
            Direction.Right => 1,
            Direction.Up => -16,
            Direction.Down => 16,
        };

        var oppositeDirection = direction switch
        {
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
        };

        var target = data.underworld_maps.Where(m => m.area == from.area && m.map == from.map + offset && level.rooms.Contains(m.map)).FirstOrDefault();
        if (target != null)
        {
            var targetMapName = $"{target.area} - {level.name} - {target.name}";

            var targetDoor = (DoorType)target.doors[(int)oppositeDirection];

            // Find the exit node that matches this door
            var targetScreen = data.underworld_screens.Where(s => s.area == target.area && s.screen == target.screen).First();
            var targetExit = targetScreen.nodes.exits.Where(e => e.direction == oppositeDirection).First();
            var targetExitName = $"{targetMapName} - {targetExit.name}";
            var targetExitNode = FindOrCreateNode(targetExitName);

            var sourceRequirement = door switch
            {
                DoorType.Open => "fixed",
                DoorType.Wall => "Never",
                DoorType.PassThrough => "fixed",
                DoorType.PassThroughNoSound => "fixed",
                DoorType.Bombable => "UseBombs",
                DoorType.Locked => "Key",
                DoorType.Locked2 => "Key",
                DoorType.Shutter => "fixed",
                _ => throw new Exception("Unknown door type")
            };

            var targetRequirement = targetDoor switch
            {
                DoorType.Open => "fixed",
                DoorType.Wall => "Never",
                DoorType.PassThrough => "fixed",
                DoorType.PassThroughNoSound => "fixed",
                DoorType.Bombable => "UseBombs",
                DoorType.Locked => "Key",
                DoorType.Locked2 => "Key",
                DoorType.Shutter => "fixed",
                _ => throw new Exception("Unknown door type")
            };

            // Connect the source exit to the target exit
            AddDirectedEdge(exitNode, targetExitNode, sourceRequirement);

            if (door != DoorType.Wall)
            {
                AddDirectedEdge(targetExitNode, exitNode, targetRequirement);
            }
        }
    }

}
