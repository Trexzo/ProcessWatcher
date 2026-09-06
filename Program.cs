namespace ProcessWatcher;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            Application.Run(new MainForm());
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.ToString(),
                "ProcessWatcher startup error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
