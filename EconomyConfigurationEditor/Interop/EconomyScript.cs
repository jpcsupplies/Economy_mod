namespace Economy.scripts
{
    using Economy.scripts.EconStructures;

    /// <summary>
    /// This class is simply a placeholder to substitue as the data store for the Editor. 
    /// It simply replicates the core data pieces from the Mod, so we can call the same functions.
    /// </summary>
    public class EconomyScript
    {
        internal static EconomyScript Instance;

        public TextLogger ServerLogger = new TextLogger(); // This is a dummy logger until Init() is called.
        public TextLogger ClientLogger = new TextLogger(); // This is a dummy logger until Init() is called.

        /// Ideally this data should be persistent until someone buys/sells/pays/joins but
        /// lacking other options it will triggers read on these events instead. bal/buy/sell/pay/join
        public EconDataStruct Data;
        public EconConfigStruct ServerConfig;
        public ReactivePricingStruct ReactivePricing;

        static EconomyScript()
        {
            Instance = new EconomyScript();
        }
    }
}
