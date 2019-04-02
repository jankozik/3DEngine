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

    Private pVertices As New List(Of Point3d)
    Private uVertices As New List(Of Point3d)

    Private mObjects3D As New Objects3DCollection()
    Private mCamera As New Point3d()
    Private mSurfaceSize As New SizeF(1.0, 1.0)
    Private mZBuffer() As Double ' ZBufferData
    Private mZBufferTransparency As Boolean = False
    Private mZBufferColorDepth As Boolean = True

    Public Property Surface As DirectBitmap

    Private txtFont As New Font("Consolas", 12, FontStyle.Bold)

    Private timer As Stopwatch
    Private framesCounter As Integer

    Public Sub New()
        timer = Stopwatch.StartNew()
    End Sub

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

    'Private Function DistanceFromPointToFace(p As Point3d, s As Face, Optional transformPoint As Boolean = False, Optional transFormFace As Boolean = False) As Double
    '    If transformPoint Then p = TranslatePoint(p)

    '    Return s.Vertices.Average(Function(v)
    '                                  If transFormFace Then
    '                                      Return TranslatePoint(v).Distance(p)
    '                                  Else
    '                                      Return v.Distance(p)
    '                                  End If
    '                              End Function)
    'End Function

    Public ReadOnly Property FramesPerSecond As Double
        Get
            If framesCounter > 1000 Then
                framesCounter = 0
                timer.Restart()
            End If
            Return framesCounter / (timer.ElapsedMilliseconds / 1000)
        End Get
    End Property

    Public Sub Render(clear As Boolean)
        framesCounter += 1

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
                Dim p2ds(f.Vertices.Count - 1) As PointF

                For v = 0 To f.Vertices.Count - 1
                    p2ds(v) = TranslatePoint(f.Vertices(v)).ToPointF()
                Next

                Surface.DrawPolygon(f.Color, p2ds)
            Next
        Next
    End Sub

    Private Sub RenderAsZBuffer(Optional fillFaces As Boolean = True, Optional wireFrame As Boolean = False)
        Dim x As Integer
        Dim y As Integer
        Dim z As Double
        Dim pf As Face
        Dim uf As Face
        Dim minZ As Double
        Dim maxZ As Double
        Dim rv As Point3d

        ResetZBuffer()

        For Each o3d In mObjects3D
            ' I think it would be faster to first process all faces (list of projected [pf] and un-projected [uf])
            '    and then render the faces in Z order (from closest to farthest).
            ' Doing so would minimize the instances where a ZPixel is render more than once.

            For Each f As Face In o3d.Value.Faces
                uVertices.Clear()
                pVertices.Clear()
                f.Vertices.ForEach(Sub(v)
                                       rv = TranslatePoint(v, , False)
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

                If o3d.Value.IsValid AndAlso o3d.Value.IsSolid Then
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
                Else
                    f.Vertices.ForEach(Sub(v) RenderZPixel(Color.Red, TranslatePoint(v).AsInt(ZBufferPixelSize), minZ, maxZ))
                End If
            Next
        Next
    End Sub

    Private Sub ResetZBuffer()
        'Tasks.Parallel.For(0, mZBuffer.Length, Sub(i As Integer)
        '                                           mZBuffer(i) = Double.MaxValue
        '                                       End Sub)

        Dim degreeOfParallelism As Integer = Environment.ProcessorCount
        Dim len As Integer = mZBuffer.Length
        Parallel.For(0, degreeOfParallelism, Sub(workerId As Integer)
                                                 Dim max As Integer = len * (workerId + 1) / degreeOfParallelism
                                                 For i As Integer = len * workerId / degreeOfParallelism To max - 1
                                                     mZBuffer(i) = Double.MaxValue
                                                 Next
                                             End Sub)
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

        If interPoint?.Z < mZBuffer(zBufferOffset) Then
            mZBuffer(zBufferOffset) = interPoint.Z
            Return interPoint.Z
        Else
            Return Double.MaxValue
        End If
    End Function

    Private Sub RenderZPixel(c As Color, p As Point3d, minZ As Double, maxZ As Double)
        RenderZPixel(c, p.X, p.Y, p.Z, minZ, maxZ)
    End Sub

    Private Sub RenderZPixel(c As Color, x As Integer, y As Integer, z As Double, minZ As Double, maxZ As Double)
        Dim alpha As Double = 1.0
        Dim colorDepth As Double = 1.0

        If mZBufferTransparency OrElse mZBufferColorDepth Then
            Dim v As Double
            If minZ = maxZ Then
                v = 1.0
            Else
                minZ -= 10  ' Not sure about this, but it does make the shading more noticeable
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

            ' TODO: Implement an averaging function so that instead of averaging with the previous pixel,
            '       the new one is averaged with all the surrounding ones. (Cheap anti-aliasing)
            'c.Add(GetZPixel(x, y, z))

            For y1 As Integer = y To y + ZBufferPixelSize
                For x1 As Integer = x To x + ZBufferPixelSize
                    Surface.Pixel(x1, y1) = c
                Next
            Next
        End If
    End Sub

    Private Function GetZPixel(x As Integer, y As Integer, z As Integer) As Color
        Dim a As Integer
        Dim r As Integer
        Dim g As Integer
        Dim b As Integer
        Dim n As Integer

        Dim c As Color
        For y1 As Integer = y To y + ZBufferPixelSize
            For x1 As Integer = x To x + ZBufferPixelSize
                c = Surface.Pixel(x1, y1)
                a += c.A
                r += c.R
                g += c.G
                b += c.B
                n += 1
            Next
        Next

        Return Color.FromArgb(a / n, r / n, g / n, b / n)
    End Function

    Private Sub RenderZLine(c1 As Color, p1 As Point3d, c2 As Color, p2 As Point3d, minZ As Double, maxZ As Double)
        Dim dx As Double = p2.X - p1.X
        Dim dy As Double = p2.Y - p1.Y
        Dim l As Double = Math.Sqrt(dx * dx + dy * dy)
        Dim a As Double = Math.Atan2(dy, dx)
        Dim p As New Point3d()
        Dim d As Double
        Dim c As Color

        Dim Blend = Function(v1 As Double, v2 As Double) (v2 - v1) * d + v1

        For r = 0 To l
            d = r / l

            p.X = p1.X + r * Math.Cos(-a)
            p.Y = p1.Y + r * Math.Sin(a)
            p.Z = Blend(p1.Z, p2.Z)

            c = Color.FromArgb(Blend(c1.A, c2.A), Blend(c1.R, c2.R), Blend(c1.G, c2.G), Blend(c1.B, c2.B))
            RenderZPixel(c, p, minZ, maxZ)
        Next
    End Sub
End Class
