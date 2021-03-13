Option Strict On

Imports System.Runtime.InteropServices

Public Class CapillaryFlow

    ' Molecular Weight Calculator routines with ActiveX Class interfaces: CapillaryFlow

    ' -------------------------------------------------------------------------------
    ' Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
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

#Region "Enum Statements"

    Public Enum ctCapillaryTypeConstants
        ctOpenTubularCapillary = 0
        ctPackedCapillary
    End Enum

    Public Enum uprUnitsPressureConstants
        uprPsi = 0
        uprPascals
        uprKiloPascals
        uprAtmospheres
        uprBar
        uprTorr
        uprDynesPerSquareCm
    End Enum

    Public Enum ulnUnitsLengthConstants
        ulnM = 0
        ulnCM
        ulnMM
        ulnMicrons
        ulnInches
    End Enum

    Public Enum uviUnitsViscosityConstants
        uviPoise = 0
        uviCentiPoise
    End Enum

    Public Enum ufrUnitsFlowRateConstants
        ufrMLPerMin = 0
        ufrULPerMin
        ufrNLPerMin
    End Enum

    Public Enum ulvUnitsLinearVelocityConstants
        ulvCmPerHr = 0
        ulvMmPerHr
        ulvCmPerMin
        ulvMmPerMin
        ulvCmPerSec
        ulvMmPerSec
    End Enum

    Public Enum utmUnitsTimeConstants
        utmHours = 0
        utmMinutes
        utmSeconds
    End Enum

    Public Enum uvoUnitsVolumeConstants
        uvoML = 0
        uvoUL
        uvoNL
        uvoPL
    End Enum

    Public Enum ucoUnitsConcentrationConstants
        ucoMolar = 0
        ucoMilliMolar
        ucoMicroMolar
        ucoNanoMolar
        ucoPicoMolar
        ucoFemtoMolar
        ucoAttoMolar
        ucoMgPerML
        ucoUgPerML
        ucoNgPerML
        ucoUgPerUL
        ucoNgPerUL
    End Enum

    Public Enum utpUnitsTemperatureConstants
        utpCelsius = 0
        utpKelvin
        utpFahrenheit
    End Enum

    Public Enum umfMassFlowRateConstants
        umfPmolPerMin = 0
        umfFmolPerMin
        umfAmolPerMin
        umfPmolPerSec
        umfFmolPerSec
        umfAmolPerSec
        umfMolesPerMin
    End Enum

    Public Enum umaMolarAmountConstants
        umaMoles = 0
        umaMilliMoles
        umaMicroMoles
        umaNanoMoles
        umaPicoMoles
        umaFemtoMoles
        umaAttoMoles
    End Enum

    Public Enum udcDiffusionCoefficientConstants
        udcCmSquaredPerHr = 0
        udcCmSquaredPerMin
        udcCmSquaredPerSec
    End Enum

    Public Enum acmAutoComputeModeConstants
        acmBackPressure = 0
        acmColumnID
        acmColumnLength
        acmDeadTime
        acmLinearVelocity
        acmVolFlowRate
        acmVolFlowRateUsingDeadTime
    End Enum

#End Region

#Region "Data classes"

    Private Class udtCapillaryFlowParametersType
        Public CapillaryType As ctCapillaryTypeConstants

        ''' <summary>
        ''' Units: dynes/cm^2
        ''' </summary>
        Public BackPressure As Double

        ''' <summary>
        ''' Units: cm
        ''' </summary>
        Public ColumnLength As Double

        ''' <summary>
        ''' Units: cm
        ''' </summary>
        Public ColumnID As Double

        ''' <summary>
        ''' Units: poise
        ''' </summary>
        Public SolventViscosity As Double

        ''' <summary>
        ''' Units: cm
        ''' </summary>
        Public ParticleDiameter As Double

        ''' <summary>
        ''' Units: mL/min
        ''' </summary>
        Public VolumetricFlowRate As Double

        ''' <summary>
        ''' Units: cm/min
        ''' </summary>
        Public LinearVelocity As Double

        ''' <summary>
        ''' Units: min
        ''' </summary>
        Public ColumnDeadTime As Double

        Public InterparticlePorosity As Double
    End Class

    Private Class udtMassRateParametersType
        ''' <summary>
        ''' Units: Molar
        ''' </summary>
        Public SampleConcentration As Double

        ''' <summary>
        ''' Units: g/mole
        ''' </summary>
        Public SampleMass As Double

        ''' <summary>
        ''' Units: mL/min
        ''' </summary>
        Public VolumetricFlowRate As Double

        ''' <summary>
        ''' Units: min
        ''' </summary>
        Public InjectionTime As Double

        ''' <summary>
        ''' Units: Moles/min
        ''' </summary>
        Public MassFlowRate As Double

        ''' <summary>
        ''' Units: moles
        ''' </summary>
        Public MolesInjected As Double
    End Class

    Private Class udtExtraColumnBroadeningParametersType

        ''' <summary>
        ''' Units: cm/min
        ''' </summary>
        Public LinearVelocity As Double

        ''' <summary>
        ''' Units: cm^2/sec
        ''' </summary>
        Public DiffusionCoefficient As Double

        ''' <summary>
        ''' Units: cm
        ''' </summary>
        Public OpenTubeLength As Double

        ''' <summary>
        ''' Units: cm
        ''' </summary>
        Public OpenTubeID As Double

        ''' <summary>
        ''' Units: sec
        ''' </summary>
        Public InitialPeakWidth As Double

        ''' <summary>
        ''' Units: sec^2
        ''' </summary>
        Public TemporalVariance As Double

        ''' <summary>
        ''' Units: sec^2
        ''' </summary>
        Public AdditionalTemporalVariance As Double

        ''' <summary>
        ''' Units: sec
        ''' </summary>
        Public ResultantPeakWidth As Double
    End Class

