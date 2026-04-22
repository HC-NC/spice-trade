using Spectre.Console;

namespace SpiceTrade.CLI.UI
{
    public static class LayoutFactory
    {
        public static Layout CreateRoot()
        {
            return new Layout().SplitRows(
                new Layout("Header").Size(3),
                new Layout("Main"),
                new Layout("Footer").Size(3)
                );
        }
    }
}
