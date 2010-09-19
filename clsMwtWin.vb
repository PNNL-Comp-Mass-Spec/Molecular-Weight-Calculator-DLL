Option Strict On

Public Class MolecularWeightCalculator

    ' Molecular Weight Calculator routines with ActiveX Class interfaces
    ' Based on Molecular Weight Calculator, v6.20 code (VB6), written by Matthew Monroe 1995-2002
    '
    ' ActiveX Dll version written by Matthew Monroe in Richland, WA (2002)
    ' Ported to VB.NET by Nik�a Blonder in Richland, WA (2005)

    ' -------------------------------------------------------------------------------
    ' Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
    ' E-mail: matthew.monroe@pnl.gov or matt@alchemistmatt.com
    ' Website: http://ncrr.pnl.gov/ or http://www.sysbio.org/resources/staff/
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


    Private Const PROGRAM_DATE As String = "March 19, 2010"

    Public Sub New()
        MyBase.New()

        mElementAndMassRoutines = New MWElementAndMassRoutines

        Compound = New MWCompoundClass(mElementAndMassRoutines)
        Peptide = New MWPeptideClass(mElementAndMassRoutines)

        CapFlow = New MWCapillaryFlowClass
        If Not mDataInitialized Then LoadDefaults()

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

    Public CapFlow As MWCapillaryFlowClass

    Private WithEvents mElementAndMassRoutines As MWElementAndMassRoutines

    Public Event ProgressReset()
    Public Event ProgressChanged(ByVal taskDescription As String, ByVal percentComplete As Single)     ' PercentComplete ranges from 0 to 100, but can contain decimal percentage values
    Public Event ProgressComplete()

#End Region

#Region "Interface Functions"
    Public Property AbbreviationRecognitionMode() As arAbbrevRecognitionModeConstants
        Get
            Return mElementAndMassRoutines.gComputationOptions.AbbrevRecognitionMode
        End Get
        Set(ByVal Value As arAbbrevRecognitionModeConstants)
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
                strVersion = System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString()
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
        Set(ByVal Value As Boolean)
            mElementAndMassRoutines.gComputationOptions.BracketsAsParentheses = Value
        End Set
    End Property


    Public Property CaseConversionMode() As MWElementAndMassRoutines.ccCaseConversionConstants
        Get
            Return mElementAndMassRoutines.gComputationOptions.CaseConversion
        End Get
        Set(ByVal Value As MWElementAndMassRoutines.ccCaseConversionConstants)
            If Value >= MWElementAndMassRoutines.ccCaseConversionConstants.ccConvertCaseUp And Value <= MWElementAndMassRoutines.ccCaseConversionConstants.ccSmartCase Then
                mElementAndMassRoutines.gComputationOptions.CaseConversion = Value
            End If
        End Set
    End Property


    Public Property DecimalSeparator() As Char
        Get
            Return mElementAndMassRoutines.gComputationOptions.DecimalSeparator
        End Get
        Set(ByVal Value As Char)
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
        Set(ByVal value As String)
            mElementAndMassRoutines.LogFolderPath = value
        End Set
    End Property

    Public Property LogMessagesToFile() As Boolean
        Get
            Return mElementAndMassRoutines.LogMessagesToFile
        End Get
        Set(ByVal value As Boolean)
            mElementAndMassRoutines.LogMessagesToFile = value
        End Set
    End Property

    Public Overridable ReadOnly Property ProgressStepDescription() As String
        Get
            Return mElementAndMassRoutines.ProgressStepDescription
        End Get
    End Property

    ' ProgressPercentComplete ranges from 0 to 100, but can contain decimal percentage values
    Public ReadOnly Property ProgressPercentComplete() As Single
        Get
            Return mElementAndMassRoutines.ProgressPercentComplete
        End Get
    End Property


    Public Property RtfFontName() As String
        Get
            Return mElementAndMassRoutines.gComputationOptions.RtfFontName
        End Get
        Set(ByVal Value As String)
            If Len(Value) > 0 Then
                mElementAndMassRoutines.gComputationOptions.RtfFontName = Value
            End If
        End Set
    End Property


    Public Property RtfFontSize() As Short
        Get
            Return mElementAndMassRoutines.gComputationOptions.RtfFontSize
        End Get
        Set(ByVal Value As Short)
            If Value > 0 Then
                mElementAndMassRoutines.gComputationOptions.RtfFontSize = Value
            End If
        End Set
    End Property


    Public Property ShowErrorDialogs() As Boolean
        Get
            Return mElementAndMassRoutines.ShowErrorMessageDialogs()
        End Get
        Set(ByVal Value As Boolean)
            mElementAndMassRoutines.SetShowErrorMessageDialogs(Value)
        End Set
    End Property


    Public Property StdDevMode() As MWElementAndMassRoutines.smStdDevModeConstants
        Get
            Return mElementAndMassRoutines.gComputationOptions.StdDevMode
        End Get
        Set(ByVal Value As MWElementAndMassRoutines.smStdDevModeConstants)
            If Value >= MWElementAndMassRoutines.smStdDevModeConstants.smShort And Value <= MWElementAndMassRoutines.smStdDevModeConstants.smDecimal Then
                mElementAndMassRoutines.gComputationOptions.StdDevMode = Value
            End If
        End Set
    End Property
