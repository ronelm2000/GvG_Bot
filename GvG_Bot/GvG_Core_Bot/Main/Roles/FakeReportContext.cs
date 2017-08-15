using System;
using System.Collections.Generic;
using System.Text;

namespace GvG_Core_Bot.Main.Roles
{
    class FakeReportContext
    {
        public string MethodName { get; private set; }
        public string Output { get; private set; }

        private FakeReportContext() { }
        public FakeReportContext (string MethodName, string Output)
        {
            this.MethodName = MethodName;
            this.Output = Output;
        }

        private static FakeReportContext _empty = new FakeReportContext() { MethodName = "", Output = "" };
        public static FakeReportContext Empty { get => _empty; }
    }
    
}
