# Tiny-Text-Editor-v2

Similar idea to Tiny Text Editor, the original one, just written in a less cancerous manner somewhat. It also has a lot of new capabilities that is already implemented and possible expansions in the future if I don't give up on them.
<br><br>
When loading a file with tab characters in it, it will replace the tab char with 4 spaces. It will also save as 4 spaces. I only expect you to load texts with single char width characters. Multi width char do not work as the cursor is misaligned. This is why the tab is cleaned to spaces.

# Keybinds: <br>
Normal mode

    I       - Insert mode
    T       - Tab mode
    :       - Command mode
    LfArrow - Go left
    RtArrow - Go right
    UpArrow - Go up
    DwArrow - Go down

Insert mode

    ESC     - Normal mode
    Tab     - Adds 4 spaces, rather than a tab char
    LfArrow - Go left
    RtArrow - Go right
    UpArrow - Go up
    DwArrow - Go down

Tab mode

    ESC     - Normal mode
    I       - Insert mode
    LfArrow - Go to left tab
    RtArrow - Go to right tab

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
    
