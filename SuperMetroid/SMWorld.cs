namespace Randomizer.Graph.Combo.SuperMetroid;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using ItemSet = Dictionary<ItemSetName, /* WeightedSet */ Dictionary<int, List<Item>>>;
using WeightedSet = Dictionary<int, List<Item>>;
using PooledItem = (ItemSetName Set, int Weight, Item Item);
using System.Diagnostics.CodeAnalysis;
using static global::Randomizer.Graph.Combo.Zelda.ZeldaYamlReader;

public enum ComplexRequirementType
{
    Always,
    Never,
    Single,
    And,
    Not,
    Or,
    Ammo,
    AmmoDrain,
    Refill,
    PartialRefill,
    EnemyKill,
    AcidFrames,
    GravitylessAcidFrames,
    DraygonElectricityFrames,
    EnemyDamage,
    HeatFrames,
    HeatFramesWithEnergyDrops,
    GravitylessHeatFrames,
    HibashiHits,
    LavaFrames,
    GravitylessLavaFrames,
    SamusEaterFrames,
    MetroidFrames,
    EnergyAtMost,
    AutoReserveTrigger,
    SpikeHits,
    ThornHits,
    DoorUnlockedAtNode,
    ObstaclesCleared,
    ObstaclesNotCleared,
    ResourceCapacity,
    ResourceAvailable,
    ResourceMissingAtMost,
    CanShineCharge,
    GetBlueSpeed,
    SpeedBall,
    Shinespark,
    ResetRoom,
    ItemNotCollectedAtNode,
    GainFlashSuit,
    UseFlashSuit,
    NoFlashSuit,
    Tech
}

// Represents a complex requirement for an edge
// This is more or less a copy of the requirement record from the SM model with some changes
// For example this will resolve item requirements to item objects for faster lookup later
public abstract record ComplexRequirement
{
    public ComplexRequirementType RequirementType { get; init; }
    internal record Always : ComplexRequirement;
    internal record Never : ComplexRequirement;
    internal record Single(Item Item) : ComplexRequirement;
    internal record And(ComplexRequirement[] Reqs) : ComplexRequirement;
    internal record Not(ComplexRequirement Req) : ComplexRequirement;
    internal record Or(ComplexRequirement[] Reqs) : ComplexRequirement;
    internal record Ammo(Item Item, int Count) : ComplexRequirement;
    internal record AmmoDrain(string Type, int Count) : ComplexRequirement;
    internal record Refill(string[] Resources) : ComplexRequirement;
    internal record PartialRefill(string Resources, int Limit) : ComplexRequirement;
    internal record EnemyKill(
        string[][] Enemies,
        string[]? ExplicitWeapons = null,
        string[]? ExcludedWeapons = null,
        string[]? FarmableAmmo = null
    ) : ComplexRequirement
    {
        internal bool CanKillWith(Inventory inventory)
        {
            // Take our flags, and reduce them to the ones that are explicitly weapons, and the ones that are not excluded weapons
            //var filteredFlags = ExplicitWeapons == null ? flags.AsEnumerable() : flags.Where(flags => ExplicitWeapons.Contains(flags.Key));
            //filteredFlags = ExcludedWeapons == null ? filteredFlags : filteredFlags.Where(flags => !ExcludedWeapons.Contains(flags.Key));

            // Go through the enemies, and see if we can kill them with the filtered flags
            foreach (var enemy in Enemies.SelectMany(e => e))
            {
                // If we can't kill this enemy, return false

                // TODO: Implement this
                // if (!CanKillEnemy(enemy, filteredFlags))
                //      return false;
            }

            return true;
        }
    };
    internal record AcidFrames(int Frames) : ComplexRequirement;
    internal record GravitylessAcidFrames(int Frames) : ComplexRequirement;
    internal record DraygonElectricityFrames(int Frames) : ComplexRequirement;
    internal record EnemyDamage(string Enemy, string Type, int Hits) : ComplexRequirement;
    internal record HeatFrames(int Frames) : ComplexRequirement;
    internal record HeatFramesWithEnergyDrops(int Frames, Model.Drop[] Drops) : ComplexRequirement;
    internal record GravitylessHeatFrames(int Frames) : ComplexRequirement;
    internal record HibashiHits(int Hits) : ComplexRequirement;
    internal record LavaFrames(int Frames) : ComplexRequirement;
    internal record GravitylessLavaFrames(int Frames) : ComplexRequirement;
    internal record SamusEaterFrames(int Frames) : ComplexRequirement;
    internal record MetroidFrames(int Frames) : ComplexRequirement;
    internal record EnergyAtMost(int Energy) : ComplexRequirement;
    internal record AutoReserveTrigger(int MinReserveEnergy, int MaxReserveEnergy) : ComplexRequirement;
    internal record SpikeHits(int Hits) : ComplexRequirement;
    internal record ThornHits(int Hits) : ComplexRequirement;
    internal record DoorUnlockedAtNode(int Node) : ComplexRequirement;
    internal record ObstaclesCleared(string[] Obstacles) : ComplexRequirement;
    internal record ObstaclesNotCleared(string[] Obstacles) : ComplexRequirement;
    internal record ResourceCapacity((Item, int)[] Capacity) : ComplexRequirement;
    internal record ResourceAvailable((Item, int)[] Available) : ComplexRequirement;
    internal record ResourceMissingAtMost((Item, int)[] MissingAtMost) : ComplexRequirement;
    internal record CanShineCharge(
        World world,
        decimal UsedTiles,
        decimal OpenEnd,
        decimal? GentleUpTiles = null,
        decimal? GentleDownTiles = null,
        decimal? SteepUpTiles = null,
        decimal? SteepDownTiles = null,
        decimal? StartingDownTiles = null
    ) : ComplexRequirement;
    internal record GetBlueSpeed(
        World world,
        decimal UsedTiles,
        decimal OpenEnd,
        decimal? GentleUpTiles = null,
        decimal? GentleDownTiles = null,
        decimal? SteepUpTiles = null,
        decimal? SteepDownTiles = null,
        decimal? StartingDownTiles = null
    ) : ComplexRequirement;
    internal record SpeedBall(
        World world,
        decimal Length,
        decimal OpenEnd,
        decimal? GentleUpTiles = null,
        decimal? GentleDownTiles = null,
        decimal? SteepUpTiles = null,
        decimal? SteepDownTiles = null,
        decimal? StartingDownTiles = null
    ) : ComplexRequirement;
    internal record Shinespark(int Frames, int? ExcessFrames = null) : ComplexRequirement;
    internal record ResetRoom(
        int[] Nodes,
        int[]? NodesToAvoid = null,
        bool? MustStayPut = null
    ) : ComplexRequirement;
    internal record ItemNotCollectedAtNode(int Node) : ComplexRequirement;
    internal record GainFlashSuit() : ComplexRequirement;
    internal record UseFlashSuit() : ComplexRequirement;
    internal record NoFlashSuit() : ComplexRequirement;
    internal record Tech(Item TechRequirement) : ComplexRequirement;

