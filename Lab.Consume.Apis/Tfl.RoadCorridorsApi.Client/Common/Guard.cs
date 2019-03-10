using System;

namespace Tfl.RoadCorridorsApi.Client.Common
{
    public static class Guard
    {
        public static void Against<TException>(bool argumentValue, string message) where TException : Exception
        {
            if (!argumentValue) return;
            throw (TException)Activator.CreateInstance(typeof(TException), message);
        }
    }
}
