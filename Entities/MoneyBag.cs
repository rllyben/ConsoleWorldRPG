using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleWorldRPG.Entities
{
    public class MoneyBag
    {
        public Currency Coins { get; set; } = new();
        public int Capacity { get; set; } = 300000; // can be upgraded
        public bool IsUnlimited => Capacity == int.MaxValue;

        public bool CanHold(int amount) => IsUnlimited || Coins.TotalBronze + amount <= Capacity;

        public bool TryAdd(int amount)
        {
            if (CanHold(amount))
            {
                Coins.Add(amount);
                return true;
            }
            return false;
        }

        public bool TrySpend(int amount) => Coins.TrySpend(amount);

        public override string ToString() => Coins.ToString();
    }

}
