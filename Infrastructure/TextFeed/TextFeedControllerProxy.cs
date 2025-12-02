using Neuma.Core.TextFeed;

namespace Neuma.Infrastructure.TextFeed
{
    /// <summary>
    /// Proxy for TextFeed system.
    /// Solves the life cycle problem: InputHandler (Singleton) is created before UI (Scene).
    /// InputHandler talks to proxy, proxy redirects to the real UI, once it's loaded.
    /// </summary>
    public sealed class TextFeedControllerProxy : ITextFeedController
    {
        public ITextFeedController? RealController { get; set; }

        public bool IsTyping => RealController?.IsTyping ?? false;

        public void SkipOrAdvance()
        {
            if (RealController != null)
            {
                RealController.SkipOrAdvance();
            }
        }
    }
}

