using Packmaster.PackmasterCode.Character;
using Packmaster.PackmasterCode.Extensions;

namespace Packmaster.PackmasterCode.Relics;

/// <summary>
/// Base class for Packmaster relics. StS1 art is images/relics/{Name}.png and {Name}Outline.png;
/// tools/sync_assets.py copies them to Packmaster/images/relics/ (and the main image to relics/big/).
/// ArtId defaults to the class name.
/// </summary>
[Pool(typeof(PackmasterRelicPool))]
public abstract class PackmasterRelic : CustomRelicModel
{
    public virtual string ArtId => GetType().Name;

    public override string PackedIconPath => $"{ArtId}.png".RelicImagePath();
    protected override string PackedIconOutlinePath => $"{ArtId}Outline.png".RelicImagePath();
    protected override string BigIconPath => $"{ArtId}.png".BigRelicImagePath();
}
