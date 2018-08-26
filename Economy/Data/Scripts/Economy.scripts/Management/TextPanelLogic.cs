namespace Economy.scripts.Management
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.ModAPI;
    using System;
    using System.Linq;
    using VRage.Game.Components;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TextPanel), false)]
    public class TextPanelLogic : MyGameLogicComponent
    {
        #region fields

        private MyObjectBuilder_EntityBase _objectBuilder;
        private bool _isInitilized;
        private IMyTextPanel _textPanel;

        #endregion

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            _objectBuilder = objectBuilder;
            if (MyAPIGateway.Multiplayer.IsServer)
            {
                this.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;

                if (!_isInitilized)
                {
                    // Use this space to hook up events. NOT TO PROCESS ANYTHING.
                    _isInitilized = true;
                    _textPanel = (IMyTextPanel) Entity;
                }
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            if (!_isInitilized ||
                EconomyScript.Instance == null ||
                EconomyScript.Instance.ServerConfig == null ||
                !EconomyScript.Instance.IsServerRegistered ||
                !EconomyScript.Instance.IsReady ||
                !EconomyScript.Instance.ServerConfig.EnableLcds)
                return;

            if (_textPanel.IsFunctional
                && _textPanel.IsWorking
                && EconomyConsts.LCDTags.Any(tag => _textPanel.CustomName.IndexOf(tag, StringComparison.InvariantCultureIgnoreCase) >= 0))
            {
                LcdManager.ProcessLcdBlock(_textPanel);
            }
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return _objectBuilder;
        }
    }
}