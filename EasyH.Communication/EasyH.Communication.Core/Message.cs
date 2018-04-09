using System;
using System.Runtime.Serialization;
using EasyH.Communication.Interfaces;
using EasyH.Core.Serialization;

namespace EasyH.Communication.Core
{
    /// <summary>
    /// 基础消息体
    /// </summary>
    [Serializable]
    [DataContract]
    public abstract class Message : IMessage
    {
        protected Message()
        {
            MessageId = String.Empty;
            Time = DateTimeOffset.UtcNow;
            MessageType = 0;
        }

        /// <summary>
        /// <para>消息类型</para>
        /// <para>通过此属性与具体的消息实体类型相匹配</para>
        /// <para>该功能通过反射进行实现</para>
        /// </summary>
        public int MessageType { get; set; }

        /// <summary>
        /// 消息ID
        /// </summary>
        [DataMember]
        public string MessageId { get; set; }

        /// <summary>
        /// 消息内容对象
        /// </summary>
        public virtual object ObjectBody { get; set; }

        /// <summary>
        /// <para>消息发生时间</para>
        /// <para>UTC</para>
        /// </summary>
        public DateTimeOffset Time { get; set; }

        /// <summary>
        /// 获取消息内容
        /// </summary>
        /// <returns>消息内容</returns>
        public override string ToString()
        {
            return this.ToJson();
        }
    }

    /// <summary>
    /// 基础消息体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    [DataContract]
    public class Message<T> : Message,IMessage<T>
    {

        /// <summary>
        /// 消息内容
        /// </summary>
        [DataMember]
        public T Body
        {
            get
            {
                if (ObjectBody is T variable)
                {
                    return variable;
                }

                return default(T);
            }
            set => ObjectBody = value;
        }
    }
}
