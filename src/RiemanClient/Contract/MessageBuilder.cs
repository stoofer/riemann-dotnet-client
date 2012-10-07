namespace RiemanClient.Contract
{
    internal static class MessageBuilder
    {
        public static Message ToMessage(this string query)
        {
            return new Message{Query = new Query(query)};
        }

        public static Message ToMessage(this StateEntry[] states)
        {
            return new Message { States = states };
        }
    }
}