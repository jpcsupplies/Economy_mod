namespace Economy.scripts
{
    public class EconomyConsts
    {
        /// <summary>
        /// The starting balance for all new players.
        /// </summary>
        /// <remarks>This will still be the default value if a Admin does not configure a custom starting balance.</remarks>
        public const decimal DefaultStartingBalance = 100;

        /// <summary>
        /// The is the Id which this mod registers iteself for sending and receiving messages through SE.
        /// </summary>
        /// <remarks>
        /// This Id needs to be unique with SE and other mods, otherwise it can send/receive  
        /// messages to/from the other registered mod by mistake, and potentially cause SE to crash.
        /// This has been generated randomly.
        /// </remarks>
        public const ushort ConnectionId = 46912;

        /// <summary>
        /// This is used to indicate the base communication version.
        /// </summary>
        /// <remarks>
        /// If we change Message classes or add a new Message class in any way, we need to update this number.
        /// This is because of potentional conflict in communications when we release a new version of the mod.
        /// ie., An established server will be running with version 1. We release a new version with different 
        /// communications classes. A Player will connect to the server, and will automatically download version 2.
        /// We would now have a Client running newer communication classes trying to talk to the Server with older classes.
        /// </remarks>
        public const int ModCommunicationVersion = 20150913; // This will be based on the date of update.
    }
}
