using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using MolecularWeightCalculator;

namespace MwtWinDllTest
{
    // Molecular Weight Calculator Dll test program

    // -------------------------------------------------------------------------------
    // Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
    // E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
    // Website: https://github.com/PNNL-Comp-Mass-Spec/Molecular-Weight-Calculator-DLL and https://omics.pnl.gov/
    // -------------------------------------------------------------------------------
    //
    // Licensed under the Apache License, Version 2.0; you may not use this file except
    // in compliance with the License.  You may obtain a copy of the License at
    // http://www.apache.org/licenses/LICENSE-2.0
    //
    // Notice: This computer software was prepared by Battelle Memorial Institute,
    // hereinafter the Contractor, under Contract No. DE-AC05-76RL0 1830 with the
    // Department of Energy (DOE).  All rights in the computer software are reserved
    // by DOE on behalf of the United States Government and the Contractor as
    // provided in the Contract.  NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAKES ANY
    // WARRANTY, EXPRESS OR IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THIS
    // SOFTWARE.  This notice including this sentence must appear on any copies of
    // this computer software.

    internal partial class frmMwtWinDllTest : Form
    {
        public frmMwtWinDllTest()
        {
            InitializeComponent();

            InitializeControls();
        }

        private MolecularWeightTool mMwtWin;
        private DataSet myDataSet;

        private void AppendColumnToTableStyle(ref DataGridTableStyle tableStyle, string mappingName, string headerText, int width = 75, bool isReadOnly = false, bool isDateTime = false, int decimalPlaces = -1)
        {
            // If intDecimalPlaces is >=0, then a format string is constructed to show the specified number of decimal places
            var TextCol = new DataGridTextBoxColumn
            {
                MappingName = mappingName,
                HeaderText = headerText,
                Width = width,
                ReadOnly = isReadOnly
            };

            if (isDateTime)
            {
                TextCol.Format = "g";
            }
            else if (decimalPlaces >= 0)
            {
                TextCol.Format = "0.";
                for (var i = 0; i < decimalPlaces; i++)
                    TextCol.Format += "0";
            }

            tableStyle.GridColumnStyles.Add(TextCol);
        }

        private void AppendBoolColumnToTableStyle(ref DataGridTableStyle tableStyle, string mappingName, string headerText, int width = 75, bool isReadOnly = false, bool sourceIsTrueFalse = true)
        {
            // If intDecimalPlaces is >=0, then a format string is constructed to show the specified number of decimal places
            var BoolCol = new DataGridBoolColumn
            {
                MappingName = mappingName,
                HeaderText = headerText,
                Width = width,
                ReadOnly = isReadOnly
            };

            if (sourceIsTrueFalse)
            {
                BoolCol.FalseValue = false;
                BoolCol.TrueValue = true;
            }
            else
            {
                BoolCol.FalseValue = 0;
                BoolCol.TrueValue = 1;
            }

            BoolCol.AllowNull = false;
            BoolCol.NullValue = Convert.DBNull;

            tableStyle.GridColumnStyles.Add(BoolCol);
        }

        private void FindPercentComposition()
        {
            mMwtWin.Compound.Formula = txtFormula.Text;

            var dblPctCompForCarbon = mMwtWin.Compound.GetPercentCompositionForElement(6);
            var strPctCompForCarbon = mMwtWin.Compound.GetPercentCompositionForElementAsString(6);

            var percentCompositionByElement = mMwtWin.Compound.GetPercentCompositionForAllElements();

            MakePercentCompositionDataSet(percentCompositionByElement);

            dgDataGrid.SetDataBinding(myDataSet, "DataTable1");
        }

        private void FindMass()
        {
            lblProgress.Text = string.Empty;

            // Can simply compute the mass of a formula using ComputeMass
            lblMass.Text = mMwtWin.ComputeMass(txtFormula.Text).ToString();

            // If we want to do more complex operations, need to fill mMwtWin.Compound with valid info
            // Then, can read out values from it
            var compound = mMwtWin.Compound;
            compound.Formula = txtFormula.Text;

            if (string.IsNullOrEmpty(compound.ErrorDescription))
            {
                lblMass.Text = compound.Mass.ToString();
                lblStatus.Text = compound.CautionDescription;
                txtFormula.Text = compound.FormulaCapitalized;
                rtfFormula.Rtf = compound.FormulaRTF;
                lblMassAndStdDev.Text = compound.MassAndStdDevString;
            }
            else
            {
                lblStatus.Text = compound.ErrorDescription;
            }
        }

        private void InitializeControls()
        {
            mMwtWin = new MolecularWeightTool { ShowErrorDialogs = true };
            mMwtWin.ProgressChanged += mMwtWin_ProgressChanged;
            mMwtWin.ProgressComplete += mMwtWin_ProgressComplete;
            mMwtWin.ProgressReset += mMwtWin_ProgressReset;

            lblDLLVersion.Text = "DLL Info: " + mMwtWin.AppDate + ", Version " + mMwtWin.AppVersion;
            PopulateComboBoxes();
        }

        private void MakeDataSet(int ionCount, Peptide.FragmentationSpectrumData[] fragSpectrum)
        {
            // Create a DataSet.
            myDataSet = new DataSet("myDataSet");

            // Create a DataTable.
            var tDataTable = new DataTable("DataTable1");

            // Create three columns, and add them to the table.
            var cCMass = new DataColumn("Mass", typeof(double));
            var cIntensity = new DataColumn("Intensity", typeof(double));
            var cSymbol = new DataColumn("Symbol", typeof(string));
            tDataTable.Columns.Add(cCMass);
            tDataTable.Columns.Add(cIntensity);
            tDataTable.Columns.Add(cSymbol);

            // Add the table to the DataSet.
            myDataSet.Tables.Add(tDataTable);

            // Append rows to the table.
            for (var lngIndex = 0; lngIndex < ionCount; lngIndex++)
            {
                // Populates the table.
                var newRow = tDataTable.NewRow();
                newRow["Mass"] = fragSpectrum[lngIndex].Mass;
                newRow["Intensity"] = fragSpectrum[lngIndex].Intensity;
                newRow["Symbol"] = fragSpectrum[lngIndex].Symbol;
                tDataTable.Rows.Add(newRow);
            }
        }

