namespace Randomizer.Graph.Combo.SuperMetroid.Model;

using MathNet.Numerics.Optimization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

internal record Room(
    int Id,
    string Name,
    string Area,
    string SubArea,
    string? SubSubArea,
    bool Playable,
    Note? Note,
    Note? DevNote,
    string? RoomAddress,
    RoomEnvironment[]? RoomEnvironments,
    Node[] Nodes,
    Link[] Links,
    Strat[] Strats,
    Obstacle[]? Obstacles,
    RoomEnemy[]? Enemies,
    ReusableNotable[]? ReusableRoomwideNotable
);

internal record ReusableNotable(string Name, Note? Note, Note? DevNote);


internal record Node
(
    int Id,
    string Name,
    string NodeType,
    string NodeSubType,
    string? NodeItem,
    string? NodeAddress,
    DoorEnvironment[]? DoorEnvironments,
    bool? UseImplicitDoorUnlocks,
    Requirement? InteractionRequires,
    int? SpawnAt,
    NodeLock[]? Locks,
    TwinDoorAddress[]? TwinDoorAddresses,
    string[]? Utilities,
    ViewableNode[]? ViewableNodes,
    string[]? Yields,
    Note? Note,
    Note? DevNote
);

internal record TwinDoorAddress(string RoomAddress, string DoorAddress);
internal record ViewableNode(int Id, Strat[] Strats);

internal record NodeLock
(
    string LockType,
    Requirement? Lock,
    string Name,
    Strat[] UnlockStrats,
    Note? Note,
    Note? DevNote,
    string[]? Yields
);

internal record Link
(
    int From,
    LinkTo[] To
);

internal record LinkTo
(
    int Id,
    Note? Note,
    Note? DevNote
);

internal record Obstacle
(
    string Id,
    string Name,
    string ObstacleType,
    Note? Note,
    Note? DevNote
) : IComparable
{
    public int CompareTo([AllowNull] object other)
    {
        if (other == null)
        {
            return 1;
        }

        if (Id == ((Obstacle)other).Id)
        {
            return 0;
        }

        return Id.CompareTo(((Obstacle)other).Id);
    }
};

internal record RoomEnemy
(
    string Id,
    string GroupName,
    string EnemyName,
    int Quantity,
    int[]? HomeNodes,
    int[]? BetweenNodes,
    Requirement? Spawn,
    Requirement? StopSpawn,
    Requirement? DropRequires,
    FarmCycle[]? FarmCycles,
    Note? Note,
    Note? DevNote
);

internal record FarmCycle
(
    string Name,
    int CycleFrames,
    Requirement Requires,
    Note? Note,
    Note? DevNote
);

internal record Runway
(
    int Length,
    int OpenEnd,
    int? GentleUpTiles,
    int? GentleDownTiles,
    int? SteepUpTiles,
    int? SteepDownTiles
);

internal record Strat
(
    int[] Link,
    string Name,
    bool? Notable,
    string? ReusableRoomwideNotable,
    EntranceCondition? EntranceCondition,
    Requirement Requires,
    ExitCondition? ExitCondition,
    object? GModeRegainMobility,
    bool? BypassesDoorShell,
    UnlockDoorItem[]? UnlockDoors,
    string[]? ClearsObstacles,
    string[]? ResetsObstacles,
    StratFailure[]? Failures,
    Note? Note,
    Note? DevNote
);

internal record UnlockDoorItem
(
    int? NodeId,
    string[] Types,
    Requirement? Requires,
    bool? UseImplicitRequires,
    Note? Note,
    Note? DevNote
);

internal record StratFailure
(
    string Name,
    int? LeadsToNode,
    Requirement? Cost,
    bool? Softlock,
    Note? Note,
    Note? DevNote
);

internal record DoorEnvironment
(
    string Physics,
    int[]? EntranceNodes,
    Note? Note,
    Note? DevNote
);

internal record RoomEnvironment
(
    bool Heated,
    List<int>? EntranceNodes,
    Note? Note,
    Note? DevNote
);


