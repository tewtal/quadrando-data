namespace Randomizer.Graph.Combo.SuperMetroid.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal record HelperCollection(HelperCategory[] HelperCategories);

internal record Helper
(
    string Name,
    Requirement Requires,
    Note? Note,
    Note? DevNote
);

internal record HelperCategory
(
    string Name,
    string Description,
    Helper[] Helpers
);
