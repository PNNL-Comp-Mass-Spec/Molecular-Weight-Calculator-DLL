Data comes from https://www.nist.gov/pml/atomic-weights-and-isotopic-compositions-relative-atomic-masses
which obtained its data from https://www.ciaaw.org/atomic-weights.htm and https://www.degruyter.com/document/doi/10.1351/PAC-REP-10-06-02/html

Scroll down and select either "Pre-formatted ASCII Table" or "Linearized ASCII Output", 
then choose either "Most common isotopes" or "all isotopes",
then choose "Get Data".

Transform the _Linearized file to a tabular format using console app TransformIsotopeMassFile.exe


Symbols used in the notes:

g Geological materials are known in which the element has an isotopic composition outside the limits for normal material. The difference between the atomic weight of the element in such materials and that given in the table may exceed the stated uncertainty.
m Modified isotopic compositions may be found in commercially available material because the material has been subjected to an undisclosed or inadvertent isotopic fractionation. Substantial deviations in atomic weight of the element from that given in the table can occur.
r Range in isotopic composition of normal terrestrial material prevents a more precise standard atomic weight being given; the tabulated atomic-weight value and uncertainty should be applicable to normal materials.


== Steps for Updating Element and Isotope Masses ==

1) Open web page https://www.nist.gov/pml/atomic-weights-and-isotopic-compositions-relative-atomic-masses

2) Select "Linearized ASCII Output" and "Most Common Isotopes"

3) Click "Get Data" and save as IsotopeMasses_Linearized.txt

4) Optional, but not required: 
   - Select "Linearized ASCII Output" and "All Isotopes"
   - Click "Get Data" and save as IsotopeMasses_All_Linearized.txt

5) Run ConvertFiles.bat, which has these commands:
   ..\TransformIsotopeMassFile\bin\Debug\net5.0\TransformIsotopeMassFile.exe IsotopeMasses_Linearized.txt
   ..\TransformIsotopeMassFile\bin\Debug\net5.0\TransformIsotopeMassFile.exe IsotopeMasses_All_Linearized.txt

6) Open Excel file IsotopeMasses.xlsx

7) Copy contents of IsotopeMasses_Elements.txt                and paste on worksheet ElementMasses_Tabular

8) Copy contents of IsotopeMasses_Tabular_WithUncertainty.txt and paste on worksheet IsotopeMasses_Tabular

9) Review changes on worksheet Elements_OldVsNew

10) Review updated C# code in column H on worksheet ElementCodeC#

11) Copy updated C# code to method MemoryLoadElements in MolecularWeightCalculatorLib\Formula\ElementsLoader.cs

12) Review changes on worksheet Isotopes_OldVsNew, including updated C# code in column J

13) Copy updated C# code to method MemoryLoadIsotopes in MolecularWeightCalculatorLib\Formula\ElementsLoader.cs

14) Run the unit tests in UnitTests\ElementTests.cs
    - Most of the unit tests will likely fail due to changed masses and changed percent composition values

15) Change these two lines from true to false in the Setup method
            mCompareValuesToExpected = true;
            mCompareTextToExpected = true;

16) Run all of the unit tests again

17) Open file UnitTestCases.txt in UnitTests\bin\Debug (or in the working directory of the unit test runner)

18) Copy updated TestCase[(...)] code and paste in ElementTests.cs

19) Change mCompareValuesToExpected and mCompareTextToExpected back to true

20) Run all of the unit tests again
