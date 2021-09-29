Public Class Object3D
    Implements ICloneable
    Public Structure Bounds3D
        Public ReadOnly Property Left As Double
        Public ReadOnly Property Right As Double
        Public ReadOnly Property Top As Double
        Public ReadOnly Property Bottom As Double
        Public ReadOnly Property Back As Double
        Public ReadOnly Property Front As Double

        Public Sub New(left As Double, top As Double, right As Double, bottom As Double, back As Double, front As Double)
            Me.Left = left
            Me.Top = top
            Me.Right = right
            Me.Bottom = bottom
            Me.Back = back
            Me.Front = front
        End Sub
    End Structure

    Public Enum TriangulationModes
        Delaunay
        QuickHull
    End Enum

    Private mVertices As New List(Of Point3d)
    Private mEdges As New List(Of Line3d)
    Private mFaces As New List(Of Face)
    Private mIsValid As Boolean
    Private mBounds As Bounds3D

    Private tessellator As New Triangualtor()
    Private mIsSolid As Boolean

    Public Sub New(vertices As List(Of Point3d), Optional triangulate As Boolean = True, Optional triangulationMode As TriangulationModes = TriangulationModes.QuickHull)
        mIsSolid = triangulate
        'mVertices = vertices

        Select Case triangulationMode
            Case TriangulationModes.Delaunay
                InitShapeDelaunay(vertices, triangulate)
            Case TriangulationModes.QuickHull
                InitShapeQuickHull(vertices, triangulate)
        End Select
    End Sub

    Public Sub New(vertices As List(Of Point3d), color As Color, Optional triangulate As Boolean = True, Optional triangulationMode As TriangulationModes = TriangulationModes.QuickHull)
        Me.New(vertices, triangulate, triangulationMode)
        Me.Color = color
    End Sub

    Public Sub New(fileName As String, length As Double, color As Color, Optional triangulate As Boolean = True, Optional triangulationMode As TriangulationModes = TriangulationModes.QuickHull)
        Dim pts As New List(Of Point3d)

        For Each line As String In IO.File.ReadAllLines(fileName)
            Dim ps() As String = line.Split(" ")
            pts.Add(New Point3d(Double.Parse(ps(0)) * length, Double.Parse(ps(1)) * length, Double.Parse(ps(2)) * length))
        Next

        mIsSolid = triangulate
        Select Case triangulationMode
            Case TriangulationModes.Delaunay
                InitShapeDelaunay(pts, triangulate)
            Case TriangulationModes.QuickHull
                InitShapeQuickHull(pts, triangulate)
        End Select
        Me.Color = color
    End Sub

    Public Property Color As Color
        Get
            Return mFaces(0).Color
        End Get
        Set(value As Color)
            mFaces.ForEach(Sub(f) f.Color = value)
        End Set
    End Property

    Public ReadOnly Property IsSolid As Boolean
        Get
            Return mIsSolid
        End Get
    End Property

    Public ReadOnly Property Bounds As Bounds3D
        Get
            Dim l As Double = Double.MaxValue
            Dim t As Double = Double.MaxValue
            Dim r As Double = Double.MinValue
            Dim b As Double = Double.MinValue
            Dim zf As Double = Double.MinValue ' zfar = back
            Dim zn As Double = Double.MaxValue ' znear = front
            mFaces.ForEach(Sub(f)
                               For Each v In f.Vertices
                                   l = Math.Min(l, v.X)
                                   t = Math.Min(t, v.Y)
                                   r = Math.Max(r, v.X)
                                   b = Math.Max(b, v.Y)
                                   zf = Math.Max(zf, v.Z)
                                   zn = Math.Min(zn, v.Z)
                               Next
                           End Sub)
            Return New Bounds3D(l, t, r, b, zf, zn)
        End Get
    End Property

    Public ReadOnly Property Faces As List(Of Face)
        Get
            Return mFaces
        End Get
    End Property

    Public ReadOnly Property Vertices As List(Of Point3d)
        Get
            Return mVertices
        End Get
    End Property

    Public ReadOnly Property Edges As List(Of Line3d)
        Get
            Return mEdges
        End Get
    End Property

    Public Sub TransformMove(dx As Double, dy As Double, dz As Double)
        For Each f In mFaces
            For Each v In f.Vertices
                v.X += dx
                v.Y += dy
                v.Z += dz
            Next
        Next

        UpdateObject()
    End Sub

    Public Sub TransformRotate(ax As Double, ay As Double, az As Double)
        For Each f In mFaces
            For i As Integer = 0 To f.Vertices.Count - 1
                f.Vertices(i) = f.Vertices(i).RotateX(ax).
                                              RotateY(ay).
                                              RotateZ(az)
            Next
        Next

        UpdateObject()
    End Sub

    Public ReadOnly Property Z As Double
        Get
            Return mFaces.Average(Function(f) f.Z)
        End Get
    End Property

    Public ReadOnly Property IsValid As Boolean
        Get
            Return mIsValid
        End Get
    End Property

    Private Sub UpdateObject()
        mVertices.Clear()
        mFaces.ForEach(Sub(f)
                           For Each v In f.Vertices
                               If Not mVertices.Contains(v) Then mVertices.Add(v)
                           Next
                           'mVertices.AddRange(f.Vertices)
                       End Sub)

        mEdges.Clear()
        Dim verticesCount As Integer
        For Each face In mFaces
            verticesCount = face.Vertices.Count
            For i As Integer = 0 To verticesCount - 1
                Dim p1 As Point3d = face.Vertices(i)
                Dim p2 As Point3d = face.Vertices((i + 1) Mod verticesCount)
                Dim edge As New Line3d(p1, p2)
                If Not mEdges.Contains(edge) Then mEdges.Add(edge)
            Next
        Next
    End Sub

    Private Sub InitShapeDelaunay(verts As List(Of Point3d), Optional triangulate As Boolean = True, Optional simplify As Boolean = True)
        ' Counter-clockwise Ordering
        ' http://stackoverflow.com/questions/8142388/in-what-order-should-i-send-my-vertices-to-opengl-for-culling
        'simplify = False
        ' http://www.openprocessing.org/sketch/31295
        If triangulate Then
            'verts = OrderPoints(verts)
            Console.WriteLine("Applying Delaunay triangulation")
            tessellator.Triangulate(verts)
            Console.WriteLine($"{If(simplify, "Simplifying geometry and e", "E")}xtracting faces")
            ExtractFaces(tessellator.Triangles, simplify)
        Else
            mFaces.Add(New Face(verts))
        End If
        UpdateObject()
        Console.WriteLine($"{mVertices.Count:N0} vertices, {mEdges.Count:N0} edges and {mFaces.Count:N0} faces")

        ' Euler's number and closed surfaces
        ' For closed surfaces V - E + F = 2
        mIsValid = (verts.Count - mEdges.Count + mFaces.Count = 2)
        'mIsValid = True
    End Sub

    Private Sub InitShapeQuickHull(verts As List(Of Point3d), Optional triangulate As Boolean = True, Optional simplify As Boolean = True)
        Dim p3d(verts.Count - 1) As QuickHull3D.Point3d
        For i As Integer = 0 To verts.Count - 1
            p3d(i) = New QuickHull3D.Point3d(verts(i).X, verts(i).Y, verts(i).Z)
        Next

        Dim hull As New QuickHull3D.Hull(p3d)
        If triangulate Then hull.Triangulate()
        Dim vs = hull.GetVertices()
        Dim fs = hull.GetFaces()

        Dim ts As New List(Of Triangle3d)
        For Each f In fs
            Dim p1 = vs(f(0))
            Dim p2 = vs(f(1))
            Dim p3 = vs(f(2))
            ts.Add(New Triangle3d(
                            New Point3d(p1.x, p1.y, p1.z),
                            New Point3d(p2.x, p2.y, p2.z),
                            New Point3d(p3.x, p3.y, p3.z)
                       )
                   )
        Next
        ExtractFaces(ts, simplify)

        UpdateObject()
        Console.WriteLine($"{mVertices.Count:N0} vertices, {mEdges.Count:N0} edges and {mFaces.Count:N0} faces")

        ' Euler's number and closed surfaces
        ' For closed surfaces V - E + F = 2
        mIsValid = (verts.Count - mEdges.Count + mFaces.Count = 2)
        'mIsValid = True
    End Sub

    Private Sub ExtractFaces(triangles As List(Of Triangle3d), simplify As Boolean)
        Dim hasCommonVertices As Boolean = False

        mFaces.Clear()

        If simplify Then
            ' http://stackoverflow.com/questions/242404/sort-four-points-in-clockwise-order
            For Each t1 In triangles
                Dim n1 As Point3d = t1.Normal

                Dim nf As Face = Nothing
                For Each cf In mFaces
                    If n1 = cf.Normal Then
                        hasCommonVertices = False
                        For Each v In t1.Vertices
                            If cf.Vertices.Contains(v) Then
                                hasCommonVertices = True
                                Exit For
                            End If
                        Next
                        If hasCommonVertices Then
                            nf = cf
                            Exit For
                        End If
                    End If
                Next

                If nf Is Nothing Then
                    nf = New Face(n1)
                    nf.Vertices.AddRange(t1.Vertices)
                    mFaces.Add(nf)
                Else
                    For Each v In t1.Vertices
                        If Not nf.Vertices.Contains(v) Then nf.Vertices.Add(v)
                    Next

                    Dim vertCount1 As Integer = nf.Vertices.Count
                    Dim vertCount2 As Integer = vertCount1 - 1

                    Dim indices(vertCount2) As Integer
                    For i As Integer = 0 To vertCount2
                        indices(i) = i
                    Next

                    Dim minPerimeter As Double = Double.MaxValue
                    Dim bestPermutation As Integer() = Nothing
                    For Each p In indices.Permutate4()
                        Dim perimeter As Double = 0

                        For i As Integer = 0 To vertCount2
                            Dim p1 = nf.Vertices(p(i))
                            Dim p2 = nf.Vertices(p((i + 1) Mod vertCount1))

                            perimeter += p1.Distance(p2)
                            If perimeter > minPerimeter Then Exit For
                        Next

                        If perimeter < minPerimeter Then
                            minPerimeter = perimeter
                            bestPermutation = p
                        End If
                    Next

                    Dim vs As New List(Of Point3d)
                    For v As Integer = 0 To vertCount2
                        vs.Add(nf.Vertices(bestPermutation(v)))
                    Next
                    nf.Vertices = vs
                End If
            Next
        Else
            For Each t1 In triangles
                Dim nf As New Face(t1.Normal)
                nf.Vertices.AddRange(t1.Vertices)
                If Not mFaces.Contains(nf) Then mFaces.Add(nf)
            Next
        End If
    End Sub

    Private Function OrderPoints(points As List(Of Point3d), Optional order As Integer = 16) As List(Of Point3d)
        Dim ordered As New List(Of Point3d)
        Dim pZOrder As New Dictionary(Of Point3d, Double)

        Dim precision As Integer = 4
        Dim multiplier As Integer = 10 ^ precision
        Dim floor As Double = points.Min(Function(p) Math.Min(Math.Min(p.X, p.Y), p.Z))
        floor = If(floor < 0, -floor, 0)
        Dim fp3d As New Point3d(floor, floor, floor)

        For Each p In points
            Dim po As Point3d = ((p + fp3d) * multiplier).AsInt()
            Dim flat() As UInteger = {CUInt(po.X), CUInt(po.Y), CUInt(po.Z)}
            Dim h() As UInteger = HilbertCurveTransform.HilbertIndexTransposed(flat, order)
            pZOrder.Add(p, (New Point3d(h(0), h(1), h(2))).Length)
        Next

        For Each pzo In pZOrder.OrderBy(Function(pz) pz.Value)
            ordered.Add(pzo.Key)
        Next

        Return ordered
    End Function

    Public Function Clone() As Object Implements ICloneable.Clone
        Dim vs As New List(Of Point3d)
        mVertices.ForEach(Sub(v) vs.Add(New Point3d(v.X, v.Y, v.Z)))
        Return New Object3D(vs, Color)
    End Function
End Class
