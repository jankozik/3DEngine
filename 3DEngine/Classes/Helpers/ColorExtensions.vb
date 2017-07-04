Imports System.Runtime.CompilerServices

Module ColorExtensions
    <Extension()>
    Public Function Add(c1 As Color, c2 As Color) As Color
        Return Color.FromArgb((CInt(c1.A) + c2.A) / 2,
                              (CInt(c1.R) + c2.R) / 2,
                              (CInt(c1.G) + c2.G) / 2,
                              (CInt(c1.B) + c2.B) / 2)
    End Function
End Module
