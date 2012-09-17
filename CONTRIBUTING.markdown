Follow these guidelines, in no particular order, to improve your chances of having a pull request merged in.

 * Make each pull request atomic and exclusive; don't send pull requests for a laundry list of changes.
 * Even better, commit in small manageable chunks.
 * Tabs, not spaces. Bracket style doesn't matter.
 * Changes to XmlDeserializer or JsonDeserializer must be accompanied by a unit test covering the change.
 * No regions except for license header
 * Code must build for .NET 3.5 Client Profile, Silverlight 4 and Windows Phone 7
 * If you didn't write the code you must provide a reference to where you obtained it and preferably the license. 
 * Use autocrlf=true `git config --global core.autocrlf true` http://help.github.com/dealing-with-lineendings/