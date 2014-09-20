namespace MarsRTS.LIB.ScreenManager
{
    /// <summary>
    /// Game screen state
    /// </summary>
    public enum ScreenState
    {
        // transitioning in
        TransitionIn,
        // active, no transition
        Active,
        // transitioning out
        TransitionOut,
        // not active, and hidden
        Hidden,
    }
}
