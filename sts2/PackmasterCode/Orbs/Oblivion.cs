// Ported from src/main/java/thePackmaster/orbs/Oblivion.java and actions/orbpack/OblivionEvokeAction.java
using Godot;
using Packmaster.PackmasterCode.Extensions;
using Packmaster.PackmasterCode.Powers.Orbpack;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Orbs;

/// <summary>
/// Passive: whenever you discard a card, increase this orb's Ruin by [Passive]. Evoke: apply [Evoke] Ruin to the
/// enemy with the most HP. Like Dark, the evoke value accumulates.
/// </summary>
public sealed class Oblivion : CustomOrbModel
{
    public const decimal BasePassive = 2m;
    public const decimal BaseEvoke = 4m;

    private decimal _evoke = BaseEvoke;

    public override Color DarkenedColor => new("4a6f80");
    public override decimal PassiveVal => ModifyOrbValue(BasePassive);
    public override decimal EvokeVal => _evoke;

    public override string? CustomIconPath => "orbs/Oblivion.png".ImagePath();

    // VERIFY: orb visuals. StS2 orbs use scenes (orbs/orb_visuals/...); this shows the StS1 image as a sprite.
    public override Node2D? CreateCustomSprite()
    {
        var tex = ResourceLoader.Load<Texture2D>("orbs/Oblivion.png".ImagePath());
        return tex == null ? null : new Sprite2D { Texture = tex };
    }

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (card.Owner == Owner) await Passive(choiceContext, null);
    }

    public override Task Passive(PlayerChoiceContext choiceContext, Creature? target)
    {
        Trigger();
        _evoke += PassiveVal;
        return Task.CompletedTask;
    }

    public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext playerChoiceContext)
    {
        var strongest = CombatState.HittableEnemies.MaxBy(c => c.CurrentHp);
        if (strongest == null) return [];
        PlayEvokeSfx();
        await Wiz.ApplyPower<RuinPower>(playerChoiceContext, strongest, EvokeVal, Owner.Creature);
        return [strongest];
    }
}
