# Molecular Weight Calculator

The Molecular Weight Calculator can be used to calculate the molecular weight and percent composition 
of chemical formulas and amino acids (peptides). It recognizes user-definable abbreviations and 
custom elemental isotopes. It also includes a Mole/Mass Converter, Mass-to-Charge Converter, 
Formula Finder, Capillary Flow Calculator, Amino Acid Notation Converter, Isotopic Distribution Calculator, 
and Peptide Sequence Fragmentation Modeler. 

The Molecular Weight Calculator is available as either a Windows-based GUI application or as a C# DLL.
* To use the GUI, either download the zip file with MolecularWeightCalculatorGUI.exe or download and run the installer
* To use the C# DLL, add it to a .NET project

## Downloads

Release versions of the GUI and DLL can be found on GitHub at\
https://github.com/PNNL-Comp-Mass-Spec/Molecular-Weight-Calculator/releases

### Continuous Integration

The latest version of the DLL may be available on the [AppVeyor CI server](https://ci.appveyor.com/project/PNNLCompMassSpec/molecular-weight-calculator-dll/build/artifacts), 
but builds are deleted after 6 months. \
[![Build status](https://ci.appveyor.com/api/projects/status/akbap1xwd92flnsv?svg=true)](https://ci.appveyor.com/project/PNNLCompMassSpec/molecular-weight-calculator-dll)

## Data Source

Element and isotope mass and percent composition values used in the Molecular Weight Calculator were obtained from NIST
* https://www.nist.gov/pml/atomic-weights-and-isotopic-compositions-relative-atomic-masses

The atomic weight data that NIST provides comes from [Atomic Weights of the Elements 2013](http://www.ciaaw.org/atomic-weights.htm) by J. Meija et al. 
The isotopic composition data comes from [Isotopic Compositions of the Elements 2009](https://doi.org/10.1351/PAC-REP-10-06-02) by M. Berglund and M.E. Wieser.

For elements with a standard atomic weight range (e.g. [6.938,6.997] for Lithium), the conventional atomic weight is used.
* See Table 3 in "Atomic weights of the elements 2013 (IUPAC Technical Report)"
* Published in Pure and Applied Chemistry, Volume 88, Issue 3, https://doi.org/10.1515/pac-2015-0305

Average mass values and/or uncertainties for 14 elements were further revised in 2018
as described in [Standard Atomic Weights of 14 Chemical Elements Revised](https://www.degruyter.com/document/doi/10.1515/ci-2018-0409/html)
and published in Chemistry International, Volume 40, Issue 4, https://doi.org/10.1515/ci-2018-0409

For radioactive elements, the mass of the most stable isotope is stored for the isotopic mass.

Naturally occurring radioactive elements have an average weight and associated uncertainty.
For the other radioactive elements, the mass of the most stable isotope is used, rounded to one decimal place.
When an average mass uncertainty is not available, a value of 0.0005 is used.

## COM Add-in

MolecularWeightCalculator.dll can be added as a COM Interop reference in Excel using VBA. Steps required:

* If you previously installed the VB.NET version of MolecularWeightCalculator.dll, unregister it
  * Start an elevated command prompt by typing "Command Prompt" in the start menu, then right click the Command Prompt item and choose "Run as administrator"
  * Run these commands (ignore any errors)

```
regsvr32 /u MwtWindll.dll
C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe /u MolecularWeightCalculator.dll
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe /u MolecularWeightCalculator.dll
C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe /u MwtWindll.dll
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe /u MwtWindll.dll
```

* Install the .NET Framework 4.7.2 runtime
  * Download from https://dotnet.microsoft.com/download/dotnet-framework/net472
* Download MolecularWeightCalcCOMInstaller.exe from the latest release on GitHub
  * https://github.com/PNNL-Comp-Mass-Spec/Molecular-Weight-Calculator-DLL/releases/
* Run MolecularWeightCalcCOMInstaller.exe
  * Files will be installed at C:\Program Files\MolecularWeightCalculatorDll
* Create a new Excel workbook
* Show the Developer toolbar
  * If hidden, right click the Ribbon (at the top of Excel) and choose "Customize the Ribbon"
  * Look for "Developer" at the right
  * Enable the checkbox for "Developer"
* Click "Visual Basic" on the Developer tab of the ribbon
* Right click the current workbook (aka current VBAProject) and choose "Insert, Module"
* On the Tools menu, choose "References"
* Click Browse, then navigate to C:\Program Files\MolecularWeightCalculatorDll
* Select file MolecularWeightCalculator64.tlb and click Open
  * Note: you select the 64-bit .tlb file and not a dll
* The Available References list will now show "Molecular Weight Calculator .NET Library"
  * Click OK to add the reference
  

Enter the following code, then on the Debug menu choose "Compile VBA Project"

```vbscript
Option Explicit

Private mMwtCalculator As New MolecularWeightTool
Private mIsoInitialized As Boolean

Public Function ComputeIsotopicMass(formula As String) As Double

On Error GoTo ErrorHandler

    If Not mIsoInitialized Then
        mMwtCalculator.SetElementMode ElementMassMode_Isotopic
        mIsoInitialized = True
    End If

    Dim mass As Double
    mass = mMwtCalculator.ComputeMass(formula)

    ComputeIsotopicMass = mass
    Exit Function

ErrorHandler:
    Debug.Print "Error: " & Err.Description
End Function
```

On the Excel worksheet, enter this in cell A1: \
`H2O`

Enter this in cell A2: \
`=ComputeMass(A1)`

When you press enter to finalize the formula, cell A2 will show \
`18.01528`

Update Cell A2 to have c6h6 and cell A2 will change to \
`78.11184`

When saving the Excel file, you must save it as a Macro-enabled workbook

### Example Excel File

File [ComputeFormulaMasses.xlsm](https://github.com/PNNL-Comp-Mass-Spec/Molecular-Weight-Calculator-DLL/blob/master/Documentation/ComputeFormulaMasses.xlsm)
at https://github.com/PNNL-Comp-Mass-Spec/Molecular-Weight-Calculator-DLL/tree/master/Documentation is an example Excel file 
demonstrating use of MolecularWeightCalculator.dll to compute formula masses.
* A screenshot of the worksheet is in [ComputeFormulaMasses.png](https://github.com/PNNL-Comp-Mass-Spec/Molecular-Weight-Calculator-DLL/blob/master/Documentation/ComputeFormulaMasses.png)
* The VBA code is visible in [ComputeFormulaMasses.vb](https://github.com/PNNL-Comp-Mass-Spec/Molecular-Weight-Calculator-DLL/blob/master/Documentation/ComputeFormulaMasses.vb)

## Contacts

Written by Matthew Monroe and Bryson Gibbons for the Department of Energy (PNNL, Richland, WA) \
Based on Molecular Weight Calculator, v6.20 code (VB6), written by Matthew Monroe 1995-2002 \
VB6 ActiveX Dll version written by Matthew Monroe in 2002 \
Ported to VB.NET by Nikša Blonder and Matthew Monroe in 2005 \
Ported to C# by Bryson Gibbons in 2021

E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov \
Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics/
Source code: https://github.com/PNNL-Comp-Mass-Spec/Molecular-Weight-Calculator

## License

Licensed under the Apache License, Version 2.0; you may not use this program except 
in compliance with the License.  You may obtain a copy of the License at 
http://www.apache.org/licenses/LICENSE-2.0
