# Introduction #

The drawing box is a custom user control that allows the user to draw messages and retrive a  list of paths that can be easily manipulated


# Details #

## Functions of note: ##
**`Clear()`**
this clears the drawing box and removes all paths from view.

**`Bitmap getImage()`**
retrieve an image that can be saved to a variety of formats.

**`string getBase64Hash()`**
this serializes the paths into an compact format.

**`string serialize()`**
this method producecs a string that allows for complete duplication of the image with appropriate scaling. (this is what is sent across the wire)

---


## Class Variables: ##
_`List<PathData> myPaths`_: this is a list of the lines that make up the image. each PathData contains a line width, color and a graphics path.


---


Add your content here.  Format your content with:
  * Text in **bold** or _italic_
  * Headings, paragraphs, and lists
  * Automatic links to other wiki pages