Imports System.Runtime.CompilerServices

Module ArrayExtensions
    <Extension()>
    Public Sub Add(Of T)(ByRef n() As T, value As T)
        ReDim Preserve n(n.Length)
        n(n.Length - 1) = value
    End Sub

    <Extension()>
    Public Sub Add(Of T)(ByRef n1() As T, n2() As T)
        n1.Add(n2, 0, n2.Length)
    End Sub

    <Extension()>
    Public Sub Add(Of T)(ByRef n1() As T, n2() As T, length As Integer)
        n1.Add(n2, 0, length)
    End Sub

    <Extension()>
    Public Sub Add(Of T)(ByRef n1() As T, n2() As T, offset As Integer, length As Integer)
        ReDim Preserve n1(n1.Length + length - 1)
        Array.Copy(n2, offset, n1, n1.Length - length, length)
    End Sub

    <Extension()>
    Public Sub RemoveByValue(Of T)(ByRef n() As T, value As T)
        n = RemoveByValue2(n, value)
    End Sub

    <Extension()>
    Private Function RemoveByValue2(Of T)(n() As T, value As T) As T()
        Dim r() As T = {}
        For i As Integer = 0 To n.Length - 1
            If Not n(i).Equals(value) Then r.Add(n(i))
        Next

        Return r
    End Function

    <Extension()>
    Public Function RemoveByIndex(Of T)(n() As T, index As Integer) As T()
        Dim r(n.Length - 2) As T
        Dim j As Integer
        For i As Integer = 0 To n.Length - 1
            If i <> index Then
                r(j) = n(i)
                j += 1
            End If
        Next

        Return r
    End Function

    <Extension()>
    Public Function Permutate(Of T)(n() As T) As T()()
        Dim subPerm()() As T
        Dim length As Integer = n.Length
        Dim res() As T = {}

        If n.Length = 2 Then
            res = New T() {n(0), n(1), n(1), n(0)}
        Else
            For i As Integer = 0 To length - 1
                subPerm = Permutate(n.RemoveByIndex(i))
                For j As Integer = 0 To subPerm.Length - 1
                    res.Add(n(i))
                    res.Add(subPerm(j))
                Next
            Next
        End If

        Dim k()() As T = {}
        For i As Integer = 0 To res.Length \ length - 1
            ReDim Preserve k(i)
            k(i) = New T() {}
            k(i).Add(res, i * length, length)
        Next

        Return k
    End Function

    <Extension()>
    Public Function Permutate2(Of T)(n() As T, Optional deLinearize As Boolean = True) As T()()
        Dim subPerm()() As T
        Dim length As Integer = n.Length
        Dim subPermNum As Integer = (length - 1).Fact()
        Dim res() As T = {}

        If n.Length = 2 Then
            res = New T() {n(0), n(1), n(1), n(0)}
        Else
            For i As Integer = 0 To length - 1
                subPerm = Permutate2(n.RemoveByIndex(i), False)
                Dim permLen As Integer = subPerm(0).Length \ subPermNum
                For j As Integer = 0 To subPermNum - 1
                    res.Add(n(i))
                    res.Add(subPerm(0), j * permLen, permLen)
                Next
            Next
        End If

        Dim k()() As T = {}
        If deLinearize Then
            For i As Integer = 0 To res.Length \ length - 1
                ReDim Preserve k(i)
                k(i) = New T() {}
                k(i).Add(res, i * length, length)
            Next
        Else
            ReDim k(0)
            k(0) = New T() {}
            k(0).Add(res)
        End If

        Return k
    End Function

    <Extension()>
    Public Function Permutate3(Of T)(n() As T, Optional deLinearize As Boolean = True) As T()()
        Dim subPerm()() As T
        Dim length As Integer = n.Length
        Dim permNum As Integer = length.Fact()
        Dim res(permNum * length - 1) As T

        If length = 2 Then
            res = New T() {n(0), n(1), n(1), n(0)}
        Else
            Dim subPermNum As Integer = permNum / length ' (length - 1).Fact() = (!5 = !6/6)
            Dim subPermLen As Integer
            Dim resIndex As Integer

            For i As Integer = 0 To length - 1
                subPerm = Permutate3(n.RemoveByIndex(i), False)
                subPermLen = subPerm(0).Length \ subPermNum
                For j As Integer = 0 To subPermNum - 1
                    res(resIndex) = n(i)
                    resIndex += 1
                    Array.Copy(subPerm(0), j * subPermLen, res, resIndex, subPermLen)
                    resIndex += subPermLen
                Next
            Next
        End If

        Dim k(If(deLinearize, permNum - 1, 0))() As T
        If deLinearize Then
            For i As Integer = 0 To permNum - 1
                k(i) = New T(length - 1) {}
                Array.Copy(res, i * length, k(i), 0, length)
            Next
        Else
            k(0) = New T(res.Length - 1) {}
            Array.Copy(res, 0, k(0), 0, res.Length)
        End If

        Return k
    End Function

    <Extension()>
    Public Iterator Function Permutate4(Of T)(n() As T) As IEnumerable(Of T())
        Dim length As Integer = n.Length

        If n.Length = 2 Then
            Yield New T() {n(0), n(1)}
            Yield New T() {n(1), n(0)}
        Else
            Dim permNum As Integer = length.Fact()
            Dim subPermLen As Integer = length - 1
            Dim subPermNum As Integer = permNum / length

            For i As Integer = 0 To length - 1
                For Each sp In Permutate4(n.RemoveByIndex(i))
                    Dim r(length - 1) As T
                    r(0) = n(i)
                    Array.Copy(sp, 0, r, 1, subPermLen)
                    Yield r
                Next
            Next
        End If
    End Function

    <Extension>
    Public Function Unique(Of T)(values()() As T) As T()()
        Dim length As Integer = values(0).Length
        Dim isEqual As Boolean = False
        Dim res()() As T = Nothing
        Dim resIndex As Integer

        For i As Integer = 0 To values.Length - 1
            For j As Integer = 0 To i - 1

                isEqual = True
                For k = 0 To length - 1
                    If Not values(i)(k).Equals(values(j)(k)) Then
                        isEqual = False
                        Exit For
                    End If
                Next
                If isEqual Then Exit For
            Next

            If Not isEqual Then
                ReDim Preserve res(resIndex)
                res(resIndex) = New T(length - 1) {}
                Array.Copy(values(i), 0, res(resIndex), 0, length)
                resIndex += 1
            End If
        Next

        Return res
    End Function

    <Extension()>
    Public Function Fact(value As Integer) As Double
        Dim result As ULong = 1

        For value = value To 2 Step -1
            result *= value
        Next

        Return result
    End Function

    <Extension()>
    Public Function ToStringList(Of T)(n() As T) As String
        Dim r As String = ""
        For i As Integer = 0 To n.Length - 1
            r += n(i).ToString() + ", "
        Next

        If r <> "" Then
            Return r.Substring(0, r.Length - 2)
        Else
            Return ""
        End If
    End Function
End Module