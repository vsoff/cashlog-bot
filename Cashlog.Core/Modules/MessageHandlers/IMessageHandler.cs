using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cashlog.Core.Common;
using Cashlog.Core.Models;
using Cashlog.Core.Modules.Messengers;

namespace Cashlog.Core.Modules.MessageHandlers
{
    public interface IMessageHandler
    {
        public MessageType MessageType { get; }
        public Task HandleAsync(UserMessageInfo userMessageInfo);
    }
}