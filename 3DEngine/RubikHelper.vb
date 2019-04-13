Public Class RubikHelper
    Public Shared SyncMastObj As New Object()
    Public Shared SyncRotationObj As New Object()

    Public Shared Sub RotateFront(r3d As Renderer)
        Task.Run(Sub() Rotate(r3d,
                              0, 2,
                              0, 2,
                              0, 0,
                              0, 0, 1))
    End Sub

    Public Shared Sub RotateBack(r3d As Renderer)
        Task.Run(Sub() Rotate(r3d,
                              0, 2,
                              0, 2,
                              2, 2,
                              0, 0, 1))
    End Sub

    Public Shared Sub RotateTop(r3d As Renderer)
        Task.Run(Sub() Rotate(r3d,
                              0, 2,
                              0, 0,
                              0, 2,
                              0, 1, 0))
    End Sub

    Public Shared Sub RotateBottom(r3d As Renderer)
        Task.Run(Sub() Rotate(r3d,
                              0, 2,
                              2, 2,
                              0, 2,
                              0, 1, 0))
    End Sub

    Public Shared Sub RotateLeft(r3d As Renderer)
        Task.Run(Sub() Rotate(r3d,
                              0, 0,
                              0, 2,
                              0, 2,
                              1, 0, 0))
    End Sub

    Public Shared Sub RotateRight(r3d As Renderer)
        Task.Run(Sub() Rotate(r3d,
                              2, 2,
                              0, 2,
                              0, 2,
                              1, 0, 0))
    End Sub

    ' TODO: Add support to rotate clockwise
    Public Shared Sub Rotate(r3d As Renderer, xi As Integer, xm As Integer, yi As Integer, ym As Integer, zi As Integer, zm As Integer, i As Integer, j As Integer, k As Integer)
        Dim n As Integer

        SyncLock SyncRotationObj
            For a As Integer = 0 To 90 - 1
                For x As Integer = xi To xm
                    For y As Integer = yi To ym
                        For z As Integer = zi To zm
                            r3d.Objects3D($"Cubie{x}{y}{z}").TransformRotate(i, -j, k)
                        Next
                    Next
                Next
                Threading.Thread.Sleep(5)
            Next
        End SyncLock

        SyncLock SyncMastObj
            If i <> 0 Then
                n = zm + 1
                For i = 0 To n / 2 - 1
                    For j = 0 To (n + 1) / 2 - 1
                        CyclicRoll(r3d,
                                    $"Cubie{xi}{i}{j}",
                                    $"Cubie{xi}{n - 1 - j}{i}",
                                    $"Cubie{xi}{n - 1 - i}{n - 1 - j}",
                                    $"Cubie{xi}{j}{n - 1 - i}")
                    Next
                Next
                Exit Sub
            End If

            If j <> 0 Then
                n = xm + 1
                For i = 0 To n / 2 - 1
                    For j = 0 To (n + 1) / 2 - 1
                        CyclicRoll(r3d,
                                    $"Cubie{i}{yi}{j}",
                                    $"Cubie{n - 1 - j}{yi}{i}",
                                    $"Cubie{n - 1 - i}{yi}{n - 1 - j}",
                                    $"Cubie{j}{yi}{n - 1 - i}")
                    Next
                Next
                Exit Sub
            End If

            If k <> 0 Then
                n = ym + 1
                For i = 0 To n / 2 - 1
                    For j = 0 To (n + 1) / 2 - 1
                        CyclicRoll(r3d,
                                    $"Cubie{i}{j}{zi}",
                                    $"Cubie{n - 1 - j}{i}{zi}",
                                    $"Cubie{n - 1 - i}{n - 1 - j}{zi}",
                                    $"Cubie{j}{n - 1 - i}{zi}")
                    Next
                Next
                Exit Sub
            End If
        End SyncLock
    End Sub

    ' https://stackoverflow.com/questions/2893101/how-to-rotate-a-n-x-n-matrix-by-90-degrees
    Private Shared Sub CyclicRoll(r3d As Renderer, ByRef a As String, ByRef b As String, ByRef c As String, ByRef d As String)
        Dim tmp As String = a
        ChangeCubieName(r3d, a, b, "", "tmp")
        ChangeCubieName(r3d, b, c, "", "tmp")
        ChangeCubieName(r3d, c, d, "", "tmp")
        ChangeCubieName(r3d, d, tmp, "", "tmp")

        ChangeCubieName(r3d, a, a, "tmp", "")
        ChangeCubieName(r3d, b, b, "tmp", "")
        ChangeCubieName(r3d, c, c, "tmp", "")
        ChangeCubieName(r3d, d, d, "tmp", "")
    End Sub

    Private Shared Sub ChangeCubieName(r3d As Renderer, oldName As String, newName As String, oldExtra As String, newExtra As String)
        Try
            oldName += oldExtra
            Dim i As Object3D = r3d.Objects3D(oldName)
            r3d.Objects3D.Remove(oldName)
            r3d.Objects3D.Add(newName + newExtra, i)
        Catch
        End Try
    End Sub
End Class