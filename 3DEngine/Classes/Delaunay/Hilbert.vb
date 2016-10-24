Namespace Delaunay
    ''' <summary>
    ''' Convert between Hilbert index and N-dimensional points.
    ''' 
    ''' The Hilbert index is expressed as an array of transposed bits. 
    ''' 
    ''' Example: 5 bits for each of n=3 coordinates.
    ''' 15-bit Hilbert integer = A B C D E F G H I J K L M N O is stored
    ''' as its Transpose                        ^
    ''' X[0] = A D G J M                    X[2]|  7
    ''' X[1] = B E H K N        <------->       | /X[1]
    ''' X[2] = C F I L O                   axes |/
    '''        high low                         0------> X[0]
    '''        
    ''' NOTE: This algorithm is derived from work done by John Skilling and published in "Programming the Hilbert curve".
    ''' (c) 2004 American Institute of Physics.
    ''' 
    ''' </summary>
    Public NotInheritable Class HilbertCurveTransform
        Private Sub New()
        End Sub
        ''' <summary>
        ''' Convert the Hilbert index into an N-dimensional point expressed as a vector of uints.
        '''
        ''' Note: In Skilling's paper, this function is named TransposetoAxes.
        ''' </summary>
        ''' <param name="transposedIndex">The Hilbert index stored in transposed form.</param>
        ''' <param name="bits">Number of bits per coordinate.</param>
        ''' <returns>Coordinate vector.</returns>
        Public Shared Function HilbertAxes(transposedIndex As UInteger(), bits As Integer) As UInteger()
            Dim X() As UInteger = DirectCast(transposedIndex.Clone(), UInteger())
            Dim nD As Integer = X.Length ' nD: Number of dimensions
            Dim N As UInteger = 2UI << (bits - 1)
            Dim P As UInteger
            Dim Q As UInteger
            Dim t As UInteger
            Dim i As Integer
            ' Gray decode by H ^ (H/2)
            t = X(nD - 1) >> 1
            ' Corrected error in Skilling's paper on the following line. The appendix had i >= 0 leading to negative array index.
            For i = nD - 1 To 1 Step -1
                X(i) = X(i) Xor X(i - 1)
            Next
            X(0) = X(0) Xor t
            ' Undo excess work
            Q = 2
            While Q <> N
                P = Q - 1
                For i = nD - 1 To 0 Step -1
                    If (X(i) And Q) <> 0UI Then
                        X(0) = X(0) Xor P
                    Else
                        ' invert
                        t = (X(0) Xor X(i)) And P
                        X(0) = X(0) Xor t
                        X(i) = X(i) Xor t
                    End If
                Next
                Q <<= 1
            End While
            ' exchange
            Return X
        End Function

        ''' <summary>
        ''' Given the axes (coordinates) of a point in N-Dimensional space, find the distance to that point along the Hilbert curve.
        ''' That distance will be transposed; broken into pieces and distributed into an array.
        ''' 
        ''' The number of dimensions is the length of the hilbertAxes array.
        '''
        ''' Note: In Skilling's paper, this function is called AxestoTranspose.
        ''' </summary>
        ''' <param name="hilbertAxes">Point in N-space.</param>
        ''' <param name="bits">Depth of the Hilbert curve. If bits is one, this is the top-level Hilbert curve.</param>
        ''' <returns>The Hilbert distance (or index) as a transposed Hilbert index.</returns>
        Public Shared Function HilbertIndexTransposed(hilbertAxes As UInteger(), bits As Integer) As UInteger()
            Dim X() As UInteger = DirectCast(hilbertAxes.Clone(), UInteger())
            Dim n As UInteger = hilbertAxes.Length
            ' n: Number of dimensions
            Dim M As UInteger = 1UI << (bits - 1)
            Dim P As UInteger
            Dim Q As UInteger
            Dim t As UInteger
            Dim i As UInteger
            ' Inverse undo
            Q = M
            While Q > 1
                P = Q - 1
                For i = 0 To n - 1
                    If (X(i) And Q) <> 0 Then
                        X(0) = X(0) Xor P
                    Else
                        ' invert
                        t = (X(0) Xor X(i)) And P
                        X(0) = X(0) Xor t
                        X(i) = X(i) Xor t
                    End If
                Next
                Q >>= 1
            End While
            ' exchange
            ' Gray encode
            For i = 1 To n - 1
                X(i) = X(i) Xor X(i - 1)
            Next
            t = 0
            Q = M
            While Q > 1
                If (X(n - 1) And Q) <> 0 Then t = t Xor Q - 1
                Q >>= 1
            End While
            For i = 0 To n - 1
                X(i) = X(i) Xor t
            Next

            Return X
        End Function
    End Class
End Namespace