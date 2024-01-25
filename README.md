# Tiny-Text-Editor-v2

Similar idea to Tiny Text Editor, the original one, just written in a less cancerous manner somewhat. It also has a lot of new capabilities that is already implemented and possible expansions in the future if I don't give up on them.
<br><br>
When loading a file with tab characters in it, it will replace the tab char with 4 spaces. It will also save as 4 spaces. I only expect you to load texts with single char width characters. Multi width char do not work as the cursor is misaligned. This is why the tab is cleaned to spaces.

Also, please don't use this for production. And, don't load any file that is larger than a few 10k lines long, it will be extremely slow or just not work at all.

To compile, please use the Microsoft Visual C# Compiler.

Help is located in the debugger. Steps: press F2 key, then type "help" or "?"

# Todo: <br>

    - (done) Add line numbers
    - Fix crashes when wrong arguments for some commands
    - Fix crashes for just random edge cases when editing text I guess 
      (I will just wrap everything in try catch loops, deal with it)
    - (done) Add text coloring
    - Add token based coloring 
      (I will let someone else do it, hehe)
    - (half) Add a built in command line (debugger)
    - (done) Add a way to open a window that is the system's command line
    - Add a way to communicate with external scripts (like LSPs and shit)
    - Add undo and redo
