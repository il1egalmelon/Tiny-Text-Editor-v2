using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

class TextEditor {
    public static List<string> lines;
    public static int virtualCursorX = 0;
    public static int virtualCursorY = 0;
    public static int offsetX = 0;
    public static int offsetY = 0;

    public static int scrollX = 0;
    public static int scrollY = 0;
    public static int scrollBufferX = 2;
    public static int scrollBufferY = 2;

    public static string exitstatus = "";

    private static bool refresh = false;

    public static void EditorMain(int whichBuffer, int startX, int startY) {
        lines = MainFunction.buffer[whichBuffer].Split('\n').ToList();

        offsetX = startX;
        offsetY = startY;

        while (true) {
            if (MainFunction.windowWidth != TabCompositor.tabX[MainFunction.currentWindow]) {
                MainFunction.windowWidth = TabCompositor.tabX[MainFunction.currentWindow];
            }
            MainFunction.numberOfWindows = MainFunction.buffer.Count;

            //ClearConsoleColumns(startX, Console.WindowWidth - startX);
            Console.Clear();

            MainFunction.windowHeight = Console.WindowHeight;
            MainFunction.textHeight = MainFunction.windowHeight - 2;

            string currentLine = lines[MainFunction.cursorY[MainFunction.currentWindow]];

            CalculateCursor();

            PrintWindow(scrollX, scrollY, startX, startY);

            PrintStatus();

            if (refresh == true) {
                refresh = false;

                continue;
            }

            try {
                Console.SetCursorPosition(virtualCursorX, virtualCursorY);
            } catch (Exception) { }

            var key = Console.ReadKey(true);

            if (MainFunction.status == "[Insert]") {
                switch (key.Key) {
                    case ConsoleKey.Escape:
                        MainFunction.status = "[Normal]";

                        break;

                    case ConsoleKey.Backspace:
                        if (MainFunction.cursorX[MainFunction.currentWindow] != 0) {
                            RemoveCharacter(MainFunction.cursorX[MainFunction.currentWindow] - 1);

                            MainFunction.cursorX[MainFunction.currentWindow]--;
                        }
                        else if (MainFunction.cursorY[MainFunction.currentWindow] != 0) {
                            string tmp = lines[MainFunction.cursorY[MainFunction.currentWindow]];
                            lines.RemoveAt(MainFunction.cursorY[MainFunction.currentWindow]);

                            MainFunction.cursorY[MainFunction.currentWindow]--;
                            int displacement = 1;
                            if (MainFunction.cursorX[MainFunction.currentWindow] == 0) {
                                displacement = 0;
                            }
                            MainFunction.cursorX[MainFunction.currentWindow] = lines[MainFunction.cursorY[MainFunction.currentWindow]].Length - displacement;
                            foreach (char insert in tmp.Reverse()) {
                                InsertCharacter(insert);
                            }
                        }

                        SetFalseSaved();

                        break;

                    case ConsoleKey.Enter:
                        InsertEnter();
                        MainFunction.cursorY[MainFunction.currentWindow]++;
                        MainFunction.cursorX[MainFunction.currentWindow] = 0;

                        SetFalseSaved();

                        break;

                    case ConsoleKey.Tab:
                        for (int i = 0; i < 4; i++) {
                            InsertCharacter(' ');
                            MainFunction.cursorX[MainFunction.currentWindow] += 1;
                        }

                        SetFalseSaved();

                        break;

                    case ConsoleKey.LeftArrow:
                        try {
                            if (MainFunction.cursorX[MainFunction.currentWindow] > 0) {
                                MainFunction.cursorX[MainFunction.currentWindow]--;
                            }
                        }
                        catch (Exception) { }

                        break;

                    case ConsoleKey.RightArrow:
                        try {
                            if (MainFunction.cursorX[MainFunction.currentWindow] < currentLine.Length - 1 && currentLine.Length > 1) {
                                MainFunction.cursorX[MainFunction.currentWindow]++;
                            }
                            else if (currentLine.Length == 1 && MainFunction.cursorX[MainFunction.currentWindow] < currentLine.Length) {
                                MainFunction.cursorX[MainFunction.currentWindow]++;
                            }
                        }
                        catch (Exception) { }

                        break;

                    case ConsoleKey.UpArrow:
                        try {
                            if (MainFunction.cursorY[MainFunction.currentWindow] > 0) {
                                MainFunction.cursorY[MainFunction.currentWindow]--;
                            }
                        }
                        catch (Exception) { }

                        break;

                    case ConsoleKey.DownArrow:
                        try {
                            if (MainFunction.cursorY[MainFunction.currentWindow] < lines.Count - 1) {
                                MainFunction.cursorY[MainFunction.currentWindow]++;
                            }
                        }
                        catch (Exception) { }

                        break;

                    default:
                        bool shit = InsertCharacter(key.KeyChar);
                        if (shit == true) {
                            MainFunction.cursorX[MainFunction.currentWindow]++;
                        }

                        SetFalseSaved();

                        break;
                }
            }
            else if (MainFunction.status == "[Normal]") {
                switch (key.Key) {
                    case ConsoleKey.I:
                        MainFunction.status = "[Insert]";

                        break;

                    case ConsoleKey.T:
                        MainFunction.status = "[Tab]";

                        break;

                    case ConsoleKey.LeftArrow:
                        try {
                            if (MainFunction.cursorX[MainFunction.currentWindow] > 0) {
                                MainFunction.cursorX[MainFunction.currentWindow]--;
                            }
                        }
                        catch (Exception) { }

                        break;

                    case ConsoleKey.RightArrow:
                        try {
                            if (MainFunction.cursorX[MainFunction.currentWindow] < currentLine.Length - 1 && currentLine.Length > 1) {
                                MainFunction.cursorX[MainFunction.currentWindow]++;
                            }
                            else if (currentLine.Length == 1 && MainFunction.cursorX[MainFunction.currentWindow] < currentLine.Length) {
                                MainFunction.cursorX[MainFunction.currentWindow]++;
                            }
                        }
                        catch (Exception) { }

                        break;

                    case ConsoleKey.UpArrow:
                        try {
                            if (MainFunction.cursorY[MainFunction.currentWindow] > 0) {
                                MainFunction.cursorY[MainFunction.currentWindow]--;
                            }
                        }
                        catch (Exception) { }

                        break;

                    case ConsoleKey.DownArrow:
                        try {
                            if (MainFunction.cursorY[MainFunction.currentWindow] < lines.Count - 1) {
                                MainFunction.cursorY[MainFunction.currentWindow]++;
                            }
                        }
                        catch (Exception) { }

                        break;
                }
            }
            else if (MainFunction.status == "[Tab]") {
                switch (key.Key) {
                    case ConsoleKey.LeftArrow:
                        exitstatus = "<-";
                        return;

                        break;

                    case ConsoleKey.RightArrow:
                        exitstatus = "->";
                        return;
                        
                        break;

                    case ConsoleKey.Escape:
                        MainFunction.status = "[Normal]";

                        break;
                }
            }

            /////////////////////////////
            //check other keys and shit//
            /////////////////////////////

            if (key.KeyChar == ':' && MainFunction.status == "[Normal]") {
                MainFunction.statusBar += ":";
                CommandHandler();
            } else if (key.Key == ConsoleKey.F1) {
                MainFunction.windowWidth = Console.WindowWidth;

                refresh = true;

                continue;
            }

            if (lines[MainFunction.cursorY[MainFunction.currentWindow]].Length < MainFunction.cursorX[MainFunction.currentWindow]) {
                MainFunction.cursorX[MainFunction.currentWindow] = lines[MainFunction.cursorY[MainFunction.currentWindow]].Length - 1;
            }
        }
    }

