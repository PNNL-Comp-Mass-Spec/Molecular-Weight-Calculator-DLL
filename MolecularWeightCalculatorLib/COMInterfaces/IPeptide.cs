using System.Runtime.InteropServices;
using MolecularWeightCalculator.Sequence;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace MolecularWeightCalculator.COMInterfaces
{
    /// <summary>
    /// Peptide information
    /// </summary>
    [Guid("BE0BB73D-727C-4607-993F-9E21626FDC13"), InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
    public interface IPeptide
    {
        // Ignore Spelling: acetyl, carbamyl, frag, immonium, phospho, phosphorylation, tryptic

        /// <summary>
        /// Get fragmentation masses
        /// </summary>
        /// <param name="fragSpectrum"></param>
        /// <returns>The number of ions in fragSpectrum[]</returns>
        int GetFragmentationMasses(out FragmentationSpectrumData[] fragSpectrum);

        // Commented out because COM does not support generic types
        // List<FragmentationSpectrumData> GetFragmentationMassesArray();

        int GetFragmentationSpectrumRequiredDataPoints();

        /// <summary>
        /// Get fragmentation spectrum options
        /// </summary>
        FragmentationSpectrumOptions GetFragmentationSpectrumOptions();

        /// <summary>
        /// Get the mass of the entire peptide, including modifications
        /// </summary>
        double GetPeptideMass();

        /// <summary>
        /// Get information on the modification with modificationId
        /// </summary>
        /// <param name="modificationId"></param>
        /// <param name="modSymbol"></param>
        /// <param name="modificationMass"></param>
        /// <param name="indicatesPhosphorylation"></param>
        /// <param name="comment"></param>
        /// <returns>True if success, false if modificationId is invalid</returns>
        bool GetModificationSymbol(
            int modificationId,
            out string modSymbol,
            out double modificationMass,
            out bool indicatesPhosphorylation,
            out string comment);

        /// <summary>
        /// The number of modifications defined
        /// </summary>
        int GetModificationSymbolCount();

        /// <summary>
        /// Get the modification symbol ID for the given modification
        /// </summary>
        /// <param name="modSymbol"></param>
        /// <returns>Id if found, -1 if not found</returns>
        int GetModificationSymbolId(string modSymbol);

        /// <summary>
        /// Get information about the residue at the given index
        /// </summary>
        /// <param name="residueIndex"></param>
        /// <param name="symbol"></param>
        /// <param name="mass"></param>
        /// <param name="isModified"></param>
        /// <param name="modificationCount"></param>
        /// <returns>True if success, false if residueIndex is invalid</returns>
        bool GetResidue(int residueIndex, out string symbol, out double mass, out bool isModified, out short modificationCount);

        /// <summary>
        /// Get the number of residues in this peptide
        /// </summary>
        int GetResidueCount();

        /// <summary>
        /// Determine the number of occurrences of the given residue in the loaded sequence
        /// </summary>
        /// <param name="residueSymbol"></param>
        /// <param name="use3LetterCode"></param>
        /// <returns>Count of matching residues</returns>
        int GetResidueCountSpecificResidue(string residueSymbol, bool use3LetterCode);

        /// <summary>
        /// Get the array of modification IDs
        /// </summary>
        /// <param name="residueIndex"></param>
        /// <param name="modificationIDs"></param>
        /// <returns>The number of modifications</returns>
        int GetResidueModificationIDs(int residueIndex, out int[] modificationIDs);

        /// <summary>
        /// Obtain the symbol at the given residue number, or string.Empty if an invalid residue number
        /// </summary>
        /// <param name="residueIndex"></param>
        /// <param name="use3LetterCode"></param>
        /// <returns>Symbol if residueIndex is valid, otherwise an empty string</returns>
        string GetResidueSymbolOnly(int residueIndex, bool use3LetterCode);

        /// <summary>
        /// Get the sequence of the current peptide, using 1-letter amino acid symbols
        /// </summary>
        string GetSequence1LetterCode();

        /// <summary>
        /// Get the sequence of the current peptide, formatting as specified
        /// </summary>
        /// <param name="use3LetterCode"></param>
        /// <param name="addSpaceEvery10Residues"></param>
        /// <param name="separateResiduesWithDash"></param>
        /// <param name="includeNAndCTerminii"></param>
        /// <param name="includeModificationSymbols"></param>
        string GetSequence(
            bool use3LetterCode = true,
            bool addSpaceEvery10Residues = false,
            bool separateResiduesWithDash = false,
            bool includeNAndCTerminii = false,
            bool includeModificationSymbols = true);

        /// <summary>
        /// Get the ammonia loss formula
        /// </summary>
        /// <remarks>
        /// Defaults to -NH3
        /// </remarks>
        string GetSymbolAmmoniaLoss();

        /// <summary>
        /// Get the phosphorylation loss formula
        /// </summary>
        /// <remarks>
        /// Defaults to -H3PO4
        /// </remarks>
        string GetSymbolPhosphoLoss();

        /// <summary>
        /// Get the water loss formula
        /// </summary>
        /// <remarks>
        /// Defaults to -H2O
        /// </remarks>
        string GetSymbolWaterLoss();

        /// <summary>
        /// Examines <paramref name="peptideResidues"/> to see where they exist in <paramref name="proteinResidues"/>
        /// Constructs a name string based on their position and based on whether the fragment is truly tryptic
        /// </summary>
        /// <param name="proteinResidues"></param>
        /// <param name="peptideResidues"></param>
        /// <param name="proteinSearchStartLoc"></param>
        string GetTrypticName(string proteinResidues, string peptideResidues, int proteinSearchStartLoc = 1);

        /// <summary>
        /// Examines <paramref name="peptideResidues"/> to see where they exist in <paramref name="proteinResidues"/>
        /// Constructs a name string based on their position and based on whether the fragment is truly tryptic
        /// In addition, returns the position of the first and last residue in <paramref name="returnResidueStart"/> and <paramref name="returnResidueEnd"/>
        /// </summary>
        /// <remarks>
        /// <para>
        /// The tryptic name is in the following format:
        ///   t1  indicates tryptic peptide 1
        ///   t2 represents tryptic peptide 2, etc.
        ///   t1.2  indicates tryptic peptide 1, plus one more tryptic peptide, i.e. t1 and t2
        ///   t5.2  indicates tryptic peptide 5, plus one more tryptic peptide, i.e. t5 and t6
        ///   t5.3  indicates tryptic peptide 5, plus two more tryptic peptides, i.e. t5, t6, and t7
        ///   40.52  means that the residues are not tryptic, and simply range from residue 40 to 52
        /// </para>
        /// <para>
        /// If the peptide residues are not present in proteinResidues, returns an empty string
        /// </para>
        /// <para>
        /// Since a peptide can occur multiple times in a protein, you can set proteinSearchStartLoc to a value larger than 1 to ignore previous hits
        /// </para>
        /// <para>
        /// If ICR2LSCompatible is True, the values returned when a peptide is not tryptic are modified to
        /// range from the starting residue to the ending residue +1
        /// </para>
        /// <para>
        /// returnResidueEnd is always equal to the position of the final residue, regardless of ICR2LSCompatible
        /// </para>
        /// </remarks>
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
        string GetTrypticName(
            string proteinResidues,
            string peptideResidues,
            out int returnResidueStart,
            out int returnResidueEnd,
            // ReSharper disable once InconsistentNaming
            bool ICR2LSCompatible = false,
            string ruleResidues = Peptide.TRYPTIC_RULE_RESIDUES,
            string exceptionResidues = Peptide.TRYPTIC_EXCEPTION_RESIDUES,
            string terminiiSymbol = Peptide.TERMINII_SYMBOL,
            bool ignoreCase = true,
            int proteinSearchStartLoc = 0);

        /// <summary>
        /// Examines <paramref name="peptideResidues"/> to see where they exist in <paramref name="proteinResidues"/>
        /// Looks for all possible matches, returning them as a comma separated list
        /// </summary>
        /// <remarks>See GetTrypticName for additional information</remarks>
        /// <param name="proteinResidues"></param>
        /// <param name="peptideResidues"></param>
        /// <param name="proteinSearchStartLoc"></param>
        /// <param name="listDelimiter"></param>
        string GetTrypticNameMultipleMatches(
            string proteinResidues,
            string peptideResidues,
            int proteinSearchStartLoc = 0,
            string listDelimiter = ", ");

        /// <summary>
        /// Examines <paramref name="peptideResidues"/> to see where they exist in <paramref name="proteinResidues"/>
        /// Looks for all possible matches, returning them as a comma separated list
        /// </summary>
        /// <remarks>See GetTrypticName for additional information</remarks>
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
        string GetTrypticNameMultipleMatches(
            string proteinResidues,
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

        /// <summary>
        /// Get the next tryptic peptide in proteinResidues, starting the search at startIndex
        /// </summary>
        /// <param name="proteinResidues"></param>
        /// <param name="searchStartLoc"></param>
        string GetTrypticPeptideNext(string proteinResidues, int searchStartLoc);

        /// <summary>
        /// Get the next tryptic peptide in proteinResidues, starting the search at startIndex
        /// </summary>
        /// <param name="proteinResidues"></param>
        /// <param name="searchStartLoc"></param>
        /// <param name="returnResidueStart"></param>
        /// <param name="returnResidueEnd"></param>
        /// <param name="ruleResidues"></param>
        /// <param name="exceptionResidues"></param>
        /// <param name="terminiiSymbol"></param>
        string GetTrypticPeptideNext(
            string proteinResidues,
            int searchStartLoc,
            out int returnResidueStart,
            out int returnResidueEnd,
            string ruleResidues = Peptide.TRYPTIC_RULE_RESIDUES,
            string exceptionResidues = Peptide.TRYPTIC_EXCEPTION_RESIDUES,
            string terminiiSymbol = Peptide.TERMINII_SYMBOL);

        /// <summary>
        /// Obtain the desired tryptic peptide from proteinResidues
        /// </summary>
        /// <param name="proteinResidues"></param>
        /// <param name="desiredPeptideNumber"></param>
        string GetTrypticPeptideByFragmentNumber(string proteinResidues, int desiredPeptideNumber);

        /// <summary>
        /// Obtain the desired tryptic peptide from proteinResidues
        /// </summary>
        /// <param name="proteinResidues"></param>
        /// <param name="desiredPeptideNumber"></param>
        /// <param name="returnResidueStart"></param>
        /// <param name="returnResidueEnd"></param>
        /// <param name="ruleResidues"></param>
        /// <param name="exceptionResidues"></param>
        /// <param name="terminiiSymbol"></param>
        /// <param name="ignoreCase"></param>
        string GetTrypticPeptideByFragmentNumber(
            string proteinResidues,
            int desiredPeptideNumber,
            out int returnResidueStart,
            out int returnResidueEnd,
            string ruleResidues = Peptide.TRYPTIC_RULE_RESIDUES,
            string exceptionResidues = Peptide.TRYPTIC_EXCEPTION_RESIDUES,
            string terminiiSymbol = Peptide.TERMINII_SYMBOL,
            bool ignoreCase = true);

        /// <summary>
        /// Examines sequence to see if it matches the cleavage rule
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="ruleResidues"></param>
        /// <param name="exceptionSuffixResidues"></param>
        /// <param name="allowPartialCleavage"></param>
        /// <param name="separationChar"></param>
        /// <param name="terminiiSymbol"></param>
        /// <param name="ignoreCase"></param>
        bool CheckSequenceAgainstCleavageRule(
            string sequence,
            string ruleResidues,
            string exceptionSuffixResidues,
            bool allowPartialCleavage,
            string separationChar = ".",
            string terminiiSymbol = Peptide.TERMINII_SYMBOL,
            bool ignoreCase = true);

        /// <summary>
        /// Examines sequence to see if it matches the cleavage rule
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="ruleResidues"></param>
        /// <param name="exceptionSuffixResidues"></param>
        /// <param name="allowPartialCleavage"></param>
        /// <param name="ruleMatchCount"></param>
        /// <param name="separationChar"></param>
        /// <param name="terminiiSymbol"></param>
        /// <param name="ignoreCase"></param>
        bool CheckSequenceAgainstCleavageRule(
            string sequence,
            string ruleResidues,
            string exceptionSuffixResidues,
            bool allowPartialCleavage,
            out short ruleMatchCount,
            string separationChar = ".",
            string terminiiSymbol = Peptide.TERMINII_SYMBOL,
            bool ignoreCase = true);

        /// <summary>
        /// Given a residue mass, return the value after immonium loss
        /// </summary>
        /// <param name="residueMass"></param>
        double ComputeImmoniumMass(double residueMass);

        /// <summary>
        /// Convert from IonType enum to string
        /// </summary>
        /// <param name="ionType"></param>
        string LookupIonTypeString(IonType ionType);

        /// <summary>
        /// Remove all residues from this peptide
        /// </summary>
        void RemoveAllResidues();

        /// <summary>
        /// Remove all possible modification symbols
        /// </summary>
        /// <remarks>
        /// Removing all modifications will invalidate any modifications present in a sequence
        /// </remarks>
        void RemoveAllModificationSymbols();

        /// <summary>
        /// Remove the modification with the given symbol
        /// </summary>
        /// <param name="modSymbol"></param>
        /// <returns>
        /// 0 if found and removed; 1 if error
        /// </returns>
        int RemoveModification(string modSymbol);

        /// <summary>
        /// Remove the modification with the given modification Id
        /// </summary>
        /// <param name="modificationId"></param>
        /// <returns>
        /// 0 if found and removed; 1 if error
        /// </returns>
        int RemoveModificationById(int modificationId);

        /// <summary>
        /// Remove the residue at the given index
        /// </summary>
        /// <param name="residueIndex"></param>
        /// <returns> 0 if found and removed; 1 if error</returns>
        int RemoveResidue(int residueIndex);

        /// <summary>
        /// Set the C terminus group using an empirical formula
        /// </summary>
        /// <remarks>
        /// Typical C terminus groups
        /// Free Acid = OH
        /// Amide = NH2
        /// </remarks>
        /// <param name="formula"></param>
        /// <param name="followingResidue"></param>
        /// <param name="use3LetterCode"></param>
        /// <returns>True if success, false if an error</returns>
        bool SetCTerminus(string formula, string followingResidue = "", bool use3LetterCode = true);

        /// <summary>
        /// Set the C terminus group using an enum
        /// </summary>
        /// <param name="cTerminusGroup"></param>
        /// <param name="followingResidue"></param>
        /// <param name="use3LetterCode"></param>
        /// <returns>True if success, false if an error</returns>
        bool SetCTerminusGroup(CTerminusGroupType cTerminusGroup, string followingResidue = "", bool use3LetterCode = true);

        /// <summary>
        /// Reset modification symbols to defaults
        /// </summary>
        void SetDefaultModificationSymbols();

        /// <summary>
        /// Reset options to defaults
        /// </summary>
        /// <remarks>
        /// Also calls SetDefaultModificationSymbols
        /// </remarks>
        void SetDefaultOptions();

        /// <summary>
        /// Set MS/MS fragmentation options
        /// </summary>
        /// <param name="newFragSpectrumOptions"></param>
        void SetFragmentationSpectrumOptions(FragmentationSpectrumOptions newFragSpectrumOptions);

        /// <summary>
        /// Adds a new modification or updates an existing one (based on modSymbol)
        /// </summary>
        /// <param name="modSymbol"></param>
        /// <param name="modificationMass"></param>
        /// <param name="indicatesPhosphorylation"></param>
        /// <param name="comment"></param>
        /// <returns>True if success, false if an error</returns>
        bool SetModificationSymbol(string modSymbol, double modificationMass, bool indicatesPhosphorylation = false, string comment = "");

        /// <summary>
        /// Set the N terminus group using an empirical formula
        /// </summary>
        /// <remarks>
        /// Typical N terminus groups
        /// Hydrogen = H
        /// Acetyl = C2OH3
        /// PyroGlu = C5O2NH6
        /// Carbamyl = CONH2
        /// PTC = C7H6NS
        /// </remarks>
        /// <param name="formula"></param>
        /// <param name="precedingResidue"></param>
        /// <param name="use3LetterCode"></param>
        /// <returns>True if success, false if an error</returns>
        bool SetNTerminus(string formula, string precedingResidue = "", bool use3LetterCode = true);

        /// <summary>
        /// Set the N terminus group using an enum
        /// </summary>
        /// <param name="nTerminusGroup"></param>
        /// <param name="precedingResidue"></param>
        /// <param name="use3LetterCode"></param>
        /// <returns>True if success, false if an error</returns>
        bool SetNTerminusGroup(NTerminusGroupType nTerminusGroup, string precedingResidue = "", bool use3LetterCode = true);

        /// <summary>
        /// Set the residue at the specified index
        /// </summary>
        /// <param name="residueIndex">0-based index of residue</param>
        /// <param name="symbol"></param>
        /// <param name="is3LetterCode"></param>
        /// <param name="phosphorylated"></param>
        int SetResidue(int residueIndex, string symbol, bool is3LetterCode = true, bool phosphorylated = false);

        /// <summary>
        /// Sets modifications on a residue
        /// </summary>
        /// <param name="residueIndex">0-based index of residue</param>
        /// <param name="modificationCount"></param>
        /// <param name="modificationIDs">0-based array</param>
        /// <returns>True if success, false if an error</returns>
        bool SetResidueModifications(int residueIndex, int modificationCount, int[] modificationIDs);

        /// <summary>
        /// Defines the peptide sequence
        /// </summary>
        /// <remarks>If <paramref name="sequence"/> is blank or contains no valid residues, then will still return 0</remarks>
        /// <param name="sequence">Peptide sequence using 1-letter amino acid symbols</param>
        /// <returns>True if success, false if an error</returns>
        bool SetSequence1LetterSymbol(string sequence);

        /// <summary>
        /// Defines the peptide sequence
        /// </summary>
        /// <remarks>If <paramref name="sequence"/> is blank or contains no valid residues, then will still return 0</remarks>
        /// <param name="sequence">Peptide sequence</param>
        /// <param name="is3LetterCode">Set to true for 3-letter amino acid symbols, false for 1-letter symbols (for example, R.ABCDEF.R)</param>
        /// <param name="oneLetterCheckForPrefixAndSuffixResidues">Set to true to check for and remove prefix and suffix residues when <paramref name="is3LetterCode"/> = false</param>
        /// <returns>True if success, false if an error</returns>
        bool SetSequence(string sequence, bool is3LetterCode, bool oneLetterCheckForPrefixAndSuffixResidues);

        /// <summary>
        /// Defines the peptide sequence
        /// </summary>
        /// <remarks>If <paramref name="sequence" /> is blank or contains no valid residues, then will still return 0</remarks>
        /// <param name="sequence">Peptide sequence, using 3-letter amino acid symbols (unless <paramref name="is3LetterCode"/> = false)</param>
        /// <param name="nTerminus">N-terminus group</param>
        /// <param name="cTerminus">C-terminus group</param>
        /// <param name="is3LetterCode">Set to true for 3-letter amino acid symbols, false for 1-letter symbols (for example, R.ABCDEF.R)</param>
        /// <param name="oneLetterCheckForPrefixAndSuffixResidues">Set to true to check for and remove prefix and suffix residues when <paramref name="is3LetterCode"/> = false</param>
        /// <param name="threeLetterCheckForPrefixHandSuffixOH">Set to true to check for and remove prefix H and OH when <paramref name="is3LetterCode"/> = true</param>
        /// <param name="addMissingModificationSymbols">Set to true to automatically add missing modification symbols (though the mod masses will be 0)</param>
        /// <returns>True if success, false if an error</returns>
        bool SetSequence(
            string sequence,
            NTerminusGroupType nTerminus = NTerminusGroupType.Hydrogen,
            CTerminusGroupType cTerminus = CTerminusGroupType.Hydroxyl,
            bool is3LetterCode = true,
            bool oneLetterCheckForPrefixAndSuffixResidues = true,
            // ReSharper disable once InconsistentNaming
            bool threeLetterCheckForPrefixHandSuffixOH = true,
            bool addMissingModificationSymbols = false);

        /// <summary>
        /// Set the symbol for ammonia loss
        /// </summary>
        /// <param name="newSymbol"></param>
        void SetSymbolAmmoniaLoss(string newSymbol);

        /// <summary>
        /// Set the symbol for phospho loss
        /// </summary>
        /// <param name="newSymbol"></param>
        void SetSymbolPhosphoLoss(string newSymbol);

        /// <summary>
        /// Set the symbol for water loss
        /// </summary>
        /// <param name="newSymbol"></param>
        void SetSymbolWaterLoss(string newSymbol);

        /// <summary>
        /// Update standard monoisotopic masses for water, hydrogen, ammonia, the phospho group, and the immonium group
        /// </summary>
        void UpdateStandardMasses();
    }
}
