using EasyH.Communication.Interfaces;

namespace EasyH.Communication.Core
{
    public static class MessageExtension
    {
        public static IMessage<T> ToMessage<T>(this IMessage message)
        {
            if (message is IMessage<T> genericMessage)
            {
                return genericMessage;
            }

            if (message.ObjectBody is T genericMessageBody)
            {
                return new Message<T>
                {
                    Body = genericMessageBody,
                    Time = message.Time,
                    MessageId = message.MessageId
                };
            }

            return default(Message<T>);
        }
    }
}
