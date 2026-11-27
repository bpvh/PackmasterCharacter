import tempfile
import textwrap
import unittest
from pathlib import Path

from porting import emit, java

CARD = '''
package thePackmaster.cards.strikepack;

public class TestStrike extends AbstractStrikePackCard {
    public final static String ID = makeID("TestStrike");
    private static final int DMG = 7;

    public TestStrike() {
        super(ID, 1, CardType.ATTACK, CardRarity.COMMON, CardTarget.ENEMY);
        damage = baseDamage = DMG;
        baseMagicNumber = magicNumber = 2;
        exhaust = true;
        this.tags.add(CardTags.STRIKE);
    }

    public void use(AbstractPlayer p, AbstractMonster m) {
        dmg(m, AbstractGameAction.AttackEffect.SLASH_DIAGONAL);
        Wiz.applyToEnemy(m, new VulnerablePower(m, magicNumber, false));
        atb(new DrawCardAction(1));
    }

    public void upp() {
        upgradeDamage(3);
        upgradeBaseCost(0);
        exhaust = false;
    }
}
'''


def parse(src):
    with tempfile.TemporaryDirectory() as d:
        p = Path(d) / "a" / "b" / "c" / "d" / "e" / "TestStrike.java"
        p.parent.mkdir(parents=True)
        p.write_text(textwrap.dedent(src))
        return java.parse_card(p), p


class Parse(unittest.TestCase):
    def test_fields(self):
        c, _ = parse(CARD)
        self.assertEqual((c.cost, c.type, c.rarity, c.target), ("1", "ATTACK", "COMMON", "ENEMY"))
        self.assertEqual(c.values, {"damage": 7, "magic": 2})
        self.assertEqual(c.upgrades, {"damage": 3})
        self.assertEqual(c.upgrade_cost, 0)
        self.assertEqual(c.keywords, ["Exhaust"])
        self.assertEqual(c.upgrade_keywords, {"Exhaust": "Remove"})
        self.assertEqual(c.tags, ["STRIKE"])
        self.assertEqual(c.ctor_rest, [])
        self.assertEqual(c.upp_rest, [])


class Emit(unittest.TestCase):
    def test_card(self):
        c, _ = parse(CARD)
        src, status = emit.card_class(c, "Base", "NS", "Deal !D! damage.")
        self.assertEqual(status, "ported")
        for expected in [
            "public sealed class TestStrike : Base",
            "base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)",
            "WithDamage(7, 3);",
            'WithVar("Magic", 2);',
            "WithKeyword(CardKeyword.Exhaust, UpgradeType.Remove);",
            "WithCostUpgradeBy(-1);",
            "WithTags(CardTag.Strike);",
            'await CommonActions.CardAttack(this, play, vfx: "vfx/vfx_attack_slash").Execute(ctx);',
            'await CommonActions.Apply<VulnerablePower>(ctx, play.Target!, this, DynamicVars["Magic"].BaseValue);',
            "await CardPileCmd.Draw(ctx, 1m, Owner);",
            "using MegaCrit.Sts2.Core.Models.Powers;",
            "WithTip(typeof(VulnerablePower));",
        ]:
            self.assertIn(expected, src)

    def test_untranslated_is_skeleton(self):
        c, _ = parse(CARD.replace("atb(new DrawCardAction(1));", "atb(new SomethingCustom(magicNumber));"))
        src, status = emit.card_class(c, "Base", "NS")
        self.assertEqual(status, "skeleton")
        self.assertIn("// TODO port: atb(new SomethingCustom(magicNumber));", src)

    def test_namespace(self):
        self.assertEqual(emit.namespace_for_package("thePackmaster.cards.strikepack"),
                         "Packmaster.PackmasterCode.Cards.Strikepack")


if __name__ == "__main__":
    unittest.main()
