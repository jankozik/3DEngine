# 3DEngine
Simple 3D rendering engine entirely written in VB.NET

Main features:
- Objects are provided by defining their vertices and an included Delaunay algorithm performs the necessary triangulations to extract the objects' faces.
- Multiple rendering modes.
- Multiple objects per scene.
- Fast GDI+ drawing.
- Save scene to GIF.

![Simple Solids](https://xfx.net/stackoverflow/3DEngine/3dengine_sample01.png)

Known issues:
- Transparency is not working.
- There seems to be a bug in the triangulation algorithm which causes it to fail when tessellating the vertices of a sphere.
- Although support to save animated GIFs is implemented, the [GifBitmapEncoder](https://msdn.microsoft.com/en-us/library/system.windows.media.imaging.gifbitmapencoder(v=vs.110).aspx) does not seem to support saving animated GIFs.
- Clockwise rotations in the Rubik's cube sample are not currently supported.

Rubik's cube sample:

![Rubik's cube](https://xfx.net/stackoverflow/3DEngine/3dengine_sample02.png)

When using the Rubik's cube sample, use the following keys to rotate each face:

* [F] Rotate Front Face
* [B] Rotate Back Face
* [T] Rotate Top Face
* [D] Rotate Bottom Face
* [L] Rotate Left Face
* [R] Rotate Right Face
