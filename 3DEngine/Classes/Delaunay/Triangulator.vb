' xFX JumpStart
' Xavier Flix (http://whenimbored.xfx.net)
' 2013 - 2017
'
'================================================================
'
' This project Is based on the following work: 
'
'================================================================
'
' Simulation of a Wireframe Cube using GDI+
' Developed by leonelmachava <leonelmachava@gmail.com>
' http://codentronix.com
'
' Copyright (c) 2011 Leonel Machava
' 
' Permission Is hereby granted, free of charge, to any person obtaining a copy of this 
' software And associated documentation files (the "Software"), to deal in the Software 
' without restriction, including without limitation the rights to use, copy, modify, 
' merge, publish, distribute, sublicense, And/Or sell copies of the Software, And to 
' permit persons to whom the Software Is furnished to do so, subject to the following 
' conditions:
' 
' The above copyright notice And this permission notice shall be included in all copies 
' Or substantial portions of the Software.
'
' THE SOFTWARE Is PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS Or IMPLIED, 
' INCLUDING BUT Not LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
' PURPOSE And NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS Or COPYRIGHT HOLDERS BE LIABLE 
' FOR ANY CLAIM, DAMAGES Or OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT Or 
' OTHERWISE, ARISING FROM, OUT OF Or IN CONNECTION WITH THE SOFTWARE Or THE USE Or OTHER 
' DEALINGS IN THE SOFTWARE.

' http://paulbourke.net/geometry/polygonmesh/

