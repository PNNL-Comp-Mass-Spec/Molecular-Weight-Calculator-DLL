' This is the VBA code in macro-enabled Excel file ComputeFormulaMasses.xlsm
' See also the Readme at https://github.com/PNNL-Comp-Mass-Spec/Molecular-Weight-Calculator-DLL

Option Explicit

Private mMwtCalculatorAvg As New MolecularWeightTool
Private mMwtCalculatorIso As New MolecularWeightTool

Private mAvgInitialized As Boolean
Private mIsoInitialized As Boolean

Public Function ComputeAverageMass(formula As String) As Double

On Error GoTo ErrorHandler

    If Not mIsoInitialized Then
        mMwtCalculatorAvg.SetElementMode ElementMassMode_Average
        mAvgInitialized = True
    End If

    Dim mass As Double
    mass = mMwtCalculatorAvg.ComputeMass(formula)

    ComputeAverageMass = mass
    Exit Function

ErrorHandler:
    Debug.Print "Error: " & Err.Description
End Function

Public Function ComputeIsotopicMass(formula As String) As Double

On Error GoTo ErrorHandler

    If Not mIsoInitialized Then
        mMwtCalculatorIso.SetElementMode ElementMassMode_Isotopic
        mIsoInitialized = True
    End If

    Dim mass As Double
    mass = mMwtCalculatorIso.ComputeMass(formula)

    ComputeIsotopicMass = mass
    Exit Function

ErrorHandler:
    Debug.Print "Error: " & Err.Description
End Function
