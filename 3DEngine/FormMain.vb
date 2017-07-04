Imports System.Drawing.Drawing2D
Imports System.Threading
Imports N3DEngine.Renderer

Public Class FormMain
    Private mouseOrigin As Point
    Private isMouseLeftButtonDown As Boolean
    Private isMouseRightButtonDown As Boolean
    Private renderThread As Thread

    Private r3D As New Renderer()

    Private gifAnim As New GifEncoder(100)
    Private gifAnimEnable As Boolean = False
    Private captureFrame As Boolean
    Private breakRenderingThread As Boolean

    Private syncObj As New Object()

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        Me.SetStyle(ControlStyles.ResizeRedraw, True)
        Me.SetStyle(ControlStyles.UserPaint, True)
    End Sub

    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CreateEventHandlers()
        InitializeScene()

        'AddObjectsToScene_Sample1()
        AddObjectsToScene_Sample2()
        'AddObjectsToScene_Sample3()
        'AddObjectsToScene_Sample4()

        Dim txt As String = ""
        For Each o In r3D.Objects3D
            For Each v In o.Value.Vertices
                txt += v.ToString() + Environment.NewLine
            Next
        Next
        IO.File.WriteAllText(IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "sphere.txt"), txt)
        r3D.Objects3D.ForEach(Sub(o3d) RandomizesFacesColors(o3d.Value))

        StartRenderingThread()
    End Sub

    Private Sub InitializeScene()
        r3D.FOV = 512
        r3D.Distance = 40

        r3D.RenderMode = RenderModes.ZBuffer 'Or RenderModes.ZBufferWireframe
        r3D.BackColor = Color.Black
        r3D.ZBufferWireframeColor = Color.White
        r3D.ZBufferPixelSize = 2
        r3D.ZBufferColorDepth = True
        r3D.ZBufferTransparency = False

        SetSurfaceSize()
    End Sub

    Private Sub CreateEventHandlers()
        AddHandler Me.FormClosing, Sub()
                                       SyncLock syncObj
                                           breakRenderingThread = True
                                       End SyncLock
                                   End Sub
        AddHandler Me.SizeChanged, Sub() SetSurfaceSize()
        AddHandler Me.KeyDown, Sub(s1 As Object, e1 As KeyEventArgs)
                                   If e1.KeyCode = Keys.Enter Then
                                       If gifAnimEnable Then
                                           gifAnimEnable = False
                                           gifAnim.Save(IO.Path.Combine(
                                                            Environment.GetFolderPath(
                                                                Environment.SpecialFolder.Desktop),
                                                                "3dcapture.gif"))
                                       Else
                                           gifAnimEnable = True
                                       End If
                                   End If
                               End Sub
    End Sub

    Private Sub StartRenderingThread()
        renderThread = New Thread(Sub()
                                      Const frameRate As Integer = 30
                                      Dim delay As Integer = 1000 / frameRate
                                      Dim delayCounter As Integer
                                      Dim sw As New Stopwatch()

                                      Do
                                          sw.Restart()
                                          SyncLock syncObj
                                              r3D.Render(True)
                                          End SyncLock
                                          sw.Stop()

                                          Thread.Sleep(Math.Max(0, delay - sw.ElapsedMilliseconds))
                                          Me.Invalidate()

                                          If gifAnimEnable Then
                                              delayCounter += delay
                                              If delayCounter >= gifAnim.FramesDelay Then
                                                  captureFrame = True
                                                  delayCounter = 0
                                              End If
                                          End If
                                      Loop Until breakRenderingThread
                                  End Sub) With {
                                    .IsBackground = True
                                  }
        renderThread.Start()
    End Sub

    Private Sub SetSurfaceSize()
        SyncLock syncObj
            r3D.SurfaceSize = Me.DisplayRectangle.Size
        End SyncLock
        r3D.Camera = New Point3d(0.0, 0.0, -10.0)
    End Sub

    Private Sub AddObjectsToScene_Sample1()
        r3D.Objects3D.Add("Cube", New Object3D(Primitives.Cube(8), Color.Blue))
        'r3D.Objects3D("Cube").TransformRotate(45, 0, 0)
    End Sub

    Private Sub AddObjectsToScene_Sample2()
        r3D.Objects3D.Add("Cube",
                          New Object3D(Primitives.Cube(8),
                                       Color.Blue))
        r3D.Objects3D.Add("Tetrahedron",
                          New Object3D(Primitives.Tetrahedron(8),
                                       Color.Red))
        r3D.Objects3D.Add("SquarePyramid",
                          New Object3D(Primitives.SquarePyramid(8),
                                       Color.Green))

        r3D.Objects3D("Cube").TransformMove(-6, 0, 0)
        r3D.Objects3D("SquarePyramid").TransformRotate(-90, 0, 0)
        r3D.Objects3D("SquarePyramid").TransformMove(18, 0, 0)
    End Sub

    Private Sub AddObjectsToScene_Sample3()
        r3D.Objects3D.Add("Cuboid", New Object3D(Primitives.Cuboid(4, 12), Color.Blue))
    End Sub

    Private Sub AddObjectsToScene_Sample4()
        'r3D.Objects3D.Add("Sphere", New Object3D(Primitives.Sphere(14), Color.Blue))
        r3D.Objects3D.Add("Sphere", New Object3D(Primitives.Sphere2(14, 45), Color.Blue))
    End Sub

    Private Sub RandomizesFacesColors(object3D As Object3D)
        Dim colors As New List(Of Color) From {
            Color.Red,
            Color.Blue,
            Color.Green,
            Color.Yellow,
            Color.Magenta,
            Color.Orange,
            Color.Cyan,
            Color.Gray,
            Color.White
        }

        'colors = Shuffle(colors)

        ' Added support for coplanar faces
        Dim j As Integer
        object3D.Faces.ForEach(Sub(face) face.Index = -1)
        object3D.Faces.ForEach(Sub(face)
                                   If face.Index = -1 Then
                                       face.Index = j
                                       face.Color = colors(j)

                                       For i As Integer = 0 To object3D.Faces.Count - 1
                                           If Not object3D.Faces(i).Equals(face) Then
                                               If object3D.Faces(i).Normal = face.Normal Then
                                                   Dim hasCommonVertices As Boolean = False
                                                   For Each v In object3D.Faces(i).Vertices
                                                       If face.Vertices.Contains(v) Then
                                                           hasCommonVertices = True
                                                           Exit For
                                                       End If
                                                   Next
                                                   If hasCommonVertices Then
                                                       object3D.Faces(i).Color = face.Color
                                                       object3D.Faces(i).Index = face.Index
                                                       Exit For
                                                   End If
                                               End If
                                           End If
                                       Next

                                       j += 1
                                       j = j Mod colors.Count
                                   End If
                               End Sub)
    End Sub

    Private Function Shuffle(Of T)(list As List(Of T)) As List(Of T)
        Dim rnd As New Random()
        Dim n As Integer = list.Count

        While n > 1
            n -= 1

            Dim k As Integer = rnd.Next(n + 1)
            Dim value As T = list(k)
            list(k) = list(n)
            list(n) = value
        End While

        Return list
    End Function

    Private Sub Main_Paint(sender As Object, e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        Dim g As Graphics = e.Graphics
        'g.SmoothingMode = SmoothingMode.AntiAlias 
        'g.InterpolationMode = InterpolationMode.Bicubic

        SyncLock syncObj
            If r3D.Surface Is Nothing Then Exit Sub
            Dim bmp As Bitmap = r3D.Surface
            g.DrawImageUnscaled(bmp, Point.Empty)

            If gifAnimEnable Then
                If captureFrame Then
                    gifAnim.AddImage(bmp.Clone())
                    captureFrame = False
                End If
                g.DrawString("Recording", Me.Font, Brushes.Red, Point.Empty)
            End If
        End SyncLock
        'DrawAxis(g)

        If r3D.Objects3D.ContainsKey("Sphere") Then
            'r3D.Objects3D("Sphere").TransformRotate(2.0, 2.5, -1.0)
        End If
    End Sub

    Private Sub DrawAxis(g As Graphics)
        Dim p1 As Point3d
        Dim p2 As Point3d
        Dim r As New Point3d(10, 10, 10)

        'r = New Point3d(Me.DisplayRectangle.Width, Me.DisplayRectangle.Height, 0)
        'r = r.UnProject(Me.DisplayRectangle.Width, Me.DisplayRectangle.Height, fov, distance)

        ' X
        p1 = r3D.TranslatePoint(New Point3d(-r.X, 0.1, 0))
        p2 = r3D.TranslatePoint(New Point3d(+r.X, 0.1, 0))
        g.DrawLine(Pens.Red, p1.ToPointF(), p2.ToPointF())

        ' Y
        p1 = r3D.TranslatePoint(New Point3d(0, -r.Y, 0))
        p2 = r3D.TranslatePoint(New Point3d(0, +r.Y, 0))
        g.DrawLine(Pens.Green, p1.ToPointF(), p2.ToPointF())

        ' Z
        Dim m As Double = Math.Max(r.X, r.Y) / 2
        p1 = r3D.TranslatePoint(New Point3d(0, 0, -m))
        p2 = r3D.TranslatePoint(New Point3d(0, 0, +m))
        g.DrawLine(Pens.Blue, p1.ToPointF(), p2.ToPointF())
    End Sub

    Private Sub Main_MouseDown(sender As Object, e As MouseEventArgs) Handles Me.MouseDown
        isMouseLeftButtonDown = (e.Button = MouseButtons.Left)
        isMouseRightButtonDown = (e.Button = MouseButtons.Right)
        mouseOrigin = e.Location
    End Sub

    Private Sub Main_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
        SyncLock syncObj
            If isMouseLeftButtonDown Then
                r3D.AngleX += e.Location.Y - mouseOrigin.Y
                r3D.AngleY -= e.Location.X - mouseOrigin.X
                'r3D.Objects3D.ForEach(Sub(o) o.Value.TransformRotate((e.Location.Y - mouseOrigin.Y),
                '                                                     -(e.Location.X - mouseOrigin.X),
                '                                                      0))
                'r3D.Camera = r3D.Camera.RotateX(e.Location.Y - mouseOrigin.Y).
                '                        RotateY(-(e.Location.X - mouseOrigin.X))
            ElseIf isMouseRightButtonDown Then
                'r3D.Objects3D.ForEach(Sub(o) o.Value.TransformMove((e.Location.X - mouseOrigin.X) / 20,
                '                                                   (e.Location.Y - mouseOrigin.Y) / 20,
                '                                                    0))
                r3D.Camera.X -= (e.Location.X - mouseOrigin.X) / 20
                r3D.Camera.Y -= (e.Location.Y - mouseOrigin.Y) / 20
            End If
        End SyncLock

        mouseOrigin = e.Location
    End Sub

    Private Sub Main_MouseUp(sender As Object, e As MouseEventArgs) Handles Me.MouseUp
        If e.Button = MouseButtons.Left Then isMouseLeftButtonDown = False
        If e.Button = MouseButtons.Right Then isMouseRightButtonDown = False
    End Sub

    Private Sub FormMain_MouseWheel(sender As Object, e As MouseEventArgs) Handles Me.MouseWheel
        'r3D.Distance -= e.Delta / 30
        'r3D.FOV -= e.Delta / 30
        SyncLock syncObj
            r3D.Camera.Z += e.Delta / 30
        End SyncLock
    End Sub
End Class