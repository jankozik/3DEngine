Public Class Face
    Implements IEquatable(Of Face)

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

    ' Newell's Method: http://www.opengl.org/wiki/Calculating_a_Surface_Normal
    Private Sub UpdateNormal()
        Dim v1 As Point3d
        Dim v2 As Point3d
        Dim n As Integer = Vertices.Count

        mNormal = New Point3d()

        For i As Integer = 0 To n - 1
            v1 = Vertices(i)
            v2 = Vertices((i + 1) Mod n)

            mNormal += v1.Cross(v2)
            mNormal.Normalize()
        Next
    End Sub

    ' http://stackoverflow.com/questions/21114796/3d-ray-quad-intersection-test-in-java
    Public Function GetPointAtIntersection(ray As Line) As Point3d
        Dim dS21 As Point3d = Vertices(1) - Vertices(0)
        Dim dS31 As Point3d = Vertices(2) - Vertices(0)
        Dim n As Point3d = dS21.Cross(dS31) ' mNormal

        Dim dR As Point3d = ray.Start - ray.End
        Dim nDotdR As Double = n.Dot(dR)
        If Math.Abs(nDotdR) < Triangualtor.Epsilon Then Return Nothing

        Dim t As Double = -n.Dot(ray.Start - Vertices(0)) / nDotdR
        Dim M As Point3d = ray.Start + (dR * t)

        Dim dMS1 As Point3d = M - Vertices(0)
        Dim u As Double = dMS1.Dot(dS21)
        Dim v As Double = dMS1.Dot(dS31)

        If u >= 0.0 AndAlso u <= dS21.Dot(dS21) AndAlso
           v >= 0.0 AndAlso v <= dS31.Dot(dS31) Then
            Return M
        Else
            Return Nothing
        End If
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

    Public Shared Operator =(f1 As Face, f2 As Face) As Boolean
        Dim counter As Integer = 0

        For Each v1 As Point3d In f1.Vertices
            For Each v2 As Point3d In f2.Vertices
                If v1 = v2 Then counter += 1
            Next
        Next

        Return counter = 4
    End Operator

    Public Shared Operator <>(f1 As Face, f2 As Face) As Boolean
        Return Not (f1 = f2)
    End Operator

    Public Shadows Function Equals(other As Face) As Boolean Implements IEquatable(Of Face).Equals
        Return Me = other
    End Function
End Class
