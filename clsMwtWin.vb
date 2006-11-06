Option Strict On

Public Class MolecularWeightCalculator

    ' Molecular Weight Calculator routines with ActiveX Class interfaces
    ' Based on Molecular Weight Calculator, v6.20 code (VB6), written by Matthew Monroe 1995-2002
    '
    ' ActiveX Dll version written by Matthew Monroe in Richland, WA (2002)
    ' Ported to VB.NET by Nikša Blonder in Richland, WA (2005)

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

    Public Sub New()
        MyBase.New()

        ElementAndMassRoutines = New MWElementAndMassRoutines

        Compound = New MWCompoundClass(ElementAndMassRoutines)
        Peptide = New MWPeptideClass(ElementAndMassRoutines)

        CapFlow = New MWCapillaryFlowClass
        If Not mDataInitialized Then LoadDefaults()
    End Sub

    Private Const PROGRAM_DATE As String = "October 11, 2005"

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

    Private mDataInitialized As Boolean

    Public Compound As MWCompoundClass
    Public Peptide As MWPeptideClass

    Public CapFlow As MWCapillaryFlowClass

    Private ElementAndMassRoutines As MWElementAndMassRoutines

    Public Sub ClearError()
        ElementAndMassRoutines.ResetErrorParamsInternal()
    End Sub

    Public Function ComputeMass(ByVal strFormula As String) As Double
        Dim objCompound As New MWCompoundClass

        With objCompound
            ' Simply assigning strFormula to .Formula will update the Mass
            .Formula = strFormula
            ComputeMass = .Mass
        End With

        objCompound = Nothing

    End Function

    Public Function ComputeIsotopicAbundances(ByRef strFormulaIn As String, ByVal intChargeState As Short, ByRef strResults As String, ByRef ConvolutedMSData2DOneBased(,) As Double, ByRef ConvolutedMSDataCount As Integer, Optional ByVal strHeaderIsotopicAbundances As String = "Isotopic Abundances for", Optional ByVal strHeaderMass As String = "Mass", Optional ByVal strHeaderFraction As String = "Fraction", Optional ByVal strHeaderIntensity As String = "Intensity") As Short
        ' Computes the Isotopic Distribution for a formula
        ' Returns 0 if success, or -1 if an error

        ComputeIsotopicAbundances = ElementAndMassRoutines.ComputeIsotopicAbundancesInternal(strFormulaIn, intChargeState, strResults, ConvolutedMSData2DOneBased, ConvolutedMSDataCount, strHeaderIsotopicAbundances, strHeaderMass, strHeaderFraction, strHeaderIntensity, False)
    End Function

    Public Function ConvoluteMass(ByVal dblMassMZ As Double, ByVal intCurrentCharge As Short, Optional ByVal intDesiredCharge As Short = 1, Optional ByVal dblChargeCarrierMass As Double = 0) As Double
        ConvoluteMass = ElementAndMassRoutines.ConvoluteMassInternal(dblMassMZ, intCurrentCharge, intDesiredCharge, dblChargeCarrierMass)
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

    Public Function GetAbbreviation(ByVal lngAbbreviationID As Integer, ByRef strSymbol As String, ByRef strFormula As String, ByRef sngCharge As Single, ByRef blnIsAminoAcid As Boolean, Optional ByRef strOneLetterSymbol As String = "", Optional ByRef strComment As String = "", Optional ByRef blnInvalidSymbolOrFormula As Boolean = False) As Integer
        ' Returns 0 if success, 1 if failure
        GetAbbreviation = ElementAndMassRoutines.GetAbbreviationInternal(lngAbbreviationID, strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, strComment, blnInvalidSymbolOrFormula)
    End Function

    Public Function GetAbbreviationCount() As Integer
        GetAbbreviationCount = ElementAndMassRoutines.GetAbbreviationCountInternal()
    End Function

    Public Function GetAbbreviationCountMax() As Integer
        GetAbbreviationCountMax = ElementAndMassRoutines.MAX_ABBREV_COUNT
    End Function

    Public Function GetAbbreviationID(ByVal strSymbol As String) As Integer
        ' Returns 0 if not found, the ID if found
        GetAbbreviationID = ElementAndMassRoutines.GetAbbreviationIDInternal(strSymbol)
    End Function

    Public Function GetAminoAcidSymbolConversion(ByRef strSymbolToFind As String, ByRef bln1LetterTo3Letter As Boolean) As String
        ' If bln1LetterTo3Letter = True, then converting 1 letter codes to 3 letter codes
        ' Returns the symbol, if found
        ' Otherwise, returns ""
        GetAminoAcidSymbolConversion = ElementAndMassRoutines.GetAminoAcidSymbolConversionInternal(strSymbolToFind, bln1LetterTo3Letter)
    End Function

    Public Function GetCautionStatement(ByVal lngCautionStatementID As Integer, ByRef strSymbolCombo As String, ByRef strCautionStatement As String) As Integer
        ' Returns the contents of CautionStatements() in the ByRef variables
        ' Returns 0 if success, 1 if failure
        GetCautionStatement = ElementAndMassRoutines.GetCautionStatementInternal(lngCautionStatementID, strSymbolCombo, strCautionStatement)
    End Function

    Public Function GetCautionStatementCount() As Integer
        ' Returns the number of Caution Statements in memory
        GetCautionStatementCount = ElementAndMassRoutines.GetCautionStatementCountInternal()
    End Function

    Public Function GetCautionStatementID(ByVal strSymbolCombo As String) As Integer
        ' Returns -1 if not found, the ID if found
        GetCautionStatementID = ElementAndMassRoutines.GetCautionStatementIDInternal(strSymbolCombo)
    End Function

    Public Function GetChargeCarrierMass() As Double
        GetChargeCarrierMass = ElementAndMassRoutines.GetChargeCarrierMassInternal()
    End Function

    Public Function GetElement(ByVal intElementID As Short, ByRef strSymbol As String, ByRef dblMass As Double, ByRef dblUncertainty As Double, ByRef sngCharge As Single, ByRef intIsotopeCount As Short) As Integer
        ' Returns the settings for the element with intElementID in the ByRef variables
        ' Returns 0 if success, 1 if failure
        GetElement = ElementAndMassRoutines.GetElementInternal(intElementID, strSymbol, dblMass, dblUncertainty, sngCharge, intIsotopeCount)
    End Function

    Public Function GetElementCount() As Integer
        ' Returns the number of elements in memory
        GetElementCount = ElementAndMassRoutines.GetElementCountInternal()
    End Function

    Public Function GetElementID(ByVal strSymbol As String) As Integer
        ' Returns 0 if not found, the ID if found
        GetElementID = ElementAndMassRoutines.GetElementIDInternal(strSymbol)
    End Function

    Public Function GetElementIsotopes(ByVal intElementID As Short, ByRef intIsotopeCount As Short, ByRef dblIsotopeMasses() As Double, ByRef sngIsotopeAbundances() As Single) As Integer
        ' Returns the Isotope masses for the element with intElementID
        GetElementIsotopes = ElementAndMassRoutines.GetElementIsotopesInternal(intElementID, intIsotopeCount, dblIsotopeMasses, sngIsotopeAbundances)
    End Function

    Public Function GetElementMode() As MWElementAndMassRoutines.emElementModeConstants
        ' Returns the element mode:
        '    emAverageMass  = 1
        '    emIsotopicMass = 2
        '    emIntegerMass  = 3
        GetElementMode = ElementAndMassRoutines.GetElementModeInternal()
    End Function

    Public Function GetElementSymbol(ByVal intElementID As Short) As String
        ' Returns the symbol for the given element ID
        GetElementSymbol = ElementAndMassRoutines.GetElementSymbolInternal(intElementID)
    End Function

    Public Function GetElementStat(ByVal intElementID As Short, ByVal eElementStat As esElementStatsConstants) As Double
        ' Returns a single bit of information about a single element
        GetElementStat = ElementAndMassRoutines.GetElementStatInternal(intElementID, eElementStat)
    End Function

    Public Function GetMessageStatement(ByRef lngMessageID As Integer, Optional ByRef strAppendText As String = "") As String
        ' Returns the message for lngMessageID
        GetMessageStatement = ElementAndMassRoutines.GetMessageStatementInternal(lngMessageID, strAppendText)
    End Function

    Public Function GetMessageStatementCount() As Integer
        GetMessageStatementCount = ElementAndMassRoutines.GetMessageStatementCountInternal()
    End Function

    Public Function IsModSymbol(ByRef strSymbol As String) As Boolean
        ' Returns True if strSymbol starts with a ModSymbol
        IsModSymbol = ElementAndMassRoutines.IsModSymbolInternal(strSymbol)
    End Function

    Private Sub LoadDefaults()
        ElementAndMassRoutines.MemoryLoadAll(MWElementAndMassRoutines.emElementModeConstants.emAverageMass)

        Me.SetElementMode(MWElementAndMassRoutines.emElementModeConstants.emAverageMass)
        Me.AbbreviationRecognitionMode = arAbbrevRecognitionModeConstants.arNormalPlusAminoAcids
        Me.BracketsTreatedAsParentheses = True
        Me.CaseConversionMode = MWElementAndMassRoutines.ccCaseConversionConstants.ccConvertCaseUp
        Me.DecimalSeparator = "."c
        Me.RtfFontName = "Arial"
        Me.RtfFontSize = 10
        Me.StdDevMode = MWElementAndMassRoutines.smStdDevModeConstants.smDecimal

        ElementAndMassRoutines.gComputationOptions.DecimalSeparator = Me.DetermineDecimalPoint()

        mDataInitialized = True
    End Sub

    Public Sub RemoveAllAbbreviations()
        ElementAndMassRoutines.RemoveAllAbbreviationsInternal()
    End Sub

    Public Sub RemoveAllCautionStatements()
        ElementAndMassRoutines.RemoveAllCautionStatementsInternal()
    End Sub

    Public Function MassToPPM(ByVal dblMassToConvert As Double, ByVal dblCurrentMZ As Double) As Double
        MassToPPM = ElementAndMassRoutines.MassToPPMInternal(dblMassToConvert, dblCurrentMZ)
    End Function

    Public Function MonoMassToMZ(ByVal dblMonoisotopicMass As Double, ByVal intCharge As Short, Optional ByVal dblChargeCarrierMass As Double = 0) As Double
        MonoMassToMZ = ElementAndMassRoutines.MonoMassToMZInternal(dblMonoisotopicMass, intCharge, dblChargeCarrierMass)
    End Function

    Public Sub RecomputeAbbreviationMasses()
        ' Use this sub to manually recompute the masses of the abbreviations
        ' Useful if we just finished setting lots of element masses, and
        '  had blnRecomputeAbbreviationMasses = False when calling .SetElement()
        ElementAndMassRoutines.RecomputeAbbreviationMassesInternal()
    End Sub

    Public Function RemoveAbbreviation(ByVal strAbbreviationSymbol As String) As Integer
        RemoveAbbreviation = ElementAndMassRoutines.RemoveAbbreviationInternal(strAbbreviationSymbol)
    End Function

    Public Function RemoveAbbreviationByID(ByVal lngAbbreviationID As Integer) As Integer
        RemoveAbbreviationByID = ElementAndMassRoutines.RemoveAbbreviationByIDInternal(lngAbbreviationID)
    End Function

    Public Function RemoveCautionStatement(ByVal strCautionSymbol As String) As Integer
        RemoveCautionStatement = ElementAndMassRoutines.RemoveCautionStatementInternal(strCautionSymbol)
    End Function

    Public Sub ResetAbbreviations()
        ElementAndMassRoutines.MemoryLoadAbbreviations()
    End Sub

    Public Sub ResetCautionStatements()
        ElementAndMassRoutines.MemoryLoadCautionStatements()
    End Sub

    Public Sub ResetElement(ByRef intElementID As Short, ByRef eSpecificStatToReset As esElementStatsConstants)
        ElementAndMassRoutines.MemoryLoadElements(GetElementMode(), intElementID, eSpecificStatToReset)
    End Sub

    Public Sub ResetMessageStatements()
        ElementAndMassRoutines.MemoryLoadMessageStatements()
    End Sub

    Public Function SetAbbreviation(ByRef strSymbol As String, ByRef strFormula As String, ByRef sngCharge As Single, ByRef blnIsAminoAcid As Boolean, Optional ByRef strOneLetterSymbol As String = "", Optional ByRef strComment As String = "", Optional ByRef blnValidateFormula As Boolean = True) As Integer
        ' Adds a new abbreviation or updates an existing one (based on strSymbol)
        ' If blnValidateFormula = True, then makes sure the formula is valid
        ' It is useful to set blnValidateFormula = False when you're defining all of the abbreviations at once,
        '  since one abbreviation can depend upon another, and if the second abbreviation hasn't yet been
        '  defined, then the parsing of the first abbreviation will fail
        ' Returns 0 if successful, otherwise, returns an Error ID
        SetAbbreviation = ElementAndMassRoutines.SetAbbreviationInternal(strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, strComment, blnValidateFormula)
    End Function

    Public Function SetAbbreviationByID(ByRef lngAbbrevID As Integer, ByRef strSymbol As String, ByRef strFormula As String, ByRef sngCharge As Single, ByRef blnIsAminoAcid As Boolean, Optional ByRef strOneLetterSymbol As String = "", Optional ByRef strComment As String = "", Optional ByRef blnValidateFormula As Boolean = True) As Integer
        ' Adds a new abbreviation or updates an existing one (based on strSymbol)
        ' If blnValidateFormula = True, then makes sure the formula is valid
        ' It is useful to set blnValidateFormula = False when you're defining all of the abbreviations at once,
        '  since one abbreviation can depend upon another, and if the second abbreviation hasn't yet been
        '  defined, then the parsing of the first abbreviation will fail
        ' Returns 0 if successful, otherwise, returns an Error ID
        SetAbbreviationByID = ElementAndMassRoutines.SetAbbreviationByIDInternal(CShort(lngAbbrevID), strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, strComment, blnValidateFormula)
    End Function

    Public Function SetCautionStatement(ByRef strNewSymbolCombo As String, ByRef strNewCautionStatement As String) As Integer
        ' Adds a new caution statement or updates an existing one (based on strSymbol)
        ' Returns 0 if successful, otherwise, returns an Error ID
        SetCautionStatement = ElementAndMassRoutines.SetCautionStatementInternal(strNewSymbolCombo, strNewCautionStatement)
    End Function

    Public Function SetChargeCarrierMass(ByRef dblMass As Double) As Object
        ElementAndMassRoutines.SetChargeCarrierMassInternal(dblMass)
    End Function

    Public Function SetElement(ByRef strSymbol As String, ByRef dblMass As Double, ByRef dblUncertainty As Double, ByRef sngCharge As Single, Optional ByRef blnRecomputeAbbreviationMasses As Boolean = True) As Integer
        ' Used to update the values for a single element (based on strSymbol)
        SetElement = ElementAndMassRoutines.SetElementInternal(strSymbol, dblMass, dblUncertainty, sngCharge, blnRecomputeAbbreviationMasses)
    End Function

    Public Function SetElementIsotopes(ByVal strSymbol As String, ByVal intIsotopeCount As Short, ByRef dblIsotopeMassesOneBased() As Double, ByRef sngIsotopeAbundancesOneBased() As Single) As Integer
        SetElementIsotopes = ElementAndMassRoutines.SetElementIsotopesInternal(strSymbol, intIsotopeCount, dblIsotopeMassesOneBased, sngIsotopeAbundancesOneBased)
    End Function

    Public Sub SetElementMode(ByRef NewElementMode As MWElementAndMassRoutines.emElementModeConstants, Optional ByRef blnMemoryLoadElementValues As Boolean = True)
        ElementAndMassRoutines.SetElementModeInternal(NewElementMode, blnMemoryLoadElementValues)
    End Sub

    Public Function SetMessageStatement(ByRef lngMessageID As Integer, ByRef strNewMessage As String) As Integer
        ' Used to replace the default message strings with foreign language equivalent ones
        SetMessageStatement = ElementAndMassRoutines.SetMessageStatementInternal(lngMessageID, strNewMessage)
    End Function

    Public Sub SortAbbreviations()
        ElementAndMassRoutines.SortAbbreviationsInternal()
    End Sub

    Public Function TextToRTF(ByRef strTextToConvert As String, Optional ByRef CalculatorMode As Boolean = False, Optional ByRef blnHighlightCharFollowingPercentSign As Boolean = True, Optional ByRef blnOverrideErrorID As Boolean = False, Optional ByRef lngErrorIDOverride As Integer = 0) As String
        ' Converts an RTF string for the given text
        ' If blnHighlightCharFollowingPercentSign is true, then changes the character
        '  following a percent sign to red (and removes the percent sign)

        ' When blnCalculatorMode = True, then does not superscript + signs and numbers following + signs
        TextToRTF = ElementAndMassRoutines.PlainTextToRtfInternal(strTextToConvert, CalculatorMode, blnHighlightCharFollowingPercentSign, blnOverrideErrorID, lngErrorIDOverride)
    End Function

    Public Function ValidateAllAbbreviations() As Integer
        ' Checks the formula of all abbreviations to make sure it's valid
        ' Marks any abbreviations as Invalid if a problem is found or a circular reference exists
        ' Returns a count of the number of invalid abbreviations found

        ValidateAllAbbreviations = ElementAndMassRoutines.ValidateAllAbbreviationsInternal()
    End Function

    Protected Overrides Sub Finalize()
        Peptide = Nothing
        CapFlow = Nothing
        Compound = Nothing

        MyBase.Finalize()
    End Sub

    Public Property AbbreviationRecognitionMode() As arAbbrevRecognitionModeConstants
        Get
            Return ElementAndMassRoutines.gComputationOptions.AbbrevRecognitionMode
        End Get
        Set(ByVal Value As arAbbrevRecognitionModeConstants)
            If Value >= arAbbrevRecognitionModeConstants.arNormalOnly And Value <= arAbbrevRecognitionModeConstants.arNoAbbreviations Then
                ElementAndMassRoutines.gComputationOptions.AbbrevRecognitionMode = Value
                ElementAndMassRoutines.ConstructMasterSymbolsList()
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
            Return ElementAndMassRoutines.gComputationOptions.BracketsAsParentheses
        End Get
        Set(ByVal Value As Boolean)
            ElementAndMassRoutines.gComputationOptions.BracketsAsParentheses = Value
        End Set
    End Property


    Public Property CaseConversionMode() As MWElementAndMassRoutines.ccCaseConversionConstants
        Get
            Return ElementAndMassRoutines.gComputationOptions.CaseConversion
        End Get
        Set(ByVal Value As MWElementAndMassRoutines.ccCaseConversionConstants)
            If Value >= MWElementAndMassRoutines.ccCaseConversionConstants.ccConvertCaseUp And Value <= MWElementAndMassRoutines.ccCaseConversionConstants.ccSmartCase Then
                ElementAndMassRoutines.gComputationOptions.CaseConversion = Value
            End If
        End Set
    End Property


    Public Property DecimalSeparator() As Char
        Get
            Return ElementAndMassRoutines.gComputationOptions.DecimalSeparator
        End Get
        Set(ByVal Value As Char)
            ElementAndMassRoutines.gComputationOptions.DecimalSeparator = Value
        End Set
    End Property

    Public ReadOnly Property ErrorDescription() As String
        Get
            Return ElementAndMassRoutines.GetErrorDescription()
        End Get
    End Property

    Public ReadOnly Property ErrorID() As Integer
        Get
            Return ElementAndMassRoutines.GetErrorID()
        End Get
    End Property

    Public ReadOnly Property ErrorCharacter() As String
        Get
            Return ElementAndMassRoutines.GetErrorCharacter()
        End Get
    End Property

    Public ReadOnly Property ErrorPosition() As Integer
        Get
            Return ElementAndMassRoutines.GetErrorPosition()
        End Get
    End Property


    Public Property RtfFontName() As String
        Get
            Return ElementAndMassRoutines.gComputationOptions.RtfFontName
        End Get
        Set(ByVal Value As String)
            If Len(Value) > 0 Then
                ElementAndMassRoutines.gComputationOptions.RtfFontName = Value
            End If
        End Set
    End Property


    Public Property RtfFontSize() As Short
        Get
            Return ElementAndMassRoutines.gComputationOptions.RtfFontSize
        End Get
        Set(ByVal Value As Short)
            If Value > 0 Then
                ElementAndMassRoutines.gComputationOptions.RtfFontSize = Value
            End If
        End Set
    End Property


    Public Property ShowErrorDialogs() As Boolean
        Get
            Return ElementAndMassRoutines.ShowErrorMessageDialogs()
        End Get
        Set(ByVal Value As Boolean)
            ElementAndMassRoutines.SetShowErrorMessageDialogs(Value)
        End Set
    End Property


    Public Property StdDevMode() As MWElementAndMassRoutines.smStdDevModeConstants
        Get
            Return ElementAndMassRoutines.gComputationOptions.StdDevMode
        End Get
        Set(ByVal Value As MWElementAndMassRoutines.smStdDevModeConstants)
            If Value >= MWElementAndMassRoutines.smStdDevModeConstants.smShort And Value <= MWElementAndMassRoutines.smStdDevModeConstants.smDecimal Then
                ElementAndMassRoutines.gComputationOptions.StdDevMode = Value
            End If
        End Set
    End Property
End Class