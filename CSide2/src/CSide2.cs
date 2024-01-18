using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;

#pragma warning disable CS8618  //nullable field not declared as ?
#pragma warning disable CS0162  //unreachable code
#pragma warning disable CS0168  //var declared never used
#pragma warning disable CS8600  //converting null ref
#pragma warning disable CS8604  //possible null ref

class TextEditor {
    public static string decimalCharacters = "0123456789";

    public static List<string> lines;
    public static int virtualCursorX = 0;
    public static int virtualCursorY = 0;
    public static int offsetX = 0;
    public static int offsetY = 0;

    public static int scrollX = 0;
    public static int scrollY = 0;
    public static int scrollBufferX = 10;
    public static int scrollBufferY = 10;

    public static int commandBarHeight = 2;

    public static string exitStatus0 = "";  //tab switching
    public static string exitStatus1 = "";  //line number toggle
    public static string lastExitStatus1 = "";
    public static string exitStatus2 = "";
    public static string exitStatus3 = "";

    public static bool refresh = false;

    public static List<string> winTwoString = new List<string>();
    public static int winTwoSizeX = 0;

    public static bool highlighterEnable = false;
    public static List<string> highlighterKeywords = new List<string>();
    public static string highlighterQuoteColor = "";
    public static string highlighterCommentColor = "";

    public static bool highlighterWinhEnable = false;
    public static List<string> highlighterWinhKeywords = new List<string>();
    public static string highlighterWinhQuoteColor = "";
    public static string highlighterWinhCommentColor = "";

