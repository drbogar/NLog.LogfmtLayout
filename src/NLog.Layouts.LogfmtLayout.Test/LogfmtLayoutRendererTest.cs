using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NLog.Layouts.LogfmtLayout.Test
{
    [TestClass]
    public class LogfmtLayoutRendererTest
    {
        readonly string loggerName = "TestLogger";
        readonly DateTime dateTime = DateTime.Now;
        readonly string message = "Hello Logfmt! :)";
        readonly LogLevel logLevel = LogLevel.Info;
        readonly string hostname = Dns.GetHostName();

        [TestMethod]
        public void CanRenderLogfmt()
        {
            LogfmtLayoutRenderer logfmtLayoutRenderer =
                new LogfmtLayoutRenderer();

            LogEventInfo logEvent = new LogEventInfo
            {
                LoggerName = loggerName,
                Level = logLevel,
                Message = message,
                TimeStamp = dateTime,
            };

            string renderedLogfmt = logfmtLayoutRenderer.Render(logEvent);
            string expectedDateTime = dateTime.ToString("o");
            string expectedLogfmt =
                string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "ts={0} level={1} msg=\"{2}\" module={3}",
                expectedDateTime,
                logLevel.ToString().ToLowerInvariant(),
                message,
                loggerName);

            Assert.AreEqual(expectedLogfmt, renderedLogfmt);
        }

        [TestMethod]
        public void CanRenderLogfmtWithProperties()
        {
            LogfmtLayoutRenderer logfmtLayoutRenderer =
                new LogfmtLayoutRenderer();

            int intValue = (new Random()).Next();
            char charValue = 'a';
            DateTime dateTimeValue = new DateTime(2015, 08, 30, 13, 47, 27);
            bool booleanValue = true;
            object objectValue = new
            {
                Id = 1001,
                Username = "pongracz.gergely",
                Enabled = true
            };

            LogEventInfo logEvent = new LogEventInfo
            {
                LoggerName = loggerName,
                Level = logLevel,
                Message = message,
                TimeStamp = dateTime,
                Properties =
                {
                    { "int", intValue },
                    { "char", charValue },
                    { "dateTime", dateTimeValue },
                    { "boolean", booleanValue },
                    { "Object", objectValue }
                }
            };

            string renderedLogfmt = logfmtLayoutRenderer.Render(logEvent);
            string expectedDateTime = dateTime.ToString("o");
            string expectedLogfmt =
                string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "ts={0} level={1} msg=\"{2}\" int={3} char={4} date_time={5} boolean={6} object=\"{7}\" module={8}",
                expectedDateTime,
                logLevel.ToString().ToLowerInvariant(),
                message,
                intValue,
                charValue,
                dateTimeValue.ToString("o"),
                booleanValue,
                objectValue,
                loggerName);

            Assert.AreEqual(expectedLogfmt, renderedLogfmt);
        }

        [TestMethod]
        public void CanRenderLogfmtWithScopeProperties()
        {
            LogfmtLayoutRenderer logfmtLayoutRenderer =
                new LogfmtLayoutRenderer();

            string spo = "one";
            string spt = "two";
            
            LogEventInfo logEvent = new LogEventInfo
            {
                LoggerName = loggerName,
                Level = logLevel,
                Message = message,
                TimeStamp = dateTime,
            };

            string expectedDateTime = dateTime.ToString("o");

            using (ScopeContext.PushProperty("scopePropertyOne", spo))
            {
                using (ScopeContext.PushProperty("scopePropertyTwo", spt))
                {
                    string renderedLogfmt2 = logfmtLayoutRenderer.Render(logEvent);
                    string expectedLogfmt2 =
                        string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        "ts={0} level={1} msg=\"{2}\" scope_property_one={3} scope_property_two={4} module={5}",
                        expectedDateTime,
                        logLevel.ToString().ToLowerInvariant(),
                        message,
                        spo,
                        spt,
                        loggerName);

                    Assert.AreEqual(expectedLogfmt2, renderedLogfmt2);
                }

                string renderedLogfmt1 = logfmtLayoutRenderer.Render(logEvent);
                string expectedLogfmt1 =
                    string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "ts={0} level={1} msg=\"{2}\" scope_property_one={3} module={4}",
                    expectedDateTime,
                    logLevel.ToString().ToLowerInvariant(),
                    message,
                    spo,
                    loggerName);

                Assert.AreEqual(expectedLogfmt1, renderedLogfmt1);
            }
        }

        [TestMethod]
        public void CanRenderLogfmtWithScopeNesteds()
        {
            LogfmtLayoutRenderer logfmtLayoutRenderer =
                new LogfmtLayoutRenderer();

            string outerText = "Outer Scope";
            string innerText = "Inner Scope";

            LogEventInfo logEvent = new LogEventInfo
            {
                LoggerName = loggerName,
                Level = logLevel,
                Message = message,
                TimeStamp = dateTime
            };

            string expectedDateTime = dateTime.ToString("o");

            using (ScopeContext.PushNestedState(outerText))
            {
                InnerOperationAsync(logfmtLayoutRenderer, logEvent, expectedDateTime, outerText, innerText).Wait();

                string renderedLogfmt1 = logfmtLayoutRenderer.Render(logEvent);
                string expectedLogfmt1 =
                    string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "ts={0} level={1} msg=\"{2}\" nested_states=\"{3}\" module={4}",
                    expectedDateTime,
                    logLevel.ToString().ToLowerInvariant(),
                    message,
                    outerText,
                    loggerName);

                Assert.AreEqual(expectedLogfmt1, renderedLogfmt1);
            }
        }

        private async Task InnerOperationAsync(LogfmtLayoutRenderer logfmtLayoutRenderer, LogEventInfo logEvent, string expectedDateTime, string outerText, string innerText)
        {
            using (ScopeContext.PushNestedState(innerText))
            {
                string renderedLogfmt2 = logfmtLayoutRenderer.Render(logEvent);
                string expectedLogfmt2 =
                    string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "ts={0} level={1} msg=\"{2}\" nested_states=\"{3}>{4}\" module={5}",
                    expectedDateTime,
                    logLevel.ToString().ToLowerInvariant(),
                    message,
                    innerText,
                    outerText,
                    loggerName);

                Assert.AreEqual(expectedLogfmt2, renderedLogfmt2);
            }
        }

        [TestMethod]
        public void CanRenderLogfmtWithExceptions()
        {
            LogfmtLayoutRenderer logfmtLayoutRenderer =
                new LogfmtLayoutRenderer();

            Exception innerException = new Exception("This is a scary \"exception\" monster.");
            Exception exception = new Exception("This is a trojal horse exception!", innerException);

            LogEventInfo logEvent = new LogEventInfo
            {
                LoggerName = loggerName,
                Level = logLevel,
                Message = message,
                TimeStamp = dateTime,
                Exception = exception
            };

            string renderedLogfmt = logfmtLayoutRenderer.Render(logEvent);
            string expectedDateTime = dateTime.ToString("o");
            string expectedLogfmt =
                string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "ts={0} level={1} msg=\"{2}\" module={3} exc=\"{4}\"",
                expectedDateTime,
                logLevel.ToString().ToLowerInvariant(),
                message,
                loggerName,
                exception.ToString().Replace('\"', '\'').Replace('\\', '/'))
                .Replace(Environment.NewLine, "|");

            Assert.AreEqual(expectedLogfmt, renderedLogfmt);
        }

        [TestMethod]
        public void CanRenderLogfmtWithEqualSignInValue()
        {
            LogfmtLayoutRenderer logfmtLayoutRenderer =
                new LogfmtLayoutRenderer();

            string strWithEqSign = "SGVsbG8gTG9nZm10ISA6KQ==";

            LogEventInfo logEvent = new LogEventInfo
            {
                LoggerName = loggerName,
                Level = logLevel,
                Message = message,
                TimeStamp = dateTime,
                Properties =
                {
                    { "str_with_eq_sign", strWithEqSign },
                }
            };

            string renderedLogfmt = logfmtLayoutRenderer.Render(logEvent);
            string expectedDateTime = dateTime.ToString("o");
            string expectedLogfmt =
                string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "ts={0} level={1} msg=\"{2}\" str_with_eq_sign=\"{3}\" module={4}",
                expectedDateTime,
                logLevel.ToString().ToLowerInvariant(),
                message,
                strWithEqSign,
                loggerName);

            Assert.AreEqual(expectedLogfmt, renderedLogfmt);
        }
    }
}
