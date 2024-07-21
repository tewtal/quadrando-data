namespace Randomizer.Graph.Combo.SuperMetroid.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal record EnemyCollection(Enemy[] Enemies);

internal record Enemy
(
    int Id,
    string Name,
    Attack[] Attacks,
    int Hp,
    int AmountOfDrops,
    Note? Note,
    Note? DevNote,
    EnemyDrops Drops,
    EnemyDrops? FarmableDrops,
    Dimension Dims,
    bool Freezable,
    bool Grapplable,
    string[] Invul,
    DamageMultiplier[] DamageMultipliers,
    string[]? Areas
);

internal record DamageMultiplier
(
    string Weapon,
    decimal Value
);

internal record Dimension
(
    int W,
    int H,
    Note? Note,
    Note? DevNote
);

internal record Attack
(
    string Name,
    int BaseDamage,
    bool? AffectedByVaria,
    bool? AffectedByGravity
);

internal record EnemyDrops
(
    decimal NoDrop,
    decimal SmallEnergy,
    decimal BigEnergy,
    decimal Missile,
    decimal Super,
    decimal PowerBomb
);
