namespace Randomizer.Graph.Combo.Zelda;

using global::Randomizer.RomModifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal static class ZeldaWorldWriter
{
    private static ReadOnlySpan<byte> ToBytes(this IEnumerable<int> data)
    {
        return data.Select(x => (byte)x).ToArray();
    }

    public static void WriteZeldaWorld(this Rom rom, World world, PRNG prng)
    {
        var data = world.ZeldaData.Data!;
        if (world.Config.Z1EntranceShuffle != Z1EntranceShuffleOption.None)
        {
            WriteOverworldMapData(rom, world, prng, data);
            WriteSpecial(rom, world, prng, data);
        }
    }

    public static void WriteSpecial(Rom rom, World world, PRNG prng, ZeldaYamlReader.YamlData data)
    {
        // Don't write the item ids, they're written elsewhere by the randomizer

        var special = data.special;
        rom.Write(0x62b89a, [(byte)special.overworld_item_room], 0);
        rom.Write(0x62b88e, [(byte)special.overworld_item_x], 0);
        //rom.Write(0x62b88a, [(byte)special.overworld_item_id], 0);
        rom.Write(0x620cb2, [(byte)special.armos_item_room], 0);
        rom.Write(0x620cb9, [(byte)special.armos_item_x], 0);
        //rom.Write(0x620cf5, [(byte)special.armos_item_id], 0);
        rom.Write(0x620cb3, special.armos_stairs.ToBytes(), 0);
        rom.Write(0x620cba, special.armos_x_pos.ToBytes(), 0);
        rom.Write(0x63b20d, special.step_ladder.ToBytes(), 0);
        rom.Write(0x63af66, special.recorder_stairs.ToBytes(), 0);
        rom.Write(0x60A010, special.recorder_dests.ToBytes(), 0);
        rom.Write(0x60A119, special.recorder_y_pos.ToBytes(), 0);
        rom.Write(0x628F35, [(byte)special.any_road_x_pos[0]], 0);
        rom.Write(0x628F3A, [(byte)special.any_road_x_pos[1]], 0);
        rom.Write(0x628F3F, [(byte)special.any_road_x_pos[2]], 0);
        // Missing start


        // This should probably go into its own WriteLevelData later when it's complete
        // Write the any road targets back to the ROM
        rom.Write(0x631334, data.levels[0].cellar_room_id_array.ToBytes(), 0);

    }

    public static void WriteOverworldMapData(Rom rom, World world, PRNG prng, ZeldaYamlReader.YamlData data)
    {
        /* Write out overworld map data */
        foreach (var map in data.overworld_maps.Where(x => x.name != "Meta"))
        {
            var levelInfoA_offset = 0x630400 + map.map;
            var levelInfoB_offset = 0x630480 + map.map;
            var levelInfoC_offset = 0x630500 + map.map;
            var levelInfoD_offset = 0x630580 + map.map;
            var levelInfoE_offset = 0x630600 + map.map;
            var levelInfoF_offset = 0x630680 + map.map;

            byte levelInfoA_data = (byte)(map.zora ? 0x08 : 0x00);
            levelInfoA_data |= (byte)(map.waves ? 0x04 : 0x00);
            levelInfoA_data |= (byte)map.palettes[1];
            levelInfoA_data |= (byte)(map.exit[0] << 4);

            byte levelInfoB_data = (byte)map.palettes[0];
            levelInfoB_data |= (byte)(map.cave << 2);

            byte levelInfoC_data = (byte)(map.enemies << 6);
            levelInfoC_data |= (byte)map.enemy_id;

            byte levelInfoD_data = (byte)(map.enemy_mode << 7);
            levelInfoD_data |= (byte)map.screen;

            byte levelInfoE_data = (byte)map.level_info_e;

            byte levelInfoF_data = (byte)(map.secret[1] == 1 ? 0x80 : 0x00);
            levelInfoF_data |= (byte)(map.secret[0] == 1 ? 0x40 : 0x00);
            levelInfoF_data |= (byte)(map.stairs << 4);
            levelInfoF_data |= (byte)(map.enemy_sides << 3);
            levelInfoF_data |= (byte)map.exit[1];

            rom.Write(levelInfoA_offset, [levelInfoA_data], 0);
            rom.Write(levelInfoB_offset, [levelInfoB_data], 0);
            rom.Write(levelInfoC_offset, [levelInfoC_data], 0);
            rom.Write(levelInfoD_offset, [levelInfoD_data], 0);
            rom.Write(levelInfoE_offset, [levelInfoE_data], 0);
            rom.Write(levelInfoF_offset, [levelInfoF_data], 0);

        }
    }

}