        private void MakePercentCompositionDataSet(Dictionary<string, string> percentCompositionByElement)
        {

            // Create a DataSet.
            myDataSet = new DataSet("myDataSet");

            // Create a DataTable.
            var tDataTable = new DataTable("DataTable1");

            // Create three columns, and add them to the table.
            var cElement = new DataColumn("Element", typeof(string));
            var cPctComp = new DataColumn("Pct Comp", typeof(string));

            tDataTable.Columns.Add(cElement);
            tDataTable.Columns.Add(cPctComp);


            // Add the table to the DataSet.
            myDataSet.Tables.Add(tDataTable);

            // Populates the table

            // Append rows to the table.
            foreach (var item in percentCompositionByElement)
            {
                var newRow = tDataTable.NewRow();
                newRow["Element"] = item.Key;
                newRow["Pct Comp"] = item.Value;
                tDataTable.Rows.Add(newRow);
            }
        }

        private void PopulateComboBoxes()
        {
            cboWeightMode.Items.Clear();
            cboWeightMode.Items.Add("Average mass");
            cboWeightMode.Items.Add("Isotopic mass");
            cboWeightMode.Items.Add("Integer mass");
            cboWeightMode.SelectedIndex = 0;

            cboStdDevMode.Items.Clear();
            cboStdDevMode.Items.Add("Short");
            cboStdDevMode.Items.Add("Scientific");
            cboStdDevMode.Items.Add("Decimal");
            cboStdDevMode.SelectedIndex = 0;

            cboFormulaFinderTestMode.Items.Clear();
            cboFormulaFinderTestMode.Items.Add("Match 200 Da, +/- 0.05 Da");
            cboFormulaFinderTestMode.Items.Add("Match 200 Da, +/- 250 ppm");
            cboFormulaFinderTestMode.Items.Add("Match 200 Da, +/- 250 ppm, limit charge range");
            cboFormulaFinderTestMode.Items.Add("Match 100 m/z, +/- 250 ppm");
            cboFormulaFinderTestMode.Items.Add("Match percent composition values");
            cboFormulaFinderTestMode.Items.Add("Match 200 Da, +/- 250 ppm, Bounded search");
            cboFormulaFinderTestMode.SelectedIndex = 0;
        }

