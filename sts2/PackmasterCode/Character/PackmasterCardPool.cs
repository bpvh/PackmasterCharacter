using BaseLib.Abstracts;
using Godot;
using Packmaster.PackmasterCode.Extensions;

namespace Packmaster.PackmasterCode.Character;

/// <summary>
/// Holds every Packmaster card: starters, system cards and all pack cards.
/// Which pack cards a run can offer is decided by the pack framework (Packs/), not by this pool.
/// </summary>
public class PackmasterCardPool : CustomCardPoolModel
{
    public override string Title => ThePackmaster.CharacterId; // Not a display name.

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();

    // Card back tint, applied as HSV over the base frame. Tuned toward the StS1 tan/brown frame.
    public override Color ShaderColor => ThePackmaster.Color;

    public override Color DeckEntryCardColor => ThePackmaster.Color;

    public override bool IsColorless => false;
}