    public ComplexRequirement()
    {
        RequirementType = this switch
        {
            Always _ => ComplexRequirementType.Always,
            Never _ => ComplexRequirementType.Never,
            Single _ => ComplexRequirementType.Single,
            And _ => ComplexRequirementType.And,
            Not _ => ComplexRequirementType.Not,
            Or _ => ComplexRequirementType.Or,
            Ammo _ => ComplexRequirementType.Ammo,
            AmmoDrain _ => ComplexRequirementType.AmmoDrain,
            Refill _ => ComplexRequirementType.Refill,
            PartialRefill _ => ComplexRequirementType.PartialRefill,
            EnemyKill _ => ComplexRequirementType.EnemyKill,
            AcidFrames _ => ComplexRequirementType.AcidFrames,
            GravitylessAcidFrames _ => ComplexRequirementType.GravitylessAcidFrames,
            DraygonElectricityFrames _ => ComplexRequirementType.DraygonElectricityFrames,
            EnemyDamage _ => ComplexRequirementType.EnemyDamage,
            HeatFrames _ => ComplexRequirementType.HeatFrames,
            HeatFramesWithEnergyDrops _ => ComplexRequirementType.HeatFramesWithEnergyDrops,
            GravitylessHeatFrames _ => ComplexRequirementType.GravitylessHeatFrames,
            HibashiHits _ => ComplexRequirementType.HibashiHits,
            LavaFrames _ => ComplexRequirementType.LavaFrames,
            GravitylessLavaFrames _ => ComplexRequirementType.GravitylessLavaFrames,
            SamusEaterFrames _ => ComplexRequirementType.SamusEaterFrames,
            MetroidFrames _ => ComplexRequirementType.MetroidFrames,
            EnergyAtMost _ => ComplexRequirementType.EnergyAtMost,
            AutoReserveTrigger _ => ComplexRequirementType.AutoReserveTrigger,
            SpikeHits _ => ComplexRequirementType.SpikeHits,
            ThornHits _ => ComplexRequirementType.ThornHits,
            DoorUnlockedAtNode _ => ComplexRequirementType.DoorUnlockedAtNode,
            ObstaclesCleared _ => ComplexRequirementType.ObstaclesCleared,
            ObstaclesNotCleared _ => ComplexRequirementType.ObstaclesNotCleared,
            ResourceCapacity _ => ComplexRequirementType.ResourceCapacity,
            ResourceAvailable _ => ComplexRequirementType.ResourceAvailable,
            ResourceMissingAtMost _ => ComplexRequirementType.ResourceMissingAtMost,
            CanShineCharge _ => ComplexRequirementType.CanShineCharge,
            GetBlueSpeed _ => ComplexRequirementType.GetBlueSpeed,
            SpeedBall _ => ComplexRequirementType.SpeedBall,
            Shinespark _ => ComplexRequirementType.Shinespark,
            ResetRoom _ => ComplexRequirementType.ResetRoom,
            ItemNotCollectedAtNode _ => ComplexRequirementType.ItemNotCollectedAtNode,
            GainFlashSuit _ => ComplexRequirementType.GainFlashSuit,
            UseFlashSuit _ => ComplexRequirementType.UseFlashSuit,
            NoFlashSuit _ => ComplexRequirementType.NoFlashSuit,
            Tech _ => ComplexRequirementType.Tech,
            _ => throw new NotImplementedException()
        };
    }