        public void TestAccessFunctions()
        {
            int intResult;
            double dblMass;

            var objResults = new frmTextbrowser();

            lblProgress.Text = string.Empty;

            objResults.Show();
            objResults.SetText = string.Empty;

            // Test Abbreviations
            var lngItemCount = mMwtWin.GetAbbreviationCount();
            for (var intIndex = 1; intIndex <= lngItemCount; intIndex++)
            {
                intResult = mMwtWin.GetAbbreviation(intIndex, out var strSymbol, out var strFormula, out var sngCharge, out var blnIsAminoAcid, out var strOneLetterSymbol, out var strComment);
                Debug.Assert(intResult == 0, "");
                Debug.Assert(mMwtWin.GetAbbreviationID(strSymbol) == intIndex, "");

                intResult = mMwtWin.SetAbbreviation(strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, strComment);
                Debug.Assert(intResult == 0, "");
            }

            // Test Caution statements
            lngItemCount = mMwtWin.GetCautionStatementCount();
            for (var intIndex = 1; intIndex <= lngItemCount; intIndex++)
            {
                intResult = mMwtWin.GetCautionStatement(intIndex, out var strSymbol, out var strStatement);
                Debug.Assert(intResult == 0, "");
                Debug.Assert(mMwtWin.GetCautionStatementID(strSymbol) == intIndex, "");

                intResult = mMwtWin.SetCautionStatement(strSymbol, strStatement);
                Debug.Assert(intResult == 0, "");
            }

            // Test Element access
            lngItemCount = mMwtWin.GetElementCount();
            for (var intIndex = 1; intIndex <= lngItemCount; intIndex++)
            {
                intResult = mMwtWin.GetElement((short)intIndex, out var strSymbol, out dblMass, out var dblUncertainty, out var sngCharge, out var intIsotopeCount);
                Debug.Assert(intResult == 0, "");
                Debug.Assert(mMwtWin.GetElementID(strSymbol) == intIndex, "");

                intResult = mMwtWin.SetElement(strSymbol, dblMass, dblUncertainty, sngCharge, false);
                Debug.Assert(intResult == 0, "");

                var dblIsotopeMasses = new double[intIsotopeCount + 1 + 1];
                var sngIsotopeAbundances = new float[intIsotopeCount + 1 + 1];

                short intIsotopeCount2 = default;
                intResult = mMwtWin.GetElementIsotopes((short)intIndex, ref intIsotopeCount2, ref dblIsotopeMasses, ref sngIsotopeAbundances);
                Debug.Assert(intIsotopeCount == intIsotopeCount2, "");
                Debug.Assert(intResult == 0, "");

                intResult = mMwtWin.SetElementIsotopes(strSymbol, intIsotopeCount, ref dblIsotopeMasses, ref sngIsotopeAbundances);
                Debug.Assert(intResult == 0, "");
            }

            // Test Message Statements access
            lngItemCount = mMwtWin.GetMessageStatementCount();
            for (var lngIndex = 1; lngIndex <= lngItemCount; lngIndex++)
            {
                var strStatement = mMwtWin.GetMessageStatement(lngIndex);

                intResult = mMwtWin.SetMessageStatement(lngIndex, strStatement);
            }

            // Test m/z conversion
            // Switch to isotopic masses

            mMwtWin.SetElementMode(ElementAndMassTools.ElementMassMode.Isotopic);

            mMwtWin.Compound.SetFormula("C19H36O5NH4");
            dblMass = mMwtWin.Compound.Mass;
            objResults.AppendText("Mass of " + mMwtWin.Compound.FormulaCapitalized + ": " + dblMass);
            for (short intCharge = 1; intCharge <= 4; intCharge++)
                objResults.AppendText("  m/z of " + intCharge + "+: " + mMwtWin.ConvoluteMass(dblMass, 0, intCharge));

            objResults.AppendText("");

            mMwtWin.Compound.SetFormula("C19H36O5NH3");
            dblMass = mMwtWin.Compound.Mass;
            objResults.AppendText("m/z values if we first lose a hydrogen before adding a proton");
            for (short intCharge = 1; intCharge <= 4; intCharge++)
                objResults.AppendText("  m/z of " + intCharge + "+: " + mMwtWin.ConvoluteMass(dblMass, 0, intCharge));

            // Test Capillary flow functions
            var capFlow = mMwtWin.CapFlow;
            capFlow.SetAutoComputeEnabled(false);
            capFlow.SetBackPressure(2000d, CapillaryFlow.UnitOfPressure.Psi);
            capFlow.SetColumnLength(40d, CapillaryFlow.UnitOfLength.CM);
            capFlow.SetColumnID(50d, CapillaryFlow.UnitOfLength.Microns);
            capFlow.SetSolventViscosity(0.0089d, CapillaryFlow.UnitOfViscosity.Poise);
            capFlow.SetInterparticlePorosity(0.33d);
            capFlow.SetParticleDiameter(2d, CapillaryFlow.UnitOfLength.Microns);
            capFlow.SetAutoComputeEnabled(true);

            objResults.AppendText("");
            objResults.AppendText("Check capillary flow calcs");
            objResults.AppendText("Linear Velocity: " + capFlow.ComputeLinearVelocity(CapillaryFlow.UnitOfLinearVelocity.CmPerSec));
            objResults.AppendText("Vol flow rate:   " + capFlow.ComputeVolFlowRate(CapillaryFlow.UnitOfFlowRate.NLPerMin) + "  (newly computed)");

            objResults.AppendText("Vol flow rate:   " + capFlow.GetVolFlowRate());
            objResults.AppendText("Back pressure:   " + capFlow.ComputeBackPressure(CapillaryFlow.UnitOfPressure.Psi));
            objResults.AppendText("Column Length:   " + capFlow.ComputeColumnLength(CapillaryFlow.UnitOfLength.CM));
            objResults.AppendText("Column ID:       " + capFlow.ComputeColumnID(CapillaryFlow.UnitOfLength.Microns));
            objResults.AppendText("Column Volume:   " + capFlow.ComputeColumnVolume(CapillaryFlow.UnitOfVolume.NL));
            objResults.AppendText("Dead time:       " + capFlow.ComputeDeadTime(CapillaryFlow.UnitOfTime.Seconds));

            objResults.AppendText("");

            objResults.AppendText("Repeat Computations, but in a different order (should give same results)");
            objResults.AppendText("Vol flow rate:   " + capFlow.ComputeVolFlowRate(CapillaryFlow.UnitOfFlowRate.NLPerMin));
            objResults.AppendText("Column ID:       " + capFlow.ComputeColumnID(CapillaryFlow.UnitOfLength.Microns));
            objResults.AppendText("Back pressure:   " + capFlow.ComputeBackPressure(CapillaryFlow.UnitOfPressure.Psi));
            objResults.AppendText("Column Length:   " + capFlow.ComputeColumnLength(CapillaryFlow.UnitOfLength.CM));

            objResults.AppendText("");

            objResults.AppendText("Old Dead time: " + capFlow.GetDeadTime(CapillaryFlow.UnitOfTime.Minutes));

            capFlow.SetAutoComputeMode(CapillaryFlow.AutoComputeMode.VolFlowRateUsingDeadTime);

            capFlow.SetDeadTime(25d, CapillaryFlow.UnitOfTime.Minutes);
            objResults.AppendText("Dead time is now 25.0 minutes");

            objResults.AppendText("Vol flow rate: " + capFlow.GetVolFlowRate(CapillaryFlow.UnitOfFlowRate.NLPerMin) + " (auto-computed since AutoComputeMode = acmVolFlowrateUsingDeadTime)");

            // Confirm that auto-compute worked

            objResults.AppendText("Vol flow rate: " + capFlow.ComputeVolFlowRateUsingDeadTime(out var dblNewPressure, CapillaryFlow.UnitOfFlowRate.NLPerMin, CapillaryFlow.UnitOfPressure.Psi) + "  (confirmation of computed volumetric flow rate)");
            objResults.AppendText("New pressure: " + dblNewPressure);

            objResults.AppendText("");

            // Can set a new back pressure, but since auto-compute is on, and the
            // auto-compute mode is acmVolFlowRateUsingDeadTime, the pressure will get changed back to
            // the pressure needed to give a vol flow rate matching the dead time
            capFlow.SetBackPressure(2000d);
            objResults.AppendText("Pressure set to 2000 psi, but auto-compute mode is acmVolFlowRateUsingDeadTime, so pressure");
            objResults.AppendText("  was automatically changed back to pressure needed to give vol flow rate matching dead time");
            objResults.AppendText("Pressure is now: " + capFlow.GetBackPressure(CapillaryFlow.UnitOfPressure.Psi) + " psi (thus, not 2000 as one might expect)");

            capFlow.SetAutoComputeMode(CapillaryFlow.AutoComputeMode.VolFlowRate);
            objResults.AppendText("Changed auto-compute mode to acmVolFlowrate.  Can now set pressure to 2000 and it will stick; plus, vol flow rate gets computed.");

            capFlow.SetBackPressure(2000d, CapillaryFlow.UnitOfPressure.Psi);

            // Calling GetVolFlowRate will get the new computed vol flow rate (since auto-compute is on)
            objResults.AppendText("Vol flow rate: " + capFlow.GetVolFlowRate());

            capFlow.SetMassRateSampleMass(1000d);
            capFlow.SetMassRateConcentration(1d, CapillaryFlow.UnitOfConcentration.MicroMolar);
            capFlow.SetMassRateVolFlowRate(600d, CapillaryFlow.UnitOfFlowRate.NLPerMin);
            capFlow.SetMassRateInjectionTime(5d, CapillaryFlow.UnitOfTime.Minutes);

            objResults.AppendText("Mass flow rate: " + capFlow.GetMassFlowRate(CapillaryFlow.UnitOfMassFlowRate.FmolPerSec) + " fmol/sec");
            objResults.AppendText("Moles injected: " + capFlow.GetMassRateMolesInjected(CapillaryFlow.UnitOfMolarAmount.FemtoMoles) + " fmoles");

            capFlow.SetMassRateSampleMass(1234d);
            capFlow.SetMassRateConcentration(1d, CapillaryFlow.UnitOfConcentration.NgPerML);

            objResults.AppendText("Computing mass flow rate for compound weighing 1234 g/mol and at 1 ng/mL concentration");
            objResults.AppendText("Mass flow rate: " + capFlow.GetMassFlowRate(CapillaryFlow.UnitOfMassFlowRate.AmolPerMin) + " amol/min");
            objResults.AppendText("Moles injected: " + capFlow.GetMassRateMolesInjected(CapillaryFlow.UnitOfMolarAmount.FemtoMoles) + " fmoles");

            capFlow.SetExtraColumnBroadeningLinearVelocity(4d, CapillaryFlow.UnitOfLinearVelocity.CmPerMin);
            capFlow.SetExtraColumnBroadeningDiffusionCoefficient(0.0003d, CapillaryFlow.UnitOfDiffusionCoefficient.CmSquaredPerMin);
            capFlow.SetExtraColumnBroadeningOpenTubeLength(5d, CapillaryFlow.UnitOfLength.CM);
            capFlow.SetExtraColumnBroadeningOpenTubeID(250d, CapillaryFlow.UnitOfLength.Microns);
            capFlow.SetExtraColumnBroadeningInitialPeakWidthAtBase(30d, CapillaryFlow.UnitOfTime.Seconds);

            objResults.AppendText("Computing broadening for 30 second wide peak through a 250 um open tube that is 5 cm long (4 cm/min)");
            objResults.AppendText(capFlow.GetExtraColumnBroadeningResultantPeakWidth(CapillaryFlow.UnitOfTime.Seconds).ToString());

            var udtFragSpectrumOptions = new Peptide.FragmentationSpectrumOptions();
            udtFragSpectrumOptions.Initialize();

            var peptide = mMwtWin.Peptide;
            peptide.SetSequence1LetterSymbol("K.AC!YEFGHRKACY*EFGHRK.G");
            // .SetSequence1LetterSymbol("K.ACYEFGHRKACYEFGHRK.G")

            // Can change the terminii to various standard groups
            peptide.SetNTerminusGroup(Peptide.NTerminusGroupType.Carbamyl);
            peptide.SetCTerminusGroup(Peptide.CTerminusGroupType.Amide);

            // Can change the terminii to any desired elements
            peptide.SetNTerminus("C2OH3"); // Acetyl group
            peptide.SetCTerminus("NH2"); // Amide group

            // Can mark third residue, Tyr, as phorphorylated
            peptide.SetResidue(3, "Tyr", true, true);

            // Can define that the * modification equals 15
            peptide.SetModificationSymbol("*", 15d, false, "");

            const string strNewSeq = "Ala-Cys-Tyr-Glu-Phe-Gly-His-Arg*-Lys-Ala-Cys-Tyr-Glu-Phe-Gly-His-Arg-Lys";
            objResults.AppendText(strNewSeq);
            peptide.SetSequence(strNewSeq);

            peptide.SetSequence("K.TQPLE*VK.-", Peptide.NTerminusGroupType.HydrogenPlusProton, Peptide.CTerminusGroupType.Hydroxyl, is3LetterCode: false);

            objResults.AppendText(peptide.GetSequence(true, false, true, false));
            objResults.AppendText(peptide.GetSequence(false, true, false, false));
            objResults.AppendText(peptide.GetSequence(true, false, true, true));

            peptide.SetCTerminusGroup(Peptide.CTerminusGroupType.None);
            objResults.AppendText(peptide.GetSequence(true, false, true, true));

            udtFragSpectrumOptions = peptide.GetFragmentationSpectrumOptions();

            udtFragSpectrumOptions.DoubleChargeIonsShow = true;
            udtFragSpectrumOptions.DoubleChargeIonsThreshold = 300f;
            udtFragSpectrumOptions.IntensityOptions.BYIonShoulder = 0d;

            udtFragSpectrumOptions.TripleChargeIonsShow = true;
            udtFragSpectrumOptions.TripleChargeIonsThreshold = 400f;

            udtFragSpectrumOptions.IonTypeOptions[(int)Peptide.IonType.AIon].ShowIon = true;

            peptide.SetFragmentationSpectrumOptions(udtFragSpectrumOptions);

            var lngIonCount = peptide.GetFragmentationMasses(out var udtFragSpectrum);

            MakeDataSet(lngIonCount, udtFragSpectrum);
            dgDataGrid.SetDataBinding(myDataSet, "DataTable1");

            objResults.AppendText(string.Empty);

            var strResults = string.Empty;
            var ConvolutedMSDataCount = default(int);

            // Really big formula to test with: C489 H300 F27 Fe8 N72 Ni6 O27 S9
            const short intChargeState = 1;
            var blnAddProtonChargeCarrier = true;
            objResults.AppendText("Isotopic abundance test with Charge=" + intChargeState);

            var ConvolutedMSData2DOneBased = new double[1, 2];
            var argstrFormulaIn = "C1255H43O2Cl";
            var intSuccess = mMwtWin.ComputeIsotopicAbundances(ref argstrFormulaIn, intChargeState, ref strResults, ref ConvolutedMSData2DOneBased, ref ConvolutedMSDataCount);
            objResults.AppendText(strResults);

            objResults.AppendText("Convert isotopic distribution to gaussian");
            var lstXYVals = new List<KeyValuePair<double, double>>();
            for (var intIndex = 1; intIndex <= ConvolutedMSDataCount; intIndex++)
                lstXYVals.Add(new KeyValuePair<double, double>(ConvolutedMSData2DOneBased[intIndex, 0], ConvolutedMSData2DOneBased[intIndex, 1]));

            const int intResolution = 2000;
            const double dblResolutionMass = 1000d;
            const int intQualityFactor = 50;

            var lstGaussianData = mMwtWin.ConvertStickDataToGaussian2DArray(lstXYVals, intResolution, dblResolutionMass, intQualityFactor);

            var sbResults = new StringBuilder();
            sbResults.AppendLine("m/z" + "\t" + "Intensity");
            for (var intIndex = 0; intIndex < lstGaussianData.Count; intIndex++)
            {
                if (lstGaussianData[intIndex].Key >= 15175d && lstGaussianData[intIndex].Key < 15193d)
                {
                    sbResults.AppendLine(lstGaussianData[intIndex].Key.ToString("0.000") + "\t" + lstGaussianData[intIndex].Value.ToString("0.000"));
                }
            }

            objResults.AppendText(sbResults.ToString());

            blnAddProtonChargeCarrier = false;
            objResults.AppendText("Isotopic abundance test with Charge=" + intChargeState + "; do not add a proton charge carrier");
            var argstrFormulaIn1 = "C1255H43O2Cl";
            intSuccess = mMwtWin.ComputeIsotopicAbundances(ref argstrFormulaIn1, intChargeState, ref strResults, ref ConvolutedMSData2DOneBased, ref ConvolutedMSDataCount, blnAddProtonChargeCarrier);
            objResults.AppendText(strResults);
        }

