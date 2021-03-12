Option Strict On

Public Class MoleMassDilution

    ' Molecular Weight Calculator routines with ActiveX Class interfaces: MoleMassDilution

    ' -------------------------------------------------------------------------------
    ' Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2002
    ' E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
    ' Website: https://github.com/PNNL-Comp-Mass-Spec/Molecular-Weight-Calculator-DLL and https://omics.pnl.gov/
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

    Public Sub New()
        MyBase.New()
        InitializeClass()
    End Sub

#Region "Enums"

    Public Enum acdAutoComputeDilutionModeConstants
        acdFindRequiredDilutionVolumes = 0
        acdFindRequiredTotalVolume
        acdFindFinalConcentration
        acdFindInitialConcentration
    End Enum

    Public Enum acqAutoComputeQuantityModeConstants
        acqFindAmount = 0
        acqFindVolume
        acqFindConcentration
    End Enum

    Private Const AMOUNT_UNITS_VOLUME_INDEX_START As uamUnitsAmountConstants = uamUnitsAmountConstants.uamLiters
    Private Const AMOUNT_UNITS_LIST_INDEX_MAX As uamUnitsAmountConstants = uamUnitsAmountConstants.uamPints
    Public Enum uamUnitsAmountConstants
        uamMoles = 0
        uamMillimoles
        uamMicroMoles
        uamNanoMoles
        uamPicoMoles
        uamFemtoMoles
        uamAttoMoles
        uamKilograms
        uamGrams
        uamMilligrams
        uamMicrograms
        uamPounds
        uamOunces
        uamLiters
        uamDeciLiters
        uamMilliLiters
        uamMicroLiters
        uamNanoLiters
        uamPicoLiters
        uamGallons
        uamQuarts
        uamPints
    End Enum

    Public Enum uevUnitsExtendedVolumeConstants
        uevL = 0
        uevDL
        uevML
        uevUL
        uevNL
        uevPL
        uevGallons
        uevQuarts
        uevPints
    End Enum

    Public Enum ummcUnitsMoleMassConcentrationConstants
        ummcMolar = 0
        ummcMilliMolar
        ummcMicroMolar
        ummcNanoMolar
        ummcPicoMolar
        ummcFemtoMolar
        ummcAttoMolar
        ummcMgPerDL
        ummcMgPerML
        ummcUgPerML
        ummcNgPerML
        ummcUgPerUL
        ummcNgPerUL
    End Enum


#End Region

#Region "Data classes"
    Private Class udtMoleMassQuantityType
        Public Amount As Double ' In Moles
        Public Volume As Double ' In L
        Public Concentration As Double ' In Molar
        Public SampleMass As Double ' In g
        Public SampleDensity As Double ' In g/mL
    End Class

    Private Class udtMoleMassDilutionValuesType
        Public InitialConcentration As Double ' In Molar
        Public StockSolutionVolume As Double ' In L
        Public FinalConcentration As Double ' In Molar
        Public DilutingSolventVolume As Double ' In L
        Public TotalFinalVolume As Double ' In L
    End Class

