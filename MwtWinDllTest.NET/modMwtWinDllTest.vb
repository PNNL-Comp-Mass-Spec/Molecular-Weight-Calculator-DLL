Option Strict On
Option Explicit On

Imports System.Collections.Generic
Imports MwtWinDll

Module modMwtWinDllTest

    ' Molecular Weight Calculator Dll test program

    ' -------------------------------------------------------------------------------
    ' Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
    ' E-mail: matthew.monroe@pnnl.gov or matt@alchemistmatt.com
    ' Website: http://ncrr.pnnl.gov/ or http://www.sysbio.org/resources/staff/
    ' -------------------------------------------------------------------------------
    ' 
    ' Licensed under the Apache License, Version 2.0; you may not use this file except
    ' in compliance with the License.  You may obtain a copy of the License at 
    ' http://www.apache.org/licenses/LICENSE-2.0
    '
    ' Notice: This computer software was prepared by Battelle Memorial Institute, 
    ' hereinafter the Contractor, under Contract No. DE-AC05-76RL0 1830 with the 
    ' Department of Energy (DOE).  All rights in the computer software are reserved 
    ' by DOE on behalf of the United States Government and the Contractor as 
    ' provided in the Contract.  NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAKES ANY 
    ' WARRANTY, EXPRESS OR IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THIS 
    ' SOFTWARE.  This notice including this sentence must appear on any copies of 
    ' this computer software.

    Public Declare Function GetTickCount Lib "kernel32" () As Integer

    Public Sub Main()
        'Dim objMwtWinDllTest As New frmMwtWinDllTest
        'objMwtWinDllTest.ShowDialog()

        Dim oMwtWin = New MolecularWeightCalculator()
        oMwtWin.SetElementMode(MWElementAndMassRoutines.emElementModeConstants.emIsotopicMass)

        Dim searchOptions = New clsFormulaFinderOptions()

        oMwtWin.FormulaFinder.CandidateElements.Clear()

        oMwtWin.FormulaFinder.AddCandidateElement("C", 70)
        oMwtWin.FormulaFinder.AddCandidateElement("H", 10)
        oMwtWin.FormulaFinder.AddCandidateElement("N", 10)
        oMwtWin.FormulaFinder.AddCandidateElement("O", 10)
        'oMwtWin.FormulaFinder.AddCandidateElement("Ser", 10)

        searchOptions.LimitChargeRange = False
        searchOptions.ChargeMin = 1
        searchOptions.ChargeMax = 1
        searchOptions.FindTargetMZ = False

        ' Dim lstResults = oMwtWin.FormulaFinder.FindMatchesByMass(200, 0.05, searchOptions)
        'Dim lstResults = oMwtWin.FormulaFinder.FindMatchesByMassPPM(200, 250, searchOptions)

        Dim percentCompSearch = True
        Dim lstResults = oMwtWin.FormulaFinder.FindMatchesByPercentComposition(400, 1, searchOptions)

        ' Dim lstResults = TestBoundedsearch(oMwtWin)

        Console.WriteLine("Result count: " & lstResults.Count)
        Console.WriteLine()

        For Each result In lstResults
            Console.Write(result.ToString() & ", charge " & result.ChargeState)
            If searchOptions.FindCharge Then
                Console.Write(", " & result.MZ.ToString("0.000") & " m/z")
            End If
            Console.WriteLine()

            If percentCompSearch Then
                Console.Write(" has")
                For Each percentCompValue In result.PercentComposition
                    Console.Write(" " & percentCompValue.Key & "=" & percentCompValue.Value.ToString("0.00") & "%")
                Next

                Console.WriteLine()
            End If
        Next

    End Sub

    Private Function TestBoundedsearch(oMwtWin As MolecularWeightCalculator) As List(Of clsFormulaFinderResult)

        Dim searchOptions = New clsFormulaFinderOptions()

        searchOptions.SearchMode = clsFormulaFinderOptions.eSearchMode.Bounded
        searchOptions.FindCharge = True

        oMwtWin.FormulaFinder.CandidateElements.Clear()

        oMwtWin.FormulaFinder.AddCandidateElement("C", 0, 39)
        oMwtWin.FormulaFinder.AddCandidateElement("H", 0, 224)
        oMwtWin.FormulaFinder.AddCandidateElement("N", 0, 33)
        oMwtWin.FormulaFinder.AddCandidateElement("O", 0, 29)
        oMwtWin.FormulaFinder.AddCandidateElement("Ser", 0, 6)

        Dim lstResults = oMwtWin.FormulaFinder.FindMatchesByMassPPM(200, 250, searchOptions)

        Return lstResults

    End Function
End Module