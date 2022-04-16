﻿using System;
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

        public MelonLoggerWrapper(MelonLogger.Instance melonLogger)
        {
            this.melonLogger = melonLogger;
        }

        public void Msg(string message)
        {
            melonLogger.Msg(message);
        }
    }
}
