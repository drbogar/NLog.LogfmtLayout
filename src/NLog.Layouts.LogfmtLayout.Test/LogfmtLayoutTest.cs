using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NLog.Layouts.LogfmtLayout.Test
{
    [TestClass]
    public class LogfmtLayoutTest
    {
        [TestMethod]
        public void CanShowContextWithAsyncTaskTarget()
        {
            LogfmtLayout logfmtLayout = new LogfmtLayout();
            DebugTarget debugTarget = new DebugTarget();
            debugTarget.Layout = logfmtLayout;
            AsyncTargetWrapper asyncTargetWrapper = new AsyncTargetWrapper();
            asyncTargetWrapper.WrappedTarget = debugTarget;
            asyncTargetWrapper.QueueLimit = 5;
            asyncTargetWrapper.OverflowAction = AsyncTargetWrapperOverflowAction.Block;
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(asyncTargetWrapper, LogLevel.Debug);

            Logger logger = LogManager.GetLogger("TestLogger");
            using (logger.PushScopeProperty("aScopeProperty", "This is a scope property value from the scope context."))
            {
                using (logger.PushScopeNested("ScopeNested"))
                {
                    logger.WithProperty("logEventProperty", "log event property").Info("Hello World!");
                }
            }
            LogManager.Flush();

            string expectedLogfmtPattern = "ts=\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}\\.\\d{7}\\+02:00 level=info msg=\"Hello World!\" log_event_property=\"log event property\" a_scope_property=\"This is a scope property value from the scope context.\" nested_states=\"ScopeNested\" module=TestLogger";
            string actualLogfmt = debugTarget.LastMessage;
            Assert.IsTrue(Regex.IsMatch(actualLogfmt, expectedLogfmtPattern), "Actual Logfmt: " + actualLogfmt);
            Assert.AreEqual(1, debugTarget.Counter, "debugTarget.Counter=" + debugTarget.Counter);
        }
    }
}
