namespace Cashlog.Core.Modules.MessageHandlers;

public static class MessageHandlerHelper
{
    public static string GetCommand(this string text, out string argument)
    {
        if (text == null || !text.StartsWith("/"))
            throw new ArgumentException(nameof(text));

        var firstSpaceIndex = text.IndexOf(' ');
        string command;
        if (firstSpaceIndex == -1)
        {
            command = text.Substring(1);
            argument = string.Empty;
        }
        else
        {
            command = text.Substring(1, firstSpaceIndex - 1);
            argument = text.Substring(firstSpaceIndex).Trim();
        }

        return command;
    }
}