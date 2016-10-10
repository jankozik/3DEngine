Namespace Delaunay
    Public Class Tetrahedron
        Implements IEquatable(Of Tetrahedron)

        Private mVertices(4 - 1) As Point3d
        Private mCenter As Point3d
        Private mRadius As Double

        Public Sub New(p() As Point3d)
            mVertices = p
            GetCenterCircumcircle()
        End Sub

        Public Sub New(p1 As Point3d, p2 As Point3d, p3 As Point3d, p4 As Point3d)
            mVertices = {p1, p2, p3, p4}
            GetCenterCircumcircle()
        End Sub

        Public ReadOnly Property Vertices As Point3d()
            Get
                Return mVertices
            End Get
        End Property

        Public ReadOnly Property Center As Point3d
            Get
                Return mCenter
            End Get
        End Property

        Public ReadOnly Property Radius As Double
            Get
                Return mRadius
            End Get
        End Property

        Public ReadOnly Property Lines() As Line()
            Get
                Return New Line() {New Line(mVertices(0), mVertices(1)),
                                   New Line(mVertices(0), mVertices(2)),
                                   New Line(mVertices(0), mVertices(3)),
                                   New Line(mVertices(1), mVertices(2)),
                                   New Line(mVertices(1), mVertices(3)),
                                   New Line(mVertices(2), mVertices(3))}
            End Get
        End Property

        Private Sub GetCenterCircumcircle()
            Dim v1 As Point3d = mVertices(0)
            Dim v2 As Point3d = mVertices(1)
            Dim v3 As Point3d = mVertices(2)
            Dim v4 As Point3d = mVertices(3)

            Dim a()() As Double = New Double()() {
                    New Double() {v2.X - v1.X, v2.Y - v1.Y, v2.Z - v1.Z},
                    New Double() {v3.X - v1.X, v3.Y - v1.Y, v3.Z - v1.Z},
                    New Double() {v4.X - v1.X, v4.Y - v1.Y, v4.Z - v1.Z}
                }

            Dim b() As Double = {
                    0.5 * (v2.X * v2.X - v1.X * v1.X + v2.Y * v2.Y - v1.Y * v1.Y + v2.Z * v2.Z - v1.Z * v1.Z),
                    0.5 * (v3.X * v3.X - v1.X * v1.X + v3.Y * v3.Y - v1.Y * v1.Y + v3.Z * v3.Z - v1.Z * v1.Z),
                    0.5 * (v4.X * v4.X - v1.X * v1.X + v4.Y * v4.Y - v1.Y * v1.Y + v4.Z * v4.Z - v1.Z * v1.Z)
                }

            Dim x(3 - 1) As Double
            If Gauss(a, b, x) = 0 Then
                mCenter = Nothing
                mRadius = -1
            Else
                mCenter = New Point3d(x(0), x(1), x(2))
                mRadius = mCenter.Distance(v1)
            End If
        End Sub

        Private Function Gauss(a()() As Double, b() As Double, x() As Double) As Double
            Dim ip(a.Length - 1) As Integer
            Dim det As Double = LU(a, ip)

            If det <> 0.0 Then Solve(a, b, ip, x)
            Return det
        End Function

        ' Solution of equations by LU decomposition
        ' https://en.wikipedia.org/wiki/LU_decomposition
        Private Function LU(a()() As Double, ip() As Integer) As Double
            Dim n As Integer = a.Length - 1
            Dim weight(n) As Double
            Dim u As Double
            Dim t As Double

            For k As Integer = 0 To n
                ip(k) = k
                u = 0.0
                For j As Integer = 0 To n
                    t = Math.Abs(a(k)(j))
                    If t > u Then u = t
                Next
                If u = 0.0 Then Return 0.0
                weight(k) = 1 / u
            Next

            Dim m As Integer
            Dim ii As Integer
            Dim ik As Integer
            Dim det As Double = 1.0
            For k As Integer = 0 To n
                u = -1.0
                m = 0
                For i As Integer = k To n
                    ii = ip(i)
                    t = Math.Abs(a(ii)(k)) * weight(ii)
                    If t > u Then
                        u = t
                        m = i
                    End If
                Next

                ik = ip(m)
                If m <> k Then
                    ip(m) = ip(k)
                    ip(k) = ik
                    det = -det
                End If

                u = a(ik)(k)
                det *= u
                If u = 0.0 Then Return 0.0
                For i As Integer = k + 1 To n
                    ii = ip(i)
                    a(ii)(k) /= u
                    t = a(ii)(k)
                    For j As Integer = k + 1 To n
                        a(ii)(j) -= t * a(ik)(j)
                    Next
                Next
            Next

            Return det
        End Function

        Private Sub Solve(a()() As Double, b() As Double, ip() As Integer, x() As Double)
            Dim n As Integer = a.Length - 1
            Dim ii As Integer
            Dim t As Double

            For i As Integer = 0 To n
                ii = ip(i)
                t = b(ii)
                For j = 0 To i - 1
                    t -= a(ii)(j) * x(j)
                Next
                x(i) = t
            Next

            For i As Integer = n To 0 Step -1
                t = x(i)
                ii = ip(i)
                For j = i + 1 To n
                    t -= a(ii)(j) * x(j)
                Next
                x(i) = t / a(ii)(i)
            Next
        End Sub

        Public Shared Operator =(t1 As Tetrahedron, t2 As Tetrahedron) As Boolean
            Dim counter As Integer = 0
            For Each p1 In t1.Vertices
                For Each p2 In t2.Vertices
                    If p1.X = p2.X AndAlso p1.Y = p2.Y AndAlso p1.Z = p2.Z Then counter += 1
                Next
            Next

            Return counter = 4
        End Operator

        Public Shared Operator <>(t1 As Tetrahedron, t2 As Tetrahedron) As Boolean
            Return Not (t1 = t2)
        End Operator

        Public Shadows Function Equals(other As Tetrahedron) As Boolean Implements IEquatable(Of Tetrahedron).Equals
            Return Me = other
        End Function
    End Class
End Namespace
