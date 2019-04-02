Imports System.Runtime.InteropServices

Namespace Delaunay
    Public Class Point3d
        Implements IEquatable(Of Point3d)

        Public Const ToRad As Double = Math.PI / 180.0
        Public Const ToDeg As Double = 180.0 / Math.PI

        Public Property X As Double
        Public Property Y As Double
        Public Property Z As Double

        Public Sub New()
        End Sub

        Public Sub New(p As Point3d)
            X = p.X
            Y = p.Y
            Z = p.Z
        End Sub

        Public Sub New(x As Double, y As Double, z As Double)
            Me.X = x
            Me.Y = y
            Me.Z = z
        End Sub

        Public Function Distance(p As Point3d) As Double
            Dim dx As Double = X - p.X
            Dim dy As Double = Y - p.Y
            Dim dz As Double = Z - p.Z
            Return Math.Sqrt(dx * dx + dy * dy + dz * dz)
        End Function

        Public Function Length() As Double
            Return Math.Sqrt(X * X + Y * Y + Z * Z)
        End Function

        Public Function Dot(p As Point3d) As Double
            Return X * p.X + Y * p.Y + Z * p.Z
        End Function

        Public Function Cross(p As Point3d) As Point3d
            Return New Point3d((Y * p.Z) - (p.Y * Z),
                               (Z * p.X) - (p.Z * X),
                               (X * p.Y) - (p.X * Y))
        End Function

        Public Sub Normalize()
            Dim len As Double = Me.Length()
            If len > 0 Then
                X /= len
                Y /= len
                Z /= len
            Else
                X = 1.0
                Y = 0.0
                Z = 0.0
            End If
        End Sub

        Public Function Normalized() As Point3d
            Dim p As New Point3d(Me)
            p.Normalize()
            Return p
        End Function

        Public Function RotateX(a As Double) As Point3d
            Dim arad As Double = a * ToRad
            Dim cosa As Double = Math.Cos(arad)
            Dim sina As Double = Math.Sin(arad)
            Return New Point3d(X, Y * cosa - Z * sina, Y * sina + Z * cosa)
        End Function

        Public Function RotateY(a As Double) As Point3d
            Dim arad As Double = a * ToRad
            Dim cosa As Double = Math.Cos(arad)
            Dim sina As Double = Math.Sin(arad)
            Return New Point3d(Z * sina + X * cosa, Y, Z * cosa - X * sina)
        End Function

        Public Function RotateZ(a As Double) As Point3d
            Dim arad As Double = a * ToRad
            Dim cosa As Double = Math.Cos(arad)
            Dim sina As Double = Math.Sin(arad)
            Return New Point3d(X * cosa - Y * sina, X * sina + Y * cosa, Z)
        End Function

        Public Function Project(viewWidth As Integer, viewheight As Integer, fov As Double, viewDistance As Double) As Point3d
            Dim factor As Double = If(viewDistance = -Z, 999, fov / (viewDistance + Z))
            Return New Point3d(X * factor + viewWidth / 2, Y * factor + viewheight / 2, Z)
        End Function

        Public Function UnProject(viewWidth As Integer, viewheight As Integer, fov As Double, viewDistance As Double) As Point3d
            Dim factor As Double = If(viewDistance = -Z, 999, fov / (viewDistance + Z))
            Return New Point3d((X - viewWidth / 2) / factor, (Y - viewheight / 2) / factor, Z)
        End Function

        Public Function ToPointF() As PointF
            Return New PointF(X, Y)
        End Function

        Public Shared Operator =(p1 As Point3d, p2 As Point3d) As Boolean
            Return p1.IsSimilar(p2)
            'Return p1.X = p2.X AndAlso p1.Y = p2.Y AndAlso p1.Z = p2.Z
        End Operator

        Public Shared Operator <>(p1 As Point3d, p2 As Point3d) As Boolean
            Return Not (p1 = p2)
        End Operator

        Public Shared Operator +(p1 As Point3d, p2 As Point3d) As Point3d
            Return New Point3d(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z)
        End Operator

        Public Shared Operator -(p1 As Point3d, p2 As Point3d) As Point3d
            Return New Point3d(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z)
        End Operator

        Public Shared Operator -(p1 As Point3d) As Point3d
            Return New Point3d(p1.X, p1.Y, -p1.Z)
        End Operator

        Public Shared Operator *(p1 As Point3d, scalar As Double) As Point3d
            Return New Point3d(p1.X * scalar, p1.Y * scalar, p1.Z * scalar)
        End Operator

        Public Shared Operator *(scalar As Double, p1 As Point3d) As Point3d
            Return p1 * scalar
        End Operator

        Public Shared Operator *(p1 As Point3d, p2 As Point3d) As Double
            Return p1.Dot(p2)
        End Operator

        Public Shared Operator /(p1 As Point3d, scalar As Double) As Point3d
            Return p1 * (1 / scalar)
        End Operator

        Public Function ToArray() As Double()
            Return {X, Y, Z}
        End Function

        Public Overrides Function ToString() As String
            Return $"({X:F2}, {Y:F2}, {Z:F2})"
        End Function

        Public Function AsInt(Optional padding As Integer = 0) As Point3d
            Dim X1 As Integer = X
            Dim Y1 As Integer = Y
            Dim Z1 As Integer = Z

            If padding > 1 Then
                X1 -= X1 Mod padding
                Y1 -= Y1 Mod padding
                Z1 -= Z1 Mod padding
            End If

            Return New Point3d(X1, Y1, Z1)
        End Function

        Public Function Compare(p As Point3d) As Integer
            Return Length.CompareTo(p.Length)
        End Function

        Public Shadows Function Equals(other As Point3d) As Boolean Implements IEquatable(Of Point3d).Equals
            Return Me = other
        End Function

        Public Function IsSimilar(p As Point3d) As Boolean
            Return Math.Abs(X - p.X) <= Triangualtor.Epsilon AndAlso
                   Math.Abs(Y - p.Y) <= Triangualtor.Epsilon AndAlso
                   Math.Abs(Z - p.Z) <= Triangualtor.Epsilon
        End Function

        Public Function AngleXY(p As Point3d) As Double
            Dim a As Double = Math.Atan2(p.Y - Y, p.X - X) * ToDeg
            If a < 0 Then a += 360.0
            Return a
        End Function

        Public Function AngleXZ(p As Point3d) As Double
            Dim a As Double = Math.Atan2(p.Z - Z, p.X - X) * ToDeg
            If a < 0 Then a += 360.0
            Return a
        End Function

        Public Function AngleYZ(p As Point3d) As Double
            Dim a As Double = Math.Atan2(p.Z - Z, p.Y - Y) * ToDeg
            If a < 0 Then a += 360.0
            Return a
        End Function

        ' Unfortunately, this doesn't seem to improve squat!
        'Private Const PI2 As Double = Math.PI / 2
        'Private Const B As Double = 4 / Math.PI
        'Private Const C As Double = -4 / (Math.PI * Math.PI)

        'Public Shared Function FastSin(a As Double) As Double
        '    Return B * a + C * a * Math.Abs(a)
        'End Function

        'Public Shared Function FastCos(a As Double) As Double
        '    Return FastSin(a + PI2)
        'End Function
    End Class
End Namespace
