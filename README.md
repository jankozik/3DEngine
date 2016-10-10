# 3DEngine
Simple 3D rendering engine entirely written in VB.NET

Main features:
- Objects are provided by defining their vertices and an included Delaunay algorithm performs the necessary triangulations to extract the object's faces.
- Multiple rendering modes, including ZBuffering.
- Multiple objects per scene.
- Fast GDI+ drawing.
- Save scene to GIF.

![Simple Solids](https://xfx.net/stackoverflow/3dengine_sample01.png)

Known issues:
- Transparency is not working.
- There seems to be a bug in the triangualtion algorithm which causes it to fail when triangualting the vertices of a sphere.
- Although support to save animated GIFs is implemented, the [GifBitmapEncoder](https://msdn.microsoft.com/en-us/library/system.windows.media.imaging.gifbitmapencoder(v=vs.110).aspx) does not seem to support saving animated GIFs.
