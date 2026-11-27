// Ported from src/main/java/thePackmaster/powers/prismaticpack/QuirionDryadPower.java
using MegaCrit.Sts2.Core.Models.Powers;
using Packmaster.PackmasterCode.Util;

namespace Packmaster.PackmasterCode.Powers.Prismaticpack;

/// <summary>The first time each combat you play a card of each color (card pool), gain [Amount] Strength and Dexterity.</summary>
public sealed class QuirionDryadPower : PackmasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string ArtId => "QuirionDryad";

    private readonly HashSet<CardPoolModel> _colors = [];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!IsOwnersCard(cardPlay.Card) || !_colors.Add(cardPlay.Card.Pool)) return;
        Flash();
        await Wiz.ApplyPower<StrengthPower>(choiceContext, Owner, Amount, Owner);
        await Wiz.ApplyPower<DexterityPower>(choiceContext, Owner, Amount, Owner);
    }
}
