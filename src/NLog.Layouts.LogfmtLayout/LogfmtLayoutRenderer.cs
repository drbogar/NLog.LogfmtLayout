using System;
using System.Collections.Generic;
using System.Text;
using NLog.LayoutRenderers;
using NLog.Config;
using System.Linq;
using System.Text.RegularExpressions;

namespace NLog.Layouts.LogfmtLayout
{
    // Recommended reading on this subject: https://brandur.org/logfmt
    [LayoutRenderer("logfmt")]
    public class LogfmtLayoutRenderer : LayoutRenderer
    {
        internal void RenderAppend(LogEventInfo logEvent, StringBuilder builder)
        {
            Append(builder, logEvent);
        }
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            builder.Append("ts=" + logEvent.TimeStamp.ToString("o") +
                " level=" + logEvent.Level.ToString().ToLowerInvariant() +
                " msg=\"" + logEvent.FormattedMessage.Replace('\"', '\'') + "\"");
            logEvent.Properties.ToList().ForEach(p =>
            {
                string valueStr = string.Empty;
                if (p.Value.GetType() == typeof(DateTime))
                {
                    valueStr = ((DateTime)p.Value).ToString("o");
                }
                else
                {
                    valueStr = p.Value.ToString().Replace('\"', '\'');
                    if (valueStr.Contains(" ") || valueStr.Contains("="))
                    {
                        valueStr = "\"" + valueStr + "\"";
                    }
                }

                builder.Append(" " + ToSnakeCase(p.Key.ToString()) + "=" + valueStr);
            });
            ScopeContext.GetAllProperties().ToList().ForEach(p =>
            {
                string valueStr = string.Empty;
                if (p.Value.GetType() == typeof(DateTime))
                {
                    valueStr = ((DateTime)p.Value).ToString("o");
                }
                else
                {
                    valueStr = p.Value.ToString().Replace('\"', '\'');
                    if (valueStr.Contains(" "))
                    {
                        valueStr = "\"" + valueStr + "\"";
                    }
                }

                builder.Append(" " + ToSnakeCase(p.Key.ToString()) + "=" + valueStr);
            });
            if (ScopeContext.GetAllNestedStates().Length > 0)
            {
                builder.Append(" nested_states=\"" + ((string)ScopeContext.GetAllNestedStates().ToList().Aggregate((acc, next) => acc + ">" + next)).Replace('\"', '\'') + "\"");
            }

            builder.Append(" module=" + logEvent.LoggerName + ((logEvent.Exception != null) ? " exc=\"" + logEvent.Exception.ToString().Replace('\"', '\'').Replace('\\', '/') + "\"" : ""));

            builder.Replace(Environment.NewLine, "|");
        }
        private static string ToSnakeCase(string text)
        {
            return (Regex.Replace(text, "(?<=[a-z0-9])[A-Z]", "_$0", RegexOptions.Compiled)).ToLowerInvariant();
        }
    }
}
