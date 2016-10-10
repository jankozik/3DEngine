Namespace Delaunay2
    Public Class Line
        Implements IEquatable(Of Line)

        Public Start As Point3d
        Public [End] As Point3d

        Public Sub New(start As Point3d, [end] As Point3d)
            Me.Start = start
            Me.End = [end]
        End Sub

        Public Sub Reverse()
            Dim tmp As Point3d = Start
            Start = [End]
            [End] = tmp
        End Sub

        Public Function Vertices() As Point3d()
            Return {Start, [End]}
        End Function

        Public Shared Operator =(l1 As Line, l2 As Line) As Boolean
            Return (l1.Start = l2.Start AndAlso l1.End = l2.End) OrElse
                   (l1.Start = l2.End AndAlso l1.End = l2.Start)
        End Operator

        Public Shared Operator <>(l1 As Line, l2 As Line) As Boolean
            Return Not (l1 = l2)
        End Operator

        Public Shared Operator +(l1 As Line, l2 As Line) As Line
            Return New Line(l1.Start + l2.Start, l1.End + l2.End)
        End Operator

        Public Shared Operator +(l1 As Line, p As Point3d) As Line
            Return New Line(l1.Start + p, l1.End + p)
        End Operator

        Public Shared Operator -(l1 As Line, l2 As Line) As Line
            Return New Line(l1.Start - l2.Start, l1.End - l2.End)
        End Operator

        Public Shared Operator -(l1 As Line, p As Point3d) As Line
            Return New Line(l1.Start - p, l1.End - p)
        End Operator

        Public Shared Operator *(l1 As Line, scalar As Double) As Line
            Return New Line(l1.Start * scalar, l1.End * scalar)
        End Operator

        Public Shared Operator *(scalar As Double, l1 As Line) As Line
            Return l1 * scalar
        End Operator

        Public Shadows Function Equals(other As Line) As Boolean Implements IEquatable(Of Line).Equals
            Return Me = other
        End Function

        Public Overrides Function ToString() As String
            Return $"{Start} -> {[End]}"
        End Function
    End Class
End Namespace