[JsonConverter(typeof(EntranceConditionConverter))] 
abstract internal record EntranceCondition
{
    internal record ComeInNormally() : EntranceCondition;
    internal record ComeInRunning(string SpeedBooster, decimal MinTiles, decimal? MaxTiles) : EntranceCondition;
    internal record ComeInJumping(string SpeedBooster, decimal MinTiles, decimal? MaxTiles) : EntranceCondition;
    internal record ComeInSpaceJumping(string SpeedBooster, decimal MinTiles, decimal? MaxTiles) : EntranceCondition;
    internal record ComeInShineCharging(decimal Length, decimal OpenEnd, decimal? GentleUpTiles, decimal? GentleDownTiles, decimal? SteepUpTiles, decimal? SteepDownTiles) : EntranceCondition;
    internal record ComeInGettingBlueSpeed(decimal Length, int OpenEnd, int? GentleUpTiles, int? GentleDownTiles, int? SteepUpTiles, int? SteepDownTiles, string? MinExtraRunSpeed, string? MaxExtraRunSpeed) : EntranceCondition;
    internal record ComeInShineCharged(int FramesRequired) : EntranceCondition;
    internal record ComeInShineChargedJumping(int FramesRequired) : EntranceCondition;
    internal record ComeInWithSpark(string? Position) : EntranceCondition;
    internal record ComeInStutterShineCharging(decimal MinTiles) : EntranceCondition;
    internal record ComeInWithBombBoost() : EntranceCondition;
    internal record ComeInWithDoorStuckSetup() : EntranceCondition;
    internal record ComeInSpeedballing(Runway Runway) : EntranceCondition;
    internal record ComeInWithTemporaryBlue() : EntranceCondition;
    internal record ComeInBlueSpinning(string? MinExtraRunSpeed, string? MaxExtraRunSpeed, decimal UnusableTiles) : EntranceCondition;
    internal record ComeInWithMockball(decimal? AdjacentMinTiles, decimal[][]? RemoteAndLandingMinTiles) : EntranceCondition;
    internal record ComeInWithSpringBallBounce(string MovementType, decimal? AdjacentMinTiles, decimal[][]? RemoteAndLandingMinTiles) : EntranceCondition;
    internal record ComeInWithBlueSpringBallBounce(string MovementType, string? MinExtraRunSpeed, string? MaxExtraRunSpeed, decimal? MinLandingTiles) : EntranceCondition;
    internal record ComeInWithStoredFallSpeed(int FallSpeedInTiles) : EntranceCondition;
    internal record ComeInWithRMode() : EntranceCondition;
    internal record ComeInWithGMode(string Mode, bool Morphed, string? Mobility) : EntranceCondition;
    internal record ComeInWithWallJumpBelow(int MinHeight) : EntranceCondition;
    internal record ComeInWithSpaceJumpBelow() : EntranceCondition;
    internal record ComeInWithPlatformBelow(decimal? MinHeight, decimal? MaxHeight, decimal? MaxLeftPosition, decimal? MinRightPosition) : EntranceCondition;
    internal record ComeInWithGrappleTeleport(int[][] BlockPositions) : EntranceCondition;
    internal record ComesThroughToilet(string comesThroughToilet) : EntranceCondition;
}

