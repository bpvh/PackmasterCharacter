using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Packmaster.PackmasterCode.Character;
using Packmaster.PackmasterCode.Extensions;

namespace Packmaster.PackmasterCode.Potions;

[Pool(typeof(PackmasterPotionPool))]
public abstract class PackmasterPotion : CustomPotionModel
{
	public override string? CustomPackedImagePath =>
		$"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionImagePath();
	public override string? CustomPackedOutlinePath =>
		$"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionOutlineImagePath();
}