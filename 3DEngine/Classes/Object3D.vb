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

    Private mVertices As List(Of Point3d)
    Private mFaces As New List(Of Face)
    Private mIsValid As Boolean
    Private Property mBounds As Bounds3D

    Private tessellator As New Triangualtor()

    Public Sub New(vertices As List(Of Point3d))
        mVertices = vertices
        InitShape()
    End Sub

    Public Sub New(vertices As List(Of Point3d), color As Color)
        Me.New(vertices)
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

    Public Sub TransformMove(dx As Double, dy As Double, dz As Double)
        For Each f In mFaces
            For Each p In f.Vertices
                p.X += dx
                p.Y += dy
                p.Z += dz
            Next
        Next
    End Sub

    Public Sub TransformRotate(ax As Double, ay As Double, az As Double)
        For Each f In mFaces
            For i As Integer = 0 To f.Vertices.Count - 1
                f.Vertices(i) = f.Vertices(i).RotateX(ax).
                                              RotateY(ay).
                                              RotateZ(az)
            Next
        Next
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

    Private Sub InitShape()
        ' Counter-clockwise Ordering
        ' http://stackoverflow.com/questions/8142388/in-what-order-should-i-send-my-vertices-to-opengl-for-culling

        ' http://www.openprocessing.org/sketch/31295
        Debug.WriteLine("Applying Delaunay triangulation")
        tessellator.SetData(mVertices)
        Debug.WriteLine("Simplifying geometry and extracting faces")
        ExtractFaces(tessellator.Triangles, True)

        ' Euler's number and closed surfaces
        ' For closed surfaces V - E + F = 2
        Dim vertices As New List(Of Point3d)
        For Each face In mFaces
            For Each vertex In face.Vertices
                If Not vertices.Contains(vertex) Then vertices.Add(vertex)
            Next
        Next

        Dim edges As New List(Of Vertice)
        For Each face In mFaces
            Dim verticesCount = face.Vertices.Count
            For i As Integer = 0 To verticesCount - 1
                Dim p1 = face.Vertices(i)
                Dim p2 = face.Vertices((i + 1) Mod verticesCount)
                Dim edge = New Vertice(p1, p2)

                If Not edges.Contains(edge) Then edges.Add(edge)
            Next
        Next

        'If vertices.Count - edges.Count + mFaces.Count <> 2 Then Throw New Exception("Shape is not closed")
        mIsValid = (vertices.Count - edges.Count + mFaces.Count = 2)
    End Sub

    Private Sub ExtractFaces(triangles As List(Of Triangle), simplify As Boolean)
        Dim hasCommonVertices As Boolean = False

        mFaces.Clear()

        If simplify Then
            ' http://stackoverflow.com/questions/242404/sort-four-points-in-clockwise-order
            For Each t1 In triangles
                Dim n1 = t1.Normal()

                Dim nf As Face = Nothing
                For Each cf In mFaces
                    If n1.Equals(cf.Normal) Then
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
                Dim nf As New Face(t1.Normal())
                nf.Vertices.AddRange(t1.Vertices)
                mFaces.Add(nf)
            Next
        End If
    End Sub

    Public Function Clone() As Object Implements ICloneable.Clone
        Dim vs As New List(Of Point3d)
        For Each v In mVertices
            vs.Add(New Point3d(v.X, v.Y, v.Z))
        Next
        Return New Object3D(vs, Color)
    End Function
End Class
