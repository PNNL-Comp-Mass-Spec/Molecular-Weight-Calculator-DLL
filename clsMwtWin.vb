Option Strict On

Imports System.Collections.Generic

Public Class MolecularWeightCalculator

    ' Molecular Weight Calculator routines with ActiveX Class interfaces
    ' Based on Molecular Weight Calculator, v6.20 code (VB6), written by Matthew Monroe 1995-2002
    '
    ' ActiveX Dll version written by Matthew Monroe in Richland, WA (2002)
    ' Ported to VB.NET by Nikša Blonder in Richland, WA (2005)

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


    Private Const PROGRAM_DATE As String = "July 14,2016"

    ''' <summary>
    ''' Constructor, assumes the elements are using average masses
    ''' </summary>
    Public Sub New()

        mElementAndMassRoutines = New MWElementAndMassRoutines()

        ' LoadDefaults calls mElementAndMassRoutines.MemoryLoadAll, which is required prior to instantiating the Peptide class.
        ' We need to get the three letter abbreviations defined prior to the Peptide class calling method UpdateStandardMasses
        If Not mDataInitialized Then LoadDefaults()

        Compound = New MWCompoundClass(mElementAndMassRoutines)
        Peptide = New MWPeptideClass(mElementAndMassRoutines)
        FormulaFinder = New MWFormulaFinder(mElementAndMassRoutines)

        CapFlow = New MWCapillaryFlowClass

    End Sub

    ''' <summary>
    ''' Constructor where the element mode can be defined
    ''' </summary>
    ''' <param name="elementMode">Mass mode for elements (average, monoisotopic, or integer)</param>
    Public Sub New(elementMode As MWElementAndMassRoutines.emElementModeConstants)
        Me.New()
        Me.SetElementMode(elementMode)
    End Sub

#Region "Constants and Enums"

    Public Enum arAbbrevRecognitionModeConstants
        arNormalOnly = 0
        arNormalPlusAminoAcids = 1
        arNoAbbreviations = 2
    End Enum

    Public Enum esElementStatsConstants
        esMass = 0
        esUncertainty = 1
        esCharge = 2
    End Enum

#End Region

#Region "Classwide Variables"
    Private mDataInitialized As Boolean

    Public Compound As MWCompoundClass
    Public Peptide As MWPeptideClass
    Public FormulaFinder As MWFormulaFinder
    Public CapFlow As MWCapillaryFlowClass

    Private WithEvents mElementAndMassRoutines As MWElementAndMassRoutines

    Public Event ProgressReset()
    Public Event ProgressChanged(taskDescription As String, percentComplete As Single)     ' PercentComplete ranges from 0 to 100, but can contain decimal percentage values
    Public Event ProgressComplete()

#End Region

#Region "Interface Functions"
    Public Property AbbreviationRecognitionMode() As arAbbrevRecognitionModeConstants
        Get
            Return mElementAndMassRoutines.gComputationOptions.AbbrevRecognitionMode
        End Get
        Set
            If Value >= arAbbrevRecognitionModeConstants.arNormalOnly And Value <= arAbbrevRecognitionModeConstants.arNoAbbreviations Then
                mElementAndMassRoutines.gComputationOptions.AbbrevRecognitionMode = Value
                mElementAndMassRoutines.ConstructMasterSymbolsList()
            End If
        End Set
    End Property

    Public ReadOnly Property AppDate() As String
        Get
            Return PROGRAM_DATE
        End Get
    End Property

    Public ReadOnly Property AppVersion() As String
        Get
            Dim strVersion As String

            Try
                strVersion = Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString()
            Catch ex As Exception
                strVersion = "??.??.??.??"
            End Try

            Return strVersion

        End Get
    End Property

    Public Property BracketsTreatedAsParentheses() As Boolean
        Get
            Return mElementAndMassRoutines.gComputationOptions.BracketsAsParentheses
        End Get
        Set
            mElementAndMassRoutines.gComputationOptions.BracketsAsParentheses = Value
        End Set
    End Property

    Public Property CaseConversionMode() As MWElementAndMassRoutines.ccCaseConversionConstants
        Get
            Return mElementAndMassRoutines.gComputationOptions.CaseConversion
        End Get
        Set
            If Value >= MWElementAndMassRoutines.ccCaseConversionConstants.ccConvertCaseUp And Value <= MWElementAndMassRoutines.ccCaseConversionConstants.ccSmartCase Then
                mElementAndMassRoutines.gComputationOptions.CaseConversion = Value
            End If
        End Set
    End Property

    Public Property DecimalSeparator() As Char
        Get
            Return mElementAndMassRoutines.gComputationOptions.DecimalSeparator
        End Get
        Set
            mElementAndMassRoutines.gComputationOptions.DecimalSeparator = Value
        End Set
    End Property

    Public ReadOnly Property ErrorDescription() As String
        Get
            Return mElementAndMassRoutines.GetErrorDescription()
        End Get
    End Property

    Public ReadOnly Property ErrorID() As Integer
        Get
            Return mElementAndMassRoutines.GetErrorID()
        End Get
    End Property

    Public ReadOnly Property ErrorCharacter() As String
        Get
            Return mElementAndMassRoutines.GetErrorCharacter()
        End Get
    End Property

    Public ReadOnly Property ErrorPosition() As Integer
        Get
            Return mElementAndMassRoutines.GetErrorPosition()
        End Get
    End Property


    Public ReadOnly Property LogFilePath() As String
        Get
            Return mElementAndMassRoutines.LogFilePath
        End Get
    End Property

    Public Property LogFolderPath() As String
        Get
            Return mElementAndMassRoutines.LogFolderPath
        End Get
        Set
            mElementAndMassRoutines.LogFolderPath = Value
        End Set
    End Property

    Public Property LogMessagesToFile() As Boolean
        Get
            Return mElementAndMassRoutines.LogMessagesToFile
        End Get
        Set
            mElementAndMassRoutines.LogMessagesToFile = Value
        End Set
    End Property

    Public Overridable ReadOnly Property ProgressStepDescription() As String
        Get
            Return mElementAndMassRoutines.ProgressStepDescription
        End Get
    End Property

    ''' <summary>
    ''' Percent complete: ranges from 0 to 100, but can contain decimal percentage values
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ProgressPercentComplete() As Single
        Get
            Return mElementAndMassRoutines.ProgressPercentComplete
        End Get
    End Property


    Public Property RtfFontName() As String
        Get
            Return mElementAndMassRoutines.gComputationOptions.RtfFontName
        End Get
        Set
            If Len(Value) > 0 Then
                mElementAndMassRoutines.gComputationOptions.RtfFontName = Value
            End If
        End Set
    End Property


    Public Property RtfFontSize() As Short
        Get
            Return mElementAndMassRoutines.gComputationOptions.RtfFontSize
        End Get
        Set
            If Value > 0 Then
                mElementAndMassRoutines.gComputationOptions.RtfFontSize = Value
            End If
        End Set
    End Property


    Public Property ShowErrorDialogs() As Boolean
        Get
            Return mElementAndMassRoutines.ShowErrorMessageDialogs()
        End Get
        Set
            mElementAndMassRoutines.SetShowErrorMessageDialogs(Value)
        End Set
    End Property


    Public Property StdDevMode() As MWElementAndMassRoutines.smStdDevModeConstants
        Get
            Return mElementAndMassRoutines.gComputationOptions.StdDevMode
        End Get
        Set
            If Value >= MWElementAndMassRoutines.smStdDevModeConstants.smShort And Value <= MWElementAndMassRoutines.smStdDevModeConstants.smDecimal Then
                mElementAndMassRoutines.gComputationOptions.StdDevMode = Value
            End If
        End Set
    End Property
#End Region

    Public Sub ClearError()
        mElementAndMassRoutines.ResetErrorParamsInternal()
    End Sub

    ''' <summary>
    ''' Compute the mass of a formula
    ''' </summary>
    ''' <param name="strFormula"></param>
    ''' <returns>Mass of the formula</returns>
    Public Function ComputeMass(strFormula As String) As Double

        ' Simply assigning strFormula to .Formula will update the Mass
        Compound.Formula = strFormula
        Return Compound.Mass(False)

    End Function


    ''' <summary>
    ''' Computes the Isotopic Distribution for a formula
    ''' </summary>
    ''' <param name="strFormulaIn">Input/output: The properly formatted formula to parse</param>
    ''' <param name="intChargeState">0 for monoisotopic (uncharged) masses; 1 or higher for convoluted m/z values</param>
    ''' <param name="strResults">Output: Table of results</param>
    ''' <param name="ConvolutedMSData2DOneBased">2D array of MSData (mass and intensity pairs)</param>
    ''' <param name="ConvolutedMSDataCount">Number of data points in ConvolutedMSData2DOneBased</param>
    ''' <returns>0 if success, -1 if an error</returns>
    ''' <remarks>
    ''' Returns uncharged mass values if intChargeState=0,
    ''' Returns M+H values if intChargeState=1
    ''' Returns convoluted m/z if intChargeState is > 1
    ''' </remarks>
    Public Function ComputeIsotopicAbundances(
      ByRef strFormulaIn As String, intChargeState As Short, ByRef strResults As String,
      ByRef ConvolutedMSData2DOneBased(,) As Double, ByRef ConvolutedMSDataCount As Integer) As Short

        Return ComputeIsotopicAbundances(strFormulaIn, intChargeState, strResults, ConvolutedMSData2DOneBased, ConvolutedMSDataCount,
                                         "Isotopic Abundances for", "Mass", "Fraction", "Intensity")

    End Function


    ''' <summary>
    ''' Computes the Isotopic Distribution for a formula
    ''' </summary>
    ''' <param name="strFormulaIn">Input/output: The properly formatted formula to parse</param>
    ''' <param name="intChargeState">0 for monoisotopic (uncharged) masses; 1 or higher for convoluted m/z values</param>
    ''' <param name="strResults">Output: Table of results</param>
    ''' <param name="ConvolutedMSData2DOneBased">2D array of MSData (mass and intensity pairs)</param>
    ''' <param name="ConvolutedMSDataCount">Number of data points in ConvolutedMSData2DOneBased</param>
    ''' <param name="blnAddProtonChargeCarrier">If blnAddProtonChargeCarrier is False, then still convolutes by charge, but doesn't add a proton</param>
    ''' <returns>0 if success, -1 if an error</returns>
    ''' <remarks>
    ''' Returns uncharged mass values if intChargeState=0,
    ''' Returns M+H values if intChargeState=1
    ''' Returns convoluted m/z if intChargeState is > 1
    ''' </remarks>
    Public Function ComputeIsotopicAbundances(
      ByRef strFormulaIn As String, intChargeState As Short, ByRef strResults As String,
      ByRef ConvolutedMSData2DOneBased(,) As Double, ByRef ConvolutedMSDataCount As Integer, blnAddProtonChargeCarrier As Boolean) As Short

        Return ComputeIsotopicAbundances(strFormulaIn, intChargeState, strResults, ConvolutedMSData2DOneBased, ConvolutedMSDataCount,
           "Isotopic Abundances for", "Mass", "Fraction", "Intensity", blnAddProtonChargeCarrier)

    End Function

    ''' <summary>
    ''' Computes the Isotopic Distribution for a formula
    ''' </summary>
    ''' <param name="strFormulaIn">Input/output: The properly formatted formula to parse</param>
    ''' <param name="intChargeState">0 for monoisotopic (uncharged) masses; 1 or higher for convoluted m/z values</param>
    ''' <param name="strResults">Output: Table of results</param>
    ''' <param name="ConvolutedMSData2DOneBased">2D array of MSData (mass and intensity pairs)</param>
    ''' <param name="ConvolutedMSDataCount">Number of data points in ConvolutedMSData2DOneBased</param>
    ''' <param name="strHeaderIsotopicAbundances">Header to use in strResults</param>
    ''' <param name="strHeaderMassToCharge">Header to use in strResults</param>
    ''' <param name="strHeaderFraction">Header to use in strResults</param>
    ''' <param name="strHeaderIntensity">Header to use in strResults</param>
    ''' <returns>0 if success, -1 if an error</returns>
    ''' <remarks>
    ''' Returns uncharged mass values if intChargeState=0,
    ''' Returns M+H values if intChargeState=1
    ''' Returns convoluted m/z if intChargeState is > 1
    ''' </remarks>
    Public Function ComputeIsotopicAbundances(
      ByRef strFormulaIn As String, intChargeState As Short, ByRef strResults As String,
      ByRef ConvolutedMSData2DOneBased(,) As Double, ByRef ConvolutedMSDataCount As Integer,
      strHeaderIsotopicAbundances As String,
      strHeaderMassToCharge As String,
      strHeaderFraction As String,
      strHeaderIntensity As String) As Short

        Dim blnAddProtonChargeCarrier = True
        Return mElementAndMassRoutines.ComputeIsotopicAbundancesInternal(strFormulaIn, intChargeState, strResults, ConvolutedMSData2DOneBased, ConvolutedMSDataCount, strHeaderIsotopicAbundances, strHeaderMassToCharge, strHeaderFraction, strHeaderIntensity, False, blnAddProtonChargeCarrier)
    End Function

    ''' <summary>
    ''' Computes the Isotopic Distribution for a formula
    ''' </summary>
    ''' <param name="strFormulaIn">Input/output: The properly formatted formula to parse</param>
    ''' <param name="intChargeState">0 for monoisotopic (uncharged) masses; 1 or higher for convoluted m/z values</param>
    ''' <param name="strResults">Output: Table of results</param>
    ''' <param name="ConvolutedMSData2DOneBased">2D array of MSData (mass and intensity pairs)</param>
    ''' <param name="ConvolutedMSDataCount">Number of data points in ConvolutedMSData2DOneBased</param>
    ''' <param name="strHeaderIsotopicAbundances">Header to use in strResults</param>
    ''' <param name="strHeaderMassToCharge">Header to use in strResults</param>
    ''' <param name="strHeaderFraction">Header to use in strResults</param>
    ''' <param name="strHeaderIntensity">Header to use in strResults</param>
    ''' <param name="blnAddProtonChargeCarrier">If blnAddProtonChargeCarrier is False, then still convolutes by charge, but doesn't add a proton</param>
    ''' <returns>0 if success, -1 if an error</returns>
    ''' <remarks>
    ''' Returns uncharged mass values if intChargeState=0,
    ''' Returns M+H values if intChargeState=1
    ''' Returns convoluted m/z if intChargeState is > 1
    ''' </remarks>
    Public Function ComputeIsotopicAbundances(
      ByRef strFormulaIn As String, intChargeState As Short, ByRef strResults As String,
      ByRef ConvolutedMSData2DOneBased(,) As Double, ByRef ConvolutedMSDataCount As Integer,
      strHeaderIsotopicAbundances As String,
      strHeaderMassToCharge As String,
      strHeaderFraction As String,
      strHeaderIntensity As String,
      blnAddProtonChargeCarrier As Boolean) As Short

        Return mElementAndMassRoutines.ComputeIsotopicAbundancesInternal(strFormulaIn, intChargeState, strResults, ConvolutedMSData2DOneBased, ConvolutedMSDataCount, strHeaderIsotopicAbundances, strHeaderMassToCharge, strHeaderFraction, strHeaderIntensity, False, blnAddProtonChargeCarrier)
    End Function

    ''' <summary>
    ''' Convert the centroided data (stick data) in XYVals to a Gaussian representation
    ''' </summary>
    ''' <param name="XYVals">XY data, as key-value pairs</param>
    ''' <param name="intResolution">Effective instrument resolution (e.g. 1000 or 20000)</param>
    ''' <param name="dblResolutionMass">The m/z value at which the resolution applies</param>
    ''' <returns>Gaussian spectrum data</returns>
    ''' <remarks></remarks>
    Public Function ConvertStickDataToGaussian2DArray(XYVals As List(Of KeyValuePair(Of Double, Double)), intResolution As Integer, dblResolutionMass As Double) As List(Of KeyValuePair(Of Double, Double))
        Dim intQualityFactor As Integer = 50
        Return ConvertStickDataToGaussian2DArray(XYVals, intResolution, dblResolutionMass, intQualityFactor)
    End Function

    ''' <summary>
    ''' Convert the centroided data (stick data) in XYVals to a Gaussian representation
    ''' </summary>
    ''' <param name="XYVals">XY data, as key-value pairs</param>
    ''' <param name="intResolution">Effective instrument resolution (e.g. 1000 or 20000)</param>
    ''' <param name="dblResolutionMass">The m/z value at which the resolution applies</param>
    ''' <param name="intQualityFactor">Gaussian quality factor (between 1 and 75, default is 50)</param>
    ''' <returns>Gaussian spectrum data</returns>
    ''' <remarks></remarks>
    Public Function ConvertStickDataToGaussian2DArray(XYVals As List(Of KeyValuePair(Of Double, Double)), intResolution As Integer, dblResolutionMass As Double, intQualityFactor As Integer) As List(Of KeyValuePair(Of Double, Double))
        Return mElementAndMassRoutines.ConvertStickDataToGaussian2DArray(XYVals, intResolution, dblResolutionMass, intQualityFactor)
    End Function

    ''' <summary>
    ''' Converts a given mass or m/z value to the MH+ m/z value
    ''' </summary>
    ''' <param name="dblMassMZ">Mass or m/z value</param>
    ''' <param name="intCurrentCharge">Current charge (0 means neutral mass)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ConvoluteMass(dblMassMZ As Double, intCurrentCharge As Short) As Double
        Return ConvoluteMass(dblMassMZ, intCurrentCharge, 1S, 0)
    End Function

    ''' <summary>
    ''' Converts a given mass or m/z value to the MH+ m/z value
    ''' </summary>
    ''' <param name="dblMassMZ">Mass or m/z value</param>
    ''' <param name="intCurrentCharge">Current charge (0 means neutral mass)</param>
    ''' <param name="intDesiredCharge">Desired charge (0 means neutral mass)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ConvoluteMass(dblMassMZ As Double, intCurrentCharge As Short, intDesiredCharge As Short) As Double
        Return ConvoluteMass(dblMassMZ, intCurrentCharge, intDesiredCharge, 0)
    End Function

    ''' <summary>
    ''' Converts a given mass or m/z value to the MH+ m/z value
    ''' </summary>
    ''' <param name="dblMassMZ">Mass or m/z value</param>
    ''' <param name="intCurrentCharge">Current charge (0 means neutral mass)</param>
    ''' <param name="intDesiredCharge">Desired charge (0 means neutral mass)</param>
    ''' <param name="dblChargeCarrierMass">Custom charge carrier mass (default is 1.00727649)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ConvoluteMass(dblMassMZ As Double, intCurrentCharge As Short, intDesiredCharge As Short, dblChargeCarrierMass As Double) As Double
        Return mElementAndMassRoutines.ConvoluteMassInternal(dblMassMZ, intCurrentCharge, intDesiredCharge, dblChargeCarrierMass)
    End Function

    ''' <summary>
    ''' Determine the decimal point symbol (period or comma)
    ''' </summary>
    ''' <returns></returns>
    Friend Shared Function DetermineDecimalPoint() As Char
        Dim strTestNumber As String
        Dim sglConversionResult As Double

        ' In VB6, the Trim(Str(Cdbl(...))) statement causes an error when the
        '  user's computer is configured for using , for decimal points but not . for the
        '  thousand's separator (instead, perhaps, using a space for thousands)
        ' Not sure of the behavior in VB.NET

        Try
            ' Determine what locale we're in (. or , for decimal point)
            strTestNumber = "5,500"
            sglConversionResult = CDbl(strTestNumber)
            If Math.Abs(sglConversionResult - 5.5) < Single.Epsilon Then
                ' Use comma as Decimal point
                Return ","c
            Else
                ' Use period as Decimal point
                Return "."c
            End If
        Catch ex As Exception
            Return "."c
        End Try

    End Function

    ''' <summary>
    ''' Get an abbreviation, by ID
    ''' </summary>
    ''' <param name="intAbbreviationID"></param>
    ''' <param name="strSymbol">Output: symbol</param>
    ''' <param name="strFormula">Output: empirical formula</param>
    ''' <param name="sngCharge">Output: charge</param>
    ''' <param name="blnIsAminoAcid">Output: true if an amino acid</param>
    ''' <returns> 0 if success, 1 if failure</returns>
    Public Function GetAbbreviation(intAbbreviationID As Integer, ByRef strSymbol As String,
            ByRef strFormula As String, ByRef sngCharge As Single,
            ByRef blnIsAminoAcid As Boolean) As Integer

        Return GetAbbreviation(intAbbreviationID, strSymbol, strFormula, sngCharge, blnIsAminoAcid, "", "", False)
    End Function

    ''' <summary>
    ''' Get an abbreviation, by ID
    ''' </summary>
    ''' <param name="intAbbreviationID"></param>
    ''' <param name="strSymbol">Output: symbol</param>
    ''' <param name="strFormula">Output: empirical formula</param>
    ''' <param name="sngCharge">Output: charge</param>
    ''' <param name="blnIsAminoAcid">Output: true if an amino acid</param>
    ''' <param name="strOneLetterSymbol">Output: one letter symbol (only used by amino acids)</param>
    ''' <param name="strComment">Output: comment</param>
    ''' <returns> 0 if success, 1 if failure</returns>
    Public Function GetAbbreviation(intAbbreviationID As Integer, ByRef strSymbol As String,
           ByRef strFormula As String, ByRef sngCharge As Single,
           ByRef blnIsAminoAcid As Boolean,
           ByRef strOneLetterSymbol As String,
           ByRef strComment As String) As Integer

        Return GetAbbreviation(intAbbreviationID, strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, strComment, False)
    End Function

    ''' <summary>
    ''' Get an abbreviation, by ID
    ''' </summary>
    ''' <param name="intAbbreviationID"></param>
    ''' <param name="strSymbol">Output: symbol</param>
    ''' <param name="strFormula">Output: empirical formula</param>
    ''' <param name="sngCharge">Output: charge</param>
    ''' <param name="blnIsAminoAcid">Output: true if an amino acid</param>
    ''' <param name="strOneLetterSymbol">Output: one letter symbol (only used by amino acids)</param>
    ''' <param name="strComment">Output: comment</param>
    ''' <param name="blnInvalidSymbolOrFormula">Output: true if an invalid symbol or formula</param>
    ''' <returns> 0 if success, 1 if failure</returns>
    Public Function GetAbbreviation(intAbbreviationID As Integer, ByRef strSymbol As String,
            ByRef strFormula As String, ByRef sngCharge As Single,
            ByRef blnIsAminoAcid As Boolean,
            ByRef strOneLetterSymbol As String,
            ByRef strComment As String,
            ByRef blnInvalidSymbolOrFormula As Boolean) As Integer

        Return mElementAndMassRoutines.GetAbbreviationInternal(
            intAbbreviationID, strSymbol, strFormula,
            sngCharge, blnIsAminoAcid, strOneLetterSymbol,
            strComment, blnInvalidSymbolOrFormula)
    End Function

    ''' <summary>
    ''' Get the number of abbreviations in memory
    ''' </summary>
    ''' <returns></returns>
    Public Function GetAbbreviationCount() As Integer
        Return mElementAndMassRoutines.GetAbbreviationCountInternal()
    End Function

    Public Function GetAbbreviationCountMax() As Integer
        Return MWElementAndMassRoutines.MAX_ABBREV_COUNT
    End Function

    ''' <summary>
    ''' Get the abbreviation ID for the given abbreviation symbol
    ''' </summary>
    ''' <param name="strSymbol"></param>
    ''' <returns>ID if found, otherwise 0</returns>
    Public Function GetAbbreviationID(strSymbol As String) As Integer
        Return mElementAndMassRoutines.GetAbbreviationIDInternal(strSymbol)
    End Function

    Public Function GetAminoAcidSymbolConversion(strSymbolToFind As String, bln1LetterTo3Letter As Boolean) As String
        ' If bln1LetterTo3Letter = True, then converting 1 letter codes to 3 letter codes
        ' Returns the symbol, if found
        ' Otherwise, returns ""
        Return mElementAndMassRoutines.GetAminoAcidSymbolConversionInternal(strSymbolToFind, bln1LetterTo3Letter)
    End Function

    ''' <summary>
    ''' Get caution statement information
    ''' </summary>
    ''' <param name="intCautionStatementID"></param>
    ''' <param name="strSymbolCombo">Output: symbol combo for the caution statement</param>
    ''' <param name="strCautionStatement">Output: caution statement text</param>
    ''' <returns>0 if success, 1 if an invalid ID</returns>
    Public Function GetCautionStatement(intCautionStatementID As Integer, ByRef strSymbolCombo As String, ByRef strCautionStatement As String) As Integer
        Return mElementAndMassRoutines.GetCautionStatementInternal(intCautionStatementID, strSymbolCombo, strCautionStatement)
    End Function

    ''' <summary>
    ''' Get the number of Caution Statements in memory
    ''' </summary>
    ''' <returns></returns>
    Public Function GetCautionStatementCount() As Integer
        Return mElementAndMassRoutines.GetCautionStatementCountInternal()
    End Function

    ''' <summary>
    ''' Get the caution statement ID for the given symbol combo
    ''' </summary>
    ''' <param name="strSymbolCombo"></param>
    ''' <returns>Statement ID if found, otherwise -1</returns>
    Public Function GetCautionStatementID(strSymbolCombo As String) As Integer
        Return mElementAndMassRoutines.GetCautionStatementIDInternal(strSymbolCombo)
    End Function

    Public Function GetChargeCarrierMass() As Double
        Return mElementAndMassRoutines.GetChargeCarrierMassInternal()
    End Function

    ''' <summary>
    ''' Returns the settings for the element with intElementID in the ByRef variables
    ''' </summary>
    ''' <param name="intElementID"></param>
    ''' <param name="strSymbol"></param>
    ''' <param name="dblMass"></param>
    ''' <param name="dblUncertainty"></param>
    ''' <param name="sngCharge"></param>
    ''' <param name="intIsotopeCount"></param>
    ''' <returns>0 if success, 1 if failure</returns>
    Public Function GetElement(intElementID As Short, ByRef strSymbol As String, ByRef dblMass As Double, ByRef dblUncertainty As Double, ByRef sngCharge As Single, ByRef intIsotopeCount As Short) As Integer
        Return mElementAndMassRoutines.GetElementInternal(intElementID, strSymbol, dblMass, dblUncertainty, sngCharge, intIsotopeCount)
    End Function

    ''' <summary>
    ''' Returns the number of elements in memory
    ''' </summary>
    ''' <returns></returns>
    Public Function GetElementCount() As Integer
        Return mElementAndMassRoutines.GetElementCountInternal()
    End Function

    ''' <summary>
    ''' Get the element ID for the given symbol
    ''' </summary>
    ''' <param name="strSymbol"></param>
    ''' <returns>ID if found, otherwise 0</returns>
    Public Function GetElementID(strSymbol As String) As Integer
        Return mElementAndMassRoutines.GetElementIDInternal(strSymbol)
    End Function

    ''' <summary>
    ''' Returns the isotope masses and abundances for the element with intElementID
    ''' </summary>
    ''' <param name="intElementID"></param>
    ''' <param name="intIsotopeCount"></param>
    ''' <param name="dblIsotopeMasses"></param>
    ''' <param name="sngIsotopeAbundances"></param>
    ''' <returns>0 if a valid ID, 1 if invalid</returns>
    Public Function GetElementIsotopes(intElementID As Short, ByRef intIsotopeCount As Short, ByRef dblIsotopeMasses() As Double, ByRef sngIsotopeAbundances() As Single) As Integer
        Return mElementAndMassRoutines.GetElementIsotopesInternal(intElementID, intIsotopeCount, dblIsotopeMasses, sngIsotopeAbundances)
    End Function

    ''' <summary>
    ''' Get the current element mode
    ''' </summary>
    ''' <returns>
    ''' emAverageMass  = 1
    ''' emIsotopicMass = 2
    ''' emIntegerMass  = 3
    ''' </returns>
    Public Function GetElementMode() As MWElementAndMassRoutines.emElementModeConstants
        Return mElementAndMassRoutines.GetElementModeInternal()
    End Function

    ''' <summary>
    ''' Return the element symbol for the given element ID
    ''' </summary>
    ''' <param name="intElementID"></param>
    ''' <returns></returns>
    ''' <remarks>1 is Hydrogen, 2 is Helium, etc.</remarks>
    Public Function GetElementSymbol(intElementID As Short) As String
        Return mElementAndMassRoutines.GetElementSymbolInternal(intElementID)
    End Function

    ''' <summary>
    ''' Returns a single bit of information about a single element
    ''' </summary>
    ''' <param name="intElementID">Element ID</param>
    ''' <param name="eElementStat">Value to obtain: mass, charge, or uncertainty</param>
    ''' <returns></returns>
    ''' <remarks>Since a value may be negative, simply returns 0 if an error</remarks>
    Public Function GetElementStat(intElementID As Short, eElementStat As esElementStatsConstants) As Double
        Return mElementAndMassRoutines.GetElementStatInternal(intElementID, eElementStat)
    End Function

    ''' <summary>
    ''' Get message text using message ID
    ''' </summary>
    ''' <param name="messageID"></param>
    ''' <returns></returns>
    Public Function GetMessageStatement(messageID As Integer) As String
        Return GetMessageStatement(messageID, String.Empty)
    End Function

    ''' <summary>
    ''' Get message text using message ID
    ''' </summary>
    ''' <param name="messageID"></param>
    ''' <param name="strAppendText"></param>
    ''' <returns></returns>
    Public Function GetMessageStatement(messageID As Integer, strAppendText As String) As String
        Return mElementAndMassRoutines.GetMessageStatementInternal(messageID, strAppendText)
    End Function

    Public Function GetMessageStatementCount() As Integer
        Return mElementAndMassRoutines.GetMessageStatementCountInternal()
    End Function

    ''' <summary>
    ''' Returns True if the first letter of strTestChar is a ModSymbol
    ''' </summary>
    ''' <param name="strSymbol"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' Invalid Mod Symbols are letters, numbers, ., -, space, (, or )
    ''' Valid Mod Symbols are ! # $ % ampersand ' * + ? ^ ` ~
    ''' </remarks>
    Public Function IsModSymbol(strSymbol As String) As Boolean
        Return mElementAndMassRoutines.IsModSymbolInternal(strSymbol)
    End Function

    Private Sub LoadDefaults()
        mElementAndMassRoutines.MemoryLoadAll(MWElementAndMassRoutines.emElementModeConstants.emAverageMass)

        Me.SetElementMode(MWElementAndMassRoutines.emElementModeConstants.emAverageMass)
        Me.AbbreviationRecognitionMode = arAbbrevRecognitionModeConstants.arNormalPlusAminoAcids
        Me.BracketsTreatedAsParentheses = True
        Me.CaseConversionMode = MWElementAndMassRoutines.ccCaseConversionConstants.ccConvertCaseUp
        Me.DecimalSeparator = "."c
        Me.RtfFontName = "Arial"
        Me.RtfFontSize = 10
        Me.StdDevMode = MWElementAndMassRoutines.smStdDevModeConstants.smDecimal

        mElementAndMassRoutines.gComputationOptions.DecimalSeparator = DetermineDecimalPoint()

        mDataInitialized = True
    End Sub

    Public Sub RemoveAllAbbreviations()
        mElementAndMassRoutines.RemoveAllAbbreviationsInternal()
    End Sub

    Public Sub RemoveAllCautionStatements()
        mElementAndMassRoutines.RemoveAllCautionStatementsInternal()
    End Sub

    Public Function MassToPPM(dblMassToConvert As Double, dblCurrentMZ As Double) As Double
        Return mElementAndMassRoutines.MassToPPMInternal(dblMassToConvert, dblCurrentMZ)
    End Function

    Public Function MonoMassToMZ(dblMonoisotopicMass As Double, intCharge As Short) As Double
        Return MonoMassToMZ(dblMonoisotopicMass, intCharge, 0)
    End Function

    Public Function MonoMassToMZ(dblMonoisotopicMass As Double, intCharge As Short, dblChargeCarrierMass As Double) As Double
        Return mElementAndMassRoutines.MonoMassToMZInternal(dblMonoisotopicMass, intCharge, dblChargeCarrierMass)
    End Function

    ''' <summary>
    ''' Recomputes the Mass for all of the loaded abbreviations
    ''' </summary>
    ''' <remarks>
    ''' Useful if we just finished setting lots of element masses, and
    ''' had blnRecomputeAbbreviationMasses = False when calling .SetElement()
    ''' </remarks>
    Public Sub RecomputeAbbreviationMasses()
        mElementAndMassRoutines.RecomputeAbbreviationMassesInternal()
    End Sub

    Public Function RemoveAbbreviation(strAbbreviationSymbol As String) As Integer
        Return mElementAndMassRoutines.RemoveAbbreviationInternal(strAbbreviationSymbol)
    End Function

    Public Function RemoveAbbreviationByID(intAbbreviationID As Integer) As Integer
        Return mElementAndMassRoutines.RemoveAbbreviationByIDInternal(intAbbreviationID)
    End Function

    Public Function RemoveCautionStatement(strCautionSymbol As String) As Integer
        Return mElementAndMassRoutines.RemoveCautionStatementInternal(strCautionSymbol)
    End Function

    Public Sub ResetAbbreviations()
        mElementAndMassRoutines.MemoryLoadAbbreviations()
    End Sub

    Public Sub ResetCautionStatements()
        mElementAndMassRoutines.MemoryLoadCautionStatements()
    End Sub

    Public Sub ResetElement(intElementID As Short, eSpecificStatToReset As esElementStatsConstants)
        mElementAndMassRoutines.MemoryLoadElements(GetElementMode(), intElementID, eSpecificStatToReset)
    End Sub

    Public Sub ResetMessageStatements()
        mElementAndMassRoutines.MemoryLoadMessageStatements()
    End Sub

    Public Function SetAbbreviation(strSymbol As String, strFormula As String, sngCharge As Single,
           blnIsAminoAcid As Boolean) As Integer
        Return SetAbbreviation(strSymbol, strFormula, sngCharge, blnIsAminoAcid, "", "", True)
    End Function

    Public Function SetAbbreviation(strSymbol As String, strFormula As String, sngCharge As Single,
            blnIsAminoAcid As Boolean,
            strOneLetterSymbol As String,
            strComment As String) As Integer
        Return SetAbbreviation(strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, strComment, True)
    End Function

    ''' <summary>
    ''' Adds a new abbreviation or updates an existing one (based on strSymbol)
    ''' </summary>
    ''' <param name="strSymbol"></param>
    ''' <param name="strFormula"></param>
    ''' <param name="sngCharge"></param>
    ''' <param name="blnIsAminoAcid"></param>
    ''' <param name="strOneLetterSymbol"></param>
    ''' <param name="strComment"></param>
    ''' <param name="blnValidateFormula">If true, make sure the formula is valid</param>
    ''' <returns>0 if successful, otherwise an error ID</returns>
    ''' <remarks>
    ''' It is useful to set blnValidateFormula = False when you're defining all of the abbreviations at once,
    '''  since one abbreviation can depend upon another, and if the second abbreviation hasn't yet been
    '''  defined, then the parsing of the first abbreviation will fail
    ''' </remarks>
    Public Function SetAbbreviation(
      strSymbol As String, strFormula As String, sngCharge As Single,
      blnIsAminoAcid As Boolean,
      strOneLetterSymbol As String,
      strComment As String,
      blnValidateFormula As Boolean) As Integer
        Return mElementAndMassRoutines.SetAbbreviationInternal(strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, strComment, blnValidateFormula)
    End Function

    Public Function SetAbbreviationByID(
      intAbbrevID As Integer, strSymbol As String, strFormula As String,
      sngCharge As Single, blnIsAminoAcid As Boolean) As Integer

        Return SetAbbreviationByID(intAbbrevID, strSymbol, strFormula, sngCharge, blnIsAminoAcid, "", "", True)
    End Function

    ''' <summary>
    ''' Adds a new abbreviation or updates an existing one (based on intAbbrevID)
    ''' </summary>
    ''' <param name="intAbbrevID">If intAbbrevID is less than 1, adds as a new abbreviation</param>
    ''' <param name="strSymbol"></param>
    ''' <param name="strFormula"></param>
    ''' <param name="sngCharge"></param>
    ''' <param name="blnIsAminoAcid"></param>
    ''' <param name="strOneLetterSymbol"></param>
    ''' <param name="strComment"></param>
    ''' <param name="blnValidateFormula">If true, make sure the formula is valid</param>
    ''' <returns>0 if successful, otherwise an error ID</returns>
    ''' <remarks>
    ''' It is useful to set blnValidateFormula = False when you're defining all of the abbreviations at once,
    '''  since one abbreviation can depend upon another, and if the second abbreviation hasn't yet been
    '''  defined, then the parsing of the first abbreviation will fail
    ''' </remarks>
    Public Function SetAbbreviationByID(
      intAbbrevID As Integer, strSymbol As String, strFormula As String,
      sngCharge As Single, blnIsAminoAcid As Boolean,
      strOneLetterSymbol As String,
      strComment As String,
      blnValidateFormula As Boolean) As Integer
        Return mElementAndMassRoutines.SetAbbreviationByIDInternal(CShort(intAbbrevID), strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, strComment, blnValidateFormula)
    End Function

    ''' <summary>
    ''' Adds a new caution statement or updates an existing one (based on strSymbolCombo)
    ''' </summary>
    ''' <param name="strSymbolCombo"></param>
    ''' <param name="strNewCautionStatement"></param>
    ''' <returns>0 if successful, otherwise, returns an Error ID</returns>
    Public Function SetCautionStatement(strSymbolCombo As String, strNewCautionStatement As String) As Integer
        Return mElementAndMassRoutines.SetCautionStatementInternal(strSymbolCombo, strNewCautionStatement)
    End Function

    Public Sub SetChargeCarrierMass(dblMass As Double)
        mElementAndMassRoutines.SetChargeCarrierMassInternal(dblMass)
    End Sub

    Public Function SetElement(strSymbol As String, dblMass As Double,
             dblUncertainty As Double, sngCharge As Single) As Integer
        Return SetElement(strSymbol, dblMass, dblUncertainty, sngCharge, True)
    End Function

    ''' <summary>
    ''' Update the values for a single element (based on strSymbol)
    ''' </summary>
    ''' <param name="strSymbol"></param>
    ''' <param name="dblMass"></param>
    ''' <param name="dblUncertainty"></param>
    ''' <param name="sngCharge"></param>
    ''' <param name="blnRecomputeAbbreviationMasses">Set to False if updating several elements</param>
    ''' <returns></returns>
    Public Function SetElement(strSymbol As String, dblMass As Double, dblUncertainty As Double,
             sngCharge As Single,
             blnRecomputeAbbreviationMasses As Boolean) As Integer

        Return mElementAndMassRoutines.SetElementInternal(strSymbol, dblMass, dblUncertainty, sngCharge, blnRecomputeAbbreviationMasses)
    End Function

    Public Function SetElementIsotopes(strSymbol As String, intIsotopeCount As Short, ByRef dblIsotopeMassesOneBased() As Double, ByRef sngIsotopeAbundancesOneBased() As Single) As Integer
        Return mElementAndMassRoutines.SetElementIsotopesInternal(strSymbol, intIsotopeCount, dblIsotopeMassesOneBased, sngIsotopeAbundancesOneBased)
    End Function

    Public Sub SetElementMode(elementMode As MWElementAndMassRoutines.emElementModeConstants)
        SetElementMode(elementMode, True)
    End Sub

    Public Sub SetElementMode(elementMode As MWElementAndMassRoutines.emElementModeConstants, blnMemoryLoadElementValues As Boolean)
        mElementAndMassRoutines.SetElementModeInternal(elementMode, blnMemoryLoadElementValues)
    End Sub

    ''' <summary>
    ''' Used to replace the default message strings with foreign language equivalent ones
    ''' </summary>
    ''' <param name="messageID"></param>
    ''' <param name="strNewMessage"></param>
    ''' <returns>0 if success; 1 if failure</returns>
    Public Function SetMessageStatement(messageID As Integer, strNewMessage As String) As Integer
        Return mElementAndMassRoutines.SetMessageStatementInternal(messageID, strNewMessage)
    End Function

    Public Sub SortAbbreviations()
        mElementAndMassRoutines.SortAbbreviationsInternal()
    End Sub

    Public Function TextToRTF(strTextToConvert As String) As String
        Return TextToRTF(strTextToConvert, False, True, False, 0)
    End Function

    Public Function TextToRTF(strTextToConvert As String, CalculatorMode As Boolean) As String
        Return TextToRTF(strTextToConvert, CalculatorMode, True, False, 0)
    End Function

    Public Function TextToRTF(strTextToConvert As String, CalculatorMode As Boolean,
            blnHighlightCharFollowingPercentSign As Boolean) As String
        Return TextToRTF(strTextToConvert, CalculatorMode, blnHighlightCharFollowingPercentSign, False, 0)
    End Function

    ''' <summary>
    ''' Converts plain text to formatted rtf text
    ''' </summary>
    ''' <param name="strTextToConvert"></param>
    ''' <param name="calculatorMode">When true, does not superscript + signs and numbers following + signs</param>
    ''' <param name="blnHighlightCharFollowingPercentSign">When true, change the character following a percent sign to red (and remove the percent sign)</param>
    ''' <param name="blnOverrideErrorID"></param>
    ''' <param name="errorIDOverride"></param>
    ''' <returns></returns>
    Public Function TextToRTF(
      strTextToConvert As String,
      CalculatorMode As Boolean,
      blnHighlightCharFollowingPercentSign As Boolean,
      blnOverrideErrorID As Boolean,
      errorIDOverride As Integer) As String
        Return mElementAndMassRoutines.PlainTextToRtfInternal(strTextToConvert, CalculatorMode, blnHighlightCharFollowingPercentSign, blnOverrideErrorID, errorIDOverride)
    End Function

    ''' <summary>
    ''' Checks the formula of all abbreviations to make sure it's valid
    ''' Marks any abbreviations as Invalid if a problem is found or a circular reference exists
    ''' </summary>
    ''' <returns>Count of the number of invalid abbreviations found</returns>
    Public Function ValidateAllAbbreviations() As Integer
        Return mElementAndMassRoutines.ValidateAllAbbreviationsInternal()
    End Function

    Protected Overrides Sub Finalize()
        Peptide = Nothing
        CapFlow = Nothing
        Compound = Nothing

        MyBase.Finalize()
    End Sub

    Private Sub mElementAndMassRoutines_ProgressChanged(taskDescription As String, percentComplete As Single) Handles mElementAndMassRoutines.ProgressChanged
        RaiseEvent ProgressChanged(taskDescription, percentComplete)
    End Sub

    Private Sub mElementAndMassRoutines_ProgressComplete() Handles mElementAndMassRoutines.ProgressComplete
        RaiseEvent ProgressComplete()
    End Sub

    Private Sub mElementAndMassRoutines_ProgressReset() Handles mElementAndMassRoutines.ProgressReset
        RaiseEvent ProgressReset()
    End Sub
End Class