using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GvG_Core_Bot.Main;
using System.Collections.Generic;
using GvG_Core_Bot.Main.Roles;
using System.Linq;

namespace GvGCore.UnitTests
{
    [TestClass]
    public class RolesUnitTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void RandomTest()
        {
            int expected_result = 100;
            int expected_roles = 6;
            IEnumerable<IGameRole> roles = (new GvGGame(null, null)).GenerateGaiaRoles(expected_roles);
            string[] results = new string[Factorial(4) * expected_result];
            for (int i = 0; i < results.Length; i++) results[i] = (from x in roles select x.GetType().Name).Aggregate((x, y) => x + ", " + y);

            // results
            string[] distinct_combos = results.Distinct().ToArray();
            int[] distincts = results.Distinct().Select((x) => results.Count((y) => x.Equals(y))).ToArray();

            Assert.IsFalse(distincts.Max() > (expected_result * 1.2d), $"There is a distinct combo with {distincts.Max()} possibilities, more than what should be ({expected_result}).");
            //Assert.Inconclusive(Environment.NewLine + one + Environment.NewLine + two + Environment.NewLine + three);
        }

        private int Factorial(int number) => (number == 0) ? 1 : Factorial(number - 1) * number;
    }
}