#End Region

    Public Sub ClearError()
        mElementAndMassRoutines.ResetErrorParamsInternal()
    End Sub

    Public Function ComputeMass(ByVal strFormula As String) As Double

        ' Simply assigning strFormula to .Formula will update the Mass
        Compound.Formula = strFormula
        Return Compound.Mass(False)

    End Function

    Public Function ComputeIsotopicAbundances(ByRef strFormulaIn As String, ByVal intChargeState As Short, ByRef strResults As String, _
                                              ByRef ConvolutedMSData2DOneBased(,) As Double, ByRef ConvolutedMSDataCount As Integer) As Short

        Return ComputeIsotopicAbundances(strFormulaIn, intChargeState, strResults, ConvolutedMSData2DOneBased, ConvolutedMSDataCount, _
                                         "Isotopic Abundances for", "Mass", "Fraction", "Intensity")

    End Function

    Public Function ComputeIsotopicAbundances(ByRef strFormulaIn As String, ByVal intChargeState As Short, ByRef strResults As String, _
                                              ByRef ConvolutedMSData2DOneBased(,) As Double, ByRef ConvolutedMSDataCount As Integer, _
                                              ByVal strHeaderIsotopicAbundances As String, _
                                              ByVal strHeaderMass As String, _
                                              ByVal strHeaderFraction As String, _
                                              ByVal strHeaderIntensity As String) As Short

        ' Computes the Isotopic Distribution for a formula
        ' Returns 0 if success, or -1 if an error

        Return mElementAndMassRoutines.ComputeIsotopicAbundancesInternal(strFormulaIn, intChargeState, strResults, ConvolutedMSData2DOneBased, ConvolutedMSDataCount, strHeaderIsotopicAbundances, strHeaderMass, strHeaderFraction, strHeaderIntensity, False)
    End Function

    ''' <summary>
    ''' Converts a given mass or m/z value to the MH+ m/z value
    ''' </summary>
    ''' <param name="dblMassMZ">Mass or m/z value</param>
    ''' <param name="intCurrentCharge">Current charge (0 means neutral mass)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ConvoluteMass(ByVal dblMassMZ As Double, ByVal intCurrentCharge As Short) As Double
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
    Public Function ConvoluteMass(ByVal dblMassMZ As Double, ByVal intCurrentCharge As Short, ByVal intDesiredCharge As Short) As Double
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
    Public Function ConvoluteMass(ByVal dblMassMZ As Double, ByVal intCurrentCharge As Short, ByVal intDesiredCharge As Short, ByVal dblChargeCarrierMass As Double) As Double
        Return mElementAndMassRoutines.ConvoluteMassInternal(dblMassMZ, intCurrentCharge, intDesiredCharge, dblChargeCarrierMass)
    End Function

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
            If sglConversionResult = 5.5 Then
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

    Public Function GetAbbreviation(ByVal intAbbreviationID As Integer, ByRef strSymbol As String, _
                                    ByRef strFormula As String, ByRef sngCharge As Single, _
                                    ByRef blnIsAminoAcid As Boolean) As Integer

        Return GetAbbreviation(intAbbreviationID, strSymbol, strFormula, sngCharge, blnIsAminoAcid, "", "", False)
    End Function

    Public Function GetAbbreviation(ByVal intAbbreviationID As Integer, ByRef strSymbol As String, _
                                ByRef strFormula As String, ByRef sngCharge As Single, _
                                ByRef blnIsAminoAcid As Boolean, _
                                ByRef strOneLetterSymbol As String, _
                                ByRef strComment As String) As Integer

        Return GetAbbreviation(intAbbreviationID, strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, strComment, False)
    End Function

    Public Function GetAbbreviation(ByVal intAbbreviationID As Integer, ByRef strSymbol As String, _
                                    ByRef strFormula As String, ByRef sngCharge As Single, _
                                    ByRef blnIsAminoAcid As Boolean, _
                                    ByRef strOneLetterSymbol As String, _
                                    ByRef strComment As String, _
                                    ByRef blnInvalidSymbolOrFormula As Boolean) As Integer

        ' Returns 0 if success, 1 if failure
        GetAbbreviation = mElementAndMassRoutines.GetAbbreviationInternal(intAbbreviationID, strSymbol, strFormula, _
                                                                          sngCharge, blnIsAminoAcid, strOneLetterSymbol, _
                                                                          strComment, blnInvalidSymbolOrFormula)
    End Function

    Public Function GetAbbreviationCount() As Integer
        GetAbbreviationCount = mElementAndMassRoutines.GetAbbreviationCountInternal()
    End Function

    Public Function GetAbbreviationCountMax() As Integer
        GetAbbreviationCountMax = MWElementAndMassRoutines.MAX_ABBREV_COUNT
    End Function

    Public Function GetAbbreviationID(ByVal strSymbol As String) As Integer
        ' Returns 0 if not found, the ID if found
        GetAbbreviationID = mElementAndMassRoutines.GetAbbreviationIDInternal(strSymbol)
    End Function

    Public Function GetAminoAcidSymbolConversion(ByVal strSymbolToFind As String, ByVal bln1LetterTo3Letter As Boolean) As String
        ' If bln1LetterTo3Letter = True, then converting 1 letter codes to 3 letter codes
        ' Returns the symbol, if found
        ' Otherwise, returns ""
        Return mElementAndMassRoutines.GetAminoAcidSymbolConversionInternal(strSymbolToFind, bln1LetterTo3Letter)
    End Function

    Public Function GetCautionStatement(ByVal intCautionStatementID As Integer, ByRef strSymbolCombo As String, ByRef strCautionStatement As String) As Integer
        ' Returns the contents of CautionStatements() in the ByRef variables
        ' Returns 0 if success, 1 if failure
        GetCautionStatement = mElementAndMassRoutines.GetCautionStatementInternal(intCautionStatementID, strSymbolCombo, strCautionStatement)
    End Function

    Public Function GetCautionStatementCount() As Integer
        ' Returns the number of Caution Statements in memory
        GetCautionStatementCount = mElementAndMassRoutines.GetCautionStatementCountInternal()
    End Function

    Public Function GetCautionStatementID(ByVal strSymbolCombo As String) As Integer
        ' Returns -1 if not found, the ID if found
        GetCautionStatementID = mElementAndMassRoutines.GetCautionStatementIDInternal(strSymbolCombo)
    End Function

    Public Function GetChargeCarrierMass() As Double
        GetChargeCarrierMass = mElementAndMassRoutines.GetChargeCarrierMassInternal()
    End Function

    Public Function GetElement(ByVal intElementID As Short, ByRef strSymbol As String, ByRef dblMass As Double, ByRef dblUncertainty As Double, ByRef sngCharge As Single, ByRef intIsotopeCount As Short) As Integer
        ' Returns the settings for the element with intElementID in the ByRef variables
        ' Returns 0 if success, 1 if failure
        GetElement = mElementAndMassRoutines.GetElementInternal(intElementID, strSymbol, dblMass, dblUncertainty, sngCharge, intIsotopeCount)
    End Function

    Public Function GetElementCount() As Integer
        ' Returns the number of elements in memory
        GetElementCount = mElementAndMassRoutines.GetElementCountInternal()
    End Function

    Public Function GetElementID(ByVal strSymbol As String) As Integer
        ' Returns 0 if not found, the ID if found
        GetElementID = mElementAndMassRoutines.GetElementIDInternal(strSymbol)
    End Function

    Public Function GetElementIsotopes(ByVal intElementID As Short, ByRef intIsotopeCount As Short, ByRef dblIsotopeMasses() As Double, ByRef sngIsotopeAbundances() As Single) As Integer
        ' Returns the Isotope masses for the element with intElementID
        GetElementIsotopes = mElementAndMassRoutines.GetElementIsotopesInternal(intElementID, intIsotopeCount, dblIsotopeMasses, sngIsotopeAbundances)
    End Function

    Public Function GetElementMode() As MWElementAndMassRoutines.emElementModeConstants
        ' Returns the element mode:
        '    emAverageMass  = 1
        '    emIsotopicMass = 2
        '    emIntegerMass  = 3
        GetElementMode = mElementAndMassRoutines.GetElementModeInternal()
    End Function

    Public Function GetElementSymbol(ByVal intElementID As Short) As String
        ' Returns the symbol for the given element ID
        GetElementSymbol = mElementAndMassRoutines.GetElementSymbolInternal(intElementID)
    End Function

    Public Function GetElementStat(ByVal intElementID As Short, ByVal eElementStat As esElementStatsConstants) As Double
        ' Returns a single bit of information about a single element
        GetElementStat = mElementAndMassRoutines.GetElementStatInternal(intElementID, eElementStat)
    End Function

    Public Function GetMessageStatement(ByVal intMessageID As Integer) As String
        Return GetMessageStatement(intMessageID, String.Empty)
    End Function

    Public Function GetMessageStatement(ByVal intMessageID As Integer, ByVal strAppendText As String) As String
        ' Returns the message for lngMessageID
        GetMessageStatement = mElementAndMassRoutines.GetMessageStatementInternal(intMessageID, strAppendText)
    End Function

    Public Function GetMessageStatementCount() As Integer
        GetMessageStatementCount = mElementAndMassRoutines.GetMessageStatementCountInternal()
    End Function

    Public Function IsModSymbol(ByVal strSymbol As String) As Boolean
        ' Returns True if strSymbol starts with a ModSymbol
        IsModSymbol = mElementAndMassRoutines.IsModSymbolInternal(strSymbol)
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

        mElementAndMassRoutines.gComputationOptions.DecimalSeparator = MolecularWeightCalculator.DetermineDecimalPoint()

        mDataInitialized = True
    End Sub

    Public Sub RemoveAllAbbreviations()
        mElementAndMassRoutines.RemoveAllAbbreviationsInternal()
    End Sub

    Public Sub RemoveAllCautionStatements()
        mElementAndMassRoutines.RemoveAllCautionStatementsInternal()
    End Sub

    Public Function MassToPPM(ByVal dblMassToConvert As Double, ByVal dblCurrentMZ As Double) As Double
        Return mElementAndMassRoutines.MassToPPMInternal(dblMassToConvert, dblCurrentMZ)
    End Function

    Public Function MonoMassToMZ(ByVal dblMonoisotopicMass As Double, ByVal intCharge As Short) As Double
        Return MonoMassToMZ(dblMonoisotopicMass, intCharge, 0)
    End Function

    Public Function MonoMassToMZ(ByVal dblMonoisotopicMass As Double, ByVal intCharge As Short, ByVal dblChargeCarrierMass As Double) As Double
        Return mElementAndMassRoutines.MonoMassToMZInternal(dblMonoisotopicMass, intCharge, dblChargeCarrierMass)
    End Function

    Public Sub RecomputeAbbreviationMasses()
        ' Use this sub to manually recompute the masses of the abbreviations
        ' Useful if we just finished setting lots of element masses, and
        '  had blnRecomputeAbbreviationMasses = False when calling .SetElement()
        mElementAndMassRoutines.RecomputeAbbreviationMassesInternal()
    End Sub

    Public Function RemoveAbbreviation(ByVal strAbbreviationSymbol As String) As Integer
        RemoveAbbreviation = mElementAndMassRoutines.RemoveAbbreviationInternal(strAbbreviationSymbol)
    End Function

    Public Function RemoveAbbreviationByID(ByVal intAbbreviationID As Integer) As Integer
        RemoveAbbreviationByID = mElementAndMassRoutines.RemoveAbbreviationByIDInternal(intAbbreviationID)
    End Function

    Public Function RemoveCautionStatement(ByVal strCautionSymbol As String) As Integer
        RemoveCautionStatement = mElementAndMassRoutines.RemoveCautionStatementInternal(strCautionSymbol)
    End Function

    Public Sub ResetAbbreviations()
        mElementAndMassRoutines.MemoryLoadAbbreviations()
    End Sub

    Public Sub ResetCautionStatements()
        mElementAndMassRoutines.MemoryLoadCautionStatements()
    End Sub

    Public Sub ResetElement(ByVal intElementID As Short, ByVal eSpecificStatToReset As esElementStatsConstants)
        mElementAndMassRoutines.MemoryLoadElements(GetElementMode(), intElementID, eSpecificStatToReset)
    End Sub

    Public Sub ResetMessageStatements()
        mElementAndMassRoutines.MemoryLoadMessageStatements()
    End Sub

    Public Function SetAbbreviation(ByVal strSymbol As String, ByVal strFormula As String, ByVal sngCharge As Single, _
                                ByVal blnIsAminoAcid As Boolean) As Integer
        Return SetAbbreviation(strSymbol, strFormula, sngCharge, blnIsAminoAcid, "", "", True)
    End Function

    Public Function SetAbbreviation(ByVal strSymbol As String, ByVal strFormula As String, ByVal sngCharge As Single, _
                                    ByVal blnIsAminoAcid As Boolean, _
                                    ByVal strOneLetterSymbol As String, _
                                    ByVal strComment As String) As Integer
        Return SetAbbreviation(strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, strComment, True)
    End Function


    Public Function SetAbbreviation(ByVal strSymbol As String, ByVal strFormula As String, ByVal sngCharge As Single, _
                                    ByVal blnIsAminoAcid As Boolean, _
                                    ByVal strOneLetterSymbol As String, _
                                    ByVal strComment As String, _
                                    ByVal blnValidateFormula As Boolean) As Integer

        ' Adds a new abbreviation or updates an existing one (based on strSymbol)
        ' If blnValidateFormula = True, then makes sure the formula is valid
        ' It is useful to set blnValidateFormula = False when you're defining all of the abbreviations at once,
        '  since one abbreviation can depend upon another, and if the second abbreviation hasn't yet been
        '  defined, then the parsing of the first abbreviation will fail
        ' Returns 0 if successful, otherwise, returns an Error ID
        Return mElementAndMassRoutines.SetAbbreviationInternal(strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, strComment, blnValidateFormula)
    End Function

    Public Function SetAbbreviationByID(ByVal intAbbrevID As Integer, ByVal strSymbol As String, ByVal strFormula As String, _
                                    ByVal sngCharge As Single, ByVal blnIsAminoAcid As Boolean) As Integer

        Return SetAbbreviationByID(intAbbrevID, strSymbol, strFormula, sngCharge, blnIsAminoAcid, "", "", True)
    End Function

    Public Function SetAbbreviationByID(ByVal intAbbrevID As Integer, ByVal strSymbol As String, ByVal strFormula As String, _
                                        ByVal sngCharge As Single, ByVal blnIsAminoAcid As Boolean, _
                                        ByVal strOneLetterSymbol As String, _
                                        ByVal strComment As String, _
                                        ByVal blnValidateFormula As Boolean) As Integer

        ' Adds a new abbreviation or updates an existing one (based on strSymbol)
        ' If blnValidateFormula = True, then makes sure the formula is valid
        ' It is useful to set blnValidateFormula = False when you're defining all of the abbreviations at once,
        '  since one abbreviation can depend upon another, and if the second abbreviation hasn't yet been
        '  defined, then the parsing of the first abbreviation will fail
        ' Returns 0 if successful, otherwise, returns an Error ID
        SetAbbreviationByID = mElementAndMassRoutines.SetAbbreviationByIDInternal(CShort(intAbbrevID), strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, strComment, blnValidateFormula)
    End Function

    Public Function SetCautionStatement(ByVal strNewSymbolCombo As String, ByVal strNewCautionStatement As String) As Integer
        ' Adds a new caution statement or updates an existing one (based on strSymbol)
        ' Returns 0 if successful, otherwise, returns an Error ID
        SetCautionStatement = mElementAndMassRoutines.SetCautionStatementInternal(strNewSymbolCombo, strNewCautionStatement)
    End Function

    Public Sub SetChargeCarrierMass(ByVal dblMass As Double)
        mElementAndMassRoutines.SetChargeCarrierMassInternal(dblMass)
    End Sub

    Public Function SetElement(ByVal strSymbol As String, ByVal dblMass As Double, _
                               ByVal dblUncertainty As Double, ByVal sngCharge As Single) As Integer
        Return SetElement(strSymbol, dblMass, dblUncertainty, sngCharge, True)
    End Function

    Public Function SetElement(ByVal strSymbol As String, ByVal dblMass As Double, ByVal dblUncertainty As Double, _
                               ByVal sngCharge As Single, _
                               ByVal blnRecomputeAbbreviationMasses As Boolean) As Integer

        ' Used to update the values for a single element (based on strSymbol)
        SetElement = mElementAndMassRoutines.SetElementInternal(strSymbol, dblMass, dblUncertainty, sngCharge, blnRecomputeAbbreviationMasses)
    End Function

    Public Function SetElementIsotopes(ByVal strSymbol As String, ByVal intIsotopeCount As Short, ByRef dblIsotopeMassesOneBased() As Double, ByRef sngIsotopeAbundancesOneBased() As Single) As Integer
        SetElementIsotopes = mElementAndMassRoutines.SetElementIsotopesInternal(strSymbol, intIsotopeCount, dblIsotopeMassesOneBased, sngIsotopeAbundancesOneBased)
    End Function

    Public Sub SetElementMode(ByVal NewElementMode As MWElementAndMassRoutines.emElementModeConstants)
        SetElementMode(NewElementMode, True)
    End Sub

    Public Sub SetElementMode(ByVal NewElementMode As MWElementAndMassRoutines.emElementModeConstants, ByVal blnMemoryLoadElementValues As Boolean)
        mElementAndMassRoutines.SetElementModeInternal(NewElementMode, blnMemoryLoadElementValues)
    End Sub

    Public Function SetMessageStatement(ByVal lngMessageID As Integer, ByVal strNewMessage As String) As Integer
        ' Used to replace the default message strings with foreign language equivalent ones
        SetMessageStatement = mElementAndMassRoutines.SetMessageStatementInternal(lngMessageID, strNewMessage)
    End Function

    Public Sub SortAbbreviations()
        mElementAndMassRoutines.SortAbbreviationsInternal()
    End Sub

    Public Function TextToRTF(ByRef strTextToConvert As String) As String
        Return TextToRTF(strTextToConvert, False, True, False, 0)
    End Function

    Public Function TextToRTF(ByRef strTextToConvert As String, ByVal CalculatorMode As Boolean) As String
        Return TextToRTF(strTextToConvert, CalculatorMode, True, False, 0)
    End Function

    Public Function TextToRTF(ByRef strTextToConvert As String, ByVal CalculatorMode As Boolean, _
                              ByVal blnHighlightCharFollowingPercentSign As Boolean) As String
        Return TextToRTF(strTextToConvert, CalculatorMode, blnHighlightCharFollowingPercentSign, False, 0)
    End Function

    Public Function TextToRTF(ByRef strTextToConvert As String, _
                              ByVal CalculatorMode As Boolean, _
                              ByVal blnHighlightCharFollowingPercentSign As Boolean, _
                              ByVal blnOverrideErrorID As Boolean, _
                              ByVal lngErrorIDOverride As Integer) As String

        ' Converts an RTF string for the given text
        ' If blnHighlightCharFollowingPercentSign is true, then changes the character
        '  following a percent sign to red (and removes the percent sign)

        ' When blnCalculatorMode = True, then does not superscript + signs and numbers following + signs
        TextToRTF = mElementAndMassRoutines.PlainTextToRtfInternal(strTextToConvert, CalculatorMode, blnHighlightCharFollowingPercentSign, blnOverrideErrorID, lngErrorIDOverride)
    End Function

    Public Function ValidateAllAbbreviations() As Integer
        ' Checks the formula of all abbreviations to make sure it's valid
        ' Marks any abbreviations as Invalid if a problem is found or a circular reference exists
        ' Returns a count of the number of invalid abbreviations found

        ValidateAllAbbreviations = mElementAndMassRoutines.ValidateAllAbbreviationsInternal()
    End Function

    Protected Overrides Sub Finalize()
        Peptide = Nothing
        CapFlow = Nothing
        Compound = Nothing

        MyBase.Finalize()
    End Sub

    Private Sub mElementAndMassRoutines_ProgressChanged(ByVal taskDescription As String, ByVal percentComplete As Single) Handles mElementAndMassRoutines.ProgressChanged
        RaiseEvent ProgressChanged(taskDescription, percentComplete)
    End Sub

    Private Sub mElementAndMassRoutines_ProgressComplete() Handles mElementAndMassRoutines.ProgressComplete
        RaiseEvent ProgressComplete()
    End Sub

    Private Sub mElementAndMassRoutines_ProgressReset() Handles mElementAndMassRoutines.ProgressReset
        RaiseEvent ProgressReset()
    End Sub
End Class