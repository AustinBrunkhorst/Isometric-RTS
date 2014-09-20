namespace MarsRTS
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (MarsRTS game = new MarsRTS())
            {
                game.Run();
            }
        }
    }
#endif
}

