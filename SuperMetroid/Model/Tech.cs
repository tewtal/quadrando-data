namespace Randomizer.Graph.Combo.SuperMetroid.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal record TechCategory(
    string Name,
    string Description,
    List<Tech> Techs
);

internal record Tech(
    string Name,
    Requirement TechRequires,
    Requirement OtherRequires,
    List<Tech> ExtensionTechs,
    Note? Note,
    Note? DevNote
);

internal record TechCollection(
    List<TechCategory> TechCategories
);
