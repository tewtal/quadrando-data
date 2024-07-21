namespace Randomizer.Graph.Combo.SuperMetroid.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal record BossScenarioCollection(BossScenario[] Scenarios);

internal record BossScenario
(
    int Id,
    string Name,
    string Boss,
    string[]? ExplicitWeapons,
    string[]? ExcludedWeapons,
    Requirement Requires,
    int? BossDodgeRate,
    int? AttackOpportunityDuration,
    DamageWindow[]? DamageWindows,
    IncomingDamage[]? IncomingDamage,
    int? ParticleFrequencyFrames,
    Note? Note,
    Note? DevNote
);

internal record DamageWindow
(
    string Name,
    decimal WindowPercent,
    Requirement Requires,
    Note? Note,
    Note? DevNote
);

internal record IncomingDamage
(
    string Name,
    string Attack,
    int FrequencyFrames,
    Requirement AvoidingRequires,
    Note? Note,
    Note? DevNote
);
