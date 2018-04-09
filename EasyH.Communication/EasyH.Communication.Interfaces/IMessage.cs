using System;

namespace EasyH.Communication.Interfaces
{
    /// <summary>
    /// 消息基础类
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// <para>消息类型</para>
        /// <para>通过此属性与具体的消息实体类型相匹配</para>
        /// <para>该功能通过反射进行实现</para>
        /// </summary>
        int MessageType { get; set; }

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
