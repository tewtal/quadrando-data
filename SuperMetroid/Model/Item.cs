namespace Randomizer.Graph.Combo.SuperMetroid.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal record Item(
    string StartingRoom,
    int StartingNode,
    List<string> StartingItems,
    List<string> StartingFlags,
    List<string> StartingLocks,
    List<StartingResource> StartingResources,
    List<string> ImplicitItems,
    List<UpgradeItem> UpgradeItems,
    List<ExpansionItem> ExpansionItems,
    List<string> GameFlags
);

internal record ConsumableResource(string Type);

internal record StartingResource(
    ConsumableResource Resource,
    int MaxAmount
);

internal record UpgradeItem(
    string Name,
    string Data
);

internal record ExpansionItem(
    string Name,
    string Data,
    ConsumableResource Resource,
    int ResourceAmount
);

internal class Items
{
    public static List<Item> Parse(string fileName)
    {
        return new List<Item>();
    }
}
