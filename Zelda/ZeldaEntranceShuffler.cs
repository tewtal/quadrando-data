namespace Randomizer.Graph.Combo.Zelda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static global::Randomizer.Graph.Combo.Zelda.ZeldaYamlReader;

internal class ZeldaEntranceShuffler
{
    private YamlData _data;
    private World _world;
    private PRNG _prng;

    public ZeldaEntranceShuffler(World world, PRNG prng)
    {
        _data = world.ZeldaData.Data!;
        _world = world;
        _prng = prng;
    }

    public void Shuffle()
    {
        /* Disallow shuffling the portal caves for now */
        /* TODO: Make this configurable */
        List<int> portalMapIds = [0x66];

        /* Find all caves and dungeon levels */
        var caveIds = _data.overworld_maps.Where(m => !portalMapIds.Contains(m.map) && m.cave > 0 && (m.secret[0] == 1 || m.secret[1] == 0)).Select(m => m.cave).ToList();
        var maps = _data.overworld_maps.Where(m => !portalMapIds.Contains(m.map) && m.cave > 0 && (m.secret[0] == 1 || m.secret[1] == 0)).ToList();

        /* Shuffle the caves */
        caveIds = _prng.Shuffle(caveIds).ToList();
        maps = _prng.Shuffle(maps).ToList();

        /* Assign the shuffled caves to the world */
        var caveQueue = new Queue<int>(caveIds);
        foreach (var map in maps)
        {
            map.cave = caveQueue.Dequeue();
        }

        // Find all four any roads
        var anyRoads = maps.Where(m => m.cave == 0x14).ToList();
        if(anyRoads.Count != 4)
        {
            throw new Exception("Expected 4 any roads");
        }

        for(int i = 0; i < anyRoads.Count; i++)
        {
            _data.levels[0].cellar_room_id_array[i] = anyRoads[i].map;
        }

        // Find all dungeons and write the recorder data
        var dungeons = maps.Where(m => m.cave < 9).OrderBy(m => m.cave).ToList();
        for(int i = 0; i < dungeons.Count; i++)
        {
            _data.special.recorder_dests[i] = (dungeons[i].map % 0x10 == 0) ? dungeons[i].map + 0x0f : dungeons[i].map - 1;
        }
    }
}
