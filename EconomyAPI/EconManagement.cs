namespace EconomyAPI
{
    using System;
    using Sandbox.ModAPI;
    using VRage.Game;

    /// <summary>
    /// Main Economy class to subcribe and listen to callbacks events.
    /// This only required for callbacks. If callsbacks are not required, then you do not need to subscribe.
    /// </summary>
    public class EconManagement
    {
        private readonly Action<object> _interModMessageHandler = new Action<object>(InterModHandleMessage);
        private bool _isSubscribed;
        private long _callbackChannel;

        /// <summary>
        /// Subscribe to the Economy API for callbacks frmo the Economy Mod.
        /// </summary>
        /// <param name="callbackChannel">Specifiy the channel to be used for callbacks. This needs to be unique for your mod.</param>
        public void Subscribe(long callbackChannel)
        {
            if (_isSubscribed
                || MyAPIGateway.Utilities == null
                || MyAPIGateway.Session == null
                || MyAPIGateway.Multiplayer == null)
                return;

            if (MyAPIGateway.Multiplayer.IsServer
                || MyAPIGateway.Session.OnlineMode.Equals(MyOnlineModeEnum.OFFLINE))
            {
                //MyAPIGateway.Multiplayer.MultiplayerActive

                _callbackChannel = callbackChannel;
                MyAPIGateway.Utilities.RegisterMessageHandler(_callbackChannel, _interModMessageHandler);
                _isSubscribed = true;
                VRage.Utils.MyLog.Default.WriteLine(string.Format("Economy API Callback has subscribed to channel {0}", _callbackChannel));
            }
        }

        public void Unsubscribe()
        {
            if (!_isSubscribed || MyAPIGateway.Utilities == null)
                return;

            MyAPIGateway.Utilities.UnregisterMessageHandler(_callbackChannel, _interModMessageHandler);
            _isSubscribed = false;
            VRage.Utils.MyLog.Default.WriteLine(string.Format("Economy API Callback has unsubscribed to channel {0}", _callbackChannel));
        }

        public bool IsSubscribed { get { return _isSubscribed; } }

        private static void InterModHandleMessage(object data)
        {
            if (data == null)
            {
                VRage.Utils.MyLog.Default.WriteLine("Economy API Callback message is empty. This could indicate the API is out of date, or using an existing subscribed channel.");
                return;
            }

            string xml = data as string;
            if (xml == null)
            {
                VRage.Utils.MyLog.Default.WriteLine("Economy API Callback message is invalid. This means indicate the API is out of date, or using an existing subscribed channel.");
                return;
            }

            // "Start Message Serialization";
            EconMessageContainer message;

            try
            {
                message = MyAPIGateway.Utilities.SerializeFromXML<EconMessageContainer>(xml);
            }
            catch
            {
                VRage.Utils.MyLog.Default.WriteLine("Economy API Callback message cannot be deserialized. This means the API is out of date, or using an existing subscribed channel.");
                return;
            }

            // "End Message Serialization";

            if (message != null && message.Content != null)
            {
                try
                {
                    message.Content.ProcessEconomyCallback();
                }
                catch (Exception e)
                {
                    VRage.Utils.MyLog.Default.WriteLine("Economy API Callback message threw an exception. This means the API is out of date, or further processing by the mod that consumes the Economy API caused an error. See the exception detail for further information: " + e);
                }
            }
        }
    }
}
