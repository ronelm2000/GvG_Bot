using GvG_Core_Bot.Main.Messages;
using System;

namespace GvG_Core_Bot.Main
{
    [Serializable]
    internal class GvGGameException : Exception
    {
        public GvGGameException()
        {
        }

        public GvGGameException(string message) : base(message)
        {
        }

        public GvGGameException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    [Serializable]
    internal class AlreadyOngoingGameGvGGameException : GvGGameException
    {
        public AlreadyOngoingGameGvGGameException() : base(ResultMessages.CreateGameException_OngoingGame)
        {
        }

        public AlreadyOngoingGameGvGGameException(string message) : base(message)
        {
        }

        public AlreadyOngoingGameGvGGameException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}