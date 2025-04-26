namespace MarketStat.TechUI;

public class ConsoleApp
{
    private readonly IEnumerable<ICommand> _commands;

    public ConsoleApp(IEnumerable<ICommand> commands)
    {
        _commands = commands;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("MarketStat TechUI - type 'help' for commands");
        while (true)
        {
            Console.Write("> ");
            var input = (Console.ReadLine() ?? "").Split(' ', 2);
            var cmdName = input[0].Trim().ToLower();
            var cmdArgs = input.Length > 1 ? input[1].Split(' ') : Array.Empty<string>();

            if (cmdName == "exit")
            {
                break;
            }

            if (cmdName == "help")
            {
                foreach (var c in _commands)
                {
                    Console.WriteLine($"  {c.HelpText}");
                }
                continue;
            }
            
            var cmd = _commands.FirstOrDefault(c => c.Name == cmdName);
            if (cmd == null)
            {
                Console.WriteLine($"Unknown command '{cmdName}'. Try 'help'.");
                continue;
            }

            try
            {
                await cmd.ExecuteAsync(cmdArgs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }
        }
    }
}