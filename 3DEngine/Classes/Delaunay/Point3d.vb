Namespace Delaunay2
    Public Class Point3d
        Implements IEquatable(Of Point3d)

        Public Const ToRad As Double = Math.PI / 180.0
        Public Const ToDeg As Double = 180.0 / Math.PI

        Public X As Double
        Public Y As Double
        Public Z As Double

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
            Return Math.Sqrt((X - p.X) ^ 2 + (Y - p.Y) ^ 2 + (Z - p.Z) ^ 2)
        End Function

        Public Function Distance2d(p As Point3d) As Double
            Return Math.Sqrt((X - p.X) ^ 2 + (Y - p.Y) ^ 2)
        End Function

        Public Function Length() As Double
            Return Math.Sqrt(X ^ 2 + Y ^ 2 + Z ^ 2)
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
            Dim rad As Double = a * ToRad
            Dim cosa As Double = Math.Cos(rad)
            Dim sina As Double = Math.Sin(rad)
            Dim yn As Double = Y * cosa - Z * sina
            Dim zn As Double = Y * sina + Z * cosa
            Return New Point3d(X, yn, zn)
        End Function

        Public Function RotateY(a As Double) As Point3d
            Dim rad As Double = a * ToRad
            Dim cosa As Double = Math.Cos(rad)
            Dim sina As Double = Math.Sin(rad)
            Dim xn As Double = Z * sina + X * cosa
            Dim zn As Double = Z * cosa - X * sina
            Return New Point3d(xn, Y, zn)
        End Function

        Public Function RotateZ(a As Double) As Point3d
            Dim rad As Double = a * ToRad
            Dim cosa As Double = Math.Cos(rad)
            Dim sina As Double = Math.Sin(rad)
            Dim xn As Double = X * cosa - Y * sina
            Dim yn As Double = X * sina + Y * cosa
            Return New Point3d(xn, yn, Z)
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
            'Return p1.IsSimilar(p2, 0.01)
            Return p1?.X = p2?.X AndAlso p1.Y = p2.Y AndAlso p1.Z = p2.Z
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

        Public Overrides Function ToString() As String
            Return $"({X:F2}, {Y:F2}, {Z:F2})"
        End Function

        Public Function AsInt(Optional padding As Integer = 0) As Point3d
            Dim X1 As Integer = Math.Round(X)
            Dim Y1 As Integer = Math.Round(Y)
            Dim Z1 As Integer = Math.Round(Z)

            If padding > 1 Then
                X1 -= X1 Mod padding
                Y1 -= Y1 Mod padding
                Z1 -= Z1 Mod padding
            End If

            Return New Point3d(X1, Y1, Z1)
        End Function

        Public Shadows Function Equals(other As Point3d) As Boolean Implements IEquatable(Of Point3d).Equals
            Return Me = other
        End Function

        Public Function IsSimilar(p As Point3d, Optional epsilon As Double = 0.0001) As Boolean
            Return Math.Abs(X - p.X) <= epsilon AndAlso
                   Math.Abs(Y - p.Y) <= epsilon AndAlso
                   Math.Abs(Z - p.Z) <= epsilon
        End Function

        Public Function AngleXY(p As Point3d) As Double
            Dim dx As Double = p.X - X
            Dim dy As Double = p.Y - Y

            Dim a As Double = Math.Atan2(dy, dx) * ToDeg
            If a < 0 Then a += 360
            Return a
        End Function

        Public Function AngleXZ(p As Point3d) As Double
            Dim dx As Double = p.X - X
            Dim dz As Double = p.Z - Z

            Dim a As Double = Math.Atan2(dz, dx) * ToDeg
            If a < 0 Then a += 360
            Return a
        End Function

        Public Function AngleYZ(p As Point3d) As Double
            Dim dy As Double = p.Y - Y
            Dim dz As Double = p.Z - Z

            Dim a As Double = Math.Atan2(dz, dy) * ToDeg
            If a < 0 Then a += 360
            Return a
        End Function
    End Class
End Namespace
