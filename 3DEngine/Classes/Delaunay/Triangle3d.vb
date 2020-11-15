Namespace Delaunay
    Public Class Triangle3d
        Implements IEquatable(Of Triangle3d)

        Private p1 As Point3d
        Private p2 As Point3d
        Private p3 As Point3d

        Public Sub New(p1 As Point3d, p2 As Point3d, p3 As Point3d)
            Me.p1 = p1
            Me.p2 = p2
            Me.p3 = p3
        End Sub

        Public ReadOnly Property Normal As Point3d
            Get
                Dim edge1 As Point3d = p2 - p1
                Dim edge2 As Point3d = p3 - p1

                Return edge1.Cross(edge2).Normalized()
            End Get
        End Property

        Public Sub Flip()
            Dim tmp As Point3d = New Point3d(p3)
            p3 = New Point3d(p1)
            p1 = tmp
        End Sub

        Public ReadOnly Property Vertices As Point3d()
            Get
                Return New Point3d() {p1, p2, p3}
            End Get
        End Property

        Public ReadOnly Property Lines As Line3d()
            Get
                Return {New Line3d(p1, p2),
                        New Line3d(p2, p3),
                        New Line3d(p3, p1)}
            End Get
        End Property

        Public Shared Operator =(t1 As Triangle3d, t2 As Triangle3d) As Boolean
            Dim lines1() As Line3d = t1.Lines
            Dim lines2() As Line3d = t2.Lines

            Dim counter As Integer = 0
            For Each l1 As Line3d In lines1
                For Each l2 As Line3d In lines2
                    If l1 = l2 Then counter += 1
                Next
            Next

            Return counter = 3
        End Operator

        Public Shared Operator <>(t1 As Triangle3d, t2 As Triangle3d) As Boolean
            Return Not (t1 = t2)
        End Operator

        Public Shadows Function Equals(other As Triangle3d) As Boolean Implements IEquatable(Of Triangle3d).Equals
            Return Me = other
        End Function
    End Class
End Namespace
