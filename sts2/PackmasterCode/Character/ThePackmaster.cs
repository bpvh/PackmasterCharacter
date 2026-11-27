using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using Packmaster.PackmasterCode.Cards.Starter;
using Packmaster.PackmasterCode.Extensions;
using Packmaster.PackmasterCode.Relics;

namespace Packmaster.PackmasterCode.Character;

/// <summary>
/// The Packmaster. StS1 stats: 75 HP, 99 gold, 3 energy.
/// Starting deck: 4 Strike, 4 Defend, Rummage, Cardistry. Starting relic: Handy Haversack.
/// Visuals still use PlaceholderCharacterModel's base-game assets (Phase 4 replaces them).
/// </summary>
public class ThePackmaster : PlaceholderCharacterModel
{
    public const string CharacterId = "ThePackmaster";

    /// <summary>StS1 characterColor (0.76, 0.65, 0.52).</summary>
    public static readonly Color Color = new(0.76f, 0.65f, 0.52f);

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 75;
    public override int StartingGold => 99;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<Strike>(),
        ModelDb.Card<Strike>(),
        ModelDb.Card<Strike>(),
        ModelDb.Card<Strike>(),
        ModelDb.Card<Defend>(),
        ModelDb.Card<Defend>(),
        ModelDb.Card<Defend>(),
        ModelDb.Card<Defend>(),
        ModelDb.Card<Rummage>(),
        ModelDb.Card<Cardistry>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<HandyHaversack>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<PackmasterCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<PackmasterRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<PackmasterPotionPool>();

    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }

    public override string CustomIconTexturePath => "character_icon_packmaster.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_packmaster.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_packmaster_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_packmaster.png".CharacterUiPath();
}
