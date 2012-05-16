Option Strict On

Class clsNumberConversionRoutines

    Public Shared Function CDblSafe(ByRef strWork As String) As Double
        On Error Resume Next
        If IsNumber(strWork) Then
            Return Double.Parse(strWork)
        End If
    End Function

    Public Shared Function CIntSafeDbl(ByRef dblWork As Double) As Short
        Return CIntSafe(dblWork)
    End Function

    Public Shared Function CIntSafe(ByRef dblWork As Double) As Short
        If dblWork <= 32767 And dblWork >= -32767 Then
            Return CShort(dblWork)
        Else
            If dblWork < 0 Then
                Return -32767
            Else
                Return 32767
            End If
        End If
    End Function

    Public Shared Function CIntSafe(ByRef strWork As String) As Short
        If IsNumber(strWork) Then
            Return CIntSafeDbl(Double.Parse(strWork))
        ElseIf strWork.ToLower = "true" Then
            Return -1
        Else
            Return 0
        End If
    End Function

    Public Shared Function CStrSafe(ByRef Item As Object) As String
        Try
            If Item Is Nothing Then
                Return String.Empty
            ElseIf Convert.IsDBNull(Item) Then
                Return String.Empty
            Else
                Return CStr(Item)
            End If
        Catch ex As Exception
            Return String.Empty
        End Try
    End Function

    Public Shared Function IsNumber(ByVal strValue As String) As Boolean        
        Try
			Return Double.TryParse(strValue, 0)
        Catch ex As Exception
            Return False
        End Try
    End Function

End Class
