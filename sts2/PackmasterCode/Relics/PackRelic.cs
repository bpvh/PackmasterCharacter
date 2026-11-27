using MegaCrit.Sts2.Core.Runs;
using Packmaster.PackmasterCode.Packs;

namespace Packmaster.PackmasterCode.Relics;

/// <summary>
/// A relic that belongs to a pack (StS1 AbstractPackmasterRelic with a pack ID): it only shows up in a run where
/// some Packmaster has that pack. Uses RelicModel.IsAllowed, which the game checks when pulling relics.
/// </summary>
public abstract class PackRelic : PackmasterRelic
{
    /// <summary>StS1 pack ID, e.g. "QuietPack".</summary>
    public abstract string PackId { get; }

    public override bool IsAllowed(IRunState runState)
    {
        if (!base.IsAllowed(runState)) return false;
        return runState.Players.Any(p => PackRunState.IsPackmaster(p) && PackRunState.Selected(p).Any(pack => pack.PackId == PackId));
    }
}
