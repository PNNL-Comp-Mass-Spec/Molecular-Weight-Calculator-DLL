Friend Class clsFormulaFinderCandidateElement

    ReadOnly mOriginalName As String

    Public Property Mass As Double
    Public Property Charge As Double

    Public Property CountMinimum As Integer
    Public Property CountMaximum As Integer

    Public Property PercentCompMinimum As Double
    Public Property PercentCompMaximum As Double

    ' ReSharper disable once ConvertToVbAutoProperty
    Public ReadOnly Property OriginalName As String
        Get
            Return mOriginalName
        End Get
    End Property

    Public Property Symbol As String
   
    Public Sub New(elementOrAbbrevSymbol As String)
        mOriginalName = String.Copy(elementOrAbbrevSymbol)
        Symbol = String.Copy(elementOrAbbrevSymbol)
    End Sub

End Class