    public static ComplexRequirement FromRequirement(World world, Model.Requirement requirement)
    {
        return requirement switch
        {
            Model.Requirement.Always => new Always(),
            Model.Requirement.Never => new Never(),
            Model.Requirement.Single single => new Single(world.GetItem("SM" + single.Req, Game.SuperMetroid)),
            Model.Requirement.And and => and.Reqs.Length == 0 ? new Always() : (and.Reqs.Length == 1 ? FromRequirement(world, and.Reqs.First()) : new And(and.Reqs.Select(r => FromRequirement(world, r)).ToArray())),
            Model.Requirement.Not not => new Not(FromRequirement(world, not.Req)),
            Model.Requirement.Or or => or.Reqs.Length == 0 ? new Always() : (or.Reqs.Length == 1 ? FromRequirement(world, or.Reqs.First()) : new Or(or.Reqs.Select(r => FromRequirement(world, r)).ToArray())),
            Model.Requirement.Ammo ammo => new Ammo(world.GetItem("SM" + ammo.Type, Game.SuperMetroid), ammo.Count),
            Model.Requirement.AmmoDrain ammoDrain => new AmmoDrain(ammoDrain.Type, ammoDrain.Count),
            Model.Requirement.Refill refill => new Refill(refill.Resources),
            Model.Requirement.PartialRefill partialRefill => new PartialRefill(partialRefill.Resources, partialRefill.Limit),
            Model.Requirement.EnemyKill enemyKill => new EnemyKill(enemyKill.Enemies, enemyKill.ExplicitWeapons, enemyKill.ExcludedWeapons, enemyKill.FarmableAmmo),
            Model.Requirement.AcidFrames acidFrames => new AcidFrames(acidFrames.Frames),
            Model.Requirement.GravitylessAcidFrames gravitylessAcidFrames => new GravitylessAcidFrames(gravitylessAcidFrames.Frames),
            Model.Requirement.DraygonElectricityFrames draygonElectricityFrames => new DraygonElectricityFrames(draygonElectricityFrames.Frames),
            Model.Requirement.EnemyDamage enemyDamage => new EnemyDamage(enemyDamage.Enemy, enemyDamage.Type, enemyDamage.Hits),
            Model.Requirement.HeatFrames heatFrames => new Single(world.GetItem("SMVaria", Game.SuperMetroid)), //new HeatFrames(heatFrames.Frames),
            Model.Requirement.HeatFramesWithEnergyDrops heatFramesWithEnergyDrops => new Single(world.GetItem("SMVaria", Game.SuperMetroid)),  //new HeatFramesWithEnergyDrops(heatFramesWithEnergyDrops.Frames, heatFramesWithEnergyDrops.Drops.Select(d => new Model.Drop(d.Type, d.Amount)).ToArray()),
            Model.Requirement.GravitylessHeatFrames gravitylessHeatFrames => new Single(world.GetItem("SMVaria", Game.SuperMetroid)), // new GravitylessHeatFrames(gravitylessHeatFrames.Frames),
            Model.Requirement.HibashiHits hibashiHits => new HibashiHits(hibashiHits.Hits),
            Model.Requirement.LavaFrames lavaFrames => new And([new Single(world.GetItem("SMGravity", Game.SuperMetroid)), new Single(world.GetItem("SMVaria", Game.SuperMetroid))]),
            Model.Requirement.GravitylessLavaFrames gravitylessLavaFrames => new GravitylessLavaFrames(gravitylessLavaFrames.Frames),
            Model.Requirement.SamusEaterFrames samusEaterFrames => new SamusEaterFrames(samusEaterFrames.Frames),
            Model.Requirement.MetroidFrames metroidFrames => new MetroidFrames(metroidFrames.Frames),
            Model.Requirement.EnergyAtMost energyAtMost => new EnergyAtMost(energyAtMost.Energy),
            Model.Requirement.AutoReserveTrigger autoReserveTrigger => new AutoReserveTrigger(autoReserveTrigger.MinReserveEnergy, autoReserveTrigger.MaxReserveEnergy),
            Model.Requirement.SpikeHits spikeHits => new SpikeHits(spikeHits.Hits),
            Model.Requirement.ThornHits thornHits => new ThornHits(thornHits.Hits),
            Model.Requirement.DoorUnlockedAtNode doorUnlockedAtNode => new DoorUnlockedAtNode(doorUnlockedAtNode.Node),
            Model.Requirement.ObstaclesCleared obstaclesCleared => new ObstaclesCleared(obstaclesCleared.Obstacles),
            Model.Requirement.ObstaclesNotCleared obstaclesNotCleared => new ObstaclesNotCleared(obstaclesNotCleared.Obstacles),
            Model.Requirement.ResourceCapacity resourceCapacity => new ResourceCapacity(
                resourceCapacity.Capacity.Select(c => c.Type switch
                {
                    "Missile" => (world.GetItem("SMMissile", Game.SuperMetroid), (int)Math.Ceiling(c.Count / 5m)),
                    "Super" => (world.GetItem("SMSuper", Game.SuperMetroid), (int)Math.Ceiling(c.Count / 5m)),
                    "PowerBomb" => (world.GetItem("SMPowerBomb", Game.SuperMetroid), (int)Math.Ceiling(c.Count / 5m)),
                    "RegularEnergy" => (world.GetItem("SMETank", Game.SuperMetroid), (int)Math.Ceiling(c.Count / 100m)),
                    "ReserveEnergy" => (world.GetItem("SMReserveTank", Game.SuperMetroid), (int)Math.Ceiling(c.Count / 100m)),
                    _ => throw new NotImplementedException()
                }).ToArray()
            ),
            Model.Requirement.ResourceAvailable resourceAvailable => new Never(),
            Model.Requirement.ResourceMissingAtMost resourceMissingAtMost => new Never(),
            Model.Requirement.CanShineCharge canShineCharge => new CanShineCharge(world, canShineCharge.UsedTiles, canShineCharge.OpenEnd, canShineCharge.GentleUpTiles, canShineCharge.GentleDownTiles, canShineCharge.SteepUpTiles, canShineCharge.SteepDownTiles, canShineCharge.StartingDownTiles),
            Model.Requirement.GetBlueSpeed getBlueSpeed => new Never(), //new GetBlueSpeed(world, getBlueSpeed.UsedTiles, getBlueSpeed.OpenEnd, getBlueSpeed.GentleUpTiles, getBlueSpeed.GentleDownTiles, getBlueSpeed.SteepUpTiles, getBlueSpeed.SteepDownTiles, getBlueSpeed.StartingDownTiles),
            Model.Requirement.SpeedBall speedBall => new Never(),//new SpeedBall(world, speedBall.Length, speedBall.OpenEnd, speedBall.GentleUpTiles, speedBall.GentleDownTiles, speedBall.SteepUpTiles, speedBall.SteepDownTiles, speedBall.StartingDownTiles),
            Model.Requirement.Shinespark shinespark => new Shinespark(shinespark.Frames, shinespark.ExcessFrames),
            Model.Requirement.ResetRoom resetRoom => new ResetRoom(resetRoom.Nodes, resetRoom.NodesToAvoid, resetRoom.MustStayPut),
            Model.Requirement.ItemNotCollectedAtNode itemNotCollectedAtNode => new ItemNotCollectedAtNode(itemNotCollectedAtNode.Node),
            Model.Requirement.GainFlashSuit gainFlashSuit => new Always(),
            Model.Requirement.UseFlashSuit useFlashSuit => new Never(),
            Model.Requirement.NoFlashSuit noFlashSuit => new Always(),
            Model.Requirement.Tech tech => new Single(world.GetItem("SM" + tech.TechRequirement, Game.SuperMetroid)),
            _ => throw new NotImplementedException()
        };
    }