    public static void PrintWindow(int column, int line, int startX, int startY) {
        List<string> input = new List<string>();
        for (int i = 0; i < MainFunction.textHeight; i++) {
            try {
                input.Add(lines[i + line]);
            }
            catch (Exception) {
                break;
            }
        }

        for (int i = 0; i < input.Count; i++) {
            try {
                for (int j = 0; j < column; j++) {
                    input[i] = input[i].Remove(0, 1);
                }
            } catch (Exception) { }
        }

        int printY = startY;

        for (int i = 0; i < MainFunction.textHeight - startY; i++) {
            Console.SetCursorPosition(startX, printY);
            try {
                for (int j = 0; j < input[i].Length && j < MainFunction.windowWidth; j++) {
                    Console.Write(input[i][j]);
                }
            }
            catch (Exception e) {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("~");
                Console.ResetColor();
            }

            printY++;
        }

        /*
        foreach (string coutline in input) {
            Console.SetCursorPosition(startX, printY);
            Console.WriteLine(coutline);

            printY++;
        }
        */
    }

    public static void PrintWindowPlusContent(int column, int line, int startX, int startY, string[] input) {
        for (int i = 0; i < input.Length; i++) {
            try {
                for (int j = 0; j < column; j++) {
                    input[i] = input[i].Remove(0, 1);
                }
            }
            catch (Exception) { }
        }

        int printY = startY;

        for (int i = 0; i < MainFunction.textHeight - startY; i++) {
            Console.SetCursorPosition(startX, printY);
            try {
                for (int j = 0; j < input[i].Length && j < MainFunction.windowWidth - scrollBufferX; j++) {
                    Console.Write(input[i][j]);
                }
            }
            catch (Exception e) {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("~");
                Console.ResetColor();
            }

            printY++;
        }
    }

