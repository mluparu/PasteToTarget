# PasteToTarget
**Alt+V** to set a paste target. **Ctrl+C** to Copy _and_ Paste

This small VS editor extension turns Copy/Paste from **Search/Copy/Go-Back/Paste** to **Set-Paste-Target/Search/Copy**. Right after Ctrl+C, before pressing Ctrl+V, it's often frustrating to navigate back to the editor I started from. This tool will do that automatically when Ctrl+C is pressed. I find this helpful and decided to publish this extension just in case others might too. But your mileage may vary :)

##Before
Copy/Paste works best when you're browsing information and find something interesting and decide to save it or use it. It's not as great when you're actively coding and decide you need a piece of code that's somewhere else. For the untrained eye it may seem you're still only doing (1) Copy and (2) Paste but the reality is:

1. **Search**: You open several new editor tabs or potentially new VS instances or browsers to search for the needed information
2. **Copy**: Finally found it! Ctrl+C to copy it to Clipboard
3. **Go Back**: You need to find your way back to where you started: you find your VS session and switch back between editor tabs until you find the one you were initially on. Hopefully the cursor is still where you left it.
4. **Paste**: You press Ctrl+V to finally paste from Clipboard and you're back to work

##After
The idea is simple - reduce the interruption of Copy/Paste by saving you a step

1. **Set Target**: You know you need some code here. Alt+V to set the Paste target for Clipboard
2. **Search**: Same as before, find your code through piles of editor windows, new VS instances and browser pages
3. **Copy**: You found it? Ctrl+C and the text won't only go into the clipboard but also in the "target" you set in step #1. No need to find your VS instance and editor window, as it will automatically be brought into focus and the text will be pasted. You can now resume coding.

A paste target can be any selection in an editor including:
* A multi-line selection,
* A block selection or
* Just a caret position
