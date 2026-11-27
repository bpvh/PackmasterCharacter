// Ported from src/main/java/thePackmaster/cards/madsciencepack/Sharpen.java
using Packmaster.PackmasterCode.CardModifiers.Madsciencepack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Cards.Madsciencepack;

/// <summary>Honing Strike: deal 8 (+2) damage. Modify a random Attack in hand: "Add 5 (+1) damage".</summary>
public sealed class Sharpen : AbstractMadScienceCard
{
    public Sharpen() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(8, 2);
        WithVar("Magic", 5, 1);
        WithTags(CardTag.Strike);
        WithTip(PmTips.Modify);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);
        await MadScience.Modify(ctx, this, Wiz.Hand(Owner), 1, random: true, c => c.Type == CardType.Attack,
            MadScience.New<AddDamageModifier>(DynamicVars["Magic"].IntValue), fromHand: true);
    }
}