    public static void PrintAtCoord(int x, int y, string input) {
        Console.SetCursorPosition(x, y);
        Console.Write(input);
    }

    public static bool InsertCharacter(char c) {
        if (!(Char.IsLetterOrDigit(c) || Char.IsPunctuation(c) || Char.IsSymbol(c) || (c == ' ') || (c == '\b'))) {
            return false;
        }

        string temp = lines[MainFunction.cursorY[MainFunction.currentWindow]];

        string temp2 = temp.Insert(MainFunction.cursorX[MainFunction.currentWindow], c.ToString());

        /*
        Console.SetCursorPosition(0, 10);
        Console.WriteLine(c);
        Console.WriteLine(temp2);
        Console.SetCursorPosition(0, 0);

        Thread.Sleep(1000);
        */

        lines[MainFunction.cursorY[MainFunction.currentWindow]] = temp2;

        return true;
    }

    public static void SetFalseSaved() {
        MainFunction.saved[MainFunction.currentWindow] = false;
    }

    public static void RemoveCharacter(int index) {
        string tmp = lines[MainFunction.cursorY[MainFunction.currentWindow]].Remove(index, 1);
        lines[MainFunction.cursorY[MainFunction.currentWindow]] = tmp;
    }

    public static void InsertEnter() {
        string tmp0 = lines[MainFunction.cursorY[MainFunction.currentWindow]];
        tmp0 = tmp0.Insert(MainFunction.cursorX[MainFunction.currentWindow], "\n");

        string[] tmp1 = tmp0.Split('\n');
        lines[MainFunction.cursorY[MainFunction.currentWindow]] = tmp1[0];
        lines.Insert(MainFunction.cursorY[MainFunction.currentWindow] + 1, tmp1[1]);
    }

    public static void CalculateCursor() {
        virtualCursorX = MainFunction.cursorX[MainFunction.currentWindow] + offsetX;
        virtualCursorY = MainFunction.cursorY[MainFunction.currentWindow] + offsetY;
        scrollX = 0;
        scrollY = 0;

        if (MainFunction.cursorX[MainFunction.currentWindow] + offsetX >= MainFunction.windowWidth - scrollBufferX) {
            scrollX = MainFunction.cursorX[MainFunction.currentWindow] - MainFunction.windowWidth + scrollBufferX + offsetX;
            virtualCursorX = MainFunction.windowWidth - scrollBufferX;
        }

        if (MainFunction.cursorY[MainFunction.currentWindow] + offsetY >= MainFunction.textHeight - scrollBufferY) {
            scrollY = MainFunction.cursorY[MainFunction.currentWindow] - MainFunction.textHeight + scrollBufferY + offsetY;
            virtualCursorY = MainFunction.textHeight - scrollBufferY;
        }
    }