    internal bool Check(Inventory inventory)
    {
        if (RequirementType is ComplexRequirementType.And)
        {
            foreach (var req in (this as And)!.Reqs)
            {
                if (!req.Check(inventory))
                    return false;
            }

            return true;
        }
        else if (RequirementType is ComplexRequirementType.Or)
        {
            foreach (var req in (this as Or)!.Reqs)
            {
                if (req.Check(inventory))
                    return true;
            }

            return false;
        }
        else if (RequirementType is ComplexRequirementType.Single)
        {
            return inventory.Has((this as Single)!.Item);
        }
        else if (RequirementType is ComplexRequirementType.Not)
        {
            return !((this as Not)!.Req.Check(inventory));
        }
        else if (RequirementType is ComplexRequirementType.Ammo)
        {
            return inventory.HasAtLeast((this as Ammo)!.Item, (int)Math.Ceiling((this as Ammo)!.Count / 5m));
        }
        else
        {
            return this.RequirementType switch
            {
                ComplexRequirementType.Always => true,
                ComplexRequirementType.Never => false,
                //ComplexRequirementType.Single when this is Single req => inventory.Has(req.Item),
                //ComplexRequirementType.And when this is And reqs => reqs.Reqs.All(req => req.Check(inventory)),
                //ComplexRequirementType.Not when this is Not req => !req.Req.Check(inventory),
                //ComplexRequirementType.Or when this is Or reqs => reqs.Reqs.Any(req => req.Check(inventory)),
                //ComplexRequirementType.Ammo when this is Ammo ammo => inventory.HasAtLeast(ammo.Item, ammo.Count / 5),
                ComplexRequirementType.AmmoDrain => true,
                ComplexRequirementType.Refill => true,
                ComplexRequirementType.PartialRefill => true,
                ComplexRequirementType.EnemyKill when this is EnemyKill enemies => enemies.CanKillWith(inventory),
                ComplexRequirementType.AcidFrames => false,
                ComplexRequirementType.GravitylessAcidFrames => false,
                ComplexRequirementType.DraygonElectricityFrames => true,
                ComplexRequirementType.EnemyDamage => true,
                ComplexRequirementType.HeatFrames => false,
                ComplexRequirementType.GravitylessHeatFrames => false,
                ComplexRequirementType.HibashiHits => true,
                ComplexRequirementType.LavaFrames => false,
                ComplexRequirementType.GravitylessLavaFrames => false,
                ComplexRequirementType.SamusEaterFrames => true,
                ComplexRequirementType.MetroidFrames => true,
                ComplexRequirementType.EnergyAtMost => true,
                ComplexRequirementType.AutoReserveTrigger => false,
                ComplexRequirementType.SpikeHits => true,
                ComplexRequirementType.ThornHits => true,
                ComplexRequirementType.DoorUnlockedAtNode => false,
                ComplexRequirementType.ObstaclesCleared => true,
                ComplexRequirementType.ObstaclesNotCleared => true,
                ComplexRequirementType.ResourceCapacity when this is ResourceCapacity capacity => capacity.Capacity.All(c => inventory.HasAtLeast(c.Item1, c.Item2)),
                ComplexRequirementType.CanShineCharge when this is CanShineCharge usedTiles => usedTiles.UsedTiles switch
                {
                    33 => inventory.Has(usedTiles.world.GetItem("SMcanShinespark")),
                    _ => false,
                },
                ComplexRequirementType.Shinespark => true,
                ComplexRequirementType.ResetRoom => false,
                ComplexRequirementType.ItemNotCollectedAtNode => false,
                _ => throw new NotImplementedException()
            };
        }
    }

