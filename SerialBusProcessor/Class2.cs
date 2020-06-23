using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SerialBusProcessor
{
    public delegate void TestProcessCallback(object[] args);
    class TestProcessor:Object
    {
        private SerialBusProcessor serialBusProcessor;
        public TestProcessCallback TestHook { get; set; }
        private Control uictrl;
        private bool testfinished;
        private bool TestFinished { get { return testfinished; } }
        public TestProcessor(Control ctrl,SerialBusProcessor sbp)
        {
            uictrl = ctrl;
            serialBusProcessor = sbp;
        }

        public bool StartTest()
        {
            if (testfinished == false)
                return false;
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(test_threadfuc));
            testfinished = false;
            thread.Start(uictrl);
            return true;
        }
        private void test_threadfuc(object arg)
        {
            TestHook(new object[] { arg });
            testfinished = true;
        }
    }
}
