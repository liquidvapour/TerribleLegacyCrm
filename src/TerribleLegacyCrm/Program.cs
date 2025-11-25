using System.Windows.Forms;

namespace TerribleLegacyCrm;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        using var connection = Database.CreateOpenConnection();
        Application.Run(new MainCrazyForm(new CrmRepository(connection)));
    }
}
