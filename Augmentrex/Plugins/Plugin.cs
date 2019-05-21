namespace Augmentrex.Plugins
{
    public abstract class Plugin
    {
        public abstract string Name { get; }

        public virtual void Start(AugmentrexContext context)
        {
        }

        public virtual void Stop(AugmentrexContext context)
        {
        }
    }
}
