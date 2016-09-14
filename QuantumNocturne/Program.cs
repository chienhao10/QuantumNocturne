//SupportExTraGoZ  <3
//r show on mini map, onload shoud,  spell not learn no show on lv 1, jungle clear(server.po?position, yellow circle,
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Data;
using SharpDX;
using QuantumNocturne.Properties;
using SebbyLib;
using Color = System.Drawing.Color;
using System.Drawing;
using TreeLib.Objects;

namespace QuantumNocturne
{
    class Program
    {

        #region Declaration
        private static Spell Q, W, E, R;
        private static SebbyLib.Orbwalking.Orbwalker Orbwalker;
        private static Menu Menu;
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private static string ChampionName = "Nocturne";
        private static int lvl1, lvl2, lvl3, lvl4;
        #endregion

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != ChampionName)
            return;
            Game.PrintChat("<font color='#f45c09'>[SugoiSeries]</font><font color='#03d8f6'> Quantum Nocturne </font><font color='#13d450'>Loaded.</font>");

            #region Spells
            Q = new Spell(SpellSlot.Q, 1200f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 425f);
            R = new Spell(SpellSlot.R, 1750f + 750f * ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Level);

            Q.SetSkillshot(0.25f, 60f, 1400f, true, SkillshotType.SkillshotLine);
            #endregion

            #region Menu
            Menu = new Menu("Quantum Nocturne", "Quantum Nocturne", true);

            Menu OrbwalkerMenu = Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new SebbyLib.Orbwalking.Orbwalker(OrbwalkerMenu);

            Menu TargetSelectorMenu = Menu.AddSubMenu(new Menu("Target Selector", "TargetSelector"));
            TargetSelector.AddToMenu(TargetSelectorMenu);
            Menu ComboMenu = Menu.AddSubMenu(new Menu("Combo", "Combo"));
            ComboMenu.AddItem(new MenuItem("ComboUseQ", "Use Q").SetValue(true));
            ComboMenu.AddItem(new MenuItem("ComboUseW", "Use W In R").SetValue(true).SetTooltip("Will Olnly Use W while R, for others pls use ElUtility").SetFontStyle(
                        FontStyle.Bold, SharpDX.Color.Red));
            ComboMenu.AddItem(new MenuItem("ComboUseE", "Use E").SetValue(true));
            ComboMenu.AddItem(new MenuItem("ComboUseR", "Use R").SetValue(true).SetTooltip("Will use R if there's target that's killable in (W + R + 3 AA) Damage"));

            Menu HarassMenu = Menu.AddSubMenu(new Menu("Harass", "Harass"));
            HarassMenu.AddItem(new MenuItem("HarassUseQ", "Use Q").SetValue(true));
            HarassMenu.AddItem(new MenuItem("HarassUseE", "Use E").SetValue(true));
            HarassMenu.AddItem(new MenuItem("HarassManaManager", "Mana Manager (%)").SetValue(new Slider(40, 1, 100)));

            Menu LaneClearMenu = Menu.AddSubMenu(new Menu("Lane Clear", "LaneClear"));
            LaneClearMenu.AddItem(new MenuItem("LaneClearUseQ", "Use Q").SetValue(true));
            LaneClearMenu.AddItem(new MenuItem("LaneClearUseE", "Use E").SetValue(true));
            LaneClearMenu.AddItem(new MenuItem("LaneClearManaManager", "Mana Manager (%)").SetValue(new Slider(40, 1, 100)));

            Menu JungleClearMenu = Menu.AddSubMenu(new Menu("Jungle Clear", "JungleClear"));
            JungleClearMenu.AddItem(new MenuItem("JungleClearUseQ", "Use Q").SetValue(true));
            JungleClearMenu.AddItem(new MenuItem("JungleClearUseE", "Use E").SetValue(true));
            JungleClearMenu.AddItem(new MenuItem("JungleClearManaManager", "Mana Manager (%)").SetValue(new Slider(40, 1, 100)));