    internal bool IsUnconditional()
    {
        // Returns true if this requirement passes with a blank inventory
        return this switch
        {
            Always => true,
            Never => false,
            Single req => false,
            And reqs => reqs.Reqs.All(req => req.IsUnconditional()),
            Not req => false,
            Or reqs => reqs.Reqs.Any(req => req.IsUnconditional()),
            Ammo ammo => false,
            AmmoDrain ammo => false,
            Refill resources => false,
            PartialRefill resources => false,
            EnemyKill enemies => false,
            AcidFrames frames => false,
            GravitylessAcidFrames frames => false,
            DraygonElectricityFrames frames => false,
            EnemyDamage enemy => false,
            HeatFrames frames => false,
            HeatFramesWithEnergyDrops frames => false,
            GravitylessHeatFrames frames => false,
            HibashiHits hits => false,
            LavaFrames frames => false,
            GravitylessLavaFrames frames => false,
            SamusEaterFrames frames => false,
            MetroidFrames frames => false,
            EnergyAtMost energy => false,
            AutoReserveTrigger minMax => false,
            SpikeHits hits => false,
            ThornHits hits => false,
            DoorUnlockedAtNode node => false,
            ObstaclesCleared obstacles => false,
            ObstaclesNotCleared obstacles => false,
            ResourceCapacity capacity => false,
            ResourceAvailable available => false,
            ResourceMissingAtMost missing => false,
            CanShineCharge usedTiles => false,
            GetBlueSpeed usedTiles => false,
            SpeedBall length => false,
            Shinespark frames => false,
            ResetRoom nodes => false,
            ItemNotCollectedAtNode node => false,
            GainFlashSuit => false,
            UseFlashSuit => false,
            NoFlashSuit => false,
            Tech tech => false,
            _ => throw new NotImplementedException()
        };
    }

