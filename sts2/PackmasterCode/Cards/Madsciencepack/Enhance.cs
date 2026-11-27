// Ported from src/main/java/thePackmaster/cards/madsciencepack/Enhance.java
using Packmaster.PackmasterCode.CardModifiers.Madsciencepack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Madsciencepack;

/// <summary>Deal 14 (+3) damage. Modify a card in hand: "Gain 6 (+2) Block".</summary>
public sealed class Enhance : AbstractMadScienceCard
{
    public Enhance() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(14, 3);
        WithVar("Magic", 6, 2);
        WithTip(PmTips.Modify);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_lightning").Execute(ctx);
        await MadScience.Modify(ctx, this, Wiz.Hand(Owner), 1, random: false, c => !c.EnergyCost.CostsX && MadScience.Playable(c),
            MadScience.New<GainBlockModifier>(DynamicVars["Magic"].IntValue), fromHand: true);
    }
}
