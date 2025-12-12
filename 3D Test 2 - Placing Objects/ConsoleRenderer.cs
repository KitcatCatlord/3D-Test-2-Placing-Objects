using System.Text;

public static class ConsoleRenderer
{
    static int width;
    static int height;

    public static void Init()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;

        width = Console.WindowWidth - 2;
        height = Console.WindowHeight;
    }

    public static char[,] CreateBuffer()
    {
        return new char[height, width];
    }
    public static void clearBuffer(char[,] buffer)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                buffer[y, x] = ' ';
            }
        }
    }
    public static bool DrawChar(char[,] buffer, int x, int y, char c)
    {
        if (x >= 0 && x < width && y >= 0 && y < height) { buffer[y, x] = c; return false; }
        return true;
    }
    public static bool DrawString(char[,] buffer, int x, int y, string s)
    {
        bool failed = false;
        for (int i = 0; i < s.Length; i++) failed = DrawChar(buffer, x + i, y, s[i]);
        return failed;
    }
    public static void DrawColourString(int x, int y, string s, string hex)
    {
        if (hex.StartsWith("#")) hex = hex.Substring(1);
        if (hex.Length != 6) return;

        int r = Convert.ToInt32(hex.Substring(0, 2), 16);
        int g = Convert.ToInt32(hex.Substring(2, 2), 16);
        int b = Convert.ToInt32(hex.Substring(4, 2), 16);

        Console.SetCursorPosition(x, y);
        Console.Write($"\u001b[38;2;{r};{g};{b}m{s}\u001b[0m");
    }
    public static void DrawColourStringBG(int x, int y, string s, string fgHex, string bgHex)
    {
        if (fgHex.StartsWith("#")) fgHex = fgHex.Substring(1);
        if (bgHex.StartsWith("#")) bgHex = bgHex.Substring(1);
        if (fgHex.Length != 6 || bgHex.Length != 6) return;

        int fr = Convert.ToInt32(fgHex.Substring(0, 2), 16);
        int fg = Convert.ToInt32(fgHex.Substring(2, 2), 16);
        int fb = Convert.ToInt32(fgHex.Substring(4, 2), 16);

        int br = Convert.ToInt32(bgHex.Substring(0, 2), 16);
        int bg = Convert.ToInt32(bgHex.Substring(2, 2), 16);
        int bb = Convert.ToInt32(bgHex.Substring(4, 2), 16);

        Console.SetCursorPosition(x, y);
        Console.Write($"\u001b[48;2;{br};{bg};{bb}m\u001b[38;2;{fr};{fg};{fb}m{s}\u001b[0m");
    }

    public static void Render(char[,] buffer)
    {
        Console.SetCursorPosition(0, 0);
        var sb = new StringBuilder(width * height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                sb.Append(buffer[y, x]);
            }
            if (y < height - 1) sb.Append('\n');
        }

        Console.Write(sb.ToString());
    }

    public static void Render(char[,] buffer, (int x, int y, string text, string fgHex, string bgHex)[] overlays)
    {
        Console.SetCursorPosition(0, 0);
        var sb = new StringBuilder(width * height);

        for (int y = 0; y < height; y++)
        {
            bool hasOverlay = false;
            int ox = 0;
            string text = "";
            string fgHex = "";
            string bgHex = "";

            for (int i = 0; i < overlays.Length; i++)
            {
                if (overlays[i].y == y)
                {
                    hasOverlay = true;
                    ox = overlays[i].x;
                    text = overlays[i].text;
                    fgHex = overlays[i].fgHex;
                    bgHex = overlays[i].bgHex;
                    break;
                }
            }

            if (!hasOverlay)
            {
                for (int x = 0; x < width; x++)
                {
                    sb.Append(buffer[y, x]);
                }
            }
            else
            {
                int x = 0;
                while (x < width && x < ox)
                {
                    sb.Append(buffer[y, x]);
                    x++;
                }

                string hex = fgHex;
                if (hex.StartsWith("#")) hex = hex.Substring(1);
                int fr = Convert.ToInt32(hex.Substring(0, 2), 16);
                int fg = Convert.ToInt32(hex.Substring(2, 2), 16);
                int fb = Convert.ToInt32(hex.Substring(4, 2), 16);

                string esc;
                if (!string.IsNullOrEmpty(bgHex))
                {
                    string bh = bgHex;
                    if (bh.StartsWith("#")) bh = bh.Substring(1);
                    int br = Convert.ToInt32(bh.Substring(0, 2), 16);
                    int bg = Convert.ToInt32(bh.Substring(2, 2), 16);
                    int bb = Convert.ToInt32(bh.Substring(4, 2), 16);
                    esc = $"\u001b[48;2;{br};{bg};{bb}m\u001b[38;2;{fr};{fg};{fb}m{text}\u001b[0m";
                }
                else
                {
                    esc = $"\u001b[38;2;{fr};{fg};{fb}m{text}\u001b[0m";
                }

                sb.Append(esc);
                x = ox + text.Length;

                while (x < width)
                {
                    sb.Append(buffer[y, x]);
                    x++;
                }
            }

            if (y < height - 1) sb.Append('\n');
        }

        Console.Write(sb.ToString());
    }
}