    public static void EditorMain(int whichBuffer, int startX, int startY) {
        lines = MainFunction.buffer[whichBuffer].Split('\n').ToList();

        offsetX = startX;
        offsetY = startY;

        winTwoString = WindowCompositor.contents[MainFunction.currentWindow].Split('\n').ToList();

        while (true) {
            Console.CursorVisible = false;

            /*new scope*/ {
                int i = 0;
                foreach (string line in lines.ToArray()) {
                    lines[i] = line.Replace("\r", "");

                    i++;
                }
            }

            string currentLine = lines[MainFunction.cursorY[MainFunction.currentWindow]];

            if (currentLine.Length < MainFunction.cursorX[MainFunction.currentWindow]) {
                MainFunction.cursorX[MainFunction.currentWindow] = currentLine.Length;
            } 

            if (MainFunction.windowWidth != TabCompositor.tabX[MainFunction.currentWindow]) {
                MainFunction.windowWidth = TabCompositor.tabX[MainFunction.currentWindow];
            }
            MainFunction.numberOfWindows = MainFunction.buffer.Count;

            Console.Clear();
            Clear.CB();

            MainFunction.windowHeight = Console.WindowHeight;
            MainFunction.textHeight = MainFunction.windowHeight - commandBarHeight;

            CalculateCursor();

            PrintWindow(scrollX, scrollY, startX, startY);

            if (exitStatus1 == "lineshow") {
                bool status = DisplayLineNumbers();

                if (status == false) {
                    return;
                }
            }

            bool haveWin = false;
            foreach (int win in WindowCompositor.winWhich) {
                if (win == MainFunction.currentWindow) {
                    haveWin = true;
                }
            }

            string saveString = "";
            foreach (string str in winTwoString) {
                saveString += str + "\n";
            }
            saveString = saveString.Remove(saveString.Length - 1, 1);
            WindowCompositor.contents[MainFunction.currentWindow] = saveString;

            if (winTwoSizeX > 0 && haveWin == true) {
                PrintWindowPlusContentWinh(WindowCompositor.scrollX[MainFunction.currentWindow], WindowCompositor.scrollY[MainFunction.currentWindow], Console.WindowWidth - winTwoSizeX + 2, 0, winTwoString.ToArray());

                PrintSeperatorForWindowTwo();
            }

            PrintStatus();

            if (refresh == true) {
                refresh = false;

                continue;
            }

            try {
                Console.SetCursorPosition(virtualCursorX, virtualCursorY);
            } catch (Exception) { }

            Console.CursorVisible = true;
            var key = Console.ReadKey(true);

            ///////////////
            //insert mode//
            ///////////////

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

                    case ConsoleKey.PageDown:
                        MainFunction.cursorY[MainFunction.currentWindow] += MainFunction.textHeight;

                        if (MainFunction.cursorY[MainFunction.currentWindow] > lines.Count - 1) {
                            MainFunction.cursorY[MainFunction.currentWindow] = lines.Count - 1;
                        }

                        break;

                    case ConsoleKey.PageUp:
                        MainFunction.cursorY[MainFunction.currentWindow] -= MainFunction.textHeight;

                        if (MainFunction.cursorY[MainFunction.currentWindow] < 0) {
                            MainFunction.cursorY[MainFunction.currentWindow] = 0;
                        }

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
                            if (MainFunction.cursorX[MainFunction.currentWindow] <= currentLine.Length && currentLine.Length > 1) {
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

            ///////////////
            //normal mode//
            ///////////////

            else if (MainFunction.status == "[Normal]") {
                switch (key.Key) {
                    case ConsoleKey.I:
                        MainFunction.status = "[Insert]";

                        break;

                    case ConsoleKey.T:
                        MainFunction.status = "[Tab]";

                        break;

                    case ConsoleKey.W:
                        MainFunction.status = "[Window]";

                        break;

                    case ConsoleKey.PageDown:
                        MainFunction.cursorY[MainFunction.currentWindow] += MainFunction.textHeight;

                        if (MainFunction.cursorY[MainFunction.currentWindow] > lines.Count - 1) {
                            MainFunction.cursorY[MainFunction.currentWindow] = lines.Count - 1;
                        }

                        break;

                    case ConsoleKey.PageUp:
                        MainFunction.cursorY[MainFunction.currentWindow] -= MainFunction.textHeight;

                        if (MainFunction.cursorY[MainFunction.currentWindow] < 0) {
                            MainFunction.cursorY[MainFunction.currentWindow] = 0;
                        }

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
                            if (MainFunction.cursorX[MainFunction.currentWindow] <= currentLine.Length && currentLine.Length > 1) {
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

            ////////////
            //tab mode//
            ////////////

            else if (MainFunction.status == "[Tab]") {
                switch (key.Key) {
                    case ConsoleKey.I:
                        MainFunction.status = "[Insert]";

                        break;

                    case ConsoleKey.W:
                        MainFunction.status = "[Window]";

                        break;
Console.WriteLine("\x1b[3J");
                    case ConsoleKey.LeftArrow:
                        exitStatus0 = "<-";
                        return;

                        break;

                    case ConsoleKey.RightArrow:
                        exitStatus0 = "->";
                        return;

                        break;

                    case ConsoleKey.Escape:
                        MainFunction.status = "[Normal]";

                        break;
                }
            }

            ///////////////
            //window mode//
            ///////////////

            else if (MainFunction.status == "[Window]") {
                switch (key.Key) {
                    case ConsoleKey.I:
                        MainFunction.status = "[Insert]";

                        break;

                    case ConsoleKey.Escape:
                        MainFunction.status = "[Normal]";

                        break;

                    case ConsoleKey.T:
                        MainFunction.status = "[Tab]";

                        break;

                    case ConsoleKey.PageDown:
                        WindowCompositor.scrollY[MainFunction.currentWindow] += MainFunction.textHeight;

                        break;

                    case ConsoleKey.PageUp:
                        WindowCompositor.scrollY[MainFunction.currentWindow] -= MainFunction.textHeight;

                        if (WindowCompositor.scrollY[MainFunction.currentWindow] < 0) {
                            WindowCompositor.scrollY[MainFunction.currentWindow] = 0;
                        }

                        break;

                    case ConsoleKey.LeftArrow:
                        WindowCompositor.scrollX[MainFunction.currentWindow]--;

                        if (WindowCompositor.scrollX[MainFunction.currentWindow] < 0) {
                            WindowCompositor.scrollX[MainFunction.currentWindow] = 0;
                        }

                        break;

                    case ConsoleKey.RightArrow:
                        WindowCompositor.scrollX[MainFunction.currentWindow]++;

                        break;

                    case ConsoleKey.DownArrow:
                        WindowCompositor.scrollY[MainFunction.currentWindow]++;

                        break;

                    case ConsoleKey.UpArrow:
                        WindowCompositor.scrollY[MainFunction.currentWindow]--;

                        if (WindowCompositor.scrollY[MainFunction.currentWindow] < 0) {
                            WindowCompositor.scrollY[MainFunction.currentWindow] = 0;
                        }

                        break;

                    case ConsoleKey.C:
                        CmdInterface.ToggleCmdWindow();

                        break;
                }
            }

            /////////////////////////////
            //check other keys and shit//
            /////////////////////////////

            if (key.KeyChar == ':' && MainFunction.status == "[Normal]") {
                MainFunction.statusBar += ":";

                ReadInputStatusBar();
                CommandHandler();
            }
            else if (key.Key == ConsoleKey.F1) {
                MainFunction.windowWidth = Console.WindowWidth;

                refresh = true;

                continue;
            }
            else if (decimalCharacters.Contains(key.KeyChar) && MainFunction.status == "[Normal]") {
                MultiShift(key.KeyChar.ToString(), currentLine);
            }
            else if (key.Key == ConsoleKey.F2) {
                DebuggerInterface.ToggleDebuggerWindow();
            }
            else if (key.KeyChar == '/' && MainFunction.status == "[Normal]") {
                ReadInputStatusBar();
                FindString();
            }
            

            ///////////////////////
            //check exit statuses//
            ///////////////////////

            if (exitStatus1 != lastExitStatus1) {
                lastExitStatus1 = exitStatus1;
            }

            return;
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

            string bufferForHighlight = "";
            try {
                for (int j = 0; j < input[i].Length && j < MainFunction.windowWidth; j++) {
                    if (highlighterEnable == false) {
                        Console.Write(input[i][j]);
                    } else {
                        bufferForHighlight += input[i][j];
                    }
                }
            }
            catch (Exception e) {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("~");
                Console.ForegroundColor = ConsoleColor.White;
            }

            if (highlighterEnable == true) {
                Colors.WriteColor(bufferForHighlight, highlighterKeywords.ToArray(), highlighterQuoteColor, highlighterCommentColor);
                bufferForHighlight = "";
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
                Console.Write(input[i + line]);
            }
            catch (Exception e) {
            }

            printY++;
        }
    }

    public static void PrintWindowPlusContentWinh(int column, int line, int startX, int startY, string[] input) {
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

            string bufferForHighlight = "";
            try {
                for (int j = 0; j < input[i + line].Length && j < MainFunction.windowWidth - scrollBufferX; j++) {
                    if (highlighterWinhEnable == false) {
                        Console.Write(input[i + line][j]);
                    }
                    else {
                        bufferForHighlight += input[i + line][j];
                    }
                }
            }
            catch (Exception e) {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("~");
                Console.ForegroundColor = ConsoleColor.White;
            }

            if (highlighterWinhEnable == true) {
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.White;
                Colors.WriteColor(bufferForHighlight, highlighterWinhKeywords.ToArray(), highlighterWinhQuoteColor, highlighterWinhCommentColor);
                bufferForHighlight = "";
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
        for (int i = 0; i < 5; i++) {
            int cursorX = MainFunction.cursorX[MainFunction.currentWindow] + offsetX;
            int cursorY = MainFunction.cursorY[MainFunction.currentWindow] + offsetY;

            /* calculate cursor X direction */ {
                (int visibleRangeSX, int visibleRangeEX) = (offsetX, offsetX);
                visibleRangeSX = scrollX + offsetX;
                visibleRangeEX = scrollX + (MainFunction.windowWidth - scrollBufferX);
    
                if (cursorX < visibleRangeSX || cursorX > visibleRangeEX) {
                    if (cursorX < visibleRangeSX) {
                        scrollX -= visibleRangeSX - cursorX;
                    } else if (cursorX > visibleRangeEX) {
                        scrollX += cursorX - visibleRangeEX;
                    }
                } else {
                    virtualCursorX = cursorX - scrollX;
                }
            }

            /* calculate cursor Y direction */ {
                (int visibleRangeSY, int visibleRangeEY) = (offsetY, offsetY);
                visibleRangeSY = scrollY + offsetY;
                visibleRangeEY = scrollY + (MainFunction.textHeight - scrollBufferY);

                if (cursorY < visibleRangeSY || cursorY > visibleRangeEY) {
                    if (cursorY < visibleRangeSY) {
                        scrollY -= visibleRangeSY - cursorY;
                    } else if (cursorY > visibleRangeEY) {
                        scrollY += cursorY - visibleRangeEY;
                    }
                } else {
                    virtualCursorY = cursorY - scrollY;
                }
            }
        }
    }

    /*
     * old calculate cursor function
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
    */

    public static void MultiShift(string allChar, string currentLine) {
        while (true) {
            MainFunction.statusBar = "Shift: " + allChar + " characters";
            PrintStatus();

            ConsoleKeyInfo key = Console.ReadKey(true);

            int shift = Convert.ToInt32(allChar);
            if (decimalCharacters.Contains(key.KeyChar)) {
                allChar += key.KeyChar;
            }
            else if (key.Key == ConsoleKey.LeftArrow) {
                for (int i = 0; i < shift; i++) {
                    if (MainFunction.cursorX[MainFunction.currentWindow] > 0) {
                        MainFunction.cursorX[MainFunction.currentWindow]--;
                    }
                }
                break;
            }
            else if (key.Key == ConsoleKey.RightArrow) {
                for (int i = 0; i < shift; i++) {
                    if (MainFunction.cursorX[MainFunction.currentWindow] <= currentLine.Length && currentLine.Length > 1) {
                        MainFunction.cursorX[MainFunction.currentWindow]++;
                    }
                    else if (currentLine.Length == 1 && MainFunction.cursorX[MainFunction.currentWindow] < currentLine.Length) {
                        MainFunction.cursorX[MainFunction.currentWindow]++;
                    }
                }
                break;
            }
            else if (key.Key == ConsoleKey.UpArrow) {
                for (int i = 0; i < shift; i++) {
                    if (MainFunction.cursorY[MainFunction.currentWindow] > 0) {
                        MainFunction.cursorY[MainFunction.currentWindow]--;
                    }
                }
                break;
            }
            else if (key.Key == ConsoleKey.DownArrow) {
                for (int i = 0; i < shift; i++) {
                    if (MainFunction.cursorY[MainFunction.currentWindow] < lines.Count - 1) {
                        MainFunction.cursorY[MainFunction.currentWindow]++;
                    }
                }
                break;
            }
            else {
                break;
            }
        }
        
        MainFunction.statusBar = "";
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
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.White;

        Console.SetCursorPosition(virtualCursorX, virtualCursorY);
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

    public static void CommandHandler(string command = "") {
        if (command == "") {
            command = MainFunction.statusBar.Split(' ')[0];
        }

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

            MainFunction.buffer.Add(File.ReadAllText(command).Replace("\t", "    "));
            MainFunction.FILEPATH.Add(command);
            MainFunction.cursorX.Add(0);
            MainFunction.cursorY.Add(0);
            MainFunction.posX.Add(0);
            MainFunction.posY.Add(0);
            MainFunction.saved.Add(true);
            WindowCompositor.contents.Add("");

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
                WindowCompositor.contents.RemoveAt(index);
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
        else if (command == ":winh") {
            try {
                command = MainFunction.statusBar.Substring(6);

                MakeNewWindow(File.ReadAllLines(command), Console.WindowWidth / 2);

                WindowCompositor.scrollX.Add(0);
                WindowCompositor.scrollY.Add(0);
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
                Console.ReadKey(true);
            }
        }
        else if (command == ":wind") {
            RemoveWindow(Console.WindowWidth);
        }
        else if (command == ":q!") {
            MainFunction.statusBar = "Press Y key to quit, all data will be lost";

            //its a miracle that I coded this function decently
            PrintStatus();

            var key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.Y) {
                Environment.Exit(0);
            }
        }
        else if (command == ":q") {
            //save all
            MainFunction.Save();

            int i = 0;
            foreach (string filePath in MainFunction.FILEPATH) {
                File.WriteAllText(filePath, MainFunction.buffer[i]);
                MainFunction.saved[i] = true;

                i++;
            }

            //quit
            Environment.Exit(0);
        }
        else if (command == ":ltog") {
            if (exitStatus1 == "lineshow") {
                exitStatus1 = "";
            }
            else {
                exitStatus1 = "lineshow";
            }
        }
        else if (command == ":hl") {
            try {
                string highlightName = MainFunction.statusBar.Split(' ')[1];

                if (!Directory.Exists("highlighters")) {
                    Directory.CreateDirectory("highlighters");

                    throw new Exception("no highlighter directory, now created");
                }

                if (highlightName == "!") {
                    goto defaultcolor;
                }

                string[] keywordsAndQuoteColor = File.ReadAllLines("highlighters/" + highlightName);

                highlighterEnable = true;
                
                //this is written so much better than TTE-v1
                highlighterKeywords.Clear();
                highlighterCommentColor = "";
                highlighterQuoteColor = "";

                foreach (string keyword in keywordsAndQuoteColor) {
                    string[] tmp0 = keyword.Split(' ');

                    if (tmp0[0] == "quote") {
                        highlighterQuoteColor = tmp0[1];
                    }
                    else if (tmp0[0] == "comment") {
                        highlighterCommentColor = tmp0[1];
                    }
                    else {
                        highlighterKeywords.Add(keyword);
                    }
                }

                if (highlighterQuoteColor == "") {
                    highlighterQuoteColor = "white";
                }

                if (highlighterCommentColor == "") {
                    highlighterCommentColor = "white";
                }

                MainFunction.statusBar = "Loaded: " + highlightName;
                PrintStatus();
                Console.ReadKey(true);

                goto outofcmp;
            } catch (Exception ex) {
                Console.WriteLine(ex);
                Console.ReadKey(true);
            }
        
        //default color "subroutine"
        defaultcolor:
            highlighterEnable = false;
        }
        else if (command == ":hlw") {
            try {
                string highlightName = MainFunction.statusBar.Split(' ')[1];

                if (!Directory.Exists("highlighters")) {
                    Directory.CreateDirectory("highlighters");

                    throw new Exception("no highlighter directory, now created");
                }

                if (highlightName == "!") {
                    goto defaultcolor;
                }

                string[] keywordsAndQuoteColor = File.ReadAllLines("highlighters/" + highlightName);

                highlighterWinhEnable = true;
                highlighterWinhCommentColor = "";
                highlighterWinhQuoteColor = "";
                
                //this is written so much better than TTE-v1
                highlighterWinhKeywords.Clear();

                foreach (string keyword in keywordsAndQuoteColor) {
                    string[] tmp0 = keyword.Split(' ');

                    if (tmp0[0] == "quote") {
                        highlighterWinhQuoteColor = tmp0[1];
                    }
                    else if (tmp0[0] == "comment") {
                        highlighterWinhCommentColor = tmp0[1];
                    }
                    else {
                        highlighterWinhKeywords.Add(keyword);
                    }
                }

                if (highlighterWinhQuoteColor == "") {
                    highlighterWinhQuoteColor = "white";
                }

                if (highlighterWinhCommentColor == "") {
                    highlighterWinhCommentColor = "white";
                }

                MainFunction.statusBar = "Loaded: " + highlightName;
                PrintStatus();
                Console.ReadKey(true);

                goto outofcmp;
            } catch (Exception ex) {
                Console.WriteLine(ex);
                Console.ReadKey(true);
            }
        
        //default color "subroutine"
        defaultcolor:
            highlighterWinhEnable = false;
        }
        else if (command == ":goto") {
            try {
                int gotoLine = Convert.ToInt32(MainFunction.statusBar.Split(' ')[1]);
                if (gotoLine < 0) {
                    gotoLine = 0;
                }
                if (gotoLine > lines.Count - 1) {
                    gotoLine = lines.Count - 1;
                }

                MainFunction.cursorY[MainFunction.currentWindow] = gotoLine;
            } catch (Exception ex) {
                Console.WriteLine(ex);
                Console.ReadKey(true);
            }
        }
        else if (command != "") {
            MainFunction.statusBar = "Unknown command";
            PrintStatus();
            Console.ReadKey(true);
        }

    outofcmp:
        MainFunction.statusBar = "";
    }

    public static void ReadInputStatusBar() {
        int i = 1;
        while (true) {
            PrintStatus();
            Console.SetCursorPosition(i, Console.WindowHeight - 1);
            var cc = Console.ReadKey(true);
            char c = cc.KeyChar;

            if (Char.IsLetterOrDigit(c) || Char.IsPunctuation(c) || Char.IsSymbol(c) || (c == ' ')) {
                MainFunction.statusBar += c;
                i++;
            }
            else if (cc.Key == ConsoleKey.Backspace) {
                MainFunction.statusBar = MainFunction.statusBar.Remove(MainFunction.statusBar.Length - 1);
                i--;
            }
            else if (cc.Key == ConsoleKey.Enter) {
                break;
            }
            else if (cc.Key == ConsoleKey.Escape) {
                MainFunction.statusBar = "";

                break;
            }

            if (MainFunction.statusBar == "") {
                break;
            }
        }
    }

    public static void MakeNewWindow(string[] displayString, int wantedSize) {
        for (int i = 0; i < displayString.Length; i++) {
            displayString[i] = displayString[i].Replace("\t", "    ");
        }

        winTwoString = displayString.ToList();
        winTwoSizeX = wantedSize;

        int resize = Console.WindowWidth - wantedSize - MainFunction.posX[MainFunction.currentWindow];
        TabCompositor.ResizeTab(MainFunction.currentWindow, resize);

        WindowCompositor.AddWindow();
    }

    public static void RemoveWindow(int restoreSize) {
        winTwoSizeX = 0;
        winTwoString.Clear();

        TabCompositor.ResizeTab(MainFunction.currentWindow, restoreSize);

        WindowCompositor.RemoveWindow();
    }

    public static bool DisplayLineNumbers() {
        List<string> ints = new List<string>();
        int i = 0;
        foreach (string s in lines) {
            string number = i.ToString();
            string space = "";
            for (int j = 0; j < MainFunction.posX[MainFunction.currentWindow] - number.Length - 1; j++) {
                space += " ";
            }
            ints.Add(space + number);

            i++;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        PrintWindowPlusContent(0, scrollY, 0, 0, ints.ToArray());
        Console.ForegroundColor = ConsoleColor.White;

        return true;
    }

    public static void PrintSeperatorForWindowTwo() {
        for (int i = 0; i < MainFunction.textHeight; i++) {
            Console.SetCursorPosition(Console.WindowWidth - Console.WindowWidth / 2, i);
            Console.Write("*");
        }
    }

    public static void ClearConsoleLineSelectable(int startLine, int howManyLines) {
        Console.SetCursorPosition(0, startLine);

        for (int i = 0; i < howManyLines; i++) {
            Console.Write(new string(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, startLine + i);
        }
    }

    public static void FindString() {
        
    }
}

//this only works on WindowsNT and Unix based systems
class CmdInterface {
    public static List<string> historyCmd = new List<string>();
    public static List<string> typedCmd = new List<string>();
    public static int scrollYCmd = 0;

    public static string ExecuteCommand(string command) {
        string output = string.Empty;

        try {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();

            if (IsWindows()) {
                processStartInfo.FileName = "cmd.exe";
                processStartInfo.Arguments = "/c " + command;
            }
            else {
                processStartInfo.FileName = "/bin/bash";
                processStartInfo.Arguments = "-c \"" + command + "\"";         
            }

            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true;

            Process process = new Process() {
                StartInfo = processStartInfo
            };
            process.Start();

            output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();
        } catch (Exception ex) {
            output = $"An error occurred: {ex.Message}";
        }

        return output;
    }

    private static bool IsWindows() {
        var os = Environment.OSVersion;
        return os.Platform == PlatformID.Win32NT || os.Platform == PlatformID.Win32S || os.Platform == PlatformID.Win32Windows || os.Platform == PlatformID.WinCE;
    }

    public static void ToggleCmdWindow() {
        Console.Clear();
        Clear.CB();

        Console.SetCursorPosition(0, 0);
        PrintCmdHistory(0);
        
        while (true) {
            Console.Write($"\u001b[32;1m{Environment.UserName}@{Environment.MachineName}\u001b[0m:\u001b[34;1m{Directory.GetCurrentDirectory()}\u001b[0m$ ");
            string input = Console.ReadLine();

            switch (input) {
                case "?esc<0>":
                    goto outofcmd;

                    break;
            }

            typedCmd.Add(input);

            string output = ExecuteCommand(input);
            historyCmd.Add(output);
            Console.Write(output);
        }

    outofcmd:
        MainFunction.status = "[Normal]";
    }

    private static void PrintCmdHistory(int startLine) {
        for (int i = startLine; i < historyCmd.Count; i++) {
            Console.Write($"{Environment.UserName}@{Environment.MachineName}$ ");
            Console.Write(typedCmd[i] + "\n");
            Console.Write(historyCmd[i]);
        }
    }
}

class Clear {
    //clears the scroll back buffer
    public static void CB() {
        Console.WriteLine("\x1b[3J");
    }
}

class DebuggerInterface {
    public static List<string> historyDebug = new List<string>();

    public static void ToggleDebuggerWindow() {
        Console.Clear();
        Clear.CB();
        Console.SetCursorPosition(0, 0);

        Console.WriteLine("(!!) WARN: This debugger has 0 safety guard on it!");
        Console.Write("Use at your own risk, press any key to continue...");
        Console.ReadKey(true);
        Console.WriteLine();

        bool exit = false;
        bool reprint = false;
        while (exit != true) {
            if (reprint == true) {
                Console.Clear();
                Clear.CB();

                foreach (string s in historyDebug) {
                    Console.Write(s);
                }

                reprint = false;
            }
            Console.Write("> ");
            Console.ForegroundColor = ConsoleColor.Red;
            string input = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;

            string command = input.Split(' ')[0];
            historyDebug.Add("> \u001b[31m" + input + "\u001b[0m\n");

            switch (command) {
                case "":
                    break;

                case "help":
                case "?":
                    HelpClass.HelpMenu();
                    Console.Clear();
                    Clear.CB();
                    reprint = true;

                    break;

                case "exit":
                case "quit":
                    exit = true;

                    break;

                case "clear":
                    historyDebug.Clear();
                    reprint = true;

                    break;

                case "save":
                    DebugClass.Save();

                    break;

                case "load":
                    DebugClass.Load();

                    break;

                case "new_cmd":
                    DebugClass.New_Cmd();
                    reprint = true;

                    break;

                case "del_cmd":
                    DebugClass.Del_Cmd();
                    reprint = true;

                    break;

                case "settabx":
                    DebugClass.SetTabX();

                    break;

                case "forceq":
                    Environment.Exit(0);

                    break;

                default:
                    PrintWH("(EE) ERR: Command not found!\n");

                    break;
            }
        }
    }

    public static void PrintWH(string input) {
        Console.Write(input);
        historyDebug.Add(input);
    }
}

class DebugClass {
    public static void Save() {
        try {
            DebuggerInterface.PrintWH("Which tab: ");
            int whichWin = Convert.ToInt32(Console.ReadLine());

            DebuggerInterface.historyDebug[DebuggerInterface.historyDebug.Count - 1] += whichWin.ToString() + "\n";

            if (whichWin == MainFunction.currentWindow) {
                File.WriteAllLines(MainFunction.FILEPATH[whichWin], TextEditor.lines);
            }
            else {
                File.WriteAllText(MainFunction.FILEPATH[whichWin], MainFunction.buffer[whichWin]);
            }

            MainFunction.saved[whichWin] = true;
        } catch (Exception ex) {
            DebuggerInterface.PrintWH(ex.ToString() + "\n");
        }
    }

    public static void Load() {
        try {
            DebuggerInterface.PrintWH("Which tab: ");
            int whichWin = Convert.ToInt32(Console.ReadLine());
            DebuggerInterface.historyDebug[DebuggerInterface.historyDebug.Count - 1] += whichWin.ToString() + "\n";

            DebuggerInterface.PrintWH("File path: ");
            string whichFile = Console.ReadLine();
            DebuggerInterface.historyDebug[DebuggerInterface.historyDebug.Count - 1] += whichFile + "\n";

            if (whichWin == MainFunction.currentWindow) {
                TextEditor.lines = File.ReadAllLines(whichFile).ToList();
            }
            else {
                MainFunction.buffer[whichWin] = File.ReadAllText(whichFile);
            }
        } catch (Exception ex) {
            DebuggerInterface.PrintWH(ex.ToString() + "\n");
        }
    }

    public static void New_Cmd() {
        DebuggerInterface.PrintWH("File path: ");
        string file = Console.ReadLine();
        DebuggerInterface.historyDebug[DebuggerInterface.historyDebug.Count - 1] += file + "\n";

        MainFunction.statusBar = ":new " + file;
        TextEditor.CommandHandler();
    }

    public static void Del_Cmd() {
        DebuggerInterface.PrintWH("Which tab: ");
        string which = Console.ReadLine();
        DebuggerInterface.historyDebug[DebuggerInterface.historyDebug.Count - 1] += which + "\n";

        MainFunction.statusBar = ":del " + which;
        TextEditor.CommandHandler();
    }

    public static void SetTabX() {
        try {
            DebuggerInterface.PrintWH("Which tab: ");
            int whichWin = Convert.ToInt32(Console.ReadLine());
            DebuggerInterface.historyDebug[DebuggerInterface.historyDebug.Count - 1] += whichWin.ToString() + "\n";

            DebuggerInterface.PrintWH("Size of X: ");
            string temp = Console.ReadLine();
            if (temp == "max") {
                temp = Console.WindowWidth.ToString();
            }
            int sizeX = Convert.ToInt32(temp);
            DebuggerInterface.historyDebug[DebuggerInterface.historyDebug.Count - 1] += sizeX.ToString() + "\n";

            TabCompositor.tabX[whichWin] = sizeX;
        } catch (Exception ex) {
            DebuggerInterface.PrintWH(ex.ToString() + "\n");
        }
    }
}

class HelpClass {
    public static string menuHeader = "help/";
    public static string menuWithin;

    public static void HelpMenu() {
        menuWithin = "";

        while (true) {
            Console.Clear();
            Clear.CB();
            Console.WriteLine("\u001b[33mType \"quit\" to quit\u001b[0m");
            Console.WriteLine("CDIR: " + menuHeader + menuWithin + "\n");

            foreach (string file in Directory.GetFiles(menuHeader + menuWithin)) {
                Console.WriteLine(file.Substring(menuHeader.Length + menuWithin.Length));
            }

            foreach (string folder in Directory.GetDirectories(menuHeader + menuWithin)) {
                Console.WriteLine(folder.Substring(menuHeader.Length + menuWithin.Length) + "/");
            }

            Console.Write("\n> ");
            string intoWithin = Console.ReadLine();

            if (intoWithin.Contains('/')) {
                try {
                    Directory.GetFiles(menuHeader + menuWithin + intoWithin);
                    menuWithin += intoWithin;
                } catch (Exception) {
                    Console.Write("Not a folder...");
                    Console.ReadKey(true);
                }
            } 
            else if (intoWithin == "." || intoWithin == "..") {
                menuWithin = RemoveLastDirectory(menuHeader + menuWithin).Substring(menuHeader.Length);
            } 
            else if (intoWithin == "quit") {
                return;
            } 
            else {
                try {
                    Console.Clear();
                    Clear.CB();
                    Console.WriteLine("\u001b[33mPress ESC key to leave\u001b[0m");
                    Console.WriteLine(File.ReadAllText(menuHeader + menuWithin + intoWithin));

                    Console.CursorVisible = false;
                    while (true) {
                        var keyPress = Console.ReadKey(true);

                        if (keyPress.Key == ConsoleKey.Escape) {
                            break;
                        }
                    }
                    Console.CursorVisible = true;
                } catch (Exception) {
                    Console.Write("Not a file...");
                    Console.ReadKey(true);
                }
            }
        }
    }

    public static string RemoveLastDirectory(string path) {
        if (string.IsNullOrEmpty(path)) {
            return path;
        }

        string trimmedPath = path.TrimEnd('/', '\\');
        int lastIndex = trimmedPath.LastIndexOfAny(new[] { '/', '\\' });

        if (lastIndex == -1) {
            return path;
        }

        return trimmedPath.Substring(0, lastIndex + 1);
    } 
}

class Colors {
    public static void WriteColor(string text, string[] keywords, string quoteColor, string commentColor) {
        foreach (string keyword in keywords) {
            string[] parts = keyword.Split(' ');

            if (parts.Length == 2) {
                string color = parts[0].ToLower();
                string word = parts[1];

                string startTag = GetColorStartTag(color);
                string endTag = GetColorEndTag();

                string pattern = $@"(?<![a-zA-Z0-9]){Regex.Escape(word)}(?![a-zA-Z0-9])";
                text = Regex.Replace(text, pattern, $"{startTag}$&{endTag}");
            }
        }

        string quotePattern = @"(""[^""]*"")|('[^']*')";
        string quoteStartTag = GetColorStartTag(quoteColor);
        string quoteEndTag = GetColorEndTag();

        text = Regex.Replace(text, quotePattern, $"{quoteStartTag}$&{quoteEndTag}");

        string commentPattern = @"(\/\/.*?(?=$|\r|\n)|\/\*[\s\S]*?\*\/)";
        string commentStartTag = GetColorStartTag(commentColor);
        string commentEndTag = GetColorEndTag();

        text = Regex.Replace(text, commentPattern, $"{commentStartTag}$&{commentEndTag}");

        Console.Write(text);
    }

    private static string GetColorStartTag(string color) {
        switch (color.ToLower()) {
            case "black":
                return "\u001b[30m";
            case "red":
                return "\u001b[31m";
            case "green":
                return "\u001b[32m";
            case "yellow":
                return "\u001b[33m";
            case "blue":
                return "\u001b[34m";
            case "magenta":
                return "\u001b[35m";
            case "cyan":
                return "\u001b[36m";
            case "white":
                return "\u001b[37m";
            case "gray":
                return "\u001b[90m";
            case "brightred":
                return "\u001b[91m";
            case "brightgreen":
                return "\u001b[92m";
            case "brightyellow":
                return "\u001b[93m";
            case "brightblue":
                return "\u001b[94m";
            case "brightmagenta":
                return "\u001b[95m";
            case "brightcyan":
                return "\u001b[96m";
            case "brightwhite":
                return "\u001b[97m";
            default:
                return "";
        }
    }

    private static string GetColorEndTag() {
        return "\u001b[0m";
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
    public static List<int> winWhich = new List<int>();  //this guy is special as it tells which tabs has windows, rather than storing for every tab
    public static List<string> contents = new List<string>();
    public static List<int> scrollX = new List<int>();
    public static List<int> scrollY = new List<int>();

    public static void AddWindow() {
        winWhich.Add(MainFunction.currentWindow);

        string saveString = "";
        foreach (string str in TextEditor.winTwoString) {
            saveString += str + "\n";
        }
        saveString = saveString.Remove(saveString.Length - 1, 1);
        contents[MainFunction.currentWindow] = saveString;
    }

    public static void RemoveWindow() {
        try {
            int i = 0;
            foreach (int win in winWhich) {
                if (win == MainFunction.currentWindow) {
                    winWhich.RemoveAt(i);

                    break;
                }

                i++;
            }

            scrollX.RemoveAt(MainFunction.currentWindow);
            scrollY.RemoveAt(MainFunction.currentWindow);
        } catch (Exception) {}
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

    public static int lineNumNumberBase = 10;  //default is decimal for line number display
    public static int spacerForLineNum = 1;

    public static void Main(string[] args) {
        //export source code
        try {
            Directory.CreateDirectory("src");
            File.WriteAllText("src/CSide2.cs", File.ReadAllText("/home/il1egalmelon/LVIM projects/CSide2.cs"));
        } catch (Exception) {}

        Console.ForegroundColor = ConsoleColor.White;
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.White;

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
        Clear.CB();

        Console.WriteLine("\x1b[3J");

        TabCompositor.NewTab(Console.WindowWidth);
        WindowCompositor.contents.Add("");
        WindowCompositor.scrollX.Add(0);
        WindowCompositor.scrollY.Add(0);

        bool boot = true;

        while (true) {
            buffer[currentWindow] = buffer[currentWindow].Replace("\t", "    ");

            bool toggleLineshow = false;
            if (TextEditor.exitStatus1 == "lineshow") {
                posX[currentWindow] = ((buffer[currentWindow].Split('\n').Length - 1).ToString()).Length + spacerForLineNum;
                TabCompositor.tabX[MainFunction.currentWindow] -= posX[currentWindow];
                toggleLineshow = true;
            }
            else {
                posX[currentWindow] = 0;
            }

            if (boot == true) {
                StartupConfigs();

                boot = false;
            }

            TextEditor.EditorMain(currentWindow, posX[currentWindow], posY[currentWindow]);

            Save();

            //restore windowWidth
            if (toggleLineshow == true) {
                TabCompositor.tabX[MainFunction.currentWindow] += posX[currentWindow];
            }

            if (TextEditor.exitStatus0 == "<-" || TextEditor.exitStatus0 == "->") {
                Swap();
            }
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
        if (TextEditor.exitStatus0 == "<-") {
            if (currentWindow == 0) {
            }
            else {
                currentWindow--;
            }
        } 
        else if (TextEditor.exitStatus0 == "->") {
            if (currentWindow + 1 >= buffer.Count) {
            }
            else {
                currentWindow++;
            }
        }

        TextEditor.scrollX = 0;
        TextEditor.scrollY = 0;
    }

    public static void StartupConfigs() {
        if (!Directory.Exists("configs")) {
            Directory.CreateDirectory("configs");
        }
        if (!File.Exists("configs/startup.txt")) {
            File.WriteAllText("configs/startup.txt", "");
        }

        string[] startupCommands = File.ReadAllLines("configs/startup.txt");

        foreach (string startupCmd in startupCommands) {
            statusBar = startupCmd;
            TextEditor.CommandHandler();
        }
    }
}
