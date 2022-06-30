using System;
using System.Text;
using NLog.Config;

namespace NLog.Layouts.LogfmtLayout
{
    [Layout("LogfmtLayout")]
    [ThreadAgnostic]
    public class LogfmtLayout : Layout
    {
        private readonly LogfmtLayoutRenderer _renderer = new LogfmtLayoutRenderer();
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            return _renderer.Render(logEvent);
        }

        protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            _renderer.RenderAppend(logEvent, target);
        }
    }
}