    internal ComplexRequirement Condense()
    {
        // Condenses a ComplexRequirement into its simplest form
        // For example, an And with a single child is the same as the child
        // An Or with a single child is the same as the child
        // Childs that are "always" can be removed
        // And with a single child that is "never" can be condensed to "never"
        // Or with a single child that is "always" can be condensed to "always"
        // And with all childs as "always" can be condensed to "always"
        // Or with all childs as "always" can be condensed to "always"

        // Start by replacing disallowed techs with "never" to make sure we prune those conditions

        if (this is Single single && single.Item.Name.StartsWith("SMcan"))
        {
            var techName = single.Item.Name.Substring(2);
            if (!SMWorld.allowedTechs.Contains(techName))
            {
                return new Never();
            }
        }

        if(this is Single singleNever && singleNever.Item.Name.ToLower() == "smnever")
        {
            return new Never();
        }

        if (IsUnconditional())
        {
            return new Always();
        }

        if (this is And and)
        {
            var condensedAnd = and.Reqs.Select(r => r.Condense()).ToArray();
            if (condensedAnd.All(r => r is Always))
            {
                return new Always();
            }

            if (condensedAnd.Any(r => r is Never))
            {
                return new Never();
            }

            // Remove always conditions, since they don't affect the outcome
            condensedAnd = condensedAnd.Where(r => !(r is Always)).ToArray();

            if (condensedAnd.Length == 1)
            {
                return condensedAnd.First();
            }

            if (condensedAnd.Length == 0)
            {
                return new Always();
            }

            return new And(condensedAnd);
        }

        if (this is Or or)
        {
            var condensedOr = or.Reqs.Select(r => r.Condense()).ToArray();
            if (condensedOr.Any(r => r is Always))
            {
                return new Always();
            }

            if (condensedOr.All(r => r is Never))
            {
                return new Never();
            }

            // Remove never conditions, since they don't affect the outcome
            condensedOr = condensedOr.Where(r => !(r is Never)).ToArray();

            if (condensedOr.Length == 1)
            {
                return condensedOr.First();
            }

            if (condensedOr.Length == 0)
            {
                return new Always();
            }

            return new Or(condensedOr);
        }

        return this;

    }
}

internal class SMWorld
{
    public static string[] allowedTechs = [
            "canMidAirMorph",
            "canUseGrapple",
            "canCrouchJump",
            "canWalljump",
            "canUnmorphBombBoost",
            "canIBJ",
            "canJumpIntoIBJ",
            "canShinespark",
            "canHorizontalShinespark",
            "canMidairShinespark",
            "canShinechargeMovement",
            "canUseSpeedEchoes",
            "canAwakenZebes",
            "canCarefulJump",
            "canDisableEquipment",
            "canDownGrab",
            "canTrivialMidAirMorph",
            "canMidAirMorph",
            "canConsecutiveWalljump",
            "canJumpIntoIBJ",
            "canBombAboveIBJ",
            "canPseudoScrew",
        ];

    public static void AdjustWorld(World world)
    {
        var jsonReader = new SMJsonReader();
        jsonReader.Load();
        jsonReader.BuildGraph(world);

        var smVertices = jsonReader.GetVertices(world);

        // World built and connected, now place it into the actual main graph
        foreach (var vtx in smVertices)
        {
            var name = vtx.TryGetValue("name", out object? nameValue) ? (string)nameValue : throw new InvalidDataException("SM vertex without a name");
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
                Item = item != null ? world.GetItem("SM" + item, Game.SuperMetroid) : null,
                ItemSet = itemset?.Select(i => new ItemSetName(i, world)).ToArray() ?? [],
                Addresses = address != null ? [(long)address.Value, (long)address.Value + 1, (long)address.Value + 5] : null,
                Game = Game.SuperMetroid
            };

            world.Graph.AddVertex(vertex);
            //Console.WriteLine($"Added vertex {vertex.Name}");
        }

