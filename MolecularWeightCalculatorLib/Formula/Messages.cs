using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    [ComVisible(false)]
    internal class Messages
    {
        // Ignore Spelling: csv, prepend, txt

        public Messages(FormulaOptions options)
        {
            mOptions = options;

            mCautionStatements = new Dictionary<string, string>(50);
            mMessageStatements = new Dictionary<int, string>(220);
        }

        private const int MESSAGE_STATEMENT_DIM_COUNT = 1600;
        private const int MAX_CAUTION_STATEMENTS = 100;
        private readonly FormulaOptions mOptions;

        /// <summary>
        /// CautionStatements.Key holds the symbol combo to look for
        /// CautionStatements.Value holds the caution statement
        /// </summary>
        private readonly Dictionary<string, string> mCautionStatements;

        /// <summary>
        /// Error messages
        /// </summary>
        private readonly Dictionary<int, string> mMessageStatements;

        internal int GetCautionStatementCount()
        {
            return mCautionStatements.Count;
        }

        internal List<string> GetCautionStatementSymbols()
        {
            return mCautionStatements.Keys.ToList();
        }

        /// <summary>
        /// Get a caution statement for the given symbol combo
        /// </summary>
        /// <param name="symbolCombo">symbol combo for the caution statement</param>
        /// <param name="cautionStatement">Output: caution statement text</param>
        /// <returns>0 if success, 1 if an invalid ID</returns>
        internal int GetCautionStatement(string symbolCombo, out string cautionStatement)
        {
            if (mCautionStatements.TryGetValue(symbolCombo, out cautionStatement))
            {
                return 0;
            }

            cautionStatement = string.Empty;
            return 1;
        }

        /// <summary>
        /// Get message text using message ID
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="appendText"></param>
        /// <returns></returns>
        /// <remarks>
        /// GetMessageStatement simply returns the message for <paramref name="messageId"/>
        /// LookupMessage formats the message, and possibly combines multiple messages, depending on the message number
        /// </remarks>
        internal string GetMessageStatement(int messageId, string appendText = "")
        {
            if (mMessageStatements.TryGetValue(messageId, out var message))
            {
                // Append Prefix to certain strings
                switch (messageId)
                {
                    // Add Error: to the front of certain error codes
                    case >= 1 and <= 99:
                    case 120:
                    case 130:
                    case 140:
                    case 260:
                    case 270:
                    case 300:
                        message = GetMessageStatement(350) + ": " + message;
                        break;
                }

                // Now append the text
                return message + appendText;
            }

            return "";
        }

        public int GetMessageStatementMaxId()
        {
            return mMessageStatements.Keys.Max();
        }

        internal string LookupCautionStatement(string compareText)
        {
            if (mCautionStatements.TryGetValue(compareText, out var message))
            {
                return message;
            }

            return string.Empty;
        }

        /// <summary>
        /// Looks up the message for <paramref name="messageId"/>
        /// Also appends any data in <paramref name="appendText"/> to the message
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="appendText"></param>
        /// <returns>The complete message</returns>
        internal string LookupMessage(int messageId, string appendText = "")
        {
            if (mMessageStatements.Count == 0)
                MemoryLoadMessageStatements();

            // Now try to find it
            if (!mMessageStatements.TryGetValue(messageId, out var message) || string.IsNullOrWhiteSpace(message))
            {
                //assume we can't find the message number
                message = "General unspecified error";
            }

            // Now prepend Prefix to certain strings
            switch (messageId)
            {
                // Add Error: to the front of certain error codes
                case >= 1 and <= 99:
                case 120:
                case 130:
                case 140:
                case 260:
                case 270:
                case 300:
                    {
                        message = LookupMessage(350) + ": " + message;
                        break;
                    }
            }

            // Now append the text
            message += appendText;

            // messageId's 1 and 18 may need to have an addendum added
            if (messageId == 1)
            {
                if (mOptions.CaseConversion == CaseConversionMode.ExactCase)
                {
                    message += " (" + LookupMessage(680) + ")";
                }
            }
            else if (messageId == 18)
            {
                if (!mOptions.BracketsAsParentheses)
                {
                    message += " (" + LookupMessage(685) + ")";
                }
                else
                {
                    message += " (" + LookupMessage(690) + ")";
                }
            }

            return message;
        }

        internal void RemoveAllCautionStatements()
        {
            mCautionStatements.Clear();
        }

        /// <summary>
        /// Look for the caution statement and remove it
        /// </summary>
        /// <param name="cautionSymbol"></param>
        /// <returns>0 if found and removed; 1 if error</returns>
        internal int RemoveCautionStatement(string cautionSymbol)
        {
            return mCautionStatements.Remove(cautionSymbol) ? 0 : 1;
        }

        /// <summary>
        /// Adds a new caution statement or updates an existing one (based on <paramref name="symbolCombo"/>)
        /// </summary>
        /// <param name="symbolCombo"></param>
        /// <param name="newCautionStatement"></param>
        /// <returns>0 if successful, otherwise, returns an Error ID</returns>
        internal int SetCautionStatement(string symbolCombo, string newCautionStatement)
        {
            var errorId = 0;
            if (symbolCombo.Length >= 1 && symbolCombo.Length <= ElementsAndAbbrevs.MAX_ABBREV_LENGTH)
            {
                // Make sure all the characters in symbolCombo are letters
                if (symbolCombo.All(char.IsLetter))
                {
                    if (newCautionStatement.Length > 0)
                    {
                        // See if symbolCombo is present in CautionStatements[]
                        var alreadyPresent = mCautionStatements.ContainsKey(symbolCombo);

                        if (!alreadyPresent && mCautionStatements.Count >= MAX_CAUTION_STATEMENTS)
                        {
                            // Too many caution statements
                            errorId = 1215;
                        }
                        else if (!alreadyPresent)
                        {
                            mCautionStatements.Add(symbolCombo, newCautionStatement);
                        }
                        else
                        {
                            mCautionStatements[symbolCombo] = newCautionStatement;
                        }
                    }
                    else
                    {
                        // Caution description length is 0
                        errorId = 1210;
                    }
                }
                else
                {
                    // Caution symbol doesn't just contain letters
                    errorId = 1205;
                }
            }
            else
            {
                // Symbol length is 0 or is greater than MAX_ABBREV_LENGTH
                errorId = 1200;
            }

            return errorId;
        }

        /// <summary>
        /// Used to replace the default message strings with foreign language equivalent ones
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="newMessage"></param>
        /// <returns>0 if success; 1 if failure</returns>
        internal int SetMessageStatement(int messageId, string newMessage)
        {
            if (messageId >= 1 && messageId <= MESSAGE_STATEMENT_DIM_COUNT && newMessage.Length > 0)
            {
                mMessageStatements[messageId] = newMessage;
                return 0;
            }

            return 1;
        }

        public void MemoryLoadAllStatements()
        {
            MemoryLoadCautionStatements();
            MemoryLoadMessageStatements();
        }

        /// <summary>
        /// Define the caution statements
        /// </summary>
        /// <remarks>Use ClearCautionStatements and AddCautionStatement to set these based on language</remarks>
        public void MemoryLoadCautionStatements()
        {
            MemoryLoadCautionStatementsEnglish(mCautionStatements);
        }

        public void MemoryLoadMessageStatements()
        {
            MemoryLoadMessageStatementsEnglish(mMessageStatements);
        }

        /// <summary>
        /// Define the caution statements
        /// </summary>
        /// <remarks>Use ClearCautionStatements and AddCautionStatement to set these based on language</remarks>
        private static void MemoryLoadCautionStatementsEnglish(Dictionary<string, string> cautions)
        {
            cautions.Clear();
            cautions.Add("Bi", "Bi means bismuth; BI means boron-iodine.  ");
            cautions.Add("Bk", "Bk means berkelium; BK means boron-potassium.  ");
            cautions.Add("Bu", "Bu means the butyl group; BU means boron-uranium.  ");
            cautions.Add("Cd", "Cd means cadmium; CD means carbon-deuterium.  ");
            cautions.Add("Cf", "Cf means californium; CF means carbon-fluorine.  ");
            cautions.Add("Co", "Co means cobalt; CO means carbon-oxygen.  ");
            cautions.Add("Cs", "Cs means cesium; CS means carbon-sulfur.  ");
            cautions.Add("Cu", "Cu means copper; CU means carbon-uranium.  ");
            cautions.Add("Dy", "Dy means dysprosium; DY means deuterium-yttrium.  ");
            cautions.Add("Hf", "Hf means hafnium; HF means hydrogen-fluorine.  ");
            cautions.Add("Ho", "Ho means holmium; HO means hydrogen-oxygen.  ");
            cautions.Add("In", "In means indium; IN means iodine-nitrogen.  ");
            cautions.Add("Nb", "Nb means niobium; NB means nitrogen-boron.  ");
            cautions.Add("Nd", "Nd means neodymium; ND means nitrogen-deuterium.  ");
            cautions.Add("Ni", "Ni means nickel; NI means nitrogen-iodine.  ");
            cautions.Add("No", "No means nobelium; NO means nitrogen-oxygen.  ");
            cautions.Add("Np", "Np means neptunium; NP means nitrogen-phosphorus.  ");
            cautions.Add("Os", "Os means osmium; OS means oxygen-sulfur.  ");
            cautions.Add("Pd", "Pd means palladium; PD means phosphorus-deuterium.  ");
            cautions.Add("Ph", "Ph means phenyl, PH means phosphorus-hydrogen.  ");
            cautions.Add("Pu", "Pu means plutonium; PU means phosphorus-uranium.  ");
            cautions.Add("Py", "Py means pyridine; PY means phosphorus-yttrium.  ");
            cautions.Add("Sb", "Sb means antimony; SB means sulfur-boron.  ");
            cautions.Add("Sc", "Sc means scandium; SC means sulfur-carbon.  ");
            cautions.Add("Si", "Si means silicon; SI means sulfur-iodine.  ");
            cautions.Add("Sn", "Sn means tin; SN means sulfur-nitrogen.  ");
            cautions.Add("TI", "TI means tritium-iodine, Ti means titanium.  ");
            cautions.Add("Yb", "Yb means ytterbium; YB means yttrium-boron.  ");
            cautions.Add("BPY", "BPY means boron-phosphorus-yttrium; Bpy means bipyridine.  ");
            cautions.Add("BPy", "BPy means boron-pyridine; Bpy means bipyridine.  ");
            cautions.Add("Bpy", "Bpy means bipyridine.  ");
            cautions.Add("Cys", "Cys means cysteine; CYS means carbon-yttrium-sulfur.  ");
            cautions.Add("His", "His means histidine; HIS means hydrogen-iodine-sulfur.  ");
            cautions.Add("Hoh", "HoH means holmium-hydrogen; HOH means hydrogen-oxygen-hydrogen (aka water).  ");
            cautions.Add("Hyp", "Hyp means hydroxyproline; HYP means hydrogen-yttrium-phosphorus.  ");
            cautions.Add("OAc", "OAc means oxygen-actinium; Oac means acetate.  ");
            cautions.Add("Oac", "Oac means acetate.  ");
            cautions.Add("Pro", "Pro means proline; PrO means praseodymium-oxygen.  ");
            cautions.Add("PrO", "Pro means proline; PrO means praseodymium-oxygen.  ");
            cautions.Add("Val", "Val means valine; VAl means vanadium-aluminum.  ");
            cautions.Add("VAl", "Val means valine; VAl means vanadium-aluminum.  ");
        }

        /// <summary>
        /// Replaces the current message statements with the default messages
        /// </summary>
        /// <param name="messages"></param>
        private static void MemoryLoadMessageStatementsEnglish(Dictionary<int, string> messages)
        {
            messages.Clear();
            messages.Add(1, "Unknown element");
            messages.Add(2, "Obsolete msg: Cannot handle more than 4 layers of embedded parentheses");
            messages.Add(3, "Missing closing parentheses");
            messages.Add(4, "Unmatched parentheses");
            messages.Add(5, "Cannot have a 0 directly after an element or dash (-)");
            messages.Add(6, "Number too large or must only be after [, -, ), or caret (^)");
            messages.Add(7, "Number too large");
            messages.Add(8, "Obsolete msg: Cannot start formula with a number; use parentheses, brackets, or dash (-)");
            messages.Add(9, "Obsolete msg: Decimal numbers cannot be used after parentheses; use a [ or a caret (^)");
            messages.Add(10, "Obsolete msg: Decimal numbers less than 1 must be in the form .5 and not 0.5");
            messages.Add(11, "Numbers should follow left brackets, not right brackets (unless 'treat brackets' as parentheses is on)");
            messages.Add(12, "A number must be present after a bracket and/or after the decimal point");
            messages.Add(13, "Missing closing bracket, ]");
            messages.Add(14, "Misplaced number; should only be after an element, [, ), -, or caret (^)");
            messages.Add(15, "Unmatched bracket");
            messages.Add(16, "Cannot handle nested brackets or brackets inside multiple hydrates (unless 'treat brackets as parentheses' is on)");
            messages.Add(17, "Obsolete msg: Cannot handle multiple hydrates (extras) in brackets");
            messages.Add(18, "'?' only allowed after '['");
            messages.Add(19, "Obsolete msg: Cannot start formula with a dash (-)");
            messages.Add(20, "There must be an isotopic mass number following the caret (^)");
            messages.Add(21, "Obsolete msg: Zero after caret (^); an isotopic mass of zero is not allowed");
            messages.Add(22, "An element must be present after the isotopic mass after the caret (^)");
            messages.Add(23, "Negative isotopic masses are not allowed after the caret (^)");
            messages.Add(24, "Isotopic masses are not allowed for abbreviations");
            messages.Add(25, "An element must be present after the leading coefficient of the dash");
            messages.Add(26, "Isotopic masses are not allowed for abbreviations; D is an abbreviation");
            messages.Add(27, "Numbers cannot contain more than one decimal point");
            messages.Add(28, "Circular abbreviation reference; can't have an abbreviation referencing a second abbreviation that depends upon the first one");
            messages.Add(29, "Obsolete msg: Cannot run percent solver until one or more lines are locked to a value.");
            messages.Add(30, "Invalid formula subtraction; one or more atoms (or too many atoms) in the right-hand formula are missing (or less abundant) in the left-hand formula");
            messages.Add(31, "Cannot use an abbreviation that has not yet been parsed.");
            messages.Add(32, "Cannot use an invalid abbreviation.");

            // Cases 50 through 74 are used during the % Solver routine
            messages.Add(50, "Target value is greater than 100%, an impossible value.");

            // Cases 75 through 99 are used in frmCalculator
            messages.Add(75, "Letters are not allowed in the calculator line");
            messages.Add(76, "Missing closing parenthesis");
            messages.Add(77, "Unmatched parentheses");
            messages.Add(78, "Misplaced number; or number too large, too small, or too long");
            messages.Add(79, "Obsolete msg: Misplaced parentheses");
            messages.Add(80, "Misplaced operator");
            messages.Add(81, "Track variable is less than or equal to 1; program bug; please notify programmer");
            messages.Add(82, "Missing operator. Note: ( is not needed OR allowed after a + or -");
            messages.Add(83, "Obsolete msg: Brackets not allowed in calculator; simply use nested parentheses");
            messages.Add(84, "Obsolete msg: Decimal numbers less than 1 must be in the form .5 and not 0.5");
            messages.Add(85, "Cannot take negative numbers to a decimal power");
            messages.Add(86, "Cannot take zero to a negative power");
            messages.Add(87, "Cannot take zero to the zeroth power");
            messages.Add(88, "Obsolete msg: Only a single positive or negative number is allowed after a caret (^)");
            messages.Add(89, "A single positive or negative number must be present after a caret (^)");
            messages.Add(90, "Numbers cannot contain more than one decimal point");
            messages.Add(91, "You tried to divide a number by zero.  Please correct the problem and recalculate.");
            messages.Add(92, "Spaces are not allowed in mathematical expressions");

            // Note that tags 93 and 94 are also used on frmMain
            messages.Add(93, "Use a period for a decimal point");
            messages.Add(94, "Use a comma for a decimal point");
            messages.Add(95, "A number must be present after a decimal point");

            // Cases 100 and up are shown when loading data from files and starting application
            messages.Add(100, "Error Saving Abbreviation File");
            messages.Add(110, "The default abbreviation file has been re-created.");
            messages.Add(115, "The old file has been renamed");
            messages.Add(120, "[AMINO ACIDS] heading not found in MWT_ABBR.DAT file.  This heading must be located before/above the [ABBREVIATIONS] heading.");
            messages.Add(125, "Obsolete msg: Select OK to continue without any abbreviations.");
            messages.Add(130, "[ABBREVIATIONS] heading not found in MWT_ABBR.DAT file.  This heading must be located before/above the [AMINO ACIDS] heading.");
            messages.Add(135, "Select OK to continue with amino acids abbreviations only.");
            messages.Add(140, "The Abbreviations File was not found in the program directory");
            messages.Add(150, "Error Loading/Creating Abbreviation File");
            messages.Add(160, "Ignoring Abbreviation -- Invalid Formula");
            messages.Add(170, "Ignoring Duplicate Abbreviation");
            messages.Add(180, "Ignoring Abbreviation; Invalid Character");
            messages.Add(190, "Ignoring Abbreviation; too long");
            messages.Add(192, "Ignoring Abbreviation; symbol length cannot be 0");
            messages.Add(194, "Ignoring Abbreviation; symbol most only contain letters");
            messages.Add(196, "Ignoring Abbreviation; Too many abbreviations in memory");
            messages.Add(200, "Ignoring Invalid Line");
            messages.Add(210, "The default elements file has been re-created.");
            messages.Add(220, "Possibly incorrect weight for element");
            messages.Add(230, "Possibly incorrect uncertainty for element");
            messages.Add(250, "Ignoring Line; Invalid Element Symbol");
            messages.Add(260, "[ELEMENTS] heading not found in MWT_ELEM.DAT file.  This heading must be located in the file.");
            messages.Add(265, "Select OK to continue with default Element values.");
            messages.Add(270, "The Elements File was not found in the program directory");
            messages.Add(280, "Error Loading/Creating Elements File");
            messages.Add(305, "Continuing with default captions.");
            messages.Add(320, "Error Saving Elements File");
            messages.Add(330, "Error Loading/Creating Values File");
            messages.Add(340, "Select OK to continue without loading default Values and Formulas.");
            messages.Add(345, "If using a Read-Only drive, use the /X switch at the command line to prevent this error.");
            messages.Add(350, "Error");
            messages.Add(360, "Error Saving Default Options File");
            messages.Add(370, "Obsolete msg: If using a Read-Only drive, you cannot save the default options.");
            messages.Add(380, "Error Saving Values and Formulas File");
            messages.Add(390, "Obsolete msg: If using a Read-Only drive, you cannot save the values and formulas.");
            messages.Add(400, "Error Loading/Creating Default Options File");
            messages.Add(410, "Select OK to continue without loading User Defaults.");
            messages.Add(420, "Obsolete msg: The Default Options file was corrupted; it will be re-created.");
            messages.Add(430, "Obsolete msg: The Values and Formulas file was corrupted; it will be re-created.");
            messages.Add(440, "The language file could not be successfully opened or was formatted incorrectly.");
            messages.Add(450, "Unable to load language-specific captions");
            messages.Add(460, "The language file could not be found in the program directory");
            messages.Add(470, "The file requested for molecular weight processing was not found");
            messages.Add(480, "File Not Found");
            messages.Add(490, "This file already exists.  Replace it?");
            messages.Add(500, "File Exists");
            messages.Add(510, "Error Reading/Writing files for batch processing");
            messages.Add(515, "Select OK to abort batch file processing.");
            messages.Add(520, "Error in program");
            messages.Add(530, "These lines of code should not have been encountered.  Please notify programmer.");
            messages.Add(540, "Obsolete msg: You can't edit elements because the /X switch was used at the command line.");
            messages.Add(545, "Obsolete msg: You can't edit abbreviations because the /X switch was used at the command line.");
            messages.Add(550, "Percent solver cannot be used when brackets are being treated as parentheses.  You can change the bracket recognition mode by choosing Change Program Preferences under the Options menu.");
            messages.Add(555, "Percent Solver not Available");
            messages.Add(560, "Maximum number of formula fields exist.");
            messages.Add(570, "Current formula is blank.");
            messages.Add(580, "Turn off Percent Solver (F11) before creating a new formula.");
            messages.Add(590, "An overflow error has occurred.  Please reduce number sizes and recalculate.");
            messages.Add(600, "An error has occurred");
            messages.Add(605, "Please exit the program and report the error to the programmer.  Select About from the Help menu to see the E-mail address.");
            messages.Add(610, "Spaces are not allowed in formulas");
            messages.Add(620, "Invalid Character");
            messages.Add(630, "Cannot copy to new formula.");
            messages.Add(645, "Obsolete msg: Maximum number of formulas is 7");
            messages.Add(650, "Current formula is blank.");
            messages.Add(655, "Percent Solver mode is on (F11 to exit mode).");
            messages.Add(660, "Warning, isotopic mass is probably too large for element");
            messages.Add(662, "Warning, isotopic mass is probably too small for element");
            messages.Add(665, "vs avg atomic wt of");
            messages.Add(670, "Warning, isotopic mass is impossibly small for element");
            messages.Add(675, "protons");
            messages.Add(680, "Note: Exact Mode is on");
            messages.Add(685, "Note: for % Solver, a left bracket must precede an x");
            messages.Add(690, "Note: brackets are being treated as parentheses");
            messages.Add(700, "One or more elements must be checked.");
            messages.Add(705, "Maximum hits must be greater than 0.");
            messages.Add(710, "Maximum hits must be less than ");
            messages.Add(715, "Minimum number of elements must be 0 or greater.");
            messages.Add(720, "Minimum number of elements must be less than maximum number of elements.");
            messages.Add(725, "Maximum number of elements must be less than 65,025");
            messages.Add(730, "An atomic weight must be entered for custom elements.");
            messages.Add(735, "Atomic Weight must be greater than 0 for custom elements.");
            messages.Add(740, "Target molecular weight must be entered.");
            messages.Add(745, "Target molecular weight must be greater than 0.");
            messages.Add(750, "Obsolete msg: Weight tolerance must be 0 or greater.");
            messages.Add(755, "A maximum molecular weight must be entered.");
            messages.Add(760, "Maximum molecular weight must be greater than 0.");
            messages.Add(765, "Target percentages must be entered for element");
            messages.Add(770, "Target percentage must be greater than 0.");
            messages.Add(775, "Custom elemental weights must contain only numbers or only letters.  If letters are used, they must be for a single valid elemental symbol or abbreviation.");
            messages.Add(780, "Custom elemental weight is empty.  If letters are used, they must be for a single valid elemental symbol or abbreviation.");
            messages.Add(785, "Unknown element or abbreviation for custom elemental weight");
            messages.Add(790, "Only single elemental symbols or abbreviations are allowed.");
            messages.Add(800, "Caution, no abbreviations were loaded -- Command has no effect.");
            messages.Add(805, "Cannot handle fractional numbers of atoms");
            messages.Add(910, "Ions are already present in the ion list.  Replace with new ions?");
            messages.Add(920, "Replace Existing Ions");
            messages.Add(930, "Loading Ion List");
            messages.Add(940, "Process aborted");
            messages.Add(945, " aborted");
            messages.Add(950, "Normalizing ions");
            messages.Add(960, "Normalizing by region");
            messages.Add(965, "Sorting by Intensity");
            messages.Add(970, "Matching Ions");
            messages.Add(980, "The clipboard is empty.  No ions to paste.");
            messages.Add(985, "No ions");
            messages.Add(990, "Pasting ion list");
            messages.Add(1000, "Determining number of ions in list");
            messages.Add(1010, "Parsing list");
            messages.Add(1020, "No valid ions were found on the clipboard.  A valid ion list is a list of mass and intensity pairs, separated by commas, tabs, or spaces.  One mass/intensity pair should be present per line.");

            messages.Add(1030, "Error writing data to file");
            messages.Add(1040, "Set Range");
            messages.Add(1050, "Start Val");
            messages.Add(1055, "End Val");
            messages.Add(1060, "Set X Axis Range");
            messages.Add(1065, "Set Y Axis Range");
            messages.Add(1070, "Enter a new Gaussian Representation quality factor.  Higher numbers result in smoother Gaussian curves, but slower updates.  Valid range is 1 to 50, default is 20.");
            messages.Add(1072, "Gaussian Representation Quality");
            messages.Add(1075, "Enter a new plotting approximation factor. Higher numbers result in faster updates, but give a less accurate graphical representation when viewing a wide mass range (zoomed out).  Valid range is 1 to 50, default is 10.");
            messages.Add(1077, "Plotting Approximation Factor");
            messages.Add(1080, "Resolving Power Specifications");
            messages.Add(1090, "Resolving Power");
            messages.Add(1100, "X Value of Specification");
            messages.Add(1110, "Please enter the approximate number of ticks to show on the axis");
            messages.Add(1115, "Axis Ticks");
            messages.Add(1120, "Creating Gaussian Representation");
            messages.Add(1130, "Preparing plot");
            messages.Add(1135, "Drawing plot");
            messages.Add(1140, "Are you sure you want to restore the default plotting options?");
            messages.Add(1145, "Restore Default Options");
            messages.Add(1150, "Auto Align Ions");
            messages.Add(1155, "Maximum Offset");
            messages.Add(1160, "Offset Increment");
            messages.Add(1165, "Aligning Ions");

            messages.Add(1200, "Caution symbol must be 1 to " + ElementsAndAbbrevs.MAX_ABBREV_LENGTH + " characters long");
            messages.Add(1205, "Caution symbol most only contain letters");
            messages.Add(1210, "Caution description length cannot be 0");
            messages.Add(1215, "Too many caution statements.  Unable to add another one.");

            messages.Add(1500, "All Files");
            messages.Add(1510, "Text Files");
            messages.Add(1515, "txt");
            messages.Add(1520, "Data Files");
            messages.Add(1525, "csv");
            messages.Add(1530, "Sequence Files");
            messages.Add(1535, "seq");
            messages.Add(1540, "Ion List Files");
            messages.Add(1545, "txt");
            messages.Add(1550, "Capillary Flow Info Files");
            messages.Add(1555, "cap");
        }
    }
}
