using System;

namespace EasyH.Communication.Interfaces
{
    /// <summary>
    /// 消息基础类
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        string MessageId { get; set; }

        /// <summary>
        /// 消息内容对象
        /// </summary>
        object ObjectBody { get; set; }

        /// <summary>
        /// <para>消息发生时间</para>
        /// <para>UTC</para>
        /// </summary>
        DateTimeOffset Time { get; set; }
    }

    /// <summary>
    /// 消息基础类
    /// </summary>
    public interface IMessage<T> : IMessage
    {
        /// <summary>
        /// 消息内容
        /// </summary>
        T Body { get; set; }
    }
}
