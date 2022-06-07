Follow these guidelines, in no particular order, to improve your chances of having a pull request merged in.

### Before you do anything else

 * DO: Read about [getting help](https://restsharp.dev/support/#get-help) in the docs.
 * DO: Follow the guidelines below when contributing.  
 * DO: Discuss bigger change in the issue before implementing it.  
 * DO NOT: Use issues to ask questions about using the library.

### Once a contribution is ready to be submitted

 * Make each pull request atomic and exclusive; don't send pull requests for a laundry list of changes.
 * Even better, commit in small manageable chunks.
 * Spaces, not tabs. Bracket style doesn't matter. Do not reformat code you didn't touch.
 * Changes should be accompanied by unit tests to show what was broken and how your patch fixes it.
 * No regions except for license header
 * Code must build for .NET 4.5.2 and .NET Standard 2.0
 * If you didn't write the code you must provide a reference to where you obtained it and preferably the license. 
 * Use autocrlf=true `git config --global core.autocrlf true` http://help.github.com/dealing-with-lineendings/
