using System;
using System.Runtime.Serialization;

namespace Livesplit.DarkSouls100Tracker.Logic
{
    [Serializable]
    internal class DarkSoulsWrongExeType : Exception
    {
        public DarkSoulsWrongExeType()
        {
        }

        public DarkSoulsWrongExeType(string message) : base(message)
        {
        }

        public DarkSoulsWrongExeType(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DarkSoulsWrongExeType(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}