Namespace Delaunay
    Public Class Triangualtor
        Public Const Epsilon As Double = 0.01

        Private mTetras As New List(Of Tetrahedron)
        Private mEdges As New List(Of Line)
        Private mSurfaceEdges As New List(Of Line)
        Private mTriangles As New List(Of Triangle)

        Public Sub New()
        End Sub

        Public ReadOnly Property Triangles As List(Of Triangle)
            Get
                Return mTriangles
            End Get
        End Property

        Public ReadOnly Property Edges As List(Of Line)
            Get
                Return mEdges
            End Get
        End Property

        Public ReadOnly Property SurfaceEdges As List(Of Line)
            Get
                Return mSurfaceEdges
            End Get
        End Property

        Public ReadOnly Property Tetrahedrons As List(Of Tetrahedron)
            Get
                Return mTetras
            End Get
        End Property

        Public Sub Triangulate(points As List(Of Point3d))
            mTetras.Clear()

            ' Obtain a tetrahedron that includes the point group
            ' Obtain a sphere including point clouds
            Dim vMax As New Point3d(-999, -999, -999)
            Dim vMin As New Point3d(999, 999, 999)
            For Each v In points
                If vMax.X < v.X Then vMax.X = v.X
                If vMax.Y < v.Y Then vMax.Y = v.Y
                If vMax.Z < v.Z Then vMax.Z = v.Z
                If vMin.X > v.X Then vMin.X = v.X
                If vMin.Y > v.Y Then vMin.Y = v.Y
                If vMin.Z > v.Z Then vMin.Z = v.Z
            Next

            Dim center As Point3d = 0.5 * (vMax - vMin)  ' Full external sphere center coordinates
            Dim r As Double = -1.0                       ' Radius
            points.ForEach(Sub(p) If r < center.Distance(p) Then r = center.Distance(p))
            r += 0.1                                     ' A little extra

            ' Obtain a tetrahedron circumscribing the sphere
            Dim v1 As New Point3d(center.X, center.Y + 3.0 * r, center.Z)
            Dim v2 As New Point3d(center.X - 2.0 * Math.Sqrt(2) * r, center.Y - r, center.Z)
            Dim v3 As New Point3d(center.X + Math.Sqrt(2) * r, center.Y - r, center.Z + Math.Sqrt(6) * r)
            Dim v4 As New Point3d(center.X + Math.Sqrt(2) * r, center.Y - r, center.Z - Math.Sqrt(6) * r)
            Dim outerTetra() As Point3d = {v1, v2, v3, v4}
            mTetras.Add(New Tetrahedron(outerTetra))

            Console.WriteLine("Analyzing points and generating tetrahedrons...")
            For Each p As Point3d In points
                ' Temporary lists for dynamically changing the geometry
                Dim tmpTetrasList As New List(Of Tetrahedron)
                Dim newTetrasList As New List(Of Tetrahedron)

                For Each t As Tetrahedron In mTetras.AsParallel()
                    If t.IsValid AndAlso t.Radius > p.Distance(t.Center) Then tmpTetrasList.Add(t)
                Next

                For Each t As Tetrahedron In tmpTetrasList
                    mTetras.Remove(t)

                    v1 = t.Vertices(0)
                    v2 = t.Vertices(1)
                    v3 = t.Vertices(2)
                    v4 = t.Vertices(3)
                    newTetrasList.Add(New Tetrahedron(v1, v2, v3, p))
                    newTetrasList.Add(New Tetrahedron(v1, v2, v4, p))
                    newTetrasList.Add(New Tetrahedron(v1, v3, v4, p))
                    newTetrasList.Add(New Tetrahedron(v2, v3, v4, p))
                Next

                Dim isRedundantTetra(newTetrasList.Count - 1) As Boolean
                Parallel.For(0, newTetrasList.Count - 1, Sub(i As Integer)
                                                             Parallel.For(i + 1, newTetrasList.Count, Sub(j As Integer)
                                                                                                          If newTetrasList(i) = newTetrasList(j) Then
                                                                                                              isRedundantTetra(i) = True
                                                                                                              isRedundantTetra(j) = True
                                                                                                          End If
                                                                                                      End Sub)
                                                         End Sub)
                'For i As Integer = 0 To newTetrasList.Count - 2
                '    'If isRedundantTetra(i) Then Continue For
                '    For j As Integer = i + 1 To newTetrasList.Count - 1
                '        If newTetrasList(i) = newTetrasList(j) Then
                '            isRedundantTetra(i) = True
                '            isRedundantTetra(j) = True
                '        End If
                '    Next
                'Next
                For i As Integer = 0 To newTetrasList.Count - 1
                    If Not isRedundantTetra(i) Then mTetras.Add(newTetrasList(i))
                Next

                Console.WriteLine($"    {points.IndexOf(p)} / {points.Count} ({points.IndexOf(p) / points.Count * 100:F2}) --> {mTetras.Count}")
            Next

            Console.WriteLine("Removing outer tetrahedrons...")
            Dim isOuter As Boolean
            Dim k As Integer = 0
            Do
                isOuter = False
                For Each p1 As Point3d In mTetras(k).Vertices
                    For Each p2 As Point3d In outerTetra
                        If p1 = p2 Then
                            isOuter = True
                            mTetras.RemoveAt(k)
                            Exit For
                        End If
                    Next
                    If isOuter Then Exit For
                Next
                If Not isOuter Then k += 1
            Loop While k < mTetras.Count

            Console.WriteLine("Adding edges...")
            mEdges.Clear()
            For Each t As Tetrahedron In mTetras
                For Each l1 As Line In t.Lines
                    If Not mEdges.Contains(l1) Then mEdges.Add(l1)
                Next
            Next

            Console.WriteLine("Obtaining faces...")
            Dim triList As New List(Of Triangle)
            For Each t As Tetrahedron In mTetras
                v1 = t.Vertices(0)
                v2 = t.Vertices(1)
                v3 = t.Vertices(2)
                v4 = t.Vertices(3)

                Dim tri1 As New Triangle(v1, v2, v3)
                Dim tri2 As New Triangle(v1, v3, v4)
                Dim tri3 As New Triangle(v1, v4, v2)
                Dim tri4 As New Triangle(v4, v3, v2)

                Dim n As Point3d
                ' Decide direction of the surface
                n = tri1.Normal
                If n.Dot(v1) > n.Dot(v4) Then tri1.Flip()

                n = tri2.Normal
                If n.Dot(v1) > n.Dot(v2) Then tri2.Flip()

                n = tri3.Normal
                If n.Dot(v1) > n.Dot(v3) Then tri3.Flip()

                n = tri4.Normal
                If n.Dot(v4) > n.Dot(v1) Then tri4.Flip()

                triList.Add(tri1)
                triList.Add(tri2)
                triList.Add(tri3)
                triList.Add(tri4)
            Next

            Console.WriteLine("Obtaining triangles...")
            mTriangles.Clear()
            Dim isSameTriangle(triList.Count - 1) As Boolean
            Parallel.For(0, triList.Count - 1, Sub(i As Integer)
                                                   Parallel.For(i + 1, triList.Count, Sub(j As Integer)
                                                                                          If triList(i) = triList(j) Then
                                                                                              isSameTriangle(i) = True
                                                                                              isSameTriangle(j) = True
                                                                                          End If
                                                                                      End Sub)
                                               End Sub)
            'For i As Integer = 0 To triList.Count - 2
            '    'If isSameTriangle(i) Then Continue For
            '    For j As Integer = i + 1 To triList.Count - 1
            '        If triList(i) = triList(j) Then
            '            isSameTriangle(i) = True
            '            isSameTriangle(j) = True
            '        End If
            '    Next
            'Next
            For i As Integer = 0 To isSameTriangle.Count - 1
                If Not isSameTriangle(i) Then mTriangles.Add(triList(i))
            Next

            Console.WriteLine("Obtaining surface edges...")
            mSurfaceEdges.Clear()
            Dim surfaceEdgeList As New List(Of Line)
            For Each t As Triangle In mTriangles
                surfaceEdgeList.AddRange(t.Lines)
            Next

            Dim isRedundantEdge(surfaceEdgeList.Count - 1) As Boolean
            For i As Integer = 0 To surfaceEdgeList.Count - 2
                For j As Integer = i + 1 To surfaceEdgeList.Count - 1
                    If surfaceEdgeList(i) = surfaceEdgeList(j) Then
                        isRedundantEdge(i) = True
                        isRedundantEdge(j) = True
                    End If
                Next
            Next
            For i As Integer = 0 To isRedundantEdge.Count - 1
                If Not isRedundantEdge(i) Then mSurfaceEdges.Add(surfaceEdgeList(i))
            Next
        End Sub
    End Class
End Namespace
