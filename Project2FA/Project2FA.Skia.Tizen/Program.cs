using Tizen.Applications;
using Uno.UI.Runtime.Skia;

namespace Project2FA.Skia.Tizen
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new TizenHost(() => new Project2FA.App(), args);
            host.Run();
        }
    }
}