    public static void PrintStatus() {
        Console.SetCursorPosition(0, MainFunction.textHeight);

        //info bar
        ClearConsoleLine();
        Console.SetCursorPosition(0, MainFunction.textHeight);
        Console.BackgroundColor = ConsoleColor.White;
        Console.ForegroundColor = ConsoleColor.Black;
        string line = new string(' ', Console.WindowWidth);
        Console.Write(line);
        Console.SetCursorPosition(0, MainFunction.textHeight);
        int i = 0;
        Console.Write(" ");
        foreach (string filePath in MainFunction.FILEPATH) {
            if (MainFunction.saved[i] == false) {
                Console.Write("*");
            }
            Console.Write(i + ": " + filePath);

            if (i < MainFunction.FILEPATH.Count - 1) {
                Console.Write(" | ");
            }

            i++;
        }
        Console.WriteLine();
        Console.ResetColor();

        //command bar
        ClearConsoleLine();
        if (MainFunction.statusBar == "") {
            Console.Write(MainFunction.status);
        }
        else {
            Console.Write(MainFunction.statusBar);
        }

        //cursor position
        int y = MainFunction.textHeight;
        int x = Console.WindowWidth - 
            (MainFunction.cursorX[MainFunction.currentWindow].ToString().Length + MainFunction.cursorY[MainFunction.currentWindow].ToString().Length) - 3;
        string msg = MainFunction.cursorX[MainFunction.currentWindow].ToString() + ", " + MainFunction.cursorY[MainFunction.currentWindow].ToString();
        Console.BackgroundColor = ConsoleColor.White;
        Console.ForegroundColor = ConsoleColor.Black;
        PrintAtCoord(x, y, msg);
        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.Black;
    }