            Menu UltimateMenu = Menu.AddSubMenu(new Menu("Ultimate Menu", "UltimateMenu"));
            UltimateMenu.AddItem(new MenuItem("InstaRSelectedTarget", "Engage with R Instantly").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)).SetTooltip("It'll Use the R if there's Enemy selected or enemy nearby, and When You have more allies")).Permashow(true, "Engage R");
            UltimateMenu.AddItem(
                new MenuItem("DontUltEnemyIfAllyNearby", "Don't Ult if Ally is Near Almost Dead Enemy").SetValue(false));

            Menu FleeMenu = Menu.AddSubMenu(new Menu("Flee", "Flee"));
            FleeMenu.AddItem(new MenuItem("FleeON", "Enable Flee").SetValue(true));
            FleeMenu.AddItem(new MenuItem("FleeK", "Flee Key").SetValue(
                            new KeyBind('J', KeyBindType.Press)));
            FleeMenu.AddItem(new MenuItem("FleeQ", "Flee With Q").SetValue(true));

            Menu MiscMenu = Menu.AddSubMenu(new Menu("Misc Menu", "MiscMenu"));
            MiscMenu.AddItem(new MenuItem("KillSteal", "Activate KillSteal?").SetValue(true));
            MiscMenu.AddItem(new MenuItem("QKillSteal", "Q KillSteal?").SetValue(true));
            MiscMenu.AddItem(new MenuItem("RKillSteal", "R KillSteal?").SetValue(false));
            MiscMenu.AddItem(new MenuItem("RQKillSteal", "R + Q Chase Kill?").SetValue(false)).SetTooltip("Will R + Q If Can Kill Enemy When No Ally Near");
            MiscMenu.AddItem(new MenuItem("EToInterrupt", "Auto E to Interrupt Enemies").SetValue(true));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                MiscMenu.SubMenu("[GapClosers] E Config").AddItem(new MenuItem("GapCloserEnemies" + enemy.ChampionName, enemy.ChampionName).SetValue(false).SetTooltip("Use E on GapClosing Champions"));

            Menu AutoLevelerMenu = Menu.AddSubMenu(new Menu("AutoLeveler Menu", "AutoLevelerMenu"));
            AutoLevelerMenu.AddItem(new MenuItem("AutoLevelUp", "AutoLevel Up Spells?").SetValue(true));
            AutoLevelerMenu.AddItem(new MenuItem("AutoLevelUp1", "First: ").SetValue(new StringList(new[] { "Q", "W", "E", "R" }, 3)));
            AutoLevelerMenu.AddItem(new MenuItem("AutoLevelUp2", "Second: ").SetValue(new StringList(new[] { "Q", "W", "E", "R" }, 0)));
            AutoLevelerMenu.AddItem(new MenuItem("AutoLevelUp3", "Third: ").SetValue(new StringList(new[] { "Q", "W", "E", "R" }, 1)));
            AutoLevelerMenu.AddItem(new MenuItem("AutoLevelUp4", "Fourth: ").SetValue(new StringList(new[] { "Q", "W", "E", "R" }, 2)));
            AutoLevelerMenu.AddItem(new MenuItem("AutoLvlStartFrom", "AutoLeveler Start from Level: ").SetValue(new Slider(2, 6, 1)));

            Menu SkinMenu = Menu.AddSubMenu(new Menu("Skins Menu", "SkinMenu"));
            SkinMenu.AddItem(new MenuItem("SkinID", "Skin ID")).SetValue(new Slider(5, 0, 7));
            var UseSkin = SkinMenu.AddItem(new MenuItem("UseSkin", "Enabled")).SetValue(true);
            UseSkin.ValueChanged += (sender, eventArgs) =>
            {
                if (!eventArgs.GetNewValue<bool>())
                {
                    ObjectManager.Player.SetSkin(ObjectManager.Player.CharData.BaseSkinName, ObjectManager.Player.BaseSkinId);
                }
            };

            Menu DrawingMenu = Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
            DrawingMenu.AddItem(new MenuItem("DR", "Draw Only Ready Spells").SetValue(true));
            DrawingMenu.AddItem(new MenuItem("DrawAA", "Draw AA Range").SetValue(true));
            DrawingMenu.AddItem(new MenuItem("DrawQ", "Draw Q Range").SetValue(false));
            DrawingMenu.AddItem(new MenuItem("DrawE", "Draw E Range").SetValue(false));
            DrawingMenu.AddItem(new MenuItem("DrawR", "Draw R Range").SetValue(false));
            DrawingMenu.AddItem(new MenuItem("DrawMap", "Draw R Range On MiniMap").SetValue(false));

            Menu CreditMenu = Menu.AddSubMenu(new Menu("Credits", "Credits"));
            CreditMenu.AddItem(new MenuItem("ME: LOVETAIWAN♥", "ME: LOVETAIWAN♥")).SetTooltip("Learning From Devs Below!").SetFontStyle(
                FontStyle.Bold, SharpDX.Color.Pink);
            CreditMenu.AddItem(new MenuItem("Sebby", "Sebby")).SetFontStyle(
                FontStyle.Bold, SharpDX.Color.Blue);
            CreditMenu.AddItem(new MenuItem("SupportExTraGoZ", "SupportExTraGoZ")).SetFontStyle(
                FontStyle.Bold, SharpDX.Color.Purple);
            CreditMenu.AddItem(new MenuItem("Soresu", "Soresu")).SetFontStyle(
                FontStyle.Bold, SharpDX.Color.Yellow);
            CreditMenu.AddItem(new MenuItem("Trees", "Trees")).SetFontStyle(
                FontStyle.Bold, SharpDX.Color.Orange);
            CreditMenu.AddItem(new MenuItem("imop", "Be Sugoi").SetValue(false)).SetFontStyle(
                FontStyle.Bold, SharpDX.Color.Green);
            if (Menu.Item("imop").GetValue<bool>())
            {
                new SoundObject(Resources.OnLoad).Play();
            }
            Menu.AddToMainMenu();
            #endregion

