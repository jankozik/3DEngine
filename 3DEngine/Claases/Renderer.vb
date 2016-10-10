Imports System.Threading

Public Class Renderer
    Public Enum RenderModes
        None = 0
        WireFrame = 2 ^ 0
        ZBuffer = 2 ^ 1
        ZBufferWireframe = 2 ^ 2
    End Enum

    Public Property FOV As Double = 256.0
    Public Property Distance As Double = 10.0
    Public Property AngleX As Double = 0.0
    Public Property AngleY As Double = 0.0
    Public Property AngleZ As Double = 0.0
    Public Property RenderMode As RenderModes = RenderModes.ZBuffer
    Public Property ZBufferPixelSize As Integer = 2
    Public Property BackColor As Color = Color.Black
    Public Property ZBufferWireframeColor As Color = Color.Black

    Private mObjects3D As New Objects3DCollection()
    Private mCamera As New Point3d()
    Private mSurfaceSize As New SizeF(1.0, 1.0)
    Private mZBuffer() As Double ' ZBufferData
    Private mZBufferTransparency As Boolean = False
    Private mZBufferColorDepth As Boolean = True
    Public Property Surface As DirectBitmap

    Private txtFont As New Font("Consolas", 12, FontStyle.Bold)

    Public ReadOnly Property ZBuffer As Double()
        Get
            Return mZBuffer
        End Get
    End Property

    Public Property SurfaceSize As SizeF
        Get
            Return mSurfaceSize
        End Get
        Set(value As SizeF)
            mSurfaceSize = value
            ReDim mZBuffer(mSurfaceSize.Height * mSurfaceSize.Width - 1)
            If Surface IsNot Nothing Then Surface.Dispose()
            Surface = New DirectBitmap(mSurfaceSize.Width, mSurfaceSize.Height)
        End Set
    End Property

    Public Property Camera As Point3d
        Get
            Return mCamera
        End Get
        Set(value As Point3d)
            mCamera = value
        End Set
    End Property

    Public ReadOnly Property Objects3D As Objects3DCollection
        Get
            Return mObjects3D
        End Get
    End Property

    Public Property ZBufferTransparency As Boolean
        Get
            Return mZBufferTransparency
        End Get
        Set(value As Boolean)
            mZBufferTransparency = value
        End Set
    End Property

    Public Property ZBufferColorDepth As Boolean
        Get
            Return mZBufferColorDepth
        End Get
        Set(value As Boolean)
            mZBufferColorDepth = value
        End Set
    End Property

    Public Function TranslatePoint(p As Point3d, Optional rotate As Boolean = True, Optional project As Boolean = True) As Point3d
        If project Then
            If rotate Then
                Return (p.RotateX(AngleX).
                         RotateY(AngleY).
                         RotateZ(AngleZ) - mCamera).
                         Project(mSurfaceSize.Width, mSurfaceSize.Height, FOV, Distance)
            Else
                Return p.Project(mSurfaceSize.Width, mSurfaceSize.Height, FOV, Distance)
            End If
        Else
            If rotate Then
                Return p.RotateX(AngleX).
                         RotateY(AngleY).
                         RotateZ(AngleZ) - mCamera
            Else
                Return p - mCamera
            End If
        End If
    End Function

    Private Function DistanceFromPointToFace(p As Point3d, s As Face, Optional transformPoint As Boolean = False, Optional transFormFace As Boolean = False) As Double
        If transformPoint Then p = TranslatePoint(p)

        Return s.Vertices.Average(Function(v)
                                      If transFormFace Then
                                          Return TranslatePoint(v).Distance(p)
                                      Else
                                          Return v.Distance(p)
                                      End If
                                  End Function)
    End Function

    Public Sub Render(clear As Boolean)
        If clear Then Surface.Clear(BackColor)

        For Each rm As RenderModes In [Enum].GetValues(GetType(RenderModes))
            If (rm And RenderMode) <> RenderModes.None Then
                Select Case rm
                    Case RenderModes.WireFrame : RenderAsWireFrame()
                    Case RenderModes.ZBuffer, RenderModes.ZBufferWireframe
                        RenderAsZBuffer(
                            (RenderMode And RenderModes.ZBuffer) = RenderModes.ZBuffer,
                            (RenderMode And RenderModes.ZBufferWireframe) = RenderModes.ZBufferWireframe)
                End Select
            End If
        Next
    End Sub

    Private Sub RenderAsWireFrame()
        For Each o3d In mObjects3D.Values
            For Each f In o3d.Faces
                Dim p2d(f.Vertices.Count - 1) As PointF

                For v = 0 To f.Vertices.Count - 1
                    p2d(v) = TranslatePoint(f.Vertices(v)).ToPointF()
                Next

                Using p As New Pen(f.Color)
                    'g.DrawPolygon(p, p2d)
                End Using
            Next
        Next
    End Sub

    Private Sub RenderAsZBuffer(Optional fillFaces As Boolean = True, Optional wireFrame As Boolean = False)
        Dim x As Integer
        Dim y As Integer
        Dim z As Double
        Dim pf As Face
        Dim pVertices As New List(Of Point3d)
        Dim uf As Face
        Dim uVertices As New List(Of Point3d)
        Dim minZ As Double
        Dim maxZ As Double

        ResetZBuffer()

        For Each o3d In mObjects3D
            'If Not o3d.Value.IsValid Then
            '    For Each v In o3d.Value.Vertices
            '        minZ = Math.Min(v.Z, minZ)
            '        maxZ = Math.Max(v.Z, maxZ)
            '    Next

            '    For Each v In o3d.Value.Vertices
            '        Dim p As Point3d = TranslatePoint(v)
            '        RenderZPixel(g, Color.Red, p.X, p.Y, p.Z, minZ, maxZ)
            '    Next
            'End If

            For Each f As Face In o3d.Value.Faces
                uVertices.Clear()
                pVertices.Clear()
                f.Vertices.ForEach(Sub(v)
                                       Dim rv As Point3d = TranslatePoint(v, , False)
                                       uVertices.Add(rv)
                                       pVertices.Add(TranslatePoint(rv, False).AsInt(ZBufferPixelSize))
                                   End Sub)
                uf = New Face(uVertices)
                pf = New Face(pVertices)

                If mZBufferTransparency OrElse mZBufferColorDepth Then
                    minZ = Double.MaxValue
                    maxZ = Double.MinValue
                    For Each v In uf.Vertices
                        minZ = Math.Min(v.Z, minZ)
                        maxZ = Math.Max(v.Z, maxZ)
                    Next
                End If

                For y = pf.Top + ZBufferPixelSize To pf.Bottom Step ZBufferPixelSize
                    For x = pf.Left + ZBufferPixelSize To pf.Right Step ZBufferPixelSize
                        If Not pf.Contains(x, y) Then Continue For

                        z = IsZBufferPixelValid(x, y, uf)
                        If z <> Double.MaxValue Then
                            If fillFaces Then RenderZPixel(f.Color, x, y, z, minZ, maxZ)

                            If wireFrame Then
                                Dim isOnEdge As Boolean = False
                                For y1 = y - ZBufferPixelSize To y + ZBufferPixelSize
                                    For x1 = x - ZBufferPixelSize To x + ZBufferPixelSize
                                        If Not pf.Contains(x1, y1) Then
                                            isOnEdge = True
                                            Exit For
                                        End If
                                    Next
                                    If isOnEdge Then Exit For
                                Next

                                If isOnEdge Then
                                    RenderZPixel(ZBufferWireframeColor, x, y, z, minZ, maxZ)
                                ElseIf Not fillFaces Then
                                    RenderZPixel(BackColor, x, y, z, minZ, maxZ)
                                End If
                            End If
                        End If
                    Next
                Next
            Next
        Next
    End Sub

    Private Sub ResetZBuffer()
        For i As Integer = 0 To mZBuffer.Length - 1
            mZBuffer(i) = Double.MaxValue
        Next
    End Sub

    ' Return Double.MaxValue if it's invalid, returns Z if otherwise
    Private Function IsZBufferPixelValid(x As Integer, y As Integer, uf As Face) As Double
        Dim zBufferOffset = y * mSurfaceSize.Width + x
        If zBufferOffset < 0 OrElse zBufferOffset >= mZBuffer.Length Then Return Double.MaxValue

        Dim ray As New Line((New Point3d(x, y, 999)).UnProject(mSurfaceSize.Width,
                                                    mSurfaceSize.Height,
                                                    FOV, Distance),
                             New Point3d(x, y, -999).UnProject(mSurfaceSize.Width,
                                                    mSurfaceSize.Height,
                                                    FOV, Distance))
        Dim interPoint As Point3d = uf.GetPointAtIntersection(ray)
        If interPoint Is Nothing Then Return Double.MaxValue

        If interPoint.Z < mZBuffer(zBufferOffset) Then
            mZBuffer(zBufferOffset) = interPoint.Z
            Return interPoint.Z
        Else
            Return Double.MaxValue
        End If
    End Function

    Private Sub RenderZPixel(c As Color, x As Integer, y As Integer, z As Double, minZ As Double, maxZ As Double)
        Dim alpha As Double = 1.0
        Dim colorDepth As Double = 1.0

        If mZBufferTransparency OrElse mZBufferColorDepth Then
            Dim v As Double
            If minZ = maxZ Then
                v = 1.0
            Else
                v = (z - minZ) / (maxZ - minZ)
            End If
            v *= (1.0 - (Distance - z) / Distance)
            v = Math.Min(1.0, Math.Max(0.0, 1.0 - v))
            colorDepth = If(mZBufferColorDepth, v, 1.0)
            alpha = If(mZBufferTransparency, v, 1.0)
        End If

        If alpha > 0 AndAlso colorDepth > 0 Then
            c = Color.FromArgb(alpha * c.A,
                          colorDepth * c.R,
                          colorDepth * c.G,
                          colorDepth * c.B)

            For y1 As Integer = y To y + ZBufferPixelSize
                For x1 As Integer = x To x + ZBufferPixelSize
                    Surface.Pixel(x1, y1) = c
                Next
            Next
        End If
    End Sub
End Class
