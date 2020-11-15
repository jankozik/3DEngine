Namespace Delaunay
    Public Class Line3d
        Implements IEquatable(Of Line3d)

        Public Property Start As Point3d
        Public Property [End] As Point3d

        Public Sub New(start As Point3d, [end] As Point3d)
            Me.Start = start
            Me.End = [end]
        End Sub

        Public Sub Reverse()
            Dim tmp As Point3d = Start
            Start = [End]
            [End] = tmp
        End Sub

        Public ReadOnly Property Vertices As Point3d()
            Get
                Return {Start, [End]}
            End Get
        End Property

        Public Shared Operator =(l1 As Line3d, l2 As Line3d) As Boolean
            Return (l1.Start = l2.Start AndAlso l1.End = l2.End) OrElse
                   (l1.Start = l2.End AndAlso l1.End = l2.Start)
        End Operator

        Public Shared Operator <>(l1 As Line3d, l2 As Line3d) As Boolean
            Return Not (l1 = l2)
        End Operator

        Public Shared Operator +(l1 As Line3d, l2 As Line3d) As Line3d
            Return New Line3d(l1.Start + l2.Start, l1.End + l2.End)
        End Operator

        Public Shared Operator +(l1 As Line3d, p As Point3d) As Line3d
            Return New Line3d(l1.Start + p, l1.End + p)
        End Operator

        Public Shared Operator -(l1 As Line3d, l2 As Line3d) As Line3d
            Return New Line3d(l1.Start - l2.Start, l1.End - l2.End)
        End Operator

        Public Shared Operator -(l1 As Line3d, p As Point3d) As Line3d
            Return New Line3d(l1.Start - p, l1.End - p)
        End Operator

        Public Shared Operator *(l1 As Line3d, scalar As Double) As Line3d
            Return New Line3d(l1.Start * scalar, l1.End * scalar)
        End Operator

        Public Shared Operator *(scalar As Double, l1 As Line3d) As Line3d
            Return l1 * scalar
        End Operator

        Public Shadows Function Equals(other As Line3d) As Boolean Implements IEquatable(Of Line3d).Equals
            Return Me = other
        End Function

        Public Overrides Function ToString() As String
            Return $"{Start} -> {[End]}"
        End Function
    End Class
End Namespace