        var smEdges = jsonReader.GetEdges(world);
        foreach (var edgeCollection in smEdges)
        {
            //var edgeCollectionData = edgeCollection.Key.Split(":").First().Split('|');
            //var requirementName = edgeCollectionData.First();
            //if (!requirementName.StartsWith("fixed"))
            //{
            //    requirementName = "SM" + requirementName;
            //}
            //var requirement = world.GetItem(requirementName, Game.SuperMetroid);
            //var requirementCount = int.Parse(edgeCollectionData.Skip(1).FirstOrDefault() ?? "1");

            var complexRequirement = ComplexRequirement.FromRequirement(world, edgeCollection.Key).Condense();
            
            // Simplify requirements from complex requirements to simple item conditions whenever possible for faster lookup
            var convertedRequirement = complexRequirement switch
            {
                ComplexRequirement.Always => new ItemCondition(world.GetItem("fixed"), 1),
                ComplexRequirement.Never => new ItemCondition(world.GetItem("never"), 1),
                ComplexRequirement.Single single => new ItemCondition(single.Item, 1),
                ComplexRequirement.Ammo ammo => new ItemCondition(ammo.Item, (int)Math.Ceiling(ammo.Count / 5m)),
                _ => new ItemCondition(world.GetItem("SMComplexRequirement", Game.SuperMetroid), 1, complexRequirement)
            };

            if (convertedRequirement.Item.Name == "never")
            {
                continue;
            }

            foreach (var edges in edgeCollection.Value.Directed)
            {
                var from = world.GetLocation(edges[0]);
                var to = world.GetLocation(edges[1]);
                if (from is null || to is null)
                {
                    throw new Exception("Name Connection Mismatch: " + $"({edges[0]}, {edges[1]}) => " + $"({from}, {to})");
                }

                world.Graph.AddDirected(from, to, convertedRequirement);
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

                world.Graph.AddDirected(from, to, convertedRequirement);
                world.Graph.AddDirected(to, from, convertedRequirement);
                //Console.WriteLine($"Added undirected edge from {from.Name} to {to.Name}");
            }
        }

        // Create a SM Meta node and hook up the helpers
        var smMeta = new Vertex()
        {
            World = world,
            Name = "SM - Meta",
            Type = VertexType.Meta,
            Game = Game.SuperMetroid
        };

        world.Graph.AddVertex(smMeta);

        foreach (var helper in jsonReader.Helpers.HelperCategories.SelectMany(h => h.Helpers))
        {
            var helperVertex = new Vertex()
            {
                World = world,
                Name = $"SM - Helper - {helper.Name}",
                Type = VertexType.Meta,
                Item = world.GetItem("SM" + helper.Name, Game.SuperMetroid),
                Game = Game.SuperMetroid
            };

            var complexRequirement = ComplexRequirement.FromRequirement(world, helper.Requires).Condense();

            // Simplify requirements from complex requirements to simple item conditions whenever possible for faster lookup
            var helperRequirement = complexRequirement switch
            {
                ComplexRequirement.Always => new ItemCondition(world.GetItem("fixed"), 1),
                ComplexRequirement.Never => new ItemCondition(world.GetItem("never"), 1),
                ComplexRequirement.Single single => new ItemCondition(single.Item, 1),
                ComplexRequirement.Ammo ammo => new ItemCondition(ammo.Item, (int)Math.Ceiling(ammo.Count / 5m)),
                _ => new ItemCondition(world.GetItem("SMComplexRequirement", Game.SuperMetroid), 1, complexRequirement)
            };

            world.Graph.AddVertex(helperVertex);
            world.Graph.AddDirected(smMeta, helperVertex, helperRequirement);
        }

        // Hook up SM techs
        foreach (var tech in jsonReader.Techs.TechCategories.SelectMany(t => t.Techs))
        {
            AddTech(world, smMeta, tech);
        }

        // Connect SM to the main world graph
        world.Graph.AddDirected(world.GetLocation("start"), world.GetLocation("SM - Meta"), world.GetItem("fixed"));
        world.Graph.AddDirected(world.GetLocation("start"), world.GetLocation("SM - Crateria - Landing Site - Bottom Left Door"), world.GetItem("fixed"));

        // Add undirected path between the games
        //world.Graph.AddDirected(world.GetLocation("Lake Hylia North West Shore"), world.GetLocation("SM - Crateria - Parlor and Alcatraz - Bottom Right Door (On the Left Shaft)"), world.GetItem("fixed"));
        //world.Graph.AddDirected(world.GetLocation("SM - Crateria - Parlor and Alcatraz - Bottom Right Door (On the Left Shaft)"), world.GetLocation("Lake Hylia North West Shore"), world.GetItem("fixed"));

        // Add norfair map to death moutain portal
        world.Graph.AddDirected(world.GetLocation("SM - Norfair - Business Center - Middle Left Door"), world.GetLocation("West Death Mountain"), world.GetItem("fixed"));
        world.Graph.AddDirected(world.GetLocation("West Death Mountain"), world.GetLocation("SM - Norfair - Business Center - Middle Left Door"), world.GetItem("fixed"));