        public void TestFormulaFinder()
        {
            var oMwtWin = new MolecularWeightTool();

            oMwtWin.SetElementMode(ElementAndMassTools.ElementMassMode.Isotopic);

            oMwtWin.FormulaFinder.CandidateElements.Clear();

            oMwtWin.FormulaFinder.AddCandidateElement("C");
            oMwtWin.FormulaFinder.AddCandidateElement("H");
            oMwtWin.FormulaFinder.AddCandidateElement("N");
            oMwtWin.FormulaFinder.AddCandidateElement("O");

            // Abbreviations are supported, for example Serine
            oMwtWin.FormulaFinder.AddCandidateElement("Ser");

            var searchOptions = new FormulaFinderOptions
            {
                LimitChargeRange = false,
                ChargeMin = 1,
                ChargeMax = 1,
                FindTargetMZ = false
            };

            cmdTestFormulaFinder.Enabled = false;
            Application.DoEvents();

            if (cboFormulaFinderTestMode.SelectedIndex == 0)
                FormulaFinderTest1(oMwtWin, searchOptions, cboFormulaFinderTestMode.Text);
            if (cboFormulaFinderTestMode.SelectedIndex == 1)
                FormulaFinderTest2(oMwtWin, searchOptions, cboFormulaFinderTestMode.Text);
            if (cboFormulaFinderTestMode.SelectedIndex == 2)
                FormulaFinderTest3(oMwtWin, searchOptions, cboFormulaFinderTestMode.Text);
            if (cboFormulaFinderTestMode.SelectedIndex == 3)
                FormulaFinderTest4(oMwtWin, searchOptions, cboFormulaFinderTestMode.Text);
            if (cboFormulaFinderTestMode.SelectedIndex == 4)
                FormulaFinderTest5(oMwtWin, searchOptions, cboFormulaFinderTestMode.Text);
            if (cboFormulaFinderTestMode.SelectedIndex == 5)
                FormulaFinderTest6(oMwtWin, searchOptions, cboFormulaFinderTestMode.Text);

            cmdTestFormulaFinder.Enabled = true;

            if (cboFormulaFinderTestMode.SelectedIndex > 5)
            {
                MessageBox.Show("Formula finder test mode not recognized", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void FormulaFinderTest1(MolecularWeightTool mwtWin, FormulaFinderOptions searchOptions, string currentTask)
        {

            // Search for 200 Da, +/- 0.05 Da
            var lstResults = mwtWin.FormulaFinder.FindMatchesByMass(200d, 0.05d, searchOptions);
            ShowFormulaFinderResults(currentTask, searchOptions, lstResults);
        }

        private void FormulaFinderTest2(MolecularWeightTool mwtWin, FormulaFinderOptions searchOptions, string currentTask)
        {

            // Search for 200 Da, +/- 250 ppm
            var lstResults = mwtWin.FormulaFinder.FindMatchesByMassPPM(200d, 250d, searchOptions);
            ShowFormulaFinderResults(currentTask, searchOptions, lstResults, true);
        }

        private void FormulaFinderTest3(MolecularWeightTool mwtWin, FormulaFinderOptions searchOptions, string currentTask)
        {
            searchOptions.LimitChargeRange = true;
            searchOptions.ChargeMin = -4;
            searchOptions.ChargeMax = 6;

            // Search for 200 Da, +/- 250 ppm
            var lstResults = mwtWin.FormulaFinder.FindMatchesByMassPPM(200d, 250d, searchOptions);
            ShowFormulaFinderResults(currentTask, searchOptions, lstResults, true);
        }

        private void FormulaFinderTest4(MolecularWeightTool mwtWin, FormulaFinderOptions searchOptions, string currentTask)
        {
            searchOptions.LimitChargeRange = true;
            searchOptions.ChargeMin = -4;
            searchOptions.ChargeMax = 6;
            searchOptions.FindTargetMZ = true;

            // Search for 100 m/z, +/- 250 ppm
            var lstResults = mwtWin.FormulaFinder.FindMatchesByMassPPM(100d, 250d, searchOptions);
            ShowFormulaFinderResults(currentTask, searchOptions, lstResults, true);
        }

        private void FormulaFinderTest5(MolecularWeightTool mwtWin, FormulaFinderOptions searchOptions, string currentTask)
        {
            mwtWin.FormulaFinder.CandidateElements.Clear();

            mwtWin.FormulaFinder.AddCandidateElement("C", 70d);
            mwtWin.FormulaFinder.AddCandidateElement("H", 10d);
            mwtWin.FormulaFinder.AddCandidateElement("N", 10d);
            mwtWin.FormulaFinder.AddCandidateElement("O", 10d);

            // Search for percent composition results, maximum mass 400 Da
            var lstResults = mwtWin.FormulaFinder.FindMatchesByPercentComposition(400d, 1d, searchOptions);
            ShowFormulaFinderResults(currentTask, searchOptions, lstResults, false, true);
        }

        private void FormulaFinderTest6(MolecularWeightTool mwtWin, FormulaFinderOptions searchOptions, string currentTask)
        {
            searchOptions.SearchMode = FormulaFinderOptions.SearchModes.Bounded;

            // Search for 200 Da, +/- 250 ppm
            var lstResults = mwtWin.FormulaFinder.FindMatchesByMassPPM(200d, 250d, searchOptions);
            ShowFormulaFinderResults(currentTask, searchOptions, lstResults, true);
        }

        private void ShowFormulaFinderResults(
            string currentTask,
            FormulaFinderOptions searchOptions,
            List<FormulaFinderResult> results,
            bool deltaMassIsPPM = false,
            bool percentCompositionSearch = false)
        {
            myDataSet = new DataSet("myDataSet");

            // Create a DataTable.
            var tDataTable = new DataTable("DataTable1");

            string massColumnName;
            if (deltaMassIsPPM)
            {
                massColumnName = "DeltaPPM";
            }
            else
            {
                massColumnName = "DeltaMass";
            }

            // Add columns to the table
            var cFormula = new DataColumn("Formula", typeof(string));
            var cMass = new DataColumn("Mass", typeof(double));
            var cDeltaMass = new DataColumn(massColumnName, typeof(double));
            var cCharge = new DataColumn("Charge", typeof(int));
            var cMZ = new DataColumn("M/Z", typeof(double));
            var cPercentComp = new DataColumn("PercentCompInfo", typeof(string));

            tDataTable.Columns.Add(cFormula);
            tDataTable.Columns.Add(cMass);
            tDataTable.Columns.Add(cDeltaMass);
            tDataTable.Columns.Add(cCharge);
            tDataTable.Columns.Add(cMZ);
            tDataTable.Columns.Add(cPercentComp);

            if (myDataSet.Tables.Count > 0)
            {
                myDataSet.Tables.Clear();
            }

            // Add the table to the DataSet.
            myDataSet.Tables.Add(tDataTable);

            var sbPercentCompInfo = new StringBuilder();

            foreach (var result in results)
            {
                // Populates the table.
                var newRow = tDataTable.NewRow();
                newRow["Formula"] = result.EmpiricalFormula;
                newRow["Mass"] = Math.Round(result.Mass, 4);

                if (deltaMassIsPPM)
                {
                    newRow[massColumnName] = result.DeltaMass.ToString("0.0");
                }
                else
                {
                    newRow[massColumnName] = result.DeltaMass.ToString("0.000");
                }

                newRow["Charge"] = result.ChargeState;

                if (searchOptions.FindCharge)
                {
                    newRow["M/Z"] = Math.Round(result.MZ, 3);
                }

                if (percentCompositionSearch)
                {
                    sbPercentCompInfo.Clear();

                    foreach (var percentCompValue in result.PercentComposition)
                        sbPercentCompInfo.Append(" " + percentCompValue.Key + "=" + percentCompValue.Value.ToString("0.00") + "%");

                    newRow["PercentCompInfo"] = sbPercentCompInfo.ToString().TrimStart();
                }
                else
                {
                    newRow["PercentCompInfo"] = string.Empty;
                }

                tDataTable.Rows.Add(newRow);
            }

            dgDataGrid.SetDataBinding(myDataSet, "DataTable1");
        }

        private void TestTrypticName()
        {
            const short DIM_CHUNK = 1000;

            const short ITERATIONS_TO_RUN = 5;
            const short MIN_PROTEIN_LENGTH = 50;
            const short MAX_PROTEIN_LENGTH = 200;
            const string POSSIBLE_RESIDUES = "ACDEFGHIKLMNPQRSTVWY";

            string strPeptideFragMwtWin;
            const int lngMatchCount = default(int);

            var objResults = new frmTextbrowser();

            lblProgress.Text = string.Empty;

            int lngMwtWinDimCount = DIM_CHUNK;
            var strPeptideNameMwtWin = new string[lngMwtWinDimCount + 1];

            Cursor = Cursors.WaitCursor;

            objResults.Show();
            objResults.SetText = string.Empty;

            //Dim lngIcr2lsWorkTime As Long
            //Dim lngIcr2lsTime As Long
            //strPeptideFragIcr2ls As String
            //lngICR2lsDimCount = DIM_CHUNK
            //ReDim strPeptideNameIcr2ls(lngICR2lsDimCount)
            //
            //Dim ICRTools As Object
            //
            //Set ICRTools = CreateObject("ICR2LS.ICR2LScls")
            //
            //objResults.AppendText("ICR2ls Version: " & ICRTools.ICR2LSversion)

            //strProtein = "MGNISFLTGGNPSSPQSIAESIYQLENTSVVFLSAWQRTTPDFQRAARASQEAMLHLDHIVNEIMRNRDQLQADGTYTGSQLEGLLNISRAVSVSPVTRAEQDDLANYGPGNGVLPSAGSSISMEKLLNKIKHRRTNSANFRIGASGEHIFIIGVDKPNRQPDSIVEFIVGDFCQHCSDIAALI"

            // Bigger protein
            var strProtein = "MMKANVTKKTLNEGLGLLERVIPSRSSNPLLTALKVETSEGGLTLSGTNLEIDLSCFVPAEVQQPENFVVPAHLFAQIVRNLGGELVELELSGQELSVRSGGSDFKLQTGDIEAYPPLSFPAQADVSLDGGELSRAFSSVRYAASNEAFQAVFRGIKLEHHGESARVVASDGYRVAIRDFPASGDGKNLIIPARSVDELIRVLKDGEARFTYGDGMLTVTTDRVKMNLKLLDGDFPDYERVIPKDIKLQVTLPATALKEAVNRVAVLADKNANNRVEFLVSEGTLRLAAEGDYGRAQDTLSVTQGGTEQAMSLAFNARHVLDALGPIDGDAELLFSGSTSPAIFRARRWGRRVYGGHGHAARLRGLLRPLRGMSALAHHPESSPPLEPRPEFA";

            objResults.AppendText("Testing GetTrypticNameMultipleMatches() function");
            objResults.AppendText("MatchList for NL: " + mMwtWin.Peptide.GetTrypticNameMultipleMatches(strProtein, "NL", lngMatchCount));
            objResults.AppendText("MatchCount = " + lngMatchCount);

            objResults.AppendText(string.Empty);
            objResults.AppendText("Testing GetTrypticPeptideByFragmentNumber function");
            for (var index = 0; index < 43; index++)
            {
                strPeptideFragMwtWin = mMwtWin.Peptide.GetTrypticPeptideByFragmentNumber(strProtein, (short)index, out var lngResidueStart, out var lngResidueEnd);
                //strPeptideFragIcr2ls = ICRTools.TrypticPeptide(strProtein, CInt(index))
                //
                //Debug.Assert strPeptideFragMwtWin = strPeptideFragIcr2ls

                if (strPeptideFragMwtWin.Length > 1)
                {
                    // Make sure lngResidueStart and lngResidueEnd are correct
                    // Do this using .GetTrypticNameMultipleMatches()
                    var strPeptideName = mMwtWin.Peptide.GetTrypticNameMultipleMatches(strProtein, strProtein.Substring(lngResidueStart, lngResidueEnd - lngResidueStart + 1));
                    Debug.Assert(strPeptideName.IndexOf("t" + index, StringComparison.Ordinal) >= 0, "");
                }
            }

            objResults.AppendText("Check of GetTrypticPeptideByFragmentNumber Complete");
            objResults.AppendText(string.Empty);

            objResults.AppendText("Test tryptic digest of: " + strProtein);
            var lngIndex = 1;
            do
            {
                strPeptideFragMwtWin = mMwtWin.Peptide.GetTrypticPeptideByFragmentNumber(strProtein, (short)lngIndex, out _, out _);
                objResults.AppendText("Tryptic fragment " + lngIndex + ": " + strPeptideFragMwtWin);
                lngIndex += 1;
            }
            while (strPeptideFragMwtWin.Length > 0);

            objResults.AppendText(string.Empty);
            var random = new Random();
            for (var lngMultipleIteration = 1; lngMultipleIteration <= ITERATIONS_TO_RUN; lngMultipleIteration++)
            {
                // Generate random protein
                var lngProteinLengthRand = random.Next(MAX_PROTEIN_LENGTH - MIN_PROTEIN_LENGTH + 1) + MIN_PROTEIN_LENGTH;

                strProtein = "";
                for (var lngResidueRand = 0; lngResidueRand < lngProteinLengthRand; lngResidueRand++)
                {
                    var strNewResidue = POSSIBLE_RESIDUES.Substring(random.Next(POSSIBLE_RESIDUES.Length), 1);
                    strProtein += strNewResidue;
                }

                objResults.AppendText("Iteration: " + lngMultipleIteration + " = " + strProtein);

                var lngMwtWinResultCount = 0;
                Debug.Write("Starting residue is ");
                var lngStartTime = Program.GetTickCount();
                for (var lngResidueStart = 0; lngResidueStart < strProtein.Length; lngResidueStart++)
                {
                    if (lngResidueStart % 10 == 0)
                    {
                        Debug.Write(lngResidueStart + ", ");
                        Application.DoEvents();
                    }

                    for (var lngResidueEnd = 0; lngResidueEnd < strProtein.Length - lngResidueStart; lngResidueEnd++)
                    {
                        if (lngResidueEnd - lngResidueStart > 50)
                        {
                            break;
                        }

                        var strPeptideResidues = strProtein.Substring(lngResidueStart, lngResidueEnd);
                        strPeptideNameMwtWin[lngMwtWinResultCount] = mMwtWin.Peptide.GetTrypticName(strProtein, strPeptideResidues, out _, out _, true);

                        lngMwtWinResultCount += 1;
                        if (lngMwtWinResultCount > lngMwtWinDimCount)
                        {
                            lngMwtWinDimCount += DIM_CHUNK;
                            Array.Resize(ref strPeptideNameMwtWin, lngMwtWinDimCount + 1);
                        }
                    }
                }

                var lngStopTime = Program.GetTickCount();
                var lngMwtWinWorkTime = lngStopTime - lngStartTime;
                Console.WriteLine("");
                Console.WriteLine("MwtWin time (" + lngMwtWinResultCount + " peptides) = " + lngMwtWinWorkTime + " msec");

                //lngIcr2lsResultCount = 0
                //Debug.Print "Starting residue is ";
                //lngStartTime = GetTickCount()
                //For lngResidueStart = 1 To Len(strProtein)
                //    If lngResidueStart Mod 10 = 0 Then
                //        Debug.Print lngResidueStart & ", ";
                //        DoEvents
                //    End If
                //    ' Use DoEvents on every iteration since Icr2ls is quite slow
                //    DoEvents
                //
                //    For lngResidueEnd = 1 To Len(strProtein) - lngResidueStart
                //        If lngResidueEnd - lngResidueStart > 50 Then
                //            Exit For
                //        End If
                //
                //        strPeptideResidues = Mid(strProtein, lngResidueStart, lngResidueEnd)
                //        strPeptideNameIcr2ls(lngIcr2lsResultCount) = ICRTools.TrypticName(strProtein, strPeptideResidues)
                //
                //        lngIcr2lsResultCount = lngIcr2lsResultCount + 1
                //        If lngIcr2lsResultCount > lngICR2lsDimCount Then
                //            lngICR2lsDimCount = lngICR2lsDimCount + DIM_CHUNK
                //            ReDim Preserve strPeptideNameIcr2ls(lngICR2lsDimCount)
                //        End If
                //    Next lngResidueEnd
                //Next lngResidueStart
                //lngStopTime = GetTickCount()
                //lngIcr2lsWorkTime = lngStopTime - lngStartTime
                //Debug.Print ""
                //Debug.Print "Icr2ls time (" & lngMwtWinResultCount & " peptides) = " & lngIcr2lsWorkTime & " msec"

                //' Check that results match
                //For lngIndex = 0 To lngMwtWinResultCount - 1
                //    If Left(strPeptideNameMwtWin(lngIndex), 1) = "t" Then
                //        If Val(Right(strPeptideNameMwtWin(lngIndex), 1)) < 5 Then
                //            ' Icr2LS does not return the correct name when strPeptideResidues contains 5 or more tryptic peptides
                //            If strPeptideNameMwtWin(lngIndex) <> strPeptideNameIcr2ls(lngIndex) Then
                //                objResults.AppendText("Difference found, index = " & lngIndex & ", " & strPeptideNameMwtWin(lngIndex) & " vs. " & strPeptideNameIcr2ls(lngIndex))
                //                blnDifferenceFound = True
                //            End If
                //        End If
                //    Else
                //        If strPeptideNameMwtWin(lngIndex) <> strPeptideNameIcr2ls(lngIndex) Then
                //            objResults.AppendText("Difference found, index = " & lngIndex & ", " & strPeptideNameMwtWin(lngIndex) & " vs. " & strPeptideNameIcr2ls(lngIndex))
                //            blnDifferenceFound = True
                //        End If
                //    End If
                //Next lngIndex
            }

            objResults.AppendText("Check of Tryptic Sequence functions Complete");

            Cursor = Cursors.Default;
        }

        private void UpdateResultsForCompound(ref Compound compound)
        {
            if (string.IsNullOrEmpty(compound.ErrorDescription))
            {
                txtFormula.Text = compound.FormulaCapitalized;
                FindMass();
            }
            else
            {
                lblStatus.Text = compound.ErrorDescription;
            }
        }

        private void cboStdDevMode_SelectedIndexChanged(object sender, EventArgs eventArgs)
        {
            switch (cboStdDevMode.SelectedIndex)
            {
                case 1:
                    mMwtWin.StdDevMode = ElementAndMassTools.StdDevMode.Scientific;
                    break;
                case 2:
                    mMwtWin.StdDevMode = ElementAndMassTools.StdDevMode.Decimal;
                    break;
                default:
                    mMwtWin.StdDevMode = ElementAndMassTools.StdDevMode.Short;
                    break;
            }
        }

        private void cboWeightMode_SelectedIndexChanged(object sender, EventArgs eventArgs)
        {
            switch (cboWeightMode.SelectedIndex)
            {
                case 1:
                    mMwtWin.SetElementMode(ElementAndMassTools.ElementMassMode.Isotopic);
                    break;
                case 2:
                    mMwtWin.SetElementMode(ElementAndMassTools.ElementMassMode.Integer);
                    break;
                default:
                    mMwtWin.SetElementMode(ElementAndMassTools.ElementMassMode.Average);
                    break;
            }
        }

        private void cmdClose_Click(object sender, EventArgs eventArgs)
        {
            if (mMwtWin != null)
            {
                mMwtWin.ProgressChanged -= mMwtWin_ProgressChanged;
                mMwtWin.ProgressComplete -= mMwtWin_ProgressComplete;
                mMwtWin.ProgressReset -= mMwtWin_ProgressReset;
            }

            mMwtWin = null;
            Close();
            Environment.Exit(0);
        }

        private void cmdConvertToEmpirical_Click(object sender, EventArgs eventArgs)
        {
            lblProgress.Text = string.Empty;

            mMwtWin.Compound.Formula = txtFormula.Text;
            mMwtWin.Compound.ConvertToEmpirical();

            UpdateResultsForCompound(ref mMwtWin.Compound);
        }

        private void cmdExpandAbbreviations_Click(object sender, EventArgs eventArgs)
        {
            lblProgress.Text = string.Empty;

            mMwtWin.Compound.Formula = txtFormula.Text;
            mMwtWin.Compound.ExpandAbbreviations();

            UpdateResultsForCompound(ref mMwtWin.Compound);
        }

        private void cmdFindMass_Click(object sender, EventArgs eventArgs)
        {
            FindMass();
            FindPercentComposition();
        }

        private void cmdTestFunctions_Click(object sender, EventArgs eventArgs)
        {
            TestAccessFunctions();
        }

        private void cmdTestGetTrypticName_Click(object sender, EventArgs eventArgs)
        {
            TestTrypticName();
        }

        private void rtfFormula_TextChanged(object sender, EventArgs e)
        {
            txtRTFSource.Text = rtfFormula.Rtf;
        }

        private void chkShowRTFSource_CheckedChanged(object sender, EventArgs e)
        {
            txtRTFSource.Visible = chkShowRTFSource.Checked;
        }

        private DateTime dtLastUpdate = DateTime.MinValue;

        private void mMwtWin_ProgressChanged(string taskDescription, float percentComplete)
        {
            lblProgress.Text = mMwtWin.ProgressStepDescription + "; " + percentComplete.ToString("0.0") + "% complete";

            if (DateTime.UtcNow.Subtract(dtLastUpdate).TotalMilliseconds > 100d)
            {
                dtLastUpdate = DateTime.UtcNow;
                Application.DoEvents();
            }
        }

        private void mMwtWin_ProgressComplete()
        {
            lblProgress.Text = mMwtWin.ProgressStepDescription + "; 100% complete";
            Application.DoEvents();
        }

        private void mMwtWin_ProgressReset()
        {
            lblProgress.Text = mMwtWin.ProgressStepDescription;
            Application.DoEvents();
        }

        private void cmdTestFormulaFinder_Click(object sender, EventArgs e)
        {
            TestFormulaFinder();
        }
    }
}
