using System;
using System.Threading.Tasks;

namespace EasyH.Communication.Interfaces
{
    /// <summary>
    /// 连接基础类
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// 连接ID
        /// </summary>
        string ConnectionId { get; }

        /// <summary>
        /// 断开连接
        /// </summary>
        Task Disconnect();


    }
}
