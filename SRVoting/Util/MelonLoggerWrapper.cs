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
#if DEBUG
        private readonly bool isDebug = true;
#else
        private readonly bool isDebug = false;
#endif

        private readonly MelonLogger.Instance melonLogger;

        public MelonLoggerWrapper(MelonLogger.Instance melonLogger)
        {
            this.melonLogger = melonLogger;
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
