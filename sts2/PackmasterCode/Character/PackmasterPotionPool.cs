using BaseLib.Abstracts;
using Packmaster.PackmasterCode.Extensions;
using Godot;

namespace Packmaster.PackmasterCode.Character;

public class PackmasterPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => Packmaster.Color;
    

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}