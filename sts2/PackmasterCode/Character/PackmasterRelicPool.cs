using BaseLib.Abstracts;
using Packmaster.PackmasterCode.Extensions;
using Godot;

namespace Packmaster.PackmasterCode.Character;

public class PackmasterRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => ThePackmaster.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}