internal class EntranceConditionConverter : JsonConverter<EntranceCondition>
{
    public EntranceCondition? ParseElement(JsonElement element)
    {
        var entranceCondition = element.EnumerateObject().First();
        return entranceCondition.Name switch
        {
            "comeInNormally" => new EntranceCondition.ComeInNormally(),
            "comeInRunning" => new EntranceCondition.ComeInRunning(
                entranceCondition.Value.GetProperty("speedBooster").ToString(),
                entranceCondition.Value.GetProperty("minTiles").GetDecimal(),
                entranceCondition.Value.TryGetProperty("maxTiles", out var maxTiles) ? maxTiles.GetDecimal() : null),
            "comeInJumping" => new EntranceCondition.ComeInJumping(
                entranceCondition.Value.GetProperty("speedBooster").ToString(),
                entranceCondition.Value.GetProperty("minTiles").GetDecimal(),
                entranceCondition.Value.TryGetProperty("maxTiles", out var maxTiles) ? maxTiles.GetDecimal() : null),
            "comeInSpaceJumping" => new EntranceCondition.ComeInSpaceJumping(
                entranceCondition.Value.GetProperty("speedBooster").ToString(),
                entranceCondition.Value.GetProperty("minTiles").GetDecimal(),
                entranceCondition.Value.TryGetProperty("maxTiles", out var maxTiles) ? maxTiles.GetDecimal() : null),
            "comeInShinecharging" => new EntranceCondition.ComeInShineCharging(
                entranceCondition.Value.GetProperty("length").GetDecimal(),
                entranceCondition.Value.GetProperty("openEnd").GetDecimal(),
                entranceCondition.Value.TryGetProperty("gentleUpTiles", out var gentleUpTiles) ? gentleUpTiles.GetDecimal() : null,
                entranceCondition.Value.TryGetProperty("gentleDownTiles", out var gentleDownTiles) ? gentleDownTiles.GetDecimal() : null,
                entranceCondition.Value.TryGetProperty("steepUpTiles", out var steepUpTiles) ? steepUpTiles.GetDecimal() : null,
                entranceCondition.Value.TryGetProperty("steepDownTiles", out var steepDownTiles) ? steepDownTiles.GetDecimal() : null),
            "comeInGettingBlueSpeed" => new EntranceCondition.ComeInGettingBlueSpeed(
                entranceCondition.Value.GetProperty("length").GetDecimal(),
                entranceCondition.Value.GetProperty("openEnd").GetInt32(),
                entranceCondition.Value.TryGetProperty("gentleUpTiles", out var gentleUpTiles) ? gentleUpTiles.GetInt32() : null,
                entranceCondition.Value.TryGetProperty("gentleDownTiles", out var gentleDownTiles) ? gentleDownTiles.GetInt32() : null,
                entranceCondition.Value.TryGetProperty("steepUpTiles", out var steepUpTiles) ? steepUpTiles.GetInt32() : null,
                entranceCondition.Value.TryGetProperty("steepDownTiles", out var steepDownTiles) ? steepDownTiles.GetInt32() : null,
                entranceCondition.Value.TryGetProperty("minExtraRunSpeed", out var minExtraRunSpeed) ? minExtraRunSpeed.GetString() : null,
                entranceCondition.Value.TryGetProperty("maxExtraRunSpeed", out var maxExtraRunSpeed) ? maxExtraRunSpeed.GetString() : null),
            "comeInShinecharged" => new EntranceCondition.ComeInShineCharged(entranceCondition.Value.GetProperty("framesRequired").GetInt32()),
            "comeInShinechargedJumping" => new EntranceCondition.ComeInShineChargedJumping(entranceCondition.Value.GetProperty("framesRequired").GetInt32()),
            "comeInWithSpark" => new EntranceCondition.ComeInWithSpark(
                entranceCondition.Value.TryGetProperty("position", out var position) ? position.GetString() : null
            ),
            "comeInStutterShinecharging" => new EntranceCondition.ComeInStutterShineCharging(entranceCondition.Value.GetProperty("minTiles").GetDecimal()),
            "comeInWithBombBoost" => new EntranceCondition.ComeInWithBombBoost(),
            "comeInWithDoorStuckSetup" => new EntranceCondition.ComeInWithDoorStuckSetup(),
            "comeInSpeedballing" => new EntranceCondition.ComeInSpeedballing(
                JsonSerializer.Deserialize<Runway>(entranceCondition.Value.GetProperty("runway").GetRawText()) ?? new Runway(0, 0, null, null, null, null)),
            "comeInWithTemporaryBlue" => new EntranceCondition.ComeInWithTemporaryBlue(),
            "comeInBlueSpinning" => new EntranceCondition.ComeInBlueSpinning(
                entranceCondition.Value.TryGetProperty("minExtraRunSpeed", out var minExtraRunSpeed) ? minExtraRunSpeed.GetString() : null,
                entranceCondition.Value.TryGetProperty("maxExtraRunSpeed", out var maxExtraRunSpeed) ? maxExtraRunSpeed.GetString() : null,
                entranceCondition.Value.GetProperty("unusableTiles").GetDecimal()),
            "comeInWithMockball" => new EntranceCondition.ComeInWithMockball(
                entranceCondition.Value.TryGetProperty("adjacentMinTiles", out var adjacentMinTiles) ? adjacentMinTiles.GetDecimal() : null,
                entranceCondition.Value.TryGetProperty("remoteAndLandingMinTiles", out var remoteAndLandingMinTiles) ? remoteAndLandingMinTiles.EnumerateArray().Select(e => e.EnumerateArray().Select(e => e.GetDecimal()).ToArray()).ToArray() : null),                
            "comeInWithSpringBallBounce" => new EntranceCondition.ComeInWithSpringBallBounce(
                entranceCondition.Value.GetProperty("movementType").GetString() ?? "",
                entranceCondition.Value.TryGetProperty("adjacentMinTiles", out var adjacentMinTiles) ? adjacentMinTiles.GetDecimal() : null,
                entranceCondition.Value.TryGetProperty("remoteAndLandingMinTiles", out var remoteAndLandingMinTiles) ? remoteAndLandingMinTiles.EnumerateArray().Select(e => e.EnumerateArray().Select(e => e.GetDecimal()).ToArray()).ToArray() : null),
            "comeInWithBlueSpringBallBounce" => new EntranceCondition.ComeInWithBlueSpringBallBounce(
                entranceCondition.Value.GetProperty("movementType").GetString() ?? "",
                entranceCondition.Value.TryGetProperty("minExtraRunSpeed", out var minExtraRunSpeed) ? minExtraRunSpeed.GetString() : null,
                entranceCondition.Value.TryGetProperty("maxExtraRunSpeed", out var maxExtraRunSpeed) ? maxExtraRunSpeed.GetString() : null,
                entranceCondition.Value.TryGetProperty("minLandingTiles", out var minLandingTiles) ? minLandingTiles.GetDecimal() : null),
            "comeInWithStoredFallSpeed" => new EntranceCondition.ComeInWithStoredFallSpeed(entranceCondition.Value.GetProperty("fallSpeedInTiles").GetInt32()),
            "comeInWithRMode" => new EntranceCondition.ComeInWithRMode(),
            "comeInWithGMode" => new EntranceCondition.ComeInWithGMode(
                entranceCondition.Value.GetProperty("mode").GetString() ?? "",
                entranceCondition.Value.GetProperty("morphed").GetBoolean(),
                entranceCondition.Value.TryGetProperty("mobility", out var mobility) ? mobility.GetString() : null),
            "comeInWithWallJumpBelow" => new EntranceCondition.ComeInWithWallJumpBelow(entranceCondition.Value.GetProperty("minHeight").GetInt32()),
            "comeInWithSpaceJumpBelow" => new EntranceCondition.ComeInWithSpaceJumpBelow(),
            "comeInWithPlatformBelow" => new EntranceCondition.ComeInWithPlatformBelow(
                entranceCondition.Value.TryGetProperty("minHeight", out var minHeight) ? minHeight.GetDecimal() : null,
                entranceCondition.Value.TryGetProperty("maxHeight", out var maxHeight) ? maxHeight.GetDecimal() : null,
                entranceCondition.Value.TryGetProperty("maxLeftPosition", out var maxLeftPosition) ? maxLeftPosition.GetDecimal() : null,
                entranceCondition.Value.TryGetProperty("minRightPosition", out var minRightPosition) ? minRightPosition.GetDecimal() : null),
            "comeInWithGrappleTeleport" => new EntranceCondition.ComeInWithGrappleTeleport(
                entranceCondition.Value.GetProperty("blockPositions").EnumerateArray().Select(e => e.EnumerateArray().Select(e => e.GetInt32()).ToArray()).ToArray()),
            "comesThroughToilet" => new EntranceCondition.ComesThroughToilet(entranceCondition.Value.GetString() ?? ""),
            _ => throw new Exception($"Unknown entrance condition: {entranceCondition.Name}")
        };
    }

