using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ServiceModel.Channels;

namespace XPhoneRestApi
{
    public class SimpleLongPolling
    {
        private static List<SimpleLongPolling> _sSubscribers = new List<SimpleLongPolling>();

        public static string TIMEOUT = "TIMEOUT";

        public static bool ChannelExists(string channel)
        {
            lock (_sSubscribers)
            {
                var all = _sSubscribers.ToList();
                foreach (var poll in all)
                {
                    if (poll._Channel.ToLower() == channel.ToLower())
                        return true;
                }
            }
            return false;
        }

        public static void Publish(string channel, string message)
        {
            lock (_sSubscribers)
            {
                var all = _sSubscribers.ToList();
                foreach (var poll in all)
                {
                    if (poll._Channel.ToLower() == channel.ToLower())
                        poll.Notify(message);
                }
            }
        }

        private TaskCompletionSource<bool> _TaskCompletion = new TaskCompletionSource<bool>();

        private string _Channel { get; set; }
        private string _Message { get; set; }
        public SimpleLongPolling(string channel)
        {
            //this._TaskCompletion = new TaskCompletionSource<bool>();
            this._Channel = channel;
            this._Message = TIMEOUT;
            lock (_sSubscribers)
            {
                _sSubscribers.Add(this);
            }
        }

        private void Notify(string message)
        {
            this._Message = message;
            this._TaskCompletion.SetResult(true);
        }

        public async Task<string> WaitAsync()
        {
#if DEBUG
            const int PollingTimeout = 30000;
#else
            const int PollingTimeout = 30000;
#endif
            await Task.WhenAny(_TaskCompletion.Task, Task.Delay(PollingTimeout));
            string message = this._Message;
            lock (_sSubscribers)
            {
                _sSubscribers.Remove(this);
            }
            return message;
        }
    }
}
