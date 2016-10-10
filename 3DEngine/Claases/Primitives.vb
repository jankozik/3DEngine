'       +
'       |
'       Y
'       |
'    C-----B
'    |  |__|_____ X__+
'    | /   |
'    D-----A
'    /
'   Z
'  /
' -

Public Class Primitives
    Public Shared Function Cube(length As Double) As List(Of Point3d)
        '   G-----F
        '  /|    /|
        ' B-----A |
        ' | H---|-E
        ' |/    |/ 
        ' C-----D

        Dim A As New Point3d(length, length, -length)
        Dim B As New Point3d(-length, length, -length)
        Dim C As New Point3d(-length, -length, -length)
        Dim D As New Point3d(length, -length, -length)
        Dim E As New Point3d(length, -length, length)
        Dim F As New Point3d(length, length, length)
        Dim G As New Point3d(-length, length, length)
        Dim H As New Point3d(-length, -length, length)

        Return New List(Of Point3d) From {A, B, C, D, E, F, G, H}
    End Function

    Public Shared Function Cuboid(shortlength As Double, longLength As Double) As List(Of Point3d)
        '   G-----------F
        '  /|          /|
        ' B-----------A |
        ' | H---------|-E
        ' |/          |/ 
        ' C-----------D

        Dim c = Cube(shortlength)
        c(0).X = longLength ' A
        c(3).X = longLength ' D
        c(4).X = longLength ' F
        c(5).X = longLength ' E

        Return c
    End Function

    Public Shared Function Sphere(radius As Double) As List(Of Point3d)
        Dim vertices As New List(Of Point3d)
        Dim arc As New List(Of Point3d)
        Dim angleStep As Double = 45.0 ' Set 90.0 to produce an octahedron

        For ca As Double = 0.0 To 180.0 Step angleStep
            arc.Add(New Point3d(radius * Math.Cos((ca - 180.0) * Point3d.ToRad),
                                   radius * Math.Sin((ca - 180.0) * Point3d.ToRad),
                                   0))
        Next
        vertices.AddRange(arc)

        Dim v As Point3d
        For ax As Double = 0 To 360.0 Step angleStep
            For i As Integer = 0 To arc.Count - 1
                v = arc(i).RotateX(ax)

                Dim isDuplicate As Boolean = False
                For j As Integer = 0 To vertices.Count - 1
                    If vertices(j).IsSimilar(v) Then
                        isDuplicate = True
                        Exit For
                    End If
                Next
                If Not isDuplicate Then vertices.Add(v)
            Next
        Next

        Return vertices
    End Function

    Public Shared Function Tetrahedron(length As Double) As List(Of Point3d)
        '      A
        '     /|\
        '    / | \
        '   /  |  \
        '  /   D   \
        ' B_________C

        Dim A As New Point3d(0, -length, 0)
        Dim B As New Point3d(-length, length, -length)
        Dim C As New Point3d(length, length, -length)
        Dim D As New Point3d(0, length, length)

        Return New List(Of Point3d) From {A, B, C, D}
    End Function

    Public Shared Function SquarePyramid(length As Double) As List(Of Point3d)
        Dim A As New Point3d(-length, -length, length)
        Dim B As New Point3d(-length, length, length)
        Dim C As New Point3d(length, length, length)
        Dim D As New Point3d(length, -length, length)
        Dim E As New Point3d(0, 0, -length)

        Return New List(Of Point3d) From {A, B, C, D, E}
    End Function
End Class