    public override EntranceCondition? Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        return ParseElement(doc.RootElement);
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, EntranceCondition value, System.Text.Json.JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}

[JsonConverter(typeof(ExitConditionConverter))]
abstract internal record ExitCondition
{
    internal record LeaveNormally() : ExitCondition;
    internal record LeaveWithRunway(decimal Length, decimal OpenEnd, decimal? GentleUpTiles, decimal? GentleDownTiles, decimal? SteepUpTiles, decimal? SteepDownTiles, decimal? StartingDownTiles) : ExitCondition;
    internal record LeaveShineCharged(int FramesRequired) : ExitCondition;
    internal record LeaveWithTemporaryBlue(string? Direction) : ExitCondition;
    internal record LeaveWithSpark(string? Position) : ExitCondition;
    internal record LeaveSpinning(Runway RemoteRunway, string? MinExtraRunSpeed, string? MaxExtraRunSpeed, string? Blue) : ExitCondition;
    internal record LeaveWithMockball(Runway RemoteRunway, Runway LandingRunway, string? MinExtraRunSpeed, string? MaxExtraRunSpeed, string? Blue) : ExitCondition;
    internal record LeaveWithSpringBallBounce(Runway RemoteRunway, Runway LandingRunway, string? MinExtraRunSpeed, string? MaxExtraRunSpeed, string? Blue, string MovementType) : ExitCondition;
    internal record LeaveSpaceJumping(Runway RemoteRunway, string? MinExtraRunSpeed, string? MaxExtraRunSpeed, string? Blue) : ExitCondition;
    internal record LeaveWithStoredFallSpeed(int FallSpeedInTiles) : ExitCondition;
    internal record LeaveWithGModeSetup(bool Knockback) : ExitCondition;
    internal record LeaveWithGMode(bool Morphed) : ExitCondition;
    internal record LeaveWithDoorFrameBelow(decimal Height) : ExitCondition;
    internal record LeaveWithPlatformBelow(decimal Height, decimal LeftPosition, decimal RightPosition) : ExitCondition;
    internal record LeaveWithGrappleTeleport(int[][] BlockPositions) : ExitCondition;
}

