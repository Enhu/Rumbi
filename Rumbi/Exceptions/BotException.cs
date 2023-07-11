using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Rumbi.Exceptions
{
    public class BotException : Exception
    {
        public BotException()
            : base() { }

        public BotException(string message)
            : base(message) { }

        public BotException(string message, params object[] args)
            : base(string.Format(CultureInfo.CurrentCulture, message, args)) { }
    }
}
