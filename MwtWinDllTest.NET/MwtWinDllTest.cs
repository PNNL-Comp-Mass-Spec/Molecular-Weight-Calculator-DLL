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
            // If decimalPlaces is >=0, then a format string is constructed to show the specified number of decimal places
            var textCol = new DataGridTextBoxColumn
            {
                MappingName = mappingName,
                HeaderText = headerText,
                Width = width,
                ReadOnly = isReadOnly
            };

            if (isDateTime)
            {
                textCol.Format = "g";
            }
            else if (decimalPlaces >= 0)
            {
                textCol.Format = "0.";
                for (var i = 0; i < decimalPlaces; i++)
                    textCol.Format += "0";
            }

            tableStyle.GridColumnStyles.Add(textCol);
        }

        private void AppendBoolColumnToTableStyle(ref DataGridTableStyle tableStyle, string mappingName, string headerText, int width = 75, bool isReadOnly = false, bool sourceIsTrueFalse = true)
        {
            // If decimalPlaces is >=0, then a format string is constructed to show the specified number of decimal places
            var boolCol = new DataGridBoolColumn
            {
                MappingName = mappingName,
                HeaderText = headerText,
                Width = width,
                ReadOnly = isReadOnly
            };

            if (sourceIsTrueFalse)
            {
                boolCol.FalseValue = false;
                boolCol.TrueValue = true;
            }
            else
            {
                boolCol.FalseValue = 0;
                boolCol.TrueValue = 1;
            }

            boolCol.AllowNull = false;
            boolCol.NullValue = Convert.DBNull;

            tableStyle.GridColumnStyles.Add(boolCol);
        }

        private void FindPercentComposition()
        {
            mMwtWin.Compound.Formula = txtFormula.Text;

            var pctCompForCarbon = mMwtWin.Compound.GetPercentCompositionForElement(6);
            var pctCompForCarbonString = mMwtWin.Compound.GetPercentCompositionForElementAsString(6);

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
            var dataTable = new DataTable("DataTable1");

            // Create three columns, and add them to the table.
            dataTable.Columns.Add(new DataColumn("Mass", typeof(double)));
            dataTable.Columns.Add(new DataColumn("Intensity", typeof(double)));
            dataTable.Columns.Add(new DataColumn("Symbol", typeof(string)));

            // Add the table to the DataSet.
            myDataSet.Tables.Add(dataTable);

            // Append rows to the table.
            for (var index = 0; index < ionCount; index++)
            {
                // Populates the table.
                var newRow = dataTable.NewRow();
                newRow["Mass"] = fragSpectrum[index].Mass;
                newRow["Intensity"] = fragSpectrum[index].Intensity;
                newRow["Symbol"] = fragSpectrum[index].Symbol;
                dataTable.Rows.Add(newRow);
            }
        }

        private void MakePercentCompositionDataSet(Dictionary<string, string> percentCompositionByElement)
        {

            // Create a DataSet.
            myDataSet = new DataSet("myDataSet");

            // Create a DataTable.
            var dataTable = new DataTable("DataTable1");

            // Create three columns, and add them to the table.
            dataTable.Columns.Add(new DataColumn("Element", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Pct Comp", typeof(string)));


            // Add the table to the DataSet.
            myDataSet.Tables.Add(dataTable);

            // Populates the table

            // Append rows to the table.
            foreach (var item in percentCompositionByElement)
            {
                var newRow = dataTable.NewRow();
                newRow["Element"] = item.Key;
                newRow["Pct Comp"] = item.Value;
                dataTable.Rows.Add(newRow);
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
            int result;
            double mass;

            var results = new frmTextbrowser();

            lblProgress.Text = string.Empty;

            results.Show();
            results.SetText = string.Empty;

            // Test Abbreviations
            var itemCount = mMwtWin.GetAbbreviationCount();
            for (var index = 1; index <= itemCount; index++)
            {
                result = mMwtWin.GetAbbreviation(index, out var symbol, out var formula, out var charge, out var isAminoAcid, out var oneLetterSymbol, out var comment);
                Debug.Assert(result == 0, "");
                Debug.Assert(mMwtWin.GetAbbreviationId(symbol) == index, "");

                result = mMwtWin.SetAbbreviation(symbol, formula, charge, isAminoAcid, oneLetterSymbol, comment);
                Debug.Assert(result == 0, "");
            }

            // Test Caution statements
            itemCount = mMwtWin.GetCautionStatementCount();
            for (var index = 1; index <= itemCount; index++)
            {
                result = mMwtWin.GetCautionStatement(index, out var symbol, out var statement);
                Debug.Assert(result == 0, "");
                Debug.Assert(mMwtWin.GetCautionStatementId(symbol) == index, "");

                result = mMwtWin.SetCautionStatement(symbol, statement);
                Debug.Assert(result == 0, "");
            }

            // Test Element access
            itemCount = mMwtWin.GetElementCount();
            for (var index = 1; index <= itemCount; index++)
            {
                result = mMwtWin.GetElement((short)index, out var symbol, out mass, out var uncertainty, out var charge, out var isotopeCount);
                Debug.Assert(result == 0, "");
                Debug.Assert(mMwtWin.GetElementId(symbol) == index, "");

                result = mMwtWin.SetElement(symbol, mass, uncertainty, charge, false);
                Debug.Assert(result == 0, "");

                var isotopeMasses = new double[isotopeCount + 1 + 1];
                var isotopeAbundances = new float[isotopeCount + 1 + 1];

                short isotopeCount2 = default;
                result = mMwtWin.GetElementIsotopes((short)index, ref isotopeCount2, ref isotopeMasses, ref isotopeAbundances);
                Debug.Assert(isotopeCount == isotopeCount2, "");
                Debug.Assert(result == 0, "");

                result = mMwtWin.SetElementIsotopes(symbol, isotopeCount, ref isotopeMasses, ref isotopeAbundances);
                Debug.Assert(result == 0, "");
            }

            // Test Message Statements access
            itemCount = mMwtWin.GetMessageStatementCount();
            for (var index = 1; index <= itemCount; index++)
            {
                var statement = mMwtWin.GetMessageStatement(index);

                result = mMwtWin.SetMessageStatement(index, statement);
            }

            // Test m/z conversion
            // Switch to isotopic masses

            mMwtWin.SetElementMode(ElementAndMassTools.ElementMassMode.Isotopic);

            mMwtWin.Compound.SetFormula("C19H36O5NH4");
            mass = mMwtWin.Compound.Mass;
            results.AppendText("Mass of " + mMwtWin.Compound.FormulaCapitalized + ": " + mass);
            for (short charge = 1; charge <= 4; charge++)
                results.AppendText("  m/z of " + charge + "+: " + mMwtWin.ConvoluteMass(mass, 0, charge));

            results.AppendText("");

            mMwtWin.Compound.SetFormula("C19H36O5NH3");
            mass = mMwtWin.Compound.Mass;
            results.AppendText("m/z values if we first lose a hydrogen before adding a proton");
            for (short charge = 1; charge <= 4; charge++)
                results.AppendText("  m/z of " + charge + "+: " + mMwtWin.ConvoluteMass(mass, 0, charge));

            // Test Capillary flow functions
            var capFlow = mMwtWin.CapFlow;
            capFlow.SetAutoComputeEnabled(false);
            capFlow.SetBackPressure(2000d, CapillaryFlow.UnitOfPressure.Psi);
            capFlow.SetColumnLength(40d, CapillaryFlow.UnitOfLength.CM);
            capFlow.SetColumnId(50d, CapillaryFlow.UnitOfLength.Microns);
            capFlow.SetSolventViscosity(0.0089d, CapillaryFlow.UnitOfViscosity.Poise);
            capFlow.SetInterparticlePorosity(0.33d);
            capFlow.SetParticleDiameter(2d, CapillaryFlow.UnitOfLength.Microns);
            capFlow.SetAutoComputeEnabled(true);

            results.AppendText("");
            results.AppendText("Check capillary flow calcs");
            results.AppendText("Linear Velocity: " + capFlow.ComputeLinearVelocity(CapillaryFlow.UnitOfLinearVelocity.CmPerSec));
            results.AppendText("Vol flow rate:   " + capFlow.ComputeVolFlowRate(CapillaryFlow.UnitOfFlowRate.NLPerMin) + "  (newly computed)");

            results.AppendText("Vol flow rate:   " + capFlow.GetVolFlowRate());
            results.AppendText("Back pressure:   " + capFlow.ComputeBackPressure(CapillaryFlow.UnitOfPressure.Psi));
            results.AppendText("Column Length:   " + capFlow.ComputeColumnLength(CapillaryFlow.UnitOfLength.CM));
            results.AppendText("Column ID:       " + capFlow.ComputeColumnId(CapillaryFlow.UnitOfLength.Microns));
            results.AppendText("Column Volume:   " + capFlow.ComputeColumnVolume(CapillaryFlow.UnitOfVolume.NL));
            results.AppendText("Dead time:       " + capFlow.ComputeDeadTime(CapillaryFlow.UnitOfTime.Seconds));

            results.AppendText("");

            results.AppendText("Repeat Computations, but in a different order (should give same results)");
            results.AppendText("Vol flow rate:   " + capFlow.ComputeVolFlowRate(CapillaryFlow.UnitOfFlowRate.NLPerMin));
            results.AppendText("Column ID:       " + capFlow.ComputeColumnId(CapillaryFlow.UnitOfLength.Microns));
            results.AppendText("Back pressure:   " + capFlow.ComputeBackPressure(CapillaryFlow.UnitOfPressure.Psi));
            results.AppendText("Column Length:   " + capFlow.ComputeColumnLength(CapillaryFlow.UnitOfLength.CM));

            results.AppendText("");

            results.AppendText("Old Dead time: " + capFlow.GetDeadTime(CapillaryFlow.UnitOfTime.Minutes));

            capFlow.SetAutoComputeMode(CapillaryFlow.AutoComputeMode.VolFlowRateUsingDeadTime);

            capFlow.SetDeadTime(25d, CapillaryFlow.UnitOfTime.Minutes);
            results.AppendText("Dead time is now 25.0 minutes");

            results.AppendText("Vol flow rate: " + capFlow.GetVolFlowRate(CapillaryFlow.UnitOfFlowRate.NLPerMin) + " (auto-computed since AutoComputeMode = acmVolFlowrateUsingDeadTime)");

            // Confirm that auto-compute worked

            results.AppendText("Vol flow rate: " + capFlow.ComputeVolFlowRateUsingDeadTime(out var newPressure, CapillaryFlow.UnitOfFlowRate.NLPerMin, CapillaryFlow.UnitOfPressure.Psi) + "  (confirmation of computed volumetric flow rate)");
            results.AppendText("New pressure: " + newPressure);

            results.AppendText("");

            // Can set a new back pressure, but since auto-compute is on, and the
            // auto-compute mode is acmVolFlowRateUsingDeadTime, the pressure will get changed back to
            // the pressure needed to give a vol flow rate matching the dead time
            capFlow.SetBackPressure(2000d);
            results.AppendText("Pressure set to 2000 psi, but auto-compute mode is acmVolFlowRateUsingDeadTime, so pressure");
            results.AppendText("  was automatically changed back to pressure needed to give vol flow rate matching dead time");
            results.AppendText("Pressure is now: " + capFlow.GetBackPressure(CapillaryFlow.UnitOfPressure.Psi) + " psi (thus, not 2000 as one might expect)");

            capFlow.SetAutoComputeMode(CapillaryFlow.AutoComputeMode.VolFlowRate);
            results.AppendText("Changed auto-compute mode to acmVolFlowrate.  Can now set pressure to 2000 and it will stick; plus, vol flow rate gets computed.");

            capFlow.SetBackPressure(2000d, CapillaryFlow.UnitOfPressure.Psi);

            // Calling GetVolFlowRate will get the new computed vol flow rate (since auto-compute is on)
            results.AppendText("Vol flow rate: " + capFlow.GetVolFlowRate());

            capFlow.SetMassRateSampleMass(1000d);
            capFlow.SetMassRateConcentration(1d, CapillaryFlow.UnitOfConcentration.MicroMolar);
            capFlow.SetMassRateVolFlowRate(600d, CapillaryFlow.UnitOfFlowRate.NLPerMin);
            capFlow.SetMassRateInjectionTime(5d, CapillaryFlow.UnitOfTime.Minutes);

            results.AppendText("Mass flow rate: " + capFlow.GetMassFlowRate(CapillaryFlow.UnitOfMassFlowRate.FmolPerSec) + " fmol/sec");
            results.AppendText("Moles injected: " + capFlow.GetMassRateMolesInjected(CapillaryFlow.UnitOfMolarAmount.FemtoMoles) + " fmoles");

            capFlow.SetMassRateSampleMass(1234d);
            capFlow.SetMassRateConcentration(1d, CapillaryFlow.UnitOfConcentration.NgPerML);

            results.AppendText("Computing mass flow rate for compound weighing 1234 g/mol and at 1 ng/mL concentration");
            results.AppendText("Mass flow rate: " + capFlow.GetMassFlowRate(CapillaryFlow.UnitOfMassFlowRate.AmolPerMin) + " amol/min");
            results.AppendText("Moles injected: " + capFlow.GetMassRateMolesInjected(CapillaryFlow.UnitOfMolarAmount.FemtoMoles) + " fmoles");

            capFlow.SetExtraColumnBroadeningLinearVelocity(4d, CapillaryFlow.UnitOfLinearVelocity.CmPerMin);
            capFlow.SetExtraColumnBroadeningDiffusionCoefficient(0.0003d, CapillaryFlow.UnitOfDiffusionCoefficient.CmSquaredPerMin);
            capFlow.SetExtraColumnBroadeningOpenTubeLength(5d, CapillaryFlow.UnitOfLength.CM);
            capFlow.SetExtraColumnBroadeningOpenTubeId(250d, CapillaryFlow.UnitOfLength.Microns);
            capFlow.SetExtraColumnBroadeningInitialPeakWidthAtBase(30d, CapillaryFlow.UnitOfTime.Seconds);

            results.AppendText("Computing broadening for 30 second wide peak through a 250 um open tube that is 5 cm long (4 cm/min)");
            results.AppendText(capFlow.GetExtraColumnBroadeningResultantPeakWidth(CapillaryFlow.UnitOfTime.Seconds).ToString());

            var fragSpectrumOptions = new Peptide.FragmentationSpectrumOptions();
            fragSpectrumOptions.Initialize();

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

            const string newSeq = "Ala-Cys-Tyr-Glu-Phe-Gly-His-Arg*-Lys-Ala-Cys-Tyr-Glu-Phe-Gly-His-Arg-Lys";
            results.AppendText(newSeq);
            peptide.SetSequence(newSeq);

            peptide.SetSequence("K.TQPLE*VK.-", Peptide.NTerminusGroupType.HydrogenPlusProton, Peptide.CTerminusGroupType.Hydroxyl, is3LetterCode: false);

            results.AppendText(peptide.GetSequence(true, false, true, false));
            results.AppendText(peptide.GetSequence(false, true, false, false));
            results.AppendText(peptide.GetSequence(true, false, true, true));

            peptide.SetCTerminusGroup(Peptide.CTerminusGroupType.None);
            results.AppendText(peptide.GetSequence(true, false, true, true));

            fragSpectrumOptions = peptide.GetFragmentationSpectrumOptions();

            fragSpectrumOptions.DoubleChargeIonsShow = true;
            fragSpectrumOptions.DoubleChargeIonsThreshold = 300f;
            fragSpectrumOptions.IntensityOptions.BYIonShoulder = 0d;

            fragSpectrumOptions.TripleChargeIonsShow = true;
            fragSpectrumOptions.TripleChargeIonsThreshold = 400f;

            fragSpectrumOptions.IonTypeOptions[(int)Peptide.IonType.AIon].ShowIon = true;

            peptide.SetFragmentationSpectrumOptions(fragSpectrumOptions);

            var ionCount = peptide.GetFragmentationMasses(out var fragSpectrum);

            MakeDataSet(ionCount, fragSpectrum);
            dgDataGrid.SetDataBinding(myDataSet, "DataTable1");

            results.AppendText(string.Empty);

            var resultString = string.Empty;
            var convolutedMSDataCount = default(int);

            // Really big formula to test with: C489 H300 F27 Fe8 N72 Ni6 O27 S9
            const short chargeState = 1;
            var addProtonChargeCarrier = true;
            results.AppendText("Isotopic abundance test with Charge=" + chargeState);

            var convolutedMSData2DOneBased = new double[1, 2];
            var formulaIn = "C1255H43O2Cl";
            var success = mMwtWin.ComputeIsotopicAbundances(ref formulaIn, chargeState, ref resultString, ref convolutedMSData2DOneBased, ref convolutedMSDataCount);
            results.AppendText(resultString);

            results.AppendText("Convert isotopic distribution to gaussian");
            var xyVals = new List<KeyValuePair<double, double>>();
            for (var index = 1; index <= convolutedMSDataCount; index++)
                xyVals.Add(new KeyValuePair<double, double>(convolutedMSData2DOneBased[index, 0], convolutedMSData2DOneBased[index, 1]));

            const int resolution = 2000;
            const double resolutionMass = 1000d;
            const int qualityFactor = 50;

            var gaussianData = mMwtWin.ConvertStickDataToGaussian2DArray(xyVals, resolution, resolutionMass, qualityFactor);

            var gaussianResults = new StringBuilder();
            gaussianResults.AppendLine("m/z" + "\t" + "Intensity");
            for (var index = 0; index < gaussianData.Count; index++)
            {
                if (gaussianData[index].Key >= 15175d && gaussianData[index].Key < 15193d)
                {
                    gaussianResults.AppendLine(gaussianData[index].Key.ToString("0.000") + "\t" + gaussianData[index].Value.ToString("0.000"));
                }
            }

            results.AppendText(gaussianResults.ToString());

            addProtonChargeCarrier = false;
            results.AppendText("Isotopic abundance test with Charge=" + chargeState + "; do not add a proton charge carrier");
            formulaIn = "C1255H43O2Cl";
            success = mMwtWin.ComputeIsotopicAbundances(ref formulaIn, chargeState, ref resultString, ref convolutedMSData2DOneBased, ref convolutedMSDataCount, addProtonChargeCarrier);
            results.AppendText(resultString);
        }

        public void TestFormulaFinder()
        {
            var mwtWin = new MolecularWeightTool();

            mwtWin.SetElementMode(ElementAndMassTools.ElementMassMode.Isotopic);

            mwtWin.FormulaFinder.CandidateElements.Clear();

            mwtWin.FormulaFinder.AddCandidateElement("C");
            mwtWin.FormulaFinder.AddCandidateElement("H");
            mwtWin.FormulaFinder.AddCandidateElement("N");
            mwtWin.FormulaFinder.AddCandidateElement("O");

            // Abbreviations are supported, for example Serine
            mwtWin.FormulaFinder.AddCandidateElement("Ser");

            var searchOptions = new FormulaFinderOptions
            {
                LimitChargeRange = false,
                ChargeMin = 1,
                ChargeMax = 1,
                FindTargetMz = false
            };

            cmdTestFormulaFinder.Enabled = false;
            Application.DoEvents();

            if (cboFormulaFinderTestMode.SelectedIndex == 0)
                FormulaFinderTest1(mwtWin, searchOptions, cboFormulaFinderTestMode.Text);
            if (cboFormulaFinderTestMode.SelectedIndex == 1)
                FormulaFinderTest2(mwtWin, searchOptions, cboFormulaFinderTestMode.Text);
            if (cboFormulaFinderTestMode.SelectedIndex == 2)
                FormulaFinderTest3(mwtWin, searchOptions, cboFormulaFinderTestMode.Text);
            if (cboFormulaFinderTestMode.SelectedIndex == 3)
                FormulaFinderTest4(mwtWin, searchOptions, cboFormulaFinderTestMode.Text);
            if (cboFormulaFinderTestMode.SelectedIndex == 4)
                FormulaFinderTest5(mwtWin, searchOptions, cboFormulaFinderTestMode.Text);
            if (cboFormulaFinderTestMode.SelectedIndex == 5)
                FormulaFinderTest6(mwtWin, searchOptions, cboFormulaFinderTestMode.Text);

            cmdTestFormulaFinder.Enabled = true;

            if (cboFormulaFinderTestMode.SelectedIndex > 5)
            {
                MessageBox.Show("Formula finder test mode not recognized", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void FormulaFinderTest1(MolecularWeightTool mwtWin, FormulaFinderOptions searchOptions, string currentTask)
        {

            // Search for 200 Da, +/- 0.05 Da
            var results = mwtWin.FormulaFinder.FindMatchesByMass(200d, 0.05d, searchOptions);
            ShowFormulaFinderResults(currentTask, searchOptions, results);
        }

        private void FormulaFinderTest2(MolecularWeightTool mwtWin, FormulaFinderOptions searchOptions, string currentTask)
        {

            // Search for 200 Da, +/- 250 ppm
            var results = mwtWin.FormulaFinder.FindMatchesByMassPPM(200d, 250d, searchOptions);
            ShowFormulaFinderResults(currentTask, searchOptions, results, true);
        }

        private void FormulaFinderTest3(MolecularWeightTool mwtWin, FormulaFinderOptions searchOptions, string currentTask)
        {
            searchOptions.LimitChargeRange = true;
            searchOptions.ChargeMin = -4;
            searchOptions.ChargeMax = 6;

            // Search for 200 Da, +/- 250 ppm
            var results = mwtWin.FormulaFinder.FindMatchesByMassPPM(200d, 250d, searchOptions);
            ShowFormulaFinderResults(currentTask, searchOptions, results, true);
        }

        private void FormulaFinderTest4(MolecularWeightTool mwtWin, FormulaFinderOptions searchOptions, string currentTask)
        {
            searchOptions.LimitChargeRange = true;
            searchOptions.ChargeMin = -4;
            searchOptions.ChargeMax = 6;
            searchOptions.FindTargetMz = true;

            // Search for 100 m/z, +/- 250 ppm
            var results = mwtWin.FormulaFinder.FindMatchesByMassPPM(100d, 250d, searchOptions);
            ShowFormulaFinderResults(currentTask, searchOptions, results, true);
        }

        private void FormulaFinderTest5(MolecularWeightTool mwtWin, FormulaFinderOptions searchOptions, string currentTask)
        {
            mwtWin.FormulaFinder.CandidateElements.Clear();

            mwtWin.FormulaFinder.AddCandidateElement("C", 70d);
            mwtWin.FormulaFinder.AddCandidateElement("H", 10d);
            mwtWin.FormulaFinder.AddCandidateElement("N", 10d);
            mwtWin.FormulaFinder.AddCandidateElement("O", 10d);

            // Search for percent composition results, maximum mass 400 Da
            var results = mwtWin.FormulaFinder.FindMatchesByPercentComposition(400d, 1d, searchOptions);
            ShowFormulaFinderResults(currentTask, searchOptions, results, false, true);
        }

        private void FormulaFinderTest6(MolecularWeightTool mwtWin, FormulaFinderOptions searchOptions, string currentTask)
        {
            searchOptions.SearchMode = FormulaFinderOptions.SearchModes.Bounded;

            // Search for 200 Da, +/- 250 ppm
            var results = mwtWin.FormulaFinder.FindMatchesByMassPPM(200d, 250d, searchOptions);
            ShowFormulaFinderResults(currentTask, searchOptions, results, true);
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
            var dataTable = new DataTable("DataTable1");

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
            dataTable.Columns.Add(new DataColumn("Formula", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Mass", typeof(double)));
            dataTable.Columns.Add(new DataColumn(massColumnName, typeof(double)));
            dataTable.Columns.Add(new DataColumn("Charge", typeof(int)));
            dataTable.Columns.Add(new DataColumn("M/Z", typeof(double)));
            dataTable.Columns.Add(new DataColumn("PercentCompInfo", typeof(string)));

            if (myDataSet.Tables.Count > 0)
            {
                myDataSet.Tables.Clear();
            }

            // Add the table to the DataSet.
            myDataSet.Tables.Add(dataTable);

            var percentCompInfo = new StringBuilder();

            foreach (var result in results)
            {
                // Populates the table.
                var newRow = dataTable.NewRow();
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
                    newRow["M/Z"] = Math.Round(result.Mz, 3);
                }

                if (percentCompositionSearch)
                {
                    percentCompInfo.Clear();

                    foreach (var percentCompValue in result.PercentComposition)
                        percentCompInfo.Append(" " + percentCompValue.Key + "=" + percentCompValue.Value.ToString("0.00") + "%");

                    newRow["PercentCompInfo"] = percentCompInfo.ToString().TrimStart();
                }
                else
                {
                    newRow["PercentCompInfo"] = string.Empty;
                }

                dataTable.Rows.Add(newRow);
            }

            dgDataGrid.SetDataBinding(myDataSet, "DataTable1");
        }

        private void TestTrypticName()
        {
            const short dimChunk = 1000;

            const short iterationsToRun = 5;
            const short minProteinLength = 50;
            const short maxProteinLength = 200;
            const string possibleResidues = "ACDEFGHIKLMNPQRSTVWY";

            string peptideFragMwtWin;
            const int matchCount = default(int);

            var results = new frmTextbrowser();

            lblProgress.Text = string.Empty;

            int mwtWinDimCount = dimChunk;
            var peptideNameMwtWin = new string[mwtWinDimCount + 1];

            Cursor = Cursors.WaitCursor;

            results.Show();
            results.SetText = string.Empty;

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

            //var protein = "MGNISFLTGGNPSSPQSIAESIYQLENTSVVFLSAWQRTTPDFQRAARASQEAMLHLDHIVNEIMRNRDQLQADGTYTGSQLEGLLNISRAVSVSPVTRAEQDDLANYGPGNGVLPSAGSSISMEKLLNKIKHRRTNSANFRIGASGEHIFIIGVDKPNRQPDSIVEFIVGDFCQHCSDIAALI"

            // Bigger protein
            var protein = "MMKANVTKKTLNEGLGLLERVIPSRSSNPLLTALKVETSEGGLTLSGTNLEIDLSCFVPAEVQQPENFVVPAHLFAQIVRNLGGELVELELSGQELSVRSGGSDFKLQTGDIEAYPPLSFPAQADVSLDGGELSRAFSSVRYAASNEAFQAVFRGIKLEHHGESARVVASDGYRVAIRDFPASGDGKNLIIPARSVDELIRVLKDGEARFTYGDGMLTVTTDRVKMNLKLLDGDFPDYERVIPKDIKLQVTLPATALKEAVNRVAVLADKNANNRVEFLVSEGTLRLAAEGDYGRAQDTLSVTQGGTEQAMSLAFNARHVLDALGPIDGDAELLFSGSTSPAIFRARRWGRRVYGGHGHAARLRGLLRPLRGMSALAHHPESSPPLEPRPEFA";

            results.AppendText("Testing GetTrypticNameMultipleMatches() function");
            results.AppendText("MatchList for NL: " + mMwtWin.Peptide.GetTrypticNameMultipleMatches(protein, "NL", matchCount));
            results.AppendText("MatchCount = " + matchCount);

            results.AppendText(string.Empty);
            results.AppendText("Testing GetTrypticPeptideByFragmentNumber function");
            for (var index = 0; index < 43; index++)
            {
                peptideFragMwtWin = mMwtWin.Peptide.GetTrypticPeptideByFragmentNumber(protein, (short)index, out var residueStart, out var residueEnd);
                //peptideFragIcr2ls = ICRTools.TrypticPeptide(protein, index);
                //
                //Debug.Assert(peptideFragMwtWin == peptideFragIcr2ls);

                if (peptideFragMwtWin.Length > 1)
                {
                    // Make sure residueStart and residueEnd are correct
                    // Do this using .GetTrypticNameMultipleMatches()
                    var peptideName = mMwtWin.Peptide.GetTrypticNameMultipleMatches(protein, protein.Substring(residueStart, residueEnd - residueStart + 1));
                    Debug.Assert(peptideName.IndexOf("t" + index, StringComparison.Ordinal) >= 0, "");
                }
            }

            results.AppendText("Check of GetTrypticPeptideByFragmentNumber Complete");
            results.AppendText(string.Empty);

            results.AppendText("Test tryptic digest of: " + protein);
            var fragIndex = 1;
            do
            {
                peptideFragMwtWin = mMwtWin.Peptide.GetTrypticPeptideByFragmentNumber(protein, (short)fragIndex, out _, out _);
                results.AppendText("Tryptic fragment " + fragIndex + ": " + peptideFragMwtWin);
                fragIndex += 1;
            }
            while (peptideFragMwtWin.Length > 0);

            results.AppendText(string.Empty);
            var random = new Random();
            for (var multipleIteration = 1; multipleIteration <= iterationsToRun; multipleIteration++)
            {
                // Generate random protein
                var proteinLengthRand = random.Next(maxProteinLength - minProteinLength + 1) + minProteinLength;

                protein = "";
                for (var residueRand = 0; residueRand < proteinLengthRand; residueRand++)
                {
                    var newResidue = possibleResidues.Substring(random.Next(possibleResidues.Length), 1);
                    protein += newResidue;
                }

                results.AppendText("Iteration: " + multipleIteration + " = " + protein);

                var mwtWinResultCount = 0;
                Debug.Write("Starting residue is ");
                var startTime = Program.GetTickCount();
                for (var residueStart = 0; residueStart < protein.Length; residueStart++)
                {
                    if (residueStart % 10 == 0)
                    {
                        Debug.Write(residueStart + ", ");
                        Application.DoEvents();
                    }

                    for (var residueEnd = 0; residueEnd < protein.Length - residueStart; residueEnd++)
                    {
                        if (residueEnd - residueStart > 50)
                        {
                            break;
                        }

                        var peptideResidues = protein.Substring(residueStart, residueEnd);
                        peptideNameMwtWin[mwtWinResultCount] = mMwtWin.Peptide.GetTrypticName(protein, peptideResidues, out _, out _, true);

                        mwtWinResultCount += 1;
                        if (mwtWinResultCount > mwtWinDimCount)
                        {
                            mwtWinDimCount += dimChunk;
                            Array.Resize(ref peptideNameMwtWin, mwtWinDimCount + 1);
                        }
                    }
                }

                var stopTime = Program.GetTickCount();
                var mwtWinWorkTime = stopTime - startTime;
                Console.WriteLine("");
                Console.WriteLine("MwtWin time (" + mwtWinResultCount + " peptides) = " + mwtWinWorkTime + " msec");

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

            results.AppendText("Check of Tryptic Sequence functions Complete");

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

        private DateTime lastUpdateTime = DateTime.MinValue;

        private void mMwtWin_ProgressChanged(string taskDescription, float percentComplete)
        {
            lblProgress.Text = mMwtWin.ProgressStepDescription + "; " + percentComplete.ToString("0.0") + "% complete";

            if (DateTime.UtcNow.Subtract(lastUpdateTime).TotalMilliseconds > 100d)
            {
                lastUpdateTime = DateTime.UtcNow;
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
