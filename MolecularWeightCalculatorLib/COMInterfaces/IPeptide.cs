using System.Runtime.InteropServices;
using MolecularWeightCalculator.Sequence;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace MolecularWeightCalculator.COMInterfaces
{
    [Guid("BE0BB73D-727C-4607-993F-9E21626FDC13"), InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
    public interface IPeptide
    {
        /// <summary>
        /// Get fragmentation masses
        /// </summary>
        /// <param name="fragSpectrum"></param>
        /// <returns>The number of ions in fragSpectrum[]</returns>
        int GetFragmentationMasses(out FragmentationSpectrumData[] fragSpectrum);

        //// Commented out because COM does not support generic types
        //List<FragmentationSpectrumData> GetFragmentationMassesArray();
        int GetFragmentationSpectrumRequiredDataPoints();
        FragmentationSpectrumOptions GetFragmentationSpectrumOptions();
        double GetPeptideMass();
        int GetModificationSymbol(int modificationId, out string modSymbol, out double modificationMass, out bool indicatesPhosphorylation, out string comment);
        int GetModificationSymbolCount();
        int GetModificationSymbolId(string modSymbol);
        int GetResidue(int residueIndex, out string symbol, out double mass, out bool isModified, out short modificationCount);
        int GetResidueCount();
        int GetResidueCountSpecificResidue(string residueSymbol, bool use3LetterCode);
        int GetResidueModificationIDs(int residueIndex, out int[] modificationIDs);
        string GetResidueSymbolOnly(int residueIndex, bool use3LetterCode);
        string GetSequence1LetterCode();

        string GetSequence(bool use3LetterCode = true,
            bool addSpaceEvery10Residues = false,
            bool separateResiduesWithDash = false,
            bool includeNAndCTerminii = false,
            bool includeModificationSymbols = true);

        string GetSymbolWaterLoss();
        string GetSymbolPhosphoLoss();
        string GetSymbolAmmoniaLoss();

        string GetTrypticName(string proteinResidues, string peptideResidues,
            int proteinSearchStartLoc = 1);

        /// <summary>
        /// Examines <paramref name="peptideResidues"/> to see where they exist in <paramref name="proteinResidues"/>
        /// Constructs a name string based on their position and based on whether the fragment is truly tryptic
        /// In addition, returns the position of the first and last residue in <paramref name="returnResidueStart"/> and <paramref name="returnResidueEnd"/>
        /// </summary>
        /// <param name="proteinResidues"></param>
        /// <param name="peptideResidues"></param>
        /// <param name="returnResidueStart">Output: start peptides of the peptide residues in the protein</param>
        /// <param name="returnResidueEnd">Output: end peptides of the peptide residues in the protein</param>
        /// <param name="ICR2LSCompatible"></param>
        /// <param name="ruleResidues"></param>
        /// <param name="exceptionResidues"></param>
        /// <param name="terminiiSymbol"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="proteinSearchStartLoc"></param>
        string GetTrypticName(string proteinResidues, string peptideResidues,
            out int returnResidueStart,
            out int returnResidueEnd,
            // ReSharper disable once InconsistentNaming
            bool ICR2LSCompatible = false,
            string ruleResidues = Peptide.TRYPTIC_RULE_RESIDUES,
            string exceptionResidues = Peptide.TRYPTIC_EXCEPTION_RESIDUES,
            string terminiiSymbol = Peptide.TERMINII_SYMBOL,
            bool ignoreCase = true,
            int proteinSearchStartLoc = 0);

        string GetTrypticNameMultipleMatches(string proteinResidues,
            string peptideResidues,
            int proteinSearchStartLoc = 0,
            string listDelimiter = ", ");

        /// <summary>
        /// Examines <paramref name="peptideResidues"/> to see where they exist in <paramref name="proteinResidues"/>
        /// Looks for all possible matches, returning them as a comma separated list
        /// </summary>
        /// <param name="proteinResidues"></param>
        /// <param name="peptideResidues"></param>
        /// <param name="returnMatchCount"></param>
        /// <param name="returnResidueStart"></param>
        /// <param name="returnResidueEnd"></param>
        /// <param name="ICR2LSCompatible"></param>
        /// <param name="ruleResidues"></param>
        /// <param name="exceptionResidues"></param>
        /// <param name="terminiiSymbol"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="proteinSearchStartLoc"></param>
        /// <param name="listDelimiter"></param>
        /// <returns>The number of matches</returns>
        string GetTrypticNameMultipleMatches(string proteinResidues,
            string peptideResidues,
            out int returnMatchCount,
            out int returnResidueStart,
            out int returnResidueEnd,
            // ReSharper disable once InconsistentNaming
            bool ICR2LSCompatible = false,
            string ruleResidues = Peptide.TRYPTIC_RULE_RESIDUES,
            string exceptionResidues = Peptide.TRYPTIC_EXCEPTION_RESIDUES,
            string terminiiSymbol = Peptide.TERMINII_SYMBOL,
            bool ignoreCase = true,
            int proteinSearchStartLoc = 0,
            string listDelimiter = ", ");

        string GetTrypticPeptideNext(string proteinResidues,
            int searchStartLoc);

        string GetTrypticPeptideNext(string proteinResidues,
            int searchStartLoc,
            out int returnResidueStart,
            out int returnResidueEnd,
            string ruleResidues = Peptide.TRYPTIC_RULE_RESIDUES,
            string exceptionResidues = Peptide.TRYPTIC_EXCEPTION_RESIDUES,
            string terminiiSymbol = Peptide.TERMINII_SYMBOL);

        string GetTrypticPeptideByFragmentNumber(string proteinResidues,
            short desiredPeptideNumber);

        string GetTrypticPeptideByFragmentNumber(string proteinResidues,
            short desiredPeptideNumber,
            out int returnResidueStart,
            out int returnResidueEnd,
            string ruleResidues = Peptide.TRYPTIC_RULE_RESIDUES,
            string exceptionResidues = Peptide.TRYPTIC_EXCEPTION_RESIDUES,
            string terminiiSymbol = Peptide.TERMINII_SYMBOL,
            bool ignoreCase = true);

        bool CheckSequenceAgainstCleavageRule(string sequence,
            string ruleResidues,
            string exceptionSuffixResidues,
            bool allowPartialCleavage,
            string separationChar = ".",
            string terminiiSymbol = Peptide.TERMINII_SYMBOL,
            bool ignoreCase = true);

        bool CheckSequenceAgainstCleavageRule(string sequence,
            string ruleResidues,
            string exceptionSuffixResidues,
            bool allowPartialCleavage,
            out short ruleMatchCount,
            string separationChar = ".",
            string terminiiSymbol = Peptide.TERMINII_SYMBOL,
            bool ignoreCase = true);

        double ComputeImmoniumMass(double residueMass);
        string LookupIonTypeString(IonType ionType);
        int RemoveAllResidues();
        int RemoveAllModificationSymbols();
        int RemoveModification(string modSymbol);
        int RemoveModificationById(int modificationId);
        int RemoveResidue(int residueIndex);
        int SetCTerminus(string formula, string followingResidue = "", bool use3LetterCode = true);

        int SetCTerminusGroup(CTerminusGroupType cTerminusGroup,
            string followingResidue = "",
            bool use3LetterCode = true);

        void SetDefaultModificationSymbols();
        void SetDefaultOptions();
        void SetFragmentationSpectrumOptions(FragmentationSpectrumOptions newFragSpectrumOptions);
        int SetModificationSymbol(string modSymbol, double modificationMass, bool indicatesPhosphorylation = false, string comment = "");
        int SetNTerminus(string formula, string precedingResidue = "", bool use3LetterCode = true);

        int SetNTerminusGroup(NTerminusGroupType nTerminusGroup,
            string precedingResidue = "",
            bool use3LetterCode = true);

        /// <summary>
        /// Set the residue at the specified index
        /// </summary>
        /// <param name="residueIndex">0-based index of residue</param>
        /// <param name="symbol"></param>
        /// <param name="is3LetterCode"></param>
        /// <param name="phosphorylated"></param>
        int SetResidue(int residueIndex,
            string symbol,
            bool is3LetterCode = true,
            bool phosphorylated = false);

        /// <summary>
        /// Sets modifications on a residue
        /// </summary>
        /// <param name="residueIndex">0-based index of residue</param>
        /// <param name="modificationCount"></param>
        /// <param name="modificationIDs">0-based array</param>
        int SetResidueModifications(int residueIndex, short modificationCount, int[] modificationIDs);

        /// <summary>
        /// Defines the peptide sequence
        /// </summary>
        /// <param name="sequence">Peptide sequence using 1-letter amino acid symbols</param>
        /// <returns>0 if success, 1 if an error</returns>
        /// <remarks>If <paramref name="sequence"/> is blank or contains no valid residues, then will still return 0</remarks>
        int SetSequence1LetterSymbol(string sequence);

        /// <summary>
        /// Defines the peptide sequence
        /// </summary>
        /// <param name="sequence">Peptide sequence</param>
        /// <param name="is3LetterCode">Set to true for 3-letter amino acid symbols, false for 1-letter symbols (for example, R.ABCDEF.R)</param>
        /// <param name="oneLetterCheckForPrefixAndSuffixResidues">Set to true to check for and remove prefix and suffix residues when <paramref name="is3LetterCode"/> = false</param>
        /// <returns>0 if success, 1 if an error</returns>
        /// <remarks>If <paramref name="sequence"/> is blank or contains no valid residues, then will still return 0</remarks>
        int SetSequence(string sequence,
            bool is3LetterCode,
            bool oneLetterCheckForPrefixAndSuffixResidues);

        /// <summary>
        /// Defines the peptide sequence
        /// </summary>
        /// <param name="sequence">Peptide sequence, using 3-letter amino acid symbols (unless <paramref name="is3LetterCode"/> = false)</param>
        /// <param name="nTerminus">N-terminus group</param>
        /// <param name="cTerminus">C-terminus group</param>
        /// <param name="is3LetterCode">Set to true for 3-letter amino acid symbols, false for 1-letter symbols (for example, R.ABCDEF.R)</param>
        /// <param name="oneLetterCheckForPrefixAndSuffixResidues">Set to true to check for and remove prefix and suffix residues when <paramref name="is3LetterCode"/> = false</param>
        /// <param name="threeLetterCheckForPrefixHandSuffixOH">Set to true to check for and remove prefix H and OH when <paramref name="is3LetterCode"/> = true</param>
        /// <param name="addMissingModificationSymbols">Set to true to automatically add missing modification symbols (though the mod masses will be 0)</param>
        /// <returns>0 if success, 1 if an error</returns>
        /// <remarks>If <paramref name="sequence" /> is blank or contains no valid residues, then will still return 0</remarks>
        int SetSequence(string sequence,
            NTerminusGroupType nTerminus = NTerminusGroupType.Hydrogen,
            CTerminusGroupType cTerminus = CTerminusGroupType.Hydroxyl,
            bool is3LetterCode = true,
            bool oneLetterCheckForPrefixAndSuffixResidues = true,
            // ReSharper disable once InconsistentNaming
            bool threeLetterCheckForPrefixHandSuffixOH = true,
            bool addMissingModificationSymbols = false);

        void SetSymbolAmmoniaLoss(string newSymbol);
        void SetSymbolPhosphoLoss(string newSymbol);
        void SetSymbolWaterLoss(string newSymbol);
        void UpdateStandardMasses();
    }
}