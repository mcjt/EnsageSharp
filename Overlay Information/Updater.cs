using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using log4net;
using PlaySharp.Toolkit.Logging;

namespace OverlayInformation
{
    public class Updater
    {
        private OverlayInformation Main { get; }
        public List<HeroContainer> Heroes { get; }
        public List<HeroContainer> AllyHeroes { get; }
        public List<HeroContainer> EnemyHeroes { get; }
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public Updater(OverlayInformation overlayInformation)
        {
            Main = overlayInformation;
            Heroes = new List<HeroContainer>();
            AllyHeroes = new List<HeroContainer>();
            EnemyHeroes = new List<HeroContainer>();
            foreach (var entity in ObjectManager.GetDormantEntities<Hero>())
            {
                OnNewHero(null, entity);
            }
            foreach (var entity in ObjectManager.GetEntities<Hero>())
            {
                OnNewHero(null, entity);
            }

            foreach (var entity in ObjectManager.GetDormantEntities<Courier>())
            {
                OnNewCour(null, entity);
            }
            foreach (var entity in ObjectManager.GetEntities<Courier>())
            {
                OnNewCour(null, entity);
            }

            EntityManager<Hero>.EntityAdded += OnNewHero;
            EntityManager<Courier>.EntityAdded += OnNewCour;
        }

        private void OnNewCour(object sender, Courier courier)
        {
            if (courier.Team==Main.Owner.Team)
                return;

            ////TODO: cour esp
        }

        private void OnNewHero(object sender, Hero hero)
        {
            if (hero.IsIllusion)
                return;
            if (Heroes.Any(x=>x.Hero.Equals(hero)))
            {
                Log.Error($"Cant init New Hero -> {hero.GetDisplayName()} [{hero.Handle}]");
                return;
            }
            var myTeam = Main.Context.Value.Owner.Team;
            var targetTeam = hero.Team;
            var isAlly = myTeam == targetTeam;
            var newHero = new HeroContainer(hero, isAlly, Main);
            try
            {
                Heroes.Add(newHero);

                if (isAlly)
                {
                    AllyHeroes.Add(newHero);
                }
                else
                {
                    EnemyHeroes.Add(newHero);
                }

                Log.Info($"New Hero -> {hero.GetDisplayName()} [{hero.Handle}] [{(isAlly ? "Ally" : "Enemy")}]");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }

        public void OnDeactivate()
        {
            foreach (var container in Heroes)
            {
                container.Flush();
            }
        }
    }
}