        // Add maridia missile refill to dark world shopping mall
        world.Graph.AddDirected(world.GetLocation("SM - Maridia - Halfie Climb Room - Bottom Right Door"), world.GetLocation("Dark Shopping Mall"), world.GetItem("fixed"));
        world.Graph.AddDirected(world.GetLocation("Dark Shopping Mall"), world.GetLocation("SM - Maridia - Halfie Climb Room - Bottom Right Door"), world.GetItem("fixed"));

        // Add lower norfair refill to mire area
        world.Graph.AddDirected(world.GetLocation("SM - Norfair - Screw Attack Room - Middle Right Door"), world.GetLocation("Mire"), world.GetItem("fixed"));
        world.Graph.AddDirected(world.GetLocation("Mire"), world.GetLocation("SM - Norfair - Screw Attack Room - Middle Right Door"), world.GetItem("fixed"));
        


        // Patch maridia main street (since we're cheating with shinespark nodes)
        world.Graph.AddDirected(
            world.GetLocation("SM - Maridia - Main Street - Bottom Door"), 
            world.GetLocation("SM - Maridia - Main Street - Speed Blocked Item"), 
            new ItemCondition(world.GetItem("SMComplexRequirement", Game.SuperMetroid), 1, new ComplexRequirement.And(
                [
                    new ComplexRequirement.Single(world.GetItem("SMcanShinespark", Game.SuperMetroid)),
                    new ComplexRequirement.Single(world.GetItem("SMSpeedBooster", Game.SuperMetroid)), 
                    new ComplexRequirement.Single(world.GetItem("SMGravity", Game.SuperMetroid))
                ]
            ))
        );

        world.StartingItems.AddItem(world.GetItem("SMf_ZebesAwake"), 1);

    }

    private static void AddTech(World world, Vertex meta, Model.Tech tech)
    {

        if (!allowedTechs.Contains(tech.Name))
        {
            return;
        }

        var techVertex = new Vertex()
        {
            World = world,
            Name = $"SM - Tech - {tech.Name}",
            Type = VertexType.Meta,
            Item = world.GetItem("SM" + tech.Name, Game.SuperMetroid),
            Game = Game.SuperMetroid
        };

        var techRequirement = ComplexRequirement.FromRequirement(world, new Model.Requirement.And([tech.TechRequires, tech.OtherRequires])).Condense();

        // Simplify requirements from complex requirements to simple item conditions whenever possible for faster lookup
        var helperRequirement = techRequirement switch
        {
            ComplexRequirement.Always => new ItemCondition(world.GetItem("fixed"), 1),
            ComplexRequirement.Never => new ItemCondition(world.GetItem("never"), 1),
            ComplexRequirement.Single single => new ItemCondition(single.Item, 1),
            ComplexRequirement.Ammo ammo => new ItemCondition(ammo.Item, (int)Math.Ceiling(ammo.Count / 5m)),
            _ => new ItemCondition(world.GetItem("SMComplexRequirement", Game.SuperMetroid), 1, techRequirement)
        };

        world.Graph.AddVertex(techVertex);
        world.Graph.AddDirected(meta, techVertex, helperRequirement);

        foreach (var extTech in tech.ExtensionTechs ?? [])
        {
            AddTech(world, meta, extTech);
        }

    }

    public static PooledItem[] GetItemSet(World world)
    {
        return
        [
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SMBombs", Game.SuperMetroid)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SMVaria", Game.SuperMetroid)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SMGravity", Game.SuperMetroid)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SMCharge", Game.SuperMetroid)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SMIce", Game.SuperMetroid)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SMWave", Game.SuperMetroid)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SMPlasma", Game.SuperMetroid)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SMSpazer", Game.SuperMetroid)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SMXRayScope", Game.SuperMetroid)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SMGrapple", Game.SuperMetroid)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SMSpringBall", Game.SuperMetroid)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SMScrewAttack", Game.SuperMetroid)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SMHiJump", Game.SuperMetroid)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SMSpaceJump", Game.SuperMetroid)),
            new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SMSpeedBooster", Game.SuperMetroid)),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SMPowerBomb", Game.SuperMetroid)), 3),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SMSuper", Game.SuperMetroid)), 3),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SMMissile", Game.SuperMetroid)), 8),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SMETank", Game.SuperMetroid)), 5),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 3, world.GetItem("SMReserveTank", Game.SuperMetroid)), 4),

            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("SMMissile", Game.SuperMetroid)), 32),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("SMSuper", Game.SuperMetroid)), 12),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("SMPowerBomb", Game.SuperMetroid)), 7),
            .. Enumerable.Repeat(new PooledItem(ItemSetName.DefaultSet, 9001, world.GetItem("SMETank", Game.SuperMetroid)), 9),

            new PooledItem(new ItemSetName("lw", world), 4, world.GetItem("SMMorph", Game.SuperMetroid))


        ];

    }
}
