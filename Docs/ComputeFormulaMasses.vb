' This is the VBA code in macro-enabled Excel file ComputeFormulaMasses.xlsm
' See also the Readme at https://github.com/PNNL-Comp-Mass-Spec/Molecular-Weight-Calculator-DLL

Option Explicit

Private mMwtCalculator As New MolecularWeightTool

Public Function ComputeIsotopicMass(formula As String) As Double

On Error GoTo ErrorHandler

    mMwtCalculator.SetElementMode (ElementMassMode.ElementMassMode_Isotopic)

    Dim mass As Double
    mass = mMwtCalculator.ComputeMass(formula)

    ComputeIsotopicMass = mass
    Exit Function

ErrorHandler:
    Debug.Print "Error: " & Err.Description
End Function

Public Function ComputeAverageMass(formula As String) As Double

On Error GoTo ErrorHandler

    mMwtCalculator.SetElementMode (ElementMassMode.ElementMassMode_Average)

    Dim mass As Double
    mass = mMwtCalculator.ComputeMass(formula)

    ComputeAverageMass = mass
    Exit Function

ErrorHandler:
    Debug.Print "Error: " & Err.Description
End Function

