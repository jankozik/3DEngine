Public Class Vertice
    Private mPoint1 As Point3d
    Private mPoint2 As Point3d

    Public Sub New(p1 As Point3d, p2 As Point3d)
        mPoint1 = p1
        mPoint2 = p2
    End Sub

    Public Property Point1 As Point3d
        Get
            Return mPoint1
        End Get
        Set(value As Point3d)
            mPoint1 = value
        End Set
    End Property

    Public Property Point2 As Point3d
        Get
            Return mPoint2
        End Get
        Set(value As Point3d)
            mPoint2 = value
        End Set
    End Property

    Public Overrides Function Equals(obj As Object) As Boolean
        If TypeOf obj Is Vertice Then
            Dim p = CType(obj, Vertice)

            Return (mPoint1.Equals(p.Point1) AndAlso mPoint2.Equals(p.Point2)) OrElse
                   (mPoint1.Equals(p.Point2) AndAlso mPoint2.Equals(p.Point1))
        Else
            Return False
        End If
    End Function
End Class