internal class ExitConditionConverter : JsonConverter<ExitCondition>
{
    public ExitCondition? ParseElement(JsonElement element)
    {
        var exitCondition = element.EnumerateObject().First();
        return exitCondition.Name switch
        {
            "leaveNormally" => new ExitCondition.LeaveNormally(),
            "leaveWithRunway" => new ExitCondition.LeaveWithRunway(
                exitCondition.Value.GetProperty("length").GetDecimal(),
                exitCondition.Value.GetProperty("openEnd").GetDecimal(),
                exitCondition.Value.TryGetProperty("gentleUpTiles", out var gentleUpTiles) ? gentleUpTiles.GetDecimal() : null,
                exitCondition.Value.TryGetProperty("gentleDownTiles", out var gentleDownTiles) ? gentleDownTiles.GetDecimal() : null,
                exitCondition.Value.TryGetProperty("steepUpTiles", out var steepUpTiles) ? steepUpTiles.GetDecimal() : null,
                exitCondition.Value.TryGetProperty("steepDownTiles", out var steepDownTiles) ? steepDownTiles.GetDecimal() : null,
                exitCondition.Value.TryGetProperty("startingDownTiles", out var startingDownTiles) ? startingDownTiles.GetDecimal() : null),
            "leaveShinecharged" => new ExitCondition.LeaveShineCharged(
                exitCondition.Value.GetProperty("framesRemaining").ValueKind switch
                {
                    JsonValueKind.Number => exitCondition.Value.GetProperty("framesRemaining").GetInt32(),
                    JsonValueKind.String => exitCondition.Value.GetProperty("framesRemaining").GetString() switch
                    {
                        "auto" => -1,
                        _ => throw new Exception($"Unknown shinecharged exit condition: {exitCondition.Value.GetProperty("framesRemaining").GetString()}")
                    },
                    _ => throw new Exception($"Unknown shinecharged exit condition: {exitCondition.Value.GetProperty("framesRemaining").GetString()}")
                }
            ),
            "leaveWithTemporaryBlue" => new ExitCondition.LeaveWithTemporaryBlue(
                exitCondition.Value.TryGetProperty("direction", out var direction) ? direction.GetString() : null
            ),
            "leaveWithSpark" => new ExitCondition.LeaveWithSpark(
                exitCondition.Value.TryGetProperty("position", out var position) ? position.GetString() : null
            ),
            "leaveSpinning" => new ExitCondition.LeaveSpinning(
                JsonSerializer.Deserialize<Runway>(exitCondition.Value.GetProperty("remoteRunway").GetRawText()) ?? new Runway(0, 0, null, null, null, null),
                exitCondition.Value.TryGetProperty("minExtraRunSpeed", out var minExtraRunSpeed) ? minExtraRunSpeed.GetString() : null,
                exitCondition.Value.TryGetProperty("maxExtraRunSpeed", out var maxExtraRunSpeed) ? maxExtraRunSpeed.GetString() : null,
                exitCondition.Value.TryGetProperty("blue", out var blue) ? blue.GetString() : null
            ),
            "leaveWithMockball" => new ExitCondition.LeaveWithMockball(
                JsonSerializer.Deserialize<Runway>(exitCondition.Value.GetProperty("remoteRunway").GetRawText()) ?? new Runway(0, 0, null, null, null, null),
                JsonSerializer.Deserialize<Runway>(exitCondition.Value.GetProperty("landingRunway").GetRawText()) ?? new Runway(0, 0, null, null, null, null),
                exitCondition.Value.TryGetProperty("minExtraRunSpeed", out var minExtraRunSpeed) ? minExtraRunSpeed.GetString() : null,
                exitCondition.Value.TryGetProperty("maxExtraRunSpeed", out var maxExtraRunSpeed) ? maxExtraRunSpeed.GetString() : null,
                exitCondition.Value.TryGetProperty("blue", out var blue) ? blue.GetString() : null
            ),
            "leaveWithSpringBallBounce" => new ExitCondition.LeaveWithSpringBallBounce(
                JsonSerializer.Deserialize<Runway>(exitCondition.Value.GetProperty("remoteRunway").GetRawText()) ?? new Runway(0, 0, null, null, null, null),
                JsonSerializer.Deserialize<Runway>(exitCondition.Value.GetProperty("landingRunway").GetRawText()) ?? new Runway(0, 0, null, null, null, null),
                exitCondition.Value.TryGetProperty("minExtraRunSpeed", out var minExtraRunSpeed) ? minExtraRunSpeed.GetString() : null,
                exitCondition.Value.TryGetProperty("maxExtraRunSpeed", out var maxExtraRunSpeed) ? maxExtraRunSpeed.GetString() : null,
                exitCondition.Value.TryGetProperty("blue", out var blue) ? blue.GetString() : null,
                exitCondition.Value.GetProperty("movementType").GetString() ?? ""
            ),
            "leaveSpaceJumping" => new ExitCondition.LeaveSpaceJumping(
                JsonSerializer.Deserialize<Runway>(exitCondition.Value.GetProperty("remoteRunway").GetRawText()) ?? new Runway(0, 0, null, null, null, null),
                exitCondition.Value.TryGetProperty("minExtraRunSpeed", out var minExtraRunSpeed) ? minExtraRunSpeed.GetString() : null,
                exitCondition.Value.TryGetProperty("maxExtraRunSpeed", out var maxExtraRunSpeed) ? maxExtraRunSpeed.GetString() : null,
                exitCondition.Value.TryGetProperty("blue", out var blue) ? blue.GetString() : null
            ),
            "leaveWithStoredFallSpeed" => new ExitCondition.LeaveWithStoredFallSpeed(exitCondition.Value.GetProperty("fallSpeedInTiles").GetInt32()),
            "leaveWithGModeSetup" => new ExitCondition.LeaveWithGModeSetup(
                exitCondition.Value.TryGetProperty("knockback", out var knockback) ? knockback.GetBoolean() : true
             ),
            "leaveWithGMode" => new ExitCondition.LeaveWithGMode(exitCondition.Value.GetProperty("morphed").GetBoolean()),
            "leaveWithDoorFrameBelow" => new ExitCondition.LeaveWithDoorFrameBelow(exitCondition.Value.GetProperty("height").GetDecimal()),
            "leaveWithPlatformBelow" => new ExitCondition.LeaveWithPlatformBelow(
                exitCondition.Value.GetProperty("height").GetDecimal(),
                exitCondition.Value.GetProperty("leftPosition").GetDecimal(),
                exitCondition.Value.GetProperty("rightPosition").GetDecimal()),
            "leaveWithGrappleTeleport" => new ExitCondition.LeaveWithGrappleTeleport(
                exitCondition.Value.GetProperty("blockPositions").EnumerateArray().Select(e => e.EnumerateArray().Select(e => e.GetInt32()).ToArray()).ToArray()),
            _ => throw new Exception($"Unknown exit condition: {exitCondition.Name}")
        };
    }

    public override ExitCondition? Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        return ParseElement(doc.RootElement);
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, ExitCondition value, System.Text.Json.JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
