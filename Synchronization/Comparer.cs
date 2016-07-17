using Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synchronization
{
    public class Comparer
    {
        public static bool AreEqual(IEnumerable<Account> firstSet, IEnumerable<Account> secondSet)
        {
            string firstSetPlain = JsonConvert.SerializeObject(firstSet);
            string secondSetPlain = JsonConvert.SerializeObject(secondSet);

            return firstSetPlain == secondSetPlain;
        }

        public static bool AreEqual(string firstSet, string secondSet)
        {
            return firstSet == secondSet;
        }
    }
}
