namespace Randomizer.Graph.Combo.Metroid;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

internal class MetroidYamlReader
{
    private YamlData? data;
    private readonly Dictionary<string, Dictionary<string, object>> vertices = [];
    private readonly Dictionary<string, DirectedUndirectedPair> edges = [];

    public YamlData? Data { get { return data; } }

    internal class YamlData
    {
        public List<Room> rooms;
        public List<Screen> screens;
    }

    internal class Screen
    {
        public string name;
        public Area area;
        public int screen;
        public Scrolling scroll;
        public NodeCollection nodes;
        public EdgeCollection edges;
    }

    internal class NodeCollection
    {
        public List<Door>? doors;
        public List<Exit>? exits;
        public List<Location>? locations;
    }

    internal class EdgeCollection
    {
        public Dictionary<string, List<object>>? undirected;
        public Dictionary<string, List<object>>? directed;
    }

    internal class Exit
    {
        public string name;
        public ExitType type;
        public Direction direction;
        public int? height;
    }

    internal class Door
    {
        public string name;
        public DoorType type;
        public Direction direction;
    }

    internal class Location
    {
        public string name;
        public LocationType type;
        public int[]? position;
        public string? item;
    }

    internal class Room
    {
        public string name;
        public Area area;
        public Scrolling scroll;
        public int[] position;
        public int[] screens;
        public List<Sprite>? sprites;
    }

    internal class Sprite
    {
        public string name;
        public int screen;
        public string location;
        public int slot;
        public SpriteType type;
        public string item;
    }

    internal enum SpriteType
    {
        Item,
        Elevator
    }

    internal enum ExitType
    {
        Scroll,
        Tunnel,
        Elevator
    }

    internal enum LocationType
    {
        Meta,
        Item,
        Boss,
        Elevator,
        Start
    }

    internal enum DoorType
    {
        Blue,
        Red,
        Purple,
        Orange
    }

    internal enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    internal enum Area : int
    {
        Brinstar = 0,
        Norfair = 1,
        Kraid = 2,
        Tourian = 3,
        Ridley = 4,
        Meta = 5
    }

    internal enum Scrolling
    {
        Vertical,
        Horizontal
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

    private void Load()
    {
        var path = Path.Combine(YamlReader.DataRoot, "../Combo/Data/Metroid");
        var screens = LoadFiles<Screen>(Path.Combine(path, "Screens"));
        var rooms = LoadFiles<Room>(Path.Combine(path, "Rooms"));

        data = new YamlData
        {
            rooms = rooms,
            screens = screens
        };
    }

    public Dictionary<string, DirectedUndirectedPair> GetForWorld(World world)
    {
        if (data is null)
        {
            Load();
        }

        return edges.Select(e => { e.Value.Directed = e.Value.Directed.Select(d => { d[0] = $"M1 - {d[0]}"; d[1] = $"M1 - {d[1]}"; return d; }).ToList(); e.Value.Undirected = e.Value.Undirected.Select(d => { d[0] = $"M1 - {d[0]}"; d[1] = $"M1 - {d[1]}"; return d; }).ToList(); return e; }).ToDictionary(e => $"{e.Key}", e => e.Value);
    }

    public List<Dictionary<string, object>> LoadYmlData(World world)
    {
        if (data is null)
        {
            Load();
        }

        return vertices.Values.Select(v => { v["name"] = $"M1 - {v["name"]}"; return v; }).ToList();
    }

    public void BuildGraph()
    {
        if (data is null)
        {
            Load();
        }

        foreach (var room in data.rooms)
        {
            BuildRoom(room);
        }

        // Construct item table data 

    }

