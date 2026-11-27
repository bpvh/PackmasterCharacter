namespace Packmaster.PackmasterCode.Powers;

/// <summary>
/// Base class for Packmaster powers.
///
/// Icons: StS1 power art is images/powers/{Name}32.png and {Name}84.png. tools/sync_assets.py
/// copies them as-is to Packmaster/images/powers/, so ArtId is the StS1 image base name
/// (defaults to the class name; override when the StS1 image was named differently).
/// </summary>
public abstract class PackmasterPower : CustomPowerModel
{
    public virtual string ArtId => GetType().Name;

    public override string CustomPackedIconPath => $"{ArtId}32.png".PowerImagePath();
    public override string CustomBigIconPath => $"{ArtId}84.png".PowerImagePath();

    public abstract override PowerType Type { get; }
    public abstract override PowerStackType StackType { get; }

    /// <summary>True if the card belongs to the player who owns this power.</summary>
    protected bool IsOwnersCard(CardModel? card) => card != null && Owner.Player != null && card.Owner == Owner.Player;
}
