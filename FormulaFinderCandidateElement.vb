Friend Class FormulaFinderCandidateElement

    Public Property Mass As Double
    Public Property Charge As Double

    Public Property CountMinimum As Integer
    Public Property CountMaximum As Integer

    Public Property PercentCompMinimum As Double
    Public Property PercentCompMaximum As Double

    Public ReadOnly Property OriginalName As String

    Public Property Symbol As String

    Public Sub New(elementOrAbbrevSymbol As String)
        OriginalName = String.Copy(elementOrAbbrevSymbol)
        Symbol = String.Copy(elementOrAbbrevSymbol)
    End Sub

    Public Overrides Function ToString() As String
        If Symbol = OriginalName Then
            Return Symbol & ": " & Mass.ToString("0.0000") & " Da, charge " & Charge.ToString()
        Else
            Return OriginalName & "(" & Symbol & "): " & Mass.ToString("0.0000") & " Da, charge " & Charge.ToString()
        End If
    End Function
End Class
