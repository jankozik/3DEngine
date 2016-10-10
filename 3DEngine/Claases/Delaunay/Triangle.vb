Namespace Delaunay2
    Public Class Triangle
        Implements IEquatable(Of Triangle)

        Private v1 As Point3d
        Private v2 As Point3d
        Private v3 As Point3d

        Public Sub New(p1 As Point3d, p2 As Point3d, p3 As Point3d)
            Me.v1 = p1
            Me.v2 = p2
            Me.v3 = p3
        End Sub

        Public Function GetNormal() As Point3d
            Dim edge1 As Point3d = v2 - v1
            Dim edge2 As Point3d = v3 - v1

            Return edge1.Cross(edge2).Normalized()
        End Function

        Public Sub TurnBack()
            Dim tmp As Point3d = New Point3d(v3)
            v3 = New Point3d(v1)
            v1 = tmp
        End Sub

        Public Function Vertices() As Point3d()
            Return New Point3d() {v1, v2, v3}
        End Function

        Public Function GetLines() As Line()
            Return {New Line(v1, v2),
                    New Line(v2, v3),
                    New Line(v3, v1)}
        End Function

        Public Sub SetVertices(p1 As Point3d, p2 As Point3d, p3 As Point3d)
            Me.v1 = p1
            Me.v2 = p2
            Me.v3 = p3
        End Sub

        Public Shared Operator =(t1 As Triangle, t2 As Triangle) As Boolean
            Dim lines1() As Line = t1.GetLines()
            Dim lines2() As Line = t2.GetLines()

            Dim counter As Integer = 0
            For i As Integer = 0 To lines1.Length - 1
                For j As Integer = 0 To lines2.Length - 1
                    If lines1(i) = lines2(j) Then counter += 1
                Next
            Next

            Return counter = 3
        End Operator

        Public Shared Operator <>(t1 As Triangle, t2 As Triangle) As Boolean
            Return Not (t1 = t2)
        End Operator

        Public Shadows Function Equals(other As Triangle) As Boolean Implements IEquatable(Of Triangle).Equals
            Return Me = other
        End Function
    End Class
End Namespace
