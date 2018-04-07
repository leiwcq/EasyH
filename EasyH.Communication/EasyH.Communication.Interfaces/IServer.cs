using System.Threading.Tasks;

namespace EasyH.Communication.Interfaces
{
    /// <summary>
    /// 服务器基础类
    /// </summary>
    public interface IServer
    {
        /// <summary>
        /// 启动服务器
        /// </summary>
        Task Start();

        /// <summary>
        /// 停止服务器
        /// </summary>
        Task Stop();
    }
}
