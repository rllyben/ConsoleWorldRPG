using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleWorldRPG.Entities
{
    public class Currency
    {
        public int Bronze { get; private set; }

        public Currency(int bronze = 0)
        {
            Bronze = Math.Max(0, bronze);
        }

        public static readonly int BronzePerSilver = 1_000;
        public static readonly int SilverPerGold = 1_000;
        public static readonly int GoldPerPlatinum = 100;
        public static readonly int PlatinumPerCrystal = 100;

        public int TotalBronze => Bronze;

        public override string ToString()
        {
            int remaining = Bronze;
            int crystals = remaining / (BronzePerSilver * SilverPerGold * GoldPerPlatinum * PlatinumPerCrystal);
            remaining %= BronzePerSilver * SilverPerGold * GoldPerPlatinum * PlatinumPerCrystal;

            int platinum = remaining / (BronzePerSilver * SilverPerGold * GoldPerPlatinum);
            remaining %= BronzePerSilver * SilverPerGold * GoldPerPlatinum;

            int gold = remaining / (BronzePerSilver * SilverPerGold);
            remaining %= BronzePerSilver * SilverPerGold;

            int silver = remaining / BronzePerSilver;
            remaining %= BronzePerSilver;

            return $"{crystals}✶ {platinum}⎔ {gold}⛀ {silver}⬢ {remaining}⬤";
        }

        public bool CanAfford(int cost) => Bronze >= cost;

        public bool TrySpend(int cost)
        {
            if (CanAfford(cost))
            {
                Bronze -= cost;
                return true;
            }
            return false;
        }

        public void Add(int amount)
        {
            Bronze += Math.Max(0, amount);
        }

    }

}
