namespace Economy.scripts.EconStructures
{
    /// <summary>
    /// Names need to be explicitly set, as they will be written to the Data file.
    /// Otherwise if we change the names, they will break.
    /// </summary>
    public enum MarketZoneType
    {
        EntitySphere,
        FixedSphere,
        FixedfBox,
    }
}
