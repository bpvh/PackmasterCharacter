import unittest

from porting import ids
from porting.loc import convert_text, merge_upgrade, VANILLA_KEYWORDS_ENG


def conv(s, **kw):
    kw.setdefault("keyword_names", VANILLA_KEYWORDS_ENG)
    return convert_text(s, **kw).text


class Ids(unittest.TestCase):
    def test_class_and_slug(self):
        self.assertEqual(ids.class_name("${ModID}:Rummage"), "Rummage")
        self.assertEqual(ids.entry("${ModID}:HandyHaversack"), "PACKMASTER-HANDY_HAVERSACK")
        self.assertEqual(ids.slug("PMBoosterBox"), "PM_BOOSTER_BOX")
        self.assertEqual(ids.slug("Strike2"), "STRIKE_2")
        self.assertEqual(ids.class_name("anniv5:some card"), "SomeCard")
        self.assertTrue(ids.needs_custom_id("PMBoosterBox"))
        self.assertFalse(ids.needs_custom_id("HandyHaversack"))


class Text(unittest.TestCase):
    def test_strike_defend(self):
        self.assertEqual(conv("Deal !D! damage."), "Deal {Damage:diff()} damage.")
        self.assertEqual(conv("Gain !B! Block."), "Gain {Block:diff()} [gold]Block[/gold].")

    def test_rummage(self):
        s = ("{@@}Deal !D! damage. NL Draw !${ModID}:m2! {@@}{!${ModID}:m2!|@=card|>1=cards} and reduce "
             "{@@}{!${ModID}:m2!|@=its|>1=their} cost by !M! this turn.")
        self.assertEqual(conv(s),
                         "Deal {Damage:diff()} damage.\nDraw {Magic2:diff()} {Magic2:cond:>1?cards|card} and reduce "
                         "{Magic2:cond:>1?their|its} cost by {Magic:diff()} this turn.")

    def test_plural_suffix(self):
        self.assertEqual(conv("{@@}Draw !M! card{!M!|>1=s}."), "Draw {Magic:diff()} card{Magic:cond:>1?s|}.")
        self.assertEqual(conv("{@@}Put !M! card{!M!|1=|@=s} into your hand."),
                         "Put {Magic:diff()} card{Magic:cond:=1?|s} into your hand.")

    def test_upgrades_conditional(self):
        self.assertEqual(conv("{@@}Create a *Smite{!Upgrades!|>0=+} in hand."),
                         "Create a [gold]Smite[/gold]{IfUpgraded:show:+|} in hand.")

    def test_energy(self):
        self.assertEqual(conv("Gain [E] [E]."), "Gain {energyPrefix:energyIcons(2)}.")
        self.assertEqual(conv("Gain [E]."), "Gain {energyPrefix:energyIcons(1)}.")

    def test_mod_and_stslib_keywords(self):
        r = convert_text("Gain 4 ${ModID}:Plated_Armor. NL stslib:Exhaustive !stslib:ex!.")
        self.assertEqual(r.text, "Gain 4 [gold]Plated Armor[/gold].\n[gold]Exhaustive[/gold] {Exhaustive:diff()}.")
        self.assertEqual(r.keywords, {"Plated Armor", "Exhaustive"})

    def test_color_prefixes(self):
        self.assertEqual(conv("At the start of your turn, gain #b2 #yStrength."),
                         "At the start of your turn, gain [blue]2[/blue] [gold]Strength[/gold].")

    def test_hex_color(self):
        self.assertEqual(conv("When [#EFC851]Divine [#EFC851]Might is played"),
                         "When [gold]Divine Might[/gold] is played")

    def test_no_double_highlight(self):
        self.assertEqual(conv("Gain #yBlock and !B! Block."), "Gain [gold]Block[/gold] and {Block:diff()} [gold]Block[/gold].")

    def test_keyword_inside_var_untouched(self):
        self.assertEqual(conv("{@@}Apply !M! Weak{!M!|>1=s}."), "Apply {Magic:diff()} [gold]Weak[/gold]{Magic:cond:>1?s|}.")

    def test_strip_auto_keywords(self):
        r = convert_text("Innate. NL Draw !M! cards. NL Exhaust.", keyword_names=VANILLA_KEYWORDS_ENG, strip_auto_keywords=True)
        self.assertEqual(r.text, "Draw {Magic:diff()} cards.")
        self.assertEqual(r.removed_keywords, ["Innate", "Exhaust"])
        self.assertEqual(r.removed_line_indices, [0, 2])
        # Other languages strip the same line indices.
        z = convert_text("固有。 NL 抽 !M! 张牌。 NL 消耗。", strip_indices=r.removed_line_indices)
        self.assertEqual(z.text, "抽 {Magic:diff()} 张牌。")

    def test_exhaust_mid_sentence_kept(self):
        self.assertEqual(conv("Exhaust a card.", strip_auto_keywords=True), "[gold]Exhaust[/gold] a card.")


