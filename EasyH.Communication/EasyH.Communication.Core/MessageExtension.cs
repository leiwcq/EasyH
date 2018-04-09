using EasyH.Communication.Interfaces;

namespace EasyH.Communication.Core
{
    public static class MessageExtension
    {
        /// <summary>
        /// 将
        /// </summary>
        /// <typeparam name="T">消息体</typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
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
                    MessageType = message.MessageType,
                    Body = genericMessageBody,
                    Time = message.Time,
                    MessageId = message.MessageId
                };
            }

            return default(Message<T>);
        }
    }
}
