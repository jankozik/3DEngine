Public Class Face
    Private mArea As Double
    Private mNormal As New Point3d()
    Private mZ As Double
    Private mCentroid As New Point3d()

    Public Property Vertices As New List(Of Point3d)
    Public Property Color As Color = Color.White
    Public Property Index As Integer
    Public Property DistanceToPoint As Double

    Public Sub New()
    End Sub

    Public Sub New(normal As Point3d)
        mNormal = normal
    End Sub

    Public Sub New(vertices As List(Of Point3d))
        Me.Vertices = vertices
        Update()
    End Sub

    Public Sub New(vertices() As Point3d)
        Me.New(New List(Of Point3d)(vertices))
    End Sub

    Public Sub New(normal As Point3d, vertices As List(Of Point3d))
        Me.New(normal)
        Me.Vertices = vertices
    End Sub

    Public Sub New(face As Face)
        Me.New(face.Vertices)
        Color = face.Color
        Index = face.Index
    End Sub

    Public ReadOnly Property Normal As Point3d
        Get
            Return mNormal
        End Get
    End Property

    Public ReadOnly Property Centroid As Point3d
        Get
            Return mCentroid
        End Get
    End Property

    Public ReadOnly Property Area As Double
        Get
            Return mArea
        End Get
    End Property

    Public ReadOnly Property Z As Double
        Get
            Return mZ
        End Get
    End Property

    Public ReadOnly Property Left As Double
        Get
            Return Vertices.Min(Function(v) v.X)
        End Get
    End Property

    Public ReadOnly Property Top As Double
        Get
            Return Vertices.Min(Function(v) v.Y)
        End Get
    End Property

    Public ReadOnly Property Right As Double
        Get
            Return Vertices.Max(Function(v) v.X)
        End Get
    End Property

    Public ReadOnly Property Bottom As Double
        Get
            Return Vertices.Max(Function(v) v.Y)
        End Get
    End Property

    Public ReadOnly Property Front As Double
        Get
            Return Vertices.Min(Function(v) v.Z)
        End Get
    End Property

    Public ReadOnly Property Back As Double
        Get
            Return Vertices.Max(Function(v) v.Z)
        End Get
    End Property

    Public Sub Update()
        UpdateNormal()
        UpdateZ()
        UpdateArea()
        UpdateCentroid()
    End Sub

    Private Sub UpdateZ()
        mZ = Vertices.Average(Function(v) v.Z)
    End Sub

    Private Sub UpdateCentroid()
        mCentroid.X = Vertices.Average(Function(v) v.X)
        mCentroid.Y = Vertices.Average(Function(v) v.Y)
        mCentroid.Z = Vertices.Average(Function(v) v.Z)

        'Dim cx As Double = 0
        'Dim cy As Double = 0
        'Dim cz As Double = 0

        'Dim j As Integer
        'Dim n = Vertices.Count

        'Dim factor As Double = 0
        'For i As Integer = 0 To n - 1
        '    j = (i + 1) Mod n
        '    factor = (Vertices(i).X * Vertices(j).Y - Vertices(j).X * Vertices(i).Y)
        '    cx += (Vertices(i).X + Vertices(j).X) * factor
        '    cy += (Vertices(i).Y + Vertices(j).Y) * factor
        '    cz += (Vertices(i).Z + Vertices(j).Z) * factor
        'Next

        'Dim a = mArea * 6.0F
        'factor = 1 / a

        'cx *= factor
        'cy *= factor
        'cz *= factor

        'mCentroid = New Point3d(cx, cy, cz)
    End Sub

    Private Sub UpdateArea()
        Dim j As Integer
        Dim n = Vertices.Count

        For i As Integer = 0 To n - 1
            j = (i + 1) Mod n
            mArea += Vertices(i).X * Vertices(j).Y
            mArea -= Vertices(i).Y * Vertices(j).X
        Next
        mArea /= 2.0
    End Sub

    ' Newell's Method
    ' http://www.opengl.org/wiki/Calculating_a_Surface_Normal
    Private Sub UpdateNormal()
        mNormal = New Point3d()

        Dim n As Integer = Vertices.Count
        For i As Integer = 0 To n - 1
            Dim v1 = Vertices(i)
            Dim v2 = Vertices((i + 1) Mod n)

            mNormal += v1.Cross(v2)
            mNormal.Normalize()
        Next
    End Sub

    ' http://stackoverflow.com/questions/21114796/3d-ray-quad-intersection-test-in-java
    Public Function GetPointAtIntersection(ray As Line, Optional epsilon As Double = 0.000001) As Point3d
        Dim dS21 = Vertices(1) - Vertices(0)
        Dim dS31 = Vertices(2) - Vertices(0)
        Dim n = dS21.Cross(dS31) ' mNormal

        Dim dR = ray.Start - ray.End
        Dim nDotdR = n.Dot(dR)
        If Math.Abs(nDotdR) < epsilon Then Return Nothing

        Dim t = -n.Dot(ray.Start - Vertices(0)) / nDotdR
        Dim M = ray.Start + (dR * t)

        Dim dMS1 = M - Vertices(0)
        Dim u = dMS1.Dot(dS21)
        Dim v = dMS1.Dot(dS31)

        If u >= 0.0 AndAlso u <= dS21.Dot(dS21) AndAlso
           v >= 0.0 AndAlso v <= dS31.Dot(dS31) Then
            Return M
        Else
            Return Nothing
        End If
    End Function

    ' http://paulbourke.net/geometry/polygonmesh/
    ' Determining if a point lies on the interior of a polygon
    ' Solution 4 (3D)
    Public Function Contains(p As Point3d, Optional epsilon As Double = 0.0001) As Boolean
        Dim p1 As New Point3d()
        Dim p2 As New Point3d()
        Dim m As Double
        Dim m1 As Double
        Dim m2 As Double
        Dim n As Integer = Vertices.Count
        Dim cosTheta As Double
        Dim angleSum As Double

        Const TAU As Double = 2 * Math.PI

        For v = 0 To n - 1
            p1 = Vertices(v) - p
            p2 = Vertices((v + 1) Mod n) - p

            m1 = p1.Length
            m2 = p2.Length
            m = m1 * m2

            If m <= epsilon Then ' The point is on a vertex
                'mPointCloseness = 1.0
                Return True
            Else
                cosTheta = p1.Dot(p2) / m
            End If

            angleSum += Math.Acos(cosTheta)
        Next

        'mPointCloseness = angleSum / TAU
        Return Math.Abs(TAU - angleSum) <= epsilon
    End Function

    Public Function Contains(x As Double, y As Double) As Boolean
        Dim result As Boolean = False
        Dim n As Integer = Vertices.Count - 1
        Dim j As Integer = n

        For i As Integer = 0 To n
            If (Vertices(i).Y < y AndAlso Vertices(j).Y >= y) OrElse
                (Vertices(j).Y < y AndAlso Vertices(i).Y >= y) Then
                If Vertices(i).X + (y - Vertices(i).Y) / (Vertices(j).Y - Vertices(i).Y) * (Vertices(j).X - Vertices(i).X) < x Then
                    result = Not result
                End If
            End If
            j = i
        Next

        Return result
    End Function

    Public Function Contains(p As PointF) As Boolean
        Return Contains(p.X, p.Y)
    End Function

    Public Function AsInt(Optional padding As Integer = 0) As Face
        Return New Face(Vertices.ConvertAll(Of Point3d)(Function(v) v.AsInt(padding)))
    End Function

    Public Overrides Function ToString() As String
        Return String.Format($"{Vertices.Count} Vertices, Normal: {mNormal}, Z: {mZ:F2}")
    End Function
End Class
