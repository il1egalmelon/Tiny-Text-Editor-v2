# Tiny-Text-Editor-v2

Similar idea to Tiny Text Editor, the original one, just written in a less cancerous manner somewhat. It also has a lot of new capabilities that is already implemented and possible expansions in the future if I don't give up on them.
<br><br>
When loading a file with tab characters in it, it will replace the tab char with 4 spaces. It will also save as 4 spaces. I only expect you to load texts with single char width characters. Multi width char do not work as the cursor is misaligned. This is why the tab is cleaned to spaces.

# Keybinds: <br>
Normal mode

    I       - Insert mode
    T       - Tab mode
    W       - Window mode
    :       - Command mode
    LfArrow - Go left
    RtArrow - Go right
    UpArrow - Go up
    DwArrow - Go down
    Page Up - Go up by a page
    Page Dw - Go down by a page

Insert mode

    ESC     - Normal mode
    Tab     - Adds 4 spaces, rather than a tab char
    LfArrow - Go left
    RtArrow - Go right
    UpArrow - Go up
    DwArrow - Go down
    Page Up - Go up by a page
    Page Dw - Go down by a page

Tab mode

    ESC     - Normal mode
    I       - Insert mode
    LfArrow - Go to left tab
    RtArrow - Go to right tab

Window mode

    ESC     - Normal mode
    I       - Insert mode
    T       - Tab mode
    LfArrow - Go left
    RtArrow - Go right
    UpArrow - Go up
    DwArrow - Go down
    Page Up - Go up by a page
    Page Dw - Go down by a page

Global

    F1      - Manual refresh

# Commands: <br>

    new <>  - New tab, <> file path
    del <>  - Delete a tab, <> tab number
    res <>  - Set resolution width, <> width number, max for max
    s       - Saves current file
    sa      - Saves all files
    winh <> - Opens a second readonly window, <> file path
    wind    - Removes the second window on the current tab
    q!      - Quit with force
    q       - Quit with save all
    ltog    - Toggle line number display for all tabs
    hl <>   - Sets the highlighter
    hlw <>  - Sets the highlighter for win2
    goto <> - Goto a line
    
# Todo: <br>

    - (done) Add line numbers
    - Fix crashes when wrong arguments for some commands
    - Fix crashes for just random edge cases when editing text I guess 
      (I will just wrap everything in try catch loops, deal with it)
    - (done) Add text coloring
    - Add token based coloring 
      (I will let someone else do it, hehe)
    - Add a built in command line (debugger)
    - Add a way to open a window that is the system's command line
    - Add a way to communicate with external scripts (like LSPs and shit)
    - Add undo and redo
    
# Some features: <br>

Startup:

    In a folder named "config", go to the "startup.txt" file.
    In there, you can add your own startup command line commands.
    Every line is one command.

    Example file:
        :hl c
        :winh test.c
        :goto 12

Text highlighting:

    This text editor has a very primative and sort of broken text highlighting feature.
    Just place your highlighting format file in the folder called "highlight".
    And you can load up the file in the command line with "highlight {}".

    The formatting for the highlighter file is as follows:
    {COLOR} {KEYWORD}
    or 
    quote {COLOR}

    These are the supported colors: black, red, green, yellow, blue, magenta, cyan, white

    Example file:
        quote cyan
        blue char
        blue short
        blue int
        blue long
        blue float
        blue double
        yellow void
        yellow unsigned
        yellow const
        yellow auto
        red if
        red else
        red break
        red continue
        red while
        red for
