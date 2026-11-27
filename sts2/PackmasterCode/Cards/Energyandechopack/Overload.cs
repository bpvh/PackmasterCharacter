// Ported from src/main/java/thePackmaster/cards/energyandechopack/Overload.java
namespace Packmaster.PackmasterCode.Cards.Energyandechopack;

/// <summary>Deal 10 damage. Increase this card's damage by 20 (+10) and its cost by 1 this combat.</summary>
public sealed class Overload : AbstractEchoCard
{
    public Overload() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithDamage(10);
        WithVar("Magic", 20, 10);
    }

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_lightning").Execute(ctx);
        // StS1 changed every in-combat instance of this card (ModifyDamageAction by uuid); StS2 changes this one.
        DynamicVars.Damage.BaseValue += DynamicVars["Magic"].BaseValue;
        EnergyCost.AddThisCombat(1);
    }
}