    public static void ClearConsoleLine() {
        int currentLineCursor = Console.CursorTop;
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, currentLineCursor);
    }

    public static void ClearConsoleColumns(int startIndex, int columnCount) {
        int currentTop = Console.CursorTop;
        int currentLeft = Console.CursorLeft;

        for (int i = startIndex; i < startIndex + columnCount; i++) {
            Console.SetCursorPosition(i, currentTop);
            Console.Write(' ');
        }

        Console.SetCursorPosition(currentLeft, currentTop);
    }

    public static void CommandHandler() {
        ReadInputStatusBar();

        string command = MainFunction.statusBar.Split(' ')[0];

        if (command == ":new") {
            try {
                command = MainFunction.statusBar.Substring(5);
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
                Console.ReadKey(true);
                goto outofcmp;
            }

            if (!File.Exists(command)) {
                File.WriteAllText(command, "");
            }

            MainFunction.buffer.Add(File.ReadAllText(command));
            MainFunction.FILEPATH.Add(command);
            MainFunction.cursorX.Add(0);
            MainFunction.cursorY.Add(0);
            MainFunction.posX.Add(0);
            MainFunction.posY.Add(0);
            MainFunction.saved.Add(true);

            TabCompositor.NewTab(MainFunction.windowWidth);
        }
        else if (command == ":del") {
            try {
                int index = Convert.ToInt32(MainFunction.statusBar.Split(' ')[1]);
                TabCompositor.DeleteTab(index);

                MainFunction.buffer.RemoveAt(index);
                MainFunction.FILEPATH.RemoveAt(index);
                MainFunction.cursorX.RemoveAt(index);
                MainFunction.cursorY.RemoveAt(index);
                MainFunction.posX.RemoveAt(index);
                MainFunction.posY.RemoveAt(index);
                MainFunction.saved.RemoveAt(index);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                Console.ReadKey(true);
            }
        }
        else if (command == ":res") {
            try {
                int index = Convert.ToInt32(MainFunction.statusBar.Split(' ')[1]);

                int size = 0;
                if (MainFunction.statusBar.Split(' ')[2] == "max") {
                    size = Console.WindowWidth;
                }
                else {
                    size = Convert.ToInt32(MainFunction.statusBar.Split(' ')[2]);
                }

                TabCompositor.ResizeTab(index, size);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                Console.ReadKey(true);
            }
        }
        else if (command == ":s") {
            MainFunction.Save();

            MainFunction.saved[MainFunction.currentWindow] = true;
            File.WriteAllText(MainFunction.FILEPATH[MainFunction.currentWindow], MainFunction.buffer[MainFunction.currentWindow]);
        }
        else if (command == ":sa") {
            MainFunction.Save();

            int i = 0;
            foreach (string filePath in MainFunction.FILEPATH) {
                File.WriteAllText(filePath, MainFunction.buffer[i]);
                MainFunction.saved[i] = true;

                i++;
            }
        }

    outofcmp:
        MainFunction.statusBar = "";
    }

    public static void ReadInputStatusBar() {
        while (true) {
            PrintStatus();
            var cc = Console.ReadKey(true);
            char c = cc.KeyChar;

            if (Char.IsLetterOrDigit(c) || Char.IsPunctuation(c) || Char.IsSymbol(c) || (c == ' ')) {
                MainFunction.statusBar += c;
            }
            else if (c == '\b') {
                MainFunction.statusBar = MainFunction.statusBar.Remove(MainFunction.statusBar.Length - 1);
            }
            else if (cc.Key == ConsoleKey.Enter) {
                break;
            }
            else if (cc.Key == ConsoleKey.Escape) {
                return;
            }

            if (MainFunction.statusBar == "") {
                return;
            }
        }
    }

    public static bool FileChecker() {
        return false;
    }

    public static void DisplayNewWindow() {
        string[] displayString = MainFunction.buffer[WindowCompositor.windowX.Count - 1].Split('\n');

        for (int i = 0; i < displayString.Length; i++) {
            displayString[i] = displayString[i].Replace("\t", "    ");
        }

        int winToDisplay = WindowCompositor.windowX.Count - 1;

        int totalCharToShift = 0;
        foreach (int window in WindowCompositor.windowX) {
            totalCharToShift += window;
        }
        totalCharToShift -= WindowCompositor.windowX[winToDisplay];

        PrintWindowPlusContent(0, 0, totalCharToShift, 0, displayString);

        WindowCompositor.createdNewWindow = false;
    }
}

class TabCompositor {
    public static List<int> tabX = new List<int>();

    public static void NewTab(int wantedSize) {
        if (wantedSize > Console.WindowWidth) {
            throw new Exception("(EE) Window creation failed!");
        }

        tabX.Add(wantedSize);
    }

    public static void DeleteTab(int index) {
        if (index > tabX.Count - 1) {
            throw new Exception("(EE) Window deletion failed!");
        }

        tabX.RemoveAt(index);
    }

    public static void ResizeTab(int index, int wantedSize) {
        if (wantedSize > Console.WindowWidth) {
            throw new Exception("(EE) Window resizing failed");
        }

        tabX[index] = wantedSize;
    }
}

class WindowCompositor {
    public static List<int> windowX = new List<int>();
    public static List<string> inputString = new List<string>();

    public static bool createdNewWindow = false;

    public static void CheckWindows() {
        //resize current window to appropriate size
        MainFunction.windowWidth = windowX[MainFunction.currentWindow];
    }

    public static void StartWindow(int wantedSizeX) {
        int windowTotalSizeX = Console.WindowWidth;

        if (wantedSizeX > windowTotalSizeX) {
            throw new Exception("(EE) Invalid window size");
        }
        else {
            windowX.Add(wantedSizeX);
            MainFunction.windowWidth = wantedSizeX;
        }
    }

