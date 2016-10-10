Imports System.IO
Imports System.Windows
Imports System.Windows.Media.Imaging

Public Class GifEncoder
    Private gEnc As New GifBitmapEncoder()

    Public ReadOnly Property FramesDelay As Integer

    Public Sub New(Optional framesDelay As Integer = 0)
        Me.FramesDelay = framesDelay

        'gEnc.Metadata = New BitmapMetadata("gif")
        'gEnc.Metadata.ApplicationName = My.Application.Info.AssemblyName
        'gEnc.Metadata.Author = New ReadOnlyCollection(Of String)(New List(Of String)(New String() {My.Application.Info.CompanyName}))
        'gEnc.Metadata.Copyright = My.Application.Info.Copyright
        'gEnc.Metadata.SetQuery("/grctlext/Delay", framesDelay.ToString())
    End Sub

    Public Sub AddImage(img As Bitmap)
        Dim bmpSrc As BitmapSource = Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                        img.GetHbitmap(),
                                        IntPtr.Zero,
                                        Int32Rect.Empty,
                                        BitmapSizeOptions.FromEmptyOptions())
        gEnc.Frames.Add(BitmapFrame.Create(bmpSrc))
    End Sub

    Public Sub Save(fileName As String)
        'If IO.File.Exists(fileName) Then IO.File.Delete(fileName)
        Using fs As New FileStream(fileName, FileMode.Create)
            gEnc.Save(fs)
        End Using
        gEnc.Frames.Clear()
    End Sub
End Class