class Upgrade(unittest.TestCase):
    def test_merge_word(self):
        self.assertEqual(merge_upgrade("Draw 1 card.", "Draw 2 cards."), "Draw {IfUpgraded:show:2 cards.|1 card.}")

    def test_merge_prefix(self):
        base = "Choose 1 of {Magic:diff()} Packs."
        upg = "Choose 1 of {Magic:diff()} Packs available."
        self.assertEqual(merge_upgrade(base, upg), "Choose 1 of {Magic:diff()} {IfUpgraded:show:Packs available.|Packs.}")

    def test_same(self):
        self.assertEqual(merge_upgrade("A", "A"), "A")
        self.assertEqual(merge_upgrade("A", None), "A")


if __name__ == "__main__":
    unittest.main()


class Merge(unittest.TestCase):
    def test_hand_edits_survive(self):
        import json
        import tempfile
        from pathlib import Path
        import convert_loc
        with tempfile.TemporaryDirectory() as d:
            path = Path(d) / "cards.json"
            merged, owned = convert_loc.merge_table(path, {"A.title": "Gen A", "B.title": "Gen B"}, {})
            merged["A.title"] = "Hand A"          # edited by hand after generation
            merged["C.title"] = "Hand C"          # added by hand
            path.write_text(json.dumps(merged))
            merged2, owned2 = convert_loc.merge_table(path, {"A.title": "Gen A2", "B.title": "Gen B2", "C.title": "Gen C"}, owned)
            self.assertEqual(merged2["A.title"], "Hand A")
            self.assertEqual(merged2["B.title"], "Gen B2")
            self.assertEqual(merged2["C.title"], "Hand C")
            self.assertNotIn("A.title", owned2)


class TrailingKeywords(unittest.TestCase):
    def test_sentence_at_end_and_start(self):
        r = convert_text("Choose 1 of 3 cards. It costs 0 this turn. Exhaust.", strip_auto_keywords=True)
        self.assertEqual(r.text, "Choose 1 of 3 cards. It costs 0 this turn.")
        self.assertEqual(r.removed_keywords, ["Exhaust"])
        r = convert_text("Innate. Draw 2 cards.", strip_auto_keywords=True)
        self.assertEqual(r.text, "Draw 2 cards.")

    def test_exhaust_verb_kept(self):
        r = convert_text("Draw 2 cards. Exhaust a card.", strip_auto_keywords=True)
        self.assertIn("Exhaust", r.text)


class MarkedKeywords(unittest.TestCase):
    def test_star_marked_keyword_line(self):
        r = convert_text("*Unplayable. NL When you draw this card, draw !M! cards.", strip_auto_keywords=True)
        self.assertEqual(r.text, "When you draw this card, draw {Magic:diff()} cards.")
        self.assertEqual(r.removed_keywords, ["Unplayable"])

    def test_two_keywords_one_line(self):
        r = convert_text("Unplayable. Retain. NL Draw 2 cards.", strip_auto_keywords=True)
        self.assertEqual(r.text, "Draw 2 cards.")
        self.assertEqual(sorted(r.removed_keywords), ["Retain", "Unplayable"])


class MergeGold(unittest.TestCase):
    def test_multi_word_name(self):
        self.assertEqual(conv("Obtain a *Curse *of *the *Bell."), "Obtain a [gold]Curse of the Bell[/gold].")