//SupportExTraGoZ drawhp <3
            #region DrawHPDamage
            var dmgAfterShave = new MenuItem("QuantumNocturne.DrawComboDamage", "Draw Damage on Enemy's HP Bar").SetValue(true);
            var drawFill =
                new MenuItem("QuantumNocturne.DrawColour", "Fill Color", true).SetValue(
                    new Circle(true, Color.SeaGreen));
            DrawingMenu.AddItem(drawFill);
            DrawingMenu.AddItem(dmgAfterShave);
            DrawDamage.DamageToUnit = CalculateDamage;
            DrawDamage.Enabled = dmgAfterShave.GetValue<bool>();
            DrawDamage.Fill = drawFill.GetValue<Circle>().Active;
            DrawDamage.FillColor = drawFill.GetValue<Circle>().Color;
            dmgAfterShave.ValueChanged +=
                delegate (object sender, OnValueChangeEventArgs eventArgs)
                {
                    DrawDamage.Enabled = eventArgs.GetNewValue<bool>();
                };

            drawFill.ValueChanged += delegate (object sender, OnValueChangeEventArgs eventArgs)
            {
                DrawDamage.Fill = eventArgs.GetNewValue<Circle>().Active;
                DrawDamage.FillColor = eventArgs.GetNewValue<Circle>().Color;
            };
            #endregion

            #region Subscriptions
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnLevelUp += Obj_AI_Base_OnLevelUp;
            Game.OnUpdate += Game_OnUpdate;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            #endregion
        }

        private static void Obj_AI_Base_OnLevelUp(Obj_AI_Base sender, EventArgs args)
        {
            if (!sender.IsMe || !Menu.Item("AutoLevelUp").GetValue<bool>() || ObjectManager.Player.Level < Menu.Item("AutoLvlStartFrom").GetValue<Slider>().Value)
                return;
            if (lvl2 == lvl3 || lvl2 == lvl4 || lvl3 == lvl4)
                return;
            int delay = 700;
            Utility.DelayAction.Add(delay, () => LevelUp(lvl1));
            Utility.DelayAction.Add(delay + 50, () => LevelUp(lvl2));
            Utility.DelayAction.Add(delay + 100, () => LevelUp(lvl3));
            Utility.DelayAction.Add(delay + 150, () => LevelUp(lvl4));
        }

        private static void LevelUp(int indx)
        {
            if (ObjectManager.Player.Level < 4)
            {
                if (indx == 0 && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                if (indx == 1 && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                if (indx == 2 && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
            }
            else
            {
                if (indx == 0)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                if (indx == 1)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                if (indx == 2)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                if (indx == 3)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Menu.Item("DrawAA").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Player.Position, SebbyLib.Orbwalking.GetRealAutoAttackRange(null), Color.White);
            }

            if (Menu.Item("DR").GetValue<bool>() && Q.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.Aqua);
            }
            else
            {
                if (Menu.Item("DrawQ").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Color.Aqua);
                }
            }

            if (Menu.Item("DR").GetValue<bool>() && E.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.Aqua);
            }
            else
            {
                if (Menu.Item("DrawE").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Player.Position, E.Range, Color.Aqua);
                }
            }
            if (Menu.Item("DR").GetValue<bool>() && R.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Red);
            }
            else
            {
                if (Menu.Item("DrawR").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Player.Position, R.Range, Color.Red);
                }
            }
            if (Menu.Item("DR").GetValue<bool>() && R.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Red, 45, true);
                Drawing.WorldToMinimap(Player.Position).To3D2();
            }
            else
            {
                if (Menu.Item("DrawMap").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Player.Position, R.Range, Color.Red, 45, true);
                }
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead || Player.IsRecalling())
                return;

            if (Menu.Item("UseSkin").GetValue<bool>())
            {
                Player.SetSkin(Player.CharData.BaseSkinName, Menu.Item("SkinID").GetValue<Slider>().Value);
            }
            switch (Orbwalker.ActiveMode)
            {
                case SebbyLib.Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case SebbyLib.Orbwalking.OrbwalkingMode.Mixed:
                    if (Player.ManaPercent > Menu.Item("HarassManaManager").GetValue<Slider>().Value)
                    {
                        Harass();
                    }
                    break;
                case SebbyLib.Orbwalking.OrbwalkingMode.LaneClear:
                    if (Player.ManaPercent > Menu.Item("LaneClearManaManager").GetValue<Slider>().Value)
                    {
                        LaneClear();
                    }
                    if (Player.ManaPercent > Menu.Item("JungleClearManaManager").GetValue<Slider>().Value)
                    {
                        JungleClear();
                    }
                    break;
            }
            if (Menu.Item("FleeON").GetValue<bool>())
            {
                if (Menu.Item("FleeK").GetValue<KeyBind>().Active)
                {
                    Flee();
                }
            }
            KS();

            if (Menu.Item("InstaRSelectedTarget").GetValue<KeyBind>().Active)
            {
                InstantlyR();
            }

            //AutoLeveler
            if (Menu.Item("AutoLevelUp").GetValue<bool>())
            {
                lvl1 = Menu.Item("AutoLevelUp1").GetValue<StringList>().SelectedIndex;
                lvl2 = Menu.Item("AutoLevelUp2").GetValue<StringList>().SelectedIndex;
                lvl3 = Menu.Item("AutoLevelUp3").GetValue<StringList>().SelectedIndex;
                lvl4 = Menu.Item("AutoLevelUp4").GetValue<StringList>().SelectedIndex;
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (E.IsReady())
            {
                var GapCloser = gapcloser.Sender;
                if (Menu.Item("GapCloserEnemies" + GapCloser.ChampionName).GetValue<bool>() && GapCloser.IsValidTarget(425))
                {
                    E.Cast(gapcloser.Sender);
                }
            }
        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero t, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Menu.Item("EToInterrupt").GetValue<bool>() && t.IsValidTarget(425) && E.IsReady())
                E.Cast(t);
        }

        private static void InstantlyR()
        {
            var target = TargetSelector.GetSelectedTarget();
            if (target == null || !target.IsValidTarget())
                target = TargetSelector.GetTarget(4000, TargetSelector.DamageType.Physical);

            if (target.IsValidTarget() && target != null && !target.IsZombie && !target.UnderTurret(true) && !target.HasBuff("rebirth"))
            {
                if (target.CountEnemiesInRange(2000) < target.CountAlliesInRange(2000) && Player.Mana > R.Instance.ManaCost)
                {
                    R.Cast(target);
                }
            }
        }

        private static void DontREnemyNearAlly()
        {
            if (Menu.Item("DontUltEnemyIfAllyNearby").GetValue<bool>())
            {
                var enemy =
                    HeroManager.Enemies.Where(
                        x => x.IsValidTarget() && x.HealthPercent < 20 && x.CountAlliesInRange(625) > 0);

                if (enemy == null || enemy != null)
                    return;
            }
        }

        private static void KS()
        {
            if (Menu.Item("KillSteal").GetValue<bool>())
            {
                var target = Orbwalker.GetTarget() as Obj_AI_Hero;

                if (target.IsValidTarget(Q.Range) && target.Health < Q.GetDamage(target) + Q.GetDamage(target) + OktwCommon.GetIncomingDamage(target))
                {
                    DontREnemyNearAlly();
                    if (!target.CanMove || target.IsStunned ||!target.IsZombie)
                        Q.Cast(target.Position);
                }
                if (target.IsValidTarget(R.Range) && target.Health < R.GetDamage(target) + OktwCommon.GetIncomingDamage(target))
                {
                    if (target.CanMove)
                    {
                        R.Cast(target);
                    }
                }
                if (target.IsValidTarget(4000) && target.Health < R.GetDamage(target) + Q.GetDamage(target) + OktwCommon.GetIncomingDamage(target) && Player.Mana > Q.ManaCost + R.ManaCost)
                {
                    DontREnemyNearAlly();
                    var ally = HeroManager.Enemies.Where(
                        x => x.IsValidTarget() && !x.IsZombie && x.CountAlliesInRange(625) > 0);
                    var enemy = HeroManager.Enemies.Where(
                        x => x.IsValidTarget() && !x.IsZombie && x.CountEnemiesInRange(625) < 2);

                    if (ally != null || enemy !=null && !target.HasBuff("guardianangle"))
                    {
                        R.Cast(target);
                        if (Q.IsReady() && target.IsValidTarget(Q.Range))
                            Q.Cast(target.ServerPosition);
                    }                
                }
            }
        }

        private static void Combo()
        {
            var target = Orbwalker.GetTarget() as Obj_AI_Hero;
            if (target == null)
                target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            if (target != null && target.IsValidTarget())
            {
                if (Menu.Item("ComboUseQ").GetValue<bool>() && target.IsValidTarget(Q.Range) && Q.IsReady() && Player.Mana > Q.Instance.ManaCost || target.Health < 5 * Player.GetAutoAttackDamage(Player))
                    Q.Cast(target.ServerPosition);
                if (Menu.Item("ComboUseW").GetValue<bool>() && target.IsValidTarget(525) && W.IsReady() && Player.Mana > W.Instance.ManaCost && R.IsChanneling)
                    W.Cast();
                if (Menu.Item("ComboUseE").GetValue<bool>() && E.IsReady() && Player.Mana > E.Instance.ManaCost && target.IsValidTarget(E.Range))
                    E.Cast(target);
                DontREnemyNearAlly();
                foreach (var ulttarget in HeroManager.Enemies.Where(ulttarget => target.IsValidTarget(2000) && OktwCommon.ValidUlt(ulttarget)))
                {
                    if (Menu.Item("ComboUseR").GetValue<bool>() && target.IsValidTarget(R.Range) && target.Health < Q.GetDamage(target) + E.GetDamage(target) + R.GetDamage(target) + 3 * Player.GetAutoAttackDamage(target) + OktwCommon.GetIncomingDamage(target) && !target.HasBuff("rebirth"))
                    {
                        R.Cast(target);
                        if (Q.IsReady() && target.IsValidTarget(Q.Range))
                            Q.Cast(target.ServerPosition);
                    }
                }
            }
        }

        private static void Harass()
        {
            var SpellQ = Menu.Item("HarassUseQ").GetValue<bool>();
            var SpellE = Menu.Item("HarassUseE").GetValue<bool>();
            var target = Orbwalker.GetTarget() as Obj_AI_Hero;
            if (target == null)
                target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);

            if (target != null && target.IsValidTarget())
            {
                if (target.IsValidTarget(Q.Range) && SpellQ)
                    Q.Cast(target.ServerPosition);

                if (E.IsReady() && SpellE && target.IsValidTarget(E.Range))
                    E.Cast(target);
            }
        }

        private static void LaneClear()
        {
            var SpellQ = Menu.Item("LaneClearUseQ").GetValue<bool>();
            var SpellE = Menu.Item("LaneClearUseE").GetValue<bool>();
            var allMinionsQ = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);
            if (allMinionsQ.Count > 1)
            {
                foreach (var minion in allMinionsQ)
                {
                    if (!minion.IsValidTarget() || minion == null)
                        return;
                    if (SpellQ && Q.IsReady())
                        Q.Cast(minion.Position);
                    if (SpellE && E.IsReady())
                        E.Cast(minion);
                }
            }
        }

        private static void JungleClear()
        {
            var SpellQ = Menu.Item("JungleClearUseQ").GetValue<bool>();
            var SpellE = Menu.Item("JungleClearUseE").GetValue<bool>();
            var allMinions = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition,
                Q.Range,
                MinionTypes.All,
                MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);
            if (Menu.Item("JungleClearUseQ").GetValue<bool>() && Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget())
                    {
                        Q.Cast(minion.ServerPosition);
                    }
                }
            }
            if (Menu.Item("JungleClearUseE").GetValue<bool>() && E.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget())
                    {
                        E.Cast(minion);
                    }
                }
            }
        }

        private static void Flee()
        {
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            var FleeQ = Menu.Item("FleeQ").GetValue<bool>();
            if (FleeQ && Q.IsReady())
                Q.Cast(Game.CursorPos);
        }

        private static float CalculateDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (W.IsReady() && Player.Mana > W.Instance.ManaCost)
            {
                damage += W.GetDamage(enemy);
            }
            if (R.IsReady() && Player.Mana > R.Instance.ManaCost)
            {
                damage += R.GetDamage(enemy);
            }
            damage += (float)Player.GetAutoAttackDamage(enemy);

            return damage;
        }
    }
}
