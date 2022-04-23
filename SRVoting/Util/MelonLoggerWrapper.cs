using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;

namespace SRVoting.Util
{
    public class MelonLoggerWrapper : ILogger
    {
        private readonly MelonLogger.Instance melonLogger;
        private bool isDebug;

        public MelonLoggerWrapper(MelonLogger.Instance melonLogger, bool isDebug = false)
        {
            this.melonLogger = melonLogger;
            this.isDebug = isDebug;
        }

        public void Msg(string message)
        {
            melonLogger.Msg(message);
        }

        public void Debug(string message)
        {
            if (isDebug)
            {
                melonLogger.Msg(message);
            }
        }
    }
}
