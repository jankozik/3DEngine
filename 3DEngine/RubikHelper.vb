Imports System.Threading

Public NotInheritable Class RubikHelper
    Public Shared SyncMastObj As New Object()
    Public Shared SyncRotationObj As New Object()

    Public Shared Sub RotateFront(r3d As Renderer, ccw As Boolean)
        Rotate(r3d,
                0, 2,
                0, 2,
                0, 0,
                0, 0, If(ccw, -1, 1))
    End Sub

    Public Shared Sub RotateBack(r3d As Renderer, ccw As Boolean)
        Rotate(r3d,
                0, 2,
                0, 2,
                2, 2,
                0, 0, If(ccw, 1, -1))
    End Sub

    Public Shared Sub RotateUp(r3d As Renderer, ccw As Boolean)
        Rotate(r3d,
                0, 2,
                0, 0,
                0, 2,
                0, If(ccw, 1, -1), 0)
    End Sub

    Public Shared Sub RotateDown(r3d As Renderer, ccw As Boolean)
        Rotate(r3d,
                0, 2,
                2, 2,
                0, 2,
                0, If(ccw, -1, 1), 0)
    End Sub

    Public Shared Sub RotateLeft(r3d As Renderer, ccw As Boolean)
        Rotate(r3d,
                0, 0,
                0, 2,
                0, 2,
                If(ccw, -1, 1), 0, 0)
    End Sub

    Public Shared Sub RotateRight(r3d As Renderer, ccw As Boolean)
        Rotate(r3d,
                2, 2,
                0, 2,
                0, 2,
                If(ccw, 1, -1), 0, 0)
    End Sub

    Public Shared Sub Rotate(r3d As Renderer, xi As Integer, xm As Integer, yi As Integer, ym As Integer, zi As Integer, zm As Integer, i As Integer, j As Integer, k As Integer)
        Dim n As Integer

        SyncLock SyncRotationObj
            For a As Integer = 0 To 90 - 1
                For x As Integer = xi To xm
                    For y As Integer = yi To ym
                        For z As Integer = zi To zm
                            SyncLock SyncMastObj
                                r3d.Objects3D($"Cubie{x}{y}{z}").TransformRotate(i, -j, k)
                            End SyncLock
                        Next
                    Next
                Next
#If Not DEBUG Then
                If (a Mod 30) = 0 Then Thread.Sleep(1)
#End If
            Next
        End SyncLock

        SyncLock SyncMastObj
            If i <> 0 Then
                n = zm + 1
                For r = 1 To If(i < 0, 3, 1)
                    For i = 0 To n / 2 - 1
                        For j = 0 To (n + 1) / 2 - 1
                            CyclicRoll(r3d,
                                        $"Cubie{xi}{i}{j}",
                                        $"Cubie{xi}{n - 1 - j}{i}",
                                        $"Cubie{xi}{n - 1 - i}{n - 1 - j}",
                                        $"Cubie{xi}{j}{n - 1 - i}")
                        Next
                    Next
                Next
                Exit Sub
            End If

            If j <> 0 Then
                n = xm + 1
                For r = 1 To If(j < 0, 3, 1)
                    For i = 0 To n / 2 - 1
                        For j = 0 To (n + 1) / 2 - 1
                            CyclicRoll(r3d,
                                    $"Cubie{i}{yi}{j}",
                                    $"Cubie{n - 1 - j}{yi}{i}",
                                    $"Cubie{n - 1 - i}{yi}{n - 1 - j}",
                                    $"Cubie{j}{yi}{n - 1 - i}")
                        Next
                    Next
                Next
                Exit Sub
            End If

            If k <> 0 Then
                n = ym + 1
                For r = 1 To If(k < 0, 3, 1)
                    For i = 0 To n / 2 - 1
                        For j = 0 To (n + 1) / 2 - 1
                            CyclicRoll(r3d,
                                    $"Cubie{i}{j}{zi}",
                                    $"Cubie{n - 1 - j}{i}{zi}",
                                    $"Cubie{n - 1 - i}{n - 1 - j}{zi}",
                                    $"Cubie{j}{n - 1 - i}{zi}")
                        Next
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

    Private Delegate Sub RotateIt(r3d As Renderer, ccw As Boolean)

    Public Shared Sub Parse(r3d As Renderer, algo As String)
        Dim tokens As String = "UDRLFB"
        Dim ccw As Boolean
        Dim r As Integer
        Dim l As Integer = algo.Length - 1
        Dim rf As RotateIt() = {New RotateIt(AddressOf RotateUp),
                                New RotateIt(AddressOf RotateDown),
                                New RotateIt(AddressOf RotateRight),
                                New RotateIt(AddressOf RotateLeft),
                                New RotateIt(AddressOf RotateFront),
                                New RotateIt(AddressOf RotateBack)}

        For i As Integer = 0 To l
            If tokens.Contains(algo(i)) Then
                Dim o As Integer = 0

                If i < l Then
                    If algo(i + 1) = "'" Then
                        o = 1
                        ccw = True
                    Else
                        ccw = False
                    End If

                    If i + o < l AndAlso Char.IsNumber(algo(i + 1 + o)) Then
                        r = Integer.Parse(algo(i + 1 + o))
                    Else
                        r = 1
                    End If
                End If

                While r > 0
                    rf(tokens.IndexOf(algo(i))).Invoke(r3d, ccw)
                    r -= 1
                End While

                i += o
            End If
        Next
    End Sub
End Class