    public static void ShrinkWindow(int index, int wantedSizeX) {
        windowX[index] = wantedSizeX;

        MainFunction.windowWidth = wantedSizeX;
    }

    public static void EnlargeWindow(int shrinkWindow, int index, int wantedSizeX) {

    }

    public static void NewWindow(int shrinkWindow, int wantedSizeX) {
        int windowTotalSizeX = Console.WindowWidth;

        int shrinkBy = windowX[shrinkWindow] - (windowTotalSizeX - wantedSizeX);
        ShrinkWindow(shrinkWindow, shrinkBy);

        windowX.Add(wantedSizeX);

        createdNewWindow = true;
    }

    public static void DeleteWindow(int index) {
        if (index == 0) {
            throw new Exception("(EE) Cannot delete main window, open new instance instead");
        }

        int useLater = windowX[index];
        windowX.RemoveAt(index);

        //enlarge window left to it
        windowX[index - 1] += useLater;

        MainFunction.buffer.RemoveAt(index);
        MainFunction.FILEPATH.RemoveAt(index);

        CheckWindows();
    }
}

class MainFunction {
    public static List<string> FILEPATH = new List<string>();
    public static List<bool> saved = new List<bool>();

    public static int windowHeight = 0;
    public static int windowWidth = 0;
    public static int textHeight;

    public static List<int> cursorX = new List<int>();
    public static List<int> cursorY = new List<int>();

    public static List<string> buffer = new List<string>();
    public static List<int> posX = new List<int>();
    public static List<int> posY = new List<int>();

    public static int currentWindow = 0;
    public static int numberOfWindows = 0;

    public static string status = "[Normal]";
    public static string statusBar = "";
    public static string commandBar = "";

    public static void Main(string[] args) {
        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.Black;

        if (args.Length == 1) {
            FILEPATH.Add(args[0]);

            if (!File.Exists(args[0])) {
                File.WriteAllText(args[0], "");
            }
        }
        else {
        enterlabel:
            Console.Write("Enter file path: ");
            string file = Console.ReadLine();

            if (!File.Exists(file)) {
                Console.Write("Create or not (Y / N): ");
                ConsoleKeyInfo pepe = Console.ReadKey(true);
                char c = pepe.KeyChar;
                Console.WriteLine();

                if (c == 'Y' || c == 'y') {
                    try {
                        File.WriteAllText(file, "");
                    } catch (Exception) {
                        Console.WriteLine("(EE) Unable to create file!");
                        goto enterlabel;
                    }
                }
                else {
                    goto enterlabel;
                }
            }

            FILEPATH.Add(file);
        }
        saved.Add(true);

        cursorX.Add(0);
        cursorY.Add(0);

        posX.Add(0);
        posY.Add(0);

        numberOfWindows = 1;

        buffer.Add(File.ReadAllText(FILEPATH[currentWindow]));

        Console.Clear();

        TabCompositor.NewTab(Console.WindowWidth);

        while (true) {
            buffer[currentWindow] = buffer[currentWindow].Replace("\t", "    ");

            TextEditor.EditorMain(currentWindow, posX[currentWindow], posY[currentWindow]);

            Save();
            Swap();
        }
    }

    public static void Save() {
        //save all lines
        string saveString = "";
        foreach (string str in TextEditor.lines) {
            saveString += str + "\n";
        }
        saveString = saveString.Remove(saveString.Length - 1, 1);
        buffer[currentWindow] = saveString;
    }

    public static void Swap() {
        if (TextEditor.exitstatus == "<-") {
            if (currentWindow == 0) {
            }
            else {
                currentWindow--;
            }
        } 
        else if (TextEditor.exitstatus == "->") {
            if (currentWindow + 1 >= buffer.Count) {
            }
            else {
                currentWindow++;
            }
        }

        TextEditor.scrollX = 0;
        TextEditor.scrollY = 0;
    }
}
