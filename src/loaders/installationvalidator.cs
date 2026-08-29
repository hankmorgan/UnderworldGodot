namespace Underworld
{
    /// <summary>
    /// Decides whether a selected game installation can be used. Godot-free on purpose, so
    /// the headless suite covers it: the launcher, the demo rejection and the scene root all
    /// need this same answer, and all three live in Godot classes the suite cannot reach.
    /// </summary>
    public static class InstallationValidator
    {
        /// <summary>
        /// Once the demo was refused outright here, because nobody had compared its .SYS
        /// fonts to the parser and a record count says nothing about bitmap layout.
        ///
        /// That reason no longer holds. The provider now identifies a font by charsize,
        /// height and rowbytes, which does constrain the layout, and it does so against
        /// whatever the player actually has. Building fonts from the player's own data was
        /// the point of all this, so a demo install gets the demo's fonts checked on their
        /// own merits.
        ///
        /// Refusing outright also removed a configuration this port supports elsewhere, and
        /// stranded a player who never chose the demo: the launcher classifies any UW1
        /// folder containing UWDEMO.EXE as demo by itself.
        /// </summary>
        /// <summary>
        /// True when the installation at <paramref name="basePath"/> can be used. On false,
        /// <paramref name="error"/> carries a reason fit to show the player.
        /// </summary>
        public static bool TryValidate(byte res, string basePath, out string error)
        {
            string dataDir = string.IsNullOrWhiteSpace(basePath)
                ? null
                : System.IO.Path.Combine(basePath, "DATA");
            return SysFontProvider.TryLoadAll(dataDir, out _, out error);
        }
    }
}
