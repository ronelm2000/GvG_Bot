using GvG_Core_Bot.Main.Roles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GvGCore.UnitTests
{
    [TestClass]
    public class ParseUnitTest
    {
        public TestContext Context { get; set; }

        [TestMethod]
        public void XxYParsesCorrectly()
        {
            Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Console.Out));
            var testString = "3x4,2x2,5x3";
            var test2D = Vector2D.Parse(testString);
            Trace.WriteLine($"{testString} Parse:");
            foreach (var val in test2D)
            {
                Trace.WriteLine($"X:{val.X}||Y:{val.Y}");
            }
            Assert.AreEqual(test2D.Count(), 3);

            var testString2 = "3X10010";
            test2D = Vector2D.Parse(testString2);
            foreach (var val in test2D)
            {
                Trace.WriteLine($"X:{val.X}||Y:{val.Y}");
            }
            Assert.AreEqual(test2D.Count(), 1);
            Assert.AreEqual(test2D.ElementAt(0).Y, 10010);
        }

    }
}
