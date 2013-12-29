using System.IO;
using ProtoBuf;

namespace RiemannClient.Contract
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

        public static Message ToMessage(this EventRecord[] events)
        {
            return new Message { Events = events };
        }

        public static byte[] Serialize(this Message message)
        {
            var stream = new MemoryStream();
            Serializer.Serialize(stream, message);
            return stream.ToArray();
        }
    }
}