#End Region

    Private Const POUNDS_PER_KG As Single = 2.20462262
    Private Const GALLONS_PER_L As Single = 0.264172052

    Private mQuantity As New udtMoleMassQuantityType
    Private mDilutionValues As New udtMoleMassDilutionValuesType

    Private mAutoComputeDilution As Boolean ' When true, automatically compute dilution results whenever any value changes
    Private mAutoComputeDilutionMode As acdAutoComputeDilutionModeConstants ' The value to compute when mAutoComputeDilution is true

    Private mAutoComputeQuantity As Boolean ' When true, automatically compute quantities whenever any value changes
    Private mAutoComputeQuantityMode As acqAutoComputeQuantityModeConstants ' The value to compute when mAutoComputeQuantity is true

    ''' <summary>
    ''' Checks if AutoCompute Dilution is enabled
    ''' If yes, calls the appropriate function
    ''' </summary>
    Private Sub CheckAutoComputeDilution()

        If mAutoComputeDilution Then
            Select Case mAutoComputeDilutionMode
                Case acdAutoComputeDilutionModeConstants.acdFindRequiredTotalVolume : ComputeDilutionTotalVolume()
                Case acdAutoComputeDilutionModeConstants.acdFindFinalConcentration : ComputeDilutionFinalConcentration()
                Case acdAutoComputeDilutionModeConstants.acdFindInitialConcentration : ComputeDilutionInitialConcentration()
                Case Else
                    ' Includes acdFindRequiredDilutionVolumes
                    ComputeDilutionRequiredStockAndDilutingSolventVolumes()
            End Select
        End If
    End Sub

    ''' <summary>
    ''' Checks if AutoCompute Quantity is enabled
    ''' If yes, calls the appropriate function
    ''' </summary>
    Private Sub CheckAutoComputeQuantity()

        If mAutoComputeQuantity Then
            Select Case mAutoComputeQuantityMode
                Case acqAutoComputeQuantityModeConstants.acqFindVolume : ComputeQuantityVolume()
                Case acqAutoComputeQuantityModeConstants.acqFindConcentration : ComputeQuantityConcentration()
                Case Else
                    ' Includes acqFindAmount
                    ComputeQuantityAmount()
            End Select
        End If
    End Sub

    ''' <summary>
    ''' Computes the Final Concentration, storing in .FinalConcentration, and returning it
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <returns></returns>
    Public Function ComputeDilutionFinalConcentration(Optional eUnits As ummcUnitsMoleMassConcentrationConstants = ummcUnitsMoleMassConcentrationConstants.ummcMolar) As Double

        With mDilutionValues
            If Math.Abs(.TotalFinalVolume) > Single.Epsilon Then
                .FinalConcentration = .InitialConcentration * .StockSolutionVolume / .TotalFinalVolume
            Else
                .TotalFinalVolume = 0
            End If

            .DilutingSolventVolume = .TotalFinalVolume - .StockSolutionVolume
            If .DilutingSolventVolume < 0 Then .DilutingSolventVolume = -1
        End With

        Return ConvertConcentration(mDilutionValues.FinalConcentration, ummcUnitsMoleMassConcentrationConstants.ummcMolar, eUnits)

    End Function

    ''' <summary>
    ''' Computes the Initial Concentration, storing in .InitialConcentration, and returning it
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <returns></returns>
    Public Function ComputeDilutionInitialConcentration(Optional eUnits As ummcUnitsMoleMassConcentrationConstants = ummcUnitsMoleMassConcentrationConstants.ummcMolar) As Double

        With mDilutionValues
            If Math.Abs(.StockSolutionVolume) > Single.Epsilon Then
                .InitialConcentration = .FinalConcentration * .TotalFinalVolume / .StockSolutionVolume
            Else
                .InitialConcentration = 0
            End If

            .DilutingSolventVolume = .TotalFinalVolume - .StockSolutionVolume
            If .DilutingSolventVolume < 0 Then .DilutingSolventVolume = -1
        End With

        Return ConvertConcentration(mDilutionValues.InitialConcentration, ummcUnitsMoleMassConcentrationConstants.ummcMolar, eUnits)

    End Function

    ''' <summary>
    ''' Computes the required dilution volumes using initial concentration, final concentration
    ''' and total final volume, storing in .StockSolutionVolume and .DilutingSolventVolume,
    ''' and returning .StockSolutionVolume
    ''' </summary>
    ''' <param name="eStockSolutionUnits"></param>
    ''' <param name="dblNewDilutingSolventVolume">Output: diluting solvent volume</param>
    ''' <param name="eDilutingSolventUnits"></param>
    ''' <returns></returns>
    Public Function ComputeDilutionRequiredStockAndDilutingSolventVolumes(
      Optional eStockSolutionUnits As uevUnitsExtendedVolumeConstants = uevUnitsExtendedVolumeConstants.uevML,
      Optional ByRef dblNewDilutingSolventVolume As Double = 0,
      Optional eDilutingSolventUnits As uevUnitsExtendedVolumeConstants = uevUnitsExtendedVolumeConstants.uevML) As Double

        With mDilutionValues
            If Math.Abs(.InitialConcentration) > Single.Epsilon Then
                .StockSolutionVolume = .FinalConcentration * .TotalFinalVolume / .InitialConcentration
            Else
                .StockSolutionVolume = 0
            End If

            .DilutingSolventVolume = .TotalFinalVolume - .StockSolutionVolume

            If .DilutingSolventVolume < 0 Then
                .DilutingSolventVolume = -1
                .StockSolutionVolume = -1
            End If
        End With

        dblNewDilutingSolventVolume = ConvertVolumeExtended(mDilutionValues.DilutingSolventVolume, uevUnitsExtendedVolumeConstants.uevL, eDilutingSolventUnits)

        Return ConvertVolumeExtended(mDilutionValues.StockSolutionVolume, uevUnitsExtendedVolumeConstants.uevL, eStockSolutionUnits)

    End Function

    ''' <summary>
    ''' Compute the total volume following the dilution, storing in .TotalFinalVolume, and returning it
    ''' </summary>
    ''' <param name="eTotalVolumeUnits"></param>
    ''' <param name="dblNewDilutingSolventVolume">Output: diluting solvent volume</param>
    ''' <param name="eDilutingSolventUnits"></param>
    ''' <returns></returns>
    Public Function ComputeDilutionTotalVolume(
      Optional eTotalVolumeUnits As uevUnitsExtendedVolumeConstants = uevUnitsExtendedVolumeConstants.uevML,
      Optional ByRef dblNewDilutingSolventVolume As Double = 0,
      Optional eDilutingSolventUnits As uevUnitsExtendedVolumeConstants = uevUnitsExtendedVolumeConstants.uevML) As Double

        With mDilutionValues
            If .InitialConcentration > 0 And .FinalConcentration > 0 Then
                .TotalFinalVolume = .InitialConcentration * .StockSolutionVolume / .FinalConcentration
                If .TotalFinalVolume < 0 Then
                    .TotalFinalVolume = 0
                End If
            Else
                .TotalFinalVolume = 0
            End If

            .DilutingSolventVolume = .TotalFinalVolume - .StockSolutionVolume
            If .DilutingSolventVolume < 0 Then .DilutingSolventVolume = -1
        End With

        dblNewDilutingSolventVolume = ConvertVolumeExtended(mDilutionValues.DilutingSolventVolume, uevUnitsExtendedVolumeConstants.uevL, eDilutingSolventUnits)

        Return ConvertVolumeExtended(mDilutionValues.TotalFinalVolume, uevUnitsExtendedVolumeConstants.uevL, eTotalVolumeUnits)

    End Function

    ''' <summary>
    ''' Computes mQuantity.Amount using mQuantity.Volume and mQuantity.Concentration, storing the result in mQuantity.Amount
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <returns>mQuantity.Amount, with the specified units</returns>
    Public Function ComputeQuantityAmount(Optional eUnits As uamUnitsAmountConstants = uamUnitsAmountConstants.uamMoles) As Double
        mQuantity.Amount = mQuantity.Concentration * mQuantity.Volume

        Return ConvertAmount(mQuantity.Amount, uamUnitsAmountConstants.uamMoles, eUnits)
    End Function

    ''' <summary>
    ''' Computes mQuantity.Concentration using mQuantity.Amount and mQuantity.Volume, storing the result in mQuantity.Concentration
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <returns>mQuantity.Concentration, with the specified units</returns>
    Public Function ComputeQuantityConcentration(Optional eUnits As ummcUnitsMoleMassConcentrationConstants = ummcUnitsMoleMassConcentrationConstants.ummcMolar) As Double
        With mQuantity
            If Math.Abs(.Volume) > Single.Epsilon Then
                .Concentration = .Amount / .Volume
            Else
                .Concentration = 0
            End If
        End With

        Return ConvertConcentration(mQuantity.Concentration, ummcUnitsMoleMassConcentrationConstants.ummcMolar, eUnits)
    End Function

    ''' <summary>
    ''' Computes mQuantity.Volume using mQuantity.Amount and mQuantity.Concentration, storing the result in mQuantity.Volume
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <returns>mQuantity.Volume, with the specified units</returns>
    Public Function ComputeQuantityVolume(Optional eUnits As uevUnitsExtendedVolumeConstants = uevUnitsExtendedVolumeConstants.uevL) As Double
        With mQuantity
            If Math.Abs(.Concentration) > Single.Epsilon Then
                .Volume = .Amount / .Concentration
            Else
                .Volume = 0
            End If
        End With

        Return ConvertVolumeExtended(mQuantity.Volume, uevUnitsExtendedVolumeConstants.uevL, eUnits)
    End Function

    ''' <summary>
    ''' This function uses .SampleMass and .SampleDensity if the units are mass and/or volume-based
    ''' </summary>
    ''' <param name="dblAmountIn"></param>
    ''' <param name="eCurrentUnits"></param>
    ''' <param name="eNewUnits"></param>
    ''' <returns></returns>
    Public Function ConvertAmount(dblAmountIn As Double, eCurrentUnits As uamUnitsAmountConstants, eNewUnits As uamUnitsAmountConstants) As Double
        Dim dblValue, dblFactor As Double
        Dim dblSampleMass, dblSampleDensity As Double
        Dim eCurrentVolumeUnits As uevUnitsExtendedVolumeConstants
        Dim eNewVolumeUnits As uevUnitsExtendedVolumeConstants

        If eCurrentUnits = eNewUnits Then
            ' No conversion, simply return dblAmountIn
            Return dblAmountIn
        End If

        If eCurrentUnits >= AMOUNT_UNITS_VOLUME_INDEX_START And eCurrentUnits <= AMOUNT_UNITS_LIST_INDEX_MAX And eNewUnits >= AMOUNT_UNITS_VOLUME_INDEX_START And eNewUnits <= AMOUNT_UNITS_LIST_INDEX_MAX Then
            ' Converting from one volume unit to another volume unit
            ' No need to explicitly specify mass or density

            eCurrentVolumeUnits = CType(eCurrentUnits - AMOUNT_UNITS_VOLUME_INDEX_START, uevUnitsExtendedVolumeConstants)
            eNewVolumeUnits = CType(eNewUnits - AMOUNT_UNITS_VOLUME_INDEX_START, uevUnitsExtendedVolumeConstants)

            Return ConvertVolumeExtended(dblAmountIn, eCurrentVolumeUnits, eNewVolumeUnits)
        Else

            dblSampleMass = mQuantity.SampleMass
            dblSampleDensity = mQuantity.SampleDensity

            dblFactor = FactorAmount(eCurrentUnits, dblSampleMass, dblSampleDensity)
            If dblFactor < 0 Then
                Return -1
            Else
                dblValue = dblAmountIn * dblFactor
            End If

            dblFactor = FactorAmount(eNewUnits, dblSampleMass, dblSampleDensity)
            If dblFactor <= 0 Then
                Return -1
            Else
                Return dblValue / dblFactor
            End If
        End If

    End Function

    ''' <summary>
    ''' Convert concentration
    ''' </summary>
    ''' <param name="dblConcentrationIn"></param>
    ''' <param name="eCurrentUnits"></param>
    ''' <param name="eNewUnits"></param>
    ''' <returns></returns>
    ''' <remarks>Duplicated function, in both CapillaryFlow and MoleMassDilution</remarks>
    Public Function ConvertConcentration(dblConcentrationIn As Double, eCurrentUnits As ummcUnitsMoleMassConcentrationConstants, eNewUnits As ummcUnitsMoleMassConcentrationConstants) As Double
        Dim dblValue, dblFactor As Double
        Dim dblSampleMass As Double

        If eCurrentUnits = eNewUnits Then
            Return dblConcentrationIn
        End If

        dblSampleMass = mQuantity.SampleMass

        dblFactor = FactorConcentration(eCurrentUnits, dblSampleMass)
        If dblFactor < 0 Then
            Return -1
        Else
            dblValue = dblConcentrationIn * dblFactor
        End If

        dblFactor = FactorConcentration(eNewUnits, dblSampleMass)
        If dblFactor <= 0 Then
            Return -1
        Else
            Return dblValue / dblFactor
        End If

    End Function

    Public Function ConvertVolumeExtended(dblVolume As Double, eCurrentUnits As uevUnitsExtendedVolumeConstants, eNewUnits As uevUnitsExtendedVolumeConstants) As Double
        Dim dblValue, dblFactor As Double

        If eCurrentUnits = eNewUnits Then
            Return dblVolume
        End If

        dblFactor = FactorVolumeExtended(eCurrentUnits)
        If dblFactor < 0 Then
            Return -1
        Else
            dblValue = dblVolume * dblFactor
        End If

        dblFactor = FactorVolumeExtended(eNewUnits)
        If dblFactor <= 0 Then
            Return -1
        Else
            Return dblValue / dblFactor
        End If

    End Function

    ''' <summary>
    ''' Multiplication factor for converting from eUnits to Moles
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <param name="dblSampleMass">required for mass-based units</param>
    ''' <param name="dblSampleDensity">required for volume-based units</param>
    ''' <returns></returns>
    Private Function FactorAmount(
      eUnits As uamUnitsAmountConstants,
      Optional dblSampleMass As Double = -1,
      Optional dblSampleDensity As Double = 0) As Double

        Dim dblFactor As Double

        If Math.Abs(dblSampleMass) < Single.Epsilon Then
            dblFactor = -1
        Else
            ' Determine the Amount multiplication dblFactor
            Select Case eUnits
                Case uamUnitsAmountConstants.uamMoles : dblFactor = 1
                Case uamUnitsAmountConstants.uamMillimoles : dblFactor = 1 / 1000.0#
                Case uamUnitsAmountConstants.uamMicroMoles : dblFactor = 1 / 1000000.0#
                Case uamUnitsAmountConstants.uamNanoMoles : dblFactor = 1 / 1000000000.0#
                Case uamUnitsAmountConstants.uamPicoMoles : dblFactor = 1 / 1000000000000.0#
                Case uamUnitsAmountConstants.uamFemtoMoles : dblFactor = 1 / 1.0E+15
                Case uamUnitsAmountConstants.uamAttoMoles : dblFactor = 1 / 1.0E+18
                Case uamUnitsAmountConstants.uamKilograms : dblFactor = 1000.0# / dblSampleMass
                Case uamUnitsAmountConstants.uamGrams : dblFactor = 1 / dblSampleMass
                Case uamUnitsAmountConstants.uamMilligrams : dblFactor = 1 / (dblSampleMass * 1000.0#)
                Case uamUnitsAmountConstants.uamMicrograms : dblFactor = 1 / (dblSampleMass * 1000000.0#)
                Case uamUnitsAmountConstants.uamPounds : dblFactor = 1000.0# / (dblSampleMass * POUNDS_PER_KG)
                Case uamUnitsAmountConstants.uamOunces : dblFactor = 1000.0# / (dblSampleMass * POUNDS_PER_KG * 16)
                Case uamUnitsAmountConstants.uamLiters : dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(uevUnitsExtendedVolumeConstants.uevL)
                Case uamUnitsAmountConstants.uamDeciLiters : dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(uevUnitsExtendedVolumeConstants.uevDL)
                Case uamUnitsAmountConstants.uamMilliLiters : dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(uevUnitsExtendedVolumeConstants.uevML)
                Case uamUnitsAmountConstants.uamMicroLiters : dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(uevUnitsExtendedVolumeConstants.uevUL)
                Case uamUnitsAmountConstants.uamNanoLiters : dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(uevUnitsExtendedVolumeConstants.uevNL)
                Case uamUnitsAmountConstants.uamPicoLiters : dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(uevUnitsExtendedVolumeConstants.uevPL)
                Case uamUnitsAmountConstants.uamGallons : dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(uevUnitsExtendedVolumeConstants.uevGallons)
                Case uamUnitsAmountConstants.uamQuarts : dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(uevUnitsExtendedVolumeConstants.uevQuarts)
                Case uamUnitsAmountConstants.uamPints : dblFactor = dblSampleDensity / dblSampleMass * FactorVolumeExtended(uevUnitsExtendedVolumeConstants.uevPints)
                Case Else : dblFactor = -1
            End Select
        End If

        Return dblFactor

    End Function

    ''' <summary>
    ''' Multiplication factor for converting from eUnits to M
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <param name="dblSampleMass">required for mass-based units</param>
    ''' <returns></returns>
    ''' <remarks>Duplicated function, in both CapillaryFlow and MoleMassDilution</remarks>
    Private Function FactorConcentration(eUnits As ummcUnitsMoleMassConcentrationConstants, Optional dblSampleMass As Double = 0) As Double
        Dim dblFactor As Double

        If Math.Abs(dblSampleMass) < Single.Epsilon Then
            dblFactor = -1
        Else
            Select Case eUnits
                Case ummcUnitsMoleMassConcentrationConstants.ummcMolar : dblFactor = 1.0#
                Case ummcUnitsMoleMassConcentrationConstants.ummcMilliMolar : dblFactor = 1 / 1000.0#
                Case ummcUnitsMoleMassConcentrationConstants.ummcMicroMolar : dblFactor = 1 / 1000000.0#
                Case ummcUnitsMoleMassConcentrationConstants.ummcNanoMolar : dblFactor = 1 / 1000000000.0#
                Case ummcUnitsMoleMassConcentrationConstants.ummcPicoMolar : dblFactor = 1 / 1000000000000.0#
                Case ummcUnitsMoleMassConcentrationConstants.ummcFemtoMolar : dblFactor = 1 / 1.0E+15
                Case ummcUnitsMoleMassConcentrationConstants.ummcAttoMolar : dblFactor = 1 / 1.0E+18
                Case ummcUnitsMoleMassConcentrationConstants.ummcMgPerDL : dblFactor = 1 / dblSampleMass / 100.0# '1/[(1 g / 1000 mg) * (1 / MW) * (10 dL/L)]
                Case ummcUnitsMoleMassConcentrationConstants.ummcMgPerML : dblFactor = 1 / dblSampleMass '1/[(1 g / 1000 mg) * (1 / MW) * (1000 mL/L)]
                Case ummcUnitsMoleMassConcentrationConstants.ummcUgPerML : dblFactor = 1 / (dblSampleMass * 1000.0#) '1/[(1 g / 1000000 ug) * (1 / MW) * (1000 mL/L)]
                Case ummcUnitsMoleMassConcentrationConstants.ummcNgPerML : dblFactor = 1 / (dblSampleMass * 1000000.0#) '1/[(1 g / 1000000000 ng) * (1 / MW) * (1000 mL/L)]
                Case ummcUnitsMoleMassConcentrationConstants.ummcUgPerUL : dblFactor = 1 / (dblSampleMass) '1/[(1 g / 1000000 ug) * (1 / MW) * (1000000 uL/L)]
                Case ummcUnitsMoleMassConcentrationConstants.ummcNgPerUL : dblFactor = 1 / (dblSampleMass * 1000.0#) '1/[(1 g / 1000000000 ng) * (1 / MW) * (1000000 uL/L)]
                Case Else : dblFactor = -1
            End Select
        End If

        Return dblFactor
    End Function

    ''' <summary>
    ''' Multiplication factor for converting from eUnits to mL
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <returns></returns>
    ''' <remarks>An extended version of the FactorVolume function in CapillaryFlow</remarks>
    Private Function FactorVolumeExtended(eUnits As uevUnitsExtendedVolumeConstants) As Double
        Dim dblFactor As Double

        ' Note: 4 quarts per gallon, 2 pints per quart
        Select Case eUnits
            Case uevUnitsExtendedVolumeConstants.uevL : dblFactor = 1 * 1000.0#
            Case uevUnitsExtendedVolumeConstants.uevDL : dblFactor = 1 * 100.0#
            Case uevUnitsExtendedVolumeConstants.uevML : dblFactor = 1.0#
            Case uevUnitsExtendedVolumeConstants.uevUL : dblFactor = 1 / 1000.0#
            Case uevUnitsExtendedVolumeConstants.uevNL : dblFactor = 1 / 1000000.0#
            Case uevUnitsExtendedVolumeConstants.uevPL : dblFactor = 1 / 1000000000.0#
            Case uevUnitsExtendedVolumeConstants.uevGallons : dblFactor = 1000.0# / GALLONS_PER_L
            Case uevUnitsExtendedVolumeConstants.uevQuarts : dblFactor = 1000.0# / GALLONS_PER_L / 4.0#
            Case uevUnitsExtendedVolumeConstants.uevPints : dblFactor = 1000.0# / GALLONS_PER_L / 8.0#
            Case Else : dblFactor = -1
        End Select

        Return dblFactor
    End Function


    ' Get Methods
    ' These retrieve the most recently computed value
    ' If mAutoComputeDilution = False, must manually call a Compute Sub to recompute the value
    ' Similarly, if mAutoComputeQuantity = False, must manually call a Compute Sub to recompute the value

    Public Function GetAutoComputeDilutionEnabled() As Boolean
        Return mAutoComputeDilution
    End Function

    Public Function GetAutoComputeDilutionMode() As acdAutoComputeDilutionModeConstants
        Return mAutoComputeDilutionMode
    End Function

    Public Function GetAutoComputeQuantityEnabled() As Boolean
        Return mAutoComputeQuantity
    End Function

    Public Function GetAutoComputeQuantityMode() As acqAutoComputeQuantityModeConstants
        Return mAutoComputeQuantityMode
    End Function

    Public Function GetDilutionFinalConcentration(Optional eUnits As ummcUnitsMoleMassConcentrationConstants = ummcUnitsMoleMassConcentrationConstants.ummcMolar) As Double
        Return ConvertConcentration(mDilutionValues.FinalConcentration, ummcUnitsMoleMassConcentrationConstants.ummcMolar, eUnits)
    End Function

    Public Function GetDilutionInitialConcentration(Optional eUnits As ummcUnitsMoleMassConcentrationConstants = ummcUnitsMoleMassConcentrationConstants.ummcMolar) As Double
        Return ConvertConcentration(mDilutionValues.InitialConcentration, ummcUnitsMoleMassConcentrationConstants.ummcMolar, eUnits)
    End Function

    Public Function GetDilutionTotalFinalVolume(Optional eUnits As uevUnitsExtendedVolumeConstants = uevUnitsExtendedVolumeConstants.uevML) As Double
        Return ConvertVolumeExtended(mDilutionValues.TotalFinalVolume, uevUnitsExtendedVolumeConstants.uevL, eUnits)
    End Function

    Public Function GetDilutionVolumeDilutingSolvent(Optional eUnits As uevUnitsExtendedVolumeConstants = uevUnitsExtendedVolumeConstants.uevML) As Double
        Return ConvertVolumeExtended(mDilutionValues.DilutingSolventVolume, uevUnitsExtendedVolumeConstants.uevL, eUnits)
    End Function

    Public Function GetDilutionVolumeStockSolution(Optional eUnits As uevUnitsExtendedVolumeConstants = uevUnitsExtendedVolumeConstants.uevML) As Double
        Return ConvertVolumeExtended(mDilutionValues.StockSolutionVolume, uevUnitsExtendedVolumeConstants.uevL, eUnits)
    End Function


    Public Function GetQuantityAmount(Optional eUnits As uamUnitsAmountConstants = uamUnitsAmountConstants.uamMoles) As Double
        Return ConvertAmount(mQuantity.Amount, uamUnitsAmountConstants.uamMoles, eUnits)
    End Function

    Public Function GetQuantityConcentration(Optional eUnits As ummcUnitsMoleMassConcentrationConstants = ummcUnitsMoleMassConcentrationConstants.ummcMolar) As Double
        Return ConvertConcentration(mQuantity.Concentration, ummcUnitsMoleMassConcentrationConstants.ummcMolar, eUnits)
    End Function

    Public Function GetQuantityVolume(Optional eUnits As uevUnitsExtendedVolumeConstants = uevUnitsExtendedVolumeConstants.uevML) As Double
        Return ConvertVolumeExtended(mQuantity.Volume, uevUnitsExtendedVolumeConstants.uevL, eUnits)
    End Function

    Public Function GetSampleDensity() As Double
        Return mQuantity.SampleDensity
    End Function

    Public Function GetSampleMass() As Double
        Return mQuantity.SampleMass
    End Function


    ' Set Methods
    ' If mAutoComputeDilution = False, must manually call a Compute Sub to recompute the value
    ' Similarly, if mAutoComputeQuantity = False, must manually call a Compute Sub to recompute the value

    Public Sub SetAutoComputeDilutionEnabled(blnAutoCompute As Boolean)
        mAutoComputeDilution = blnAutoCompute
    End Sub

    Public Sub SetAutoComputeDilutionMode(eAutoComputeMode As acdAutoComputeDilutionModeConstants)
        If eAutoComputeMode >= acdAutoComputeDilutionModeConstants.acdFindRequiredDilutionVolumes And eAutoComputeMode <= acdAutoComputeDilutionModeConstants.acdFindFinalConcentration Then
            mAutoComputeDilutionMode = eAutoComputeMode
        End If
    End Sub

    Public Sub SetAutoComputeQuantityEnabled(blnAutoCompute As Boolean)
        mAutoComputeQuantity = blnAutoCompute
    End Sub

    Public Sub SetAutoComputeQuantityMode(eAutoComputeMode As acqAutoComputeQuantityModeConstants)
        If eAutoComputeMode >= acqAutoComputeQuantityModeConstants.acqFindAmount And eAutoComputeMode <= acqAutoComputeQuantityModeConstants.acqFindConcentration Then
            mAutoComputeQuantityMode = eAutoComputeMode
        End If
    End Sub


    Public Sub SetDilutionFinalConcentration(dblConcentration As Double, Optional eUnits As ummcUnitsMoleMassConcentrationConstants = ummcUnitsMoleMassConcentrationConstants.ummcMolar)
        mDilutionValues.FinalConcentration = ConvertConcentration(dblConcentration, eUnits, ummcUnitsMoleMassConcentrationConstants.ummcMolar)
        CheckAutoComputeDilution()
    End Sub

    Public Sub SetDilutionInitialConcentration(dblConcentration As Double, Optional eUnits As ummcUnitsMoleMassConcentrationConstants = ummcUnitsMoleMassConcentrationConstants.ummcMolar)
        mDilutionValues.InitialConcentration = ConvertConcentration(dblConcentration, eUnits, ummcUnitsMoleMassConcentrationConstants.ummcMolar)
        CheckAutoComputeDilution()
    End Sub

    Public Sub SetDilutionTotalFinalVolume(dblVolume As Double, Optional eUnits As uevUnitsExtendedVolumeConstants = uevUnitsExtendedVolumeConstants.uevML)
        mDilutionValues.TotalFinalVolume = ConvertVolumeExtended(dblVolume, eUnits, uevUnitsExtendedVolumeConstants.uevL)
        CheckAutoComputeDilution()
    End Sub

    Public Sub SetDilutionVolumeDilutingSolvent(dblVolume As Double, Optional eUnits As uevUnitsExtendedVolumeConstants = uevUnitsExtendedVolumeConstants.uevML)
        mDilutionValues.DilutingSolventVolume = ConvertVolumeExtended(dblVolume, eUnits, uevUnitsExtendedVolumeConstants.uevL)
        CheckAutoComputeDilution()
    End Sub

    Public Sub SetDilutionVolumeStockSolution(dblVolume As Double, Optional eUnits As uevUnitsExtendedVolumeConstants = uevUnitsExtendedVolumeConstants.uevML)
        mDilutionValues.StockSolutionVolume = ConvertVolumeExtended(dblVolume, eUnits, uevUnitsExtendedVolumeConstants.uevL)
        CheckAutoComputeDilution()
    End Sub

    Public Sub SetQuantityAmount(dblAmount As Double, Optional eUnits As uamUnitsAmountConstants = uamUnitsAmountConstants.uamMoles)
        mQuantity.Amount = ConvertAmount(dblAmount, eUnits, uamUnitsAmountConstants.uamMoles)
        CheckAutoComputeQuantity()
    End Sub

    Public Sub SetQuantityConcentration(dblConcentration As Double, Optional eUnits As ummcUnitsMoleMassConcentrationConstants = ummcUnitsMoleMassConcentrationConstants.ummcMolar)
        mQuantity.Concentration = ConvertConcentration(dblConcentration, eUnits, ummcUnitsMoleMassConcentrationConstants.ummcMolar)
        CheckAutoComputeQuantity()
    End Sub

    Public Sub SetQuantityVolume(dblVolume As Double, Optional eUnits As uevUnitsExtendedVolumeConstants = uevUnitsExtendedVolumeConstants.uevML)
        mQuantity.Volume = ConvertVolumeExtended(dblVolume, eUnits, uevUnitsExtendedVolumeConstants.uevL)
    End Sub

    Public Sub SetSampleDensity(dblDensityInGramsPerML As Double)
        If dblDensityInGramsPerML >= 0 Then
            mQuantity.SampleDensity = dblDensityInGramsPerML
        Else
            mQuantity.SampleDensity = 0
        End If
        CheckAutoComputeQuantity()
    End Sub

    Public Sub SetSampleMass(dblMassInGramsPerMole As Double)
        If dblMassInGramsPerMole >= 0 Then
            mQuantity.SampleMass = dblMassInGramsPerMole
        Else
            mQuantity.SampleMass = 0
        End If
        CheckAutoComputeQuantity()
    End Sub


    Private Sub InitializeClass()
        Me.SetAutoComputeDilutionEnabled(False)
        Me.SetAutoComputeQuantityEnabled(False)

        Me.SetAutoComputeDilutionMode(acdAutoComputeDilutionModeConstants.acdFindRequiredDilutionVolumes)

        Me.SetAutoComputeQuantityMode(acqAutoComputeQuantityModeConstants.acqFindConcentration)

        Me.SetQuantityAmount(1, uamUnitsAmountConstants.uamMoles)
        Me.SetQuantityVolume(100, uevUnitsExtendedVolumeConstants.uevML)
        Me.SetQuantityConcentration(1, ummcUnitsMoleMassConcentrationConstants.ummcMolar)

        Me.SetDilutionInitialConcentration(10, ummcUnitsMoleMassConcentrationConstants.ummcMolar)
        Me.SetDilutionVolumeStockSolution(3, uevUnitsExtendedVolumeConstants.uevML)
        Me.SetDilutionFinalConcentration(2, ummcUnitsMoleMassConcentrationConstants.ummcMolar)
        Me.SetDilutionVolumeDilutingSolvent(12, uevUnitsExtendedVolumeConstants.uevML)
        Me.SetDilutionTotalFinalVolume(15, uevUnitsExtendedVolumeConstants.uevML)

        ' Recompute
        Me.ComputeQuantityAmount()
        Me.ComputeDilutionRequiredStockAndDilutingSolventVolumes()

        Me.SetAutoComputeDilutionEnabled(True)
        Me.SetAutoComputeQuantityEnabled(True)

    End Sub

    Public ReadOnly Property AmountsUnitListCount() As Short
        Get
            Return CType(AMOUNT_UNITS_LIST_INDEX_MAX + 1, Short)
        End Get
    End Property

    Public ReadOnly Property AmountsUnitListVolumeIndexStart() As Short
        Get
            Return CType(AMOUNT_UNITS_VOLUME_INDEX_START, Short)
        End Get
    End Property

    Public ReadOnly Property AmountsUnitListVolumeIndexEnd() As Short
        Get
            Return CType(AMOUNT_UNITS_LIST_INDEX_MAX, Integer)
        End Get
    End Property

End Class