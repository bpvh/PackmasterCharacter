// Ported from src/main/java/thePackmaster/relics/quietpack/TarredFeather.java
using BaseLib.Hooks;
using MegaCrit.Sts2.Core.Entities.Relics;
using Packmaster.PackmasterCode.Cards.Quietpack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Relics.Quietpack;

/// <summary>Shop relic (Quiet pack). Your maximum hand size is increased by 2. At the start of each combat, add 2 Weights to your hand.</summary>
public sealed class TarredFeather : PackRelic, IMaxHandSizeModifier
{
    private const int ExtraHandSize = 2;
    private const int Weights = 2;

    public override RelicRarity Rarity => RelicRarity.Shop;
    public override string PackId => "QuietPack";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Weight>()];

    public int ModifyMaxHandSize(Player player, int currentMaxHandSize) =>
        player == Owner ? currentMaxHandSize + ExtraHandSize : currentMaxHandSize;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || !Wiz.IsFirstTurn(Owner)) return;
        Flash();
        for (int i = 0; i < Weights; i++) await Wiz.MakeInHand(Wiz.CreateCard<Weight>(Owner), Owner);
    }
}