#End Region


    ' Conversion Factors
    Private Const CM_PER_INCH As Single = 2.54
    Private Const PI As Double = 3.14159265359

    Private mCapillaryFlowParameters As New udtCapillaryFlowParametersType
    Private mMassRateParameters As New udtMassRateParametersType
    Private mExtraColumnBroadeningParameters As New udtExtraColumnBroadeningParametersType

    ''' <summary>
    ''' When true, automatically compute results whenever any value changes
    ''' </summary>
    Private mAutoCompute As Boolean

    ''' <summary>
    ''' The value to compute when mAutoCompute is true
    ''' </summary>
    Private mAutoComputeMode As acmAutoComputeModeConstants

    Private Sub CheckAutoCompute()
        If mAutoCompute Then
            Select Case mAutoComputeMode
                Case acmAutoComputeModeConstants.acmBackPressure : ComputeBackPressure()
                Case acmAutoComputeModeConstants.acmColumnID : ComputeColumnID()
                Case acmAutoComputeModeConstants.acmColumnLength : ComputeColumnLength()
                Case acmAutoComputeModeConstants.acmDeadTime : ComputeDeadTime()
                Case acmAutoComputeModeConstants.acmLinearVelocity : ComputeLinearVelocity()
                Case acmAutoComputeModeConstants.acmVolFlowRateUsingDeadTime : ComputeVolFlowRateUsingDeadTime()
                Case Else
                    ' Includes acmVolFlowRate
                    ComputeVolFlowRate()
            End Select
        End If
    End Sub

    ''' <summary>
    ''' Computes the back pressure, stores in .BackPressure, and returns it
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <returns></returns>
    Public Function ComputeBackPressure(Optional eUnits As uprUnitsPressureConstants = uprUnitsPressureConstants.uprPsi) As Double

        Dim dblBackPressure, dblRadius As Double

        With mCapillaryFlowParameters

            dblRadius = .ColumnID / 2.0#

            If Math.Abs(dblRadius) > Single.Epsilon Then
                If .CapillaryType = ctCapillaryTypeConstants.ctOpenTubularCapillary Then
                    ' Open tubular capillary
                    dblBackPressure = (.VolumetricFlowRate * 8 * .SolventViscosity * .ColumnLength) / (dblRadius ^ 4 * PI * 60) ' Pressure in dynes/cm^2
                Else
                    ' Packed capillary
                    If Math.Abs(.ParticleDiameter) > Single.Epsilon And Math.Abs(.InterparticlePorosity) > Single.Epsilon Then
                        ' Flow rate in mL/sec
                        dblBackPressure = (.VolumetricFlowRate * 180 * .SolventViscosity * .ColumnLength * (1 - .InterparticlePorosity) ^ 2) / (.ParticleDiameter ^ 2 * .InterparticlePorosity ^ 2 * PI * dblRadius ^ 2 * 60) / .InterparticlePorosity
                    Else
                        dblBackPressure = 0
                    End If
                End If
            Else
                dblBackPressure = 0
            End If

            .BackPressure = dblBackPressure
        End With

        ' Compute Dead Time (and Linear Velocity)
        ' Must send false for RecalculateVolFlowRate since we're finding the back pressure, not volumetric flow rate
        ComputeDeadTime(utmUnitsTimeConstants.utmMinutes, False)

        ' Return Back Pressure
        Return ConvertPressure(dblBackPressure, uprUnitsPressureConstants.uprDynesPerSquareCm, eUnits)

    End Function

    ''' <summary>
    ''' Computes the column length, stores in .ColumnLength, and returns it
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <returns></returns>
    Public Function ComputeColumnLength(Optional eUnits As ulnUnitsLengthConstants = ulnUnitsLengthConstants.ulnCM) As Double

        Dim dblColumnLength, dblRadius As Double

        With mCapillaryFlowParameters

            dblRadius = .ColumnID / 2.0#

            If Math.Abs(.SolventViscosity) > Single.Epsilon And Math.Abs(.VolumetricFlowRate) > Single.Epsilon Then
                If .CapillaryType = ctCapillaryTypeConstants.ctOpenTubularCapillary Then
                    ' Open tubular capillary
                    dblColumnLength = (.BackPressure * dblRadius ^ 4 * PI * 60) / (8 * .SolventViscosity * .VolumetricFlowRate) ' Column length in cm
                Else
                    ' Packed capillary
                    If Math.Abs(.InterparticlePorosity - 1) > Single.Epsilon Then
                        ' Flow rate in mL/sec
                        dblColumnLength = (.BackPressure * .ParticleDiameter ^ 2 * .InterparticlePorosity ^ 2 * PI * dblRadius ^ 2 * 60) * .InterparticlePorosity / (180 * .SolventViscosity * .VolumetricFlowRate * (1 - .InterparticlePorosity) ^ 2)
                    Else
                        dblColumnLength = 0
                    End If
                End If
            Else
                dblColumnLength = 0
            End If

            .ColumnLength = dblColumnLength
        End With

        ' Compute Dead Time (and Linear Velocity)
        ComputeDeadTime(utmUnitsTimeConstants.utmMinutes, True)

        ' Return Column Length
        Return ConvertLength(dblColumnLength, ulnUnitsLengthConstants.ulnCM, eUnits)

    End Function

    Public Function ComputeColumnVolume(Optional eUnits As uvoUnitsVolumeConstants = 0) As Double
        ' Computes the column volume and returns it (does not store it)

        Dim dblColumnVolume, dblRadius As Double

        With mCapillaryFlowParameters

            dblRadius = .ColumnID / 2.0#

            dblColumnVolume = .ColumnLength * PI * dblRadius ^ 2 ' In mL

            If .CapillaryType = ctCapillaryTypeConstants.ctPackedCapillary Then
                dblColumnVolume *= .InterparticlePorosity
            End If
        End With

        Return ConvertVolume(dblColumnVolume, uvoUnitsVolumeConstants.uvoML, eUnits)

    End Function

    ''' <summary>
    ''' Computes the column length, stores in .ColumnLength, and returns it
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <returns></returns>
    Public Function ComputeColumnID(Optional eUnits As ulnUnitsLengthConstants = ulnUnitsLengthConstants.ulnMicrons) As Double

        Dim dblRadius As Double

        With mCapillaryFlowParameters

            If Math.Abs(.BackPressure) > Single.Epsilon Then
                If .CapillaryType = ctCapillaryTypeConstants.ctOpenTubularCapillary Then
                    ' Open tubular capillary
                    dblRadius = ((.VolumetricFlowRate * 8 * .SolventViscosity * .ColumnLength) / (.BackPressure * PI * 60)) ^ (0.25)
                Else
                    ' Packed capillary
                    If Math.Abs(.ParticleDiameter) > Single.Epsilon And Math.Abs(.InterparticlePorosity - 1) > Single.Epsilon Then
                        ' Flow rate in mL/sec
                        dblRadius = ((.VolumetricFlowRate * 180 * .SolventViscosity * .ColumnLength * (1 - .InterparticlePorosity) ^ 2) / (.BackPressure * .ParticleDiameter ^ 2 * .InterparticlePorosity ^ 2 * PI * 60) / .InterparticlePorosity) ^ 0.5
                    Else
                        dblRadius = 0
                    End If
                End If
            Else
                dblRadius = 0
            End If

            .ColumnID = dblRadius * 2.0#
        End With

        ' Compute Dead Time (and Linear Velocity)
        ComputeDeadTime(utmUnitsTimeConstants.utmMinutes, True)

        ' Return Column ID
        Return ConvertLength(dblRadius * 2.0#, ulnUnitsLengthConstants.ulnCM, eUnits)

    End Function

    ''' <summary>
    ''' Computes the column dead time, stores in .ColumnDeadTime, and returns it
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <param name="blnRecalculateVolFlowRate"></param>
    ''' <returns></returns>
    Public Function ComputeDeadTime(Optional eUnits As utmUnitsTimeConstants = utmUnitsTimeConstants.utmMinutes, Optional blnRecalculateVolFlowRate As Boolean = True) As Double

        Dim dblDeadTime As Double

        ' Dead time is dependent on Linear Velocity, so compute
        ComputeLinearVelocity(ulvUnitsLinearVelocityConstants.ulvCmPerSec, blnRecalculateVolFlowRate)

        With mCapillaryFlowParameters

            If Math.Abs(.LinearVelocity) > Single.Epsilon Then
                dblDeadTime = .ColumnLength / .LinearVelocity ' Dead time in minutes
            Else
                dblDeadTime = 0
            End If

            .ColumnDeadTime = dblDeadTime
        End With

        ' Return Dead Time
        Return ConvertTime(dblDeadTime, utmUnitsTimeConstants.utmMinutes, eUnits)

    End Function

    Public Function ComputeExtraColumnBroadeningResultantPeakWidth(Optional eUnits As utmUnitsTimeConstants = utmUnitsTimeConstants.utmSeconds) As Double
        ComputeExtraColumnBroadeningValues()

        Return GetExtraColumnBroadeningResultantPeakWidth(eUnits)
    End Function

    Private Sub ComputeExtraColumnBroadeningValues()
        Dim dblInitialPeakVariance As Double
        Dim dblSumOfVariances As Double

        With mExtraColumnBroadeningParameters
            If Math.Abs(.LinearVelocity) > Single.Epsilon And Math.Abs(.DiffusionCoefficient) > Single.Epsilon Then
                .TemporalVariance = .OpenTubeID ^ 2 * .OpenTubeLength / (96 * .DiffusionCoefficient * .LinearVelocity / 60) ' in sec^2
            Else
                .TemporalVariance = 0
            End If

            dblInitialPeakVariance = (.InitialPeakWidth / 4) ^ 2

            dblSumOfVariances = dblInitialPeakVariance + .TemporalVariance + .AdditionalTemporalVariance

            If dblSumOfVariances >= 0 Then
                ' ResultantPeakWidth at the base = 4 sigma  and  sigma = Sqr(Total_Variance)
                .ResultantPeakWidth = 4 * Math.Sqrt(dblSumOfVariances)
            Else
                .ResultantPeakWidth = 0
            End If
        End With
    End Sub

    ''' <summary>
    ''' Computes the Linear velocity, stores in .LinearVelocity, and returns it
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <param name="blnRecalculateVolFlowRate"></param>
    ''' <returns></returns>
    Public Function ComputeLinearVelocity(Optional eUnits As ulvUnitsLinearVelocityConstants = ulvUnitsLinearVelocityConstants.ulvCmPerSec, Optional blnRecalculateVolFlowRate As Boolean = True) As Double

        Dim dblLinearVelocity, dblRadius As Double

        If blnRecalculateVolFlowRate Then
            ComputeVolFlowRate(ufrUnitsFlowRateConstants.ufrMLPerMin)
        End If

        With mCapillaryFlowParameters
            dblRadius = .ColumnID / 2.0#
            If Math.Abs(dblRadius) > Single.Epsilon Then
                dblLinearVelocity = .VolumetricFlowRate / (PI * dblRadius ^ 2) ' Units in cm/min

                ' Divide Linear Velocity by epsilon if a packed capillary
                If .CapillaryType = ctCapillaryTypeConstants.ctPackedCapillary And Math.Abs(.InterparticlePorosity) > Single.Epsilon Then
                    dblLinearVelocity /= .InterparticlePorosity
                End If
            Else
                dblLinearVelocity = 0
            End If

            .LinearVelocity = dblLinearVelocity
        End With

        ' Return Linear Velocity
        Return ConvertLinearVelocity(dblLinearVelocity, ulvUnitsLinearVelocityConstants.ulvCmPerMin, eUnits)

    End Function

    ''' <summary>
    ''' Computes the MassFlowRate and Moles Injected, stores in .MassFlowRate and .MolesInjected, and returns MassFlowRate
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <returns></returns>
    Public Function ComputeMassFlowRate(Optional eUnits As umfMassFlowRateConstants = umfMassFlowRateConstants.umfFmolPerSec) As Double

        ComputeMassRateValues()
        Return GetMassFlowRate(eUnits)

    End Function

    ''' <summary>
    ''' Computes the MassFlowRate and Moles Injected, stores in .MassFlowRate and .MolesInjected, and returns MassFlowRate
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <returns></returns>
    Public Function ComputeMassRateMolesInjected(Optional eUnits As umaMolarAmountConstants = umaMolarAmountConstants.umaFemtoMoles) As Double

        ComputeMassRateValues()
        Return GetMassRateMolesInjected(eUnits)

    End Function

    Private Sub ComputeMassRateValues()

        With mMassRateParameters
            .MassFlowRate = .SampleConcentration * .VolumetricFlowRate / 1000 ' Compute mass flow rate in moles/min

            .MolesInjected = .MassFlowRate * .InjectionTime ' Compute moles injected in moles
        End With

    End Sub

    ''' <summary>
    ''' Computes the optimum linear velocity, based on
    ''' mCapillaryFlowParameters.ParticleDiameter
    ''' and mExtraColumnBroadeningParameters.DiffusionCoefficient
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <returns></returns>
    Public Function ComputeOptimumLinearVelocityUsingParticleDiamAndDiffusionCoeff(Optional eUnits As ulvUnitsLinearVelocityConstants = ulvUnitsLinearVelocityConstants.ulvCmPerSec) As Double

        Dim dblOptimumLinearVelocity As Double

        With mCapillaryFlowParameters
            If Math.Abs(.ParticleDiameter) > Single.Epsilon Then
                dblOptimumLinearVelocity = 3 * mExtraColumnBroadeningParameters.DiffusionCoefficient / .ParticleDiameter

                dblOptimumLinearVelocity = ConvertLinearVelocity(dblOptimumLinearVelocity, ulvUnitsLinearVelocityConstants.ulvCmPerSec, eUnits)
            End If
        End With

        Return dblOptimumLinearVelocity

    End Function

    Public Function ComputeMeCNViscosity(dblPercentAcetonitrile As Double, dblTemperature As Double, Optional eTemperatureUnits As utpUnitsTemperatureConstants = utpUnitsTemperatureConstants.utpCelsius, Optional eViscosityUnits As uviUnitsViscosityConstants = uviUnitsViscosityConstants.uviPoise) As Double

        Dim dblPhi As Double ' Fraction Acetonitrile
        Dim dblKelvin As Double
        Dim dblViscosityInCentiPoise As Double

        Try
            dblPhi = dblPercentAcetonitrile / 100.0#
            If dblPhi < 0 Then dblPhi = 0
            If dblPhi > 100 Then dblPhi = 100

            dblKelvin = ConvertTemperature(dblTemperature, eTemperatureUnits, utpUnitsTemperatureConstants.utpKelvin)

            If dblKelvin > 0 Then
                dblViscosityInCentiPoise = Math.Exp(dblPhi * (-3.476 + 726 / dblKelvin) + (1 - dblPhi) * (-5.414 + 1566 / dblKelvin) + dblPhi * (1 - dblPhi) * (-1.762 + 929 / dblKelvin))
            Else
                dblViscosityInCentiPoise = 0
            End If

            Return ConvertViscosity(dblViscosityInCentiPoise, uviUnitsViscosityConstants.uviCentiPoise, eViscosityUnits)
        Catch ex As Exception
            Return 0
        End Try

    End Function

    ''' <summary>
    ''' Computes the Volumetric flow rate, stores in .VolumetricFlowRate, and returns it
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <returns></returns>
    Public Function ComputeVolFlowRate(Optional eUnits As ufrUnitsFlowRateConstants = ufrUnitsFlowRateConstants.ufrNLPerMin) As Double

        Dim dblVolFlowRate, dblRadius As Double

        With mCapillaryFlowParameters

            dblRadius = .ColumnID / 2.0#

            If Math.Abs(.SolventViscosity) > Single.Epsilon And Math.Abs(.ColumnLength) > Single.Epsilon Then
                If .CapillaryType = ctCapillaryTypeConstants.ctOpenTubularCapillary Then
                    ' Open tubular capillary
                    dblVolFlowRate = (.BackPressure * dblRadius ^ 4 * PI) / (8 * .SolventViscosity * .ColumnLength) ' Flow rate in mL/sec
                Else
                    ' Packed capillary
                    If Math.Abs(.InterparticlePorosity - 1) > Single.Epsilon Then
                        ' Flow rate in mL/sec
                        dblVolFlowRate = (.BackPressure * .ParticleDiameter ^ 2 * .InterparticlePorosity ^ 2 * PI * dblRadius ^ 2) * .InterparticlePorosity / (180 * .SolventViscosity * .ColumnLength * (1 - .InterparticlePorosity) ^ 2)
                    Else
                        dblVolFlowRate = 0
                    End If
                End If

                ' Convert dblVolFlowRate to mL/min
                dblVolFlowRate *= 60
            Else
                dblVolFlowRate = 0
            End If

            .VolumetricFlowRate = dblVolFlowRate
        End With

        ' Compute Dead Time (and Linear Velocity)
        ComputeDeadTime(utmUnitsTimeConstants.utmMinutes, False)

        Return ConvertVolFlowRate(dblVolFlowRate, ufrUnitsFlowRateConstants.ufrMLPerMin, eUnits)
    End Function

    ''' <summary>
    ''' Computes the Volumetric flow rate using the dead time, stores in .VolumetricFlowRate, and returns it
    ''' This requires modifying the pressure value to give the computed volumetric flow rate
    ''' </summary>
    ''' <param name="dblNewBackPressure">Output: new back pressure</param>
    ''' <param name="eUnits"></param>
    ''' <param name="ePressureUnits"></param>
    ''' <returns></returns>
    Public Function ComputeVolFlowRateUsingDeadTime(
      <Out> Optional ByRef dblNewBackPressure As Double = 0,
      Optional eUnits As ufrUnitsFlowRateConstants = ufrUnitsFlowRateConstants.ufrNLPerMin,
      Optional ePressureUnits As uprUnitsPressureConstants = uprUnitsPressureConstants.uprPsi) As Double

        Dim dblVolFlowRate, dblRadius As Double

        With mCapillaryFlowParameters

            dblRadius = .ColumnID / 2.0#

            ' First find vol flow rate that gives observed dead time
            If Math.Abs(.ColumnDeadTime) > Single.Epsilon Then

                dblVolFlowRate = .ColumnLength * (PI * dblRadius ^ 2) / .ColumnDeadTime ' Vol flow rate in mL/sec

                If .CapillaryType = ctCapillaryTypeConstants.ctPackedCapillary Then
                    ' Packed Capillary
                    dblVolFlowRate *= .InterparticlePorosity
                End If

                ' Store the new value
                .VolumetricFlowRate = dblVolFlowRate

                ' Now find pressure that gives computed dblVolFlowRate
                ' The ComputeBackPressure sub will store the new pressure
                dblNewBackPressure = ComputeBackPressure(ePressureUnits)
            Else
                dblVolFlowRate = 0
                .VolumetricFlowRate = 0
            End If

        End With

        ' Compute Linear Velocity (but not the dead time)
        ComputeLinearVelocity(ulvUnitsLinearVelocityConstants.ulvCmPerSec, False)

        Return ConvertVolFlowRate(dblVolFlowRate, ufrUnitsFlowRateConstants.ufrMLPerMin, eUnits)
    End Function

    ''' <summary>
    ''' Convert concentration
    ''' </summary>
    ''' <param name="dblConcentrationIn"></param>
    ''' <param name="eCurrentUnits"></param>
    ''' <param name="eNewUnits"></param>
    ''' <returns></returns>
    ''' <remarks>Duplicated function, in both CapillaryFlow and MoleMassDilution</remarks>
    Public Function ConvertConcentration(dblConcentrationIn As Double, eCurrentUnits As ucoUnitsConcentrationConstants, eNewUnits As ucoUnitsConcentrationConstants) As Double
        Dim dblValue, dblFactor As Double
        Dim dblSampleMass As Double

        If eCurrentUnits = eNewUnits Then
            Return dblConcentrationIn
        End If

        dblSampleMass = mMassRateParameters.SampleMass

        dblFactor = FactorConcentration(eCurrentUnits, dblSampleMass)
        If Math.Abs(dblFactor + 1) < Single.Epsilon Then
            Return -1
        Else
            dblValue = dblConcentrationIn * dblFactor
        End If

        dblFactor = FactorConcentration(eNewUnits, dblSampleMass)
        If Math.Abs(dblFactor + 1) < Single.Epsilon Or Math.Abs(dblFactor) < Single.Epsilon Then
            Return -1
        Else
            Return dblValue / dblFactor
        End If

    End Function

    Public Function ConvertDiffusionCoefficient(dblDiffusionCoefficientIn As Double, eCurrentUnits As udcDiffusionCoefficientConstants, eNewUnits As udcDiffusionCoefficientConstants) As Double
        Dim dblValue, dblFactor As Double

        If eCurrentUnits = eNewUnits Then
            Return dblDiffusionCoefficientIn
        End If

        dblFactor = FactorDiffusionCoeff(eCurrentUnits)
        If Math.Abs(dblFactor + 1) < Single.Epsilon Then
            Return -1
        Else
            dblValue = dblDiffusionCoefficientIn * dblFactor
        End If

        dblFactor = FactorDiffusionCoeff(eNewUnits)
        If Math.Abs(dblFactor + 1) < Single.Epsilon Or Math.Abs(dblFactor) < Single.Epsilon Then
            Return -1
        Else
            Return dblValue / dblFactor
        End If

    End Function

    Public Function ConvertLength(dblLengthIn As Double, eCurrentUnits As ulnUnitsLengthConstants, eNewUnits As ulnUnitsLengthConstants) As Double
        Dim dblValue, dblFactor As Double

        If eCurrentUnits = eNewUnits Then
            Return dblLengthIn
        End If

        dblFactor = FactorLength(eCurrentUnits)
        If Math.Abs(dblFactor + 1) < Single.Epsilon Then
            Return -1
        Else
            dblValue = dblLengthIn * dblFactor
        End If

        dblFactor = FactorLength(eNewUnits)
        If Math.Abs(dblFactor + 1) < Single.Epsilon Or Math.Abs(dblFactor) < Single.Epsilon Then
            Return -1
        Else
            Return dblValue / dblFactor
        End If

    End Function

    Public Function ConvertLinearVelocity(dblLinearVelocityIn As Double, eCurrentUnits As ulvUnitsLinearVelocityConstants, eNewUnits As ulvUnitsLinearVelocityConstants) As Double
        Dim dblValue, dblFactor As Double

        If eCurrentUnits = eNewUnits Then
            Return dblLinearVelocityIn
        End If

        dblFactor = FactorLinearVelocity(eCurrentUnits)
        If Math.Abs(dblFactor + 1) < Single.Epsilon Then
            Return -1
        Else
            dblValue = dblLinearVelocityIn * dblFactor
        End If

        dblFactor = FactorLinearVelocity(eNewUnits)
        If Math.Abs(dblFactor + 1) < Single.Epsilon Or Math.Abs(dblFactor) < Single.Epsilon Then
            Return -1
        Else
            Return dblValue / dblFactor
        End If

    End Function

    Public Function ConvertMassFlowRate(dblMassFlowRateIn As Double, eCurrentUnits As umfMassFlowRateConstants, eNewUnits As umfMassFlowRateConstants) As Double
        Dim dblValue, dblFactor As Double

        If eCurrentUnits = eNewUnits Then
            Return dblMassFlowRateIn
        End If

        dblFactor = FactorMassFlowRate(eCurrentUnits)
        If Math.Abs(dblFactor + 1) < Single.Epsilon Then
            Return -1
        Else
            dblValue = dblMassFlowRateIn * dblFactor
        End If

        dblFactor = FactorMassFlowRate(eNewUnits)
        If Math.Abs(dblFactor + 1) < Single.Epsilon Or Math.Abs(dblFactor) < Single.Epsilon Then
            Return -1
        Else
            Return dblValue / dblFactor
        End If

    End Function

    Public Function ConvertMoles(dblMolesIn As Double, eCurrentUnits As umaMolarAmountConstants, eNewUnits As umaMolarAmountConstants) As Double
        Dim dblValue, dblFactor As Double

        If eCurrentUnits = eNewUnits Then
            Return dblMolesIn
        End If

        dblFactor = FactorMoles(eCurrentUnits)
        If Math.Abs(dblFactor + 1) < Single.Epsilon Then
            Return -1
        Else
            dblValue = dblMolesIn * dblFactor
        End If

        dblFactor = FactorMoles(eNewUnits)
        If Math.Abs(dblFactor + 1) < Single.Epsilon Or Math.Abs(dblFactor) < Single.Epsilon Then
            Return -1
        Else
            Return dblValue / dblFactor
        End If

    End Function

    Public Function ConvertPressure(dblPressureIn As Double, eCurrentUnits As uprUnitsPressureConstants, eNewUnits As uprUnitsPressureConstants) As Double
        Dim dblValue, dblFactor As Double

        If eCurrentUnits = eNewUnits Then
            Return dblPressureIn
        End If

        dblFactor = FactorPressure(eCurrentUnits)
        If Math.Abs(dblFactor + 1) < Single.Epsilon Then
            Return -1
        Else
            dblValue = dblPressureIn * dblFactor
        End If

        dblFactor = FactorPressure(eNewUnits)
        If Math.Abs(dblFactor + 1) < Single.Epsilon Or Math.Abs(dblFactor) < Single.Epsilon Then
            Return -1
        Else
            Return dblValue / dblFactor
        End If

    End Function

    Public Function ConvertTemperature(dblTemperatureIn As Double, eCurrentUnits As utpUnitsTemperatureConstants, eNewUnits As utpUnitsTemperatureConstants) As Double
        Dim dblValue As Double

        If eCurrentUnits = eNewUnits Then
            Return dblTemperatureIn
        End If

        ' First convert to Kelvin
        Select Case eCurrentUnits
            Case utpUnitsTemperatureConstants.utpCelsius
                ' K = C + 273
                dblValue = dblTemperatureIn + 273
            Case utpUnitsTemperatureConstants.utpFahrenheit
                ' Convert to Kelvin: C = 5/9*(F-32) and K = C + 273
                dblValue = 5.0# / 9.0# * (dblTemperatureIn - 32) + 273
            Case Else
                ' Includes utpKelvin
                ' Assume already Kelvin
        End Select

        ' We cannot get colder than absolute 0
        If dblValue < 0 Then dblValue = 0

        ' Now convert to the target units
        Select Case eNewUnits
            Case utpUnitsTemperatureConstants.utpCelsius
                ' C = K - 273
                dblValue -= 273
            Case utpUnitsTemperatureConstants.utpFahrenheit
                ' Convert to Fahrenheit: C = K - 273 and F = (9/5)C + 32
                dblValue = 9.0# / 5.0# * (dblValue - 273) + 32
            Case Else
                ' Includes utpKelvin
                ' Already in Kelvin
        End Select

        Return dblValue

    End Function

    Public Function ConvertTime(dblTimeIn As Double, eCurrentUnits As utmUnitsTimeConstants, eNewUnits As utmUnitsTimeConstants) As Double
        Dim dblValue, dblFactor As Double

        If eCurrentUnits = eNewUnits Then
            Return dblTimeIn
        End If

        dblFactor = FactorTime(eCurrentUnits)
        If Math.Abs(dblFactor + 1) < Single.Epsilon Then
            Return -1
        Else
            dblValue = dblTimeIn * dblFactor
        End If

        dblFactor = FactorTime(eNewUnits)
        If Math.Abs(dblFactor + 1) < Single.Epsilon Or Math.Abs(dblFactor) < Single.Epsilon Then
            Return -1
        Else
            Return dblValue / dblFactor
        End If

    End Function

    Public Function ConvertViscosity(dblViscosityIn As Double, eCurrentUnits As uviUnitsViscosityConstants, eNewUnits As uviUnitsViscosityConstants) As Double
        Dim dblValue, dblFactor As Double

        If eCurrentUnits = eNewUnits Then
            Return dblViscosityIn
        End If

        dblFactor = FactorViscosity(eCurrentUnits)
        If Math.Abs(dblFactor + 1) < Single.Epsilon Then
            Return -1
        Else
            dblValue = dblViscosityIn * dblFactor
        End If

        dblFactor = FactorViscosity(eNewUnits)
        If Math.Abs(dblFactor + 1) < Single.Epsilon Or Math.Abs(dblFactor) < Single.Epsilon Then
            Return -1
        Else
            Return dblValue / dblFactor
        End If

    End Function

    Public Function ConvertVolFlowRate(dblVolFlowRateIn As Double, eCurrentUnits As ufrUnitsFlowRateConstants, eNewUnits As ufrUnitsFlowRateConstants) As Double
        Dim dblValue, dblFactor As Double

        If eCurrentUnits = eNewUnits Then
            Return dblVolFlowRateIn
        End If

        dblFactor = FactorVolFlowRate(eCurrentUnits)
        If Math.Abs(dblFactor + 1) < Single.Epsilon Then
            Return -1
        Else
            dblValue = dblVolFlowRateIn * dblFactor
        End If

        dblFactor = FactorVolFlowRate(eNewUnits)
        If Math.Abs(dblFactor + 1) < Single.Epsilon Or Math.Abs(dblFactor) < Single.Epsilon Then
            Return -1
        Else
            Return dblValue / dblFactor
        End If

    End Function

    Public Function ConvertVolume(dblVolume As Double, eCurrentUnits As uvoUnitsVolumeConstants, eNewUnits As uvoUnitsVolumeConstants) As Double
        Dim dblValue, dblFactor As Double

        If eCurrentUnits = eNewUnits Then
            Return dblVolume
        End If

        dblFactor = FactorVolume(eCurrentUnits)
        If Math.Abs(dblFactor + 1) < Single.Epsilon Then
            Return -1
        Else
            dblValue = dblVolume * dblFactor
        End If

        dblFactor = FactorVolume(eNewUnits)
        If Math.Abs(dblFactor + 1) < Single.Epsilon Or Math.Abs(dblFactor) < Single.Epsilon Then
            Return -1
        Else
            Return dblValue / dblFactor
        End If

    End Function

    ''' <summary>
    ''' Multiplication factor for converting from eUnits to M
    ''' dblSampleMass is required for mass-based units
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <param name="dblSampleMass"></param>
    ''' <returns></returns>
    ''' <remarks>Duplicated function, in both CapillaryFlow and MoleMassDilution</remarks>
    Private Function FactorConcentration(eUnits As ucoUnitsConcentrationConstants, Optional dblSampleMass As Double = 0) As Double
        Dim dblFactor As Double

        If Math.Abs(dblSampleMass) < Single.Epsilon Then
            dblFactor = -1
        Else
            Select Case eUnits
                Case ucoUnitsConcentrationConstants.ucoMolar : dblFactor = 1.0#
                Case ucoUnitsConcentrationConstants.ucoMilliMolar : dblFactor = 1 / 1000.0#
                Case ucoUnitsConcentrationConstants.ucoMicroMolar : dblFactor = 1 / 1000000.0#
                Case ucoUnitsConcentrationConstants.ucoNanoMolar : dblFactor = 1 / 1000000000.0#
                Case ucoUnitsConcentrationConstants.ucoPicoMolar : dblFactor = 1 / 1000000000000.0#
                Case ucoUnitsConcentrationConstants.ucoFemtoMolar : dblFactor = 1 / 1.0E+15
                Case ucoUnitsConcentrationConstants.ucoAttoMolar : dblFactor = 1 / 1.0E+18
                Case ucoUnitsConcentrationConstants.ucoMgPerML : dblFactor = 1 / dblSampleMass '1/[(1 g / 1000 mg) * (1 / MW) * (1000 mL/L)]
                Case ucoUnitsConcentrationConstants.ucoUgPerML : dblFactor = 1 / (dblSampleMass * 1000.0#) '1/[(1 g / 1000000 ug) * (1 / MW) * (1000 mL/L)]
                Case ucoUnitsConcentrationConstants.ucoNgPerML : dblFactor = 1 / (dblSampleMass * 1000000.0#) '1/[(1 g / 1000000000 ng) * (1 / MW) * (1000 mL/L)]
                Case ucoUnitsConcentrationConstants.ucoUgPerUL : dblFactor = 1 / (dblSampleMass) '1/[(1 g / 1000000 ug) * (1 / MW) * (1000000 uL/L)]
                Case ucoUnitsConcentrationConstants.ucoNgPerUL : dblFactor = 1 / (dblSampleMass * 1000.0#) '1/[(1 g / 1000000000 ng) * (1 / MW) * (1000000 uL/L)]
                Case Else : dblFactor = -1
            End Select
        End If

        Return dblFactor
    End Function

    ''' <summary>
    ''' Multiplication factor for converting from eUnits to Cm
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <returns></returns>
    Private Function FactorLength(eUnits As ulnUnitsLengthConstants) As Double

        Select Case eUnits
            Case ulnUnitsLengthConstants.ulnM : Return 100.0#
            Case ulnUnitsLengthConstants.ulnCM : Return 1.0#
            Case ulnUnitsLengthConstants.ulnMM : Return 1 / 10.0#
            Case ulnUnitsLengthConstants.ulnMicrons : Return 1 / 10000.0#
            Case ulnUnitsLengthConstants.ulnInches : Return CM_PER_INCH
            Case Else : Return -1
        End Select

    End Function

    ''' <summary>
    ''' Multiplication factor for converting from eUnits to Cm/Min
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <returns></returns>
    Private Function FactorLinearVelocity(eUnits As ulvUnitsLinearVelocityConstants) As Double

        Select Case eUnits
            Case ulvUnitsLinearVelocityConstants.ulvCmPerHr : Return 1 / 60.0#
            Case ulvUnitsLinearVelocityConstants.ulvMmPerHr : Return 1 / 60.0# / 10.0#
            Case ulvUnitsLinearVelocityConstants.ulvCmPerMin : Return 1
            Case ulvUnitsLinearVelocityConstants.ulvMmPerMin : Return 1 / 10.0#
            Case ulvUnitsLinearVelocityConstants.ulvCmPerSec : Return 60.0#
            Case ulvUnitsLinearVelocityConstants.ulvMmPerSec : Return 60.0# / 10.0#
            Case Else : Return -1
        End Select

    End Function

    ''' <summary>
    ''' Multiplication factor for converting from eUnits to moles/min
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <returns></returns>
    Private Function FactorMassFlowRate(eUnits As umfMassFlowRateConstants) As Double

        Select Case eUnits
            Case umfMassFlowRateConstants.umfPmolPerMin : Return 1 / 1000000000000.0#
            Case umfMassFlowRateConstants.umfFmolPerMin : Return 1 / 1.0E+15
            Case umfMassFlowRateConstants.umfAmolPerMin : Return 1 / 1.0E+18
            Case umfMassFlowRateConstants.umfPmolPerSec : Return 1 / (1000000000000.0# / 60.0#)
            Case umfMassFlowRateConstants.umfFmolPerSec : Return 1 / (1.0E+15 / 60.0#)
            Case umfMassFlowRateConstants.umfAmolPerSec : Return 1 / (1.0E+18 / 60.0#)
            Case umfMassFlowRateConstants.umfMolesPerMin : Return 1.0#
            Case Else : Return -1
        End Select

    End Function

    ''' <summary>
    ''' Multiplication factor for converting from eUnits to moles
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <returns></returns>
    Private Function FactorMoles(eUnits As umaMolarAmountConstants) As Double

        Select Case eUnits
            Case umaMolarAmountConstants.umaMoles : Return 1.0#
            Case umaMolarAmountConstants.umaMilliMoles : Return 1 / 1000.0#
            Case umaMolarAmountConstants.umaMicroMoles : Return 1 / 1000000.0#
            Case umaMolarAmountConstants.umaNanoMoles : Return 1 / 1000000000.0#
            Case umaMolarAmountConstants.umaPicoMoles : Return 1 / 1000000000000.0#
            Case umaMolarAmountConstants.umaFemtoMoles : Return 1 / 1.0E+15
            Case umaMolarAmountConstants.umaAttoMoles : Return 1 / 1.0E+18
            Case Else : Return -1
        End Select

    End Function

    ''' <summary>
    ''' Multiplication factor for converting from eUnits to dynes per cm^2
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <returns></returns>
    Private Function FactorPressure(eUnits As uprUnitsPressureConstants) As Double

        Select Case eUnits
            Case uprUnitsPressureConstants.uprPsi : Return 68947.57
            Case uprUnitsPressureConstants.uprPascals : Return 10.0#
            Case uprUnitsPressureConstants.uprKiloPascals : Return 10000.0#
            Case uprUnitsPressureConstants.uprAtmospheres : Return 1013250.0#
            Case uprUnitsPressureConstants.uprBar : Return 1000000.0#
            Case uprUnitsPressureConstants.uprTorr : Return 1333.22
            Case uprUnitsPressureConstants.uprDynesPerSquareCm : Return 1
            Case Else : Return -1
        End Select

    End Function

    ''' <summary>
    ''' Multiplication factor for converting from eUnits to minutes
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <returns></returns>
    Private Function FactorTime(eUnits As utmUnitsTimeConstants) As Double

        Select Case eUnits
            Case utmUnitsTimeConstants.utmHours : Return 60.0#
            Case utmUnitsTimeConstants.utmMinutes : Return 1.0#
            Case utmUnitsTimeConstants.utmSeconds : Return 1 / 60.0#
            Case Else : Return -1
        End Select

    End Function

    ''' <summary>
    ''' Multiplication factor for converting from eUnits to cm^2/sec
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <returns></returns>
    Private Function FactorDiffusionCoeff(eUnits As udcDiffusionCoefficientConstants) As Double

        Select Case eUnits
            Case udcDiffusionCoefficientConstants.udcCmSquaredPerHr : Return 1 / 3600.0#
            Case udcDiffusionCoefficientConstants.udcCmSquaredPerMin : Return 1 / 60.0#
            Case udcDiffusionCoefficientConstants.udcCmSquaredPerSec : Return 1.0#
            Case Else : Return -1
        End Select

    End Function

    ''' <summary>
    ''' Multiplication factor for converting from eUnits to poise
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <returns></returns>
    Private Function FactorViscosity(eUnits As uviUnitsViscosityConstants) As Double

        Select Case eUnits
            Case uviUnitsViscosityConstants.uviPoise : Return 1.0#
            Case uviUnitsViscosityConstants.uviCentiPoise : Return 1 / 100.0#
            Case Else : Return -1
        End Select

    End Function

    ''' <summary>
    ''' Multiplication factor for converting from eUnits to mL/min
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <returns></returns>
    Private Function FactorVolFlowRate(eUnits As ufrUnitsFlowRateConstants) As Double

        Select Case eUnits
            Case ufrUnitsFlowRateConstants.ufrMLPerMin : FactorVolFlowRate = 1.0#
            Case ufrUnitsFlowRateConstants.ufrULPerMin : FactorVolFlowRate = 1 / 1000.0#
            Case ufrUnitsFlowRateConstants.ufrNLPerMin : FactorVolFlowRate = 1 / 1000000.0#
            Case Else : FactorVolFlowRate = -1
        End Select

    End Function

    ''' <summary>
    ''' Multiplication factor for converting from eUnits to mL
    ''' </summary>
    ''' <param name="eUnits"></param>
    ''' <returns></returns>
    Private Function FactorVolume(eUnits As uvoUnitsVolumeConstants) As Double

        Select Case eUnits
            Case uvoUnitsVolumeConstants.uvoML : Return 1.0#
            Case uvoUnitsVolumeConstants.uvoUL : Return 1 / 1000.0#
            Case uvoUnitsVolumeConstants.uvoNL : Return 1 / 1000000.0#
            Case uvoUnitsVolumeConstants.uvoPL : Return 1 / 1000000000.0#
            Case Else : Return -1
        End Select

    End Function

    ' Get Methods
    ' Gets the most recently computed value
    ' If mAutoCompute = False, then must manually call a Compute Sub to recompute the value

    Public Function GetAutoComputeEnabled() As Boolean
        Return mAutoCompute
    End Function

    Public Function GetAutoComputeMode() As acmAutoComputeModeConstants
        Return mAutoComputeMode
    End Function

    Public Function GetBackPressure(Optional eUnits As uprUnitsPressureConstants = uprUnitsPressureConstants.uprPsi) As Double
        Return ConvertPressure(mCapillaryFlowParameters.BackPressure, uprUnitsPressureConstants.uprDynesPerSquareCm, eUnits)
    End Function

    Public Function GetCapillaryType() As ctCapillaryTypeConstants
        Return mCapillaryFlowParameters.CapillaryType
    End Function

    Public Function GetColumnID(Optional eUnits As ulnUnitsLengthConstants = ulnUnitsLengthConstants.ulnMicrons) As Double
        Return ConvertLength(mCapillaryFlowParameters.ColumnID, ulnUnitsLengthConstants.ulnCM, eUnits)
    End Function

    Public Function GetColumnLength(Optional eUnits As ulnUnitsLengthConstants = ulnUnitsLengthConstants.ulnCM) As Double
        Return ConvertLength(mCapillaryFlowParameters.ColumnLength, ulnUnitsLengthConstants.ulnCM, eUnits)
    End Function

    Public Function GetColumnVolume(Optional eUnits As uvoUnitsVolumeConstants = uvoUnitsVolumeConstants.uvoUL) As Double
        ' Column volume isn't stored; simply re-compute it
        Return ComputeColumnVolume(eUnits)
    End Function

    Public Function GetDeadTime(Optional eUnits As utmUnitsTimeConstants = utmUnitsTimeConstants.utmMinutes) As Double
        Return ConvertTime(mCapillaryFlowParameters.ColumnDeadTime, utmUnitsTimeConstants.utmMinutes, eUnits)
    End Function

    Public Function GetExtraColumnBroadeningAdditionalVarianceInSquareSeconds() As Double
        Return mExtraColumnBroadeningParameters.AdditionalTemporalVariance
    End Function

    Public Function GetExtraColumnBroadeningDiffusionCoefficient(Optional eUnits As udcDiffusionCoefficientConstants = udcDiffusionCoefficientConstants.udcCmSquaredPerSec) As Double
        Return ConvertDiffusionCoefficient(mExtraColumnBroadeningParameters.DiffusionCoefficient, udcDiffusionCoefficientConstants.udcCmSquaredPerSec, eUnits)
    End Function

    Public Function GetExtraColumnBroadeningInitialPeakWidthAtBase(Optional eUnits As utmUnitsTimeConstants = utmUnitsTimeConstants.utmSeconds) As Double
        Return ConvertTime(mExtraColumnBroadeningParameters.InitialPeakWidth, utmUnitsTimeConstants.utmSeconds, eUnits)
    End Function

    Public Function GetExtraColumnBroadeningLinearVelocity(Optional eUnits As ulvUnitsLinearVelocityConstants = ulvUnitsLinearVelocityConstants.ulvMmPerMin) As Double
        Return ConvertLinearVelocity(mExtraColumnBroadeningParameters.LinearVelocity, ulvUnitsLinearVelocityConstants.ulvCmPerMin, eUnits)
    End Function

    Public Function GetExtraColumnBroadeningOpenTubeID(Optional eUnits As ulnUnitsLengthConstants = ulnUnitsLengthConstants.ulnMicrons) As Double
        Return ConvertLength(mExtraColumnBroadeningParameters.OpenTubeID, ulnUnitsLengthConstants.ulnCM, eUnits)
    End Function

    Public Function GetExtraColumnBroadeningOpenTubeLength(Optional eUnits As ulnUnitsLengthConstants = ulnUnitsLengthConstants.ulnCM) As Double
        Return ConvertLength(mExtraColumnBroadeningParameters.OpenTubeLength, ulnUnitsLengthConstants.ulnCM, eUnits)
    End Function

    Public Function GetExtraColumnBroadeningResultantPeakWidth(Optional eUnits As utmUnitsTimeConstants = utmUnitsTimeConstants.utmSeconds) As Double
        Return ConvertTime(mExtraColumnBroadeningParameters.ResultantPeakWidth, utmUnitsTimeConstants.utmSeconds, eUnits)
    End Function

    Public Function GetExtraColumnBroadeningTemporalVarianceInSquareSeconds() As Double
        Return mExtraColumnBroadeningParameters.TemporalVariance
    End Function

    Public Function GetInterparticlePorosity() As Double
        Return mCapillaryFlowParameters.InterparticlePorosity
    End Function

    Public Function GetLinearVelocity(Optional eUnits As ulvUnitsLinearVelocityConstants = ulvUnitsLinearVelocityConstants.ulvCmPerSec) As Double
        Return ConvertLinearVelocity(mCapillaryFlowParameters.LinearVelocity, ulvUnitsLinearVelocityConstants.ulvCmPerMin, eUnits)
    End Function

    Public Function GetMassRateConcentration(Optional eUnits As ucoUnitsConcentrationConstants = ucoUnitsConcentrationConstants.ucoMicroMolar) As Double
        Return ConvertConcentration(mMassRateParameters.SampleConcentration, ucoUnitsConcentrationConstants.ucoMolar, eUnits)
    End Function

    Public Function GetMassRateInjectionTime(Optional eUnits As utmUnitsTimeConstants = utmUnitsTimeConstants.utmMinutes) As Double
        Return ConvertTime(mMassRateParameters.InjectionTime, utmUnitsTimeConstants.utmMinutes, eUnits)
    End Function

    Public Function GetMassFlowRate(Optional eUnits As umfMassFlowRateConstants = umfMassFlowRateConstants.umfFmolPerSec) As Double
        Return ConvertMassFlowRate(mMassRateParameters.MassFlowRate, umfMassFlowRateConstants.umfMolesPerMin, eUnits)
    End Function

    Public Function GetMassRateMolesInjected(Optional eUnits As umaMolarAmountConstants = umaMolarAmountConstants.umaFemtoMoles) As Double
        Return ConvertMoles(mMassRateParameters.MolesInjected, umaMolarAmountConstants.umaMoles, eUnits)
    End Function

    Public Function GetMassRateSampleMass() As Double
        Return mMassRateParameters.SampleMass
    End Function

    Public Function GetMassRateVolFlowRate(Optional eUnits As ufrUnitsFlowRateConstants = ufrUnitsFlowRateConstants.ufrNLPerMin) As Double
        Return ConvertVolFlowRate(mMassRateParameters.VolumetricFlowRate, ufrUnitsFlowRateConstants.ufrMLPerMin, eUnits)
    End Function

    Public Function GetParticleDiameter(Optional eUnits As ulnUnitsLengthConstants = ulnUnitsLengthConstants.ulnMicrons) As Double
        Return ConvertLength(mCapillaryFlowParameters.ParticleDiameter, ulnUnitsLengthConstants.ulnCM, eUnits)
    End Function

    Public Function GetSolventViscosity(Optional eUnits As uviUnitsViscosityConstants = uviUnitsViscosityConstants.uviPoise) As Double
        Return ConvertViscosity(mCapillaryFlowParameters.SolventViscosity, uviUnitsViscosityConstants.uviPoise, eUnits)
    End Function

    Public Function GetVolFlowRate(Optional eUnits As ufrUnitsFlowRateConstants = ufrUnitsFlowRateConstants.ufrNLPerMin) As Double
        Return ConvertVolFlowRate(mCapillaryFlowParameters.VolumetricFlowRate, ufrUnitsFlowRateConstants.ufrMLPerMin, eUnits)
    End Function


    ' Set Methods
    ' If mAutoCompute = False, then must manually call a Compute Sub to recompute other values

    Public Sub SetAutoComputeEnabled(blnAutoCompute As Boolean)
        mAutoCompute = blnAutoCompute
    End Sub

    Public Sub SetAutoComputeMode(eAutoComputeMode As acmAutoComputeModeConstants)
        If eAutoComputeMode >= acmAutoComputeModeConstants.acmBackPressure And eAutoComputeMode <= acmAutoComputeModeConstants.acmVolFlowRateUsingDeadTime Then
            mAutoComputeMode = eAutoComputeMode
        End If
    End Sub

    Public Sub SetBackPressure(dblBackPressure As Double, Optional eUnits As uprUnitsPressureConstants = uprUnitsPressureConstants.uprPsi)
        mCapillaryFlowParameters.BackPressure = ConvertPressure(dblBackPressure, eUnits, uprUnitsPressureConstants.uprDynesPerSquareCm)
        CheckAutoCompute()
    End Sub

    Public Sub SetCapillaryType(eCapillaryType As ctCapillaryTypeConstants)
        If eCapillaryType >= ctCapillaryTypeConstants.ctOpenTubularCapillary And eCapillaryType <= ctCapillaryTypeConstants.ctPackedCapillary Then
            mCapillaryFlowParameters.CapillaryType = eCapillaryType
        End If
        CheckAutoCompute()
    End Sub

    Public Sub SetColumnID(dblColumnID As Double, Optional eUnits As ulnUnitsLengthConstants = ulnUnitsLengthConstants.ulnMicrons)
        mCapillaryFlowParameters.ColumnID = ConvertLength(dblColumnID, eUnits, ulnUnitsLengthConstants.ulnCM)
        CheckAutoCompute()
    End Sub

    Public Sub SetColumnLength(dblColumnLength As Double, Optional eUnits As ulnUnitsLengthConstants = ulnUnitsLengthConstants.ulnCM)
        mCapillaryFlowParameters.ColumnLength = ConvertLength(dblColumnLength, eUnits, ulnUnitsLengthConstants.ulnCM)
        CheckAutoCompute()
    End Sub

    Public Sub SetDeadTime(dblDeadTime As Double, Optional eUnits As utmUnitsTimeConstants = utmUnitsTimeConstants.utmMinutes)
        mCapillaryFlowParameters.ColumnDeadTime = ConvertTime(dblDeadTime, eUnits, utmUnitsTimeConstants.utmMinutes)
        CheckAutoCompute()
    End Sub

    Public Sub SetExtraColumnBroadeningAdditionalVariance(dblAdditionalVarianceInSquareSeconds As Double)
        mExtraColumnBroadeningParameters.AdditionalTemporalVariance = dblAdditionalVarianceInSquareSeconds
        ComputeExtraColumnBroadeningValues()
    End Sub

    Public Sub SetExtraColumnBroadeningDiffusionCoefficient(dblDiffusionCoefficient As Double, Optional eUnits As udcDiffusionCoefficientConstants = udcDiffusionCoefficientConstants.udcCmSquaredPerSec)
        mExtraColumnBroadeningParameters.DiffusionCoefficient = ConvertDiffusionCoefficient(dblDiffusionCoefficient, eUnits, udcDiffusionCoefficientConstants.udcCmSquaredPerSec)
        ComputeExtraColumnBroadeningValues()
    End Sub

    Public Sub SetExtraColumnBroadeningInitialPeakWidthAtBase(dblWidth As Double, Optional eUnits As utmUnitsTimeConstants = utmUnitsTimeConstants.utmSeconds)
        mExtraColumnBroadeningParameters.InitialPeakWidth = ConvertTime(dblWidth, eUnits, utmUnitsTimeConstants.utmSeconds)
        ComputeExtraColumnBroadeningValues()
    End Sub

    Public Sub SetExtraColumnBroadeningLinearVelocity(dblLinearVelocity As Double, Optional eUnits As ulvUnitsLinearVelocityConstants = ulvUnitsLinearVelocityConstants.ulvMmPerMin)
        mExtraColumnBroadeningParameters.LinearVelocity = ConvertLinearVelocity(dblLinearVelocity, eUnits, ulvUnitsLinearVelocityConstants.ulvCmPerMin)
        ComputeExtraColumnBroadeningValues()
    End Sub

    Public Sub SetExtraColumnBroadeningOpenTubeID(dblOpenTubeID As Double, Optional eUnits As ulnUnitsLengthConstants = ulnUnitsLengthConstants.ulnMicrons)
        mExtraColumnBroadeningParameters.OpenTubeID = ConvertLength(dblOpenTubeID, eUnits, ulnUnitsLengthConstants.ulnCM)
        ComputeExtraColumnBroadeningValues()
    End Sub

    Public Sub SetExtraColumnBroadeningOpenTubeLength(dblLength As Double, Optional eUnits As ulnUnitsLengthConstants = ulnUnitsLengthConstants.ulnCM)
        mExtraColumnBroadeningParameters.OpenTubeLength = ConvertLength(dblLength, eUnits, ulnUnitsLengthConstants.ulnCM)
        ComputeExtraColumnBroadeningValues()
    End Sub

    Public Sub SetInterparticlePorosity(dblPorosity As Double)
        If dblPorosity >= 0 And dblPorosity <= 1 Then
            mCapillaryFlowParameters.InterparticlePorosity = dblPorosity
        End If
        CheckAutoCompute()
    End Sub

    Public Sub SetMassRateConcentration(dblConcentration As Double, Optional eUnits As ucoUnitsConcentrationConstants = ucoUnitsConcentrationConstants.ucoMicroMolar)
        mMassRateParameters.SampleConcentration = ConvertConcentration(dblConcentration, eUnits, ucoUnitsConcentrationConstants.ucoMolar)
        ComputeMassRateValues()
    End Sub

    Public Sub SetMassRateInjectionTime(dblInjectionTime As Double, Optional eUnits As utmUnitsTimeConstants = utmUnitsTimeConstants.utmMinutes)
        mMassRateParameters.InjectionTime = ConvertTime(dblInjectionTime, eUnits, utmUnitsTimeConstants.utmMinutes)
        ComputeMassRateValues()
    End Sub

    Public Sub SetMassRateSampleMass(dblMassInGramsPerMole As Double)
        If dblMassInGramsPerMole >= 0 Then
            mMassRateParameters.SampleMass = dblMassInGramsPerMole
        Else
            mMassRateParameters.SampleMass = 0
        End If
        ComputeMassRateValues()
    End Sub

    Public Sub SetMassRateVolFlowRate(dblVolFlowRate As Double, Optional eUnits As ufrUnitsFlowRateConstants = ufrUnitsFlowRateConstants.ufrNLPerMin)
        mMassRateParameters.VolumetricFlowRate = ConvertVolFlowRate(dblVolFlowRate, eUnits, ufrUnitsFlowRateConstants.ufrMLPerMin)
        ComputeMassRateValues()
    End Sub

    Public Sub SetParticleDiameter(dblParticleDiameter As Double, Optional eUnits As ulnUnitsLengthConstants = ulnUnitsLengthConstants.ulnMicrons)
        mCapillaryFlowParameters.ParticleDiameter = ConvertLength(dblParticleDiameter, eUnits, ulnUnitsLengthConstants.ulnCM)
        CheckAutoCompute()
    End Sub

    Public Sub SetSolventViscosity(dblSolventViscosity As Double, Optional eUnits As uviUnitsViscosityConstants = uviUnitsViscosityConstants.uviPoise)
        mCapillaryFlowParameters.SolventViscosity = ConvertViscosity(dblSolventViscosity, eUnits, uviUnitsViscosityConstants.uviPoise)
        CheckAutoCompute()
    End Sub

    Public Sub SetVolFlowRate(dblVolFlowRate As Double, Optional eUnits As ufrUnitsFlowRateConstants = ufrUnitsFlowRateConstants.ufrNLPerMin)
        mCapillaryFlowParameters.VolumetricFlowRate = ConvertVolFlowRate(dblVolFlowRate, eUnits, ufrUnitsFlowRateConstants.ufrMLPerMin)
        CheckAutoCompute()
    End Sub

    Private Sub InitializeClass()
        Me.SetAutoComputeEnabled(False)

        Me.SetAutoComputeMode(acmAutoComputeModeConstants.acmVolFlowRate)
        Me.SetCapillaryType(ctCapillaryTypeConstants.ctPackedCapillary)
        Me.SetBackPressure(3000, uprUnitsPressureConstants.uprPsi)
        Me.SetColumnLength(50, ulnUnitsLengthConstants.ulnCM)
        Me.SetColumnID(75, ulnUnitsLengthConstants.ulnMicrons)
        Me.SetSolventViscosity(0.0089, uviUnitsViscosityConstants.uviPoise)
        Me.SetParticleDiameter(5, ulnUnitsLengthConstants.ulnMicrons)
        Me.SetInterparticlePorosity(0.4)

        Me.SetMassRateConcentration(1, ucoUnitsConcentrationConstants.ucoMicroMolar)
        Me.SetMassRateVolFlowRate(600, ufrUnitsFlowRateConstants.ufrNLPerMin)
        Me.SetMassRateInjectionTime(5, utmUnitsTimeConstants.utmMinutes)

        ' Recompute
        ComputeVolFlowRate()
        ComputeMassRateValues()
        ComputeExtraColumnBroadeningValues()

        Me.SetAutoComputeEnabled(True)

    End Sub

End Class