    private Dictionary<string, object> CreateNode(Dictionary<string, object> nodeData)
    {
        vertices.Add((string)nodeData["name"], nodeData);
        return nodeData;
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

        if (undirected)
        {
            edgePair.Undirected.Add([(string)from["name"], (string)to["name"]]);
        }
        else
        {
            edgePair.Directed.Add([(string)from["name"], (string)to["name"]]);
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

    private void BuildRoom(Room room)
    {
        var roomName = $"{room.area} - {room.name}";
        int prevScreen = 0;

        foreach (var (screenNum, screenId) in room.screens.Select((s, i) => (i, s)))
        {
            var screen = data.screens.Find(s => s.area == room.area && s.screen == screenId);
            var screenName = $"{roomName} - {screen.name} ({screenNum})";
            int[] position = room.scroll switch
            {
                Scrolling.Horizontal => new int[] { room.position[0] + screenNum, room.position[1] },
                Scrolling.Vertical => new int[] { room.position[0], room.position[1] + screenNum },
                _ => throw new NotImplementedException(),
            };

            // Add and connect the doors for this screen
            foreach (var door in screen.nodes?.doors ?? new List<Door>())
            {
                var doorName = $"{screenName} - {door.name}";
                var doorNode = FindOrCreateNode(doorName);

                int[] targetDoorPosition = door.direction switch
                {
                    Direction.Up => [position[0], position[1] - 1],
                    Direction.Down => [position[0], position[1] + 1],
                    Direction.Left => [position[0] - 1, position[1]],
                    Direction.Right => [position[0] + 1, position[1]],
                    _ => throw new NotImplementedException(),
                };

                var edgeWeight = door.type switch
                {
                    DoorType.Blue => "fixed",
                    DoorType.Red => "Missile",
                    DoorType.Purple => "Missile|2",
                    DoorType.Orange => "Missile|3",
                    _ => throw new NotImplementedException(),
                };

                var targetDoorName = FindDoor(room, door, targetDoorPosition);
                if (targetDoorName != null)
                {
                    var targetDoorNode = FindOrCreateNode(targetDoorName);
                    AddDirectedEdge(doorNode, targetDoorNode, edgeWeight);
                }
            }

            // Connect elevators
            var elevator = screen.nodes?.exits?.Find(e => e.type == ExitType.Elevator && e.direction == Direction.Down && room.screens.Length == 1);
            if (elevator is not null)
            {
                var targetRoom = data.rooms.Find(r => r.position[0] == position[0] && r.position[1] == position[1] + 1);
                var targetScreenId = targetRoom.screens[0];
                var targetScreen = data.screens.Find(s => s.area == targetRoom.area && s.screen == targetScreenId);
                var targetExit = targetScreen.nodes.exits?.Find(e => e.type == ExitType.Elevator && e.direction == Direction.Up);
                var targetExitName = $"{targetRoom.area} - {targetRoom.name} - {targetScreen.name} (0) - {targetExit?.name}";

                var elevatorName = $"{screenName} - {elevator.name}";

                var elevatorNode = FindOrCreateNode(elevatorName);
                var targetExitNode = FindOrCreateNode(targetExitName);

                AddUndirectedEdge(elevatorNode, targetExitNode, "fixed");
            }

            // Check if this room is a "middle" room
            if (screen.nodes?.doors is null && screen.nodes?.locations is null)
            {
                if (screen.nodes?.exits is not null && screen.nodes?.exits.Count == 2)
                {
                    bool basicRoom = (screen.nodes.exits[0].direction, screen.nodes.exits[0].type, screen.nodes.exits[1].direction, screen.nodes.exits[1].type) switch
                    {
                        (Direction.Up, ExitType.Scroll, Direction.Down, ExitType.Scroll) => true,
                        (Direction.Down, ExitType.Scroll, Direction.Up, ExitType.Scroll) => true,
                        (Direction.Left, ExitType.Scroll, Direction.Right, ExitType.Scroll) => true,
                        (Direction.Right, ExitType.Scroll, Direction.Left, ExitType.Scroll) => true,
                        _ => false
                    };

                    if (basicRoom && screen.edges.directed is null && (screen.edges.undirected?.Count ?? 0) == 1)
                    {
                        continue;
                    }
                }
            }


            // Connect exits
            if (screenNum > 0)
            {
                var (fromDirection, targetDirection) = room.scroll switch
                {
                    Scrolling.Horizontal => (Direction.Left, Direction.Right),
                    Scrolling.Vertical => (Direction.Up, Direction.Down),
                    _ => throw new NotImplementedException(),
                };


                if (screen.nodes?.exits is not null)
                {
                    var toScreen = data.screens.Find(s => s.area == room.area && s.screen == room.screens[prevScreen]);
                    var toScreenName = $"{roomName} - {toScreen.name} ({prevScreen})";

                    foreach (var exit in screen.nodes.exits.Where(e => e.direction == fromDirection))
                    {
                        var exitName = $"{screenName} - {exit.name}";
                        var targetExits = toScreen.nodes?.exits?.Where(e => e.direction == targetDirection);
                        foreach (var targetExit in targetExits ?? new List<Exit>())
                        {
                            var targetExitName = $"{toScreenName} - {targetExit.name}";
                            var exitNode = FindOrCreateNode(exitName);
                            var targetExitNode = FindOrCreateNode(targetExitName);
                            AddUndirectedEdge(exitNode, targetExitNode, "fixed");
                        }
                    }
                }
            }

            // Create location nodes
            foreach (var location in screen.nodes?.locations ?? new List<Location>())
            {
                var locationName = $"{screenName} - {location.name}";
                var locationNode = FindOrCreateNode(locationName, location.item);

                // Connect to sprites
                var itemSprite = room.sprites?.Find(s => s.screen == screenNum && s.location == location.name && s.type == SpriteType.Item);
                if (itemSprite is not null)
                {
                    var itemName = $"{screenName} - {itemSprite.name}";
                    var itemNode = CreateNode(new()
                    {
                        { "name", itemName },
                        { "type", VertexType.Item },
                        { "item", null },
                        { "itemset", (string[])["metroid"] },
                    });

                    AddUndirectedEdge(locationNode, itemNode, "fixed");
                }
            }

            var allEdges = screen.edges?.undirected?.Select(u => ("undirected", u.Key, u.Value)).ToList() ?? new();
            allEdges.AddRange(screen.edges?.directed?.Select(d => ("directed", d.Key, d.Value)).ToList() ?? new());

            // Create all edges
            foreach (var (type, weight, edges) in allEdges)
            {
                foreach (var edge in edges ?? new())
                {
                    var edgeNames = ((List<object>)edge).Cast<string>().ToList();
                    var fromNodeName = $"{screenName} - {edgeNames[0]}";
                    var toNodeName = $"{screenName} - {edgeNames[1]}";

                    var fromNode = FindOrCreateNode(fromNodeName);
                    var toNode = FindOrCreateNode(toNodeName);

                    AddEdge(fromNode, toNode, weight, type == "undirected");
                }
            }

            prevScreen = screenNum;
        }
    }

    private string? FindDoor(Room fromRoom, Door fromDoor, int[] targetDoorPosition)
    {
        foreach (var room in data.rooms.Where(r => r.area == fromRoom.area))
        {
            foreach (var (screenNum, screenId) in room.screens.Select((s, i) => (i, s)))
            {
                int[] position = room.scroll switch
                {
                    Scrolling.Horizontal => new int[] { room.position[0] + screenNum, room.position[1] },
                    Scrolling.Vertical => new int[] { room.position[0], room.position[1] + screenNum },
                    _ => throw new NotImplementedException(),
                };

                if (position[0] == targetDoorPosition[0] && position[1] == targetDoorPosition[1])
                {
                    var screen = data.screens.Find(s => s.area == fromRoom.area && s.screen == screenId);
                    var roomName = $"{room.area} - {room.name}";
                    var screenName = $"{roomName} - {screen.name} ({screenNum})";

                    var targetDoor = fromDoor.direction switch
                    {
                        Direction.Up => screen.nodes.doors?.Find(d => d.direction == Direction.Down),
                        Direction.Down => screen.nodes.doors?.Find(d => d.direction == Direction.Up),
                        Direction.Left => screen.nodes.doors?.Find(d => d.direction == Direction.Right),
                        Direction.Right => screen.nodes.doors?.Find(d => d.direction == Direction.Left),
                        _ => throw new NotImplementedException(),
                    } ?? throw new Exception($"Could not find door in {screenName} to match {fromDoor.name}");

                    var targetDoorName = $"{screenName} - {targetDoor.name}";
                    return targetDoorName;
                }
            }
        }

        return null;
    }

    public Dictionary<int, byte[]> BuildPortalRooms(World world)
    {
        if(data is null)
        {
            Load();
        }

        // This will create new rooms for the portals, add it to the graph by patching the room/screen definitions and return a list of patch data to write to the ROM
        var patchData = new Dictionary<int, byte[]>();

        // Portal 1 (New door in brinstar shaft)

        // Patch the shaft data so the logic knows there's a door there
        var brinstarShaft = data.rooms.Find(r => r.area == Area.Brinstar && r.name == "Left Vertical Shaft")!;
        brinstarShaft.screens[11] = 0x03;

        // Create a new dummy room behind this door
        var newRoom = new Room
        {
            name = "Brinstar Portal",
            area = Area.Brinstar,
            position = new int[] { 0x0C, 0x0C },
            screens = new int[] { 0x1F },
            scroll = Scrolling.Horizontal,
            sprites = []
        };

        data.rooms.Add(newRoom);

        // This should take care of the logic implications of this new door, now add the patch data
        patchData.Add(0x68253E + (0x20 * 0x0C) + 0x0B, new byte[] { 0x03, 0x1F });


        return patchData;
    }
}

