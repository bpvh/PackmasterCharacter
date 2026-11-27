// Namespaces almost every card, power and relic needs. Kept to namespaces that don't define
// base-game content models, so our class names (Strike, Defend, ...) never clash with vanilla ones.
// Vanilla powers/cards (MegaCrit.Sts2.Core.Models.Powers / .Cards) are imported per file.
global using BaseLib.Abstracts;
global using BaseLib.Extensions;
global using BaseLib.Utils;
global using MegaCrit.Sts2.Core.Commands;
global using MegaCrit.Sts2.Core.Entities.Cards;
global using MegaCrit.Sts2.Core.Entities.Creatures;
global using MegaCrit.Sts2.Core.Entities.Players;
global using MegaCrit.Sts2.Core.Entities.Powers;
global using MegaCrit.Sts2.Core.GameActions.Multiplayer;
global using MegaCrit.Sts2.Core.HoverTips;
global using MegaCrit.Sts2.Core.Localization.DynamicVars;
global using MegaCrit.Sts2.Core.Models;
global using MegaCrit.Sts2.Core.ValueProps;
