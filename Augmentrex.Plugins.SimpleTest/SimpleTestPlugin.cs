using System.Reflection;

namespace Augmentrex.Plugins.SimpleTest
{
    public sealed class SimpleTestPlugin : Plugin
    {
        public override string Name { get; } =
            Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title;

        public override void Start(AugmentrexContext context)
        {
            context.InfoLine("Simple test plugin started.");
        }

        public override void Stop(AugmentrexContext context)
        {
            context.InfoLine("Simple test plugin stopped.");
        }
    }
}
