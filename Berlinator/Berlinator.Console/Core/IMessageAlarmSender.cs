using System.Threading.Tasks;

namespace Berlinator.Console.Core
{
    public interface IMessageAlarmSender
    {
        Task SendMessage(string text);
    }
}
