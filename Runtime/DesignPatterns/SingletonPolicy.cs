namespace AdriKat.Toolkit.DesignPatterns
{
    public enum SingletonPolicy
    {
        /// <summary>
        /// The first created instance remains, new ones get destroyed.
        /// </summary>
        FirstStays,
        /// <summary>
        /// The new instance replaces the old one.
        /// </summary>
        LastStays
    }
}
